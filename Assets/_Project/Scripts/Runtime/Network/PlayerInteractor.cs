using LastCallCrew.Drink;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LastCallCrew.Network
{
    /// <summary>
    /// Interaction du joueur avec une proposition de verre (P1-002 puis P1-012).
    /// Le propriétaire vise, lit ses entrées et envoie des demandes ; l'hôte revérifie émetteur,
    /// cible, portée, disponibilité et, pour un refus, la durée de maintien qu'il mesure lui-même.
    /// </summary>
    public class PlayerInteractor : NetworkBehaviour
    {
        [SerializeField] InputActionAsset inputActions;
        [SerializeField] Transform cameraRoot;
        [SerializeField] float interactRange = 3f;
        [SerializeField] float serverRangeTolerance = 1f;
        [Tooltip("Marge (s) appliquée par l'hôte à la durée de maintien d'un refus, pour la gigue réseau.")]
        [SerializeField] float serverHoldTolerance = 0.15f;

        InputActionAsset _runtimeActions;
        InputAction _accept;
        InputAction _refuse;
        DrinkOffer _aimedOffer;
        string _message;
        float _messageUntil;

        // Côté propriétaire : maintien de refus en cours.
        bool _holding;
        float _holdStartTime;
        DrinkOffer _holdOffer;

        // Côté hôte : début de refus reçu pour ce joueur.
        bool _serverRefuseActive;
        ulong _serverRefuseTargetId;
        double _serverRefuseStart;

        public override void OnNetworkSpawn()
        {
            if (!IsOwner)
                return;

            _runtimeActions = Instantiate(inputActions);
            _accept = _runtimeActions.FindAction("Player/Interact", true);
            _refuse = _runtimeActions.FindAction("Player/Refuse", true);
            _runtimeActions.Enable();
        }

        public override void OnNetworkDespawn()
        {
            if (_runtimeActions != null)
            {
                _runtimeActions.Disable();
                Destroy(_runtimeActions);
                _runtimeActions = null;
            }
        }

        void Update()
        {
            if (!IsOwner || _runtimeActions == null)
                return;

            _aimedOffer = null;
            if (Physics.Raycast(cameraRoot.position, cameraRoot.forward, out RaycastHit hit, interactRange))
                _aimedOffer = hit.collider.GetComponentInParent<DrinkOffer>();

            if (_holding)
                UpdateHold();
            else if (_aimedOffer != null)
                ReadNewInput();
        }

        void ReadNewInput()
        {
            if (_accept.WasPressedThisFrame())
            {
                AcceptRequestRpc(new NetworkObjectReference(_aimedOffer.NetworkObject));
            }
            else if (_refuse.WasPressedThisFrame())
            {
                _holding = true;
                _holdStartTime = Time.time;
                _holdOffer = _aimedOffer;
                RefuseStartRpc(new NetworkObjectReference(_holdOffer.NetworkObject));
            }
        }

        void UpdateHold()
        {
            bool aimLost = _aimedOffer != _holdOffer;
            bool released = !_refuse.IsPressed();
            bool completed = Time.time - _holdStartTime >= _holdOffer.RefuseDurationSeconds;
            if (!aimLost && !released && !completed)
                return;

            // Fin de maintien (relâché, visée perdue ou durée atteinte) : l'hôte décide d'après sa propre mesure.
            _holding = false;
            RefuseEndRpc(new NetworkObjectReference(_holdOffer.NetworkObject));
            _holdOffer = null;
        }

        // ---- Hôte : les trois RPC viennent du propriétaire (RequireOwnership par défaut) ----

        [Rpc(SendTo.Server)]
        void AcceptRequestRpc(NetworkObjectReference target, RpcParams rpcParams = default)
        {
            DrinkRequestOutcome outcome = ServerResolve(target, rpcParams.Receive.SenderClientId, out DrinkOffer offer, out float distance);
            if (outcome == DrinkRequestOutcome.Accepted)
                outcome = DrinkRules.EvaluateAccept(distance, interactRange + serverRangeTolerance, offer.IsAvailable);
            if (outcome == DrinkRequestOutcome.Accepted)
                offer.ServerDecide(DrinkDecision.Accepted, OwnerClientId);
            ServerReport("accept", outcome, distance);
        }

        [Rpc(SendTo.Server)]
        void RefuseStartRpc(NetworkObjectReference target, RpcParams rpcParams = default)
        {
            _serverRefuseActive = false;
            DrinkRequestOutcome outcome = ServerResolve(target, rpcParams.Receive.SenderClientId, out DrinkOffer offer, out float distance);
            if (outcome == DrinkRequestOutcome.Accepted)
            {
                // Début valide : on exige seulement portée et disponibilité maintenant ; la durée est jugée à la fin.
                outcome = DrinkRules.EvaluateAccept(distance, interactRange + serverRangeTolerance, offer.IsAvailable);
            }

            if (outcome == DrinkRequestOutcome.Accepted)
            {
                _serverRefuseActive = true;
                _serverRefuseTargetId = offer.NetworkObjectId;
                _serverRefuseStart = Time.realtimeSinceStartupAsDouble;
                Debug.Log($"[P1-012] host: refusal hold started by client {OwnerClientId}");
                return;
            }

            ServerReport("refuse-start", outcome, distance);
        }

        [Rpc(SendTo.Server)]
        void RefuseEndRpc(NetworkObjectReference target, RpcParams rpcParams = default)
        {
            DrinkRequestOutcome outcome = ServerResolve(target, rpcParams.Receive.SenderClientId, out DrinkOffer offer, out float distance);
            if (outcome == DrinkRequestOutcome.Accepted)
            {
                bool startedOnThisTarget = _serverRefuseActive && _serverRefuseTargetId == offer.NetworkObjectId;
                if (!startedOnThisTarget)
                {
                    outcome = DrinkRequestOutcome.RejectedNoRefusalStarted;
                }
                else
                {
                    double held = Time.realtimeSinceStartupAsDouble - _serverRefuseStart;
                    outcome = DrinkRules.EvaluateRefuse(
                        distance, interactRange + serverRangeTolerance, offer.IsAvailable,
                        held, offer.RefuseDurationSeconds, serverHoldTolerance);
                    if (outcome == DrinkRequestOutcome.Refused)
                        offer.ServerDecide(DrinkDecision.Refused, OwnerClientId);
                }
            }

            _serverRefuseActive = false;
            ServerReport("refuse-end", outcome, distance);
        }

        // Vérifie l'émetteur et la cible ; renvoie Accepted si les deux sont valides (la suite est jugée par DrinkRules).
        DrinkRequestOutcome ServerResolve(NetworkObjectReference target, ulong senderClientId, out DrinkOffer offer, out float distance)
        {
            offer = null;
            distance = float.PositiveInfinity;
            if (senderClientId != OwnerClientId)
                return DrinkRequestOutcome.RejectedInvalidSender;
            if (!target.TryGet(out NetworkObject targetObject) || !targetObject.TryGetComponent(out offer))
                return DrinkRequestOutcome.RejectedInvalidTarget;

            Vector3 origin = cameraRoot.position;
            distance = Vector3.Distance(origin, offer.ClosestPoint(origin));
            return DrinkRequestOutcome.Accepted;
        }

        void ServerReport(string request, DrinkRequestOutcome outcome, float distance)
        {
            Debug.Log($"[P1-012] host: client={OwnerClientId} {request} -> {outcome} (distance {distance:0.00} m)");
            ResultRpc(outcome);
        }

        // ---- Retour au propriétaire ----

        [Rpc(SendTo.Owner)]
        void ResultRpc(DrinkRequestOutcome outcome)
        {
            switch (outcome)
            {
                case DrinkRequestOutcome.Accepted:
                case DrinkRequestOutcome.Refused:
                    return; // la décision est affichée par DrinkOffer, depuis l'état répliqué
                case DrinkRequestOutcome.HoldTooShort:
                    _message = "Refus annulé (relâché trop tôt)";
                    break;
                case DrinkRequestOutcome.RejectedUnavailable:
                    _message = "Verre indisponible";
                    break;
                case DrinkRequestOutcome.RejectedOutOfRange:
                    _message = "Trop loin";
                    break;
                default:
                    _message = "Demande refusée";
                    break;
            }
            _messageUntil = Time.time + 1.5f;
        }

        void OnGUI()
        {
            if (!IsOwner)
                return;

            float cx = Screen.width * 0.5f;
            float cy = Screen.height * 0.5f;
            GUI.Label(new Rect(cx - 10, cy - 12, 20, 24), "+");

            if (_holding && _holdOffer != null)
            {
                float progress = Mathf.Clamp01((Time.time - _holdStartTime) / _holdOffer.RefuseDurationSeconds);
                GUI.Box(new Rect(cx - 60, cy + 20, 120, 12), GUIContent.none);
                Color previous = GUI.color;
                GUI.color = new Color(0.9f, 0.3f, 0.3f);
                GUI.DrawTexture(new Rect(cx - 60, cy + 20, 120 * progress, 12), Texture2D.whiteTexture);
                GUI.color = previous;
                GUI.Label(new Rect(cx - 80, cy + 34, 160, 24), "Refus en cours…");
            }
            else if (_aimedOffer != null)
            {
                string prompt = _aimedOffer.IsAvailable
                    ? "[E] Accepter   [Clic droit maintenu] Refuser"
                    : $"Indisponible ({_aimedOffer.RemainingCooldownSeconds:0} s)";
                GUI.Label(new Rect(cx - 160, cy + 15, 320, 24), prompt);
            }

            if (Time.time < _messageUntil)
                GUI.Label(new Rect(cx - 120, cy + 58, 240, 24), _message);
        }
    }
}

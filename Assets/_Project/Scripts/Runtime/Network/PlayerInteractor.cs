using LastCallCrew.Dialogue;
using LastCallCrew.Drink;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LastCallCrew.Network
{
    /// <summary>
    /// Interaction du joueur avec une proposition de verre (P1-002 puis P1-012).
    /// Le propriétaire vise, lit ses entrées et envoie des demandes ; l'hôte revérifie émetteur,
    /// cible, portée et, pour un refus, la durée de maintien qu'il mesure lui-même.
    /// </summary>
    [RequireComponent(typeof(PlayerIntoxication))]
    [RequireComponent(typeof(PlayerCarry))]
    [RequireComponent(typeof(PlayerWake))]
    public class PlayerInteractor : NetworkBehaviour
    {
        [SerializeField] InputActionAsset inputActions;
        [SerializeField] Transform cameraRoot;
        [SerializeField] float interactRange = 3f;
        [SerializeField] float serverRangeTolerance = 1f;
        [Tooltip("Marge (s) appliquée par l'hôte à la durée de maintien d'un refus, pour la gigue réseau.")]
        [SerializeField] float serverHoldTolerance = 0.15f;

        PlayerIntoxication _intoxication;
        PlayerKnockout _knockout;
        PlayerCarry _carry;
        PlayerWake _wake;
        InputActionAsset _runtimeActions;
        InputAction _accept;
        InputAction _refuse;
        InputAction _wakeAction;
        InputAction[] _dialogueChoice = new InputAction[DialogueRules.ChoiceCount];
        InputAction _dialogueCancel;
        NightRound _nightRound;
        DrinkOffer _aimedOffer;
        PlayerCarry _aimedPartner;
        string _message;
        float _messageUntil;

        /// <summary>Proposition visée par le propriétaire (null sinon). Lu par le HUD de debug.</summary>
        public DrinkOffer AimedOffer => _aimedOffer;
        /// <summary>Partenaire KO visé et portable (null sinon). Lu par le HUD de debug.</summary>
        public PlayerCarry AimedPartner => _aimedPartner;
        /// <summary>Portée maximale acceptée par l'hôte (portée du raycast + tolérance), partagée avec le portage.</summary>
        public float MaxServerRange => interactRange + serverRangeTolerance;
        /// <summary>Origine de la visée (caméra), utilisée par l'hôte pour juger la portée d'un réveil.</summary>
        public Vector3 AimOrigin => cameraRoot.position;
        public bool IsWaking => _wakeHolding;
        /// <summary>P1-015 : vrai si Échap vient de fermer un dialogue pendant cette frame (il ne doit pas aussi libérer la souris).</summary>
        public bool ConsumedCancelThisFrame => _cancelFrame == Time.frameCount;
        int _cancelFrame = -1;
        /// <summary>PNJ à dialogue visé (null sinon). Lu par le HUD.</summary>
        public NpcDialogue AimedNpc => _aimedNpc;
        /// <summary>PNJ dont le dialogue est ouvert pour ce joueur (null sinon). Lu par le HUD : seul le propriétaire le voit.</summary>
        public NpcDialogue OpenDialogue => _dialogueNpc;
        /// <summary>Partenaire en cours de réveil (null sinon). Lu par le HUD.</summary>
        public PlayerCarry WakeTarget => _wakeTarget;
        public float WakeProgress =>
            _wakeHolding ? Mathf.Clamp01((Time.time - _wakeStartTime) / _wake.HoldSeconds) : 0f;
        public bool IsHolding => _holding;
        public float HoldProgress =>
            _holding && _holdOffer != null ? Mathf.Clamp01((Time.time - _holdStartTime) / _holdOffer.RefuseDurationSeconds) : 0f;
        /// <summary>Message local bref (rejet ou annulation) ; vide s'il n'y en a pas.</summary>
        public string FeedbackMessage => Time.time < _messageUntil ? _message : _wake != null ? _wake.FeedbackMessage : string.Empty;

        // Côté propriétaire : maintien de refus en cours.
        bool _holding;
        float _holdStartTime;
        DrinkOffer _holdOffer;

        // Côté propriétaire : dialogue ouvert (P1-014). Le numéro de manche mémorisé le ferme à la relance.
        NpcDialogue _aimedNpc;
        NpcDialogue _dialogueNpc;
        int _dialogueRound;

        // Côté hôte : dialogue ouvert par ce joueur (validé par l'hôte, requis pour accepter un choix).
        bool _serverDialogueOpen;
        ulong _serverDialogueTargetId;

        // Côté propriétaire : maintien de réveil en cours.
        bool _wakeHolding;
        float _wakeStartTime;
        PlayerCarry _wakeTarget;

        // Côté hôte : début de refus reçu pour ce joueur.
        bool _serverRefuseActive;
        ulong _serverRefuseTargetId;
        double _serverRefuseStart;

        public override void OnNetworkSpawn()
        {
            _intoxication = GetComponent<PlayerIntoxication>();
            _knockout = GetComponent<PlayerKnockout>();
            _carry = GetComponent<PlayerCarry>();
            _wake = GetComponent<PlayerWake>();
            if (!IsOwner)
                return;

            _runtimeActions = Instantiate(inputActions);
            _accept = _runtimeActions.FindAction("Player/Interact", true);
            _refuse = _runtimeActions.FindAction("Player/Refuse", true);
            _wakeAction = _runtimeActions.FindAction("Player/Wake", true);
            for (int i = 0; i < _dialogueChoice.Length; i++)
                _dialogueChoice[i] = _runtimeActions.FindAction("Player/DialogueChoice" + (i + 1), true);
            _dialogueCancel = _runtimeActions.FindAction("Player/DialogueCancel", true);
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
            _aimedPartner = null;
            _aimedNpc = null;

            // P1-033 : après le résultat de la nuit, plus aucune interaction (les maintiens en cours sont abandonnés).
            if (NightOutcome.IsFrozen)
            {
                _dialogueNpc = null;
                _holding = false;
                _holdOffer = null;
                _wakeHolding = false;
                _wakeTarget = null;
                return;
            }

            // P1-020 : un joueur KO n'interagit plus ; un refus en cours est abandonné sans décision (l'hôte le rejetterait de toute façon).
            if (_knockout.IsKnockedOut)
            {
                _dialogueNpc = null; // P1-014 : un KO ferme le dialogue sans réaction
                _holding = false;
                _holdOffer = null;
                _wakeHolding = false;
                _wakeTarget = null;
                return;
            }

            // P1-021 : pendant un portage, E est réservé au lâcher ; boire et refuser sont bloqués (l'hôte le vérifie aussi).
            if (_carry.IsCarrying)
            {
                _dialogueNpc = null; // P1-014 : porter quelqu'un ferme le dialogue
                _holding = false;
                _holdOffer = null;
                _wakeHolding = false;
                _wakeTarget = null;
                if (_accept.WasPressedThisFrame())
                    _carry.RequestDropRpc();
                return;
            }

            if (Physics.Raycast(cameraRoot.position, cameraRoot.forward, out RaycastHit hit, interactRange))
            {
                _aimedOffer = hit.collider.GetComponentInParent<DrinkOffer>();
                PlayerCarry partner = hit.collider.GetComponentInParent<PlayerCarry>();
                if (partner != null && partner != _carry && partner.IsKnockedOut && !partner.IsCarried)
                    _aimedPartner = partner;
                _aimedNpc = hit.collider.GetComponentInParent<NpcDialogue>();
            }

            // P1-014 : tant qu'un dialogue est ouvert, seules ses touches sont lues (1/2/3, Échap).
            if (_dialogueNpc != null)
            {
                UpdateDialogue();
                return;
            }

            if (_wakeHolding)
                UpdateWakeHold();
            else if (_holding)
                UpdateHold();
            else if (_aimedNpc != null)
            {
                // E ouvre la proposition du PNJ ; les commandes boire/porter/refuser/réveiller ne changent pas.
                if (_accept.WasPressedThisFrame())
                    BeginDialogue(_aimedNpc);
            }
            else if (_aimedPartner != null)
            {
                if (_accept.WasPressedThisFrame())
                    _carry.RequestCarryRpc(new NetworkObjectReference(_aimedPartner.NetworkObject));
                else if (_wakeAction.WasPressedThisFrame())
                {
                    // P1-022 : l'hôte mesure la durée entre ce début et la fin signalée.
                    _wakeHolding = true;
                    _wakeStartTime = Time.time;
                    _wakeTarget = _aimedPartner;
                    _wake.RequestStartRpc(new NetworkObjectReference(_wakeTarget.NetworkObject));
                }
            }
            else if (_aimedOffer != null)
                ReadNewInput();
        }

        void BeginDialogue(NpcDialogue npc)
        {
            _dialogueNpc = npc;
            _dialogueRound = CurrentRound();
            OpenDialogueRpc(new NetworkObjectReference(npc.NetworkObject));
        }

        // Fermeture locale sans réaction : Échap, perte de la cible / éloignement, ou nouvelle manche.
        void UpdateDialogue()
        {
            if (_dialogueCancel.WasPressedThisFrame())
                _cancelFrame = Time.frameCount;
            if (_dialogueCancel.WasPressedThisFrame() || _aimedNpc != _dialogueNpc || CurrentRound() != _dialogueRound)
            {
                _dialogueNpc = null;
                CloseDialogueRpc();
                return;
            }

            for (int i = 0; i < _dialogueChoice.Length; i++)
            {
                if (!_dialogueChoice[i].WasPressedThisFrame())
                    continue;
                ChooseDialogueRpc(new NetworkObjectReference(_dialogueNpc.NetworkObject), (byte)(i + 1));
                _dialogueNpc = null; // choix immédiat : la carte se ferme, la réaction arrive de l'hôte
                return;
            }
        }

        int CurrentRound()
        {
            if (_nightRound == null)
                _nightRound = FindFirstObjectByType<NightRound>();
            return _nightRound != null ? _nightRound.Round : 0;
        }

        void UpdateWakeHold()
        {
            bool aimLost = _aimedPartner != _wakeTarget;
            bool released = !_wakeAction.IsPressed();
            bool completed = Time.time - _wakeStartTime >= _wake.HoldSeconds;
            if (!aimLost && !released && !completed)
                return;

            // Fin de maintien (relâché, visée perdue ou durée atteinte) : l'hôte décide d'après sa propre mesure.
            _wakeHolding = false;
            _wake.RequestEndRpc(new NetworkObjectReference(_wakeTarget.NetworkObject));
            _wakeTarget = null;
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
                outcome = DrinkRules.EvaluateAccept(distance, interactRange + serverRangeTolerance);
            if (outcome == DrinkRequestOutcome.Accepted)
            {
                // Décision et gain d'ivresse dans le même chemin autoritaire : seule une acceptation validée donne des points.
                offer.ServerDecide(DrinkDecision.Accepted, OwnerClientId);
                _intoxication.ServerAddDrink();
            }
            ServerReport("accept", outcome, distance);
        }

        [Rpc(SendTo.Server)]
        void RefuseStartRpc(NetworkObjectReference target, RpcParams rpcParams = default)
        {
            _serverRefuseActive = false;
            DrinkRequestOutcome outcome = ServerResolve(target, rpcParams.Receive.SenderClientId, out DrinkOffer offer, out float distance);
            if (outcome == DrinkRequestOutcome.Accepted)
            {
                // Début valide : on exige seulement la portée maintenant ; la durée est jugée à la fin.
                outcome = DrinkRules.EvaluateAccept(distance, interactRange + serverRangeTolerance);
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
                        distance, interactRange + serverRangeTolerance,
                        held, offer.RefuseDurationSeconds, serverHoldTolerance);
                    if (outcome == DrinkRequestOutcome.Refused)
                        offer.ServerDecide(DrinkDecision.Refused, OwnerClientId);
                }
            }

            _serverRefuseActive = false;
            ServerReport("refuse-end", outcome, distance);
        }

        /// <summary>
        /// Hôte : abandonne un début de refus et un dialogue ouverts (appelé à l'entrée en KO, au portage d'un partenaire
        /// et à la relance de manche).
        /// </summary>
        public void ServerCancelRefusal()
        {
            _serverRefuseActive = false;
            _serverDialogueOpen = false;
        }

        // ---- Dialogue du PNJ (P1-014) : l'hôte valide l'ouverture puis chaque choix ----

        DialogueOutcome ServerDialogue(NetworkObjectReference target, ulong senderClientId, int choice, bool isChoice, out NpcDialogue npc)
        {
            npc = null;
            var request = new DialogueRequest
            {
                InvalidSender = senderClientId != OwnerClientId,
                InvalidTarget = !target.TryGet(out NetworkObject no) || !no.TryGetComponent(out npc),
                Frozen = NightOutcome.IsFrozen,
                KnockedOut = _knockout.IsKnockedOut,
                Carried = _carry.IsCarried,
                Carrying = _carry.IsCarrying,
                DialogueOpen = _serverDialogueOpen,
                Distance = float.PositiveInfinity,
                MaxDistance = MaxServerRange,
            };
            if (npc != null)
            {
                request.DialogueOpen = _serverDialogueOpen && _serverDialogueTargetId == npc.NetworkObjectId;
                Vector3 origin = cameraRoot.position;
                request.Distance = Vector3.Distance(origin, npc.ClosestPoint(origin));
            }
            return isChoice ? DialogueRules.EvaluateChoice(request, choice) : DialogueRules.EvaluateOpen(request);
        }

        [Rpc(SendTo.Server)]
        void OpenDialogueRpc(NetworkObjectReference target, RpcParams rpcParams = default)
        {
            DialogueOutcome outcome = ServerDialogue(target, rpcParams.Receive.SenderClientId, 0, false, out NpcDialogue npc);
            _serverDialogueOpen = outcome == DialogueOutcome.Opened;
            if (_serverDialogueOpen)
                _serverDialogueTargetId = npc.NetworkObjectId;
            Debug.Log($"[P1-014] host: client={OwnerClientId} dialogue open -> {outcome}");
            if (!_serverDialogueOpen)
                DialogueRejectedRpc(outcome);
        }

        [Rpc(SendTo.Server)]
        void ChooseDialogueRpc(NetworkObjectReference target, byte choice, RpcParams rpcParams = default)
        {
            DialogueOutcome outcome = ServerDialogue(target, rpcParams.Receive.SenderClientId, choice, true, out NpcDialogue npc);
            if (outcome == DialogueOutcome.Chosen)
            {
                // Un choix valide est accepté une seule fois : le dialogue se ferme avant de produire la réaction.
                _serverDialogueOpen = false;
                npc.ServerReact(OwnerClientId, choice);
            }
            else
            {
                _serverDialogueOpen = false;
                Debug.Log($"[P1-014] host: client={OwnerClientId} dialogue choice {choice} -> {outcome}");
                DialogueRejectedRpc(outcome);
            }
        }

        [Rpc(SendTo.Server)]
        void CloseDialogueRpc(RpcParams rpcParams = default)
        {
            if (rpcParams.Receive.SenderClientId == OwnerClientId)
                _serverDialogueOpen = false;
        }

        [Rpc(SendTo.Owner)]
        void DialogueRejectedRpc(DialogueOutcome outcome)
        {
            _dialogueNpc = null;
            _message = outcome == DialogueOutcome.RejectedOutOfRange ? "TROP LOIN" : "DEMANDE REFUSÉE";
            _messageUntil = Time.time + 1.5f;
        }

        // Vérifie l'émetteur et la cible ; renvoie Accepted si les deux sont valides (la suite est jugée par DrinkRules).
        DrinkRequestOutcome ServerResolve(NetworkObjectReference target, ulong senderClientId, out DrinkOffer offer, out float distance)
        {
            offer = null;
            distance = float.PositiveInfinity;
            if (NightOutcome.IsFrozen)
                return DrinkRequestOutcome.RejectedFrozen;
            if (senderClientId != OwnerClientId)
                return DrinkRequestOutcome.RejectedInvalidSender;
            if (_knockout.IsKnockedOut)
                return DrinkRequestOutcome.RejectedKnockedOut;
            if (_carry.IsCarrying)
                return DrinkRequestOutcome.RejectedCarrying;
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
                    return; // la décision est affichée par le HUD, depuis l'état répliqué de DrinkOffer
                case DrinkRequestOutcome.HoldTooShort:
                    _message = "REFUS ANNULÉ";
                    break;
                case DrinkRequestOutcome.RejectedOutOfRange:
                    _message = "TROP LOIN";
                    break;
                case DrinkRequestOutcome.RejectedKnockedOut:
                    return; // l'écran KO du HUD suffit
                case DrinkRequestOutcome.RejectedCarrying:
                    _message = "VOUS PORTEZ QUELQU'UN";
                    break;
                case DrinkRequestOutcome.RejectedFrozen:
                    _message = "NUIT TERMINÉE";
                    break;
                default:
                    _message = "DEMANDE REFUSÉE";
                    break;
            }
            _messageUntil = Time.time + 1.5f;
        }
    }
}

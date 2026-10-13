using LastCallCrew.Carry;
using LastCallCrew.Config;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace LastCallCrew.Network
{
    /// <summary>
    /// Portage d'un partenaire KO (P1-021). L'hôte valide les demandes et écrit l'état durable
    /// (qui porte qui, point de dépose) ; chaque instance en déduit le suivi de la racine du joueur porté.
    /// Suivi : pas de parenting NGO. Pendant le portage, le NetworkTransform et le collider du joueur porté
    /// sont désactivés localement sur chaque instance et sa racine est replacée chaque frame sur le point
    /// de portage du porteur. Au lâcher, la racine est posée à la position répliquée et le NetworkTransform
    /// est réactivé avec un Teleport par son propriétaire.
    /// </summary>
    [DefaultExecutionOrder(200)]
    [RequireComponent(typeof(PlayerKnockout))]
    public class PlayerCarry : NetworkBehaviour
    {
        [SerializeField] TuningConfig tuning;
        [Tooltip("Point du porteur sur lequel la racine du joueur porté est replacée (dos / épaule).")]
        [SerializeField] Transform carryPoint;
        [SerializeField] Transform cameraRoot;
        [Tooltip("Distance (m) devant le porteur où le joueur lâché est posé.")]
        [SerializeField] float dropDistance = 1.2f;

        // Sur le porteur : identifiant NetworkObject du joueur porté + 1 (0 = personne).
        readonly NetworkVariable<ulong> _carryingIdPlusOne = new NetworkVariable<ulong>();
        // Sur le joueur porté : identifiant NetworkObject de son porteur + 1 (0 = personne).
        readonly NetworkVariable<ulong> _carriedByIdPlusOne = new NetworkVariable<ulong>();
        readonly NetworkVariable<Vector3> _dropPosition = new NetworkVariable<Vector3>();

        PlayerKnockout _knockout;
        PlayerInteractor _interactor;
        NetworkTransform _networkTransform;
        Collider _collider;
        bool _following;
        string _message;
        float _messageUntil;

        public bool IsCarrying => _carryingIdPlusOne.Value != 0;
        public bool IsCarried => _carriedByIdPlusOne.Value != 0;
        public bool IsKnockedOut => _knockout != null && _knockout.IsKnockedOut;
        /// <summary>Multiplicateur de vitesse appliqué par PlayerController : valeur du TuningConfig pendant le portage.</summary>
        public float SpeedMultiplier => CarryRules.SpeedMultiplier(IsCarrying, tuning);
        /// <summary>ClientId (propriétaire) du joueur porté, ou null. Pour l'affichage.</summary>
        public ulong? CarriedPlayerClientId => TryResolve(_carryingIdPlusOne.Value, out PlayerCarry other) ? other.OwnerClientId : (ulong?)null;
        /// <summary>ClientId (propriétaire) du porteur, ou null. Pour l'affichage.</summary>
        public ulong? CarrierClientId => TryResolve(_carriedByIdPlusOne.Value, out PlayerCarry other) ? other.OwnerClientId : (ulong?)null;
        public string FeedbackMessage => Time.time < _messageUntil ? _message : string.Empty;

        public override void OnNetworkSpawn()
        {
            if (tuning == null)
                throw new System.InvalidOperationException("PlayerCarry : TuningConfig non assigné.");
            tuning.ThrowIfInvalid();
            _knockout = GetComponent<PlayerKnockout>();
            _interactor = GetComponent<PlayerInteractor>();
            _networkTransform = GetComponent<NetworkTransform>();
            _collider = GetComponent<Collider>();

        }

        public override void OnNetworkDespawn()
        {
            // Hôte : un porteur ou un porté qui disparaît ne laisse pas d'état de portage orphelin.
            if (IsServer && NetworkManager != null && !NetworkManager.ShutdownInProgress)
            {
                if (IsCarrying)
                    ServerDrop();
                else if (IsCarried && TryResolve(_carriedByIdPlusOne.Value, out PlayerCarry carrier))
                    carrier._carryingIdPlusOne.Value = 0;
            }

            if (_following)
                EndFollow(false);
        }

        // ---- Demandes du propriétaire (RequireOwnership par défaut) ----

        [Rpc(SendTo.Server)]
        public void RequestCarryRpc(NetworkObjectReference target, RpcParams rpcParams = default)
        {
            CarryOutcome outcome = ServerStart(target, rpcParams.Receive.SenderClientId);
            Debug.Log($"[P1-021] host: client={OwnerClientId} carry request -> {outcome}");
            ResultRpc(outcome);
        }

        [Rpc(SendTo.Server)]
        public void RequestDropRpc(RpcParams rpcParams = default)
        {
            CarryOutcome outcome = rpcParams.Receive.SenderClientId != OwnerClientId
                ? CarryOutcome.RejectedInvalidSender
                : CarryRules.EvaluateDrop(IsCarrying);
            if (outcome == CarryOutcome.Dropped)
                ServerDrop();
            else
                Debug.Log($"[P1-021] host: client={OwnerClientId} drop request -> {outcome}");
            ResultRpc(outcome);
        }

        [Rpc(SendTo.Owner)]
        void ResultRpc(CarryOutcome outcome)
        {
            string message;
            switch (outcome)
            {
                case CarryOutcome.Started:
                case CarryOutcome.Dropped:
                    return; // l'état répliqué suffit
                case CarryOutcome.RejectedOutOfRange: message = "TROP LOIN"; break;
                case CarryOutcome.RejectedTargetNotKnockedOut: message = "CE JOUEUR N'EST PAS KO"; break;
                case CarryOutcome.RejectedTargetAlreadyCarried: message = "DÉJÀ PORTÉ"; break;
                default: message = "PORTAGE REFUSÉ"; break;
            }
            _message = message;
            _messageUntil = Time.time + 1.5f;
        }

        // ---- Hôte ----

        CarryOutcome ServerStart(NetworkObjectReference target, ulong senderClientId)
        {
            if (senderClientId != OwnerClientId)
                return CarryOutcome.RejectedInvalidSender;
            if (!target.TryGet(out NetworkObject targetObject) || !targetObject.TryGetComponent(out PlayerCarry other))
                return CarryOutcome.RejectedInvalidTarget;

            Vector3 origin = cameraRoot.position;
            float distance = other == this ? 0f : Vector3.Distance(origin, other.GetComponent<Collider>().ClosestPoint(origin));
            var request = new CarryStartRequest
            {
                IsSelf = other == this,
                RequesterKnockedOut = IsKnockedOut,
                RequesterIsCarried = IsCarried,
                RequesterIsCarrying = IsCarrying,
                TargetKnockedOut = other.IsKnockedOut,
                TargetIsCarried = other.IsCarried,
                TargetIsCarrying = other.IsCarrying,
                Distance = distance,
                MaxDistance = _interactor.MaxServerRange,
            };

            CarryOutcome outcome = CarryRules.EvaluateStart(request);
            if (outcome == CarryOutcome.Started)
            {
                _carryingIdPlusOne.Value = other.NetworkObjectId + 1;
                other._carriedByIdPlusOne.Value = NetworkObjectId + 1;
            }
            return outcome;
        }

        /// <summary>Hôte : lâche le joueur porté près du porteur. Sans effet si rien n'est porté.</summary>
        public void ServerDrop()
        {
            if (!IsServer || !IsCarrying)
                return;

            if (TryResolve(_carryingIdPlusOne.Value, out PlayerCarry other))
            {
                other._dropPosition.Value = CarryRules.DropPosition(transform.position, transform.forward, dropDistance);
                other._carriedByIdPlusOne.Value = 0;
            }
            _carryingIdPlusOne.Value = 0;
            Debug.Log($"[P1-021] host: client={OwnerClientId} dropped the carried player");
        }

        /// <summary>Hôte : si ce joueur est porté, son porteur le lâche (ex. sortie de KO injectée).</summary>
        public void ServerReleaseFromCarrier()
        {
            if (IsServer && IsCarried && TryResolve(_carriedByIdPlusOne.Value, out PlayerCarry carrier))
                carrier.ServerDrop();
        }

        // ---- Suivi (toutes les instances) ----

        void LateUpdate()
        {
            if (!IsSpawned)
                return;

            if (IsCarried)
            {
                if (!_following)
                    BeginFollow();
                if (TryResolve(_carriedByIdPlusOne.Value, out PlayerCarry carrier) && carrier.carryPoint != null)
                {
                    transform.SetPositionAndRotation(
                        carrier.carryPoint.position,
                        Quaternion.Euler(0f, carrier.transform.eulerAngles.y, 0f));
                }
            }
            else if (_following)
            {
                EndFollow(true);
            }
        }

        void BeginFollow()
        {
            _following = true;
            if (_networkTransform != null) _networkTransform.enabled = false;
            if (_collider != null) _collider.enabled = false;
        }

        void EndFollow(bool placeAtDrop)
        {
            _following = false;
            if (placeAtDrop)
                transform.position = _dropPosition.Value;
            if (_collider != null) _collider.enabled = true;
            if (_networkTransform != null)
            {
                _networkTransform.enabled = true;
                if (placeAtDrop && IsOwner)
                    _networkTransform.Teleport(transform.position, transform.rotation, transform.localScale);
            }
        }

        bool TryResolve(ulong idPlusOne, out PlayerCarry other)
        {
            other = null;
            if (idPlusOne == 0 || NetworkManager == null || NetworkManager.SpawnManager == null)
                return false;
            if (!NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(idPlusOne - 1, out NetworkObject no) || no == null)
                return false;
            return no.TryGetComponent(out other);
        }
    }
}

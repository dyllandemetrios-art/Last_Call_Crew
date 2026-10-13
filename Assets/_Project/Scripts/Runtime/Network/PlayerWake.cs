using LastCallCrew.Config;
using LastCallCrew.Wake;
using Unity.Netcode;
using UnityEngine;

namespace LastCallCrew.Network
{
    /// <summary>
    /// Réveil d'un joueur KO (P1-022). Sur le joueur KO : échéance du réveil automatique (temps serveur) et
    /// compteur de réveils, écrits par l'hôte. Sur le réveilleur : demandes de début et de fin de maintien ;
    /// l'hôte mesure lui-même la durée. Tous les réveils passent par <see cref="ServerWake"/> : un seul gagne.
    /// </summary>
    [RequireComponent(typeof(PlayerKnockout))]
    [RequireComponent(typeof(PlayerIntoxication))]
    public class PlayerWake : NetworkBehaviour
    {
        [SerializeField] TuningConfig tuning;
        [Tooltip("Marge (s) appliquée par l'hôte à la durée de maintien d'un réveil, pour la gigue réseau.")]
        [SerializeField] float serverHoldTolerance = 0.15f;

        // Sur le joueur KO : temps serveur du réveil automatique (WakeRules.NoDeadline = aucun).
        readonly NetworkVariable<double> _autoWakeDeadline = new NetworkVariable<double>(WakeRules.NoDeadline);
        // Sur le joueur réveillé : nombre de réveils et origine du dernier (0 = automatique, sinon ClientId du réveilleur + 1).
        readonly NetworkVariable<int> _wakeCount = new NetworkVariable<int>();
        readonly NetworkVariable<ulong> _lastWakerPlusOne = new NetworkVariable<ulong>();

        PlayerKnockout _knockout;
        PlayerIntoxication _intoxication;
        PlayerCarry _carry;
        PlayerInteractor _interactor;
        string _message;
        float _messageUntil;

        // Côté hôte : maintien reçu de ce joueur (réveilleur).
        bool _serverHoldActive;
        ulong _serverHoldTargetId;
        double _serverHoldStart;

        /// <summary>Durée de maintien requise (TuningConfig), lue par le propriétaire pour sa jauge.</summary>
        public float HoldSeconds => tuning.WaterWakeHoldSeconds;
        /// <summary>Secondes avant le réveil automatique de ce joueur ; 0 s'il n'est pas KO.</summary>
        public float AutoWakeRemaining =>
            _knockout != null && _knockout.IsKnockedOut && NetworkManager != null
                ? WakeRules.RemainingSeconds(NetworkManager.ServerTime.Time, _autoWakeDeadline.Value)
                : 0f;
        public int WakeCount => _wakeCount.Value;
        /// <summary>ClientId du réveilleur au dernier réveil, ou null si automatique.</summary>
        public ulong? LastWakerClientId => _lastWakerPlusOne.Value == 0 ? (ulong?)null : _lastWakerPlusOne.Value - 1;
        public string FeedbackMessage => Time.time < _messageUntil ? _message : string.Empty;

        public override void OnNetworkSpawn()
        {
            if (tuning == null)
                throw new System.InvalidOperationException("PlayerWake : TuningConfig non assigné.");
            tuning.ThrowIfInvalid();
            _knockout = GetComponent<PlayerKnockout>();
            _intoxication = GetComponent<PlayerIntoxication>();
            _carry = GetComponent<PlayerCarry>();
            _interactor = GetComponent<PlayerInteractor>();
        }

        // ---- Échéance automatique et chemin de réveil unique (hôte) ----

        /// <summary>Hôte : appelé à l'entrée en KO ; l'instant du KO est la référence du réveil automatique.</summary>
        public void ServerOnKnockedOut()
        {
            if (!IsServer)
                return;
            _autoWakeDeadline.Value = WakeRules.AutoWakeDeadline(NetworkManager.ServerTime.Time, tuning);
            _serverHoldActive = false;
        }

        /// <summary>Hôte : appelé à la sortie de KO, quelle qu'en soit la cause.</summary>
        public void ServerOnWoken()
        {
            if (!IsServer)
                return;
            _autoWakeDeadline.Value = WakeRules.NoDeadline;
        }

        /// <summary>Hôte, relance (P1-033) : plus de maintien ni d'échéance de réveil automatique.</summary>
        public void ServerResetForRound()
        {
            if (!IsServer)
                return;
            _serverHoldActive = false;
            _autoWakeDeadline.Value = WakeRules.NoDeadline;
        }

        /// <summary>Hôte : abandonne un maintien en cours (le réveilleur tombe KO).</summary>
        public void ServerCancelHold() => _serverHoldActive = false;

        void Update()
        {
            // P1-033 : le réveil automatique ne modifie plus rien une fois le résultat décidé.
            if (!IsServer || !IsSpawned || !_knockout.IsKnockedOut || NightOutcome.IsFrozen)
                return;
            if (WakeRules.IsAutoWakeDue(NetworkManager.ServerTime.Time, _autoWakeDeadline.Value))
                ServerWake(WakeCause.Auto, null);
        }

        /// <summary>
        /// Hôte : seul chemin de réveil. Fixe l'ivresse puis sort du KO ; sans effet si le joueur n'est plus KO,
        /// ce qui garantit qu'un réveil manuel et un réveil automatique simultanés n'en produisent qu'un.
        /// </summary>
        public bool ServerWake(WakeCause cause, PlayerWake waker)
        {
            if (!IsServer || !_knockout.IsKnockedOut)
                return false;

            float drunkenness = WakeRules.DrunkennessAfter(cause, tuning);
            _intoxication.ServerSetValue(drunkenness);
            _knockout.ServerExitKnockout();
            _lastWakerPlusOne.Value = waker != null ? waker.OwnerClientId + 1 : 0;
            _wakeCount.Value++;
            Debug.Log($"[P1-022] host: client {OwnerClientId} woken ({cause}) drunkenness={drunkenness:0.##}");
            return true;
        }

        // ---- Demandes du propriétaire du réveilleur (RequireOwnership par défaut) ----

        [Rpc(SendTo.Server)]
        public void RequestStartRpc(NetworkObjectReference target, RpcParams rpcParams = default)
        {
            _serverHoldActive = false;
            PlayerWake other = null;
            WakeRequest request = default;
            WakeOutcome outcome = NightOutcome.IsFrozen
                ? WakeOutcome.RejectedFrozen
                : ServerBuildRequest(target, rpcParams.Receive.SenderClientId, out other, out request);
            if (outcome == WakeOutcome.HoldStarted)
                outcome = WakeRules.EvaluateStart(request);

            if (outcome == WakeOutcome.HoldStarted)
            {
                _serverHoldActive = true;
                _serverHoldTargetId = other.NetworkObjectId;
                _serverHoldStart = Time.realtimeSinceStartupAsDouble;
                Debug.Log($"[P1-022] host: client {OwnerClientId} wake hold started -> client {other.OwnerClientId}");
                return;
            }

            Debug.Log($"[P1-022] host: client {OwnerClientId} wake start -> {outcome}");
            ResultRpc(outcome);
        }

        [Rpc(SendTo.Server)]
        public void RequestEndRpc(NetworkObjectReference target, RpcParams rpcParams = default)
        {
            PlayerWake other = null;
            WakeRequest request = default;
            WakeOutcome outcome = NightOutcome.IsFrozen
                ? WakeOutcome.RejectedFrozen
                : ServerBuildRequest(target, rpcParams.Receive.SenderClientId, out other, out request);
            if (outcome == WakeOutcome.HoldStarted)
            {
                bool startedOnThisTarget = _serverHoldActive && _serverHoldTargetId == other.NetworkObjectId;
                double held = Time.realtimeSinceStartupAsDouble - _serverHoldStart;
                outcome = WakeRules.EvaluateEnd(request, startedOnThisTarget, held, tuning.WaterWakeHoldSeconds, serverHoldTolerance);
                if (outcome == WakeOutcome.WokenByWater && !other.ServerWake(WakeCause.Water, this))
                    outcome = WakeOutcome.RejectedTargetNotKnockedOut;
                Debug.Log($"[P1-022] host: client {OwnerClientId} wake end -> {outcome} (held {held:0.00} s, distance {request.Distance:0.00} m)");
            }
            else
            {
                Debug.Log($"[P1-022] host: client {OwnerClientId} wake end -> {outcome}");
            }

            _serverHoldActive = false;
            ResultRpc(outcome);
        }

        // Vérifie émetteur et cible, puis relève l'état des deux joueurs ; renvoie HoldStarted si la requête est construite.
        WakeOutcome ServerBuildRequest(NetworkObjectReference target, ulong senderClientId, out PlayerWake other, out WakeRequest request)
        {
            other = null;
            request = default;
            if (senderClientId != OwnerClientId)
                return WakeOutcome.RejectedInvalidSender;
            if (!target.TryGet(out NetworkObject targetObject) || !targetObject.TryGetComponent(out other))
                return WakeOutcome.RejectedInvalidTarget;

            // Le collider d'un joueur porté est désactivé : la portée n'est alors pas évaluée (rejet par WakeRules avant).
            float distance = float.PositiveInfinity;
            if (other != this && !other._carry.IsCarried)
            {
                Vector3 origin = _interactor.AimOrigin;
                distance = Vector3.Distance(origin, other.GetComponent<Collider>().ClosestPoint(origin));
            }

            request = new WakeRequest
            {
                IsSelf = other == this,
                RequesterKnockedOut = _knockout.IsKnockedOut,
                RequesterIsCarried = _carry.IsCarried,
                RequesterIsCarrying = _carry.IsCarrying,
                TargetKnockedOut = other._knockout.IsKnockedOut,
                TargetIsCarried = other._carry.IsCarried,
                Distance = distance,
                MaxDistance = _interactor.MaxServerRange,
            };
            return WakeOutcome.HoldStarted;
        }

        [Rpc(SendTo.Owner)]
        void ResultRpc(WakeOutcome outcome)
        {
            string message;
            switch (outcome)
            {
                case WakeOutcome.HoldStarted:
                case WakeOutcome.WokenByWater:
                    return; // le HUD affiche le succès depuis l'état répliqué du joueur réveillé
                case WakeOutcome.HoldTooShort: message = "RÉVEIL ANNULÉ"; break;
                case WakeOutcome.RejectedOutOfRange: message = "TROP LOIN"; break;
                case WakeOutcome.RejectedTargetCarried: message = "LÂCHEZ-LE D'ABORD"; break;
                case WakeOutcome.RejectedTargetNotKnockedOut: message = "DÉJÀ RÉVEILLÉ"; break;
                case WakeOutcome.RejectedFrozen: message = "NUIT TERMINÉE"; break;
                default: message = "RÉVEIL REFUSÉ"; break;
            }
            _message = message;
            _messageUntil = Time.time + 1.5f;
        }
    }
}

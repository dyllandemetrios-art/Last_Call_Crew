using LastCallCrew.Config;
using LastCallCrew.Night;
using Unity.Netcode;
using UnityEngine;

namespace LastCallCrew.Network
{
    /// <summary>
    /// Chronomètre de nuit partagé (P1-030). L'hôte écrit deux valeurs durables seulement : l'état et l'échéance
    /// en temps serveur. Chaque instance calcule le temps restant avec NetworkManager.ServerTime ;
    /// rien n'est répliqué à chaque frame ni à chaque seconde, et un nouvel arrivant reçoit l'état par les
    /// NetworkVariable. Aucune décision de victoire ou d'échec : l'expiration n'est qu'un état.
    /// </summary>
    public class NightTimer : NetworkBehaviour
    {
        [SerializeField] TuningConfig tuning;

        readonly NetworkVariable<NightState> _state = new NetworkVariable<NightState>(NightState.Waiting);
        readonly NetworkVariable<double> _deadline = new NetworkVariable<double>();

        public NightState State => _state.Value;
        /// <summary>Échéance en temps serveur (valide dès que l'état n'est plus Waiting). Lecture seule, pour P1-032.</summary>
        public double DeadlineServerTime => _deadline.Value;
        /// <summary>Durée totale configurée, pour la proportion affichée par le HUD (lecture seule).</summary>
        public float DurationSeconds => tuning.NightDurationSeconds;
        public bool IsExpired => NightTimerRules.IsExpiredForDisplay(State, RemainingSeconds);
        public bool IsFinalCountdown => NightTimerRules.IsFinalCountdown(State, RemainingSeconds);
        public float RemainingSeconds =>
            NightTimerRules.Remaining(_state.Value, ClockNow(), _deadline.Value, tuning);

        // P1-033 : une fois le résultat décidé, le temps affiché reste celui de la décision.
        double ClockNow()
        {
            double now = NetworkManager != null ? NetworkManager.ServerTime.Time : 0d;
            return NightOutcome.IsFrozen ? System.Math.Min(now, NightOutcome.CurrentDecidedTime) : now;
        }

        /// <summary>Hôte uniquement (NightRound) : retour à l'attente ; le départ se refait tout seul avec les joueurs requis.</summary>
        public void ServerResetForRound()
        {
            if (!IsServer)
                return;
            _deadline.Value = 0d;
            _state.Value = NightState.Waiting;
        }

        public override void OnNetworkSpawn()
        {
            if (tuning == null)
                throw new System.InvalidOperationException("NightTimer : TuningConfig non assigné.");
            tuning.ThrowIfInvalid();
        }

        void Update()
        {
            if (!IsServer || !IsSpawned)
                return;

            double now = NetworkManager.ServerTime.Time;
            // Le nombre de clients connectés est lu, jamais écrit : une déconnexion ne touche ni l'état ni l'échéance.
            if (NightTimerRules.ShouldStart(_state.Value, NetworkManager.ConnectedClientsIds.Count))
            {
                _deadline.Value = NightTimerRules.Deadline(now, tuning);
                _state.Value = NightState.Running;
                Debug.Log($"[P1-030] host: night started, duration {tuning.NightDurationSeconds:0.##} s, deadline {_deadline.Value:0.00}");
            }
            else if (!NightOutcome.IsFrozen && NightTimerRules.ShouldExpire(_state.Value, now, _deadline.Value))
            {
                _state.Value = NightState.Expired;
                Debug.Log("[P1-030] host: night expired");
            }
        }
    }
}

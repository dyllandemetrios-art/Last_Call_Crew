using System.Collections.Generic;
using LastCallCrew.Night;
using Unity.Netcode;
using UnityEngine;

namespace LastCallCrew.Network
{
    /// <summary>
    /// Conclusion de la nuit (P1-032). L'hôte seul décide : il compare les instants de retour répliqués
    /// (PlayerReturn) à l'échéance du NightTimer, en LateUpdate pour que les retours enregistrés pendant la même
    /// frame (ReturnZone.Update) soient déjà à jour. Le résultat et l'instant de décision sont écrits une seule fois
    /// puis ne changent plus ; aucun client ne peut en demander un, aucun trafic continu. Un client qui rejoint
    /// reçoit l'état par les NetworkVariable. Les contrôles ne sont pas bloqués (P1-033).
    /// </summary>
    [DefaultExecutionOrder(100)]
    public class NightOutcome : NetworkBehaviour
    {
        [SerializeField] NightTimer timer;

        readonly NetworkVariable<NightResult> _result = new NetworkVariable<NightResult>(NightResult.Pending);
        readonly NetworkVariable<double> _decidedAt = new NetworkVariable<double>(ReturnRules.NotReturned);
        readonly List<double> _returnTimes = new List<double>(4);

        public NightResult Result => _result.Value;
        /// <summary>Temps serveur de la décision ; ReturnRules.NotReturned tant qu'elle n'a pas eu lieu.</summary>
        public double DecidedServerTime => _decidedAt.Value;

        public override void OnNetworkSpawn()
        {
            if (timer == null)
                timer = FindFirstObjectByType<NightTimer>();
            if (timer == null)
                throw new System.InvalidOperationException("NightOutcome : NightTimer introuvable.");
        }

        void LateUpdate()
        {
            NetworkManager nm = NetworkManager;
            if (!IsServer || !IsSpawned || _result.Value != NightResult.Pending)
                return;

            _returnTimes.Clear();
            foreach (NetworkClient client in nm.ConnectedClientsList)
            {
                if (client.PlayerObject != null && client.PlayerObject.TryGetComponent(out PlayerReturn pr))
                    _returnTimes.Add(pr.ReturnServerTime);
            }

            NightResult evaluated = NightOutcomeRules.Evaluate(timer.State, timer.DeadlineServerTime, _returnTimes);
            NightResult next = NightOutcomeRules.Next(_result.Value, evaluated);
            if (next == _result.Value)
                return;

            _decidedAt.Value = nm.ServerTime.Time;
            _result.Value = next;
            Debug.Log($"[P1-032] host: night {next} at server time {_decidedAt.Value:0.000} (deadline {timer.DeadlineServerTime:0.000}, {_returnTimes.Count} player(s))");
        }
    }
}

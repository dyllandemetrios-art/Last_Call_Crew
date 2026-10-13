using LastCallCrew.Config;
using LastCallCrew.Night;
using Unity.Netcode;
using UnityEngine;

namespace LastCallCrew.Network
{
    /// <summary>
    /// Manche de la nuit et relance (P1-033). Le compteur de manche est répliqué : il rend la relance
    /// idempotente (une seule remise à zéro par résultat) et permet à chaque instance de nettoyer sa présentation
    /// une seule fois quand il change. Seul l'hôte relance ; la demande d'un client est jugée et refusée par l'hôte.
    /// La remise à zéro se fait en place, sans recharger la scène ni couper la session.
    /// </summary>
    public class NightRound : NetworkBehaviour
    {
        [SerializeField] NightTimer timer;
        [SerializeField] NightOutcome outcome;
        [SerializeField] TuningConfig tuning;

        readonly NetworkVariable<int> _round = new NetworkVariable<int>(1);

        /// <summary>Numéro de la manche courante (1 à la première partie).</summary>
        public int Round => _round.Value;

        public override void OnNetworkSpawn()
        {
            if (timer == null) timer = FindFirstObjectByType<NightTimer>();
            if (outcome == null) outcome = FindFirstObjectByType<NightOutcome>();
            if (tuning == null)
                throw new System.InvalidOperationException("NightRound : TuningConfig non assigné.");
            tuning.ThrowIfInvalid();
        }

        /// <summary>Demande de relance. N'importe quelle instance peut l'envoyer ; l'hôte ne l'accepte que de lui-même.</summary>
        [Rpc(SendTo.Server)]
        public void RequestRestartRpc(RpcParams rpcParams = default)
        {
            bool senderIsHost = rpcParams.Receive.SenderClientId == NetworkManager.ServerClientId;
            RestartOutcome result = RestartRules.Evaluate(senderIsHost, outcome.Result);
            Debug.Log($"[P1-033] host: restart request from client {rpcParams.Receive.SenderClientId} -> {result}");
            if (result == RestartOutcome.Restarted)
                ServerRestart();
        }

        /// <summary>
        /// Hôte uniquement. Ordre : portages et maintiens, KO et réveils, ivresse, retours, poses ; puis le
        /// chronomètre, le résultat et enfin le compteur de manche. Le chronomètre est remis à l'attente avant
        /// que le résultat redevienne Pending, et les retours sont déjà effacés : l'évaluation de la même frame
        /// (LateUpdate) ne peut donc pas conclure à partir d'anciennes données.
        /// </summary>
        void ServerRestart()
        {
            // 1. Portages et maintiens en cours (positions de dépose = points de départ).
            foreach (NetworkClient client in NetworkManager.ConnectedClientsList)
            {
                NetworkObject player = client.PlayerObject;
                if (player == null) continue;
                Vector3 start = RestartRules.StartPosition(player.OwnerClientId);
                if (player.TryGetComponent(out PlayerCarry carry)) carry.ServerResetForRound(start);
                if (player.TryGetComponent(out PlayerInteractor interactor)) interactor.ServerCancelRefusal();
                if (player.TryGetComponent(out PlayerWake wake)) wake.ServerResetForRound();
            }

            // 2. KO (et ses réveils), 3. ivresse, 4. retours, 5. poses.
            foreach (NetworkClient client in NetworkManager.ConnectedClientsList)
            {
                NetworkObject player = client.PlayerObject;
                if (player == null) continue;
                if (player.TryGetComponent(out PlayerKnockout knockout)) knockout.ServerExitKnockout();
                if (player.TryGetComponent(out PlayerIntoxication intoxication)) intoxication.ServerSetValue(tuning.DrunkennessMin);
                if (player.TryGetComponent(out PlayerReturn ret))
                    ret.ServerResetForRound(RestartRules.StartPosition(player.OwnerClientId), NetworkManager.ServerTime.Time);
                if (player.TryGetComponent(out PlayerController controller))
                    controller.ResetPoseRpc(RestartRules.StartPosition(player.OwnerClientId), Quaternion.identity);
            }

            // 6. Chronomètre à l'attente (il repartira une seule fois), 7. résultat, 8. compteur de manche.
            timer.ServerResetForRound();
            outcome.ServerResetForRound();
            _round.Value++;
            Debug.Log($"[P1-033] host: round {_round.Value} started (timer waiting, result pending)");
        }
    }
}

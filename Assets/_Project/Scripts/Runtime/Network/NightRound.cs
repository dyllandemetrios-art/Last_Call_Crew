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
        [Tooltip("PNJ à dialogue (P1-014), remis à zéro à la relance. Optionnel.")]
        [SerializeField] NpcDialogue npc;

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
            RestartOutcome result = RestartRules.Evaluate(senderIsHost, outcome.Result, NetworkManager.ConnectedClientsIds.Count);
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
        ReturnZone _zone;
        HouseKeys _keys;
        HouseWallet _wallet;
        readonly System.Collections.Generic.List<ulong> _connectedIds = new System.Collections.Generic.List<ulong>(4);

        static Vector3 StartFor(NetworkObject player) =>
            RestartRules.StartPosition(player.TryGetComponent(out PlayerController c) ? c.Slot : 0);

        void ServerRestart()
        {
            // P1-050 : sorties mémorisées et retour désactivé avant de replacer les joueurs.
            if (_zone == null) _zone = FindFirstObjectByType<ReturnZone>();
            if (_zone != null) _zone.ServerResetForRound();

            // 0. Emplacements : rang par ClientId croissant parmi les joueurs connectés (P1-040), pas ClientId × 3.
            _connectedIds.Clear();
            foreach (NetworkClient client in NetworkManager.ConnectedClientsList)
                if (client.PlayerObject != null)
                    _connectedIds.Add(client.ClientId);
            foreach (NetworkClient client in NetworkManager.ConnectedClientsList)
                if (client.PlayerObject != null && client.PlayerObject.TryGetComponent(out PlayerController slotted))
                    slotted.ServerSetSlot(PlayerSlots.Rank(client.ClientId, _connectedIds));

            // 1. Portages et maintiens en cours (positions de dépose = points de départ).
            foreach (NetworkClient client in NetworkManager.ConnectedClientsList)
            {
                NetworkObject player = client.PlayerObject;
                if (player == null) continue;
                Vector3 start = StartFor(player);
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
                    ret.ServerResetForRound(StartFor(player), NetworkManager.ServerTime.Time);
                if (player.TryGetComponent(out PlayerController controller))
                    controller.ResetPoseRpc(StartFor(player), Quaternion.Euler(0f, RestartRules.StartYaw, 0f));
            }

            // 6. Chronomètre à l'attente (il repartira une seule fois), 7. résultat, 8. compteur de manche.
            if (npc == null) npc = FindFirstObjectByType<NpcDialogue>();
            if (npc != null) npc.ServerResetForRound();
            // P1-051 : les clés réapparaissent avant que les joueurs reprennent le contrôle (le gel tombe avec le résultat).
            if (_keys == null) _keys = FindFirstObjectByType<HouseKeys>();
            if (_keys != null) _keys.ServerResetForRound();
            // P1-052 : le portefeuille réapparaît dans le même ordre sûr, indépendamment des clés.
            if (_wallet == null) _wallet = FindFirstObjectByType<HouseWallet>();
            if (_wallet != null) _wallet.ServerResetForRound();
            timer.ServerResetForRound();
            outcome.ServerResetForRound();
            _round.Value++;
            Debug.Log($"[P1-033] host: round {_round.Value} started (timer waiting, result pending)");
        }
    }
}

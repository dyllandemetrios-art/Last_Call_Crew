using Unity.Netcode;
using UnityEngine;

namespace LastCallCrew.Network
{
    /// <summary>
    /// Zone de retour (P1-031) devenue l'intérieur de la maison (P1-050). Objet de scène ordinaire : seule l'instance hôte agit. Elle teste, à chaque frame,
    /// la position de chaque joueur dans le volume du trigger (collider séparé du visuel) ; les joueurs ne sont
    /// pas des corps physiques et le collider d'un joueur porté est désactivé, donc la position est la seule
    /// source fiable. Un porteur dans la zone enregistre aussi son partenaire porté (état P1-021).
    /// Aucune demande client n'est lue.
    /// </summary>
    public class ReturnZone : MonoBehaviour
    {
        [Tooltip("Collider trigger de la zone. Distinct du visuel.")]
        [SerializeField] Collider trigger;
        [Tooltip("Rendu de la zone (greybox), coloré à l'exécution.")]
        [SerializeField] Renderer[] visuals;
        [SerializeField] Color zoneColor = new Color(0.2f, 0.85f, 0.45f);

        void Awake()
        {
            if (visuals == null)
                return;
            var block = new MaterialPropertyBlock();
            block.SetColor("_BaseColor", zoneColor);
            foreach (Renderer r in visuals)
                if (r != null)
                    r.SetPropertyBlock(block);
        }

        readonly LastCallCrew.Night.HouseExitTracker _tracker = new LastCallCrew.Night.HouseExitTracker();
        readonly System.Collections.Generic.Dictionary<ulong, bool> _presence = new System.Collections.Generic.Dictionary<ulong, bool>();
        readonly System.Collections.Generic.HashSet<ulong> _settled = new System.Collections.Generic.HashSet<ulong>();
        readonly System.Collections.Generic.List<ulong> _roster = new System.Collections.Generic.List<ulong>(4);
        NightOutcome _outcome;

        /// <summary>Hôte, relance (P1-050) : sorties mémorisées effacées ; le retour redevient inactif (NightOutcome le remet à zéro).</summary>
        public void ServerResetForRound() => _tracker.Reset();

        void Update()
        {
            NetworkManager nm = NetworkManager.Singleton;
            if (nm == null || !nm.IsServer || !nm.IsListening || trigger == null)
            {
                _tracker.Reset(); // hors session hôte : aucune mémoire d'une partie précédente
                return;
            }
            // P1-033 : plus aucun retour n'est enregistré une fois le résultat décidé.
            if (NightOutcome.IsFrozen)
                return;
            if (_outcome == null)
                _outcome = FindFirstObjectByType<NightOutcome>();
            if (_outcome == null || !_outcome.IsSpawned)
                return;

            double now = nm.ServerTime.Time;
            _presence.Clear();
            _settled.Clear();
            _roster.Clear();

            // 1. Position réelle de chaque joueur. Tant que sa pose de départ (apparition ou relance) n'est pas arrivée sur
            //    l'hôte, la position de passage ne compte ni comme présence ni comme sortie.
            foreach (NetworkClient client in nm.ConnectedClientsList)
            {
                NetworkObject player = client.PlayerObject;
                if (player == null)
                    continue;
                _roster.Add(player.OwnerClientId);
                bool settled = !player.TryGetComponent(out PlayerReturn self) || self.ServerPoseSettled(player.transform.position, now);
                if (settled)
                    _settled.Add(player.OwnerClientId);
                _presence[player.OwnerClientId] = settled && IsInside(player.transform.position);
            }

            // 2. Un joueur porté compte dans la maison seulement tant que son porteur y est (son collider est désactivé et
            //    sa position suit le porteur) ; un KO non porté suit sa propre position.
            foreach (NetworkClient client in nm.ConnectedClientsList)
            {
                NetworkObject player = client.PlayerObject;
                if (player == null)
                    continue;
                ulong id = player.OwnerClientId;
                bool carried = player.TryGetComponent(out PlayerCarry carry) && carry.IsCarried;
                bool observable;
                if (carried)
                {
                    ulong? carrierId = carry.CarrierClientId;
                    bool carrierInside = carrierId.HasValue && _presence.TryGetValue(carrierId.Value, out bool inside) && inside;
                    observable = carrierId.HasValue && _settled.Contains(carrierId.Value);
                    _presence[id] = carrierInside;
                }
                else
                    observable = _settled.Contains(id);
                if (observable)
                    _tracker.Observe(id, _presence[id]);
            }

            // 3. Activation du retour (hôte seul, une fois) : tous les joueurs de la manche sont sortis au moins une fois.
            if (!_outcome.ReturnActive && _tracker.TryActivate(_roster))
            {
                _outcome.ServerSetReturnActive(true);
                Debug.Log($"[P1-050] host: return activated at server time {now:0.00} ({_roster.Count} player(s) have left the house)");
            }

            // 4. Retour actif : instant d'entrée dans la maison (P1-031a). Inactif : personne n'est marqué rentré.
            bool active = _outcome.ReturnActive;
            foreach (NetworkClient client in nm.ConnectedClientsList)
            {
                NetworkObject player = client.PlayerObject;
                if (player == null || !player.TryGetComponent(out PlayerReturn ret))
                    continue;
                _presence.TryGetValue(player.OwnerClientId, out bool present);
                ret.ServerSetPresence(active && present, now);
            }
        }

        bool IsInside(Vector3 point) => (trigger.ClosestPoint(point) - point).sqrMagnitude < 1e-6f;
    }
}

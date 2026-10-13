using Unity.Netcode;
using UnityEngine;

namespace LastCallCrew.Network
{
    /// <summary>
    /// Zone de sortie (P1-031). Objet de scène ordinaire : seule l'instance hôte agit. Elle teste, à chaque frame,
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

        void Update()
        {
            NetworkManager nm = NetworkManager.Singleton;
            // P1-033 : plus aucun retour n'est enregistré une fois le résultat décidé.
            if (nm == null || !nm.IsServer || !nm.IsListening || trigger == null || NightOutcome.IsFrozen)
                return;

            double now = nm.ServerTime.Time;
            foreach (NetworkClient client in nm.ConnectedClientsList)
            {
                NetworkObject player = client.PlayerObject;
                if (player == null || !IsInside(player.transform.position))
                    continue;

                // Après une relance, ignorer la position tant que la pose de départ n'est pas arrivée sur l'hôte.
                if (player.TryGetComponent(out PlayerReturn self))
                {
                    if (!self.ServerPoseSettled(player.transform.position, now))
                        continue;
                    self.ServerMarkReturned(now);
                }

                // Un porteur qui rentre ramène son partenaire porté, même si le collider de celui-ci est désactivé.
                if (player.TryGetComponent(out PlayerCarry carrier)
                    && carrier.TryGetCarriedPlayer(out PlayerCarry carried)
                    && carried.TryGetComponent(out PlayerReturn partner))
                    partner.ServerMarkReturned(now);
            }
        }

        bool IsInside(Vector3 point) => (trigger.ClosestPoint(point) - point).sqrMagnitude < 1e-6f;
    }
}

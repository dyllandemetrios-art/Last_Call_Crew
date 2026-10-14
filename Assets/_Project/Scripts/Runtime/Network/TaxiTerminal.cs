using LastCallCrew.Night;
using UnityEngine;

namespace LastCallCrew.Network
{
    /// <summary>
    /// Borne d'un arrêt de taxi (P1-055). Objet de scène ordinaire (pas de NetworkObject) : l'arrêt est identifié par son
    /// numéro dans la demande réseau. Porte le collider visé, la zone d'embarquement (trigger séparé du visuel) et les
    /// points de dépose utilisés à l'arrivée.
    /// </summary>
    public class TaxiTerminal : MonoBehaviour
    {
        [SerializeField] TaxiStop stop;
        [Tooltip("Collider de la borne, visé par le joueur (E).")]
        [SerializeField] Collider post;
        [Tooltip("Zone d'embarquement : trigger posé sur le quai, distinct du taxi et de la chaussée.")]
        [SerializeField] Collider zone;
        [Tooltip("Points de dépose à l'arrivée sur ce quai, un par joueur (rang par identifiant croissant). Orientation = cap du joueur.")]
        [SerializeField] Transform[] dropPoints;

        public TaxiStop Stop => stop;

        public Vector3 ClosestPoint(Vector3 from) => post.ClosestPoint(from);

        /// <summary>Vrai si le point est dans la zone d'embarquement (même test que la zone de retour).</summary>
        public bool Contains(Vector3 point) => zone != null && (zone.ClosestPoint(point) - point).sqrMagnitude < 1e-6f;

        public void DropPose(int rank, out Vector3 position, out Quaternion rotation)
        {
            Transform t = dropPoints[((rank % dropPoints.Length) + dropPoints.Length) % dropPoints.Length];
            position = t.position;
            rotation = Quaternion.Euler(0f, t.eulerAngles.y, 0f);
        }
    }
}

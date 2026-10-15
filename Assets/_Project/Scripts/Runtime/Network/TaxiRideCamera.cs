using Unity.Netcode;
using UnityEngine;

namespace LastCallCrew.Network
{
    /// <summary>
    /// Caméra de trajet en taxi (P1-056a), locale au propriétaire. Pendant un trajet avec passagers, la caméra du joueur passe
    /// en troisième personne derrière et au-dessus du taxi ; la pose est déduite du véhicule déjà synchronisé par le temps
    /// serveur (aucun état réseau). S'exécute après <see cref="CollectiveTaxi"/>, <see cref="PlayerCarry"/> et
    /// <see cref="PlayerKnockout"/> pour ne pas trembler. À la fin (arrivée, annulation, relance), la pose locale précédente
    /// est restaurée exactement, vue KO d'un joueur porté comprise ; si l'état KO a changé entre-temps ou si la relance a
    /// annulé le trajet, la pose debout d'origine est reprise. Les joueurs non propriétaires ne sont jamais touchés.
    /// </summary>
    [DefaultExecutionOrder(400)]
    public class TaxiRideCamera : NetworkBehaviour
    {
        [SerializeField] Transform cameraRoot;
        [Tooltip("Distance (m) derrière le taxi.")]
        [SerializeField] float distance = 7f;
        [Tooltip("Hauteur (m) de la caméra au-dessus du taxi.")]
        [SerializeField] float height = 3f;
        [Tooltip("Le point visé se trouve à cette distance (m) devant le taxi.")]
        [SerializeField] float lookAhead = 1.5f;
        [Tooltip("Hauteur (m) du point visé, au-dessus du taxi (le toit est à ≈ 1,9 m).")]
        [SerializeField] float lookHeight = 1.5f;
        [Tooltip("Vitesse de suivi de la position (1/s). Plus grand = plus rigide.")]
        [SerializeField] float followSpeed = 8f;
        [Tooltip("Vitesse de suivi de l'orientation (1/s).")]
        [SerializeField] float turnSpeed = 8f;

        PlayerKnockout _knockout;
        bool _active;
        bool _wasKnockedOut;
        int _epoch;
        Vector3 _savedPosition;
        Quaternion _savedRotation;
        Vector3 _standingPosition;
        Quaternion _standingRotation;
        Vector3 _position;
        Quaternion _rotation;

        public override void OnNetworkSpawn()
        {
            _knockout = GetComponent<PlayerKnockout>();
            if (cameraRoot != null)
            {
                _standingPosition = cameraRoot.localPosition;
                _standingRotation = cameraRoot.localRotation;
            }
        }

        public override void OnNetworkDespawn()
        {
            if (_active)
                Restore(CollectiveTaxi.Current);
        }

        void LateUpdate()
        {
            if (!IsSpawned || !IsOwner || cameraRoot == null)
                return;

            CollectiveTaxi taxi = CollectiveTaxi.Current;
            bool riding = CollectiveTaxi.IsRiding && taxi.Vehicle != null;
            if (riding && !_active)
                Begin(taxi);
            else if (!riding && _active)
                Restore(taxi);
            if (_active)
                Follow(taxi.Vehicle);
        }

        void Begin(CollectiveTaxi taxi)
        {
            _active = true;
            _epoch = taxi.Epoch;
            _wasKnockedOut = _knockout != null && _knockout.IsKnockedOut;
            _savedPosition = cameraRoot.localPosition;
            _savedRotation = cameraRoot.localRotation;
            _position = cameraRoot.position;
            _rotation = cameraRoot.rotation;
        }

        void Follow(Transform vehicle)
        {
            Vector3 forward = vehicle.forward;
            Vector3 desired = vehicle.position - forward * distance + Vector3.up * height;
            _position = Vector3.Lerp(_position, desired, 1f - Mathf.Exp(-followSpeed * Time.deltaTime));
            Vector3 target = vehicle.position + forward * lookAhead + Vector3.up * lookHeight;
            Quaternion look = Quaternion.LookRotation(target - _position, Vector3.up);
            _rotation = Quaternion.Slerp(_rotation, look, 1f - Mathf.Exp(-turnSpeed * Time.deltaTime));
            cameraRoot.SetPositionAndRotation(_position, _rotation);
        }

        void Restore(CollectiveTaxi taxi)
        {
            _active = false;
            bool cancelled = taxi != null && taxi.Epoch != _epoch;
            bool sameView = !cancelled && _knockout != null && _knockout.IsKnockedOut == _wasKnockedOut;
            cameraRoot.localPosition = sameView ? _savedPosition : _standingPosition;
            cameraRoot.localRotation = sameView ? _savedRotation : _standingRotation;
        }
    }
}

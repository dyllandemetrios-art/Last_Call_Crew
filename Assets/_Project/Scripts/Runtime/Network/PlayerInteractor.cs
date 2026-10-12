using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LastCallCrew.Network
{
    /// <summary>
    /// Interaction P1-002. Le propriétaire vise avec un raycast court et envoie une demande ;
    /// l'hôte revérifie la cible et la distance avant de changer l'état.
    /// </summary>
    public class PlayerInteractor : NetworkBehaviour
    {
        [SerializeField] InputActionAsset inputActions;
        [SerializeField] Transform cameraRoot;
        [SerializeField] float interactRange = 3f;
        [SerializeField] float serverRangeTolerance = 1f;

        InputActionAsset _runtimeActions;
        InputAction _interact;
        InteractiveCube _aimedCube;
        string _message;
        float _messageUntil;

        public override void OnNetworkSpawn()
        {
            if (!IsOwner)
                return;

            _runtimeActions = Instantiate(inputActions);
            _interact = _runtimeActions.FindAction("Player/Interact", true);
            _runtimeActions.Enable();
        }

        public override void OnNetworkDespawn()
        {
            if (_runtimeActions != null)
            {
                _runtimeActions.Disable();
                Destroy(_runtimeActions);
                _runtimeActions = null;
            }
        }

        void Update()
        {
            if (!IsOwner || _runtimeActions == null)
                return;

            _aimedCube = null;
            if (Physics.Raycast(cameraRoot.position, cameraRoot.forward, out RaycastHit hit, interactRange))
                _aimedCube = hit.collider.GetComponentInParent<InteractiveCube>();

            if (_aimedCube != null && _interact.WasPressedThisFrame())
                RequestInteractRpc(new NetworkObjectReference(_aimedCube.NetworkObject));
        }

        // Exécuté sur l'hôte ; seul le propriétaire du joueur peut l'appeler (RequireOwnership par défaut).
        [Rpc(SendTo.Server)]
        void RequestInteractRpc(NetworkObjectReference target)
        {
            bool accepted = ServerValidate(target, out InteractiveCube cube, out string reason);
            if (accepted)
                cube.ServerToggle();

            Debug.Log(accepted
                ? $"[P1-002] host accepted client={OwnerClientId} -> cube={(cube.IsOn ? "ON" : "OFF")}"
                : $"[P1-002] host rejected client={OwnerClientId}: {reason}");
            ResultRpc(accepted);
        }

        bool ServerValidate(NetworkObjectReference target, out InteractiveCube cube, out string reason)
        {
            cube = null;
            if (!target.TryGet(out NetworkObject targetObject) || !targetObject.TryGetComponent(out cube))
            {
                reason = "cible invalide";
                return false;
            }

            Vector3 origin = cameraRoot.position;
            float distance = Vector3.Distance(origin, cube.ClosestPoint(origin));
            if (distance > interactRange + serverRangeTolerance)
            {
                reason = $"hors de portée ({distance:0.00} m)";
                return false;
            }

            reason = null;
            return true;
        }

        [Rpc(SendTo.Owner)]
        void ResultRpc(bool accepted)
        {
            if (accepted)
                return;
            _message = "Interaction refusée";
            _messageUntil = Time.time + 1.5f;
        }

        void OnGUI()
        {
            if (!IsOwner)
                return;

            float cx = Screen.width * 0.5f;
            float cy = Screen.height * 0.5f;
            GUI.Label(new Rect(cx - 10, cy - 12, 20, 24), "+");
            if (_aimedCube != null)
                GUI.Label(new Rect(cx - 60, cy + 15, 120, 24), "[E] Activer");
            if (Time.time < _messageUntil)
                GUI.Label(new Rect(cx - 80, cy + 40, 160, 24), _message);
        }
    }
}

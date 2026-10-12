using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LastCallCrew.Network
{
    /// <summary>
    /// Déplacement minimal d'une capsule joueur. Seul le propriétaire lit les entrées ;
    /// la position est répliquée par un NetworkTransform en autorité propriétaire.
    /// </summary>
    public class PlayerController : NetworkBehaviour
    {
        [SerializeField] InputActionAsset inputActions;
        [SerializeField] Transform cameraRoot;
        [SerializeField] Camera playerCamera;
        [SerializeField] AudioListener audioListener;
        [SerializeField] float moveSpeed = 5f;
        [SerializeField] float lookSensitivity = 0.1f;

        InputActionAsset _runtimeActions;
        InputAction _move;
        InputAction _look;
        float _pitch;
        bool _autoWalk;

        public override void OnNetworkSpawn()
        {
            playerCamera.enabled = IsOwner;
            audioListener.enabled = IsOwner;
            if (!IsOwner)
                return;

            transform.position = new Vector3(OwnerClientId * 3f, 1f, 0f);
            _runtimeActions = Instantiate(inputActions);
            _move = _runtimeActions.FindAction("Player/Move", true);
            _look = _runtimeActions.FindAction("Player/Look", true);
            _runtimeActions.Enable();
            // Entrée scriptée pour les tests sans humain : -lcc-autowalk (marche en cercle).
            _autoWalk = System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-lcc-autowalk") >= 0;
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

            if (_autoWalk)
            {
                transform.Rotate(0f, 30f * Time.deltaTime, 0f);
                transform.position += transform.forward * (moveSpeed * 0.5f * Time.deltaTime);
                return;
            }

            Vector2 look = _look.ReadValue<Vector2>() * lookSensitivity;
            transform.Rotate(0f, look.x, 0f);
            _pitch = Mathf.Clamp(_pitch - look.y, -80f, 80f);
            cameraRoot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);

            Vector2 move = _move.ReadValue<Vector2>();
            Vector3 direction = transform.right * move.x + transform.forward * move.y;
            transform.position += direction * (moveSpeed * Time.deltaTime);
        }
    }
}

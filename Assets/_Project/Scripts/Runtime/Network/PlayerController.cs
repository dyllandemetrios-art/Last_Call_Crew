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

        PlayerKnockout _knockout;
        PlayerCarry _carry;
        InputActionAsset _runtimeActions;
        InputAction _move;
        InputAction _look;
        float _pitch;
        bool _autoWalk;

        public override void OnNetworkSpawn()
        {
            _knockout = GetComponent<PlayerKnockout>();
            _carry = GetComponent<PlayerCarry>();
            playerCamera.enabled = IsOwner;
            audioListener.enabled = IsOwner;
            if (!IsOwner)
                return;

            transform.position = LastCallCrew.Night.RestartRules.StartPosition(OwnerClientId);
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

            // P1-020 : un joueur KO ne se déplace ni ne tourne la caméra (auto-walk compris).
            if (_knockout != null && _knockout.IsKnockedOut)
                return;

            // P1-033 : après le résultat de la nuit, ni déplacement ni regard jusqu'à la relance.
            if (NightOutcome.IsFrozen)
                return;

            if (_autoWalk)
            {
                transform.Rotate(0f, 30f * Time.deltaTime, 0f);
                transform.position += transform.forward * (moveSpeed * CarrySpeedFactor * 0.5f * Time.deltaTime);
                return;
            }

            Vector2 look = _look.ReadValue<Vector2>() * lookSensitivity;
            transform.Rotate(0f, look.x, 0f);
            _pitch = Mathf.Clamp(_pitch - look.y, -80f, 80f);
            cameraRoot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);

            Vector2 move = _move.ReadValue<Vector2>();
            Vector3 direction = transform.right * move.x + transform.forward * move.y;
            transform.position += direction * (moveSpeed * CarrySpeedFactor * Time.deltaTime);
        }

        /// <summary>
        /// P1-033 : l'hôte demande au propriétaire de se replacer au départ (la position est en autorité propriétaire).
        /// </summary>
        [Rpc(SendTo.Owner)]
        public void ResetPoseRpc(Vector3 position, Quaternion rotation)
        {
            transform.SetPositionAndRotation(position, rotation);
            _pitch = 0f;
            if (cameraRoot != null && !(_knockout != null && _knockout.IsKnockedOut))
                cameraRoot.localRotation = Quaternion.identity;
            if (TryGetComponent(out Unity.Netcode.Components.NetworkTransform nt) && nt.enabled)
                nt.Teleport(position, rotation, transform.localScale);
        }

        // P1-021 : le multiplicateur de portage (TuningConfig) s'applique sans toucher à la vitesse de base.
        float CarrySpeedFactor => _carry != null ? _carry.SpeedMultiplier : 1f;
    }
}

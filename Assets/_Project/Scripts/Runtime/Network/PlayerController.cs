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
        InputAction _release;
        PlayerInteractor _interactor;
        bool _captured;
        bool _wasFrozen;
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
            _release = _runtimeActions.FindAction("Player/DialogueCancel", true);
            _interactor = GetComponent<PlayerInteractor>();
            _runtimeActions.Enable();
            // P1-015 : le propriétaire capture le pointeur dès son apparition ; un clic gauche le reprend après Échap ou un changement de fenêtre.
            SetCaptured(true);
            // Entrée scriptée pour les tests sans humain : -lcc-autowalk (marche en cercle).
            _autoWalk = System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-lcc-autowalk") >= 0;
        }

        public override void OnNetworkDespawn()
        {
            if (_runtimeActions != null)
            {
                SetCaptured(false);
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

            // Pointeur libéré : pas de regard (le delta d'un curseur libre ferait tourner la caméra).
            Vector2 look = _captured ? _look.ReadValue<Vector2>() * lookSensitivity : Vector2.zero;
            transform.Rotate(0f, look.x, 0f);
            _pitch = Mathf.Clamp(_pitch - look.y, -80f, 80f);
            cameraRoot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);

            Vector2 move = _move.ReadValue<Vector2>();
            Vector3 direction = transform.right * move.x + transform.forward * move.y;
            transform.position += direction * (moveSpeed * CarrySpeedFactor * Time.deltaTime);
            PushOutOfObstacles();
        }

        // P1-010 : le joueur se déplace par transform (sans Rigidbody) ; on le repousse donc horizontalement hors des colliders
        // solides de la scène (murs, comptoir, tables, PNJ). Les autres joueurs, les triggers et le sol ne comptent pas.
        static readonly Collider[] ObstacleBuffer = new Collider[16];
        CapsuleCollider _body;

        void PushOutOfObstacles()
        {
            if (_body == null && !TryGetComponent(out _body))
                return;

            for (int pass = 0; pass < 3; pass++)
            {
                Vector3 center = transform.TransformPoint(_body.center);
                float half = Mathf.Max(_body.height * 0.5f - _body.radius, 0f);
                int count = Physics.OverlapCapsuleNonAlloc(
                    center + Vector3.up * half, center - Vector3.up * half, _body.radius,
                    ObstacleBuffer, ~0, QueryTriggerInteraction.Ignore);

                bool moved = false;
                for (int i = 0; i < count; i++)
                {
                    Collider other = ObstacleBuffer[i];
                    if (other == _body || other.transform.IsChildOf(transform) || (other is MeshCollider mesh && !mesh.convex)
                        || other.GetComponentInParent<PlayerController>() != null)
                        continue;
                    if (!Physics.ComputePenetration(_body, transform.position, transform.rotation,
                            other, other.transform.position, other.transform.rotation, out Vector3 direction, out float distance))
                        continue;
                    direction.y = 0f;
                    if (direction.sqrMagnitude < 1e-6f)
                        continue;
                    transform.position += direction.normalized * distance;
                    moved = true;
                }
                if (!moved)
                    break;
            }
        }

        // P1-015 : exécuté après tous les Update, donc après PlayerInteractor, qui a déjà pu consommer Échap pour fermer un dialogue.
        void LateUpdate()
        {
            if (!IsOwner || _runtimeActions == null)
                return;

            // Le bilan (bouton RELANCER) a besoin d'un curseur libre : libération au gel, reprise à la relance (sur transition uniquement).
            bool frozen = NightOutcome.IsFrozen;
            if (frozen != _wasFrozen)
            {
                _wasFrozen = frozen;
                SetCaptured(!frozen);
            }
            if (frozen || !Application.isFocused)
                return;

            if (_captured)
            {
                if (_release.WasPressedThisFrame() && !_interactor.ConsumedCancelThisFrame)
                    SetCaptured(false);
                else if (Cursor.lockState != CursorLockMode.Locked && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                    SetCaptured(true); // verrou perdu sans demande (focus, éditeur) : un clic volontaire le reprend, sans forcer chaque frame
            }
            else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                SetCaptured(true);
        }

        void SetCaptured(bool captured)
        {
            _captured = captured;
            Cursor.lockState = captured ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !captured;
        }

        // Indication minimale quand la capture est perdue ; le HUD n'est pas modifié.
        void OnGUI()
        {
            if (!IsOwner || _runtimeActions == null || NightOutcome.IsFrozen || (_captured && Cursor.lockState == CursorLockMode.Locked))
                return;
            const float w = 420f, h = 34f;
            var rect = new Rect((Screen.width - w) * 0.5f, Screen.height - 70f, w, h);
            GUI.Box(rect, "Clic gauche : reprendre la souris");
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

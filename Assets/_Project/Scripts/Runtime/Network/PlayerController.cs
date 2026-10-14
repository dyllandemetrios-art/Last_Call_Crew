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

        // P1-040 : emplacement de départ attribué par l'hôte (ordre des joueurs connectés), jamais déduit du ClientId.
        readonly NetworkVariable<int> _slot = new NetworkVariable<int>(0);

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
            if (IsServer)
                _slot.Value = FirstFreeSlot();
            playerCamera.enabled = IsOwner;
            audioListener.enabled = IsOwner;
            if (!IsOwner)
                return;

            transform.position = LastCallCrew.Night.RestartRules.StartPosition(_slot.Value);
            _runtimeActions = Instantiate(inputActions);
            _move = _runtimeActions.FindAction("Player/Move", true);
            _look = _runtimeActions.FindAction("Player/Look", true);
            _release = _runtimeActions.FindAction("Player/DialogueCancel", true);
            _interactor = GetComponent<PlayerInteractor>();
            _runtimeActions.Enable();
            // P1-015 : le propriétaire capture le pointeur dès son apparition ; Échap le libère puis le reprend (P1-015a).
            SetCaptured(true);
            // Entrée scriptée pour les tests sans humain : -lcc-autowalk (marche en cercle).
            _autoWalk = System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-lcc-autowalk") >= 0;
        }

        /// <summary>Emplacement de départ courant (répliqué).</summary>
        public int Slot => _slot.Value;

        /// <summary>Hôte, relance : réattribue l'emplacement selon l'ordre des joueurs connectés.</summary>
        public void ServerSetSlot(int slot)
        {
            if (IsServer)
                _slot.Value = slot;
        }

        int FirstFreeSlot()
        {
            var used = new System.Collections.Generic.List<int>(2);
            foreach (NetworkClient client in NetworkManager.ConnectedClientsList)
                if (client.PlayerObject != null && client.PlayerObject != NetworkObject
                    && client.PlayerObject.TryGetComponent(out PlayerController other))
                    used.Add(other._slot.Value);
            return LastCallCrew.Night.PlayerSlots.FirstFree(used);
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
            Vector2 look = _captured && Cursor.lockState == CursorLockMode.Locked ? _look.ReadValue<Vector2>() * lookSensitivity : Vector2.zero;
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

            // P1-015a : seul Échap bascule la capture ; un clic gauche ne la reprend plus, il reste pour les boutons et champs du HUD.
            if (!_release.WasPressedThisFrame())
                return;
            if (_interactor.ConsumedCancelThisFrame)
                return; // cet Échap vient de fermer un dialogue
#if UNITY_EDITOR
            // L'éditeur relâche lui-même le verrou sur Échap : on ne fait que suivre son état pour ne pas lutter contre lui.
            SetCaptured(!_captured);
#else
            // Libre (ou verrou perdu au changement de fenêtre) : on recapture ; verrouillé : on libère.
            SetCaptured(Cursor.lockState != CursorLockMode.Locked);
#endif
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
            GUI.Box(rect, "Échap : reprendre la souris");
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

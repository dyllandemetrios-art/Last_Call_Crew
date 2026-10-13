using Unity.Netcode;
using UnityEngine;

namespace LastCallCrew.Network
{
    /// <summary>
    /// État KO d'un joueur (P1-020). Donnée durable répliquée, écrite par l'hôte uniquement.
    /// Seul l'enfant visuel est couché : la racine réseau et le collider restent verticaux et stables.
    /// </summary>
    public class PlayerKnockout : NetworkBehaviour
    {
        [Tooltip("Enfant portant le mesh de la capsule ; c'est lui qui est couché.")]
        [SerializeField] Transform visual;
        [Tooltip("Position locale du visuel quand le joueur est debout.")]
        [SerializeField] Vector3 standingLocalPosition = Vector3.zero;
        [Tooltip("Position locale du visuel couché (rapproché du sol).")]
        [SerializeField] Vector3 knockedOutLocalPosition = new Vector3(0f, -0.5f, 0f);
        [Tooltip("Rotation locale (Euler) du visuel couché.")]
        [SerializeField] Vector3 knockedOutLocalEuler = new Vector3(0f, 0f, 90f);
        [Tooltip("Durée (s) de la transition visuelle, locale à chaque instance.")]
        [SerializeField] float visualTransitionSeconds = 0.25f;

        [Header("Caméra du propriétaire en KO (P1-020a)")]
        [Tooltip("Caméra du joueur (enfant de la racine). Seul le propriétaire la déplace.")]
        [SerializeField] Transform cameraRoot;
        [Tooltip("Position locale de la caméra en KO : derrière et au-dessus du joueur.")]
        [SerializeField] Vector3 knockedOutCameraLocalPosition = new Vector3(0f, 1.8f, -3.4f);
        [Tooltip("Rotation locale (Euler) de la caméra en KO, orientée vers le corps couché.")]
        [SerializeField] Vector3 knockedOutCameraLocalEuler = new Vector3(32f, 0f, 0f);
        [Tooltip("Durée (s) du recul de la caméra à l'entrée en KO et du retour à la sortie.")]
        [SerializeField] float cameraTransitionSeconds = 0.35f;

        readonly NetworkVariable<bool> _isKnockedOut = new NetworkVariable<bool>();

        PlayerInteractor _interactor;
        PlayerCarry _carry;
        PlayerWake _wake;
        float _blend;

        // Caméra (propriétaire uniquement) : pose première personne mémorisée et avancement du recul.
        Vector3 _standingCameraPosition;
        Quaternion _standingCameraRotation = Quaternion.identity;
        float _cameraBlend;

        public bool IsKnockedOut => _isKnockedOut.Value;

        public override void OnNetworkSpawn()
        {
            _interactor = GetComponent<PlayerInteractor>();
            _carry = GetComponent<PlayerCarry>();
            _wake = GetComponent<PlayerWake>();
            // Un client qui rejoint après le KO reçoit l'état par la NetworkVariable : visuel appliqué sans transition.
            _blend = _isKnockedOut.Value ? 1f : 0f;
            ApplyVisual();

            if (IsOwner && cameraRoot != null)
            {
                _standingCameraPosition = cameraRoot.localPosition;
                _standingCameraRotation = cameraRoot.localRotation;
                _cameraBlend = _isKnockedOut.Value ? 1f : 0f;
                ApplyCamera();
            }
        }

        /// <summary>Hôte uniquement. Idempotent : sans effet (et sans log) si le joueur est déjà KO.</summary>
        public void ServerEnterKnockout()
        {
            if (!IsServer || _isKnockedOut.Value)
                return;
            _isKnockedOut.Value = true;
            if (_interactor != null)
                _interactor.ServerCancelRefusal();
            // P1-021 : un porteur qui tombe KO lâche son partenaire.
            if (_carry != null)
                _carry.ServerDrop();
            // P1-022 : l'instant du KO fixe l'échéance du réveil automatique ; un maintien de réveil en cours est abandonné.
            if (_wake != null)
            {
                _wake.ServerCancelHold();
                _wake.ServerOnKnockedOut();
            }
            Debug.Log($"[P1-020] host: client {OwnerClientId} is knocked out");
        }

        /// <summary>
        /// Hôte uniquement. Relève le joueur (P1-022 : appelé par PlayerWake, seul chemin de réveil) ;
        /// le visuel et la caméra repassent debout depuis l'état répliqué.
        /// </summary>
        public void ServerExitKnockout()
        {
            if (!IsServer || !_isKnockedOut.Value)
                return;
            _isKnockedOut.Value = false;
            if (_wake != null)
                _wake.ServerOnWoken();
            // P1-021 : un joueur relevé n'est plus porté.
            if (_carry != null)
                _carry.ServerReleaseFromCarrier();
        }

        void Update()
        {
            if (!IsSpawned || visual == null)
                return;

            float target = _isKnockedOut.Value ? 1f : 0f;
            if (Mathf.Approximately(_blend, target))
                return;
            _blend = visualTransitionSeconds <= 0f
                ? target
                : Mathf.MoveTowards(_blend, target, Time.deltaTime / visualTransitionSeconds);
            ApplyVisual();
        }

        // LateUpdate : prend le pas sur PlayerController qui réécrit la rotation de la caméra pendant le retour.
        void LateUpdate()
        {
            if (!IsSpawned || !IsOwner || cameraRoot == null)
                return;

            float target = _isKnockedOut.Value ? 1f : 0f;
            if (_cameraBlend == 0f && target == 1f)
            {
                // Début de la chute : la pose première personne actuelle est la pose à restaurer.
                _standingCameraPosition = cameraRoot.localPosition;
                _standingCameraRotation = cameraRoot.localRotation;
            }

            if (Mathf.Approximately(_cameraBlend, target))
            {
                _cameraBlend = target;
                if (target == 1f)
                    ApplyCamera(); // aucun contrôle de caméra pendant le KO : la pose reste exactement celle-ci
                return;
            }

            _cameraBlend = cameraTransitionSeconds <= 0f
                ? target
                : Mathf.MoveTowards(_cameraBlend, target, Time.deltaTime / cameraTransitionSeconds);
            ApplyCamera();
        }

        void ApplyCamera()
        {
            // Courbe douce pour que le recul soit lisible sans à-coup.
            float t = Mathf.SmoothStep(0f, 1f, _cameraBlend);
            cameraRoot.localPosition = Vector3.Lerp(_standingCameraPosition, knockedOutCameraLocalPosition, t);
            cameraRoot.localRotation = Quaternion.Slerp(_standingCameraRotation, Quaternion.Euler(knockedOutCameraLocalEuler), t);
        }

        void ApplyVisual()
        {
            if (visual == null)
                return;
            visual.localPosition = Vector3.Lerp(standingLocalPosition, knockedOutLocalPosition, _blend);
            visual.localRotation = Quaternion.Slerp(Quaternion.identity, Quaternion.Euler(knockedOutLocalEuler), _blend);
        }
    }
}

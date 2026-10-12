using Unity.Netcode;
using UnityEngine;

namespace LastCallCrew.Network
{
    /// <summary>
    /// Cube interactif de la scène de test P1-002. Seul l'hôte change l'état ;
    /// le visuel découle uniquement de la NetworkVariable répliquée.
    /// </summary>
    public class InteractiveCube : NetworkBehaviour
    {
        [SerializeField] Renderer cubeRenderer;
        [SerializeField] Collider cubeCollider;
        [SerializeField] Color offColor = new Color(0.8f, 0.15f, 0.15f);
        [SerializeField] Color onColor = new Color(0.2f, 0.85f, 0.25f);

        readonly NetworkVariable<bool> _isOn = new NetworkVariable<bool>();
        MaterialPropertyBlock _block;

        public bool IsOn => _isOn.Value;

        public override void OnNetworkSpawn()
        {
            _isOn.OnValueChanged += OnStateChanged;
            ApplyVisual(_isOn.Value);
        }

        public override void OnNetworkDespawn()
        {
            _isOn.OnValueChanged -= OnStateChanged;
        }

        public Vector3 ClosestPoint(Vector3 from) => cubeCollider.ClosestPoint(from);

        /// <summary>Appelé uniquement par l'hôte après validation de la demande.</summary>
        public void ServerToggle()
        {
            if (!IsServer)
                return;
            _isOn.Value = !_isOn.Value;
        }

        void OnStateChanged(bool previous, bool current) => ApplyVisual(current);

        void ApplyVisual(bool isOn)
        {
            _block ??= new MaterialPropertyBlock();
            _block.SetColor("_BaseColor", isOn ? onColor : offColor);
            cubeRenderer.SetPropertyBlock(_block);
        }
    }
}

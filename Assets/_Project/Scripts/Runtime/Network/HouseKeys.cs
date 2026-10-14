using LastCallCrew.Night;
using Unity.Netcode;
using UnityEngine;

namespace LastCallCrew.Network
{
    /// <summary>
    /// Clés de la maison (P1-051) : un seul état collectif répliqué, écrit par l'hôte (Disponible / Prises). Pas d'inventaire,
    /// de porteur ni de serrure. Le visuel et le collider de visée suivent l'état réseau ; PlayerInteractor demande la prise
    /// et l'hôte la valide (<see cref="KeyRules"/>). Le collider est un trigger : visable par le raycast, jamais un obstacle.
    /// </summary>
    public class HouseKeys : NetworkBehaviour
    {
        [SerializeField] Collider keysCollider;
        [Tooltip("Rendu des clés ; masqué quand elles sont prises.")]
        [SerializeField] GameObject visual;

        readonly NetworkVariable<bool> _taken = new NetworkVariable<bool>(false);

        public bool IsTaken => _taken.Value;

        public Vector3 ClosestPoint(Vector3 from) => keysCollider.ClosestPoint(from);

        public override void OnNetworkSpawn()
        {
            _taken.OnValueChanged += OnTakenChanged;
            Apply(_taken.Value); // un client qui rejoint reçoit l'état courant avec le spawn
        }

        public override void OnNetworkDespawn() => _taken.OnValueChanged -= OnTakenChanged;

        void OnTakenChanged(bool previous, bool current) => Apply(current);

        void Apply(bool taken)
        {
            if (visual != null) visual.SetActive(!taken);
            if (keysCollider != null) keysCollider.enabled = !taken;
        }

        /// <summary>Hôte uniquement, après validation : les clés sont prises pour tout le groupe.</summary>
        public void ServerTake(ulong takerClientId)
        {
            if (!IsServer || _taken.Value)
                return;
            _taken.Value = KeyRules.NextTaken(_taken.Value, KeyOutcome.Taken);
            Debug.Log($"[P1-051] host: client {takerClientId} took the house keys");
        }

        /// <summary>Hôte uniquement (NightRound) : les clés réapparaissent avant que les joueurs reprennent le contrôle.</summary>
        public void ServerResetForRound()
        {
            if (IsServer && _taken.Value)
                _taken.Value = KeyRules.AfterRoundReset();
        }
    }
}

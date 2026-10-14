using LastCallCrew.Night;
using Unity.Netcode;
using UnityEngine;

namespace LastCallCrew.Network
{
    /// <summary>
    /// Portefeuille de la maison (P1-052) : état collectif répliqué, écrit par l'hôte, indépendant des clés (<see cref="HouseKeys"/>).
    /// Composant parallèle volontaire : aucun inventaire, argent ou taxi. Visuel et collider de visée dérivés de l'état réseau ;
    /// le collider est un trigger (visable, jamais un obstacle).
    /// </summary>
    public class HouseWallet : NetworkBehaviour
    {
        [SerializeField] Collider walletCollider;
        [Tooltip("Rendu du portefeuille ; masqué quand il est pris.")]
        [SerializeField] GameObject visual;

        readonly NetworkVariable<bool> _taken = new NetworkVariable<bool>(false);

        public bool IsTaken => _taken.Value;

        public Vector3 ClosestPoint(Vector3 from) => walletCollider.ClosestPoint(from);

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
            if (walletCollider != null) walletCollider.enabled = !taken;
        }

        /// <summary>Hôte uniquement, après validation : le portefeuille est pris pour tout le groupe.</summary>
        public void ServerTake(ulong takerClientId)
        {
            if (!IsServer || _taken.Value)
                return;
            _taken.Value = WalletRules.NextTaken(_taken.Value, KeyOutcome.Taken);
            Debug.Log($"[P1-052] host: client {takerClientId} took the house wallet");
        }

        /// <summary>Hôte uniquement (NightRound) : le portefeuille réapparaît avant que les joueurs reprennent le contrôle.</summary>
        public void ServerResetForRound()
        {
            if (IsServer && _taken.Value)
                _taken.Value = WalletRules.AfterRoundReset();
        }
    }
}

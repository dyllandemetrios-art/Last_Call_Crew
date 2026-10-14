using LastCallCrew.Night;
using Unity.Netcode;
using UnityEngine;

namespace LastCallCrew.Network
{
    /// <summary>
    /// Téléphone de la maison (P1-053) : état collectif répliqué, écrit par l'hôte, indépendant des clés et du portefeuille.
    /// Aucun menu ni appel. Composant parallèle volontaire à <see cref="HouseKeys"/> / <see cref="HouseWallet"/> (pas d'inventaire).
    /// </summary>
    public class HousePhone : NetworkBehaviour
    {
        [SerializeField] Collider phoneCollider;
        [Tooltip("Rendu du téléphone ; masqué quand il est pris.")]
        [SerializeField] GameObject visual;

        readonly NetworkVariable<bool> _taken = new NetworkVariable<bool>(false);

        public bool IsTaken => _taken.Value;

        public Vector3 ClosestPoint(Vector3 from) => phoneCollider.ClosestPoint(from);

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
            if (phoneCollider != null) phoneCollider.enabled = !taken;
        }

        /// <summary>Hôte uniquement, après validation : le téléphone est pris pour tout le groupe.</summary>
        public void ServerTake(ulong takerClientId)
        {
            if (!IsServer || _taken.Value)
                return;
            _taken.Value = PhoneRules.NextTaken(_taken.Value, KeyOutcome.Taken);
            Debug.Log($"[P1-053] host: client {takerClientId} took the house phone");
        }

        /// <summary>Hôte uniquement (NightRound) : le téléphone réapparaît avant que les joueurs reprennent le contrôle.</summary>
        public void ServerResetForRound()
        {
            if (IsServer && _taken.Value)
                _taken.Value = PhoneRules.AfterRoundReset();
        }
    }
}

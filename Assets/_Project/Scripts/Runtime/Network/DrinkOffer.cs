using LastCallCrew.Config;
using LastCallCrew.Drink;
using Unity.Netcode;
using UnityEngine;

namespace LastCallCrew.Network
{
    /// <summary>
    /// Proposition de verre temporaire (P1-012). L'hôte seul décide ; la dernière décision et
    /// l'instant de retour de disponibilité sont répliqués, le visuel et le texte en découlent.
    /// </summary>
    public class DrinkOffer : NetworkBehaviour
    {
        [SerializeField] TuningConfig tuning;
        [SerializeField] Renderer offerRenderer;
        [SerializeField] Collider offerCollider;
        [SerializeField] Color availableColor = new Color(1f, 0.8f, 0.1f);
        [SerializeField] Color unavailableColor = new Color(0.35f, 0.35f, 0.35f);

        readonly NetworkVariable<DrinkDecision> _lastDecision = new NetworkVariable<DrinkDecision>();
        readonly NetworkVariable<ulong> _lastDeciderClientId = new NetworkVariable<ulong>();
        readonly NetworkVariable<int> _decisionCount = new NetworkVariable<int>();
        // Instant (temps serveur NGO) auquel la proposition redevient disponible.
        readonly NetworkVariable<double> _unavailableUntil = new NetworkVariable<double>();

        MaterialPropertyBlock _block;
        int _shownAvailability = -1;

        public float RefuseDurationSeconds => tuning.RefuseDurationSeconds;
        public DrinkDecision LastDecision => _lastDecision.Value;
        public ulong LastDeciderClientId => _lastDeciderClientId.Value;
        public int DecisionCount => _decisionCount.Value;
        public bool IsAvailable => NetworkManager.ServerTime.Time >= _unavailableUntil.Value;
        public float RemainingCooldownSeconds =>
            Mathf.Max(0f, (float)(_unavailableUntil.Value - NetworkManager.ServerTime.Time));

        public override void OnNetworkSpawn()
        {
            if (tuning == null)
                throw new System.InvalidOperationException("DrinkOffer : TuningConfig non assigné.");
            tuning.ThrowIfInvalid();
            _decisionCount.OnValueChanged += OnDecisionCountChanged;
        }

        public override void OnNetworkDespawn()
        {
            _decisionCount.OnValueChanged -= OnDecisionCountChanged;
        }

        public Vector3 ClosestPoint(Vector3 from) => offerCollider.ClosestPoint(from);

        /// <summary>Appelé uniquement par l'hôte après validation ; démarre le délai d'indisponibilité.</summary>
        public void ServerDecide(DrinkDecision decision, ulong deciderClientId)
        {
            if (!IsServer)
                return;
            _lastDecision.Value = decision;
            _lastDeciderClientId.Value = deciderClientId;
            _unavailableUntil.Value = NetworkManager.ServerTime.Time + tuning.NpcOfferCooldownSeconds;
            _decisionCount.Value++;
        }

        void OnDecisionCountChanged(int previous, int current) =>
            Debug.Log($"[P1-012] local={NetworkManager.LocalClientId} decision #{current}: {_lastDecision.Value} by client {_lastDeciderClientId.Value}");

        void Update()
        {
            if (!IsSpawned)
                return;

            int availability = IsAvailable ? 1 : 0;
            if (availability == _shownAvailability)
                return;
            _shownAvailability = availability;
            _block ??= new MaterialPropertyBlock();
            _block.SetColor("_BaseColor", availability == 1 ? availableColor : unavailableColor);
            offerRenderer.SetPropertyBlock(_block);
        }

        void OnGUI()
        {
            if (!IsSpawned)
                return;

            string decision = _decisionCount.Value == 0
                ? "Aucune décision"
                : $"Dernière décision n°{_decisionCount.Value} : joueur {_lastDeciderClientId.Value} a " +
                  (_lastDecision.Value == DrinkDecision.Accepted ? "ACCEPTÉ" : "REFUSÉ");
            string state = IsAvailable ? "Verre disponible" : $"Verre indisponible ({RemainingCooldownSeconds:0} s)";
            GUI.Label(new Rect(Screen.width * 0.5f - 200, 10, 400, 24), decision);
            GUI.Label(new Rect(Screen.width * 0.5f - 200, 32, 400, 24), state);
        }
    }
}

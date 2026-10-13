using LastCallCrew.Config;
using LastCallCrew.Drink;
using Unity.Netcode;
using UnityEngine;

namespace LastCallCrew.Network
{
    /// <summary>
    /// Proposition de verre temporaire (P1-012, corrigée en P1-013a). L'hôte seul décide ;
    /// la dernière décision, son auteur et un compteur sont répliqués pour l'affichage et le diagnostic.
    /// La proposition reste utilisable immédiatement par les deux joueurs : aucun état de disponibilité.
    /// </summary>
    public class DrinkOffer : NetworkBehaviour
    {
        [SerializeField] TuningConfig tuning;
        [SerializeField] Renderer offerRenderer;
        [SerializeField] Collider offerCollider;
        [SerializeField] Color offerColor = new Color(1f, 0.8f, 0.1f);

        readonly NetworkVariable<DrinkDecision> _lastDecision = new NetworkVariable<DrinkDecision>();
        readonly NetworkVariable<ulong> _lastDeciderClientId = new NetworkVariable<ulong>();
        readonly NetworkVariable<int> _decisionCount = new NetworkVariable<int>();

        public float RefuseDurationSeconds => tuning.RefuseDurationSeconds;
        public DrinkDecision LastDecision => _lastDecision.Value;
        public ulong LastDeciderClientId => _lastDeciderClientId.Value;
        public int DecisionCount => _decisionCount.Value;

        public override void OnNetworkSpawn()
        {
            if (tuning == null)
                throw new System.InvalidOperationException("DrinkOffer : TuningConfig non assigné.");
            tuning.ThrowIfInvalid();

            var block = new MaterialPropertyBlock();
            block.SetColor("_BaseColor", offerColor);
            offerRenderer.SetPropertyBlock(block);

            _decisionCount.OnValueChanged += OnDecisionCountChanged;
        }

        public override void OnNetworkDespawn()
        {
            _decisionCount.OnValueChanged -= OnDecisionCountChanged;
        }

        public Vector3 ClosestPoint(Vector3 from) => offerCollider.ClosestPoint(from);

        /// <summary>Appelé uniquement par l'hôte après validation.</summary>
        public void ServerDecide(DrinkDecision decision, ulong deciderClientId)
        {
            if (!IsServer)
                return;
            _lastDecision.Value = decision;
            _lastDeciderClientId.Value = deciderClientId;
            _decisionCount.Value++;
        }

        void OnDecisionCountChanged(int previous, int current) =>
            Debug.Log($"[P1-012] local={NetworkManager.LocalClientId} decision #{current}: {_lastDecision.Value} by client {_lastDeciderClientId.Value}");
    }
}

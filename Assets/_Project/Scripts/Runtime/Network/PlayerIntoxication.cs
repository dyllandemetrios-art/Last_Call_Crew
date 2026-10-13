using LastCallCrew.Config;
using LastCallCrew.Intoxication;
using Unity.Netcode;
using UnityEngine;

namespace LastCallCrew.Network
{
    /// <summary>
    /// Ivresse d'un joueur (P1-013). L'hôte calcule gains et décroissance puis réplique la valeur ;
    /// toutes les instances (hôte compris) lisent la valeur répliquée et en déduisent le palier.
    /// </summary>
    public class PlayerIntoxication : NetworkBehaviour
    {
        [SerializeField] TuningConfig tuning;
        [Tooltip("Intervalle minimal (s) entre deux écritures réseau pendant la décroissance.")]
        [SerializeField] float decayReplicationInterval = 0.5f;
        [Tooltip("Variation minimale de la valeur pour justifier une écriture pendant la décroissance.")]
        [SerializeField] float decayReplicationStep = 0.1f;

        readonly NetworkVariable<float> _value = new NetworkVariable<float>();

        float _serverValue;
        float _nextDecayWriteTime;

        public float Value => _value.Value;
        /// <summary>Maximum de l'échelle (seuil KO du TuningConfig), pour l'affichage.</summary>
        public float MaxValue => tuning.KoThreshold;
        public IntoxicationStage Stage => IntoxicationRules.GetStage(_value.Value, tuning);

        public override void OnNetworkSpawn()
        {
            if (tuning == null)
                throw new System.InvalidOperationException("PlayerIntoxication : TuningConfig non assigné.");
            tuning.ThrowIfInvalid();

            if (IsServer)
            {
                _serverValue = tuning.DrunkennessMin;
                _value.Value = _serverValue;
            }
        }

        /// <summary>Hôte uniquement : appelé par la décision d'acceptation déjà validée (P1-012).</summary>
        public void ServerAddDrink()
        {
            if (!IsServer)
                return;
            float before = _serverValue;
            _serverValue = IntoxicationRules.AddDrink(_serverValue, tuning);
            Write();
            Debug.Log($"[P1-013] host: client {OwnerClientId} drink {before:0.00} -> {_serverValue:0.00} ({IntoxicationRules.GetStage(_serverValue, tuning)})");
        }

        void Update()
        {
            if (!IsServer || !IsSpawned)
                return;

            float previous = _serverValue;
            _serverValue = IntoxicationRules.ApplyDecay(_serverValue, Time.deltaTime, tuning);
            if (_serverValue == previous || _serverValue == _value.Value)
                return;

            bool stageChanged = IntoxicationRules.GetStage(_serverValue, tuning) != Stage;
            bool reachedMinimum = _serverValue <= tuning.DrunkennessMin;
            bool intervalElapsed = Time.time >= _nextDecayWriteTime;
            bool bigEnough = Mathf.Abs(_value.Value - _serverValue) >= decayReplicationStep;

            // Écriture immédiate si le palier change ou si le minimum est atteint ; sinon au plus toutes les 0,5 s.
            if (stageChanged || reachedMinimum || (intervalElapsed && bigEnough))
                Write();
        }

        void Write()
        {
            _value.Value = _serverValue;
            _nextDecayWriteTime = Time.time + decayReplicationInterval;
        }
    }
}

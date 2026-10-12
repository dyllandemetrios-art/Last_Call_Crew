using System;
using System.Collections.Generic;
using UnityEngine;

namespace LastCallCrew.Config
{
    /// <summary>
    /// Réglages provisoires de la boucle P1 (ivresse, refus, KO, entraide).
    /// Les valeurs sont modifiables dans l'Inspector uniquement ; le code ne fait que les lire.
    /// Ce sont des points de départ du GDD, pas des décisions de game design définitives.
    /// </summary>
    [CreateAssetMenu(fileName = "TuningConfig", menuName = "Last Call Crew/Tuning Config")]
    public class TuningConfig : ScriptableObject
    {
        [Header("Ivresse (points sur l'échelle 0-100)")]
        [Tooltip("Valeur minimale de l'échelle d'ivresse.")]
        [SerializeField] float _drunkennessMin = 0f;
        [Tooltip("Seuil à partir duquel le joueur est Pompette.")]
        [SerializeField] float _pompetteThreshold = 30f;
        [Tooltip("Seuil à partir duquel le joueur est Ivre.")]
        [SerializeField] float _ivreThreshold = 60f;
        [Tooltip("Valeur maximale de l'échelle. Atteindre cette valeur provoque le KO.")]
        [SerializeField] float _koThreshold = 100f;
        [Tooltip("Points d'ivresse gagnés par verre bu.")]
        [SerializeField] float _gainPerDrink = 20f;
        [Tooltip("Points d'ivresse perdus par seconde, passivement.")]
        [SerializeField] float _passiveDecayPerSecond = 0.2f;
        [Tooltip("Points d'ivresse retirés par une eau bue hors réveil.")]
        [SerializeField] float _waterReduction = 15f;

        [Header("Refus d'un verre")]
        [Tooltip("Durée de maintien normale pour refuser, en secondes.")]
        [SerializeField] float _refuseDurationSeconds = 2f;
        [Tooltip("Multiplicateur de la durée de refus pour le profil Sociable (x la durée normale).")]
        [SerializeField] float _sociableRefuseMultiplier = 2f;
        [Tooltip("Délai minimal entre deux propositions du même PNJ, en secondes.")]
        [SerializeField] float _npcOfferCooldownSeconds = 20f;

        [Header("KO et entraide")]
        [Tooltip("Durée de maintien pour réveiller un joueur KO avec de l'eau, en secondes.")]
        [SerializeField] float _waterWakeHoldSeconds = 3f;
        [Tooltip("Ivresse (0-100) après un réveil avec de l'eau.")]
        [SerializeField] float _drunkennessAfterWaterWake = 50f;
        [Tooltip("Délai avant le réveil automatique d'un joueur KO, en secondes.")]
        [SerializeField] float _autoWakeSeconds = 45f;
        [Tooltip("Ivresse (0-100) après un réveil automatique.")]
        [SerializeField] float _drunkennessAfterAutoWake = 90f;
        [Tooltip("Multiplicateur de vitesse du porteur pendant le portage (0 exclu, 1 = vitesse normale).")]
        [SerializeField] float _carrySpeedMultiplier = 0.6f;

        public float DrunkennessMin => _drunkennessMin;
        public float PompetteThreshold => _pompetteThreshold;
        public float IvreThreshold => _ivreThreshold;
        /// <summary>Maximum de l'échelle et seuil de KO (même valeur).</summary>
        public float KoThreshold => _koThreshold;
        public float GainPerDrink => _gainPerDrink;
        public float PassiveDecayPerSecond => _passiveDecayPerSecond;
        public float WaterReduction => _waterReduction;

        public float RefuseDurationSeconds => _refuseDurationSeconds;
        public float SociableRefuseMultiplier => _sociableRefuseMultiplier;
        public float NpcOfferCooldownSeconds => _npcOfferCooldownSeconds;

        public float WaterWakeHoldSeconds => _waterWakeHoldSeconds;
        public float DrunkennessAfterWaterWake => _drunkennessAfterWaterWake;
        public float AutoWakeSeconds => _autoWakeSeconds;
        public float DrunkennessAfterAutoWake => _drunkennessAfterAutoWake;
        public float CarrySpeedMultiplier => _carrySpeedMultiplier;

        public bool IsValid => GetValidationErrors().Count == 0;

        /// <summary>Liste les invariants violés ; vide si la configuration est cohérente.</summary>
        public IReadOnlyList<string> GetValidationErrors()
        {
            var errors = new List<string>();

            if (!(_drunkennessMin < _pompetteThreshold))
                errors.Add("Le minimum d'ivresse doit être inférieur au seuil Pompette.");
            if (!(_pompetteThreshold < _ivreThreshold))
                errors.Add("Le seuil Pompette doit être inférieur au seuil Ivre.");
            if (!(_ivreThreshold < _koThreshold))
                errors.Add("Le seuil Ivre doit être inférieur au seuil KO.");

            if (!(_gainPerDrink > 0f))
                errors.Add("Le gain par verre doit être strictement positif.");
            if (!(_passiveDecayPerSecond >= 0f))
                errors.Add("La décroissance passive ne peut pas être négative.");
            if (!(_waterReduction > 0f))
                errors.Add("La réduction par l'eau doit être strictement positive.");

            if (!(_refuseDurationSeconds > 0f))
                errors.Add("La durée de refus doit être strictement positive.");
            if (!(_sociableRefuseMultiplier > 0f))
                errors.Add("Le multiplicateur Sociable doit être strictement positif.");
            if (!(_npcOfferCooldownSeconds > 0f))
                errors.Add("Le délai entre deux propositions d'un PNJ doit être strictement positif.");
            if (!(_waterWakeHoldSeconds > 0f))
                errors.Add("La durée de maintien du réveil à l'eau doit être strictement positive.");
            if (!(_autoWakeSeconds > 0f))
                errors.Add("Le délai de réveil automatique doit être strictement positif.");

            if (!InDrunkennessRange(_drunkennessAfterWaterWake))
                errors.Add("L'ivresse après réveil à l'eau doit être comprise entre le minimum et le seuil KO.");
            if (!InDrunkennessRange(_drunkennessAfterAutoWake))
                errors.Add("L'ivresse après réveil automatique doit être comprise entre le minimum et le seuil KO.");

            if (!(_carrySpeedMultiplier > 0f && _carrySpeedMultiplier <= 1f))
                errors.Add("Le multiplicateur de vitesse de portage doit être dans ]0 ; 1].");

            return errors;
        }

        /// <summary>À appeler par les systèmes qui consomment la config : refuse une configuration incohérente.</summary>
        public void ThrowIfInvalid()
        {
            IReadOnlyList<string> errors = GetValidationErrors();
            if (errors.Count > 0)
                throw new InvalidOperationException($"TuningConfig '{name}' invalide :\n- {string.Join("\n- ", errors)}");
        }

        bool InDrunkennessRange(float value) => value >= _drunkennessMin && value <= _koThreshold;

        void OnValidate()
        {
            foreach (string error in GetValidationErrors())
                Debug.LogError($"[TuningConfig '{name}'] {error}", this);
        }
    }
}

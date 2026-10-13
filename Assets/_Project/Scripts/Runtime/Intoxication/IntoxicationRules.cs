using LastCallCrew.Config;
using UnityEngine;

namespace LastCallCrew.Intoxication
{
    /// <summary>Paliers d'ivresse, déduits de la valeur et des seuils du TuningConfig.</summary>
    public enum IntoxicationStage : byte
    {
        Sober = 0,
        Pompette = 1,
        Ivre = 2,
        KoThreshold = 3,
    }

    /// <summary>Calculs purs d'ivresse (sans NGO) : bornes, gain d'un verre, décroissance, palier.</summary>
    public static class IntoxicationRules
    {
        public static float Clamp(float value, TuningConfig tuning)
        {
            if (float.IsNaN(value))
                return tuning.DrunkennessMin;
            return Mathf.Clamp(value, tuning.DrunkennessMin, tuning.KoThreshold);
        }

        public static float AddDrink(float value, TuningConfig tuning) =>
            Clamp(value + tuning.GainPerDrink, tuning);

        /// <summary>
        /// Applique la décroissance passive pour un temps écoulé ; un temps négatif ou nul ne change rien.
        /// Au seuil KO la valeur est figée : elle ne décroît pas tant qu'un système explicite ne la modifie pas (P1-013a).
        /// </summary>
        public static float ApplyDecay(float value, float elapsedSeconds, TuningConfig tuning)
        {
            float clamped = Clamp(value, tuning);
            if (clamped >= tuning.KoThreshold || !(elapsedSeconds > 0f))
                return clamped;
            return Clamp(clamped - tuning.PassiveDecayPerSecond * elapsedSeconds, tuning);
        }

        public static IntoxicationStage GetStage(float value, TuningConfig tuning)
        {
            if (value >= tuning.KoThreshold)
                return IntoxicationStage.KoThreshold;
            if (value >= tuning.IvreThreshold)
                return IntoxicationStage.Ivre;
            if (value >= tuning.PompetteThreshold)
                return IntoxicationStage.Pompette;
            return IntoxicationStage.Sober;
        }

        public static string Label(IntoxicationStage stage)
        {
            switch (stage)
            {
                case IntoxicationStage.Pompette: return "Pompette";
                case IntoxicationStage.Ivre: return "Ivre";
                case IntoxicationStage.KoThreshold: return "Seuil KO";
                default: return "Sobre";
            }
        }
    }
}

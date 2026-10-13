using LastCallCrew.Config;

namespace LastCallCrew.Intoxication
{
    /// <summary>Règle pure d'entrée en KO (P1-020), évaluée par l'hôte.</summary>
    public static class KnockoutRules
    {
        /// <summary>Vrai uniquement au franchissement : valeur au seuil KO (ou au-dessus) et joueur pas déjà KO.</summary>
        public static bool ShouldEnterKnockout(float intoxication, TuningConfig tuning, bool alreadyKnockedOut)
        {
            if (alreadyKnockedOut || float.IsNaN(intoxication))
                return false;
            return intoxication >= tuning.KoThreshold;
        }
    }
}

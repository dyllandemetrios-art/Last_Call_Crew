namespace LastCallCrew.Night
{
    /// <summary>
    /// Règles pures du retour collectif (P1-031), sans NGO. L'instant de retour est un temps serveur
    /// (même horloge que l'échéance du chronomètre) ; <see cref="NotReturned"/> est la sentinelle « pas encore rentré ».
    /// </summary>
    public static class ReturnRules
    {
        /// <summary>Instant de retour d'un joueur qui n'est pas rentré (le temps serveur est toujours ≥ 0).</summary>
        public const double NotReturned = -1d;

        public static bool IsReturned(double returnTime) => returnTime > NotReturned;

        /// <summary>Un retour n'est enregistré qu'une seule fois : seulement si aucun instant n'existe encore.</summary>
        public static bool ShouldRecord(double currentReturnTime) => !IsReturned(currentReturnTime);

        /// <summary>Instant conservé après une tentative d'enregistrement : le premier est toujours gardé.</summary>
        public static double Record(double currentReturnTime, double serverTimeNow) =>
            ShouldRecord(currentReturnTime) ? serverTimeNow : currentReturnTime;

        /// <summary>Libellé de la carte joueur.</summary>
        public static string Label(double returnTime) => IsReturned(returnTime) ? "RENTRÉ" : "ENCORE DEHORS";
    }
}

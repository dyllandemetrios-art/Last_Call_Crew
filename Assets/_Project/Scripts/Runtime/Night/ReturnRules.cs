namespace LastCallCrew.Night
{
    /// <summary>
    /// Règles pures du retour collectif (P1-031, P1-031a), sans NGO. L'instant de retour est le début de la présence actuelle dans la zone, en temps serveur
    /// (même horloge que l'échéance du chronomètre) ; <see cref="NotReturned"/> est la sentinelle « pas encore rentré ».
    /// </summary>
    public static class ReturnRules
    {
        /// <summary>Instant de retour d'un joueur qui n'est pas rentré (le temps serveur est toujours ≥ 0).</summary>
        public const double NotReturned = -1d;

        public static bool IsReturned(double returnTime) => returnTime > NotReturned;

        /// <summary>
        /// Instant de retour après une évaluation de présence (P1-031a). Présent : l'instant de début de la présence actuelle
        /// est conservé s'il existe, sinon <paramref name="serverTimeNow"/> est enregistré. Absent : le retour est effacé,
        /// une nouvelle entrée enregistrera un nouvel instant.
        /// </summary>
        public static double Next(double currentReturnTime, bool present, double serverTimeNow)
        {
            if (!present)
                return NotReturned;
            return IsReturned(currentReturnTime) ? currentReturnTime : serverTimeNow;
        }

        /// <summary>Libellé de la carte joueur.</summary>
        public static string Label(double returnTime) => IsReturned(returnTime) ? "RENTRÉ" : "ENCORE DEHORS";
    }
}

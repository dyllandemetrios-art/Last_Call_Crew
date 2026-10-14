namespace LastCallCrew.Night
{
    /// <summary>
    /// Règles pures du portefeuille de la maison (P1-052). Même principe collectif que les clés : les types de demande et
    /// de résultat (<see cref="KeyRequest"/>, <see cref="KeyOutcome"/>) et la validation de <see cref="KeyRules"/> sont réutilisés
    /// tels quels (aucune duplication de logique) ; l'état du portefeuille reste indépendant de celui des clés.
    /// </summary>
    public static class WalletRules
    {
        public static KeyOutcome Evaluate(bool alreadyTaken, in KeyRequest r) => KeyRules.Evaluate(alreadyTaken, r);

        public static bool NextTaken(bool taken, KeyOutcome outcome) => KeyRules.NextTaken(taken, outcome);

        public static bool AfterRoundReset() => KeyRules.AfterRoundReset();

        public static bool AfterRequesterDisconnected(bool taken) => KeyRules.AfterRequesterDisconnected(taken);

        public static string StatusLabel(bool taken) => taken ? "PORTEFEUILLE · PRIS" : "PORTEFEUILLE · À PRENDRE";
    }
}

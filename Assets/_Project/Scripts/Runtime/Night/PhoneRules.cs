namespace LastCallCrew.Night
{
    /// <summary>
    /// Règles pures du téléphone de la maison (P1-053) : mêmes validations collectives que les clés et le portefeuille
    /// (<see cref="KeyRules"/> réutilisé tel quel), état indépendant.
    /// </summary>
    public static class PhoneRules
    {
        public static KeyOutcome Evaluate(bool alreadyTaken, in KeyRequest r) => KeyRules.Evaluate(alreadyTaken, r);

        public static bool NextTaken(bool taken, KeyOutcome outcome) => KeyRules.NextTaken(taken, outcome);

        public static bool AfterRoundReset() => KeyRules.AfterRoundReset();

        public static bool AfterRequesterDisconnected(bool taken) => KeyRules.AfterRequesterDisconnected(taken);

        public static string StatusLabel(bool taken) => taken ? "TÉLÉPHONE · PRIS" : "TÉLÉPHONE · À PRENDRE";
    }

    /// <summary>
    /// Checklist de départ (P1-053) : clés + portefeuille + téléphone. Trois états indépendants lus ensemble ; ce n'est qu'une
    /// information (elle n'empêche pas de sortir) et ne devient pas un inventaire général.
    /// </summary>
    public static class DepartureChecklist
    {
        public static bool AllTaken(bool keys, bool wallet, bool phone) => keys && wallet && phone;

        public static int TakenCount(bool keys, bool wallet, bool phone) => (keys ? 1 : 0) + (wallet ? 1 : 0) + (phone ? 1 : 0);

        /// <summary>« PRÊTS À PARTIR » n'est annoncé qu'au passage de « pas tout » à « tout » (jamais au rattrapage d'un arrivant).</summary>
        public static bool BecameReady(bool wasReady, bool isReady) => !wasReady && isReady;

        public const string ReadyStatus = "DÉPART · PRÊT";
        public const string ReadyAnnouncement = "PRÊTS À PARTIR";
    }
}

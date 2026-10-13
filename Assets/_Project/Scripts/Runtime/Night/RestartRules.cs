namespace LastCallCrew.Night
{
    /// <summary>Issue d'une demande de relance, jugée par l'hôte (P1-033).</summary>
    public enum RestartOutcome : byte
    {
        Restarted,
        RejectedNotHost,
        RejectedNotFinished,
    }

    /// <summary>Règles pures du gel et de la relance (sans NGO).</summary>
    public static class RestartRules
    {
        /// <summary>Le gameplay est gelé dès qu'un résultat terminal existe, jusqu'à la relance.</summary>
        public static bool IsFrozen(NightResult result) => result != NightResult.Pending;

        /// <summary>Seul l'hôte relance, et seulement après un résultat : une seconde demande (déjà relancé) est ignorée.</summary>
        public static RestartOutcome Evaluate(bool senderIsHost, NightResult result)
        {
            if (!senderIsHost)
                return RestartOutcome.RejectedNotHost;
            if (!IsFrozen(result))
                return RestartOutcome.RejectedNotFinished;
            return RestartOutcome.Restarted;
        }

        /// <summary>Position de départ d'un joueur (même formule que PlayerController, factorisée pour la relance).</summary>
        public static UnityEngine.Vector3 StartPosition(ulong clientId) =>
            new UnityEngine.Vector3(clientId * 3f, 1f, 0f);

        public static string FinalState(bool returned, bool knockedOut) =>
            (returned ? "RENTRÉ" : "DEHORS") + " · " + (knockedOut ? "KO" : "DEBOUT");
    }
}

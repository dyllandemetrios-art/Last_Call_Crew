namespace LastCallCrew.Night
{
    /// <summary>Issue d'une demande de relance, jugée par l'hôte (P1-033).</summary>
    public enum RestartOutcome : byte
    {
        Restarted,
        RejectedNotHost,
        RejectedNotFinished,
        /// <summary>P1-040 : la relance attend que les deux joueurs soient de nouveau connectés.</summary>
        RejectedNotEnoughPlayers,
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

        public const int PlayersRequired = 2;

        /// <summary>Comme <see cref="Evaluate(bool, NightResult)"/>, avec en plus le nombre de joueurs actuellement connectés.</summary>
        public static RestartOutcome Evaluate(bool senderIsHost, NightResult result, int connectedPlayers)
        {
            RestartOutcome basic = Evaluate(senderIsHost, result);
            if (basic != RestartOutcome.Restarted)
                return basic;
            return connectedPlayers < PlayersRequired ? RestartOutcome.RejectedNotEnoughPlayers : RestartOutcome.Restarted;
        }

        /// <summary>Orientation de départ (P1-050) : vers l'ouest, face à la porte de la maison.</summary>
        public const float StartYaw = 270f;

        /// <summary>
        /// Position de départ d'un emplacement (P1-040, P1-050) : dans la maison (centre vers (20 ; 16)), en file devant la
        /// porte ouest, 1,6 m d'écart. L'emplacement vient de l'ordre des joueurs connectés (<see cref="PlayerSlots"/>),
        /// jamais d'un ClientId NGO.
        /// </summary>
        public static UnityEngine.Vector3 StartPosition(int slot) =>
            new UnityEngine.Vector3(21f, 1f, 15.2f + 1.6f * slot);

        public static string FinalState(bool returned, bool knockedOut) =>
            (returned ? "RENTRÉ" : "DEHORS") + " · " + (knockedOut ? "KO" : "DEBOUT");
    }
}

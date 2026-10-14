namespace LastCallCrew.Night
{
    /// <summary>Issue d'une demande de prise des clés, jugée par l'hôte (P1-051).</summary>
    public enum KeyOutcome : byte
    {
        Taken,
        RejectedAlreadyTaken,
        RejectedInvalidSender,
        RejectedInvalidTarget,
        RejectedFrozen,
        RejectedKnockedOut,
        RejectedCarried,
        RejectedCarrying,
        RejectedOutOfRange,
    }

    /// <summary>État du joueur et de la cible au moment d'une demande, lu par l'hôte.</summary>
    public struct KeyRequest
    {
        public bool InvalidSender;
        public bool InvalidTarget;
        public bool Frozen;
        public bool KnockedOut;
        public bool Carried;
        public bool Carrying;
        public float Distance;
        public float MaxDistance;
    }

    /// <summary>
    /// Règles pures des clés de la maison (P1-051, sans NGO) : un objectif collectif, une seule prise par manche.
    /// L'état appartient à la manche, pas au joueur : une déconnexion ne le change pas, la relance le remet à zéro.
    /// </summary>
    public static class KeyRules
    {
        /// <summary>Validation d'une demande par l'hôte. Les clés déjà prises ne produisent aucune seconde transition.</summary>
        public static KeyOutcome Evaluate(bool alreadyTaken, in KeyRequest r)
        {
            if (r.InvalidSender) return KeyOutcome.RejectedInvalidSender;
            if (r.InvalidTarget) return KeyOutcome.RejectedInvalidTarget;
            if (r.Frozen) return KeyOutcome.RejectedFrozen;
            if (r.KnockedOut) return KeyOutcome.RejectedKnockedOut;
            if (r.Carried) return KeyOutcome.RejectedCarried;
            if (r.Carrying) return KeyOutcome.RejectedCarrying;
            if (alreadyTaken) return KeyOutcome.RejectedAlreadyTaken;
            if (!(r.Distance <= r.MaxDistance)) return KeyOutcome.RejectedOutOfRange;
            return KeyOutcome.Taken;
        }

        /// <summary>État collectif après une demande : seule une prise acceptée le fait passer à « prises ».</summary>
        public static bool NextTaken(bool taken, KeyOutcome outcome) => taken || outcome == KeyOutcome.Taken;

        /// <summary>Relance de manche : les clés sont de nouveau à prendre.</summary>
        public static bool AfterRoundReset() => false;

        /// <summary>Déconnexion du demandeur après une prise : l'état collectif est conservé.</summary>
        public static bool AfterRequesterDisconnected(bool taken) => taken;

        public static string StatusLabel(bool taken) => taken ? "CLÉS · PRISES" : "CLÉS · À PRENDRE";
    }
}

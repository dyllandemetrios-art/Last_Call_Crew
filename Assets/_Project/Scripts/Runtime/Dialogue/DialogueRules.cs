namespace LastCallCrew.Dialogue
{
    /// <summary>Résultat de la validation d'une demande de dialogue par l'hôte (P1-014).</summary>
    public enum DialogueOutcome : byte
    {
        Opened,
        Chosen,
        Closed,
        RejectedInvalidSender,
        RejectedInvalidTarget,
        RejectedKnockedOut,
        RejectedCarried,
        RejectedCarrying,
        RejectedFrozen,
        RejectedOutOfRange,
        RejectedNotOpen,
        RejectedInvalidChoice,
    }

    /// <summary>État du joueur et de la cible au moment d'une demande, lu par l'hôte.</summary>
    public struct DialogueRequest
    {
        public bool InvalidSender;
        public bool InvalidTarget;
        public bool KnockedOut;
        public bool Carried;
        public bool Carrying;
        public bool Frozen;
        public bool DialogueOpen;
        public float Distance;
        public float MaxDistance;
    }

    /// <summary>Règles pures du premier dialogue à choix (sans NGO). Aucun cas général : un PNJ, trois réponses.</summary>
    public static class DialogueRules
    {
        public const int ChoiceCount = 3;

        public static bool IsValidChoice(int choice) => choice >= 1 && choice <= ChoiceCount;

        /// <summary>Conditions communes à l'ouverture et au choix : identité, cible, KO, portage, gel, portée.</summary>
        static DialogueOutcome Common(in DialogueRequest r)
        {
            if (r.InvalidSender) return DialogueOutcome.RejectedInvalidSender;
            if (r.InvalidTarget) return DialogueOutcome.RejectedInvalidTarget;
            if (r.Frozen) return DialogueOutcome.RejectedFrozen;
            if (r.KnockedOut) return DialogueOutcome.RejectedKnockedOut;
            if (r.Carried) return DialogueOutcome.RejectedCarried;
            if (r.Carrying) return DialogueOutcome.RejectedCarrying;
            if (!(r.Distance <= r.MaxDistance)) return DialogueOutcome.RejectedOutOfRange;
            return DialogueOutcome.Opened;
        }

        public static DialogueOutcome EvaluateOpen(in DialogueRequest r) => Common(r);

        /// <summary>Un choix n'est accepté que si le dialogue a été ouvert côté hôte et que toutes les conditions tiennent encore.</summary>
        public static DialogueOutcome EvaluateChoice(in DialogueRequest r, int choice)
        {
            DialogueOutcome common = Common(r);
            if (common != DialogueOutcome.Opened) return common;
            if (!r.DialogueOpen) return DialogueOutcome.RejectedNotOpen;
            if (!IsValidChoice(choice)) return DialogueOutcome.RejectedInvalidChoice;
            return DialogueOutcome.Chosen;
        }
    }
}

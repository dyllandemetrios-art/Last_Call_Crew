namespace LastCallCrew.Drink
{
    /// <summary>Décision prise sur une proposition de verre.</summary>
    public enum DrinkDecision : byte
    {
        None = 0,
        Accepted = 1,
        Refused = 2,
    }

    /// <summary>Résultat de la validation d'une demande par l'hôte.</summary>
    public enum DrinkRequestOutcome : byte
    {
        Accepted,
        Refused,
        RejectedInvalidSender,
        RejectedInvalidTarget,
        /// <summary>Demande émise par un joueur KO (P1-020).</summary>
        RejectedKnockedOut,
        RejectedOutOfRange,
        /// <summary>Refus relâché avant la durée requise : annulé, aucune décision.</summary>
        HoldTooShort,
        /// <summary>Fin de refus reçue sans début valide.</summary>
        RejectedNoRefusalStarted,
    }

    /// <summary>Règles de décision pures (sans NGO), évaluées par l'hôte.</summary>
    public static class DrinkRules
    {
        public static DrinkRequestOutcome EvaluateAccept(float distance, float maxDistance)
        {
            if (!(distance <= maxDistance))
                return DrinkRequestOutcome.RejectedOutOfRange;
            return DrinkRequestOutcome.Accepted;
        }

        /// <param name="heldSeconds">Durée du maintien mesurée par l'hôte.</param>
        /// <param name="requiredSeconds">Durée de refus configurée.</param>
        /// <param name="toleranceSeconds">Marge pour la gigue réseau (>= 0).</param>
        public static DrinkRequestOutcome EvaluateRefuse(
            float distance, float maxDistance,
            double heldSeconds, float requiredSeconds, float toleranceSeconds)
        {
            if (!(distance <= maxDistance))
                return DrinkRequestOutcome.RejectedOutOfRange;
            if (!(heldSeconds >= requiredSeconds - toleranceSeconds))
                return DrinkRequestOutcome.HoldTooShort;
            return DrinkRequestOutcome.Refused;
        }
    }
}

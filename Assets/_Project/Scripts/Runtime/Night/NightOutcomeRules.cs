using System.Collections.Generic;

namespace LastCallCrew.Night
{
    /// <summary>Résultat de la nuit (P1-032). Terminal : seul Pending peut encore changer.</summary>
    public enum NightResult : byte
    {
        Pending = 0,
        Victory = 1,
        Failure = 2,
    }

    /// <summary>Raison du résultat (P1-040), répliquée avec lui et écrite une seule fois : le HUD ne la déduit pas d'un texte.</summary>
    public enum NightReason : byte
    {
        None = 0,
        /// <summary>Résultat issu de la comparaison retours / échéance (P1-032).</summary>
        Deadline = 1,
        /// <summary>Un client s'est déconnecté pendant une manche active.</summary>
        PartnerDisconnected = 2,
    }

    /// <summary>
    /// Règles pures de conclusion de la nuit, sans NGO. La comparaison utilise uniquement les instants serveur
    /// (retour de chaque joueur et échéance), jamais l'ordre d'exécution des mises à jour.
    /// </summary>
    public static class NightOutcomeRules
    {
        public const int MinPlayersForVictory = 2;

        /// <summary>
        /// Victoire : au moins deux joueurs et tous ont un instant de retour ≤ échéance (égalité acceptée).
        /// Échec : chronomètre expiré sans victoire. Sinon Pending. Un chronomètre en attente ne conclut rien.
        /// </summary>
        public static NightResult Evaluate(NightState timerState, double deadline, IReadOnlyList<double> returnTimes)
        {
            if (timerState == NightState.Waiting)
                return NightResult.Pending;

            if (AllReturnedBy(deadline, returnTimes))
                return NightResult.Victory;

            return timerState == NightState.Expired ? NightResult.Failure : NightResult.Pending;
        }

        public static bool AllReturnedBy(double deadline, IReadOnlyList<double> returnTimes)
        {
            if (returnTimes == null || returnTimes.Count < MinPlayersForVictory)
                return false;
            for (int i = 0; i < returnTimes.Count; i++)
            {
                double t = returnTimes[i];
                if (!ReturnRules.IsReturned(t) || t > deadline)
                    return false;
            }
            return true;
        }

        /// <summary>Le premier résultat terminal gagne : un résultat déjà décidé n'est jamais remplacé.</summary>
        public static NightResult Next(NightResult current, NightResult evaluated) =>
            current != NightResult.Pending ? current : evaluated;

        /// <summary>
        /// Départ d'un client (P1-040). Avant le démarrage de la manche (chronomètre en attente) : rien ne change.
        /// Manche active (en cours ou échéance atteinte) sans résultat : échec immédiat. Résultat déjà décidé : inchangé.
        /// </summary>
        public static NightResult OnClientDisconnected(NightState timerState, NightResult current)
        {
            if (current != NightResult.Pending || timerState == NightState.Waiting)
                return current;
            return NightResult.Failure;
        }

        public static string Banner(NightResult result, NightReason reason)
        {
            if (result == NightResult.Failure && reason == NightReason.PartnerDisconnected)
                return "ÉCHEC — PARTENAIRE DÉCONNECTÉ";
            return Banner(result);
        }

        public static string Banner(NightResult result)
        {
            switch (result)
            {
                case NightResult.Victory: return "VICTOIRE — TOUT LE MONDE EST RENTRÉ";
                case NightResult.Failure: return "ÉCHEC — QUELQU'UN EST RESTÉ DEHORS";
                default: return string.Empty;
            }
        }
    }
}

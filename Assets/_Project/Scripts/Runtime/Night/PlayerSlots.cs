using System.Collections.Generic;

namespace LastCallCrew.Night
{
    /// <summary>
    /// Emplacements de départ des joueurs (P1-040), indépendants des ClientId NGO. Règles pures, sans NGO.
    /// </summary>
    public static class PlayerSlots
    {
        /// <summary>Premier emplacement non utilisé (0, 1, 2…) : un joueur qui rejoint reprend l'emplacement libéré.</summary>
        public static int FirstFree(IEnumerable<int> usedSlots)
        {
            var used = new HashSet<int>(usedSlots);
            int slot = 0;
            while (used.Contains(slot))
                slot++;
            return slot;
        }

        /// <summary>
        /// Emplacement de chaque joueur connecté à la relance : rang par ClientId croissant (l'hôte, d'identifiant 0,
        /// prend l'emplacement 0). Ordre déterministe, identique quelles que soient les valeurs des identifiants.
        /// </summary>
        public static int Rank(ulong clientId, IReadOnlyList<ulong> connectedClientIds)
        {
            int rank = 0;
            for (int i = 0; i < connectedClientIds.Count; i++)
                if (connectedClientIds[i] < clientId)
                    rank++;
            return rank;
        }
    }
}

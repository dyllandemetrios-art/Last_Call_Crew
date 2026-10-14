using System.Collections.Generic;

namespace LastCallCrew.Night
{
    /// <summary>
    /// Activation du retour (P1-050), logique pure sans NGO. Suit, pour chaque joueur de la manche, s'il a déjà quitté
    /// la maison ; le retour s'active une seule fois, quand tous les joueurs de la manche sont sortis au moins une fois.
    /// Irréversible jusqu'à <see cref="Reset"/> (relance). Hôte seul : l'état partagé est écrit par l'hôte.
    /// </summary>
    public sealed class HouseExitTracker
    {
        readonly HashSet<ulong> _exited = new HashSet<ulong>();

        public bool Active { get; private set; }

        public bool HasExited(ulong clientId) => _exited.Contains(clientId);

        /// <summary>Observe la position d'un joueur : être dehors une fois suffit, y revenir n'efface rien.</summary>
        public void Observe(ulong clientId, bool insideHouse)
        {
            if (!insideHouse)
                _exited.Add(clientId);
        }

        /// <summary>
        /// Vrai uniquement à l'instant de l'activation : au moins <see cref="RestartRules.PlayersRequired"/> joueurs et
        /// tous sortis au moins une fois. Déjà actif : toujours faux (une seule activation).
        /// </summary>
        public bool TryActivate(IReadOnlyList<ulong> roster)
        {
            if (Active || roster == null || roster.Count < RestartRules.PlayersRequired)
                return false;
            for (int i = 0; i < roster.Count; i++)
                if (!_exited.Contains(roster[i]))
                    return false;
            Active = true;
            return true;
        }

        /// <summary>Relance : sorties mémorisées et activation effacées.</summary>
        public void Reset()
        {
            _exited.Clear();
            Active = false;
        }
    }
}

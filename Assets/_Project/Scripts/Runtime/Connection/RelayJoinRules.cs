namespace LastCallCrew.Connection
{
    /// <summary>Phase locale de la connexion en ligne (P1-003). Aucune autorité : simple état d'interface.</summary>
    public enum ConnectionPhase : byte
    {
        Idle,
        Connecting,
        Hosting,
        Joined,
        Stopping,
    }

    /// <summary>Règles pures de la connexion par code (sans Unity ni service) : normalisation du code et transitions locales.</summary>
    public static class RelayJoinRules
    {
        public const int CodeMaxLength = 12;

        /// <summary>Code saisi → forme comparée par le service : espaces retirés aux extrémités, majuscules.</summary>
        public static string Normalize(string raw) =>
            string.IsNullOrWhiteSpace(raw) ? string.Empty : raw.Trim().ToUpperInvariant();

        /// <summary>Un code plausible est non vide, court et alphanumérique ; le service reste seul juge de son existence.</summary>
        public static bool IsPlausibleCode(string normalized)
        {
            if (string.IsNullOrEmpty(normalized) || normalized.Length > CodeMaxLength)
                return false;
            foreach (char c in normalized)
                if (!((c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9')))
                    return false;
            return true;
        }

        /// <summary>Héberger ou rejoindre n'est possible que sans opération en cours ni session active (anti double-clic).</summary>
        public static bool CanBegin(ConnectionPhase phase) => phase == ConnectionPhase.Idle;

        public static bool CanStop(ConnectionPhase phase) => phase == ConnectionPhase.Hosting || phase == ConnectionPhase.Joined;

        public static string PhaseLabel(ConnectionPhase phase)
        {
            switch (phase)
            {
                case ConnectionPhase.Connecting: return "CONNEXION…";
                case ConnectionPhase.Hosting: return "HÔTE EN LIGNE";
                case ConnectionPhase.Joined: return "CONNECTÉ";
                case ConnectionPhase.Stopping: return "ARRÊT…";
                default: return "HORS LIGNE";
            }
        }
    }
}

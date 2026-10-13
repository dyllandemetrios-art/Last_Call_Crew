using LastCallCrew.Config;

namespace LastCallCrew.Night
{
    /// <summary>État durable du chronomètre de nuit (P1-030).</summary>
    public enum NightState : byte
    {
        Waiting = 0,
        Running = 1,
        Expired = 2,
    }

    /// <summary>Règles pures du chronomètre (sans NGO) : départ, échéance, temps restant, affichage.</summary>
    public static class NightTimerRules
    {
        public const int PlayersToStart = 2;
        public const float FinalCountdownSeconds = 10f;

        /// <summary>Le départ n'a lieu qu'une fois : seulement depuis l'attente, avec assez de joueurs connectés.</summary>
        public static bool ShouldStart(NightState state, int connectedPlayers) =>
            state == NightState.Waiting && connectedPlayers >= PlayersToStart;

        public static double Deadline(double serverTimeNow, TuningConfig tuning) =>
            serverTimeNow + tuning.NightDurationSeconds;

        public static bool ShouldExpire(NightState state, double serverTimeNow, double deadline) =>
            state == NightState.Running && serverTimeNow >= deadline;

        /// <summary>Temps restant, jamais négatif ; la durée complète en attente, 0 une fois expiré.</summary>
        public static float Remaining(NightState state, double serverTimeNow, double deadline, TuningConfig tuning)
        {
            switch (state)
            {
                case NightState.Running: return (float)System.Math.Max(0d, deadline - serverTimeNow);
                case NightState.Expired: return 0f;
                default: return tuning.NightDurationSeconds;
            }
        }

        /// <summary>Un chronomètre en cours dont l'échéance est passée s'affiche déjà expiré, sans attendre l'état répliqué.</summary>
        public static bool IsExpiredForDisplay(NightState state, float remaining) =>
            state == NightState.Expired || (state == NightState.Running && remaining <= 0f);

        public static bool IsFinalCountdown(NightState state, float remaining) =>
            state == NightState.Running && remaining > 0f && remaining <= FinalCountdownSeconds;

        /// <summary>MM:SS, arrondi à la seconde supérieure (00:00 seulement à zéro).</summary>
        public static string Format(float remainingSeconds)
        {
            int total = remainingSeconds > 0f ? (int)System.Math.Ceiling(remainingSeconds) : 0;
            return $"{total / 60:00}:{total % 60:00}";
        }
    }
}

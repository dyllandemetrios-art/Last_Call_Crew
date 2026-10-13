using LastCallCrew.Config;
using UnityEngine;

namespace LastCallCrew.Wake
{
    /// <summary>Résultat de la validation d'un réveil manuel par l'hôte.</summary>
    public enum WakeOutcome : byte
    {
        HoldStarted,
        WokenByWater,
        RejectedInvalidSender,
        RejectedInvalidTarget,
        RejectedSelf,
        RejectedRequesterKnockedOut,
        RejectedRequesterCarried,
        RejectedRequesterCarrying,
        RejectedTargetNotKnockedOut,
        RejectedTargetCarried,
        RejectedOutOfRange,
        RejectedNoHoldStarted,
        HoldTooShort,
    }

    /// <summary>Origine d'un réveil.</summary>
    public enum WakeCause : byte
    {
        Water,
        Auto,
    }

    /// <summary>État des deux joueurs lu par l'hôte au début ou à la fin d'un maintien.</summary>
    public struct WakeRequest
    {
        public bool IsSelf;
        public bool RequesterKnockedOut;
        public bool RequesterIsCarried;
        public bool RequesterIsCarrying;
        public bool TargetKnockedOut;
        public bool TargetIsCarried;
        public float Distance;
        public float MaxDistance;
    }

    /// <summary>Règles pures de réveil (sans NGO) : validation, délais, ivresse finale.</summary>
    public static class WakeRules
    {
        /// <summary>Valeur de la deadline répliquée quand aucun réveil automatique n'est programmé.</summary>
        public const double NoDeadline = -1d;

        /// <summary>Conditions communes au début et à la fin du maintien.</summary>
        public static WakeOutcome Validate(in WakeRequest r)
        {
            if (r.IsSelf) return WakeOutcome.RejectedSelf;
            if (r.RequesterKnockedOut) return WakeOutcome.RejectedRequesterKnockedOut;
            if (r.RequesterIsCarried) return WakeOutcome.RejectedRequesterCarried;
            if (r.RequesterIsCarrying) return WakeOutcome.RejectedRequesterCarrying;
            if (!r.TargetKnockedOut) return WakeOutcome.RejectedTargetNotKnockedOut;
            if (r.TargetIsCarried) return WakeOutcome.RejectedTargetCarried;
            if (!(r.Distance <= r.MaxDistance)) return WakeOutcome.RejectedOutOfRange;
            return WakeOutcome.HoldStarted;
        }

        public static WakeOutcome EvaluateStart(in WakeRequest r) => Validate(r);

        /// <summary>Fin de maintien : mêmes conditions qu'au début, puis durée mesurée par l'hôte avec marge de gigue.</summary>
        public static WakeOutcome EvaluateEnd(in WakeRequest r, bool startedOnThisTarget, double heldSeconds, float requiredSeconds, float toleranceSeconds)
        {
            if (!startedOnThisTarget) return WakeOutcome.RejectedNoHoldStarted;
            WakeOutcome outcome = Validate(r);
            if (outcome != WakeOutcome.HoldStarted) return outcome;
            return heldSeconds + toleranceSeconds >= requiredSeconds ? WakeOutcome.WokenByWater : WakeOutcome.HoldTooShort;
        }

        public static double AutoWakeDeadline(double knockoutTime, TuningConfig tuning) =>
            knockoutTime + tuning.AutoWakeSeconds;

        public static bool IsAutoWakeDue(double now, double deadline) =>
            deadline > NoDeadline && now >= deadline;

        /// <summary>Secondes restantes avant le réveil automatique (0 si aucune échéance ou déjà dépassée).</summary>
        public static float RemainingSeconds(double now, double deadline) =>
            deadline > NoDeadline ? (float)System.Math.Max(0d, deadline - now) : 0f;

        public static float DrunkennessAfter(WakeCause cause, TuningConfig tuning) =>
            cause == WakeCause.Water ? tuning.DrunkennessAfterWaterWake : tuning.DrunkennessAfterAutoWake;

        public static float HoldProgress(double heldSeconds, TuningConfig tuning) =>
            Mathf.Clamp01((float)(heldSeconds / tuning.WaterWakeHoldSeconds));
    }
}

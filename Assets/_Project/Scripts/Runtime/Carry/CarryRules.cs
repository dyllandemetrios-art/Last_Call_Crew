using LastCallCrew.Config;
using UnityEngine;

namespace LastCallCrew.Carry
{
    /// <summary>Résultat de la validation d'une demande de portage par l'hôte.</summary>
    public enum CarryOutcome : byte
    {
        Started,
        Dropped,
        RejectedInvalidSender,
        RejectedInvalidTarget,
        RejectedSelf,
        RejectedRequesterKnockedOut,
        RejectedRequesterCarried,
        RejectedAlreadyCarrying,
        RejectedTargetNotKnockedOut,
        RejectedTargetAlreadyCarried,
        RejectedTargetCarrying,
        RejectedOutOfRange,
        RejectedNotCarrying,
        /// <summary>Demande reçue après le résultat de la nuit (P1-033) : gameplay gelé.</summary>
        RejectedFrozen,
    }

    /// <summary>État des deux joueurs au moment d'une demande de prise, lu par l'hôte.</summary>
    public struct CarryStartRequest
    {
        public bool IsSelf;
        public bool RequesterKnockedOut;
        public bool RequesterIsCarried;
        public bool RequesterIsCarrying;
        public bool TargetKnockedOut;
        public bool TargetIsCarried;
        public bool TargetIsCarrying;
        public float Distance;
        public float MaxDistance;
    }

    /// <summary>Règles pures de portage (sans NGO) : validation, vitesse, point de dépose.</summary>
    public static class CarryRules
    {
        public static CarryOutcome EvaluateStart(in CarryStartRequest r)
        {
            if (r.IsSelf) return CarryOutcome.RejectedSelf;
            if (r.RequesterKnockedOut) return CarryOutcome.RejectedRequesterKnockedOut;
            if (r.RequesterIsCarried) return CarryOutcome.RejectedRequesterCarried;
            if (r.RequesterIsCarrying) return CarryOutcome.RejectedAlreadyCarrying;
            if (!r.TargetKnockedOut) return CarryOutcome.RejectedTargetNotKnockedOut;
            if (r.TargetIsCarried) return CarryOutcome.RejectedTargetAlreadyCarried;
            if (r.TargetIsCarrying) return CarryOutcome.RejectedTargetCarrying;
            if (!(r.Distance <= r.MaxDistance)) return CarryOutcome.RejectedOutOfRange;
            return CarryOutcome.Started;
        }

        public static CarryOutcome EvaluateDrop(bool requesterIsCarrying) =>
            requesterIsCarrying ? CarryOutcome.Dropped : CarryOutcome.RejectedNotCarrying;

        /// <summary>Multiplicateur de vitesse du porteur : valeur du TuningConfig pendant le portage, 1 sinon.</summary>
        public static float SpeedMultiplier(bool isCarrying, TuningConfig tuning) =>
            isCarrying ? tuning.CarrySpeedMultiplier : 1f;

        /// <summary>Point de dépose : devant le porteur, à sa hauteur de racine (donc sur le sol), jamais sous le niveau.</summary>
        public static Vector3 DropPosition(Vector3 carrierPosition, Vector3 carrierForward, float distance)
        {
            Vector3 flat = new Vector3(carrierForward.x, 0f, carrierForward.z);
            if (flat.sqrMagnitude < 1e-6f)
                flat = Vector3.forward;
            return carrierPosition + flat.normalized * distance;
        }
    }
}

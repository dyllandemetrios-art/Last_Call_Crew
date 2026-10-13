using LastCallCrew.Config;
using LastCallCrew.Wake;
using NUnit.Framework;
using UnityEngine;

namespace LastCallCrew.Tests
{
    public class WakeRulesTests
    {
        TuningConfig _t;

        [SetUp]
        public void SetUp() => _t = ScriptableObject.CreateInstance<TuningConfig>();

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_t);

        // Demande valide : réveilleur debout et libre, cible KO au sol, à portée.
        static WakeRequest Valid() => new WakeRequest
        {
            TargetKnockedOut = true,
            Distance = 2f,
            MaxDistance = 4f,
        };

        [Test]
        public void ValidRequest_StartsHold() =>
            Assert.AreEqual(WakeOutcome.HoldStarted, WakeRules.EvaluateStart(Valid()));

        [Test]
        public void Self_IsRejected()
        {
            WakeRequest r = Valid(); r.IsSelf = true;
            Assert.AreEqual(WakeOutcome.RejectedSelf, WakeRules.EvaluateStart(r));
        }

        [Test]
        public void RequesterKnockedOut_IsRejected()
        {
            WakeRequest r = Valid(); r.RequesterKnockedOut = true;
            Assert.AreEqual(WakeOutcome.RejectedRequesterKnockedOut, WakeRules.EvaluateStart(r));
        }

        [Test]
        public void RequesterCarried_IsRejected()
        {
            WakeRequest r = Valid(); r.RequesterIsCarried = true;
            Assert.AreEqual(WakeOutcome.RejectedRequesterCarried, WakeRules.EvaluateStart(r));
        }

        [Test]
        public void RequesterCarrying_IsRejected()
        {
            WakeRequest r = Valid(); r.RequesterIsCarrying = true;
            Assert.AreEqual(WakeOutcome.RejectedRequesterCarrying, WakeRules.EvaluateStart(r));
        }

        [Test]
        public void StandingTarget_IsRejected()
        {
            WakeRequest r = Valid(); r.TargetKnockedOut = false;
            Assert.AreEqual(WakeOutcome.RejectedTargetNotKnockedOut, WakeRules.EvaluateStart(r));
        }

        [Test]
        public void CarriedTarget_IsRejected()
        {
            WakeRequest r = Valid(); r.TargetIsCarried = true;
            Assert.AreEqual(WakeOutcome.RejectedTargetCarried, WakeRules.EvaluateStart(r));
        }

        [Test]
        public void OutOfRange_IsRejected()
        {
            WakeRequest r = Valid(); r.Distance = 4.01f;
            Assert.AreEqual(WakeOutcome.RejectedOutOfRange, WakeRules.EvaluateStart(r));
        }

        [Test]
        public void ExactMaxRange_IsAccepted()
        {
            WakeRequest r = Valid(); r.Distance = r.MaxDistance;
            Assert.AreEqual(WakeOutcome.HoldStarted, WakeRules.EvaluateStart(r));
        }

        [Test]
        public void NaNDistance_IsRejected()
        {
            WakeRequest r = Valid(); r.Distance = float.NaN;
            Assert.AreEqual(WakeOutcome.RejectedOutOfRange, WakeRules.EvaluateStart(r));
        }

        [Test]
        public void End_FullHold_Wakes() =>
            Assert.AreEqual(WakeOutcome.WokenByWater,
                WakeRules.EvaluateEnd(Valid(), true, _t.WaterWakeHoldSeconds, _t.WaterWakeHoldSeconds, 0.15f));

        [Test]
        public void End_WithinTolerance_Wakes() =>
            Assert.AreEqual(WakeOutcome.WokenByWater,
                WakeRules.EvaluateEnd(Valid(), true, _t.WaterWakeHoldSeconds - 0.1, _t.WaterWakeHoldSeconds, 0.15f));

        [Test]
        public void End_TooShort_DoesNotWake() =>
            Assert.AreEqual(WakeOutcome.HoldTooShort,
                WakeRules.EvaluateEnd(Valid(), true, _t.WaterWakeHoldSeconds - 1.0, _t.WaterWakeHoldSeconds, 0.15f));

        [Test]
        public void End_WithoutStart_IsRejected() =>
            Assert.AreEqual(WakeOutcome.RejectedNoHoldStarted,
                WakeRules.EvaluateEnd(Valid(), false, 10, _t.WaterWakeHoldSeconds, 0.15f));

        [Test]
        public void End_TargetAlreadyAwake_IsRejected()
        {
            WakeRequest r = Valid(); r.TargetKnockedOut = false;
            Assert.AreEqual(WakeOutcome.RejectedTargetNotKnockedOut,
                WakeRules.EvaluateEnd(r, true, 10, _t.WaterWakeHoldSeconds, 0.15f));
        }

        [Test]
        public void End_TargetBecameCarried_IsRejected()
        {
            WakeRequest r = Valid(); r.TargetIsCarried = true;
            Assert.AreEqual(WakeOutcome.RejectedTargetCarried,
                WakeRules.EvaluateEnd(r, true, 10, _t.WaterWakeHoldSeconds, 0.15f));
        }

        [Test]
        public void End_TargetWentOutOfRange_IsRejected()
        {
            WakeRequest r = Valid(); r.Distance = 9f;
            Assert.AreEqual(WakeOutcome.RejectedOutOfRange,
                WakeRules.EvaluateEnd(r, true, 10, _t.WaterWakeHoldSeconds, 0.15f));
        }

        [Test]
        public void AutoWakeDeadline_UsesTuningDelay() =>
            Assert.AreEqual(100d + _t.AutoWakeSeconds, WakeRules.AutoWakeDeadline(100d, _t), 1e-9);

        [Test]
        public void AutoWake_NotDueBeforeDeadline_DueAtDeadline()
        {
            double deadline = WakeRules.AutoWakeDeadline(10d, _t);
            Assert.IsFalse(WakeRules.IsAutoWakeDue(deadline - 0.01, deadline));
            Assert.IsTrue(WakeRules.IsAutoWakeDue(deadline, deadline));
        }

        [Test]
        public void AutoWake_NoDeadline_NeverDue() =>
            Assert.IsFalse(WakeRules.IsAutoWakeDue(1e9, WakeRules.NoDeadline));

        [Test]
        public void Remaining_CountsDownAndClamps()
        {
            Assert.AreEqual(30f, WakeRules.RemainingSeconds(70d, 100d), 1e-4f);
            Assert.AreEqual(0f, WakeRules.RemainingSeconds(120d, 100d));
            Assert.AreEqual(0f, WakeRules.RemainingSeconds(5d, WakeRules.NoDeadline));
        }

        [Test]
        public void Drunkenness_ComesFromTuning()
        {
            Assert.AreEqual(_t.DrunkennessAfterWaterWake, WakeRules.DrunkennessAfter(WakeCause.Water, _t));
            Assert.AreEqual(_t.DrunkennessAfterAutoWake, WakeRules.DrunkennessAfter(WakeCause.Auto, _t));
            Assert.AreEqual(50f, WakeRules.DrunkennessAfter(WakeCause.Water, _t));
            Assert.AreEqual(90f, WakeRules.DrunkennessAfter(WakeCause.Auto, _t));
        }

        [Test]
        public void HoldProgress_ClampsToUnit()
        {
            Assert.AreEqual(0.5f, WakeRules.HoldProgress(_t.WaterWakeHoldSeconds * 0.5, _t), 1e-4f);
            Assert.AreEqual(1f, WakeRules.HoldProgress(_t.WaterWakeHoldSeconds * 3, _t));
            Assert.AreEqual(0f, WakeRules.HoldProgress(-1, _t));
        }
    }
}

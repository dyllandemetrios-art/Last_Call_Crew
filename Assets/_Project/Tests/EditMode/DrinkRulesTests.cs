using LastCallCrew.Config;
using LastCallCrew.Drink;
using NUnit.Framework;
using UnityEngine;

namespace LastCallCrew.Tests
{
    public class DrinkRulesTests
    {
        const float Range = 4f;
        const float Tolerance = 0.15f;
        TuningConfig _config;

        [SetUp]
        public void SetUp() => _config = ScriptableObject.CreateInstance<TuningConfig>();

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_config);

        [Test]
        public void Accept_InRange_IsAccepted() =>
            Assert.AreEqual(DrinkRequestOutcome.Accepted, DrinkRules.EvaluateAccept(2f, Range));

        [Test]
        public void Accept_AtExactRange_IsAccepted() =>
            Assert.AreEqual(DrinkRequestOutcome.Accepted, DrinkRules.EvaluateAccept(Range, Range));

        [Test]
        public void Accept_OutOfRange_IsRejected() =>
            Assert.AreEqual(DrinkRequestOutcome.RejectedOutOfRange, DrinkRules.EvaluateAccept(Range + 0.01f, Range));

        [Test]
        public void Accept_InvalidDistance_IsRejectedAsOutOfRange()
        {
            Assert.AreEqual(DrinkRequestOutcome.RejectedOutOfRange, DrinkRules.EvaluateAccept(float.NaN, Range));
            Assert.AreEqual(DrinkRequestOutcome.RejectedOutOfRange, DrinkRules.EvaluateAccept(float.PositiveInfinity, Range));
        }

        [Test]
        public void Refuse_HeldForConfiguredDuration_IsRefused()
        {
            float required = _config.RefuseDurationSeconds;
            Assert.AreEqual(DrinkRequestOutcome.Refused,
                DrinkRules.EvaluateRefuse(2f, Range, required, required, Tolerance));
        }

        [Test]
        public void Refuse_HeldWithinTolerance_IsRefused()
        {
            float required = _config.RefuseDurationSeconds;
            Assert.AreEqual(DrinkRequestOutcome.Refused,
                DrinkRules.EvaluateRefuse(2f, Range, required - Tolerance, required, Tolerance));
        }

        [Test]
        public void Refuse_ReleasedTooEarly_IsCancelledWithoutDecision()
        {
            float required = _config.RefuseDurationSeconds;
            Assert.AreEqual(DrinkRequestOutcome.HoldTooShort,
                DrinkRules.EvaluateRefuse(2f, Range, required * 0.5, required, Tolerance));
            Assert.AreEqual(DrinkRequestOutcome.HoldTooShort,
                DrinkRules.EvaluateRefuse(2f, Range, 0.0, required, Tolerance));
        }

        [Test]
        public void Refuse_OutOfRange_IsRejectedEvenIfHeldLongEnough()
        {
            float required = _config.RefuseDurationSeconds;
            Assert.AreEqual(DrinkRequestOutcome.RejectedOutOfRange,
                DrinkRules.EvaluateRefuse(Range + 1f, Range, required, required, Tolerance));
        }

        [Test]
        public void ConsecutiveValidRequests_GiveTheSameOutcome()
        {
            // Aucun état partagé entre deux demandes : deux demandes valides de suite donnent la même issue.
            Assert.AreEqual(DrinkRequestOutcome.Accepted, DrinkRules.EvaluateAccept(2f, Range));
            Assert.AreEqual(DrinkRequestOutcome.Accepted, DrinkRules.EvaluateAccept(2f, Range));
        }

        [Test]
        public void Refuse_NaNHold_IsNotRefused() =>
            Assert.AreEqual(DrinkRequestOutcome.HoldTooShort,
                DrinkRules.EvaluateRefuse(2f, Range, double.NaN, _config.RefuseDurationSeconds, Tolerance));
    }
}

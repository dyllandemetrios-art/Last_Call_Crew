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
        public void Accept_InRangeAndAvailable_IsAccepted() =>
            Assert.AreEqual(DrinkRequestOutcome.Accepted, DrinkRules.EvaluateAccept(2f, Range, true));

        [Test]
        public void Accept_AtExactRange_IsAccepted() =>
            Assert.AreEqual(DrinkRequestOutcome.Accepted, DrinkRules.EvaluateAccept(Range, Range, true));

        [Test]
        public void Accept_OutOfRange_IsRejected() =>
            Assert.AreEqual(DrinkRequestOutcome.RejectedOutOfRange, DrinkRules.EvaluateAccept(Range + 0.01f, Range, true));

        [Test]
        public void Accept_Unavailable_IsRejected() =>
            Assert.AreEqual(DrinkRequestOutcome.RejectedUnavailable, DrinkRules.EvaluateAccept(2f, Range, false));

        [Test]
        public void Accept_InvalidDistance_IsRejectedAsOutOfRange()
        {
            Assert.AreEqual(DrinkRequestOutcome.RejectedOutOfRange, DrinkRules.EvaluateAccept(float.NaN, Range, true));
            Assert.AreEqual(DrinkRequestOutcome.RejectedOutOfRange, DrinkRules.EvaluateAccept(float.PositiveInfinity, Range, true));
        }

        [Test]
        public void Refuse_HeldForConfiguredDuration_IsRefused()
        {
            float required = _config.RefuseDurationSeconds;
            Assert.AreEqual(DrinkRequestOutcome.Refused,
                DrinkRules.EvaluateRefuse(2f, Range, true, required, required, Tolerance));
        }

        [Test]
        public void Refuse_HeldWithinTolerance_IsRefused()
        {
            float required = _config.RefuseDurationSeconds;
            Assert.AreEqual(DrinkRequestOutcome.Refused,
                DrinkRules.EvaluateRefuse(2f, Range, true, required - Tolerance, required, Tolerance));
        }

        [Test]
        public void Refuse_ReleasedTooEarly_IsCancelledWithoutDecision()
        {
            float required = _config.RefuseDurationSeconds;
            Assert.AreEqual(DrinkRequestOutcome.HoldTooShort,
                DrinkRules.EvaluateRefuse(2f, Range, true, required * 0.5, required, Tolerance));
            Assert.AreEqual(DrinkRequestOutcome.HoldTooShort,
                DrinkRules.EvaluateRefuse(2f, Range, true, 0.0, required, Tolerance));
        }

        [Test]
        public void Refuse_OutOfRangeOrUnavailable_IsRejectedEvenIfHeldLongEnough()
        {
            float required = _config.RefuseDurationSeconds;
            Assert.AreEqual(DrinkRequestOutcome.RejectedOutOfRange,
                DrinkRules.EvaluateRefuse(Range + 1f, Range, true, required, required, Tolerance));
            Assert.AreEqual(DrinkRequestOutcome.RejectedUnavailable,
                DrinkRules.EvaluateRefuse(2f, Range, false, required, required, Tolerance));
        }

        [Test]
        public void Refuse_NaNHold_IsNotRefused() =>
            Assert.AreEqual(DrinkRequestOutcome.HoldTooShort,
                DrinkRules.EvaluateRefuse(2f, Range, true, double.NaN, _config.RefuseDurationSeconds, Tolerance));
    }
}

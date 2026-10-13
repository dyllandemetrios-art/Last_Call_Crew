using LastCallCrew.Night;
using NUnit.Framework;

namespace LastCallCrew.Tests
{
    public class ReturnRulesTests
    {
        [Test]
        public void Sentinel_MeansNotReturned() =>
            Assert.IsFalse(ReturnRules.IsReturned(ReturnRules.NotReturned));

        [TestCase(0d)]
        [TestCase(12.5d)]
        [TestCase(1e6d)]
        public void AnyServerTime_IsReturned(double t) =>
            Assert.IsTrue(ReturnRules.IsReturned(t));

        [Test]
        public void Next_Entering_RecordsNow() =>
            Assert.AreEqual(42.25d, ReturnRules.Next(ReturnRules.NotReturned, true, 42.25d));

        [Test]
        public void Next_Entering_AtTimeZero_IsStillReturned() =>
            Assert.IsTrue(ReturnRules.IsReturned(ReturnRules.Next(ReturnRules.NotReturned, true, 0d)));

        [Test]
        public void Next_Staying_KeepsEntryInstant() =>
            Assert.AreEqual(10d, ReturnRules.Next(10d, true, 99d));

        [Test]
        public void Next_Leaving_ClearsReturn() =>
            Assert.AreEqual(ReturnRules.NotReturned, ReturnRules.Next(10d, false, 99d));

        [Test]
        public void Next_StayingOutside_StaysNotReturned() =>
            Assert.AreEqual(ReturnRules.NotReturned, ReturnRules.Next(ReturnRules.NotReturned, false, 99d));

        [Test]
        public void Next_ReEntering_ReplacesOldInstant()
        {
            double t = ReturnRules.Next(ReturnRules.NotReturned, true, 10d);
            t = ReturnRules.Next(t, false, 20d);
            t = ReturnRules.Next(t, true, 30d);
            Assert.AreEqual(30d, t);
        }

        [Test]
        public void LeaverThenOtherEntering_IsNotAVictoryUntilBothPresent()
        {
            double a = ReturnRules.Next(ReturnRules.NotReturned, true, 10d);
            a = ReturnRules.Next(a, false, 12d);
            double b = ReturnRules.Next(ReturnRules.NotReturned, true, 15d);
            Assert.AreEqual(NightResult.Pending,
                NightOutcomeRules.Evaluate(NightState.Running, 100d, new[] { a, b }));
            a = ReturnRules.Next(a, true, 18d);
            Assert.AreEqual(NightResult.Victory,
                NightOutcomeRules.Evaluate(NightState.Running, 100d, new[] { a, b }));
        }

        [Test]
        public void Label_ReflectsState()
        {
            Assert.AreEqual("ENCORE DEHORS", ReturnRules.Label(ReturnRules.NotReturned));
            Assert.AreEqual("RENTRÉ", ReturnRules.Label(3d));
        }
    }
}

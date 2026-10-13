using LastCallCrew.Night;
using NUnit.Framework;

namespace LastCallCrew.Tests
{
    public class ReturnRulesTests
    {
        [Test]
        public void Sentinel_MeansNotReturned()
        {
            Assert.IsFalse(ReturnRules.IsReturned(ReturnRules.NotReturned));
            Assert.IsTrue(ReturnRules.ShouldRecord(ReturnRules.NotReturned));
        }

        [TestCase(0d)]
        [TestCase(12.5d)]
        [TestCase(1e6d)]
        public void AnyServerTime_IsReturned(double t)
        {
            Assert.IsTrue(ReturnRules.IsReturned(t));
            Assert.IsFalse(ReturnRules.ShouldRecord(t));
        }

        [Test]
        public void Record_FromSentinel_StoresNow() =>
            Assert.AreEqual(42.25d, ReturnRules.Record(ReturnRules.NotReturned, 42.25d));

        [Test]
        public void Record_Twice_KeepsFirstInstant()
        {
            double first = ReturnRules.Record(ReturnRules.NotReturned, 10d);
            double second = ReturnRules.Record(first, 99d);
            Assert.AreEqual(10d, second);
        }

        [Test]
        public void Record_AtTimeZero_IsStillRecorded()
        {
            double t = ReturnRules.Record(ReturnRules.NotReturned, 0d);
            Assert.IsTrue(ReturnRules.IsReturned(t));
            Assert.AreEqual(0d, ReturnRules.Record(t, 5d));
        }

        [Test]
        public void Label_ReflectsState()
        {
            Assert.AreEqual("ENCORE DEHORS", ReturnRules.Label(ReturnRules.NotReturned));
            Assert.AreEqual("RENTRÉ", ReturnRules.Label(3d));
        }
    }
}

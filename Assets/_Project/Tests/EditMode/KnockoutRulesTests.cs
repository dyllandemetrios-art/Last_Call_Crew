using LastCallCrew.Config;
using LastCallCrew.Intoxication;
using NUnit.Framework;
using UnityEngine;

namespace LastCallCrew.Tests
{
    public class KnockoutRulesTests
    {
        TuningConfig _t;

        [SetUp]
        public void SetUp() => _t = ScriptableObject.CreateInstance<TuningConfig>();

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_t);

        [Test]
        public void AtKoThreshold_EntersKnockout() =>
            Assert.IsTrue(KnockoutRules.ShouldEnterKnockout(_t.KoThreshold, _t, false));

        [Test]
        public void AboveKoThreshold_EntersKnockout() =>
            Assert.IsTrue(KnockoutRules.ShouldEnterKnockout(_t.KoThreshold + 5f, _t, false));

        [TestCase(0f)]
        [TestCase(59.99f)]
        [TestCase(99.99f)]
        public void BelowKoThreshold_DoesNotEnterKnockout(float value) =>
            Assert.IsFalse(KnockoutRules.ShouldEnterKnockout(value, _t, false));

        [Test]
        public void AlreadyKnockedOut_NeverRetriggers()
        {
            Assert.IsFalse(KnockoutRules.ShouldEnterKnockout(_t.KoThreshold, _t, true));
            Assert.IsFalse(KnockoutRules.ShouldEnterKnockout(_t.KoThreshold + 50f, _t, true));
        }

        [Test]
        public void NaN_DoesNotEnterKnockout() =>
            Assert.IsFalse(KnockoutRules.ShouldEnterKnockout(float.NaN, _t, false));

        [Test]
        public void FiveDrinksThenExtraDrink_EntersKnockoutOnlyOnce()
        {
            float value = _t.DrunkennessMin;
            bool knockedOut = false;
            int entries = 0;
            for (int i = 0; i < 8; i++)
            {
                value = IntoxicationRules.AddDrink(value, _t);
                if (KnockoutRules.ShouldEnterKnockout(value, _t, knockedOut))
                {
                    knockedOut = true;
                    entries++;
                }
            }
            Assert.AreEqual(1, entries);
        }

        [Test]
        public void ThresholdComesFromConfig()
        {
            var custom = ScriptableObject.CreateInstance<TuningConfig>();
            try
            {
                typeof(TuningConfig).GetField("_koThreshold", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(custom, 80f);
                Assert.IsTrue(KnockoutRules.ShouldEnterKnockout(80f, custom, false));
                Assert.IsFalse(KnockoutRules.ShouldEnterKnockout(80f, _t, false));
            }
            finally
            {
                Object.DestroyImmediate(custom);
            }
        }
    }
}

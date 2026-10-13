using LastCallCrew.Config;
using LastCallCrew.Intoxication;
using NUnit.Framework;
using UnityEngine;

namespace LastCallCrew.Tests
{
    public class IntoxicationRulesTests
    {
        TuningConfig _t;

        [SetUp]
        public void SetUp() => _t = ScriptableObject.CreateInstance<TuningConfig>();

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_t);

        [Test]
        public void AddDrink_AddsConfiguredGain() =>
            Assert.AreEqual(_t.GainPerDrink, IntoxicationRules.AddDrink(_t.DrunkennessMin, _t));

        [Test]
        public void AddDrink_NeverExceedsKoThreshold()
        {
            Assert.AreEqual(_t.KoThreshold, IntoxicationRules.AddDrink(_t.KoThreshold - 1f, _t));
            Assert.AreEqual(_t.KoThreshold, IntoxicationRules.AddDrink(_t.KoThreshold, _t));
        }

        [Test]
        public void FiveDrinksFromZero_ReachKoThresholdExactly()
        {
            float v = _t.DrunkennessMin;
            for (int i = 0; i < 5; i++)
                v = IntoxicationRules.AddDrink(v, _t);
            Assert.AreEqual(_t.KoThreshold, v);
        }

        [Test]
        public void Decay_UsesConfiguredRateAndElapsedTime() =>
            Assert.AreEqual(50f - _t.PassiveDecayPerSecond * 10f, IntoxicationRules.ApplyDecay(50f, 10f, _t), 1e-4f);

        [Test]
        public void Decay_NeverGoesBelowMinimum()
        {
            Assert.AreEqual(_t.DrunkennessMin, IntoxicationRules.ApplyDecay(1f, 1000f, _t));
            Assert.AreEqual(_t.DrunkennessMin, IntoxicationRules.ApplyDecay(_t.DrunkennessMin, 5f, _t));
        }

        [Test]
        public void Decay_WithZeroOrNegativeElapsed_ChangesNothing()
        {
            Assert.AreEqual(40f, IntoxicationRules.ApplyDecay(40f, 0f, _t));
            Assert.AreEqual(40f, IntoxicationRules.ApplyDecay(40f, -3f, _t));
        }

        [Test]
        public void Decay_AtKoThreshold_IsFrozen()
        {
            Assert.AreEqual(_t.KoThreshold, IntoxicationRules.ApplyDecay(_t.KoThreshold, 1f, _t));
            Assert.AreEqual(_t.KoThreshold, IntoxicationRules.ApplyDecay(_t.KoThreshold, 3600f, _t));
        }

        [Test]
        public void Decay_AboveKoThreshold_IsClampedToTheMaximumAndFrozen() =>
            Assert.AreEqual(_t.KoThreshold, IntoxicationRules.ApplyDecay(_t.KoThreshold + 50f, 10f, _t));

        [Test]
        public void Decay_JustBelowKoThreshold_StillDecays() =>
            Assert.Less(IntoxicationRules.ApplyDecay(_t.KoThreshold - 1f, 1f, _t), _t.KoThreshold - 1f);

        [Test]
        public void ReachingKoThroughDrinks_StaysAtKoDespiteLongDecayTime()
        {
            float v = _t.DrunkennessMin;
            for (int i = 0; i < 5; i++)
                v = IntoxicationRules.AddDrink(v, _t);
            for (int i = 0; i < 100; i++)
                v = IntoxicationRules.ApplyDecay(v, 0.1f, _t);
            Assert.AreEqual(_t.KoThreshold, v);
            Assert.AreEqual(IntoxicationStage.KoThreshold, IntoxicationRules.GetStage(v, _t));
        }

        [Test]
        public void Clamp_BoundsValuesAndHandlesNaN()
        {
            Assert.AreEqual(_t.DrunkennessMin, IntoxicationRules.Clamp(-20f, _t));
            Assert.AreEqual(_t.KoThreshold, IntoxicationRules.Clamp(250f, _t));
            Assert.AreEqual(_t.DrunkennessMin, IntoxicationRules.Clamp(float.NaN, _t));
            Assert.AreEqual(42f, IntoxicationRules.Clamp(42f, _t));
        }

        [TestCase(0f, IntoxicationStage.Sober)]
        [TestCase(29.99f, IntoxicationStage.Sober)]
        [TestCase(30f, IntoxicationStage.Pompette)]
        [TestCase(59.99f, IntoxicationStage.Pompette)]
        [TestCase(60f, IntoxicationStage.Ivre)]
        [TestCase(99.99f, IntoxicationStage.Ivre)]
        [TestCase(100f, IntoxicationStage.KoThreshold)]
        public void Stage_FollowsConfiguredThresholds(float value, IntoxicationStage expected) =>
            Assert.AreEqual(expected, IntoxicationRules.GetStage(value, _t));

        [Test]
        public void Stage_UsesConfigThresholds_NotLiterals()
        {
            var custom = ScriptableObject.CreateInstance<TuningConfig>();
            try
            {
                typeof(TuningConfig).GetField("_pompetteThreshold", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(custom, 10f);
                Assert.AreEqual(IntoxicationStage.Pompette, IntoxicationRules.GetStage(15f, custom));
                Assert.AreEqual(IntoxicationStage.Sober, IntoxicationRules.GetStage(15f, _t));
            }
            finally
            {
                Object.DestroyImmediate(custom);
            }
        }

        [Test]
        public void Labels_AreDistinct()
        {
            Assert.AreEqual("Sobre", IntoxicationRules.Label(IntoxicationStage.Sober));
            Assert.AreEqual("Pompette", IntoxicationRules.Label(IntoxicationStage.Pompette));
            Assert.AreEqual("Ivre", IntoxicationRules.Label(IntoxicationStage.Ivre));
            Assert.AreEqual("Seuil KO", IntoxicationRules.Label(IntoxicationStage.KoThreshold));
        }
    }
}

using System.Reflection;
using LastCallCrew.Config;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace LastCallCrew.Tests
{
    public class TuningConfigTests
    {
        const string AssetPath = "Assets/_Project/Data/Tuning/TuningConfig.asset";

        TuningConfig _config;

        [SetUp]
        public void SetUp() => _config = ScriptableObject.CreateInstance<TuningConfig>();

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_config);

        static void Set(TuningConfig config, string field, float value) =>
            typeof(TuningConfig).GetField(field, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(config, value);

        static void AssertDefaults(TuningConfig c)
        {
            Assert.AreEqual(0f, c.DrunkennessMin);
            Assert.AreEqual(30f, c.PompetteThreshold);
            Assert.AreEqual(60f, c.IvreThreshold);
            Assert.AreEqual(100f, c.KoThreshold);
            Assert.AreEqual(20f, c.GainPerDrink);
            Assert.AreEqual(0.2f, c.PassiveDecayPerSecond);
            Assert.AreEqual(15f, c.WaterReduction);
            Assert.AreEqual(2f, c.RefuseDurationSeconds);
            Assert.AreEqual(2f, c.SociableRefuseMultiplier);
            Assert.AreEqual(20f, c.NpcOfferCooldownSeconds);
            Assert.AreEqual(3f, c.WaterWakeHoldSeconds);
            Assert.AreEqual(50f, c.DrunkennessAfterWaterWake);
            Assert.AreEqual(45f, c.AutoWakeSeconds);
            Assert.AreEqual(90f, c.DrunkennessAfterAutoWake);
            Assert.AreEqual(0.6f, c.CarrySpeedMultiplier);
            Assert.AreEqual(300f, c.NightDurationSeconds);
        }

        [Test]
        public void Defaults_MatchGddValues_AndAreValid()
        {
            AssertDefaults(_config);
            Assert.IsTrue(_config.IsValid);
            Assert.DoesNotThrow(_config.ThrowIfInvalid);
        }

        [Test]
        public void TuningConfigAsset_ExistsWithGddValues_AndIsValid()
        {
            var asset = AssetDatabase.LoadAssetAtPath<TuningConfig>(AssetPath);
            Assert.IsNotNull(asset, $"Asset manquant : {AssetPath}");
            AssertDefaults(asset);
            Assert.IsTrue(asset.IsValid);
        }

        [TestCase("_drunkennessMin", 30f)]      // min == Pompette
        [TestCase("_pompetteThreshold", 60f)]   // Pompette == Ivre
        [TestCase("_pompetteThreshold", 0f)]    // Pompette == min
        [TestCase("_ivreThreshold", 100f)]      // Ivre == KO
        [TestCase("_ivreThreshold", 20f)]       // Ivre < Pompette
        [TestCase("_koThreshold", 60f)]         // KO == Ivre
        [TestCase("_gainPerDrink", 0f)]
        [TestCase("_passiveDecayPerSecond", -0.1f)]
        [TestCase("_waterReduction", 0f)]
        [TestCase("_refuseDurationSeconds", 0f)]
        [TestCase("_sociableRefuseMultiplier", 0f)]
        [TestCase("_npcOfferCooldownSeconds", -1f)]
        [TestCase("_waterWakeHoldSeconds", 0f)]
        [TestCase("_autoWakeSeconds", -5f)]
        [TestCase("_drunkennessAfterWaterWake", -1f)]
        [TestCase("_drunkennessAfterWaterWake", 101f)]
        [TestCase("_drunkennessAfterAutoWake", 100.5f)]
        [TestCase("_carrySpeedMultiplier", 0f)]
        [TestCase("_carrySpeedMultiplier", 1.2f)]
        [TestCase("_carrySpeedMultiplier", -0.5f)]
        [TestCase("_nightDurationSeconds", 0f)]
        [TestCase("_nightDurationSeconds", -10f)]
        [TestCase("_gainPerDrink", float.NaN)]
        public void InvalidValue_IsReported_AndRefusedByThrowIfInvalid(string field, float value)
        {
            Set(_config, field, value);

            Assert.IsFalse(_config.IsValid);
            Assert.IsNotEmpty(_config.GetValidationErrors());
            Assert.Throws<System.InvalidOperationException>(_config.ThrowIfInvalid);
        }

        [Test]
        public void BoundaryValues_AreAccepted()
        {
            Set(_config, "_carrySpeedMultiplier", 1f);
            Set(_config, "_drunkennessAfterWaterWake", 0f);
            Set(_config, "_drunkennessAfterAutoWake", 100f);
            Set(_config, "_passiveDecayPerSecond", 0f);

            Assert.IsTrue(_config.IsValid);
        }
    }
}

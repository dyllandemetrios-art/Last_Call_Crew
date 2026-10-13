using LastCallCrew.Config;
using LastCallCrew.Night;
using NUnit.Framework;
using UnityEngine;

namespace LastCallCrew.Tests
{
    public class NightTimerRulesTests
    {
        TuningConfig _t;

        [SetUp]
        public void SetUp() => _t = ScriptableObject.CreateInstance<TuningConfig>();

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_t);

        [Test]
        public void Start_NeedsTwoPlayers()
        {
            Assert.IsFalse(NightTimerRules.ShouldStart(NightState.Waiting, 0));
            Assert.IsFalse(NightTimerRules.ShouldStart(NightState.Waiting, 1));
            Assert.IsTrue(NightTimerRules.ShouldStart(NightState.Waiting, 2));
            Assert.IsTrue(NightTimerRules.ShouldStart(NightState.Waiting, 3));
        }

        [Test]
        public void Start_HappensOnlyOnce()
        {
            Assert.IsFalse(NightTimerRules.ShouldStart(NightState.Running, 2));
            Assert.IsFalse(NightTimerRules.ShouldStart(NightState.Expired, 2));
        }

        [Test]
        public void Deadline_UsesConfiguredDuration() =>
            Assert.AreEqual(100d + _t.NightDurationSeconds, NightTimerRules.Deadline(100d, _t), 1e-9);

        [Test]
        public void DefaultDuration_IsFiveMinutes() => Assert.AreEqual(300f, _t.NightDurationSeconds);

        [Test]
        public void Expire_OnlyFromRunning_AtOrAfterDeadline()
        {
            Assert.IsFalse(NightTimerRules.ShouldExpire(NightState.Running, 109.99, 110));
            Assert.IsTrue(NightTimerRules.ShouldExpire(NightState.Running, 110, 110));
            Assert.IsFalse(NightTimerRules.ShouldExpire(NightState.Waiting, 500, 110));
            Assert.IsFalse(NightTimerRules.ShouldExpire(NightState.Expired, 500, 110));
        }

        [Test]
        public void Remaining_CountsDown_NeverNegative()
        {
            Assert.AreEqual(30f, NightTimerRules.Remaining(NightState.Running, 70, 100, _t), 1e-4f);
            Assert.AreEqual(0f, NightTimerRules.Remaining(NightState.Running, 130, 100, _t));
            Assert.AreEqual(0f, NightTimerRules.Remaining(NightState.Expired, 130, 100, _t));
            Assert.AreEqual(_t.NightDurationSeconds, NightTimerRules.Remaining(NightState.Waiting, 5, 0, _t));
        }

        [Test]
        public void ExpiredForDisplay_IncludesRunningPastDeadline()
        {
            Assert.IsTrue(NightTimerRules.IsExpiredForDisplay(NightState.Expired, 0));
            Assert.IsTrue(NightTimerRules.IsExpiredForDisplay(NightState.Running, 0));
            Assert.IsFalse(NightTimerRules.IsExpiredForDisplay(NightState.Running, 0.1f));
            Assert.IsFalse(NightTimerRules.IsExpiredForDisplay(NightState.Waiting, 0));
        }

        [Test]
        public void FinalCountdown_LastTenSecondsOnly()
        {
            Assert.IsTrue(NightTimerRules.IsFinalCountdown(NightState.Running, 10f));
            Assert.IsTrue(NightTimerRules.IsFinalCountdown(NightState.Running, 0.5f));
            Assert.IsFalse(NightTimerRules.IsFinalCountdown(NightState.Running, 10.01f));
            Assert.IsFalse(NightTimerRules.IsFinalCountdown(NightState.Running, 0f));
            Assert.IsFalse(NightTimerRules.IsFinalCountdown(NightState.Waiting, 5f));
        }

        [TestCase(300f, "05:00")]
        [TestCase(299.2f, "05:00")]
        [TestCase(299f, "04:59")]
        [TestCase(61f, "01:01")]
        [TestCase(60f, "01:00")]
        [TestCase(9.01f, "00:10")]
        [TestCase(0.01f, "00:01")]
        [TestCase(0f, "00:00")]
        [TestCase(-3f, "00:00")]
        public void Format_IsMmSs(float seconds, string expected) =>
            Assert.AreEqual(expected, NightTimerRules.Format(seconds));
    }
}

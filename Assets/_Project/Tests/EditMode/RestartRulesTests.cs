using LastCallCrew.Night;
using NUnit.Framework;
using UnityEngine;

namespace LastCallCrew.Tests
{
    public class RestartRulesTests
    {
        [Test]
        public void Frozen_OnlyAfterTerminalResult()
        {
            Assert.IsFalse(RestartRules.IsFrozen(NightResult.Pending));
            Assert.IsTrue(RestartRules.IsFrozen(NightResult.Victory));
            Assert.IsTrue(RestartRules.IsFrozen(NightResult.Failure));
        }

        [TestCase(NightResult.Victory)]
        [TestCase(NightResult.Failure)]
        public void Host_CanRestart_AfterResult(NightResult result) =>
            Assert.AreEqual(RestartOutcome.Restarted, RestartRules.Evaluate(true, result));

        [TestCase(NightResult.Victory)]
        [TestCase(NightResult.Failure)]
        [TestCase(NightResult.Pending)]
        public void Client_CanNeverRestart(NightResult result) =>
            Assert.AreEqual(RestartOutcome.RejectedNotHost, RestartRules.Evaluate(false, result));

        [Test]
        public void Host_CannotRestart_BeforeResult() =>
            Assert.AreEqual(RestartOutcome.RejectedNotFinished, RestartRules.Evaluate(true, NightResult.Pending));

        [Test]
        public void SecondRestartRequest_AfterRestart_IsIgnored()
        {
            // Après la relance le résultat redevient Pending : une demande répétée ne relance pas une deuxième fois.
            Assert.AreEqual(RestartOutcome.Restarted, RestartRules.Evaluate(true, NightResult.Failure));
            Assert.AreEqual(RestartOutcome.RejectedNotFinished, RestartRules.Evaluate(true, NightResult.Pending));
        }

        [TestCase(0UL, 0f)]
        [TestCase(1UL, 3f)]
        [TestCase(2UL, 6f)]
        public void StartPosition_MatchesSpawnFormula(ulong id, float x) =>
            Assert.AreEqual(new Vector3(x, 1f, 0f), RestartRules.StartPosition(id));

        [Test]
        public void FinalState_Texts()
        {
            Assert.AreEqual("RENTRÉ · DEBOUT", RestartRules.FinalState(true, false));
            Assert.AreEqual("RENTRÉ · KO", RestartRules.FinalState(true, true));
            Assert.AreEqual("DEHORS · DEBOUT", RestartRules.FinalState(false, false));
            Assert.AreEqual("DEHORS · KO", RestartRules.FinalState(false, true));
        }
    }
}

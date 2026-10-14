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

        [TestCase(0, 0f)]
        [TestCase(1, 3f)]
        public void StartPosition_FollowsSlot(int slot, float x) =>
            Assert.AreEqual(new Vector3(x, 1f, 0f), RestartRules.StartPosition(slot));

        [Test]
        public void Restart_NeedsTwoConnectedPlayers()
        {
            Assert.AreEqual(RestartOutcome.RejectedNotEnoughPlayers, RestartRules.Evaluate(true, NightResult.Failure, 1));
            Assert.AreEqual(RestartOutcome.Restarted, RestartRules.Evaluate(true, NightResult.Failure, 2));
            Assert.AreEqual(RestartOutcome.RejectedNotHost, RestartRules.Evaluate(false, NightResult.Failure, 1));
            Assert.AreEqual(RestartOutcome.RejectedNotFinished, RestartRules.Evaluate(true, NightResult.Pending, 2));
        }

        [Test]
        public void TwoSlots_WithHighOrSparseClientIds_StayInsideTheStartZone()
        {
            // L'hôte (0) et un client reconnecté avec un identifiant élevé : emplacements 0 et 1, pas x = 3 × identifiant.
            var ids = new System.Collections.Generic.List<ulong> { 0UL, 47UL };
            Assert.AreEqual(0, PlayerSlots.Rank(0UL, ids));
            Assert.AreEqual(1, PlayerSlots.Rank(47UL, ids));
            Assert.AreEqual(new Vector3(3f, 1f, 0f), RestartRules.StartPosition(PlayerSlots.Rank(47UL, ids)));

            var sparse = new System.Collections.Generic.List<ulong> { 5UL, 2UL };
            Assert.AreEqual(0, PlayerSlots.Rank(2UL, sparse));
            Assert.AreEqual(1, PlayerSlots.Rank(5UL, sparse));
        }

        [Test]
        public void FirstFree_ReusesAFreedSlot()
        {
            Assert.AreEqual(0, PlayerSlots.FirstFree(new int[0]));
            Assert.AreEqual(1, PlayerSlots.FirstFree(new[] { 0 }));
            Assert.AreEqual(1, PlayerSlots.FirstFree(new[] { 0, 2 }));
            Assert.AreEqual(0, PlayerSlots.FirstFree(new[] { 1 }));
        }

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

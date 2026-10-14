using LastCallCrew.Night;
using NUnit.Framework;

namespace LastCallCrew.Tests
{
    public class NightOutcomeRulesTests
    {
        const double Deadline = 300d;
        static readonly double None = ReturnRules.NotReturned;

        static NightResult Eval(NightState state, params double[] times) =>
            NightOutcomeRules.Evaluate(state, Deadline, times);

        [Test]
        public void TwoPlayersBack_BeforeDeadline_IsVictory() =>
            Assert.AreEqual(NightResult.Victory, Eval(NightState.Running, 100, 250));

        [Test]
        public void Victory_IsDecidedEvenAfterExpiredState_IfInstantsAreInTime() =>
            Assert.AreEqual(NightResult.Victory, Eval(NightState.Expired, 100, 250));

        [Test]
        public void ExactlyAtDeadline_IsAccepted() =>
            Assert.AreEqual(NightResult.Victory, Eval(NightState.Expired, 100, Deadline));

        [Test]
        public void JustAfterDeadline_IsNotAccepted() =>
            Assert.AreEqual(NightResult.Failure, Eval(NightState.Expired, 100, Deadline + 0.001));

        [Test]
        public void JustBeforeDeadline_IsAccepted() =>
            Assert.AreEqual(NightResult.Victory, Eval(NightState.Expired, 100, Deadline - 0.001));

        [Test]
        public void OnePlayerMissing_StaysPending_WhileRunning() =>
            Assert.AreEqual(NightResult.Pending, Eval(NightState.Running, 100, None));

        [Test]
        public void OnePlayerMissing_IsFailure_AtExpiry() =>
            Assert.AreEqual(NightResult.Failure, Eval(NightState.Expired, 100, None));

        [Test]
        public void LateReturnAfterDeadline_DoesNotMakeVictory() =>
            Assert.AreEqual(NightResult.Failure, Eval(NightState.Expired, 100, 305));

        [Test]
        public void SinglePlayer_NeverWins()
        {
            Assert.AreEqual(NightResult.Pending, Eval(NightState.Running, 100));
            Assert.AreEqual(NightResult.Failure, Eval(NightState.Expired, 100));
        }

        [Test]
        public void NoPlayers_NeverWins() =>
            Assert.AreEqual(NightResult.Failure, Eval(NightState.Expired));

        [Test]
        public void ThirdPlayer_MustAlsoBeBack()
        {
            Assert.AreEqual(NightResult.Pending, Eval(NightState.Running, 100, 120, None));
            Assert.AreEqual(NightResult.Failure, Eval(NightState.Expired, 100, 120, None));
            Assert.AreEqual(NightResult.Victory, Eval(NightState.Running, 100, 120, 130));
        }

        [Test]
        public void WaitingTimer_NeverConcludes()
        {
            Assert.AreEqual(NightResult.Pending, Eval(NightState.Waiting, 1, 2));
            Assert.AreEqual(NightResult.Pending, Eval(NightState.Waiting));
        }

        [Test]
        public void ReturnAtTimeZero_Counts()
        {
            Assert.AreEqual(NightResult.Victory, Eval(NightState.Running, 0, 0));
        }

        [Test]
        public void FirstTerminalResult_Wins()
        {
            Assert.AreEqual(NightResult.Victory, NightOutcomeRules.Next(NightResult.Victory, NightResult.Failure));
            Assert.AreEqual(NightResult.Failure, NightOutcomeRules.Next(NightResult.Failure, NightResult.Victory));
            Assert.AreEqual(NightResult.Failure, NightOutcomeRules.Next(NightResult.Pending, NightResult.Failure));
            Assert.AreEqual(NightResult.Pending, NightOutcomeRules.Next(NightResult.Pending, NightResult.Pending));
        }

        [Test]
        public void Banner_TextsMatchSpec()
        {
            Assert.AreEqual("VICTOIRE — TOUT LE MONDE EST RENTRÉ", NightOutcomeRules.Banner(NightResult.Victory));
            Assert.AreEqual("ÉCHEC — QUELQU'UN EST RESTÉ DEHORS", NightOutcomeRules.Banner(NightResult.Failure));
            Assert.AreEqual(string.Empty, NightOutcomeRules.Banner(NightResult.Pending));
        }

        [Test]
        public void Disconnect_BeforeRoundStarts_ChangesNothing() =>
            Assert.AreEqual(NightResult.Pending, NightOutcomeRules.OnClientDisconnected(NightState.Waiting, NightResult.Pending));

        [TestCase(NightState.Running)]
        [TestCase(NightState.Expired)]
        public void Disconnect_DuringActiveRound_IsAnImmediateFailure(NightState state) =>
            Assert.AreEqual(NightResult.Failure, NightOutcomeRules.OnClientDisconnected(state, NightResult.Pending));

        [TestCase(NightResult.Victory)]
        [TestCase(NightResult.Failure)]
        public void Disconnect_AfterAResult_KeepsIt(NightResult decided) =>
            Assert.AreEqual(decided, NightOutcomeRules.OnClientDisconnected(NightState.Running, decided));

        [Test]
        public void FirstTerminalResult_IsImmutable_EvenIfADisconnectFollowsAVictory()
        {
            NightResult victory = NightOutcomeRules.Next(NightResult.Pending, NightResult.Victory);
            NightResult afterDisconnect = NightOutcomeRules.OnClientDisconnected(NightState.Running, victory);
            Assert.AreEqual(NightResult.Victory, NightOutcomeRules.Next(afterDisconnect, NightResult.Failure));
        }

        [Test]
        public void Banner_ShowsTheDisconnectReason()
        {
            Assert.AreEqual("ÉCHEC — PARTENAIRE DÉCONNECTÉ", NightOutcomeRules.Banner(NightResult.Failure, NightReason.PartnerDisconnected));
            Assert.AreEqual("ÉCHEC — QUELQU'UN EST RESTÉ DEHORS", NightOutcomeRules.Banner(NightResult.Failure, NightReason.Deadline));
            Assert.AreEqual("VICTOIRE — TOUT LE MONDE EST RENTRÉ", NightOutcomeRules.Banner(NightResult.Victory, NightReason.None));
        }
    }
}

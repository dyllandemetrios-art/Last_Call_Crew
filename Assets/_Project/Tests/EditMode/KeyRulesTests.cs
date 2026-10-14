using LastCallCrew.Night;
using NUnit.Framework;

namespace LastCallCrew.Tests
{
    public class KeyRulesTests
    {
        static KeyRequest Valid => new KeyRequest { Distance = 1.5f, MaxDistance = 4f };

        [Test]
        public void Available_ValidRequest_IsTaken()
        {
            KeyOutcome outcome = KeyRules.Evaluate(false, Valid);
            Assert.AreEqual(KeyOutcome.Taken, outcome);
            Assert.IsTrue(KeyRules.NextTaken(false, outcome));
        }

        [Test]
        public void AlreadyTaken_SecondRequest_ChangesNothing()
        {
            KeyOutcome outcome = KeyRules.Evaluate(true, Valid);
            Assert.AreEqual(KeyOutcome.RejectedAlreadyTaken, outcome);
            Assert.IsTrue(KeyRules.NextTaken(true, outcome)); // reste pris, aucune seconde transition
        }

        [Test]
        public void TwoNearSimultaneousRequests_OnlyTheFirstIsAccepted()
        {
            bool taken = false;
            KeyOutcome first = KeyRules.Evaluate(taken, Valid);
            taken = KeyRules.NextTaken(taken, first);
            KeyOutcome second = KeyRules.Evaluate(taken, Valid);
            Assert.AreEqual(KeyOutcome.Taken, first);
            Assert.AreEqual(KeyOutcome.RejectedAlreadyTaken, second);
        }

        [TestCase("sender", KeyOutcome.RejectedInvalidSender)]
        [TestCase("target", KeyOutcome.RejectedInvalidTarget)]
        [TestCase("frozen", KeyOutcome.RejectedFrozen)]
        [TestCase("ko", KeyOutcome.RejectedKnockedOut)]
        [TestCase("carried", KeyOutcome.RejectedCarried)]
        [TestCase("carrying", KeyOutcome.RejectedCarrying)]
        [TestCase("range", KeyOutcome.RejectedOutOfRange)]
        public void InvalidRequests_AreRejected_AndKeepTheStateAvailable(string flaw, KeyOutcome expected)
        {
            KeyRequest r = Valid;
            switch (flaw)
            {
                case "sender": r.InvalidSender = true; break;
                case "target": r.InvalidTarget = true; break;
                case "frozen": r.Frozen = true; break;
                case "ko": r.KnockedOut = true; break;
                case "carried": r.Carried = true; break;
                case "carrying": r.Carrying = true; break;
                case "range": r.Distance = 9f; break;
            }
            KeyOutcome outcome = KeyRules.Evaluate(false, r);
            Assert.AreEqual(expected, outcome);
            Assert.IsFalse(KeyRules.NextTaken(false, outcome));
        }

        [Test]
        public void NonFiniteDistance_IsOutOfRange()
        {
            KeyRequest r = Valid;
            r.Distance = float.PositiveInfinity;
            Assert.AreEqual(KeyOutcome.RejectedOutOfRange, KeyRules.Evaluate(false, r));
            r.Distance = float.NaN;
            Assert.AreEqual(KeyOutcome.RejectedOutOfRange, KeyRules.Evaluate(false, r));
        }

        [Test]
        public void RoundReset_MakesTheKeysAvailableAgain() =>
            Assert.IsFalse(KeyRules.AfterRoundReset());

        [Test]
        public void RequesterDisconnect_AfterSuccess_KeepsTheCollectiveState()
        {
            Assert.IsTrue(KeyRules.AfterRequesterDisconnected(true));
            Assert.IsFalse(KeyRules.AfterRequesterDisconnected(false));
        }

        [Test]
        public void StatusLabels() =>
            Assert.AreEqual(("CLÉS · À PRENDRE", "CLÉS · PRISES"), (KeyRules.StatusLabel(false), KeyRules.StatusLabel(true)));
    }
}

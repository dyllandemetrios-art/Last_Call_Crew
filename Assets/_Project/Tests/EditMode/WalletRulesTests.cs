using LastCallCrew.Night;
using NUnit.Framework;

namespace LastCallCrew.Tests
{
    public class WalletRulesTests
    {
        static KeyRequest Valid => new KeyRequest { Distance = 1.5f, MaxDistance = 4f };

        [Test]
        public void Available_ValidRequest_IsTaken()
        {
            KeyOutcome outcome = WalletRules.Evaluate(false, Valid);
            Assert.AreEqual(KeyOutcome.Taken, outcome);
            Assert.IsTrue(WalletRules.NextTaken(false, outcome));
        }

        [Test]
        public void AlreadyTaken_SecondRequest_ChangesNothing()
        {
            KeyOutcome outcome = WalletRules.Evaluate(true, Valid);
            Assert.AreEqual(KeyOutcome.RejectedAlreadyTaken, outcome);
            Assert.IsTrue(WalletRules.NextTaken(true, outcome));
        }

        [Test]
        public void InvalidRequest_KeepsTheWalletAvailable()
        {
            KeyRequest r = Valid;
            r.Distance = 12f;
            KeyOutcome outcome = WalletRules.Evaluate(false, r);
            Assert.AreEqual(KeyOutcome.RejectedOutOfRange, outcome);
            Assert.IsFalse(WalletRules.NextTaken(false, outcome));
        }

        [Test]
        public void RoundReset_AndDisconnect_FollowTheCollectiveRule()
        {
            Assert.IsFalse(WalletRules.AfterRoundReset());
            Assert.IsTrue(WalletRules.AfterRequesterDisconnected(true));
        }

        [Test]
        public void WalletAndKeys_AreIndependent()
        {
            bool keys = false, wallet = false;
            wallet = WalletRules.NextTaken(wallet, WalletRules.Evaluate(wallet, Valid));
            Assert.IsTrue(wallet);
            Assert.IsFalse(keys); // prendre le portefeuille ne touche pas aux clés
            keys = KeyRules.NextTaken(keys, KeyRules.Evaluate(keys, Valid));
            Assert.IsTrue(keys);
            keys = KeyRules.AfterRoundReset();
            Assert.IsFalse(keys);
            Assert.IsTrue(wallet); // réinitialiser l'un ne touche pas l'autre ; la relance réinitialise les deux séparément
            wallet = WalletRules.AfterRoundReset();
            Assert.IsFalse(wallet);
        }

        [Test]
        public void StatusLabels() =>
            Assert.AreEqual(("PORTEFEUILLE · À PRENDRE", "PORTEFEUILLE · PRIS"), (WalletRules.StatusLabel(false), WalletRules.StatusLabel(true)));
    }
}

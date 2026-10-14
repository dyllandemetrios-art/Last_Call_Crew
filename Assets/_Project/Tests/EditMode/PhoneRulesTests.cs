using LastCallCrew.Night;
using NUnit.Framework;

namespace LastCallCrew.Tests
{
    public class PhoneRulesTests
    {
        static KeyRequest Valid => new KeyRequest { Distance = 1.5f, MaxDistance = 4f };

        [Test]
        public void Available_ValidRequest_IsTaken()
        {
            KeyOutcome outcome = PhoneRules.Evaluate(false, Valid);
            Assert.AreEqual(KeyOutcome.Taken, outcome);
            Assert.IsTrue(PhoneRules.NextTaken(false, outcome));
        }

        [Test]
        public void AlreadyTaken_SecondRequest_ChangesNothing() =>
            Assert.AreEqual(KeyOutcome.RejectedAlreadyTaken, PhoneRules.Evaluate(true, Valid));

        [Test]
        public void InvalidRequest_KeepsThePhoneAvailable()
        {
            KeyRequest r = Valid;
            r.KnockedOut = true;
            KeyOutcome outcome = PhoneRules.Evaluate(false, r);
            Assert.AreEqual(KeyOutcome.RejectedKnockedOut, outcome);
            Assert.IsFalse(PhoneRules.NextTaken(false, outcome));
        }

        [Test]
        public void RoundReset_AndDisconnect_FollowTheCollectiveRule()
        {
            Assert.IsFalse(PhoneRules.AfterRoundReset());
            Assert.IsTrue(PhoneRules.AfterRequesterDisconnected(true));
        }

        [Test]
        public void ThreeObjects_AreIndependent_UntilTheFullRoundReset()
        {
            bool keys = false, wallet = false, phone = false;
            phone = PhoneRules.NextTaken(phone, PhoneRules.Evaluate(phone, Valid));
            Assert.IsTrue(phone);
            Assert.IsFalse(keys || wallet);
            keys = KeyRules.NextTaken(keys, KeyRules.Evaluate(keys, Valid));
            Assert.IsFalse(DepartureChecklist.AllTaken(keys, wallet, phone));
            wallet = WalletRules.NextTaken(wallet, WalletRules.Evaluate(wallet, Valid));
            Assert.IsTrue(DepartureChecklist.AllTaken(keys, wallet, phone));
            keys = KeyRules.AfterRoundReset();
            wallet = WalletRules.AfterRoundReset();
            phone = PhoneRules.AfterRoundReset();
            Assert.AreEqual(0, DepartureChecklist.TakenCount(keys, wallet, phone));
        }

        [TestCase(false, false, false, 0)]
        [TestCase(true, false, true, 2)]
        [TestCase(true, true, true, 3)]
        public void Checklist_CountsTheThreeStates(bool k, bool w, bool p, int count)
        {
            Assert.AreEqual(count, DepartureChecklist.TakenCount(k, w, p));
            Assert.AreEqual(count == 3, DepartureChecklist.AllTaken(k, w, p));
        }

        [Test]
        public void ReadyIsAnnouncedOnlyWhenGoingFromTwoToThree()
        {
            Assert.IsFalse(DepartureChecklist.BecameReady(false, false));
            Assert.IsTrue(DepartureChecklist.BecameReady(false, true));   // 2 -> 3 : une seule annonce
            Assert.IsFalse(DepartureChecklist.BecameReady(true, true));   // déjà prêt : pas de répétition
            Assert.IsFalse(DepartureChecklist.BecameReady(true, false));  // relance : rien à annoncer
        }

        [Test]
        public void StatusLabels()
        {
            Assert.AreEqual(("TÉLÉPHONE · À PRENDRE", "TÉLÉPHONE · PRIS"), (PhoneRules.StatusLabel(false), PhoneRules.StatusLabel(true)));
            Assert.AreEqual("DÉPART · PRÊT", DepartureChecklist.ReadyStatus);
            Assert.AreEqual("PRÊTS À PARTIR", DepartureChecklist.ReadyAnnouncement);
        }
    }
}

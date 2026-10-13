using LastCallCrew.Dialogue;
using NUnit.Framework;

namespace LastCallCrew.Tests
{
    public class DialogueRulesTests
    {
        // Demande valide : joueur debout, libre, à portée, dialogue ouvert côté hôte.
        static DialogueRequest Valid() => new DialogueRequest { DialogueOpen = true, Distance = 2f, MaxDistance = 4f };

        [Test]
        public void ValidOpen_IsOpened() =>
            Assert.AreEqual(DialogueOutcome.Opened, DialogueRules.EvaluateOpen(Valid()));

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void ValidChoice_IsChosen(int choice) =>
            Assert.AreEqual(DialogueOutcome.Chosen, DialogueRules.EvaluateChoice(Valid(), choice));

        [TestCase(0)]
        [TestCase(4)]
        [TestCase(-1)]
        [TestCase(255)]
        public void InvalidChoice_IsRejected(int choice) =>
            Assert.AreEqual(DialogueOutcome.RejectedInvalidChoice, DialogueRules.EvaluateChoice(Valid(), choice));

        [Test]
        public void Choice_WithoutOpenDialogue_IsRejected()
        {
            DialogueRequest r = Valid(); r.DialogueOpen = false;
            Assert.AreEqual(DialogueOutcome.RejectedNotOpen, DialogueRules.EvaluateChoice(r, 1));
        }

        [Test]
        public void InvalidSender_IsRejected()
        {
            DialogueRequest r = Valid(); r.InvalidSender = true;
            Assert.AreEqual(DialogueOutcome.RejectedInvalidSender, DialogueRules.EvaluateOpen(r));
            Assert.AreEqual(DialogueOutcome.RejectedInvalidSender, DialogueRules.EvaluateChoice(r, 1));
        }

        [Test]
        public void InvalidTarget_IsRejected()
        {
            DialogueRequest r = Valid(); r.InvalidTarget = true;
            Assert.AreEqual(DialogueOutcome.RejectedInvalidTarget, DialogueRules.EvaluateChoice(r, 2));
        }

        [Test]
        public void KnockedOut_IsRejected()
        {
            DialogueRequest r = Valid(); r.KnockedOut = true;
            Assert.AreEqual(DialogueOutcome.RejectedKnockedOut, DialogueRules.EvaluateOpen(r));
            Assert.AreEqual(DialogueOutcome.RejectedKnockedOut, DialogueRules.EvaluateChoice(r, 1));
        }

        [Test]
        public void Carried_IsRejected()
        {
            DialogueRequest r = Valid(); r.Carried = true;
            Assert.AreEqual(DialogueOutcome.RejectedCarried, DialogueRules.EvaluateChoice(r, 1));
        }

        [Test]
        public void Carrying_IsRejected()
        {
            DialogueRequest r = Valid(); r.Carrying = true;
            Assert.AreEqual(DialogueOutcome.RejectedCarrying, DialogueRules.EvaluateChoice(r, 1));
        }

        [Test]
        public void Frozen_IsRejected()
        {
            DialogueRequest r = Valid(); r.Frozen = true;
            Assert.AreEqual(DialogueOutcome.RejectedFrozen, DialogueRules.EvaluateOpen(r));
            Assert.AreEqual(DialogueOutcome.RejectedFrozen, DialogueRules.EvaluateChoice(r, 3));
        }

        [Test]
        public void OutOfRange_IsRejected_AndExactRangeAccepted()
        {
            DialogueRequest r = Valid(); r.Distance = 4.01f;
            Assert.AreEqual(DialogueOutcome.RejectedOutOfRange, DialogueRules.EvaluateChoice(r, 1));
            r.Distance = r.MaxDistance;
            Assert.AreEqual(DialogueOutcome.Chosen, DialogueRules.EvaluateChoice(r, 1));
            r.Distance = float.NaN;
            Assert.AreEqual(DialogueOutcome.RejectedOutOfRange, DialogueRules.EvaluateChoice(r, 1));
        }

        [Test]
        public void ChoiceCount_IsThree() => Assert.AreEqual(3, DialogueRules.ChoiceCount);
    }
}

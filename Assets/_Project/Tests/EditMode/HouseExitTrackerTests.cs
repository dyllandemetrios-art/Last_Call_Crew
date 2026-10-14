using LastCallCrew.Night;
using NUnit.Framework;
using UnityEngine;

namespace LastCallCrew.Tests
{
    public class HouseExitTrackerTests
    {
        static readonly ulong[] Pair = { 0UL, 1UL };

        [Test]
        public void NobodyOutside_DoesNotActivate()
        {
            var t = new HouseExitTracker();
            t.Observe(0UL, true);
            t.Observe(1UL, true);
            Assert.IsFalse(t.TryActivate(Pair));
            Assert.IsFalse(t.Active);
        }

        [Test]
        public void OnlyOnePlayerOutside_DoesNotActivate()
        {
            var t = new HouseExitTracker();
            t.Observe(0UL, false);
            t.Observe(1UL, true);
            Assert.IsFalse(t.TryActivate(Pair));
        }

        [Test]
        public void EveryoneHasBeenOutside_ActivatesOnce()
        {
            var t = new HouseExitTracker();
            t.Observe(0UL, false);
            t.Observe(1UL, false);
            Assert.IsTrue(t.TryActivate(Pair));
            Assert.IsTrue(t.Active);
            Assert.IsFalse(t.TryActivate(Pair)); // une seule activation
        }

        [Test]
        public void ExitThenEarlyReturn_KeepsTheExit_ButWaitsForThePartner()
        {
            var t = new HouseExitTracker();
            t.Observe(0UL, false);
            t.Observe(0UL, true); // revient avant son partenaire
            Assert.IsTrue(t.HasExited(0UL));
            Assert.IsFalse(t.TryActivate(Pair));
            t.Observe(1UL, false);
            Assert.IsTrue(t.TryActivate(Pair));
        }

        [Test]
        public void Activation_IsIrreversible_UntilReset()
        {
            var t = new HouseExitTracker();
            t.Observe(0UL, false);
            t.Observe(1UL, false);
            t.TryActivate(Pair);
            t.Observe(0UL, true);
            t.Observe(1UL, true);
            Assert.IsTrue(t.Active);
        }

        [Test]
        public void Reset_ClearsExitsAndActivation()
        {
            var t = new HouseExitTracker();
            t.Observe(0UL, false);
            t.Observe(1UL, false);
            t.TryActivate(Pair);
            t.Reset();
            Assert.IsFalse(t.Active);
            Assert.IsFalse(t.HasExited(0UL));
            Assert.IsFalse(t.TryActivate(Pair));
        }

        [Test]
        public void ASingleConnectedPlayer_CanNeverActivate()
        {
            var t = new HouseExitTracker();
            t.Observe(0UL, false);
            Assert.IsFalse(t.TryActivate(new[] { 0UL }));
        }

        [Test]
        public void Roster_UsesTheCurrentPlayers_NotTheirClientIdValues()
        {
            var t = new HouseExitTracker();
            t.Observe(0UL, false);
            t.Observe(47UL, false);
            Assert.IsTrue(t.TryActivate(new[] { 0UL, 47UL }));
        }

        [TestCase(0, 15.2f)]
        [TestCase(1, 16.8f)]
        public void StartSlots_AreInsideTheHouse_AndDoNotOverlap(int slot, float z)
        {
            Vector3 p = RestartRules.StartPosition(slot);
            Assert.AreEqual(21f, p.x, 1e-4f);
            Assert.AreEqual(z, p.z, 1e-4f);
            Assert.IsTrue(p.x > 15.5f && p.x < 24.5f && p.z > 11.5f && p.z < 20.5f, "dans l'intérieur 9 × 9 de la maison");
        }

        [Test]
        public void TwoStartSlots_AreAtLeastOneCapsuleApart_EvenWithHighClientIds()
        {
            var ids = new System.Collections.Generic.List<ulong> { 0UL, 47UL };
            Vector3 a = RestartRules.StartPosition(PlayerSlots.Rank(0UL, ids));
            Vector3 b = RestartRules.StartPosition(PlayerSlots.Rank(47UL, ids));
            Assert.Greater(Vector3.Distance(a, b), 1.2f);
        }
    }
}

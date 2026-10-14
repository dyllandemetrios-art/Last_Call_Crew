using LastCallCrew.Connection;
using NUnit.Framework;

namespace LastCallCrew.Tests
{
    public class RelayJoinRulesTests
    {
        [TestCase("  abc123 ", "ABC123")]
        [TestCase("xy9z", "XY9Z")]
        [TestCase("", "")]
        [TestCase("   ", "")]
        [TestCase(null, "")]
        public void Normalize_TrimsAndUppercases(string raw, string expected) =>
            Assert.AreEqual(expected, RelayJoinRules.Normalize(raw));

        [TestCase("ABC123", true)]
        [TestCase("", false)]
        [TestCase("AB CD", false)]
        [TestCase("AB-CD", false)]
        [TestCase("ABCDEFGHIJKLM", false)]
        public void IsPlausibleCode_FiltersObviousGarbage(string code, bool expected) =>
            Assert.AreEqual(expected, RelayJoinRules.IsPlausibleCode(code));

        [Test]
        public void OnlyIdleCanBegin()
        {
            Assert.IsTrue(RelayJoinRules.CanBegin(ConnectionPhase.Idle));
            foreach (var p in new[] { ConnectionPhase.Connecting, ConnectionPhase.Hosting, ConnectionPhase.Joined, ConnectionPhase.Stopping })
                Assert.IsFalse(RelayJoinRules.CanBegin(p));
        }

        [Test]
        public void OnlyActiveSessionsCanStop()
        {
            Assert.IsTrue(RelayJoinRules.CanStop(ConnectionPhase.Hosting));
            Assert.IsTrue(RelayJoinRules.CanStop(ConnectionPhase.Joined));
            Assert.IsFalse(RelayJoinRules.CanStop(ConnectionPhase.Idle));
            Assert.IsFalse(RelayJoinRules.CanStop(ConnectionPhase.Connecting));
            Assert.IsFalse(RelayJoinRules.CanStop(ConnectionPhase.Stopping));
        }
    }
}

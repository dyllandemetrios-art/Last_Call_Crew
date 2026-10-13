using LastCallCrew.Carry;
using LastCallCrew.Config;
using NUnit.Framework;
using UnityEngine;

namespace LastCallCrew.Tests
{
    public class CarryRulesTests
    {
        TuningConfig _t;

        [SetUp]
        public void SetUp() => _t = ScriptableObject.CreateInstance<TuningConfig>();

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_t);

        // Demande valide : porteur debout et libre, cible KO libre, à portée.
        static CarryStartRequest Valid() => new CarryStartRequest
        {
            TargetKnockedOut = true,
            Distance = 2f,
            MaxDistance = 4f,
        };

        [Test]
        public void ValidRequest_Starts() =>
            Assert.AreEqual(CarryOutcome.Started, CarryRules.EvaluateStart(Valid()));

        [Test]
        public void AtExactRange_Starts()
        {
            CarryStartRequest r = Valid();
            r.Distance = r.MaxDistance;
            Assert.AreEqual(CarryOutcome.Started, CarryRules.EvaluateStart(r));
        }

        [Test]
        public void OutOfRange_IsRejected()
        {
            CarryStartRequest r = Valid();
            r.Distance = r.MaxDistance + 0.01f;
            Assert.AreEqual(CarryOutcome.RejectedOutOfRange, CarryRules.EvaluateStart(r));
        }

        [Test]
        public void InvalidDistance_IsRejected()
        {
            CarryStartRequest r = Valid();
            r.Distance = float.NaN;
            Assert.AreEqual(CarryOutcome.RejectedOutOfRange, CarryRules.EvaluateStart(r));
        }

        [Test]
        public void SelfCarry_IsRejected()
        {
            CarryStartRequest r = Valid();
            r.IsSelf = true;
            Assert.AreEqual(CarryOutcome.RejectedSelf, CarryRules.EvaluateStart(r));
        }

        [Test]
        public void StandingTarget_IsRejected()
        {
            CarryStartRequest r = Valid();
            r.TargetKnockedOut = false;
            Assert.AreEqual(CarryOutcome.RejectedTargetNotKnockedOut, CarryRules.EvaluateStart(r));
        }

        [Test]
        public void KnockedOutRequester_IsRejected()
        {
            CarryStartRequest r = Valid();
            r.RequesterKnockedOut = true;
            Assert.AreEqual(CarryOutcome.RejectedRequesterKnockedOut, CarryRules.EvaluateStart(r));
        }

        [Test]
        public void RequesterAlreadyCarrying_IsRejected()
        {
            CarryStartRequest r = Valid();
            r.RequesterIsCarrying = true;
            Assert.AreEqual(CarryOutcome.RejectedAlreadyCarrying, CarryRules.EvaluateStart(r));
        }

        [Test]
        public void RequesterBeingCarried_IsRejected()
        {
            CarryStartRequest r = Valid();
            r.RequesterIsCarried = true;
            Assert.AreEqual(CarryOutcome.RejectedRequesterCarried, CarryRules.EvaluateStart(r));
        }

        [Test]
        public void TargetAlreadyCarried_IsRejected()
        {
            CarryStartRequest r = Valid();
            r.TargetIsCarried = true;
            Assert.AreEqual(CarryOutcome.RejectedTargetAlreadyCarried, CarryRules.EvaluateStart(r));
        }

        [Test]
        public void TargetThatCarriesSomeone_IsRejected()
        {
            CarryStartRequest r = Valid();
            r.TargetIsCarrying = true;
            Assert.AreEqual(CarryOutcome.RejectedTargetCarrying, CarryRules.EvaluateStart(r));
        }

        [Test]
        public void Drop_OnlyWhenCarrying()
        {
            Assert.AreEqual(CarryOutcome.Dropped, CarryRules.EvaluateDrop(true));
            Assert.AreEqual(CarryOutcome.RejectedNotCarrying, CarryRules.EvaluateDrop(false));
        }

        [Test]
        public void Speed_UsesConfiguredMultiplierOnlyWhileCarrying()
        {
            Assert.AreEqual(_t.CarrySpeedMultiplier, CarryRules.SpeedMultiplier(true, _t));
            Assert.AreEqual(1f, CarryRules.SpeedMultiplier(false, _t));
            Assert.AreEqual(0.6f, CarryRules.SpeedMultiplier(true, _t), 1e-6f);
        }

        [Test]
        public void Speed_FollowsTheConfigNotALiteral()
        {
            var custom = ScriptableObject.CreateInstance<TuningConfig>();
            try
            {
                typeof(TuningConfig).GetField("_carrySpeedMultiplier", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(custom, 0.25f);
                Assert.AreEqual(0.25f, CarryRules.SpeedMultiplier(true, custom));
            }
            finally
            {
                Object.DestroyImmediate(custom);
            }
        }

        [Test]
        public void DropPosition_IsInFrontOfTheCarrierAtTheSameHeight()
        {
            Vector3 drop = CarryRules.DropPosition(new Vector3(2f, 1f, 3f), new Vector3(0f, 0f, 1f), 1.2f);
            Assert.AreEqual(2f, drop.x, 1e-5f);
            Assert.AreEqual(1f, drop.y, 1e-5f);
            Assert.AreEqual(4.2f, drop.z, 1e-5f);
        }

        [Test]
        public void DropPosition_IgnoresPitchAndNeverGoesBelowTheCarrier()
        {
            // Une direction fortement inclinée vers le bas ne doit pas enfoncer le joueur sous le niveau.
            Vector3 drop = CarryRules.DropPosition(new Vector3(0f, 1f, 0f), new Vector3(1f, -5f, 0f), 1f);
            Assert.AreEqual(1f, drop.y, 1e-5f);
            Assert.AreEqual(1f, drop.x, 1e-5f);
        }

        [Test]
        public void DropPosition_WithDegenerateForward_StillMoves()
        {
            Vector3 drop = CarryRules.DropPosition(Vector3.zero, Vector3.up, 1f);
            Assert.AreEqual(1f, drop.magnitude, 1e-5f);
            Assert.AreEqual(0f, drop.y, 1e-5f);
        }
    }
}

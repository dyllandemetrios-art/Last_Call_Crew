using LastCallCrew.Night;
using NUnit.Framework;
using UnityEngine;

namespace LastCallCrew.Tests
{
    public class TaxiRulesTests
    {
        static TaxiRequest Valid => new TaxiRequest { Distance = 1.5f, MaxDistance = 4f, PhoneTaken = true, WalletTaken = true };

        static readonly TaxiPassenger Ready = new TaxiPassenger { InZone = true };
        static TaxiPassenger[] TwoReady => new[] { Ready, Ready };

        // ---- Appel ----

        [Test]
        public void Call_FromOtherStop_WithPhone_StartsEmptyTrip()
        {
            Assert.AreEqual(TaxiOutcome.Called, TaxiRules.Evaluate(TaxiState.AtA, TaxiStop.B, Valid, null));
            Assert.AreEqual(TaxiState.EmptyToB, TaxiRules.AfterCall(TaxiState.AtA, TaxiStop.B));
            Assert.AreEqual(TaxiState.EmptyToA, TaxiRules.AfterCall(TaxiState.AtB, TaxiStop.A));
        }

        [Test]
        public void Call_WithoutPhone_IsRejected()
        {
            TaxiRequest r = Valid;
            r.PhoneTaken = false;
            Assert.AreEqual(TaxiOutcome.RejectedNoPhone, TaxiRules.Evaluate(TaxiState.AtA, TaxiStop.B, r, null));
        }

        [Test]
        public void Call_DoesNotNeedWalletNorPartner()
        {
            TaxiRequest r = Valid;
            r.WalletTaken = false;
            Assert.AreEqual(TaxiOutcome.Called, TaxiRules.Evaluate(TaxiState.AtB, TaxiStop.A, r, new TaxiPassenger[0]));
        }

        [Test]
        public void SecondCall_WhileComing_DoesNotChangeTheTaxi()
        {
            Assert.AreEqual(TaxiOutcome.RejectedAlreadyComing, TaxiRules.Evaluate(TaxiState.EmptyToB, TaxiStop.B, Valid, null));
            Assert.AreEqual(TaxiState.EmptyToB, TaxiRules.AfterCall(TaxiState.EmptyToB, TaxiStop.B));
            Assert.AreEqual(TaxiState.EmptyToB, TaxiRules.AfterCall(TaxiState.EmptyToB, TaxiStop.A));
        }

        [Test]
        public void Request_WhileMovingElsewhere_IsRejected()
        {
            Assert.AreEqual(TaxiOutcome.RejectedInMotion, TaxiRules.Evaluate(TaxiState.EmptyToB, TaxiStop.A, Valid, TwoReady));
            Assert.AreEqual(TaxiOutcome.RejectedInMotion, TaxiRules.Evaluate(TaxiState.PassengersToB, TaxiStop.B, Valid, TwoReady));
            Assert.AreEqual(TaxiOutcome.RejectedInMotion, TaxiRules.Evaluate(TaxiState.PassengersToA, TaxiStop.B, Valid, TwoReady));
        }

        // ---- Départ ----

        [Test]
        public void Departure_AllReady_Departs()
        {
            Assert.AreEqual(TaxiOutcome.Departed, TaxiRules.Evaluate(TaxiState.AtA, TaxiStop.A, Valid, TwoReady));
            Assert.AreEqual(TaxiState.PassengersToB, TaxiRules.AfterDeparture(TaxiState.AtA));
            Assert.AreEqual(TaxiState.PassengersToA, TaxiRules.AfterDeparture(TaxiState.AtB));
            Assert.AreEqual(TaxiOutcome.Departed, TaxiRules.Evaluate(TaxiState.AtB, TaxiStop.B, Valid, TwoReady));
        }

        [Test]
        public void Departure_WithoutPhone_IsRejectedBeforeWallet()
        {
            TaxiRequest r = Valid;
            r.PhoneTaken = false;
            r.WalletTaken = false;
            Assert.AreEqual(TaxiOutcome.RejectedNoPhone, TaxiRules.Evaluate(TaxiState.AtA, TaxiStop.A, r, TwoReady));
        }

        [Test]
        public void Departure_WithoutWallet_IsRejected()
        {
            TaxiRequest r = Valid;
            r.WalletTaken = false;
            Assert.AreEqual(TaxiOutcome.RejectedNoWallet, TaxiRules.Evaluate(TaxiState.AtA, TaxiStop.A, r, TwoReady));
        }

        [Test]
        public void Departure_PartnerOutsideZone_Waits()
        {
            var players = new[] { Ready, new TaxiPassenger { InZone = false } };
            Assert.AreEqual(TaxiOutcome.RejectedPartnerMissing, TaxiRules.Evaluate(TaxiState.AtA, TaxiStop.A, Valid, players));
        }

        [Test]
        public void Departure_SingleConnectedPlayer_IsRejected()
        {
            Assert.AreEqual(TaxiOutcome.RejectedNotEnoughPlayers, TaxiRules.Evaluate(TaxiState.AtA, TaxiStop.A, Valid, new[] { Ready }));
            Assert.AreEqual(TaxiOutcome.RejectedNotEnoughPlayers, TaxiRules.Evaluate(TaxiState.AtA, TaxiStop.A, Valid, null));
        }

        [Test]
        public void Departure_KnockedOutPartnerOnTheGround_IsRejected()
        {
            var ko = new TaxiPassenger { InZone = true, KnockedOut = true };
            Assert.IsFalse(ko.IsReady);
            Assert.AreEqual(TaxiOutcome.RejectedPartnerKnockedOutNotCarried, TaxiRules.Evaluate(TaxiState.AtA, TaxiStop.A, Valid, new[] { Ready, ko }));
        }

        [Test]
        public void Departure_KnockedOutPartnerCarriedByPlayerInZone_Departs()
        {
            var ko = new TaxiPassenger { InZone = true, KnockedOut = true, Carried = true, CarrierInZone = true };
            Assert.IsTrue(ko.IsReady);
            Assert.AreEqual(TaxiOutcome.Departed, TaxiRules.Evaluate(TaxiState.AtA, TaxiStop.A, Valid, new[] { Ready, ko }));
        }

        [Test]
        public void Departure_KnockedOutPartnerCarriedOutsideZone_IsRejected()
        {
            var ko = new TaxiPassenger { InZone = false, KnockedOut = true, Carried = true, CarrierInZone = false };
            Assert.AreEqual(TaxiOutcome.RejectedPartnerMissing, TaxiRules.Evaluate(TaxiState.AtA, TaxiStop.A, Valid, new[] { new TaxiPassenger { InZone = false }, ko }));
        }

        [Test]
        public void Departure_CarriedFlagWithoutKnockout_StillNeedsTheZone()
        {
            var odd = new TaxiPassenger { InZone = false, Carried = true, CarrierInZone = true };
            Assert.IsFalse(odd.IsReady);
        }

        // ---- Demandeur et cible ----

        [Test]
        public void Request_WrongSenderOrTarget_IsRejectedFirst()
        {
            TaxiRequest r = Valid;
            r.InvalidSender = true;
            r.InvalidTarget = true;
            Assert.AreEqual(TaxiOutcome.RejectedInvalidSender, TaxiRules.Evaluate(TaxiState.AtA, TaxiStop.A, r, TwoReady));
            r.InvalidSender = false;
            Assert.AreEqual(TaxiOutcome.RejectedInvalidTarget, TaxiRules.Evaluate(TaxiState.AtA, TaxiStop.A, r, TwoReady));
        }

        [Test]
        public void Request_FrozenKnockedOutCarriedOrFar_IsRejected()
        {
            TaxiRequest r = Valid;
            r.Frozen = true;
            Assert.AreEqual(TaxiOutcome.RejectedFrozen, TaxiRules.Evaluate(TaxiState.AtA, TaxiStop.A, r, TwoReady));
            r = Valid;
            r.RequesterKnockedOut = true;
            Assert.AreEqual(TaxiOutcome.RejectedKnockedOut, TaxiRules.Evaluate(TaxiState.AtA, TaxiStop.A, r, TwoReady));
            r = Valid;
            r.RequesterCarried = true;
            Assert.AreEqual(TaxiOutcome.RejectedCarried, TaxiRules.Evaluate(TaxiState.AtA, TaxiStop.A, r, TwoReady));
            r = Valid;
            r.Distance = 9f;
            Assert.AreEqual(TaxiOutcome.RejectedOutOfRange, TaxiRules.Evaluate(TaxiState.AtB, TaxiStop.A, r, TwoReady));
            r.Distance = float.NaN;
            Assert.AreEqual(TaxiOutcome.RejectedOutOfRange, TaxiRules.Evaluate(TaxiState.AtB, TaxiStop.A, r, TwoReady));
        }

        // ---- Transitions ----

        [Test]
        public void Arrival_ParksAtTheDestination()
        {
            Assert.AreEqual(TaxiState.AtA, TaxiRules.AfterArrival(TaxiState.EmptyToA));
            Assert.AreEqual(TaxiState.AtA, TaxiRules.AfterArrival(TaxiState.PassengersToA));
            Assert.AreEqual(TaxiState.AtB, TaxiRules.AfterArrival(TaxiState.EmptyToB));
            Assert.AreEqual(TaxiState.AtB, TaxiRules.AfterArrival(TaxiState.PassengersToB));
            Assert.AreEqual(TaxiState.AtA, TaxiRules.AfterArrival(TaxiState.AtA));
        }

        [Test]
        public void RoundTrip_ReturnsToTheStart()
        {
            TaxiState s = TaxiState.AtA;
            s = TaxiRules.AfterArrival(TaxiRules.AfterDeparture(s));
            Assert.AreEqual(TaxiState.AtB, s);
            s = TaxiRules.AfterArrival(TaxiRules.AfterDeparture(s));
            Assert.AreEqual(TaxiState.AtA, s);
        }

        [Test]
        public void EmptyCall_ThenGroupDeparture_AreTwoSeparateTransitions()
        {
            TaxiState s = TaxiRules.AfterCall(TaxiState.AtB, TaxiStop.A);
            Assert.AreEqual(TaxiState.EmptyToA, s);
            Assert.IsFalse(TaxiRules.IsCarryingPassengers(s));
            s = TaxiRules.AfterArrival(s);
            Assert.AreEqual(TaxiState.AtA, s);
            Assert.AreEqual(TaxiState.PassengersToB, TaxiRules.AfterDeparture(s));
        }

        [Test]
        public void RoundReset_BringsTheTaxiBackToA_FromAnyState()
        {
            foreach (TaxiState state in System.Enum.GetValues(typeof(TaxiState)))
                Assert.AreEqual(TaxiState.AtA, TaxiRules.AfterRoundReset());
        }

        [Test]
        public void OnlyPassengerTripsBlockControls()
        {
            Assert.IsFalse(TaxiRules.IsCarryingPassengers(TaxiState.AtA));
            Assert.IsFalse(TaxiRules.IsCarryingPassengers(TaxiState.EmptyToB));
            Assert.IsTrue(TaxiRules.IsCarryingPassengers(TaxiState.PassengersToA));
            Assert.IsTrue(TaxiRules.IsCarryingPassengers(TaxiState.PassengersToB));
        }

        [Test]
        public void Prompts_FollowTheState()
        {
            Assert.AreEqual("E — PARTIR VERS LA VILLE", TaxiRules.Prompt(TaxiState.AtA, TaxiStop.A));
            Assert.AreEqual("E — APPELER LE TAXI", TaxiRules.Prompt(TaxiState.AtA, TaxiStop.B));
            Assert.AreEqual("TAXI EN APPROCHE", TaxiRules.Prompt(TaxiState.EmptyToB, TaxiStop.B));
            Assert.AreEqual("E — RENTRER À LA COLOC", TaxiRules.Prompt(TaxiState.AtB, TaxiStop.B));
            Assert.AreEqual("TAXI EN ROUTE", TaxiRules.Prompt(TaxiState.PassengersToB, TaxiStop.B));
            Assert.AreEqual("TÉLÉPHONE OUBLIÉ", TaxiRules.Feedback(TaxiOutcome.RejectedNoPhone));
            Assert.AreEqual("PORTEFEUILLE OUBLIÉ", TaxiRules.Feedback(TaxiOutcome.RejectedNoWallet));
            Assert.AreEqual("EN ATTENTE DU PARTENAIRE", TaxiRules.Feedback(TaxiOutcome.RejectedPartnerMissing));
            Assert.AreEqual(string.Empty, TaxiRules.StatusLabel(TaxiState.AtA, 0));
            Assert.AreEqual("TAXI · EN ROUTE → B · 9 s", TaxiRules.StatusLabel(TaxiState.PassengersToB, 9));
        }

        // ---- Trajet ----

        [Test]
        public void Path_StartsAndEndsOnTheStops()
        {
            TaxiPath path = TaxiPath.CreateDefault();
            path.Sample(0f, true, out Vector3 start, out _);
            path.Sample(1f, true, out Vector3 end, out _);
            Assert.AreEqual(0f, Vector3.Distance(start, TaxiPath.DefaultAxis[0]), 0.01f);
            Assert.AreEqual(0f, Vector3.Distance(end, TaxiPath.DefaultAxis[3]), 0.01f);
            path.Sample(0f, false, out Vector3 backStart, out _);
            path.Sample(1f, false, out Vector3 backEnd, out _);
            Assert.AreEqual(0f, Vector3.Distance(backStart, end), 0.01f);
            Assert.AreEqual(0f, Vector3.Distance(backEnd, start), 0.01f);
        }

        [Test]
        public void Path_LengthIsCloseToTheMapAxis_AndCornersAreRounded()
        {
            TaxiPath path = TaxiPath.CreateDefault();
            Assert.Less(path.Length, 12f + 130f + 17f);
            Assert.Greater(path.Length, 150f);
        }

        [Test]
        public void Path_ProgressIsMonotonicAndStaysOnTheRoad()
        {
            TaxiPath path = TaxiPath.CreateDefault();
            path.Sample(0f, true, out Vector3 previous, out _);
            float travelled = 0f;
            for (int i = 1; i <= 200; i++)
            {
                path.Sample(i / 200f, true, out Vector3 p, out float yaw);
                travelled += Vector3.Distance(previous, p);
                previous = p;
                Assert.IsFalse(float.IsNaN(yaw));
                Assert.GreaterOrEqual(p.z, -4.01f);
                Assert.LessOrEqual(p.x, 0.01f);
                Assert.GreaterOrEqual(p.x, -130.01f);
                Assert.AreEqual(0f, p.y);
            }
            Assert.AreEqual(path.Length, travelled, path.Length * 0.02f);
        }

        [Test]
        public void Path_EaseIsMonotonicWithSoftEnds()
        {
            float last = 0f;
            for (int i = 1; i <= 100; i++)
            {
                float e = TaxiPath.Ease(i / 100f);
                Assert.GreaterOrEqual(e, last);
                last = e;
            }
            Assert.AreEqual(0f, TaxiPath.Ease(0f));
            Assert.AreEqual(1f, TaxiPath.Ease(1f), 1e-5f);
            Assert.Less(TaxiPath.Ease(0.05f), 0.05f);
        }

        [Test]
        public void Path_TheTripEndsFacingTheDepartureDirection()
        {
            TaxiPath path = TaxiPath.CreateDefault();
            path.Sample(1f, true, out _, out float yawB);
            path.Sample(1f, false, out _, out float yawA);
            Assert.AreEqual(0f, Mathf.DeltaAngle(yawB, 180f), 0.5f);
            Assert.AreEqual(0f, Mathf.DeltaAngle(yawA, 180f), 0.5f);
        }
    }
}

using System.Collections.Generic;

namespace LastCallCrew.Night
{
    /// <summary>État répliqué du taxi collectif (P1-055). A = coloc/quartier résidentiel, B = place du quartier nocturne.</summary>
    public enum TaxiState : byte
    {
        AtA,
        AtB,
        EmptyToA,
        EmptyToB,
        PassengersToA,
        PassengersToB,
    }

    public enum TaxiStop : byte
    {
        A = 0,
        B = 1,
    }

    /// <summary>Issue d'une interaction avec une borne, jugée par l'hôte (P1-055).</summary>
    public enum TaxiOutcome : byte
    {
        Called,
        Departed,
        RejectedInvalidSender,
        RejectedInvalidTarget,
        RejectedFrozen,
        RejectedKnockedOut,
        RejectedCarried,
        RejectedOutOfRange,
        RejectedInMotion,
        RejectedAlreadyComing,
        RejectedNoPhone,
        RejectedNoWallet,
        RejectedNotEnoughPlayers,
        RejectedPartnerMissing,
        RejectedPartnerKnockedOutNotCarried,
    }

    /// <summary>État du demandeur et de la borne au moment d'une interaction, lu par l'hôte.</summary>
    public struct TaxiRequest
    {
        public bool InvalidSender;
        public bool InvalidTarget;
        public bool Frozen;
        public bool RequesterKnockedOut;
        public bool RequesterCarried;
        public float Distance;
        public float MaxDistance;
        public bool PhoneTaken;
        public bool WalletTaken;
    }

    /// <summary>Situation d'un joueur connecté vis-à-vis de la zone d'embarquement de l'arrêt courant.</summary>
    public struct TaxiPassenger
    {
        public bool InZone;
        public bool KnockedOut;
        public bool Carried;
        /// <summary>Vrai si le porteur de ce joueur se trouve dans la zone (sans objet s'il n'est pas porté).</summary>
        public bool CarrierInZone;

        /// <summary>Un joueur debout compte s'il est dans la zone ; un KO ne compte que porté par un joueur présent.</summary>
        public bool IsReady => KnockedOut ? Carried && CarrierInZone : InZone;
    }

    /// <summary>
    /// Règles pures du taxi collectif (P1-055, sans NGO) : validation d'une interaction par l'hôte et transitions d'état.
    /// Le taxi est unique ; une seconde demande pendant un trajet ne le modifie pas.
    /// </summary>
    public static class TaxiRules
    {
        public const int PlayersRequired = 2;
        public const float DefaultTripSeconds = 12f;

        public static TaxiState AtStop(TaxiStop stop) => stop == TaxiStop.A ? TaxiState.AtA : TaxiState.AtB;

        public static bool IsParked(TaxiState state) => state == TaxiState.AtA || state == TaxiState.AtB;

        public static bool IsMoving(TaxiState state) => !IsParked(state);

        /// <summary>Vrai pendant un trajet avec passagers : contrôles, regard et interactions bloqués.</summary>
        public static bool IsCarryingPassengers(TaxiState state) =>
            state == TaxiState.PassengersToA || state == TaxiState.PassengersToB;

        /// <summary>Arrêt où le taxi est garé, ou vers lequel il roule.</summary>
        public static TaxiStop Stop(TaxiState state) =>
            state == TaxiState.AtA || state == TaxiState.EmptyToA || state == TaxiState.PassengersToA ? TaxiStop.A : TaxiStop.B;

        public static TaxiState AfterCall(TaxiState state, TaxiStop caller) =>
            IsParked(state) && Stop(state) != caller ? (caller == TaxiStop.A ? TaxiState.EmptyToA : TaxiState.EmptyToB) : state;

        public static TaxiState AfterDeparture(TaxiState state) =>
            state == TaxiState.AtA ? TaxiState.PassengersToB : state == TaxiState.AtB ? TaxiState.PassengersToA : state;

        public static TaxiState AfterArrival(TaxiState state)
        {
            switch (state)
            {
                case TaxiState.EmptyToA:
                case TaxiState.PassengersToA:
                    return TaxiState.AtA;
                case TaxiState.EmptyToB:
                case TaxiState.PassengersToB:
                    return TaxiState.AtB;
                default:
                    return state;
            }
        }

        /// <summary>Relance de manche : le taxi revient à A, tout trajet est annulé.</summary>
        public static TaxiState AfterRoundReset() => TaxiState.AtA;

        /// <summary>
        /// Validation par l'hôte d'une interaction sur la borne <paramref name="stop"/> : appel si le taxi est garé à l'autre arrêt,
        /// départ s'il est garé à celui-ci. <paramref name="players"/> contient tous les joueurs connectés (appel : non lu).
        /// </summary>
        public static TaxiOutcome Evaluate(TaxiState state, TaxiStop stop, in TaxiRequest r, IReadOnlyList<TaxiPassenger> players)
        {
            if (r.InvalidSender) return TaxiOutcome.RejectedInvalidSender;
            if (r.InvalidTarget) return TaxiOutcome.RejectedInvalidTarget;
            if (r.Frozen) return TaxiOutcome.RejectedFrozen;
            if (r.RequesterKnockedOut) return TaxiOutcome.RejectedKnockedOut;
            if (r.RequesterCarried) return TaxiOutcome.RejectedCarried;
            if (!(r.Distance <= r.MaxDistance)) return TaxiOutcome.RejectedOutOfRange;

            if (IsMoving(state))
                return Stop(state) == stop && !IsCarryingPassengers(state) ? TaxiOutcome.RejectedAlreadyComing : TaxiOutcome.RejectedInMotion;

            if (Stop(state) != stop)
                return r.PhoneTaken ? TaxiOutcome.Called : TaxiOutcome.RejectedNoPhone;

            if (!r.PhoneTaken) return TaxiOutcome.RejectedNoPhone;
            if (!r.WalletTaken) return TaxiOutcome.RejectedNoWallet;
            if (players == null || players.Count < PlayersRequired) return TaxiOutcome.RejectedNotEnoughPlayers;
            for (int i = 0; i < players.Count; i++)
            {
                TaxiPassenger p = players[i];
                if (p.IsReady)
                    continue;
                return p.KnockedOut && !p.Carried ? TaxiOutcome.RejectedPartnerKnockedOutNotCarried : TaxiOutcome.RejectedPartnerMissing;
            }
            return TaxiOutcome.Departed;
        }

        /// <summary>Libellé de l'invite à une borne, d'après l'état répliqué (affichage seulement).</summary>
        public static string Prompt(TaxiState state, TaxiStop stop)
        {
            if (IsParked(state))
            {
                if (Stop(state) != stop)
                    return "E — APPELER LE TAXI";
                return stop == TaxiStop.A ? "E — PARTIR VERS LA VILLE" : "E — RENTRER À LA COLOC";
            }
            if (IsCarryingPassengers(state))
                return "TAXI EN ROUTE";
            return Stop(state) == stop ? "TAXI EN APPROCHE" : "TAXI EN ROUTE";
        }

        public static string StatusLabel(TaxiState state, int secondsLeft)
        {
            if (IsParked(state))
                return string.Empty;
            string where = Stop(state) == TaxiStop.A ? "A" : "B";
            string kind = IsCarryingPassengers(state) ? "EN ROUTE" : "À VIDE";
            return $"TAXI · {kind} → {where} · {secondsLeft} s";
        }

        public static string Feedback(TaxiOutcome outcome)
        {
            switch (outcome)
            {
                case TaxiOutcome.Called: return "TAXI APPELÉ";
                case TaxiOutcome.Departed: return string.Empty;
                case TaxiOutcome.RejectedOutOfRange: return "TROP LOIN";
                case TaxiOutcome.RejectedFrozen: return "NUIT TERMINÉE";
                case TaxiOutcome.RejectedInMotion: return "TAXI EN ROUTE";
                case TaxiOutcome.RejectedAlreadyComing: return "TAXI DÉJÀ EN APPROCHE";
                case TaxiOutcome.RejectedNoPhone: return "TÉLÉPHONE OUBLIÉ";
                case TaxiOutcome.RejectedNoWallet: return "PORTEFEUILLE OUBLIÉ";
                case TaxiOutcome.RejectedNotEnoughPlayers: return "EN ATTENTE DU PARTENAIRE";
                case TaxiOutcome.RejectedPartnerMissing: return "EN ATTENTE DU PARTENAIRE";
                case TaxiOutcome.RejectedPartnerKnockedOutNotCarried: return "PORTEZ VOTRE PARTENAIRE KO";
                case TaxiOutcome.RejectedKnockedOut: return string.Empty;
                default: return "DEMANDE REFUSÉE";
            }
        }
    }
}

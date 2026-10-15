using System.Collections.Generic;
using LastCallCrew.Night;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace LastCallCrew.Network
{
    /// <summary>
    /// Taxi collectif (P1-055), unique, dans la scène continue. L'hôte décide : état, destination, instants de départ et
    /// durée (petites NetworkVariable écrites à chaque transition, jamais à chaque frame). Chaque instance déduit la position
    /// du véhicule de ces mêmes valeurs et du même trajet (<see cref="TaxiPath"/>) ; aucune position n'est répliquée.
    /// Pendant un trajet avec passagers, chaque instance place localement les joueurs sur des sièges stables, avec le même
    /// modèle que le portage : NetworkTransform et collider suspendus. À l'arrivée, chaque propriétaire réactive son
    /// NetworkTransform et se téléporte à son point de dépose. Les joueurs portés ne sont pas touchés : PlayerCarry les
    /// replace sur leur porteur, donc le portage est conservé.
    /// </summary>
    [DefaultExecutionOrder(150)]
    public class CollectiveTaxi : NetworkBehaviour
    {
        [SerializeField] TaxiTerminal terminalA;
        [SerializeField] TaxiTerminal terminalB;
        [Tooltip("Racine du véhicule greybox : déplacée localement, sans collider.")]
        [SerializeField] Transform vehicle;
        [Tooltip("Sièges (un par joueur, rang par identifiant croissant), enfants du véhicule.")]
        [SerializeField] Transform[] seats;
        [Tooltip("Durée d'un trajet, à vide ou avec passagers (s).")]
        [SerializeField] float tripSeconds = TaxiRules.DefaultTripSeconds;
        [SerializeField] float cornerRadius = 8f;

        // Écrit à chaque transition seulement. epoch : incrémenté à la relance, pour distinguer arrivée et annulation.
        readonly NetworkVariable<int> _epoch = new NetworkVariable<int>(0);
        readonly NetworkVariable<TaxiState> _state = new NetworkVariable<TaxiState>(TaxiState.AtA);
        readonly NetworkVariable<double> _tripStart = new NetworkVariable<double>(-1d);
        readonly NetworkVariable<float> _tripDuration = new NetworkVariable<float>(TaxiRules.DefaultTripSeconds);

        static CollectiveTaxi _current;
        TaxiPath _path;
        bool _riding;
        int _rideEpoch;
        TaxiStop _rideDestination;
        float _nextPlayerRefresh;
        readonly List<PlayerController> _players = new List<PlayerController>(4);
        readonly HashSet<PlayerController> _seated = new HashSet<PlayerController>();
        readonly List<TaxiPassenger> _scratch = new List<TaxiPassenger>(4);
        readonly List<ulong> _scratchIds = new List<ulong>(4);
        readonly List<bool> _scratchInZone = new List<bool>(4);
        HousePhone _phone;
        HouseWallet _wallet;

        public static CollectiveTaxi Current => _current != null && _current.IsSpawned ? _current : null;

        /// <summary>Vrai pendant un trajet avec passagers : lu par le propriétaire pour bloquer déplacement, regard et interactions.</summary>
        public static bool IsRiding => Current != null && TaxiRules.IsCarryingPassengers(_current._state.Value);

        public TaxiState State => _state.Value;
        /// <summary>Véhicule greybox/modèle déplacé localement ; lu par la caméra de trajet (P1-056a).</summary>
        public Transform Vehicle => vehicle;
        /// <summary>Numéro de relance répliqué : change quand un trajet est annulé (P1-056a, lecture seule).</summary>
        public int Epoch => _epoch.Value;
        public float TripSeconds => tripSeconds;

        public float SecondsLeft
        {
            get
            {
                if (TaxiRules.IsParked(_state.Value) || NetworkManager == null)
                    return 0f;
                return Mathf.Max(0f, (float)(_tripStart.Value + _tripDuration.Value - NetworkManager.ServerTime.Time));
            }
        }

        public override void OnNetworkSpawn()
        {
            _current = this;
            _path = new TaxiPath(TaxiPath.DefaultAxis, cornerRadius, 180f, 180f);
            if (IsServer)
            {
                _tripDuration.Value = Mathf.Max(1f, tripSeconds);
                NetworkManager.OnClientDisconnectCallback += OnClientDisconnected;
            }
            PlaceVehicle();
        }

        public override void OnNetworkDespawn()
        {
            if (NetworkManager != null)
                NetworkManager.OnClientDisconnectCallback -= OnClientDisconnected;
            if (_riding)
                EndRide();
            if (_current == this)
                _current = null;
        }

        // ---- Hôte ----

        void Update()
        {
            if (!IsServer || !IsSpawned || NetworkManager.ShutdownInProgress)
                return;
            if (TaxiRules.IsMoving(_state.Value) && NetworkManager.ServerTime.Time >= _tripStart.Value + _tripDuration.Value)
                ServerArrive();
        }

        TaxiTerminal Terminal(TaxiStop stop) => stop == TaxiStop.A ? terminalA : terminalB;

        /// <summary>
        /// Hôte : juge une interaction sur la borne <paramref name="stopIndex"/> (appel ou départ selon l'état) et applique la
        /// transition. La partie propre au demandeur (émetteur, gel, KO, porté, portée maximale) est déjà remplie.
        /// </summary>
        public TaxiOutcome ServerInteract(byte stopIndex, TaxiRequest request, Vector3 origin)
        {
            if (!IsServer)
                return TaxiOutcome.RejectedInvalidSender;

            bool valid = stopIndex <= (byte)TaxiStop.B;
            var stop = valid ? (TaxiStop)stopIndex : TaxiStop.A;
            request.InvalidTarget = !valid || Terminal(stop) == null;
            request.Distance = request.InvalidTarget ? float.PositiveInfinity : Vector3.Distance(origin, Terminal(stop).ClosestPoint(origin));
            if (_phone == null) _phone = FindFirstObjectByType<HousePhone>();
            if (_wallet == null) _wallet = FindFirstObjectByType<HouseWallet>();
            request.PhoneTaken = _phone != null && _phone.IsTaken;
            request.WalletTaken = _wallet != null && _wallet.IsTaken;

            BuildPassengers(request.InvalidTarget ? null : Terminal(stop));
            TaxiOutcome outcome = TaxiRules.Evaluate(_state.Value, stop, request, _scratch);
            if (outcome == TaxiOutcome.Called)
                ServerBegin(TaxiRules.AfterCall(_state.Value, stop));
            else if (outcome == TaxiOutcome.Departed)
                ServerBegin(TaxiRules.AfterDeparture(_state.Value));
            Debug.Log($"[P1-055] host: taxi {_state.Value} stop {stopIndex} -> {outcome}");
            return outcome;
        }

        void BuildPassengers(TaxiTerminal zone)
        {
            _scratch.Clear();
            _scratchIds.Clear();
            _scratchInZone.Clear();
            if (zone == null)
                return;
            foreach (NetworkClient client in NetworkManager.ConnectedClientsList)
            {
                NetworkObject player = client.PlayerObject;
                if (player == null)
                    continue;
                _scratchIds.Add(player.OwnerClientId);
                _scratchInZone.Add(zone.Contains(player.transform.position));
            }

            int index = 0;
            foreach (NetworkClient client in NetworkManager.ConnectedClientsList)
            {
                NetworkObject player = client.PlayerObject;
                if (player == null)
                    continue;
                bool carried = player.TryGetComponent(out PlayerCarry carry) && carry.IsCarried;
                bool knockedOut = carry != null && carry.IsKnockedOut;
                bool carrierInZone = false;
                if (carried && carry.CarrierClientId.HasValue)
                {
                    int carrierIndex = _scratchIds.IndexOf(carry.CarrierClientId.Value);
                    carrierInZone = carrierIndex >= 0 && _scratchInZone[carrierIndex];
                }
                _scratch.Add(new TaxiPassenger { InZone = _scratchInZone[index], KnockedOut = knockedOut, Carried = carried, CarrierInZone = carrierInZone });
                index++;
            }
        }

        void ServerBegin(TaxiState next)
        {
            _tripDuration.Value = Mathf.Max(1f, tripSeconds);
            _tripStart.Value = NetworkManager.ServerTime.Time;
            _state.Value = next;
            Debug.Log($"[P1-055] host: taxi -> {next} at server time {_tripStart.Value:0.00} for {_tripDuration.Value:0.0} s");
        }

        void ServerArrive()
        {
            TaxiState next = TaxiRules.AfterArrival(_state.Value);
            if (next == _state.Value)
                return;
            _state.Value = next;
            Debug.Log($"[P1-055] host: taxi arrived -> {next}");
        }

        /// <summary>Hôte, relance : le taxi revient à A, tout trajet est annulé et personne ne reste attaché.</summary>
        public void ServerResetForRound()
        {
            if (!IsServer)
                return;
            _epoch.Value++;
            _tripStart.Value = -1d;
            _state.Value = TaxiRules.AfterRoundReset();
            Debug.Log("[P1-055] host: taxi reset to A for the new round");
        }

        // Déconnexion pendant un trajet avec passagers : l'échec P1-040 est décidé par NightOutcome ; le passager restant est
        // déposé tout de suite à destination pour ne pas rester bloqué dans l'état taxi.
        void OnClientDisconnected(ulong clientId)
        {
            if (!IsServer || !IsSpawned || NetworkManager.ShutdownInProgress || clientId == NetworkManager.ServerClientId)
                return;
            if (TaxiRules.IsCarryingPassengers(_state.Value))
                ServerArrive();
        }

        // ---- Toutes les instances ----

        void LateUpdate()
        {
            if (!IsSpawned)
                return;

            PlaceVehicle();

            bool riding = TaxiRules.IsCarryingPassengers(_state.Value);
            if (riding && !_riding)
                BeginRide();
            else if (!riding && _riding)
                EndRide();
            if (_riding)
                SeatPlayers();
        }

        void PlaceVehicle()
        {
            if (vehicle == null || _path == null || NetworkManager == null)
                return;
            TaxiState state = _state.Value;
            Vector3 position;
            float yaw;
            if (TaxiRules.IsParked(state))
                _path.Sample(state == TaxiState.AtA ? 0f : 1f, true, out position, out yaw);
            else
            {
                float t = _tripDuration.Value > 0f ? (float)((NetworkManager.ServerTime.Time - _tripStart.Value) / _tripDuration.Value) : 1f;
                _path.Sample(Mathf.Clamp01(t), TaxiRules.Stop(state) == TaxiStop.B, out position, out yaw);
            }
            vehicle.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));
        }

        void RefreshPlayers()
        {
            _players.Clear();
            foreach (PlayerController p in FindObjectsByType<PlayerController>(FindObjectsSortMode.None))
                if (p.IsSpawned)
                    _players.Add(p);
            _players.Sort((a, b) => a.OwnerClientId.CompareTo(b.OwnerClientId));
            _nextPlayerRefresh = Time.unscaledTime + 0.5f;
        }

        void BeginRide()
        {
            _riding = true;
            _rideEpoch = _epoch.Value;
            _rideDestination = TaxiRules.Stop(_state.Value);
            RefreshPlayers();
        }

        void SeatPlayers()
        {
            if (Time.unscaledTime >= _nextPlayerRefresh)
                RefreshPlayers();
            for (int i = 0; i < _players.Count; i++)
            {
                PlayerController player = _players[i];
                if (player == null)
                    continue;
                // Un partenaire porté suit son porteur (PlayerCarry) : il reste porté dans le taxi et à l'arrivée.
                if (player.TryGetComponent(out PlayerCarry carry) && carry.IsCarried)
                    continue;
                SuspendSync(player);
                _seated.Add(player);
                Transform seat = seats[i % seats.Length];
                player.transform.SetPositionAndRotation(seat.position, Quaternion.Euler(0f, vehicle.eulerAngles.y, 0f));
            }
        }

        static void SuspendSync(PlayerController player)
        {
            if (player.TryGetComponent(out NetworkTransform nt) && nt.enabled)
                nt.enabled = false;
            if (player.TryGetComponent(out Collider col) && col.enabled)
                col.enabled = false;
        }

        // Fin de trajet : arrivée normale (dépose sur le quai de destination) ou annulation par la relance (poses de départ).
        void EndRide()
        {
            _riding = false;
            bool cancelled = _epoch.Value != _rideEpoch;
            TaxiTerminal destination = Terminal(_rideDestination);
            RefreshPlayers();
            foreach (PlayerController player in _seated)
            {
                if (player == null || !player.IsSpawned)
                    continue;
                Vector3 position;
                Quaternion rotation;
                if (cancelled || destination == null)
                {
                    position = RestartRules.StartPosition(player.Slot);
                    rotation = Quaternion.Euler(0f, RestartRules.StartYaw, 0f);
                }
                else
                    destination.DropPose(_players.IndexOf(player), out position, out rotation);

                player.transform.SetPositionAndRotation(position, rotation);
                if (player.TryGetComponent(out Collider col))
                    col.enabled = true;
                if (player.TryGetComponent(out NetworkTransform nt))
                {
                    nt.enabled = true;
                    if (player.IsOwner)
                        nt.Teleport(position, rotation, player.transform.localScale);
                }
            }
            _seated.Clear();
        }
    }
}

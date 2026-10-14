using System;
using System.Threading.Tasks;
using LastCallCrew.Connection;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;

namespace LastCallCrew.Network
{
    /// <summary>
    /// Connexion en ligne par code (P1-003) : authentification anonyme, session Unity Multiplayer Services sur Relay en DTLS,
    /// puis démarrage de NGO par la session sur le NetworkManager existant. Relay ne fait que transporter : l'hôte garde
    /// l'autorité. Responsabilité courte : initialiser, héberger, rejoindre, arrêter, exposer un état et un message d'erreur.
    /// </summary>
    public class RelayConnection : MonoBehaviour
    {
        // Hôte + un client (P1 : deux joueurs).
        const int MaxPlayers = 2;

        ISession _session;
        ConnectionPhase _phase = ConnectionPhase.Idle;

        public ConnectionPhase Phase => _phase;
        /// <summary>Code de session de l'hôte (vide hors session).</summary>
        public string JoinCode { get; private set; } = string.Empty;
        /// <summary>Dernier message d'erreur lisible (vide s'il n'y en a pas) ; effacé à la prochaine tentative.</summary>
        public string ErrorMessage { get; private set; } = string.Empty;

        // P1-040 : si NGO s'arrête côté client sans que ce joueur l'ait demandé (hôte parti, coupure), retour hors ligne lisible.
        void Start()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientStopped += OnNgoClientStopped;
                NetworkManager.Singleton.OnClientDisconnectCallback += OnNgoClientDisconnected;
            }
        }

        void OnDisable() => DetachSession();

        void OnDestroy()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientStopped -= OnNgoClientStopped;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnNgoClientDisconnected;
            }
            DetachSession();
            _session = null;
        }

        void OnNgoClientStopped(bool wasHost)
        {
            if (wasHost || _phase != ConnectionPhase.Joined)
                return;
            ISession session = _session;
            DetachSession();
            ResetLocalState();
            ErrorMessage = HostGoneMessage;
            Debug.Log("[P1-040] client: NGO stopped without a local request (host gone)");
            if (session != null)
                _ = LeaveQuietly(session);
        }

        // P1-040 : un client disparu sans avoir quitté proprement (processus tué, coupure) garde sa place dans la session du
        // service et ne pourrait plus la rejoindre avec la même identité. L'hôte libère donc les places autres que la sienne
        // (P1 = deux joueurs : tout autre membre est le partenaire parti).
        void OnNgoClientDisconnected(ulong clientId)
        {
            NetworkManager nm = NetworkManager.Singleton;
            if (_phase != ConnectionPhase.Hosting || _session == null || nm == null || nm.ShutdownInProgress || clientId == NetworkManager.ServerClientId)
                return;
            _ = ReleaseDepartedSeats(_session);
        }

        static async Task ReleaseDepartedSeats(ISession session)
        {
            try
            {
                string me = session.CurrentPlayer.Id;
                foreach (IReadOnlyPlayer player in new System.Collections.Generic.List<IReadOnlyPlayer>(session.Players))
                {
                    if (player.Id == me)
                        continue;
                    await session.AsHost().RemovePlayerAsync(player.Id);
                    Debug.Log("[P1-040] host: released the session seat of a departed player");
                }
            }
            catch (Exception e)
            {
                Debug.Log($"[P1-040] host: seat release skipped ({e.GetType().Name}: {e.Message})");
            }
        }

        static async Task LeaveQuietly(ISession session)
        {
            try { await session.LeaveAsync(); }
            catch (Exception e) { Debug.Log($"[P1-040] leave after host loss: {e.GetType().Name}"); }
        }

        const string HostGoneMessage = "HÔTE DÉCONNECTÉ : la session est terminée. Vous pouvez héberger ou rejoindre une nouvelle partie.";

        public async void BeginHost()
        {
            if (!RelayJoinRules.CanBegin(_phase))
                return;
            await HostAsync();
        }

        public async void BeginJoin(string rawCode)
        {
            if (!RelayJoinRules.CanBegin(_phase))
                return;
            await JoinAsync(rawCode);
        }

        public async void BeginStop()
        {
            if (!RelayJoinRules.CanStop(_phase))
                return;
            await StopAsync();
        }

        async Task HostAsync()
        {
            _phase = ConnectionPhase.Connecting;
            ErrorMessage = string.Empty;
            try
            {
                await EnsureSignedInAsync();
                SessionOptions options = new SessionOptions { MaxPlayers = MaxPlayers }
                    .WithRelayNetwork()
                    .WithNetworkOptions(new NetworkOptions { RelayProtocol = RelayProtocol.DTLS });
                _session = await MultiplayerService.Instance.CreateSessionAsync(options);
                AttachSession();
                JoinCode = _session.Code;
                _phase = ConnectionPhase.Hosting;
                Debug.Log($"[P1-003] host: Relay session created, code {JoinCode}, protocol DTLS, NGO host={NetworkManager.Singleton.IsHost}");
            }
            catch (Exception e)
            {
                Fail("héberger", e);
            }
        }

        async Task JoinAsync(string rawCode)
        {
            string code = RelayJoinRules.Normalize(rawCode);
            if (!RelayJoinRules.IsPlausibleCode(code))
            {
                ErrorMessage = "Code invalide : lettres et chiffres uniquement.";
                return;
            }

            _phase = ConnectionPhase.Connecting;
            ErrorMessage = string.Empty;
            try
            {
                await EnsureSignedInAsync();
                _session = await MultiplayerService.Instance.JoinSessionByCodeAsync(code);
                AttachSession();
                JoinCode = _session.Code;
                _phase = ConnectionPhase.Joined;
                Debug.Log($"[P1-003] client: joined Relay session {JoinCode}, NGO client={NetworkManager.Singleton.IsClient}");
            }
            catch (Exception e)
            {
                Fail("rejoindre", e);
            }
        }

        async Task StopAsync()
        {
            _phase = ConnectionPhase.Stopping;
            ISession session = _session;
            DetachSession();
            try
            {
                if (session != null)
                {
                    if (session.IsHost)
                        await session.AsHost().DeleteAsync();
                    else
                        await session.LeaveAsync();
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[P1-003] stop: session cleanup reported {e.GetType().Name}: {e.Message}");
            }
            finally
            {
                ResetLocalState();
                Debug.Log("[P1-003] session stopped, NGO listening=" + (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening));
            }
        }

        // Authentification Unity anonyme : un seul passage, idempotent. Le jeton n'est jamais journalisé.
        static async Task EnsureSignedInAsync()
        {
            while (UnityServices.State == ServicesInitializationState.Initializing)
                await Task.Yield();
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
            {
                var options = new InitializationOptions();
                string profile = ProfileName();
                if (!string.IsNullOrEmpty(profile))
                    options.SetProfile(profile);
                await UnityServices.InitializeAsync(options);
            }
            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        // Deux instances d'un même ordinateur (Multiplayer Play Mode, deux builds) partageraient sinon la même identité
        // anonyme : un profil distinct par instance. `-lcc-profile nom` le force ; dans l'éditeur il dérive du dossier du projet.
        static string ProfileName()
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
                if (args[i] == "-lcc-profile")
                    return args[i + 1];
#if UNITY_EDITOR
            return "lcc" + Application.dataPath.GetHashCode().ToString("x8");
#else
            return null;
#endif
        }

        void AttachSession()
        {
            if (_session == null)
                return;
            _session.Deleted += OnSessionEnded;
            _session.RemovedFromSession += OnSessionEnded;
        }

        void DetachSession()
        {
            if (_session == null)
                return;
            _session.Deleted -= OnSessionEnded;
            _session.RemovedFromSession -= OnSessionEnded;
        }

        // L'hôte a fermé la session ou ce joueur en a été retiré : retour à l'état hors ligne sans redémarrer le jeu.
        void OnSessionEnded()
        {
            if (_phase != ConnectionPhase.Hosting && _phase != ConnectionPhase.Joined)
                return;
            bool wasClient = _phase == ConnectionPhase.Joined;
            DetachSession();
            ResetLocalState();
            ErrorMessage = wasClient ? HostGoneMessage : "Session terminée.";
            Debug.Log("[P1-003] session ended by the service or the host");
        }

        void Fail(string action, Exception e)
        {
            Debug.LogWarning($"[P1-003] could not {action}: {e.GetType().Name}: {e.Message}");
            DetachSession();
            ErrorMessage = Describe(action, e);
            ResetLocalState();
        }

        void ResetLocalState()
        {
            _session = null;
            JoinCode = string.Empty;
            _phase = ConnectionPhase.Idle;
            NetworkManager nm = NetworkManager.Singleton;
            if (nm != null && nm.IsListening)
                nm.Shutdown();
        }

        static string Describe(string action, Exception e)
        {
            if (e is SessionException se)
            {
                switch (se.Error)
                {
                    case SessionError.SessionNotFound:
                        return "Code introuvable ou session fermée. Vérifiez le code puis réessayez.";
                    case SessionError.RateLimitExceeded:
                        return "Trop de tentatives. Patientez quelques secondes puis réessayez.";
                    case SessionError.InvalidParameter:
                        return "Code refusé par le service. Vérifiez le code puis réessayez.";
                    case SessionError.NotAuthorized:
                    case SessionError.Forbidden:
                        return "Connexion refusée par le service.";
                }
                // Ancienne connexion du même joueur encore enregistrée (processus tué) : le service la libère après la détection.
                if (se.Message != null && se.Message.IndexOf("already a member", StringComparison.OrdinalIgnoreCase) >= 0)
                    return "Votre ancienne connexion est encore enregistrée par le service. Patientez environ une minute puis réessayez.";
                // Session pleine (P1-040) : le service répond « session is full » ; P1 reste limité à deux joueurs.
                if (se.Message != null && se.Message.IndexOf("full", StringComparison.OrdinalIgnoreCase) >= 0)
                    return "SESSION COMPLÈTE : la partie a déjà deux joueurs. Réessayez avec un autre code.";
                // Autres refus de session (ex. caractère absent de l'alphabet des codes) : le code ne peut pas exister.
                if (action == "rejoindre")
                    return "Code invalide ou session indisponible. Vérifiez le code puis réessayez.";
            }
            if (e is AuthenticationException)
                return "Authentification Unity impossible. Vérifiez la connexion Internet puis réessayez.";
            if (e is RequestFailedException)
                return $"Service Unity indisponible : impossible de {action}. Réessayez.";
            return $"Impossible de {action} : {e.Message}";
        }
    }
}

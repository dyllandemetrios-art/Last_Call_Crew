using LastCallCrew.Drink;
using LastCallCrew.Intoxication;
using LastCallCrew.Night;
using Unity.Netcode;
using UnityEngine;

namespace LastCallCrew.Network
{
    /// <summary>
    /// HUD de debug commun (P1-013a) : panneau Ivresse en haut à droite, invite et feedback en bas au centre.
    /// Les mêmes valeurs répliquées sont affichées sur chaque instance ; seule la mention VOUS / AUTRE change.
    /// </summary>
    public class IntoxicationHud : MonoBehaviour
    {
        const float FeedbackSeconds = 2f;

        readonly DebugGuiKit _kit = new DebugGuiKit();
        PlayerIntoxication[] _players = System.Array.Empty<PlayerIntoxication>();
        PlayerKnockout[] _knockouts = System.Array.Empty<PlayerKnockout>();
        PlayerCarry[] _carries = System.Array.Empty<PlayerCarry>();
        PlayerWake[] _wakes = System.Array.Empty<PlayerWake>();
        PlayerReturn[] _returns = System.Array.Empty<PlayerReturn>();
        int[] _seenReturned = System.Array.Empty<int>();
        float _returnFeedbackUntil;
        // P1-050 : -1 = pas encore observé (un nouvel arrivant ne déclenche pas le feedback), puis 0 / 1.
        int _seenReturnActive = -1;
        HouseKeys _keys;
        HouseWallet _wallet;
        HousePhone _phone;
        int _seenKeysTaken = -1;
        int _seenWalletTaken = -1;
        int _seenPhoneTaken = -1;
        int _seenReady = -1;
        string _returnFeedbackText;
        int[] _seenWakeCounts = System.Array.Empty<int>();
        DrinkOffer _offer;
        NightTimer _night;
        NightOutcome _outcome;
        NightRound _round;
        NpcDialogue _npc;
        int _seenRound = -1;
        float _nextRefresh;
        int _seenDecisionCount = -1;
        float _decisionFeedbackUntil;
        float _wakeFeedbackUntil;
        string _wakeFeedbackText;

        void OnDestroy() => _kit.Destroy();

        void OnGUI()
        {
            var nm = NetworkManager.Singleton;
            if (nm == null || !nm.IsListening)
                return;

            _kit.Ensure();
            Refresh();
            ResetPresentationOnNewRound();

            PlayerKnockout localKnockout = nm.LocalClient?.PlayerObject != null
                ? nm.LocalClient.PlayerObject.GetComponent<PlayerKnockout>()
                : null;
            bool localKnockedOut = localKnockout != null && localKnockout.IsKnockedOut;
            PlayerCarry localCarry = nm.LocalClient?.PlayerObject != null
                ? nm.LocalClient.PlayerObject.GetComponent<PlayerCarry>()
                : null;

            // Voile léger d'abord, pour que les panneaux (dont celui du partenaire) restent par-dessus et lisibles.
            if (localKnockedOut)
                _kit.Panel(new Rect(0f, 0f, Screen.width, Screen.height), new Color(0f, 0f, 0f, 0.42f));

            DrawIntoxicationPanel();
            DrawNightTimer();
            _koBottom = 0f;
            DrawInteractionFeedback(nm, localCarry);
            if (localCarry != null && localCarry.IsCarrying)
                DrawCarryBanner(localCarry);
            if (localKnockedOut)
                DrawKnockoutMessage(localCarry, nm.LocalClient.PlayerObject.GetComponent<PlayerWake>());
            // Après le message KO : le bandeau de résultat se place dessous au lieu de passer derrière.
            DrawNightOutcome();
            DrawKeysStatus();
        }

        // P1-033 : à chaque changement de manche répliqué, les feedbacks temporaires et les compteurs observés
        // repartent de zéro, une seule fois par instance.
        void ResetPresentationOnNewRound()
        {
            if (_round == null || !_round.IsSpawned || _round.Round == _seenRound)
                return;
            bool first = _seenRound < 0;
            _seenRound = _round.Round;
            if (first)
                return;
            for (int i = 0; i < _seenWakeCounts.Length; i++) _seenWakeCounts[i] = -1;
            for (int i = 0; i < _seenReturned.Length; i++) _seenReturned[i] = -1;
            _seenDecisionCount = -1;
            _seenReturnActive = -1;
            _seenKeysTaken = -1;
            _seenWalletTaken = -1;
            _seenPhoneTaken = -1;
            _seenReady = -1;
            _wakeFeedbackText = null;
            _returnFeedbackText = null;
            _wakeFeedbackUntil = 0f;
            _returnFeedbackUntil = 0f;
            _decisionFeedbackUntil = 0f;
        }

        void Refresh()
        {
            if (Time.unscaledTime < _nextRefresh)
                return;
            _nextRefresh = Time.unscaledTime + 0.5f;
            _players = FindObjectsByType<PlayerIntoxication>(FindObjectsSortMode.None);
            System.Array.Sort(_players, (a, b) => a.OwnerClientId.CompareTo(b.OwnerClientId));
            if (_knockouts.Length != _players.Length)
            {
                _knockouts = new PlayerKnockout[_players.Length];
                _carries = new PlayerCarry[_players.Length];
                _wakes = new PlayerWake[_players.Length];
                _returns = new PlayerReturn[_players.Length];
                _seenReturned = new int[_players.Length];
                for (int i = 0; i < _seenReturned.Length; i++)
                    _seenReturned[i] = -1; // -1 : pas encore observé, donc pas de feedback au rattrapage d'un nouveau client
                _seenWakeCounts = new int[_players.Length];
                for (int i = 0; i < _seenWakeCounts.Length; i++)
                    _seenWakeCounts[i] = -1; // -1 : pas encore observé, donc pas de feedback au rattrapage d'un nouveau client
            }
            for (int i = 0; i < _players.Length; i++)
            {
                _knockouts[i] = _players[i] != null ? _players[i].GetComponent<PlayerKnockout>() : null;
                _carries[i] = _players[i] != null ? _players[i].GetComponent<PlayerCarry>() : null;
                _wakes[i] = _players[i] != null ? _players[i].GetComponent<PlayerWake>() : null;
                _returns[i] = _players[i] != null ? _players[i].GetComponent<PlayerReturn>() : null;
            }
            if (_offer == null)
                _offer = FindFirstObjectByType<DrinkOffer>();
            if (_night == null)
                _night = FindFirstObjectByType<NightTimer>();
            if (_outcome == null)
                _outcome = FindFirstObjectByType<NightOutcome>();
            if (_round == null)
                _round = FindFirstObjectByType<NightRound>();
            if (_npc == null)
                _npc = FindFirstObjectByType<NpcDialogue>();
        }

        // ---- Panneau Ivresse (haut à droite) ----

        void DrawIntoxicationPanel()
        {
            float u(float v) => _kit.U(v);
            float width = u(480f);
            float cardHeight = u(110f);
            float x = Screen.width - width - u(16f);
            float y = u(16f);

            int shown = 0;
            foreach (PlayerIntoxication p in _players)
                if (p != null && p.IsSpawned)
                    shown++;

            float height = u(46f) + shown * (cardHeight + u(8f)) + u(4f);
            _kit.Panel(new Rect(x, y, width, height), DebugGuiKit.PanelColor);
            DebugGuiKit.Label(new Rect(x + u(14f), y + u(4f), width - u(28f), u(40f)), "IVRESSE", _kit.Title, new Color(0.7f, 0.82f, 1f));

            float cy = y + u(46f);
            for (int i = 0; i < _players.Length; i++)
            {
                PlayerIntoxication p = _players[i];
                if (p == null || !p.IsSpawned)
                    continue;
                bool knockedOut = _knockouts[i] != null && _knockouts[i].IsKnockedOut;
                bool carried = _carries[i] != null && _carries[i].IsCarried;
                bool returned = _returns[i] != null && _returns[i].IsReturned;
                DrawPlayerCard(p, knockedOut, carried, returned, new Rect(x + u(10f), cy, width - u(20f), cardHeight));
                cy += cardHeight + u(8f);
            }
        }

        void DrawPlayerCard(PlayerIntoxication player, bool knockedOut, bool carried, bool returned, Rect card)
        {
            float u(float v) => _kit.U(v);
            IntoxicationStage stage = player.Stage;
            Color stageColor = StageColor(stage);

            _kit.Panel(card, player.IsOwner ? DebugGuiKit.PanelHighlightColor : new Color(1f, 1f, 1f, 0.07f));

            string who = player.IsOwner ? "VOUS" : "AUTRE";
            float nameWidth = card.width * 0.54f - u(12f);
            DebugGuiKit.Label(new Rect(card.x + u(12f), card.y + u(6f), nameWidth, u(40f)),
                $"{who} · Joueur {player.OwnerClientId}", _kit.Body, player.IsOwner ? new Color(0.65f, 0.9f, 1f) : Color.white);
            DebugGuiKit.Label(new Rect(card.x + card.width * 0.54f, card.y + u(4f), card.width * 0.46f - u(12f), u(44f)),
                $"{Mathf.RoundToInt(player.Value)} / {Mathf.RoundToInt(player.MaxValue)}", _kit.Value, Color.white);

            string stageText = knockedOut ? (carried ? "KO · PORTÉ" : "KO · À TERRE") : IntoxicationRules.Label(stage).ToUpperInvariant();
            DebugGuiKit.Label(new Rect(card.x + u(12f), card.y + u(48f), card.width - u(24f), u(30f)),
                stageText, _kit.Body, stageColor);
            // État de retour (P1-031) : à droite de la ligne de palier.
            DebugGuiKit.Label(new Rect(card.x + card.width * 0.5f, card.y + u(48f), card.width * 0.5f - u(12f), u(30f)),
                returned ? "RENTRÉ" : (_outcome != null && _outcome.IsSpawned && !_outcome.ReturnActive ? "MAISON" : "ENCORE DEHORS"), _kit.Tag,
                returned ? new Color(0.45f, 0.95f, 0.6f) : new Color(1f, 1f, 1f, 0.6f));
            _kit.Bar(new Rect(card.x + u(12f), card.y + u(82f), card.width - u(24f), u(18f)), player.Value / player.MaxValue, stageColor);
        }

        static Color StageColor(IntoxicationStage stage)
        {
            switch (stage)
            {
                case IntoxicationStage.Pompette: return new Color(0.98f, 0.82f, 0.25f);
                case IntoxicationStage.Ivre: return new Color(0.98f, 0.5f, 0.2f);
                case IntoxicationStage.KoThreshold: return new Color(0.95f, 0.22f, 0.28f);
                default: return new Color(0.4f, 0.85f, 0.5f);
            }
        }

        // ---- Message KO du joueur local (centre) ----

        void DrawKnockoutMessage(PlayerCarry localCarry, PlayerWake localWake)
        {
            float u(float v) => _kit.U(v);
            // Centré et assez étroit pour ne jamais recouvrir le panneau Ivresse (droite) ni le panneau réseau (gauche).
            float width = Mathf.Min(u(900f), Screen.width - 2f * u(480f + 32f));
            ulong? carrier = localCarry != null ? localCarry.CarrierClientId : null;
            // Pas de compte à rebours de réveil pendant le bilan : le réveil automatique est gelé.
            bool showWake = localWake != null && !NightOutcome.IsFrozen;
            // Placé dans le tiers haut pour laisser le corps couché, cadré par la caméra de KO, visible au centre (P1-020a).
            float height = u(120f) + (carrier.HasValue ? u(56f) : 0f) + (showWake ? u(56f) : 0f);
            var rect = new Rect((Screen.width - width) * 0.5f, Mathf.Max(Screen.height * 0.1f, _timerBottom + _kit.U(12f)), width, height);
            _koBottom = rect.yMax;
            _kit.Panel(rect, new Color(0.05f, 0.02f, 0.03f, 0.88f));
            DebugGuiKit.Label(new Rect(rect.x, rect.y, rect.width, u(120f)), "VOUS ÊTES KO", _kit.Display, new Color(1f, 0.35f, 0.4f));
            float y = rect.y + u(118f);
            if (carrier.HasValue)
            {
                DebugGuiKit.Label(new Rect(rect.x, y, rect.width, u(50f)), $"PORTÉ PAR JOUEUR {carrier.Value}", _kit.Prompt, new Color(0.65f, 0.9f, 1f));
                y += u(56f);
            }
            if (showWake)
                DebugGuiKit.Label(new Rect(rect.x, y, rect.width, u(50f)),
                    $"RÉVEIL AUTOMATIQUE DANS {Mathf.CeilToInt(localWake.AutoWakeRemaining)} s", _kit.Prompt, new Color(1f, 0.85f, 0.4f));
        }

        // ---- Chronomètre de nuit (carte centrée en haut) ----

        const float TimerCardWidth = 560f;
        const float TimerCardHeight = 156f;
        // Seuils d'affichage seulement (aucune règle) : orange puis rouge.
        const float UrgentSeconds = 10f;
        const float CriticalSeconds = 5f;

        // Bas de la carte NUIT (pixels écran), pour que les messages centraux se placent dessous.
        float _timerBottom;
        // Bas du message KO du joueur local (0 s'il n'est pas KO), pour placer le bandeau de résultat dessous.
        float _koBottom;

        void DrawNightTimer()
        {
            float u(float v) => _kit.U(v);
            _timerBottom = 0f;
            if (_night == null || !_night.IsSpawned)
                return;

            bool waiting = _night.State == NightState.Waiting;
            bool expired = _night.IsExpired;
            float remaining = _night.RemainingSeconds;
            float fraction = waiting ? 1f : expired ? 0f : Mathf.Clamp01(remaining / _night.DurationSeconds);

            string text;
            GUIStyle style = _kit.Display;
            Color color;
            Color fill;
            Color background = DebugGuiKit.PanelColor;
            if (waiting)
            {
                text = "EN ATTENTE DU PARTENAIRE";
                style = _kit.Prompt;
                color = new Color(1f, 0.85f, 0.4f);
                fill = new Color(0.5f, 0.45f, 0.25f);
            }
            else if (expired)
            {
                text = "TEMPS ÉCOULÉ";
                style = _kit.Banner;
                color = new Color(1f, 0.35f, 0.4f);
                fill = color;
                background = new Color(0.16f, 0.04f, 0.06f, 0.9f);
            }
            else
            {
                text = NightTimerRules.Format(remaining);
                if (remaining <= CriticalSeconds) color = new Color(1f, 0.3f, 0.35f);
                else if (remaining <= UrgentSeconds) color = new Color(1f, 0.6f, 0.3f);
                else color = Color.white;
                fill = remaining <= UrgentSeconds ? color : new Color(0.45f, 0.7f, 1f);
            }

            float width = Mathf.Min(u(TimerCardWidth), Screen.width * 0.42f);
            var card = new Rect((Screen.width - width) * 0.5f, u(16f), width, u(TimerCardHeight));
            _timerBottom = card.yMax;
            _kit.Panel(card, background);
            DebugGuiKit.Label(new Rect(card.x + u(14f), card.y + u(4f), card.width - u(28f), u(40f)), "NUIT", _kit.Title, new Color(0.7f, 0.82f, 1f));
            // P1-050 : tant que le retour n'est pas activé, la maison ne peut pas faire gagner la nuit.
            if (!waiting && !expired && _outcome != null && _outcome.IsSpawned && _outcome.Result == NightResult.Pending && !_outcome.ReturnActive)
                DebugGuiKit.Label(new Rect(card.x + card.width * 0.3f, card.y + u(4f), card.width * 0.7f - u(14f), u(30f)),
                    "MAISON · QUITTEZ LES LIEUX", _kit.Tag, new Color(1f, 0.85f, 0.4f));
            DebugGuiKit.Label(new Rect(card.x, card.y + u(38f), card.width, u(80f)), text, style, color);
            _kit.Bar(new Rect(card.x + u(14f), card.y + u(TimerCardHeight - 32f), card.width - u(28f), u(18f)), fraction, fill);
        }

        // P1-051/P1-052/P1-053 : checklist compacte de départ (clés, portefeuille, téléphone), à gauche, sans nouveau grand
        // panneau. Chaque état est lu séparément depuis son propre état répliqué ; « DÉPART · PRÊT » quand les trois sont pris.
        void DrawKeysStatus()
        {
            if (_keys == null) _keys = FindFirstObjectByType<HouseKeys>();
            if (_wallet == null) _wallet = FindFirstObjectByType<HouseWallet>();
            if (_phone == null) _phone = FindFirstObjectByType<HousePhone>();
            bool hasKeys = _keys != null && _keys.IsSpawned;
            bool hasWallet = _wallet != null && _wallet.IsSpawned;
            bool hasPhone = _phone != null && _phone.IsSpawned;
            int lines = (hasKeys ? 1 : 0) + (hasWallet ? 1 : 0) + (hasPhone ? 1 : 0);
            if (lines == 0)
                return;
            float u(float v) => _kit.U(v);
            bool ready = hasKeys && hasWallet && hasPhone && DepartureChecklist.AllTaken(_keys.IsTaken, _wallet.IsTaken, _phone.IsTaken);
            float lineHeight = u(30f);
            float height = lineHeight * (lines + (ready ? 1 : 0)) + u(10f);
            // Milieu gauche : libre à 960 × 540 comme à 1280 × 720 (ni invites du bas, ni panneaux du haut) ; assez large pour « PORTEFEUILLE · À PRENDRE ».
            var rect = new Rect(u(16f), Screen.height * 0.42f, u(380f), height);
            _kit.Panel(rect, DebugGuiKit.PanelColor);
            float y = rect.y + u(5f);
            Color done = new Color(0.45f, 0.95f, 0.6f);
            if (hasKeys)
            {
                DebugGuiKit.Label(new Rect(rect.x + u(10f), y, rect.width - u(20f), lineHeight), KeyRules.StatusLabel(_keys.IsTaken), _kit.Tag, _keys.IsTaken ? done : new Color(1f, 0.9f, 0.4f));
                y += lineHeight;
            }
            if (hasWallet)
            {
                DebugGuiKit.Label(new Rect(rect.x + u(10f), y, rect.width - u(20f), lineHeight), WalletRules.StatusLabel(_wallet.IsTaken), _kit.Tag, _wallet.IsTaken ? done : new Color(0.6f, 0.8f, 1f));
                y += lineHeight;
            }
            if (hasPhone)
            {
                DebugGuiKit.Label(new Rect(rect.x + u(10f), y, rect.width - u(20f), lineHeight), PhoneRules.StatusLabel(_phone.IsTaken), _kit.Tag, _phone.IsTaken ? done : new Color(0.85f, 0.88f, 1f));
                y += lineHeight;
            }
            if (ready)
                DebugGuiKit.Label(new Rect(rect.x + u(10f), y, rect.width - u(20f), lineHeight), DepartureChecklist.ReadyStatus, _kit.Tag, done);
        }

        // ---- Bilan de la nuit (carte centrale, sous le message KO) ----

        void DrawNightOutcome()
        {
            if (_outcome == null || !_outcome.IsSpawned || _outcome.Result == NightResult.Pending)
                return;
            float u(float v) => _kit.U(v);
            bool victory = _outcome.Result == NightResult.Victory;
            bool isHost = NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer;

            int rows = 0;
            foreach (PlayerIntoxication p in _players)
                if (p != null && p.IsSpawned)
                    rows++;

            // Pleine largeur utile : placée sous les panneaux latéraux (y >= 46 % de l'écran), elle ne les recouvre pas.
            float width = Mathf.Min(u(1000f), Screen.width - u(32f));
            float height = u(96f) + rows * u(40f) + u(104f);
            var rect = new Rect((Screen.width - width) * 0.5f, Mathf.Max(Screen.height * 0.46f, _koBottom + u(12f)), width, height);
            Color accent = victory ? new Color(0.5f, 1f, 0.6f) : new Color(1f, 0.4f, 0.45f);
            _kit.Panel(rect, victory ? new Color(0.04f, 0.16f, 0.08f, 0.94f) : new Color(0.18f, 0.04f, 0.06f, 0.94f));
            DebugGuiKit.Label(new Rect(rect.x, rect.y, rect.width, u(60f)), NightOutcomeRules.Banner(_outcome.Result, _outcome.Reason), _kit.Prompt, accent);

            string remaining = _night != null ? NightTimerRules.Format(_night.RemainingSeconds) : "00:00";
            DebugGuiKit.Label(new Rect(rect.x, rect.y + u(52f), rect.width, u(40f)), "TEMPS RESTANT " + remaining, _kit.Prompt, new Color(0.8f, 0.88f, 1f));

            float y = rect.y + u(96f);
            for (int i = 0; i < _players.Length; i++)
            {
                PlayerIntoxication p = _players[i];
                if (p == null || !p.IsSpawned)
                    continue;
                bool ko = _knockouts[i] != null && _knockouts[i].IsKnockedOut;
                bool returned = _returns[i] != null && _returns[i].IsReturned;
                string who = (p.IsOwner ? "VOUS" : "AUTRE") + " · Joueur " + p.OwnerClientId;
                DebugGuiKit.Label(new Rect(rect.x + u(16f), y, rect.width * 0.34f, u(36f)), who, _kit.Body, p.IsOwner ? new Color(0.65f, 0.9f, 1f) : Color.white);
                DebugGuiKit.Label(new Rect(rect.x + rect.width * 0.34f, y, rect.width * 0.38f, u(36f)), RestartRules.FinalState(returned, ko), _kit.Body,
                    returned ? new Color(0.45f, 0.95f, 0.6f) : new Color(1f, 0.55f, 0.5f));
                DebugGuiKit.Label(new Rect(rect.x + rect.width * 0.72f, y, rect.width * 0.28f - u(16f), u(36f)), "IVRESSE " + Mathf.RoundToInt(p.Value), _kit.Body, Color.white);
                y += u(40f);
            }

            float bottom = rect.yMax - u(16f);
            if (isHost && _round != null)
            {
                var button = new Rect(rect.x + rect.width * 0.5f - u(260f), bottom - u(56f), u(520f), u(56f));
                // P1-040 : la relance attend le retour du partenaire (l'hôte la refuserait de toute façon).
                bool enough = NetworkManager.Singleton.ConnectedClientsIds.Count >= RestartRules.PlayersRequired;
                GUI.enabled = enough;
                if (GUI.Button(button, enough ? "RELANCER" : "ATTENTE DU PARTENAIRE", _kit.Button))
                    _round.RequestRestartRpc();
                GUI.enabled = true;
            }
            else
            {
                DebugGuiKit.Label(new Rect(rect.x, bottom - u(56f), rect.width, u(56f)), "EN ATTENTE DE L'HÔTE", _kit.Prompt, new Color(1f, 0.85f, 0.4f));
            }
        }

        // ---- Bandeau du porteur (haut au centre, sous la carte NUIT) ----

        void DrawCarryBanner(PlayerCarry localCarry)
        {
            float u(float v) => _kit.U(v);
            ulong? carried = localCarry.CarriedPlayerClientId;
            float width = Mathf.Min(u(640f), Screen.width * 0.34f);
            var rect = new Rect((Screen.width - width) * 0.5f, Mathf.Max(u(16f), _timerBottom + u(8f)), width, u(56f));
            _kit.Panel(rect, DebugGuiKit.PanelColor);
            DebugGuiKit.Label(rect, carried.HasValue ? $"VOUS PORTEZ JOUEUR {carried.Value}" : "VOUS PORTEZ UN JOUEUR", _kit.Prompt, new Color(0.65f, 0.9f, 1f));
        }

        // ---- Invite, progression du refus et feedback (bas au centre) ----

        void DrawInteractionFeedback(NetworkManager nm, PlayerCarry localCarry)
        {
            float u(float v) => _kit.U(v);
            PlayerInteractor local = nm.LocalClient?.PlayerObject != null
                ? nm.LocalClient.PlayerObject.GetComponent<PlayerInteractor>()
                : null;
            if (local == null)
                return;

            float centerX = Screen.width * 0.5f;
            float bottom = Screen.height - u(24f);

            // Réticule discret.
            DebugGuiKit.Label(new Rect(centerX - u(20f), Screen.height * 0.5f - u(20f), u(40f), u(40f)), "+", _kit.Prompt, new Color(1f, 1f, 1f, 0.85f));

            // Zone du bas : feedback (au-dessus) puis invite ou barre de refus.
            float promptWidth = Mathf.Min(u(1120f), Screen.width - u(40f));
            float promptHeight = u(76f);
            var promptRect = new Rect(centerX - promptWidth * 0.5f, bottom - promptHeight, promptWidth, promptHeight);

            float feedbackBottom = promptRect.y;
            // P1-033 : plus d'invite d'action pendant le bilan (le gameplay est gelé).
            if (NightOutcome.IsFrozen)
                return;
            if (local.IsWaking)
            {
                // Grande barre de réveil : plus haute que celle du refus pour être lue d'un coup d'œil.
                var wakeRect = new Rect(promptRect.x, promptRect.y - u(30f), promptRect.width, promptHeight + u(30f));
                _kit.Panel(wakeRect, DebugGuiKit.PanelColor);
                DebugGuiKit.Label(new Rect(wakeRect.x, wakeRect.y + u(4f), wakeRect.width, u(34f)), "RÉVEIL EN COURS", _kit.Prompt, new Color(0.55f, 0.85f, 1f));
                _kit.Bar(new Rect(wakeRect.x + u(24f), wakeRect.y + u(48f), wakeRect.width - u(48f), u(40f)), local.WakeProgress, new Color(0.35f, 0.7f, 0.98f));
            }
            else if (local.IsHolding)
            {
                _kit.Panel(promptRect, DebugGuiKit.PanelColor);
                DebugGuiKit.Label(new Rect(promptRect.x, promptRect.y + u(4f), promptRect.width, u(34f)), "REFUS EN COURS…", _kit.Prompt, new Color(1f, 0.55f, 0.5f));
                _kit.Bar(new Rect(promptRect.x + u(24f), promptRect.y + u(44f), promptRect.width - u(48f), u(22f)), local.HoldProgress, new Color(0.95f, 0.3f, 0.3f));
            }
            else if (localCarry != null && localCarry.IsCarrying)
            {
                _kit.Panel(promptRect, DebugGuiKit.PanelColor);
                DebugGuiKit.Label(promptRect, "E — LÂCHER", _kit.Prompt, new Color(1f, 0.85f, 0.4f));
            }
            else if (local.OpenDialogue != null)
            {
                // P1-014 : carte de dialogue, visible seulement pour le joueur concerné.
                NpcDialogue npc = local.OpenDialogue;
                var card = new Rect(promptRect.x, promptRect.yMax - u(236f), promptRect.width, u(236f));
                feedbackBottom = card.y;
                _kit.Panel(card, DebugGuiKit.PanelColor);
                DebugGuiKit.Label(new Rect(card.x + u(16f), card.y + u(6f), card.width - u(32f), u(36f)), npc.NpcName.ToUpperInvariant(), _kit.Title, new Color(0.7f, 0.82f, 1f));
                DebugGuiKit.Label(new Rect(card.x + u(16f), card.y + u(40f), card.width - u(32f), u(40f)), npc.Prompt, _kit.Body, Color.white);
                for (int n = 1; n <= 3; n++)
                    DebugGuiKit.Label(new Rect(card.x + u(32f), card.y + u(76f) + (n - 1) * u(38f), card.width - u(48f), u(36f)), n + " — " + npc.Choice(n), _kit.Body, new Color(1f, 0.85f, 0.4f));
                DebugGuiKit.Label(new Rect(card.x + u(16f), card.yMax - u(40f), card.width - u(32f), u(32f)), "ÉCHAP — FERMER", _kit.Tag, new Color(1f, 1f, 1f, 0.6f));
            }
            else if (local.AimedNpc != null)
            {
                _kit.Panel(promptRect, DebugGuiKit.PanelColor);
                DebugGuiKit.Label(promptRect, "E — PARLER", _kit.Prompt, new Color(0.55f, 0.85f, 1f));
            }
            else if (local.AimedPhone != null)
            {
                _kit.Panel(promptRect, DebugGuiKit.PanelColor);
                DebugGuiKit.Label(promptRect, "E — PRENDRE LE TÉLÉPHONE", _kit.Prompt, new Color(0.8f, 0.85f, 0.95f));
            }
            else if (local.AimedWallet != null)
            {
                _kit.Panel(promptRect, DebugGuiKit.PanelColor);
                DebugGuiKit.Label(promptRect, "E — PRENDRE LE PORTEFEUILLE", _kit.Prompt, new Color(0.55f, 0.75f, 1f));
            }
            else if (local.AimedKeys != null)
            {
                _kit.Panel(promptRect, DebugGuiKit.PanelColor);
                DebugGuiKit.Label(promptRect, "E — PRENDRE LES CLÉS", _kit.Prompt, new Color(1f, 0.9f, 0.4f));
            }
            else if (local.AimedPartner != null)
            {
                // Deux lignes empilées : porter (E) et réveiller (F maintenu) ne tiennent pas côte à côte à 1280 px.
                var tall = new Rect(promptRect.x, promptRect.y - promptHeight * 0.8f, promptRect.width, promptHeight * 1.8f);
                feedbackBottom = tall.y;
                _kit.Panel(tall, DebugGuiKit.PanelColor);
                float line = tall.height * 0.5f;
                DebugGuiKit.Label(new Rect(tall.x, tall.y, tall.width, line), $"E — PORTER JOUEUR {local.AimedPartner.OwnerClientId}", _kit.Prompt, new Color(0.65f, 0.9f, 1f));
                DebugGuiKit.Label(new Rect(tall.x, tall.y + line, tall.width, line), "MAINTENIR F — RÉVEILLER AVEC DE L'EAU", _kit.Prompt, new Color(0.55f, 0.85f, 1f));
            }
            else if (local.AimedOffer != null)
            {
                _kit.Panel(promptRect, DebugGuiKit.PanelColor);
                float left = promptRect.width * 0.28f;
                DebugGuiKit.Label(new Rect(promptRect.x, promptRect.y, left, promptRect.height), "E — BOIRE", _kit.Prompt, new Color(0.55f, 0.95f, 0.6f));
                DebugGuiKit.Label(new Rect(promptRect.x + left, promptRect.y, promptRect.width - left, promptRect.height), "MAINTENIR CLIC DROIT — REFUSER", _kit.Prompt, new Color(1f, 0.6f, 0.55f));
            }

            DrawFeedback(local, localCarry, new Rect(promptRect.x, feedbackBottom - u(70f), promptRect.width, u(56f)));
        }

        // Réveil réussi : détecté par le compteur répliqué de chaque joueur, donc identique sur toutes les instances.
        void UpdateWakeFeedback(ulong localClientId)
        {
            for (int i = 0; i < _wakes.Length; i++)
            {
                PlayerWake w = _wakes[i];
                if (w == null || !w.IsSpawned)
                    continue;
                int count = w.WakeCount;
                int seen = _seenWakeCounts[i];
                _seenWakeCounts[i] = count;
                if (seen < 0 || count <= seen)
                    continue;

                ulong? waker = w.LastWakerClientId;
                if (w.OwnerClientId == localClientId)
                    _wakeFeedbackText = waker.HasValue ? $"JOUEUR {waker.Value} VOUS A RÉVEILLÉ" : "VOUS VOUS ÊTES RÉVEILLÉ";
                else if (waker.HasValue && waker.Value == localClientId)
                    _wakeFeedbackText = $"VOUS AVEZ RÉVEILLÉ JOUEUR {w.OwnerClientId}";
                else
                    _wakeFeedbackText = $"JOUEUR {w.OwnerClientId} EST RÉVEILLÉ";
                _wakeFeedbackUntil = Time.unscaledTime + FeedbackSeconds;
            }
        }

        // Retour enregistré : détecté par l'état répliqué de chaque joueur, donc identique sur toutes les instances.
        void UpdateReturnFeedback(ulong localClientId)
        {
            // P1-051 : « CLÉS RÉCUPÉRÉES » sur la transition répliquée (pas au rattrapage d'un client qui arrive tard).
            if (_keys == null)
                _keys = FindFirstObjectByType<HouseKeys>();
            if (_keys != null && _keys.IsSpawned)
            {
                int taken = _keys.IsTaken ? 1 : 0;
                int seenTaken = _seenKeysTaken;
                _seenKeysTaken = taken;
                if (seenTaken == 0 && taken == 1)
                {
                    _returnFeedbackText = "CLÉS RÉCUPÉRÉES";
                    _returnFeedbackUntil = Time.unscaledTime + 2f;
                }
            }
            // P1-052 : même mécanisme pour le portefeuille, observé indépendamment.
            if (_wallet == null)
                _wallet = FindFirstObjectByType<HouseWallet>();
            if (_wallet != null && _wallet.IsSpawned)
            {
                int walletTaken = _wallet.IsTaken ? 1 : 0;
                int seenWallet = _seenWalletTaken;
                _seenWalletTaken = walletTaken;
                if (seenWallet == 0 && walletTaken == 1)
                {
                    _returnFeedbackText = "PORTEFEUILLE RÉCUPÉRÉ";
                    _returnFeedbackUntil = Time.unscaledTime + 2f;
                }
            }
            // P1-053 : téléphone observé indépendamment, puis la checklist complète (une seule annonce par passage à « tout pris »).
            if (_phone == null)
                _phone = FindFirstObjectByType<HousePhone>();
            if (_phone != null && _phone.IsSpawned)
            {
                int phoneTaken = _phone.IsTaken ? 1 : 0;
                int seenPhone = _seenPhoneTaken;
                _seenPhoneTaken = phoneTaken;
                if (seenPhone == 0 && phoneTaken == 1)
                {
                    _returnFeedbackText = "TÉLÉPHONE RÉCUPÉRÉ";
                    _returnFeedbackUntil = Time.unscaledTime + 2f;
                }
            }
            if (_keys != null && _keys.IsSpawned && _wallet != null && _wallet.IsSpawned && _phone != null && _phone.IsSpawned)
            {
                int ready = DepartureChecklist.AllTaken(_keys.IsTaken, _wallet.IsTaken, _phone.IsTaken) ? 1 : 0;
                int seenReady = _seenReady;
                _seenReady = ready;
                if (seenReady >= 0 && DepartureChecklist.BecameReady(seenReady == 1, ready == 1))
                {
                    _returnFeedbackText = DepartureChecklist.ReadyAnnouncement; // remplace le feedback de la dernière prise
                    _returnFeedbackUntil = Time.unscaledTime + 2f;
                }
            }

            if (_outcome != null && _outcome.IsSpawned)
            {
                int active = _outcome.ReturnActive ? 1 : 0;
                int seenActive = _seenReturnActive;
                _seenReturnActive = active;
                if (seenActive == 0 && active == 1)
                {
                    _returnFeedbackText = "RETOUR ACTIVÉ";
                    _returnFeedbackUntil = Time.unscaledTime + 2f;
                    return;
                }
            }

            string text = null;
            int newlyReturned = 0;
            bool localReturned = false;
            ulong partnerId = 0;
            for (int i = 0; i < _returns.Length; i++)
            {
                PlayerReturn r = _returns[i];
                if (r == null || !r.IsSpawned)
                    continue;
                int now = r.IsReturned ? 1 : 0;
                int seen = _seenReturned[i];
                _seenReturned[i] = now;
                if (seen < 0 || now <= seen)
                    continue;
                newlyReturned++;
                if (r.OwnerClientId == localClientId) localReturned = true;
                else partnerId = r.OwnerClientId;
            }

            if (newlyReturned == 0)
                return;
            if (newlyReturned > 1) text = "VOUS ÊTES RENTRÉS ENSEMBLE";
            else if (localReturned) text = "VOUS ÊTES RENTRÉ";
            else text = $"JOUEUR {partnerId} EST RENTRÉ";
            _returnFeedbackText = text;
            _returnFeedbackUntil = Time.unscaledTime + FeedbackSeconds;
        }

        void DrawFeedback(PlayerInteractor local, PlayerCarry localCarry, Rect rect)
        {
            // Nouvelle décision répliquée : même feedback sur toutes les instances.
            if (_offer != null)
            {
                int count = _offer.DecisionCount;
                if (_seenDecisionCount < 0)
                    _seenDecisionCount = count;
                else if (count != _seenDecisionCount)
                {
                    _seenDecisionCount = count;
                    _decisionFeedbackUntil = Time.unscaledTime + FeedbackSeconds;
                }
            }

            UpdateWakeFeedback(local.OwnerClientId);
            UpdateReturnFeedback(local.OwnerClientId);

            string local_message = local.FeedbackMessage;
            if (string.IsNullOrEmpty(local_message) && localCarry != null)
                local_message = localCarry.FeedbackMessage;
            string text = null;
            Color color = Color.white;
            if (!string.IsNullOrEmpty(local_message))
            {
                text = local_message;
                color = new Color(1f, 0.85f, 0.35f);
            }
            else if (_wakeFeedbackText != null && Time.unscaledTime < _wakeFeedbackUntil)
            {
                text = _wakeFeedbackText;
                color = new Color(0.55f, 0.85f, 1f);
            }
            else if (_npc != null && !string.IsNullOrEmpty(_npc.ActiveReaction))
            {
                // Réaction du PNJ (P1-014) : répliquée, donc la même pour les deux joueurs pendant la même durée.
                text = _npc.NpcName + " : " + _npc.ActiveReaction;
                color = new Color(1f, 0.85f, 0.4f);
            }
            else if (_returnFeedbackText != null && Time.unscaledTime < _returnFeedbackUntil)
            {
                text = _returnFeedbackText;
                color = new Color(0.45f, 0.95f, 0.6f);
            }
            else if (_offer != null && Time.unscaledTime < _decisionFeedbackUntil)
            {
                bool accepted = _offer.LastDecision == DrinkDecision.Accepted;
                ulong who = _offer.LastDeciderClientId;
                string subject = who == local.OwnerClientId ? "VOUS AVEZ" : $"JOUEUR {who} A";
                text = $"{subject} {(accepted ? "ACCEPTÉ" : "REFUSÉ")} LE VERRE";
                color = accepted ? new Color(0.55f, 0.95f, 0.6f) : new Color(1f, 0.6f, 0.55f);
            }

            if (text == null)
                return;
            _kit.Panel(rect, DebugGuiKit.PanelColor);
            DebugGuiKit.Label(rect, text, _kit.Prompt, color);
        }
    }
}

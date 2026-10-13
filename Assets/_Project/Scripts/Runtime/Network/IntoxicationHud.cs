using LastCallCrew.Drink;
using LastCallCrew.Intoxication;
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
        int[] _seenWakeCounts = System.Array.Empty<int>();
        DrinkOffer _offer;
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
            DrawInteractionFeedback(nm, localCarry);
            if (localCarry != null && localCarry.IsCarrying)
                DrawCarryBanner(localCarry);
            if (localKnockedOut)
                DrawKnockoutMessage(localCarry, nm.LocalClient.PlayerObject.GetComponent<PlayerWake>());
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
                _seenWakeCounts = new int[_players.Length];
                for (int i = 0; i < _seenWakeCounts.Length; i++)
                    _seenWakeCounts[i] = -1; // -1 : pas encore observé, donc pas de feedback au rattrapage d'un nouveau client
            }
            for (int i = 0; i < _players.Length; i++)
            {
                _knockouts[i] = _players[i] != null ? _players[i].GetComponent<PlayerKnockout>() : null;
                _carries[i] = _players[i] != null ? _players[i].GetComponent<PlayerCarry>() : null;
                _wakes[i] = _players[i] != null ? _players[i].GetComponent<PlayerWake>() : null;
            }
            if (_offer == null)
                _offer = FindFirstObjectByType<DrinkOffer>();
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
                DrawPlayerCard(p, knockedOut, carried, new Rect(x + u(10f), cy, width - u(20f), cardHeight));
                cy += cardHeight + u(8f);
            }
        }

        void DrawPlayerCard(PlayerIntoxication player, bool knockedOut, bool carried, Rect card)
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
            float width = Mathf.Min(u(900f), Screen.width - u(40f));
            ulong? carrier = localCarry != null ? localCarry.CarrierClientId : null;
            // Placé dans le tiers haut pour laisser le corps couché, cadré par la caméra de KO, visible au centre (P1-020a).
            float height = u(120f) + (carrier.HasValue ? u(56f) : 0f) + (localWake != null ? u(56f) : 0f);
            var rect = new Rect((Screen.width - width) * 0.5f, Screen.height * 0.1f, width, height);
            _kit.Panel(rect, new Color(0.05f, 0.02f, 0.03f, 0.88f));
            DebugGuiKit.Label(new Rect(rect.x, rect.y, rect.width, u(120f)), "VOUS ÊTES KO", _kit.Huge, new Color(1f, 0.35f, 0.4f));
            float y = rect.y + u(118f);
            if (carrier.HasValue)
            {
                DebugGuiKit.Label(new Rect(rect.x, y, rect.width, u(50f)), $"PORTÉ PAR JOUEUR {carrier.Value}", _kit.Prompt, new Color(0.65f, 0.9f, 1f));
                y += u(56f);
            }
            if (localWake != null)
                DebugGuiKit.Label(new Rect(rect.x, y, rect.width, u(50f)),
                    $"RÉVEIL AUTOMATIQUE DANS {Mathf.CeilToInt(localWake.AutoWakeRemaining)} s", _kit.Prompt, new Color(1f, 0.85f, 0.4f));
        }

        // ---- Bandeau du porteur (haut au centre) ----

        void DrawCarryBanner(PlayerCarry localCarry)
        {
            float u(float v) => _kit.U(v);
            ulong? carried = localCarry.CarriedPlayerClientId;
            float width = Mathf.Min(u(640f), Screen.width * 0.34f);
            var rect = new Rect((Screen.width - width) * 0.5f, u(16f), width, u(56f));
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

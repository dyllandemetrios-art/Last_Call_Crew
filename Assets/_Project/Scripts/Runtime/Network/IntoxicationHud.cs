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
        DrinkOffer _offer;
        float _nextRefresh;
        int _seenDecisionCount = -1;
        float _decisionFeedbackUntil;

        void OnDestroy() => _kit.Destroy();

        void OnGUI()
        {
            var nm = NetworkManager.Singleton;
            if (nm == null || !nm.IsListening)
                return;

            _kit.Ensure();
            Refresh();
            DrawIntoxicationPanel();
            DrawInteractionFeedback(nm);
        }

        void Refresh()
        {
            if (Time.unscaledTime < _nextRefresh)
                return;
            _nextRefresh = Time.unscaledTime + 0.5f;
            _players = FindObjectsByType<PlayerIntoxication>(FindObjectsSortMode.None);
            System.Array.Sort(_players, (a, b) => a.OwnerClientId.CompareTo(b.OwnerClientId));
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
            foreach (PlayerIntoxication p in _players)
            {
                if (p == null || !p.IsSpawned)
                    continue;
                DrawPlayerCard(p, new Rect(x + u(10f), cy, width - u(20f), cardHeight));
                cy += cardHeight + u(8f);
            }
        }

        void DrawPlayerCard(PlayerIntoxication player, Rect card)
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

            DebugGuiKit.Label(new Rect(card.x + u(12f), card.y + u(48f), card.width - u(24f), u(30f)),
                IntoxicationRules.Label(stage).ToUpperInvariant(), _kit.Body, stageColor);
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

        // ---- Invite, progression du refus et feedback (bas au centre) ----

        void DrawInteractionFeedback(NetworkManager nm)
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

            if (local.IsHolding)
            {
                _kit.Panel(promptRect, DebugGuiKit.PanelColor);
                DebugGuiKit.Label(new Rect(promptRect.x, promptRect.y + u(4f), promptRect.width, u(34f)), "REFUS EN COURS…", _kit.Prompt, new Color(1f, 0.55f, 0.5f));
                _kit.Bar(new Rect(promptRect.x + u(24f), promptRect.y + u(44f), promptRect.width - u(48f), u(22f)), local.HoldProgress, new Color(0.95f, 0.3f, 0.3f));
            }
            else if (local.AimedOffer != null)
            {
                _kit.Panel(promptRect, DebugGuiKit.PanelColor);
                float left = promptRect.width * 0.28f;
                DebugGuiKit.Label(new Rect(promptRect.x, promptRect.y, left, promptRect.height), "E — BOIRE", _kit.Prompt, new Color(0.55f, 0.95f, 0.6f));
                DebugGuiKit.Label(new Rect(promptRect.x + left, promptRect.y, promptRect.width - left, promptRect.height), "MAINTENIR CLIC DROIT — REFUSER", _kit.Prompt, new Color(1f, 0.6f, 0.55f));
            }

            DrawFeedback(local, new Rect(promptRect.x, promptRect.y - u(70f), promptRect.width, u(56f)));
        }

        void DrawFeedback(PlayerInteractor local, Rect rect)
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

            string local_message = local.FeedbackMessage;
            string text = null;
            Color color = Color.white;
            if (!string.IsNullOrEmpty(local_message))
            {
                text = local_message;
                color = new Color(1f, 0.85f, 0.35f);
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

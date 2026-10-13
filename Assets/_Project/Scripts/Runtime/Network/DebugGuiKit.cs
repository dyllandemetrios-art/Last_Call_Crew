using UnityEngine;

namespace LastCallCrew.Network
{
    /// <summary>
    /// Outils IMGUI partagés par les HUD de debug : échelle selon la hauteur d'écran, styles mis en cache
    /// (créés une seule fois, recréés seulement si la résolution change), fond sombre semi-opaque et barres.
    /// À utiliser depuis OnGUI uniquement.
    /// </summary>
    public sealed class DebugGuiKit
    {
        public static readonly Color PanelColor = new Color(0.05f, 0.06f, 0.09f, 0.82f);
        public static readonly Color PanelHighlightColor = new Color(0.12f, 0.2f, 0.32f, 0.9f);

        public float Scale { get; private set; }
        public GUIStyle Title { get; private set; }
        public GUIStyle Body { get; private set; }
        public GUIStyle Value { get; private set; }
        public GUIStyle Prompt { get; private set; }
        public GUIStyle Huge { get; private set; }
        /// <summary>Grand affichage centré (chronomètre) : plus petit que Huge pour tenir dans une carte.</summary>
        public GUIStyle Display { get; private set; }
        /// <summary>Message court plein cadre (ex. TEMPS ÉCOULÉ) : un cran sous Display pour tenir sur la largeur d'une carte.</summary>
        public GUIStyle Banner { get; private set; }
        public GUIStyle Button { get; private set; }

        Texture2D _white;
        int _builtForHeight = -1;

        /// <summary>Prépare l'échelle et les styles ; coûte un test d'entier quand rien ne change.</summary>
        public void Ensure()
        {
            if (_white == null)
            {
                _white = new Texture2D(1, 1) { hideFlags = HideFlags.HideAndDontSave };
                _white.SetPixel(0, 0, Color.white);
                _white.Apply();
            }

            if (_builtForHeight == Screen.height && Title != null)
                return;

            _builtForHeight = Screen.height;
            Scale = Mathf.Clamp(Screen.height / 1080f, 0.6f, 2.5f);
            Title = MakeStyle(24, FontStyle.Bold, TextAnchor.MiddleLeft, new Color(0.7f, 0.82f, 1f));
            Body = MakeStyle(28, FontStyle.Normal, TextAnchor.MiddleLeft, Color.white);
            Value = MakeStyle(34, FontStyle.Bold, TextAnchor.MiddleRight, Color.white);
            Prompt = MakeStyle(32, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
            Huge = MakeStyle(84, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
            Display = MakeStyle(68, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
            Banner = MakeStyle(50, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
            Button = new GUIStyle(GUI.skin.button)
            {
                fontSize = Mathf.RoundToInt(28 * Scale),
                fontStyle = FontStyle.Bold,
            };
        }

        public void Destroy()
        {
            if (_white != null)
                Object.Destroy(_white);
            _white = null;
        }

        public float U(float units) => units * Scale;

        public void Panel(Rect rect, Color color) => Fill(rect, color);

        public void Bar(Rect rect, float fraction, Color fillColor)
        {
            Fill(rect, new Color(0f, 0f, 0f, 0.65f));
            float inset = Mathf.Max(1f, U(2f));
            var inner = new Rect(rect.x + inset, rect.y + inset, (rect.width - 2f * inset) * Mathf.Clamp01(fraction), rect.height - 2f * inset);
            Fill(inner, fillColor);
        }

        public static void Label(Rect rect, string text, GUIStyle style, Color color)
        {
            Color previous = style.normal.textColor;
            style.normal.textColor = color;
            GUI.Label(rect, text, style);
            style.normal.textColor = previous;
        }

        void Fill(Rect rect, Color color)
        {
            Color previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, _white);
            GUI.color = previous;
        }

        GUIStyle MakeStyle(int baseSize, FontStyle fontStyle, TextAnchor anchor, Color color)
        {
            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(baseSize * Scale),
                fontStyle = fontStyle,
                alignment = anchor,
                wordWrap = false,
                clipping = TextClipping.Clip,
            };
            style.normal.textColor = color;
            return style;
        }
    }
}

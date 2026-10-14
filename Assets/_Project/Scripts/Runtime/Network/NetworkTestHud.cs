using LastCallCrew.Connection;
using Unity.Netcode;
using UnityEngine;

namespace LastCallCrew.Network
{
    /// <summary>Boutons temporaires réseau : local Host / Client (P1-001) et connexion Relay par code (P1-003).</summary>
    public class NetworkTestHud : MonoBehaviour
    {
        readonly DebugGuiKit _kit = new DebugGuiKit();
        float _nextLog;
        RelayConnection _relay;
        string _codeInput = string.Empty;
        GUIStyle _field;
        string _copiedCode = string.Empty;
        float _copiedAt;
        const float CopiedNoticeSeconds = 5f;

        void Start()
        {
            // Lancement scripté : -lcc-role host|client (tests sans clic, ex. build autonome + éditeur).
            _relay = GetComponent<RelayConnection>();
            if (_relay == null)
                _relay = gameObject.AddComponent<RelayConnection>();

            var nm = NetworkManager.Singleton;
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] != "-lcc-role") continue;
                if (args[i + 1] == "host") nm.StartHost();
                else if (args[i + 1] == "client") nm.StartClient();
            }
        }

        void OnDestroy() => _kit.Destroy();

        // P1-003a : un nouveau code d'hôte est copié une seule fois dans le presse-papiers (la souris est capturée en jeu,
        // donc aucun bouton n'est cliquable) ; le champ reste sélectionnable au clavier (Tab, Ctrl+C) en secours.
        void CopyNewJoinCode()
        {
            string code = _relay != null ? _relay.JoinCode : string.Empty;
            if (string.IsNullOrEmpty(code))
            {
                _copiedCode = string.Empty;
                return;
            }
            if (code == _copiedCode)
                return;
            GUIUtility.systemCopyBuffer = code;
            _copiedCode = code;
            _copiedAt = Time.unscaledTime;
        }

        void Update()
        {
            CopyNewJoinCode();
            var nm = NetworkManager.Singleton;
            if (nm == null || !nm.IsListening || Time.time < _nextLog) return;
            _nextLog = Time.time + 5f;
            foreach (var kv in nm.SpawnManager.SpawnedObjects)
            {
                if (!kv.Value.IsPlayerObject) continue;
                Debug.Log($"[P1-001] local={nm.LocalClientId} player owner={kv.Value.OwnerClientId} pos={kv.Value.transform.position} yaw={kv.Value.transform.eulerAngles.y:0}");
            }
        }

        // Panneau réseau compact, en haut à gauche (présentation P1-013a ; la logique Host/Client/Shutdown est inchangée).
        void OnGUI()
        {
            _kit.Ensure();
            float u(float v) => _kit.U(v);
            DrawBuildLabel(u);
            var nm = NetworkManager.Singleton;
            float width = u(380f);
            float x = u(16f);
            float y = u(16f);

            if (nm == null)
            {
                _kit.Panel(new Rect(x, y, width, u(60f)), DebugGuiKit.PanelColor);
                DebugGuiKit.Label(new Rect(x + u(14f), y, width - u(28f), u(60f)), "NetworkManager introuvable", _kit.Body, Color.white);
            }
            else if (!nm.IsListening)
            {
                bool idle = RelayJoinRules.CanBegin(_relay.Phase);
                bool error = !string.IsNullOrEmpty(_relay.ErrorMessage);
                // P1-040 : l'erreur revient à la ligne ; le panneau grandit avec elle (lisible à 960×540 comme à 1280×720).
                GUIStyle errorStyle = ErrorStyle();
                float errorHeight = error ? errorStyle.CalcHeight(new GUIContent(_relay.ErrorMessage), width - u(28f)) + u(12f) : u(8f);
                float height = u(46f) + u(64f) + u(40f) + u(52f) + u(64f) + u(64f) + errorHeight;
                _kit.Panel(new Rect(x, y, width, height), DebugGuiKit.PanelColor);
                DebugGuiKit.Label(new Rect(x + u(14f), y + u(4f), width - u(28f), u(40f)),
                    "RÉSEAU · " + RelayJoinRules.PhaseLabel(_relay.Phase), _kit.Title, new Color(0.7f, 0.82f, 1f));
                float cy = y + u(46f);
                GUI.enabled = idle;
                if (GUI.Button(new Rect(x + u(10f), cy, width - u(20f), u(56f)), "HÉBERGER EN LIGNE", _kit.Button)) _relay.BeginHost();
                cy += u(64f);
                DebugGuiKit.Label(new Rect(x + u(14f), cy, width - u(28f), u(36f)), "CODE DE L'HÔTE", _kit.Body, Color.white);
                cy += u(36f);
                _codeInput = GUI.TextField(new Rect(x + u(10f), cy, width - u(20f), u(46f)), _codeInput, RelayJoinRules.CodeMaxLength, Field());
                cy += u(52f);
                if (GUI.Button(new Rect(x + u(10f), cy, width - u(20f), u(56f)), "REJOINDRE", _kit.Button)) _relay.BeginJoin(_codeInput);
                cy += u(64f);
                float half = (width - u(30f)) * 0.5f;
                if (GUI.Button(new Rect(x + u(10f), cy, half, u(56f)), "HÔTE LOCAL", _kit.Button)) nm.StartHost();
                if (GUI.Button(new Rect(x + u(20f) + half, cy, half, u(56f)), "CLIENT LOCAL", _kit.Button)) nm.StartClient();
                GUI.enabled = true;
                if (error)
                    DebugGuiKit.Label(new Rect(x + u(14f), cy + u(62f), width - u(28f), errorHeight), _relay.ErrorMessage, errorStyle, new Color(1f, 0.55f, 0.5f));
            }
            else
            {
                bool online = !string.IsNullOrEmpty(_relay.JoinCode);
                _kit.Panel(new Rect(x, y, width, u(46f) + u(44f) + u(64f) + (online ? u(100f) : 0f)), DebugGuiKit.PanelColor);
                DebugGuiKit.Label(new Rect(x + u(14f), y + u(4f), width - u(28f), u(40f)), "RÉSEAU", _kit.Title, new Color(0.7f, 0.82f, 1f));
                string role = nm.IsHost ? "HÔTE" : "CLIENT";
                DebugGuiKit.Label(new Rect(x + u(14f), y + u(46f), width - u(28f), u(40f)),
                    $"{role} · ID {nm.LocalClientId} · {nm.ConnectedClientsIds.Count} joueur(s)", _kit.Body, Color.white);
                float by = y + u(46f) + u(44f);
                if (online)
                {
                    // Code lisible, sélectionnable (champ en lecture seule) et copiable.
                    DebugGuiKit.Label(new Rect(x + u(14f), by, width - u(28f), u(36f)), "CODE", _kit.Body, Color.white);
                    if (Time.unscaledTime - _copiedAt < CopiedNoticeSeconds && _copiedCode == _relay.JoinCode)
                        DebugGuiKit.Label(new Rect(x + u(14f), by, width - u(28f), u(36f)), "CODE COPIÉ", _kit.Tag, new Color(0.45f, 0.95f, 0.6f));
                    GUI.TextField(new Rect(x + u(10f), by + u(36f), width - u(20f), u(56f)), _relay.JoinCode, Field(true));
                    by += u(100f);
                }
                GUI.enabled = _relay.Phase != ConnectionPhase.Stopping;
                if (GUI.Button(new Rect(x + u(10f), by, width - u(20f), u(56f)), "ARRÊTER", _kit.Button))
                {
                    if (RelayJoinRules.CanStop(_relay.Phase)) _relay.BeginStop();
                    else nm.Shutdown();
                }
                GUI.enabled = true;
            }
        }

        // Style de champ à l'échelle du HUD (police agrandie, texte en majuscules lisible).
        GUIStyle Field(bool large = false)
        {
            if (_field == null)
            {
                _field = new GUIStyle(GUI.skin.textField) { alignment = TextAnchor.MiddleCenter };
                _field.normal.textColor = Color.white;
            }
            _field.fontSize = Mathf.RoundToInt(_kit.U(large ? 34f : 26f));
            return _field;
        }

        // P1-004 : identifiant discret de la version testée (version, identifiant de build Unity, mode développement).
        void DrawBuildLabel(System.Func<float, float> u)
        {
            string guid = Application.buildGUID;
            string id = string.IsNullOrEmpty(guid) ? "éditeur" : guid.Substring(0, System.Math.Min(8, guid.Length));
            string text = $"LCC v{Application.version} · build {id}" + (Debug.isDebugBuild ? " · DEV" : string.Empty);
            DebugGuiKit.Label(new Rect(u(16f), Screen.height - u(34f), u(520f), u(28f)), text, _kit.Body, new Color(1f, 1f, 1f, 0.55f));
        }

        GUIStyle _errorStyle;

        // Style du message d'erreur : taille du corps de texte du HUD, retour à la ligne, alignement à gauche en haut.
        GUIStyle ErrorStyle()
        {
            if (_errorStyle == null)
                _errorStyle = new GUIStyle(_kit.Body) { wordWrap = true, alignment = TextAnchor.UpperLeft };
            _errorStyle.fontSize = Mathf.RoundToInt(_kit.U(22f));
            return _errorStyle;
        }
    }
}

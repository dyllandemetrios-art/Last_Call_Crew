using Unity.Netcode;
using UnityEngine;

namespace LastCallCrew.Network
{
    /// <summary>Boutons temporaires Host / Client / Shutdown pour la scène de test P1-001.</summary>
    public class NetworkTestHud : MonoBehaviour
    {
        readonly DebugGuiKit _kit = new DebugGuiKit();
        float _nextLog;

        void Start()
        {
            // Lancement scripté : -lcc-role host|client (tests sans clic, ex. build autonome + éditeur).
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

        void Update()
        {
            var nm = NetworkManager.Singleton;
            if (nm == null || !nm.IsListening || Time.time < _nextLog) return;
            _nextLog = Time.time + 5f;
            foreach (var kv in nm.SpawnManager.SpawnedObjects)
            {
                if (!kv.Value.IsPlayerObject) continue;
                Debug.Log($"[P1-001] local={nm.LocalClientId} player owner={kv.Value.OwnerClientId} pos={kv.Value.transform.position}");
            }
        }

        // Panneau réseau compact, en haut à gauche (présentation P1-013a ; la logique Host/Client/Shutdown est inchangée).
        void OnGUI()
        {
            _kit.Ensure();
            float u(float v) => _kit.U(v);
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
                _kit.Panel(new Rect(x, y, width, u(46f) + 2f * u(64f) + u(8f)), DebugGuiKit.PanelColor);
                DebugGuiKit.Label(new Rect(x + u(14f), y + u(4f), width - u(28f), u(40f)), "RÉSEAU", _kit.Title, new Color(0.7f, 0.82f, 1f));
                if (GUI.Button(new Rect(x + u(10f), y + u(46f), width - u(20f), u(56f)), "HÔTE", _kit.Button)) nm.StartHost();
                if (GUI.Button(new Rect(x + u(10f), y + u(46f) + u(64f), width - u(20f), u(56f)), "CLIENT", _kit.Button)) nm.StartClient();
            }
            else
            {
                _kit.Panel(new Rect(x, y, width, u(46f) + u(44f) + u(64f)), DebugGuiKit.PanelColor);
                DebugGuiKit.Label(new Rect(x + u(14f), y + u(4f), width - u(28f), u(40f)), "RÉSEAU", _kit.Title, new Color(0.7f, 0.82f, 1f));
                string role = nm.IsHost ? "HÔTE" : "CLIENT";
                DebugGuiKit.Label(new Rect(x + u(14f), y + u(46f), width - u(28f), u(40f)),
                    $"{role} · ID {nm.LocalClientId} · {nm.ConnectedClientsIds.Count} joueur(s)", _kit.Body, Color.white);
                if (GUI.Button(new Rect(x + u(10f), y + u(46f) + u(44f), width - u(20f), u(56f)), "ARRÊTER", _kit.Button)) nm.Shutdown();
            }
        }
    }
}

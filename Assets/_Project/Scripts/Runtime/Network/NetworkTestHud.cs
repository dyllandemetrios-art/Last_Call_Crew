using Unity.Netcode;
using UnityEngine;

namespace LastCallCrew.Network
{
    /// <summary>Boutons temporaires Host / Client / Shutdown pour la scène de test P1-001.</summary>
    public class NetworkTestHud : MonoBehaviour
    {
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

        void OnGUI()
        {
            var nm = NetworkManager.Singleton;
            GUILayout.BeginArea(new Rect(10, 10, 260, 200));
            if (nm == null)
            {
                GUILayout.Label("NetworkManager introuvable");
            }
            else if (!nm.IsListening)
            {
                if (GUILayout.Button("Host", GUILayout.Height(40))) nm.StartHost();
                if (GUILayout.Button("Client", GUILayout.Height(40))) nm.StartClient();
            }
            else
            {
                string role = nm.IsHost ? "Host" : "Client";
                GUILayout.Label($"{role} - ClientId {nm.LocalClientId} - joueurs {nm.ConnectedClientsIds.Count}");
                if (GUILayout.Button("Shutdown", GUILayout.Height(40))) nm.Shutdown();
            }
            GUILayout.EndArea();
        }
    }
}

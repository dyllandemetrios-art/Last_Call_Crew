using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.Netcode.Transports.UTP;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using LastCallCrew.Network;

public static class BuildP1001
{
    public static string Run()
    {
        // Prefab joueur
        var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.AddComponent<NetworkObject>();
        var nt = player.AddComponent<NetworkTransform>();
        nt.AuthorityMode = NetworkTransform.AuthorityModes.Owner;
        var camRoot = new GameObject("CameraRoot");
        camRoot.transform.SetParent(player.transform, false);
        camRoot.transform.localPosition = new Vector3(0, 0.6f, 0);
        var cam = camRoot.AddComponent<Camera>();
        cam.depth = 10;
        var listener = camRoot.AddComponent<AudioListener>();
        var pc = player.AddComponent<PlayerController>();
        var so = new SerializedObject(pc);
        so.FindProperty("inputActions").objectReferenceValue = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions");
        so.FindProperty("cameraRoot").objectReferenceValue = camRoot.transform;
        so.FindProperty("playerCamera").objectReferenceValue = cam;
        so.FindProperty("audioListener").objectReferenceValue = listener;
        so.ApplyModifiedPropertiesWithoutUndo();
        var prefab = PrefabUtility.SaveAsPrefabAsset(player, "Assets/_Project/Prefabs/Player.prefab");
        Object.DestroyImmediate(player);

        // Scène
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var light = new GameObject("Directional Light");
        var l = light.AddComponent<Light>(); l.type = LightType.Directional;
        light.transform.rotation = Quaternion.Euler(50, -30, 0);
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground"; ground.transform.localScale = new Vector3(5, 1, 5);
        var camGo = new GameObject("Overview Camera"); camGo.tag = "MainCamera";
        var oc = camGo.AddComponent<Camera>(); oc.depth = -10;
        camGo.transform.SetPositionAndRotation(new Vector3(0, 15, -15), Quaternion.Euler(45, 0, 0));
        var nmGo = new GameObject("NetworkManager");
        var nm = nmGo.AddComponent<NetworkManager>();
        var utp = nmGo.AddComponent<UnityTransport>();
        nm.NetworkConfig.NetworkTransport = utp;
        nm.NetworkConfig.PlayerPrefab = prefab;
        nmGo.AddComponent<NetworkTestHud>();
        EditorSceneManager.SaveScene(scene, "Assets/_Project/Scenes/NetworkTest.unity");
        return "ok";
    }
}

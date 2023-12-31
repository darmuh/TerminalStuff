using HarmonyLib;
using System.Reflection;
using TerminalStuff;
using Unity.Netcode;
using UnityEngine;
using GameObject = UnityEngine.GameObject;

[HarmonyPatch]
public class netObject
{

    [HarmonyPostfix, HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.Start))]
    public static void Init()
    {
        if (networkPrefab != null)
            return;

        var MainAssetBundle = AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("TerminalStuff.darmuhngo"));
        networkPrefab = (GameObject)MainAssetBundle.LoadAsset("darmuhNGO");
        networkPrefab.AddComponent<NetHandler>();

        NetworkManager.Singleton.AddNetworkPrefab(networkPrefab);
    }

    [HarmonyPostfix, HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.Awake))]
    static void SpawnNetworkHandler()
    {
        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
        {
            var networkHandlerHost = Object.Instantiate(networkPrefab, Vector3.zero, Quaternion.identity);
            networkHandlerHost.GetComponent<NetworkObject>().Spawn();
        }
    }

    static GameObject networkPrefab;
}
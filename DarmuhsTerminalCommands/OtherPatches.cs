using HarmonyLib;
using System.Reflection;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using static TerminalStuff.LeaveTerminal;
using Object = UnityEngine.Object;
using TerminalApi;
using Plugin = TerminalStuff.Plugin;
using AssetBundle = UnityEngine.AssetBundle;
using static TerminalStuff.AllMyTerminalPatches;


namespace TerminalStuff
{
    [HarmonyPatch(typeof(StartOfRound), "openingDoorsSequence")]
    public class OpeningDoorsPatch
    {
        public static string getDangerLevel = "";
        public static void Postfix(ref StartOfRound __instance)
        {
            string dangerLevel = __instance.currentLevel.riskLevel;
            getDangerLevel = dangerLevel;
        }
    }
    [HarmonyPatch(typeof(StartOfRound), "Start")]
    public class StartRoundPatch
    {
        public static void Postfix(ref StartOfRound __instance)
        {
            Plugin.instance.splitViewCreated = false;
            Terminal_RunTerminalEvents_Patch.AddDuplicateRenderObjects(); //addSplitViewObjects
        }

    }
    [HarmonyPatch(typeof(GameNetworkManager), "Start")]
    public class GameStartPatch
    {
        public static void Postfix(ref  GameNetworkManager __instance)
        {
            Plugin.AddKeywords();
            Plugin.Log.LogInfo("Enabled Commands added");
        }
    }
 /*   [HarmonyPatch(typeof(StartOfRound), "Update")]
    public class StartRoundUpdatePatch
    {
        public static void Postfix(ref StartOfRound __instance)
        {
            //careful, this is updated constantly

             //hope this doesnt take up performance...
            if (!Terminal_Awake_Patch.isTermInUse && Terminal_Awake_Patch.alwaysOnDisplay && Terminal_Awake_Patch.displayVarSET)
            {
                GameObject terminalScreen = GameObject.Find("Environment/HangarShip/Terminal/Canvas");
                //__instance.terminalUIScreen.gameObject.SetActive(true);
                terminalScreen.SetActive(true);
                Terminal_Awake_Patch.displayVarSET = false;
                Plugin.Log.LogInfo("Attempting to keep screen on");
                return;
            }
            else if (!Terminal_Awake_Patch.isTermInUse && !Terminal_Awake_Patch.alwaysOnDisplay && Terminal_Awake_Patch.displayVarSET)
            {
                GameObject terminalScreen = GameObject.Find("Environment/HangarShip/Terminal/Canvas");
                terminalScreen.SetActive(false);
                Terminal_Awake_Patch.displayVarSET = false;
                Plugin.Log.LogInfo("Attempting to turn screen back off"); //lol
                return;
            }
            else
                return;
        }
    }*/

/*
    public class rpcPatchStuff : NetworkBehaviour
    {
        public static rpcPatchStuff instance;

        [HarmonyPatch]
        public class GameNetManPatch
        {
            [HarmonyPatch(typeof(GameNetworkManager), "Start")]
            public static void Postfix(ref GameNetworkManager __instance)
            {

                NetworkManager.Singleton.AddNetworkPrefab(Plugin.instance.myNetworkPrefab);
                Plugin.Log.LogInfo("networkprefab added from plugin instance");

            }
        }

        [HarmonyPatch]
        public class StartNetPatch
        {
            [HarmonyPatch(typeof(StartOfRound), "Awake")]
            public static void Postfix(ref StartOfRound __instance)
            {
                if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
                {
                    var networkHandlerHost = Object.Instantiate(Plugin.instance.myNetworkPrefab, Vector3.zero, Quaternion.identity);
                    networkHandlerHost.GetComponent<NetworkObject>().Spawn();
                    rpcPatchStuff.instance = networkHandlerHost.AddComponent<rpcPatchStuff>();

                    Plugin.Log.LogInfo("networkprefab spawned from network object and assigned to rpcPatchStuff.instance");
                }
            }
        }


        [ServerRpc(RequireOwnership = false)]
        public void TestServerRpc()
        {
            Plugin.Log.LogInfo("SERVER - Test");
            TestClientRpc();
        }

        [ClientRpc]
        public void TestClientRpc()
        {
            Plugin.Log.LogInfo("CLIENT - Test");
        }

        public void Test()
        {
            Plugin.Log.LogInfo("COMMAND - Test");
            TestServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        public void SyncFlashlightColorServerRpc(Color flashlightColor, string playername)
        {
            NetworkManager networkManager = base.NetworkManager;
            if ((object)networkManager == null || !networkManager.IsListening)
            {
                return;
            }

            if (__rpc_exec_stage != __RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
            {

                ServerRpcParams serverRpcParams = default(ServerRpcParams);
                FastBufferWriter bufferWriter = __beginSendServerRpc(3484508350u, serverRpcParams, RpcDelivery.Reliable);
                WriteColorToBuffer(bufferWriter, flashlightColor);

                // Manually serialize the string to bytes and send
                byte[] playerNameBytes = Encoding.UTF8.GetBytes(playername);
                BytePacker.WriteValueBitPacked(bufferWriter, playerNameBytes.Length);

                bufferWriter.WriteBytes(playerNameBytes);

                __endSendServerRpc(ref bufferWriter, 3484508350u, serverRpcParams, RpcDelivery.Reliable);
            }

            if (__rpc_exec_stage == __RpcExecStage.Server && (networkManager.IsServer || networkManager.IsHost))
            {
                SyncFlashlightColorClientRpc(flashlightColor, playername);
                Plugin.Log.LogInfo("SyncFlashlightColorServerRpc: MOVING TO CLIENTRPC (IsClient or IsHost)");
            }
        }

        [ClientRpc]
        public void SyncFlashlightColorClientRpc(Color flashlightColor, string playername)
        {
            NetworkManager networkManager = base.NetworkManager;
            if ((object)networkManager != null && networkManager.IsListening)
            {
                if (__rpc_exec_stage != __RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
                {
                    ClientRpcParams clientRpcParams = default(ClientRpcParams);
                    FastBufferWriter bufferWriter = __beginSendClientRpc(2670202430u, clientRpcParams, RpcDelivery.Reliable);
                    WriteColorToBuffer(bufferWriter, flashlightColor);

                    // Manually serialize the string to bytes and send
                    byte[] playerNameBytes = Encoding.UTF8.GetBytes(playername);
                    BytePacker.WriteValueBitPacked(bufferWriter, playerNameBytes.Length);

                    bufferWriter.WriteBytes(playerNameBytes);

                    __endSendClientRpc(ref bufferWriter, 2670202430u, clientRpcParams, RpcDelivery.Reliable);
                    Plugin.Log.LogInfo("SyncFlashlightColorClientRpc: IS SERVER OR HOST, NOT CLIENT");
                }

                if (__rpc_exec_stage == __RpcExecStage.Client && (networkManager.IsClient || networkManager.IsHost))
                {
                    Plugin.Log.LogInfo("SyncFlashlightColorClientRpc: SETTING COLOR (IsClient or IsHost)");
                    SetFlashlightColor(flashlightColor, playername);
                }
            }
        }

        private void WriteColorToBuffer(FastBufferWriter bufferWriter, Color color)
        {
            BytePacker.WriteValueBitPacked(bufferWriter, (int)(color.r * 255));
            BytePacker.WriteValueBitPacked(bufferWriter, (int)(color.g * 255));
            BytePacker.WriteValueBitPacked(bufferWriter, (int)(color.b * 255));
            BytePacker.WriteValueBitPacked(bufferWriter, (int)(color.a * 255));
        }

        private void SetFlashlightColor(Color flashlightColor, string playername)
        {
            if (GameNetworkManager.Instance.localPlayerController.pocketedFlashlight != null &&
                GameNetworkManager.Instance.localPlayerController.pocketedFlashlight.playerHeldBy.playerUsername == playername)
            {
                FlashlightItem flashlight = GameNetworkManager.Instance.localPlayerController.pocketedFlashlight.gameObject.GetComponent<FlashlightItem>();
                flashlight.flashlightBulb.color = flashlightColor;
                flashlight.flashlightBulbGlow.color = flashlightColor;
                Plugin.Log.LogInfo($"Setting flashlight color to {flashlightColor} for player {playername}");
                GameNetworkManager.Instance.localPlayerController.helmetLight.color = flashlightColor;
                // Additional logic if needed...
            }
        }

    } */
}

using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using TerminalApi;
using static TerminalStuff.AllMyTerminalPatches;
using static UnityEngine.GraphicsBuffer;
using Object = UnityEngine.Object;
using System.Diagnostics.Eventing.Reader;
using static TerminalApi.Events.Events;
using Steamworks;

namespace TerminalStuff
{
    public class NetHandler : NetworkBehaviour
    {

        public static NetHandler Instance { get; private set; }
        public static Terminal patchTerminal = null;
        public static bool netNodeSet = false;

        [ClientRpc]
        public void TestClientRpc()
        {

            NetworkManager networkManager = base.NetworkManager;
            if ((object)networkManager != null && networkManager.IsListening)
            {
                if (__rpc_exec_stage != __RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
                {
                    string stringTest = "TEST - isHost/isServer (exec stage not client)";
                    Plugin.Log.LogInfo($"{stringTest}");
                }

                if (__rpc_exec_stage == __RpcExecStage.Client && (networkManager.IsClient || networkManager.IsHost))
                {
                    string stringTest = "CLIENT - Test (isClient)";
                    Plugin.Log.LogInfo($"{stringTest}");
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void TestServerRpc()
        {
            NetworkManager networkManager = base.NetworkManager;

            if (__rpc_exec_stage == __RpcExecStage.Server && (networkManager.IsHost || networkManager.IsServer))
                Plugin.Log.LogInfo("TEST - Server (isHost/isServer)");
            else if (__rpc_exec_stage != __RpcExecStage.Server && (networkManager.IsHost || networkManager.IsServer))
            {
                string stringTest = "TEST - isHost/isServer (exec stage not server)";
                Plugin.Log.LogInfo($"{stringTest}");
            }
            else
            {
                Plugin.Log.LogInfo("no conditions met");
            }

            TestClientRpc();
        }

        public void Test()
        {
            Plugin.Log.LogInfo("Test() original method - Test");
            TestServerRpc();

        }

        //Load New Node SYNC

        [ServerRpc(RequireOwnership = false)]
        public void nodeLoadServerRpc(string nodeName, string terminalEvent)
        {
            NetworkManager networkManager = base.NetworkManager;

            if (__rpc_exec_stage == __RpcExecStage.Server && (networkManager.IsHost || networkManager.IsServer))
            {
                Plugin.Log.LogInfo($"Server: Attempting to sync {nodeName} / {terminalEvent} between players...");
            }

            else if (__rpc_exec_stage != __RpcExecStage.Server && (networkManager.IsHost || networkManager.IsServer))
            {
                Plugin.Log.LogInfo($"Exec stage not server");
            }
            else
            {
                Plugin.Log.LogInfo("no conditions met");
            }

            nodeLoadClientRpc(nodeName, terminalEvent);
        }

        [ClientRpc]
        public void nodeLoadClientRpc(string nodeName, string terminalEvent)
        {
            NetworkManager networkManager = base.NetworkManager;
            if ((object)networkManager != null && networkManager.IsListening)
            {
                if (__rpc_exec_stage != __RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
                {
                    Plugin.Log.LogInfo($"Nothing needed here I think.");
                }

                if (__rpc_exec_stage == __RpcExecStage.Client && (networkManager.IsClient || networkManager.IsHost))
                {
                    //string stringTest = "TEST - isHost/isServer (exec stage not client)";
                    //string stringTest = "TEST - isHost/isServer (exec stage not client)";
                    //string stringTest = "TEST - isHost/isServer (exec stage not client)";
                    if (!netNodeSet)
                    {
                        Plugin.Log.LogInfo("Client syncing node");
                        NetHandler.Instance.syncNodes(nodeName, terminalEvent);
                    }
                    else
                    {
                        Plugin.Log.LogInfo("netNodeSet is true. setting to false and NOT duplicating code run.");
                        netNodeSet = false; //test
                        Plugin.instance.syncedNodes = false;
                        return;
                    }

                }
            }
        }

        private void syncNodes(string nodeName, string terminalEvent)
        {
            Terminal getTerminal = Object.FindObjectOfType<Terminal>();
            if (getTerminal !=null && nodeName != null && nodeName != "Always-On Display" && !Plugin.instance.syncedNodes)
            {
                Plugin.Log.LogInfo($"getTerminal: {getTerminal}");
                TerminalKeyword[] terminalKeywords = FindObjectsOfType<TerminalKeyword>();
                TerminalKeyword[] wordsList = getTerminal.terminalNodes.allKeywords;

                List<string> excludedEvents = new List<string>
                {
                    "enemies",
                    "doors",
                    "lights",
                    "alwayson",
                    "shipLightsColor",
                    "quit",
                    "leverdo",
                    "betterescan",
                    "flashlight",
                    "teleport",
                    "inversetp",
                    "vitalsUpgrade",
                    "vitals",
                    "danger",
                    "gamble",
                    "healme",
                    "fov",
                    "kickYes",
                };

                if (terminalEvent != string.Empty && !excludedEvents.Contains(terminalEvent) )
                {
                    TerminalNode dummyNode = TerminalNode.CreateInstance<TerminalNode>();
                    dummyNode.terminalEvent = terminalEvent;
                    getTerminal.RunTerminalEvents(dummyNode);
                    netNodeSet = true; //test
                    Plugin.instance.syncedNodes = true;
                }
                else
                {
                    foreach (TerminalKeyword terminalKeyword in wordsList)
                    {
                        if(terminalKeyword != null && terminalKeyword.specialKeywordResult != null)
                        {
                            if (terminalKeyword.specialKeywordResult.name == nodeName && getTerminal.currentNode != null)
                            {
                                TerminalNode dummyNode = TerminalNode.CreateInstance<TerminalNode>();
                                dummyNode.displayText = terminalKeyword.specialKeywordResult.displayText;
                                getTerminal.LoadNewNode(dummyNode);
                                Plugin.Log.LogInfo("currentNode was not null loaded text from node?");
                                netNodeSet = true; //test
                                Plugin.instance.syncedNodes = true;
                                break;
                            }
                            else if (terminalKeyword.specialKeywordResult.name == nodeName && getTerminal.currentNode == null)
                            {
                                TerminalNode dummyNode = TerminalNode.CreateInstance<TerminalNode>();
                                dummyNode.displayText = terminalKeyword.specialKeywordResult.displayText;
                                getTerminal.LoadNewNode(dummyNode);
                                Plugin.Log.LogInfo("currentNode was null - loaded text from node?");
                                netNodeSet = true; //test
                                Plugin.instance.syncedNodes = true;
                                break;
                            }

                        } 
                    }
                }
                netNodeSet = false; //test
                Plugin.instance.syncedNodes = false;
            }
            else
                Plugin.Log.LogInfo($"Client: failed to set node with node name ({nodeName})");
        }


        //Always-On Display SYNC

        [ServerRpc(RequireOwnership = false)]
        public void alwaysOnServerRpc(bool aod)
        {
            NetworkManager networkManager = base.NetworkManager;

            if (__rpc_exec_stage == __RpcExecStage.Server && (networkManager.IsHost || networkManager.IsServer))
            {
                //string stringTest = "TEST - isHost/isServer (exec stage not client)";
                //Terminal_Awake_Patch.alwaysOnDisplay = aod;
                Plugin.Log.LogInfo($"Server: telling all to set alwaysondisplay to {aod}");
            }
            else if (__rpc_exec_stage != __RpcExecStage.Server && (networkManager.IsHost || networkManager.IsServer))
            {
                Plugin.Log.LogInfo($"Exec stage not server");
            }
            else
            {
                Plugin.Log.LogInfo("no conditions met");
            }

            alwaysOnClientRpc(aod);
        }

        [ClientRpc]
        public void alwaysOnClientRpc(bool aod)
        {
            NetworkManager networkManager = base.NetworkManager;
            if ((object)networkManager != null && networkManager.IsListening)
            {
                if (__rpc_exec_stage != __RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
                {
                    Plugin.Log.LogInfo($"Nothing needed here I think.");
                }

                if (__rpc_exec_stage == __RpcExecStage.Client && (networkManager.IsClient || networkManager.IsHost))
                {
                    //string stringTest = "TEST - isHost/isServer (exec stage not client)";
                    patchTerminal = Object.FindObjectOfType<Terminal>();
                    Terminal_Awake_Patch.alwaysOnDisplay = aod;
                    if(Terminal_Awake_Patch.isTermInUse == false && Terminal_Awake_Patch.alwaysOnDisplay == true)
                        patchTerminal.StartCoroutine(patchTerminal.waitUntilFrameEndToSetActive(active: true));
                    else if (Terminal_Awake_Patch.isTermInUse == false && Terminal_Awake_Patch.alwaysOnDisplay == false)
                        patchTerminal.StartCoroutine(patchTerminal.waitUntilFrameEndToSetActive(active: false));
                    Plugin.Log.LogInfo($"Client: set alwaysondisplay to {aod}");
                }
            }
        }


        //Ship Color changes
        [ServerRpc(RequireOwnership = false)]
        public void ShipColorALLServerRpc(Color newColor, string target)
        {
            NetworkManager networkManager = base.NetworkManager;

            if (__rpc_exec_stage == __RpcExecStage.Server && (networkManager.IsHost || networkManager.IsServer))
            {
                //string stringTest = "TEST - isHost/isServer (exec stage not client)";
                GameObject.Find("Environment/HangarShip/ShipElectricLights/Area Light (3)").GetComponent<Light>().color = newColor;
                GameObject.Find("Environment/HangarShip/ShipElectricLights/Area Light (4)").GetComponent<Light>().color = newColor;
                GameObject.Find("Environment/HangarShip/ShipElectricLights/Area Light (5)").GetComponent<Light>().color = newColor;
                Plugin.Log.LogInfo($"Server: Ship Color change for all lights received. Color: {newColor} Name: {target} ");
            }
            else if (__rpc_exec_stage != __RpcExecStage.Server && (networkManager.IsHost || networkManager.IsServer))
            {
                Plugin.Log.LogInfo($"Exec stage not server");
            }
            else
            {
                Plugin.Log.LogInfo("no conditions met");
            }

            ShipColorALLClientRpc(newColor, target);
        }

        [ClientRpc]
        public void ShipColorALLClientRpc(Color newColor, string target)
        {
            NetworkManager networkManager = base.NetworkManager;
            if ((object)networkManager != null && networkManager.IsListening)
            {
                if (__rpc_exec_stage != __RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
                {
                    //string stringTest = "TEST - isHost/isServer (exec stage not client)";
                    //GameObject.Find("Environment/HangarShip/ShipElectricLights/Area Light (3)").GetComponent<Light>().color = newColor;
                    //GameObject.Find("Environment/HangarShip/ShipElectricLights/Area Light (4)").GetComponent<Light>().color = newColor;
                    //GameObject.Find("Environment/HangarShip/ShipElectricLights/Area Light (5)").GetComponent<Light>().color = newColor;
                    Plugin.Log.LogInfo($"color set already?");

                }

                if (__rpc_exec_stage == __RpcExecStage.Client && (networkManager.IsClient || networkManager.IsHost))
                {
                    //string stringTest = "TEST - isHost/isServer (exec stage not client)";
                    GameObject.Find("Environment/HangarShip/ShipElectricLights/Area Light (3)").GetComponent<Light>().color = newColor;
                    GameObject.Find("Environment/HangarShip/ShipElectricLights/Area Light (4)").GetComponent<Light>().color = newColor;
                    GameObject.Find("Environment/HangarShip/ShipElectricLights/Area Light (5)").GetComponent<Light>().color = newColor;
                    Plugin.Log.LogInfo($"Client: Ship Color change for all lights received. Color: {newColor} Name: {target} ");
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void ShipColorFRONTServerRpc(Color newColor, string target)
        {
            NetworkManager networkManager = base.NetworkManager;

            if (__rpc_exec_stage == __RpcExecStage.Server && (networkManager.IsHost || networkManager.IsServer))
            {
                //string stringTest = "TEST - isHost/isServer (exec stage not client)";
                GameObject.Find("Environment/HangarShip/ShipElectricLights/Area Light (3)").GetComponent<Light>().color = newColor;
                Plugin.Log.LogInfo($"Server: Ship Color change received for front lights. Color: {newColor} Name: {target} ");
            }
            else if (__rpc_exec_stage != __RpcExecStage.Server && (networkManager.IsHost || networkManager.IsServer))
            {
                Plugin.Log.LogInfo($"Exec stage not server");
            }
            else
            {
                Plugin.Log.LogInfo("no conditions met");
            }

            ShipColorFRONTClientRpc(newColor, target);
        }

        [ClientRpc]
        public void ShipColorFRONTClientRpc(Color newColor, string target)
        {
            NetworkManager networkManager = base.NetworkManager;
            if ((object)networkManager != null && networkManager.IsListening)
            {
                if (__rpc_exec_stage != __RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
                {
                    //string stringTest = "TEST - isHost/isServer (exec stage not client)";
                    //GameObject.Find("Environment/HangarShip/ShipElectricLights/Area Light (3)").GetComponent<Light>().color = newColor;
                    //GameObject.Find("Environment/HangarShip/ShipElectricLights/Area Light (4)").GetComponent<Light>().color = newColor;
                    //GameObject.Find("Environment/HangarShip/ShipElectricLights/Area Light (5)").GetComponent<Light>().color = newColor;
                    Plugin.Log.LogInfo($"color set already?");

                }

                if (__rpc_exec_stage == __RpcExecStage.Client && (networkManager.IsClient || networkManager.IsHost))
                {
                    //string stringTest = "TEST - isHost/isServer (exec stage not client)";
                    GameObject.Find("Environment/HangarShip/ShipElectricLights/Area Light (3)").GetComponent<Light>().color = newColor;
                    Plugin.Log.LogInfo($"Client: Ship Color change received for front lights. Color: {newColor} Name: {target} ");
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void ShipColorMIDServerRpc(Color newColor, string target)
        {
            NetworkManager networkManager = base.NetworkManager;

            if (__rpc_exec_stage == __RpcExecStage.Server && (networkManager.IsHost || networkManager.IsServer))
            {
                //string stringTest = "TEST - isHost/isServer (exec stage not client)";
                GameObject.Find("Environment/HangarShip/ShipElectricLights/Area Light (4)").GetComponent<Light>().color = newColor;
                Plugin.Log.LogInfo($"Server: Ship Color change received for middle lights. Color: {newColor} Name: {target} ");
            }
            else if (__rpc_exec_stage != __RpcExecStage.Server && (networkManager.IsHost || networkManager.IsServer))
            {
                Plugin.Log.LogInfo($"Exec stage not server");
            }
            else
            {
                Plugin.Log.LogInfo("no conditions met");
            }

            ShipColorMIDClientRpc(newColor, target);
        }

        [ClientRpc]
        public void ShipColorMIDClientRpc(Color newColor, string target)
        {
            NetworkManager networkManager = base.NetworkManager;
            if ((object)networkManager != null && networkManager.IsListening)
            {
                if (__rpc_exec_stage != __RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
                {
                    //string stringTest = "TEST - isHost/isServer (exec stage not client)";
                    //GameObject.Find("Environment/HangarShip/ShipElectricLights/Area Light (3)").GetComponent<Light>().color = newColor;
                    //GameObject.Find("Environment/HangarShip/ShipElectricLights/Area Light (4)").GetComponent<Light>().color = newColor;
                    //GameObject.Find("Environment/HangarShip/ShipElectricLights/Area Light (5)").GetComponent<Light>().color = newColor;
                    Plugin.Log.LogInfo($"color set already?");

                }

                if (__rpc_exec_stage == __RpcExecStage.Client && (networkManager.IsClient || networkManager.IsHost))
                {
                    //string stringTest = "TEST - isHost/isServer (exec stage not client)";
                    GameObject.Find("Environment/HangarShip/ShipElectricLights/Area Light (4)").GetComponent<Light>().color = newColor;
                    Plugin.Log.LogInfo($"Client: Ship Color change received for middle lights. Color: {newColor} Name: {target} ");
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void ShipColorBACKServerRpc(Color newColor, string target)
        {
            NetworkManager networkManager = base.NetworkManager;

            if (__rpc_exec_stage == __RpcExecStage.Server && (networkManager.IsHost || networkManager.IsServer))
            {
                //string stringTest = "TEST - isHost/isServer (exec stage not client)";
                GameObject.Find("Environment/HangarShip/ShipElectricLights/Area Light (5)").GetComponent<Light>().color = newColor;
                Plugin.Log.LogInfo($"Server: Ship Color change received for back lights. Color: {newColor} Name: {target} ");
            }
            else if (__rpc_exec_stage != __RpcExecStage.Server && (networkManager.IsHost || networkManager.IsServer))
            {
                Plugin.Log.LogInfo($"Exec stage not server");
            }
            else
            {
                Plugin.Log.LogInfo("no conditions met");
            }

            ShipColorBACKClientRpc(newColor, target);
        }

        [ClientRpc]
        public void ShipColorBACKClientRpc(Color newColor, string target)
        {
            NetworkManager networkManager = base.NetworkManager;
            if ((object)networkManager != null && networkManager.IsListening)
            {
                if (__rpc_exec_stage != __RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
                {
                    //string stringTest = "TEST - isHost/isServer (exec stage not client)";
                    //GameObject.Find("Environment/HangarShip/ShipElectricLights/Area Light (3)").GetComponent<Light>().color = newColor;
                    //GameObject.Find("Environment/HangarShip/ShipElectricLights/Area Light (4)").GetComponent<Light>().color = newColor;
                    //GameObject.Find("Environment/HangarShip/ShipElectricLights/Area Light (5)").GetComponent<Light>().color = newColor;
                    Plugin.Log.LogInfo($"color set already?");

                }

                if (__rpc_exec_stage == __RpcExecStage.Client && (networkManager.IsClient || networkManager.IsHost))
                {
                    //string stringTest = "TEST - isHost/isServer (exec stage not client)";
                    GameObject.Find("Environment/HangarShip/ShipElectricLights/Area Light (5)").GetComponent<Light>().color = newColor;
                    Plugin.Log.LogInfo($"Client: Ship Color change received for back lights. Color: {newColor} Name: {target} ");
                }
            }
        }


        //Flashlights

        [ServerRpc(RequireOwnership = false)]
        public void FlashColorServerRpc(Color newColor, string colorName, ulong playerID, string playerName)
        {
            NetworkManager networkManager = base.NetworkManager;

            if (__rpc_exec_stage == __RpcExecStage.Server && (networkManager.IsHost || networkManager.IsServer))
            {
                //string stringTest = "TEST - isHost/isServer (exec stage not client)";
                Plugin.Log.LogInfo($"Server: Flashlight Color change received for {playerName}({playerID}). Color: {newColor} - {colorName} ");
                setFlash(newColor, colorName, playerID, playerName);
            }
            else if (__rpc_exec_stage != __RpcExecStage.Server && (networkManager.IsHost || networkManager.IsServer))
            {
                Plugin.Log.LogInfo($"Exec stage not server");
            }
            else
            {
                Plugin.Log.LogInfo("no conditions met");
            }

            FlashColorClientRpc(newColor, colorName, playerID, playerName);
        }

        [ClientRpc]
        public void FlashColorClientRpc(Color newColor, string colorName, ulong playerID, string playerName)
        {
            NetworkManager networkManager = base.NetworkManager;
            if ((object)networkManager != null && networkManager.IsListening)
            {
                if (__rpc_exec_stage != __RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
                {
                    //string stringTest = "TEST - isHost/isServer (exec stage not client)";
                    //GameObject.Find("Environment/HangarShip/ShipElectricLights/Area Light (3)").GetComponent<Light>().color = newColor;
                    //GameObject.Find("Environment/HangarShip/ShipElectricLights/Area Light (4)").GetComponent<Light>().color = newColor;
                    //GameObject.Find("Environment/HangarShip/ShipElectricLights/Area Light (5)").GetComponent<Light>().color = newColor;
                    Plugin.Log.LogInfo($"color set already?");

                }

                if (__rpc_exec_stage == __RpcExecStage.Client && (networkManager.IsClient || networkManager.IsHost))
                {
                    //string stringTest = "TEST - isHost/isServer (exec stage not client)";
                    setFlash(newColor, colorName, playerID, playerName);
                    Plugin.Log.LogInfo($"Client: Flashlight Color change received for {playerName}({playerID}). Color: {newColor} - {colorName} ");
                }
            }
        }

        private void setFlash(Color newColor, string colorName, ulong playerID, string playerName)
        {
            //Plugin.Log.LogInfo($"Finding ObjectsOfType GrabbableObject");
            GrabbableObject[] objectsOfType = Object.FindObjectsOfType<GrabbableObject>();
            //Plugin.Log.LogInfo($"setting getMyFlash to null grabbableobject");
            GrabbableObject getMyFlash = null; // Initialize to null outside the loop
           //Plugin.Log.LogInfo($"for each new grabbable object in objects of type");
            foreach (GrabbableObject thisFlash in objectsOfType)
            {

                //Plugin.Log.LogInfo($"checking grabbable object for playername and flashlight properties");
                if (thisFlash.playerHeldBy != null)  //nesting
                {
                    if (thisFlash.playerHeldBy.playerUsername == playerName && thisFlash.gameObject.name == "FlashlightItem(Clone)") //explicitly set object name to avoid issues
                    {
                        //Plugin.Log.LogInfo($"found a matching object");
                        //Plugin.Log.LogInfo($"[FOUND OBJECT] Held by: {thisFlash.playerHeldBy} - Name {thisFlash.gameObject.name}");
                        getMyFlash = thisFlash;
                        break;
                    }
                }
                else
                {
                    //Plugin.Log.LogInfo($"[OBJECT] Held by: {thisFlash.playerHeldBy} - Name {thisFlash.gameObject.name}");
                }
            }

            // Move the null check outside the loop
            if (getMyFlash != null)
            {
                Plugin.Log.LogInfo($"getMyFlash is not null, getting/setting components");

                // Use TryGetComponent to safely get the FlashlightItem component
                if (getMyFlash.gameObject.TryGetComponent<FlashlightItem>(out FlashlightItem flashlightItem))
                {
                    if (flashlightItem.flashlightBulb != null && flashlightItem.flashlightBulbGlow != null)
                    {
                        flashlightItem.flashlightBulb.color = newColor;
                        flashlightItem.flashlightBulbGlow.color = newColor;
                        Plugin.Log.LogInfo($"Setting flashlight color to {colorName} for player {playerName}");
                        Plugin.instance.fSuccess = true;

                        if (StartOfRound.Instance.allPlayerScripts[playerID].helmetLight)
                        {
                            StartOfRound.Instance.allPlayerScripts[playerID].helmetLight.color = newColor;
                            //GameNetworkManager.Instance.localPlayerController.helmetLight.color = flashlightColor;
                            Plugin.instance.hSuccess = true;
                        }
                    }
                    else
                    {
                        Plugin.Log.LogInfo($"flashlightBulb or flashlightBulbGlow is null on {getMyFlash.gameObject}");
                    }
                }
                else
                {
                    Plugin.Log.LogInfo($"FlashlightItem component not found on {getMyFlash.gameObject}");
                }
            }
        }

//DO NOT REMOVE
public override void OnNetworkSpawn()
{

    if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
        Instance?.gameObject.GetComponent<NetworkObject>().Despawn();
    Instance = this;

    base.OnNetworkSpawn();
}


}
}


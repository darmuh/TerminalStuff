using GameNetcodeStuff;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using Unity.Netcode;
using UnityEngine;
using static TerminalApi.TerminalApi;
using static TerminalStuff.AllMyTerminalPatches;
using Object = UnityEngine.Object;

namespace TerminalStuff
{
    public class NetHandler : NetworkBehaviour
    {

        public static NetHandler Instance { get; private set; }
        public static Terminal patchTerminal = null;
        public static bool netNodeSet = false;
        public static bool arraySender = false;
        public bool endFlashRainbow = false;
        internal static TerminalNode netNode = CreateTerminalNode("", true);

        //Load New Node SYNC

        [ServerRpc(RequireOwnership = false)]
        public void NodeLoadServerRpc(string nodeName, string nodeText, int nodeNumber = -1)
        {
            NetworkManager networkManager = base.NetworkManager;
            if (netNodeSet && (networkManager.IsHost || networkManager.IsServer))
            {
                Plugin.MoreLogs("RPC called from host, sending to client RPC ");
                if (nodeNumber != -1)
                    NodeLoadClientRpc(nodeName, nodeText, true, nodeNumber);
                else
                    NodeLoadClientRpc(nodeName, nodeText, true);
                return;
            }
            else if (!netNodeSet && networkManager.IsHost || networkManager.IsServer)
            {
                Plugin.MoreLogs($"Host: attempting to sync node {nodeName}/{nodeNumber}");
                if (nodeNumber != -1)
                    SyncNodes(nodeName, nodeText, nodeNumber);
                else
                    SyncNodes(nodeName, nodeText);
            }
            else
            {
                Plugin.MoreLogs($"Server: This should only be coming from clients");
                if (nodeNumber != -1)
                    NodeLoadClientRpc(nodeName, nodeText, true, nodeNumber);
                else
                    NodeLoadClientRpc(nodeName, nodeText, true);
            }

            Plugin.MoreLogs("Server: Attempting to sync nodes between clients.");
        }

        [ClientRpc]
        public void NodeLoadClientRpc(string nodeName, string nodeText, bool fromHost, int nodeNumber = -1)
        {
            NetworkManager networkManager = base.NetworkManager;
            if(fromHost && (networkManager.IsHost || networkManager.IsServer))
            {
                NetNodeReset(false);
                Plugin.MoreLogs("Node detected coming from host, resetting nNS and ending RPC");
                return;
            }

            if (!netNodeSet)
            {
                Plugin.MoreLogs($"Client: attempting to sync node, {nodeName}/{nodeNumber}");
                if (nodeNumber != -1)
                    SyncNodes(nodeName, nodeText, nodeNumber);
                else
                    SyncNodes(nodeName, nodeText); 
            }
            else
            {
                Plugin.MoreLogs("Client: netNodeSet is true, no sync required.");
                NetNodeReset(false);
                return;
            }
        }

        internal static bool NetNodeReset(bool set)
        {
            netNodeSet = set;
            return netNodeSet;
        }

        private static void SyncViewNodeWithNum(TerminalNode node, int nodeNumber)
        {
            if (nodeNumber == 1) // cams
            {
                ViewCommands.TermCamsEvent(out string displayText);
                node.displayText = displayText;
                return;
            }
            else if (nodeNumber == 2) //overlay
            {
                ViewCommands.OverlayTermEvent(out string displayText);
                node.displayText = displayText;
                return;
            }
            else if (nodeNumber == 3) //minimap
            {
                ViewCommands.MiniMapTermEvent(out string displayText);
                node.displayText = displayText;
                return;
            }
            else if (nodeNumber == 4) //minicams
            {
                ViewCommands.MiniCamsTermEvent(out string displayText);
                node.displayText = displayText;
                return;
            }
            else if (nodeNumber == 5) //map
            {
                ViewCommands.TermMapEvent(out string displayText);
                node.displayText = displayText;
                return;
            }
            else
                Plugin.MoreLogs("No matching views detected");
        }

        private void SyncNodes(string nodeName, string nodeText, int nodeNumber = -1)
        {
            
            TerminalNode node = Object.FindObjectsOfType<TerminalNode>().FirstOrDefault(obj => obj.name == nodeName);

            NetNodeReset(true);

            if (nodeNumber != -1 && nodeNumber <= ViewCommands.termViewNodes.Count)
            {
                TerminalNode viewNode = ViewCommands.termViewNodes[nodeNumber];
                //viewNode.displayText = nodeText;
                Plugin.Terminal.LoadNewNode(viewNode);
                SyncViewNodeWithNum(viewNode, nodeNumber);
                ViewCommands.DisplayTextUpdater(out string newText);
                viewNode.displayText = newText;
                Plugin.MoreLogs($"Attempting to load {nodeName}, ViewNode: {nodeNumber}");
            }
            else if (MoreCommands.infoOnlyNodes.Contains(node) || MoreCommands.otherActionNodes.Contains(node) || ShipControls.shipControlNodes.Contains(node))
            {
                netNode.displayText = nodeText;
                Plugin.Terminal.LoadNewNode(netNode);
                Plugin.MoreLogs($"Attempting to load {nodeName}'s displayText:\n {nodeText}");
            }
            else
            {
                netNode.displayText = nodeText;
                Plugin.Terminal.LoadNewNode(netNode);
                Plugin.MoreLogs($"{nodeName} not matching known nodes. Only displaying text:\n{nodeText}");
            }

            NetNodeReset(false);

        }

        [ServerRpc(RequireOwnership = false)]
        public void SyncDropShipServerRpc()
        {
            Plugin.MoreLogs($"Server: Attempting to sync dropship between players...");
            SyncDropShipClientRpc();
        }

        [ClientRpc]
        public void SyncDropShipClientRpc()
        {
            NetworkManager networkManager = base.NetworkManager;
            if (networkManager.IsHost || networkManager.IsServer)
            {
                Plugin.MoreLogs("Syncing dropship from host");
                int[] itemsOrdered = Plugin.Terminal.orderedItemsFromTerminal.ToArray();
                SendItemsToAllServerRpc(itemsOrdered);
            }
            
        }

        [ServerRpc(RequireOwnership = true)]
        public void SendItemsToAllServerRpc(int[] itemsOrdered)
        {
            Plugin.MoreLogs("Server: Sending items to clients...");
            SendItemsToAllClientRpc(itemsOrdered);
        }
        [ClientRpc]
        public void SendItemsToAllClientRpc(int[] itemsOrdered)
        {
            NetworkManager networkManager = base.NetworkManager;
            if (!networkManager.IsHost || !networkManager.IsServer)
            {
                Plugin.MoreLogs("Client: Converting item list to terminal...");
                List<int> receivedList = new List<int>(itemsOrdered);
                Plugin.Terminal.orderedItemsFromTerminal = receivedList;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void SyncCreditsServerRpc(int newCreds, int items)
        {
            Plugin.MoreLogs("Server: syncing credits and items...");
            SyncCreditsClientRpc(newCreds, items);
        }
        [ClientRpc]
        public void SyncCreditsClientRpc(int newCreds, int items)
        {
            
            NetworkManager networkManager = base.NetworkManager;
            if (networkManager.IsHost || networkManager.IsServer)
            {
                Plugin.Terminal.SyncGroupCreditsServerRpc(newCreds, items);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void StartAoDServerRpc(bool aod)
        {
            Plugin.MoreLogs($"Server: syncing alwaysondisplay to {aod}");
            AoDClientRpc(aod);
        }

        [ServerRpc(RequireOwnership = false)]
        public void AoDServerRpc(bool aod)
        {
            Plugin.MoreLogs($"Server: syncing alwaysondisplay to {aod}");
            AoDClientRpc(aod);
        }
        [ClientRpc]
        public void AoDClientRpc(bool aod)
        {
            Plugin.MoreLogs($"Client: setting alwaysondisplay to {aod}");
            TerminalStartPatch.alwaysOnDisplay = aod;
            if (TerminalStartPatch.isTermInUse == false && aod == true)
                TerminalStartPatch.ToggleScreen(aod);
            else if (TerminalStartPatch.isTermInUse == false && aod == false)
                TerminalStartPatch.ToggleScreen(aod);   
        }


        //Ship Color changes
        [ServerRpc(RequireOwnership = false)]
        public void ShipColorALLServerRpc(Color newColor, string target)
        {
            ShipColorALLClientRpc(newColor, target);
        }

        [ClientRpc]
        public void ShipColorALLClientRpc(Color newColor, string target)
        {
            GameObject.Find("Environment/HangarShip/ShipElectricLights/Area Light (3)").GetComponent<Light>().color = newColor;
            GameObject.Find("Environment/HangarShip/ShipElectricLights/Area Light (4)").GetComponent<Light>().color = newColor;
            GameObject.Find("Environment/HangarShip/ShipElectricLights/Area Light (5)").GetComponent<Light>().color = newColor;
            Plugin.MoreLogs($"Client: Ship Color change for all lights received. Color: {newColor} Name: {target} ");
        }

        [ServerRpc(RequireOwnership = false)]
        public void ShipColorFRONTServerRpc(Color newColor, string target)
        {
            ShipColorFRONTClientRpc(newColor, target);
        }

        [ClientRpc]
        public void ShipColorFRONTClientRpc(Color newColor, string target)
        {
            GameObject.Find("Environment/HangarShip/ShipElectricLights/Area Light (3)").GetComponent<Light>().color = newColor;
            Plugin.MoreLogs($"Client: Ship Color change received for front lights. Color: {newColor} Name: {target} ");
        }

        [ServerRpc(RequireOwnership = false)]
        public void ShipColorMIDServerRpc(Color newColor, string target)
        {
            Plugin.MoreLogs("serverRpc called");
            ShipColorMIDClientRpc(newColor, target);
        }

        [ClientRpc]
        public void ShipColorMIDClientRpc(Color newColor, string target)
        {
            GameObject.Find("Environment/HangarShip/ShipElectricLights/Area Light (4)").GetComponent<Light>().color = newColor;
            Plugin.MoreLogs($"Client: Ship Color change received for middle lights. Color: {newColor} Name: {target} ");
        }

        [ServerRpc(RequireOwnership = false)]
        public void ShipColorBACKServerRpc(Color newColor, string target)
        {
            Plugin.MoreLogs("serverRpc called");
            ShipColorBACKClientRpc(newColor, target);
        }

        [ClientRpc]
        public void ShipColorBACKClientRpc(Color newColor, string target)
        {
            GameObject.Find("Environment/HangarShip/ShipElectricLights/Area Light (5)").GetComponent<Light>().color = newColor;
            Plugin.MoreLogs($"Client: Ship Color change received for back lights. Color: {newColor} Name: {target} ");
        }


        //Flashlights

        [ServerRpc(RequireOwnership = false)]
        public void FlashColorServerRpc(Color newColor, string colorName, ulong playerID, string playerName)
        {
            //Plugin.MoreLogs("Fcolor serverRpc called");
            FlashColorClientRpc(newColor, colorName, playerID, playerName);
        }

        [ClientRpc]
        public void FlashColorClientRpc(Color newColor, string colorName, ulong playerID, string playerName)
        {
            //Plugin.MoreLogs("Fcolor clientRpc called");
            SetFlash(newColor, playerID, playerName);
        }

        private GrabbableObject FindFlashlightObject(string playerName)
        {
            GrabbableObject[] objectsOfType = Object.FindObjectsOfType<GrabbableObject>();
            GrabbableObject getMyFlash = null;

            foreach (GrabbableObject thisFlash in objectsOfType)
            {
                if (thisFlash.playerHeldBy != null)
                {
                    if (thisFlash.playerHeldBy.playerUsername == playerName && thisFlash.gameObject.name.Contains("Flashlight"))
                    {
                        getMyFlash = thisFlash;
                        break;
                    }
                }
            }

            return getMyFlash;
        }

        private void SetFlash(Color newColor, ulong playerID, string playerName)
        {
            GrabbableObject getMyFlash = FindFlashlightObject(playerName);

            // Move the null check outside the loop
            if (getMyFlash != null)
            {
                // Use TryGetComponent to safely get the FlashlightItem component
                if (getMyFlash.gameObject.TryGetComponent<FlashlightItem>(out FlashlightItem flashlightItem))
                {
                    if (flashlightItem.flashlightBulb != null && flashlightItem.flashlightBulbGlow != null)
                    {
                        flashlightItem.flashlightBulb.color = newColor;
                        flashlightItem.flashlightBulbGlow.color = newColor;
                        Plugin.instance.fSuccess = true;

                        if (StartOfRound.Instance.allPlayerScripts[playerID].helmetLight)
                        {
                            StartOfRound.Instance.allPlayerScripts[playerID].helmetLight.color = newColor;
                            Plugin.instance.hSuccess = true;
                        }
                    }
                    else
                    {
                        Plugin.MoreLogs($"flashlightBulb or flashlightBulbGlow is null on {getMyFlash.gameObject}");
                    }
                }
                else
                {
                    Plugin.Log.LogError($"FlashlightItem component not found on {getMyFlash.gameObject}");
                }
            }
        }

        public void CycleThroughRainbowFlash()
        {

            // Start the new coroutine for the rainbow effect
            string playerName = GameNetworkManager.Instance.localPlayerController.playerUsername;
            ulong playerID = GameNetworkManager.Instance.localPlayerController.playerClientId;
            PlayerControllerB getPlayer = StartOfRound.Instance.localPlayerController;

            endFlashRainbow = false;
            StartCoroutine(RainbowFlashCoroutine(playerName, playerID, getPlayer));
            Plugin.MoreLogs($"{playerName} trying to set flashlight to rainbow mode!");

        }

        private IEnumerator RainbowFlashCoroutine(string playerName, ulong playerID, PlayerControllerB player)
        {
            GrabbableObject getMyFlash = FindFlashlightObject(playerName);
            if (getMyFlash != null)
            {
                getMyFlash.itemProperties.itemName += "(Rainbow)";

                while (!player.isPlayerDead && !endFlashRainbow)
                {
                    float rainbowSpeed = 0.4f;
                    float hue = Mathf.PingPong(Time.time * rainbowSpeed, 1f);
                    Color flashlightColor = Color.HSVToRGB(hue, 1f, 1f);

                    if (getMyFlash.isHeld && !getMyFlash.deactivated)
                    {
                        Instance.FlashColorServerRpc(flashlightColor, "rainbow", playerID, playerName);

                        // Wait for a short duration before updating the color again
                        yield return new WaitForSeconds(0.05f);
                    }
                    else
                    {
                        yield return new WaitForSeconds(0.05f);
                    }


                    if (StartOfRound.Instance.allPlayersDead || getMyFlash.insertedBattery.empty || !getMyFlash.isHeld)
                    {
                        Plugin.MoreLogs("ending flashy rainbow");
                        endFlashRainbow = true;
                    }

                }
                string returnItemName = getMyFlash.itemProperties.itemName.Replace("(Rainbow)", "");
                getMyFlash.itemProperties.itemName = returnItemName;
            }
            else
                Plugin.Log.LogError("no flashlights found");


        }


        //DO NOT REMOVE
        public override void OnNetworkSpawn()
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                if (Instance != null && Instance.gameObject != null)
                {
                    NetworkObject networkObject = Instance.gameObject.GetComponent<NetworkObject>();

                    if (networkObject != null)
                    {
                        networkObject.Despawn();
                        Plugin.Log.LogInfo("Nethandler despawned!");
                    }
                }
            }

            Instance = this;
            base.OnNetworkSpawn();
            Plugin.Log.LogInfo("Nethandler Spawned!");
        }


    }
}


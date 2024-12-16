using GameNetcodeStuff;
using OpenLib.Events;
using System.Collections;
using System.Collections.Generic;
using TerminalStuff.EventSub;
using TerminalStuff.PluginCore;
using TerminalStuff.SpecialStuff;
using Unity.Netcode;
using UnityEngine;
using static OpenLib.CoreMethods.AddingThings;
using static OpenLib.CoreMethods.LogicHandling;
using static TerminalStuff.EventSub.TerminalStart;

namespace TerminalStuff
{
    public class NetHandler : NetworkBehaviour
    {
        public static Events.CustomEvent ShipReset = new();
        internal static NetHandler Instance { get; private set; }
        internal static Terminal patchTerminal = null;
        internal static bool netNodeSet = false;
        internal bool endFlashRainbow = false;
        internal static TerminalNode netNode = CreateDummyNode("", true, "");
        internal static bool rainbowFlashEnum = false;

        //Load New Node SYNC

        [ServerRpc(RequireOwnership = false)]
        internal void NodeLoadServerRpc(string topRightText, string nodeName, string nodeText, int nodeNumber = -1)
        {
            NetworkManager networkManager = base.NetworkManager;
            if (netNodeSet && (networkManager.IsHost || networkManager.IsServer))
            {
                Plugin.MoreLogs("RPC called from host, sending to client RPC ");
                NodeLoadClientRpc(topRightText, nodeName, nodeText, true, nodeNumber);
                return;
            }
            else if (!netNodeSet && networkManager.IsHost || networkManager.IsServer)
            {
                //if (!Plugin.instance.Terminal.terminalUIScreen.gameObject.activeSelf)
                //return;

                Plugin.MoreLogs($"Host: attempting to sync node {nodeName}/{nodeNumber}");
                SyncNodes(topRightText, nodeName, nodeText, nodeNumber);
            }
            else
            {
                Plugin.MoreLogs($"Server: This should only be coming from clients");
                NodeLoadClientRpc(topRightText, nodeName, nodeText, true, nodeNumber);
            }

            Plugin.MoreLogs("Server: Attempting to sync nodes between clients.");
        }

        [ClientRpc]
        internal void NodeLoadClientRpc(string topRightText, string nodeName, string nodeText, bool fromHost, int nodeNumber = -1)
        {
            //if (!Plugin.instance.Terminal.terminalUIScreen.gameObject.activeSelf)
            //return;

            NetworkManager networkManager = base.NetworkManager;
            if (fromHost && (networkManager.IsHost || networkManager.IsServer))
            {
                NetNodeReset(false);
                Plugin.MoreLogs("Node detected coming from host, resetting nNS and ending RPC");
                return;
            }

            if (!netNodeSet)
            {
                Plugin.MoreLogs($"Client: attempting to sync node, {nodeName}/{nodeNumber}");
                SyncNodes(topRightText, nodeName, nodeText, nodeNumber);
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

        private void SyncNodes(string topRightText, string nodeName, string nodeText, int nodeNumber = -1)
        {

            if (!TryGetFromAllNodes(nodeName, out TerminalNode node))
            {
                DefaultSync(nodeName, nodeText);
                Plugin.MoreLogs($"{nodeName} not matching known nodes. Only displaying text:\n{nodeText}");
                return;
            }
            else
            {
                NetNodeReset(true);

                if (nodeNumber != -1 && nodeNumber <= ConfigSettings.TerminalStuffMain.ListNumToString.Count)
                {
                    TerminalNode viewNode = StartofHandling.FindViewNode(nodeNumber);

                    if (viewNode == null)
                        return;

                    viewNode.displayText = ViewCommands.SyncViewNodeWithNum(nodeNumber, nodeText);

                    if (viewNode.displayText != nodeText)
                        viewNode.displayText = nodeText;

                    Plugin.instance.Terminal.LoadNewNode(viewNode);
                    //Plugin.instance.Terminal.currentNode.displayText = viewNode.displayText;
                    Plugin.MoreLogs($"Non terminal user: Attempting to load {nodeName}, ViewNode: {nodeNumber}\n {viewNode.displayText}");
                }
                else if(nodeNumber == 100 && nodeName == "ViewInsideShipCam 1")
                {
                    if (viewMonitorVanilla == null)
                        return;

                    Plugin.instance.Terminal.LoadNewNode(viewMonitorVanilla);
                    Plugin.MoreLogs($"Non terminal user: Attempting to load vanilla viewMonitor: {nodeName}");
                }
                else
                {
                    DefaultSync(nodeName, nodeText, node);
                    Plugin.MoreLogs($"Non terminal user: Attempting to load {nodeName}'s displayText:\n {nodeText}");
                }


                Plugin.instance.Terminal.topRightText.text = topRightText;
                NetNodeReset(false);
            }
        }

        private void DefaultSync(string nodeName, string nodeText, TerminalNode newNode = null)
        {
            if (newNode != null && vanillaNodes.Contains(newNode))
            {
                MoreCamStuff.CamPersistance(nodeName);
                MoreCamStuff.VideoPersist(nodeName);
                Plugin.instance.Terminal.LoadNewNode(newNode);
                return;
            }

            MoreCamStuff.CamPersistance(nodeName);
            MoreCamStuff.VideoPersist(nodeName);
            netNode.displayText = nodeText;
            Plugin.instance.Terminal.LoadNewNode(netNode);

            Plugin.MoreLogs($"Only displaying {nodeName} text.");
        }

        [ServerRpc(RequireOwnership = false)]
        internal void SyncDropShipServerRpc(bool isRefund = false)
        {
            Plugin.MoreLogs($"Server: Attempting to sync dropship between players...");
            SyncDropShipClientRpc(isRefund);
        }

        [ClientRpc]
        internal void SyncDropShipClientRpc(bool isRefund)
        {
            NetworkManager networkManager = base.NetworkManager;
            if (networkManager.IsHost || networkManager.IsServer)
            {
                if (isRefund)
                {
                    Plugin.MoreLogs("Host received refund command, clearing orderedItems list");
                    Plugin.instance.Terminal.orderedItemsFromTerminal.Clear();
                    CostCommands.storeCart.Clear();
                    return;
                }

                Plugin.MoreLogs("Host syncing dropship to storeCart after purchase");
                int[] itemsOrdered = [.. Plugin.instance.Terminal.orderedItemsFromTerminal];
                CostCommands.storeCart = Plugin.instance.Terminal.orderedItemsFromTerminal;
                SendItemsToAllServerRpc(itemsOrdered);
            }
        }

        internal static void SyncMyVideoChoiceToEveryone(string videoPlaying)
        {
            if (!ConfigSettings.NetworkedNodes.Value || !ConfigSettings.ModNetworking.Value || !ConfigSettings.VideoSync.Value)
                return;

            if (Instance == null || StartOfRound.Instance == null || StartOfRound.Instance.localPlayerController == null)
                return;

            Plugin.MoreLogs($"Sending {videoPlaying} as videoPlaying to other clients with active screens");
            Instance.SyncVideoChoiceServerRpc(((int)StartOfRound.Instance.localPlayerController.playerClientId), videoPlaying);
        }

        [ServerRpc(RequireOwnership = false)]
        internal void SyncVideoChoiceServerRpc(int fromClient, string videoPlaying)
        {
            Plugin.Spam($"Server: Attempting to sync video choice: {videoPlaying}");
            SyncVideoChoiceClientRpc(fromClient, videoPlaying);
        }

        [ClientRpc]
        internal void SyncVideoChoiceClientRpc(int fromClient, string videoPlaying)
        {
            if (!Plugin.instance.Terminal.terminalUIScreen.gameObject.activeSelf)
                return;

            if (((int)StartOfRound.Instance.localPlayerController.playerClientId) == fromClient)
            {
                Plugin.MoreLogs($"This is the client sending the video name {videoPlaying}");
                return;
            }
            else if (VideoManager.currentlyPlaying == videoPlaying)
            {
                Plugin.MoreLogs($"video already set to {videoPlaying}");
                return;
            }
            else
            {
                VideoManager.currentlyPlaying = videoPlaying;
                Plugin.MoreLogs($"currentlyPlaying set to {videoPlaying}");
                return;
            }

        }

        internal static void SyncMyCamsBoolToEveryone(bool myCams)
        {
            if (!ConfigSettings.NetworkedNodes.Value || !ConfigSettings.ModNetworking.Value)
                return;

            if (Instance == null || StartOfRound.Instance == null || StartOfRound.Instance.localPlayerController == null)
                return;

            if (Plugin.instance.activeCam == myCams)
                return;

            Instance.SyncActiveCamsBoolServerRpc(((int)StartOfRound.Instance.localPlayerController.playerClientId), myCams);
        }

        [ServerRpc(RequireOwnership = false)]
        internal void SyncActiveCamsBoolServerRpc(int fromClient, bool value)
        {

            Plugin.MoreLogs($"Server: Attempting to sync active cams bool...");
            SyncActiveCamsClientRpc(fromClient, value);
        }

        [ClientRpc]
        internal void SyncActiveCamsClientRpc(int fromClient, bool value)
        {
            if (!Plugin.instance.Terminal.terminalUIScreen.gameObject.activeSelf)
                return;

            if (((int)StartOfRound.Instance.localPlayerController.playerClientId) == fromClient)
            {
                Plugin.MoreLogs($"This is the client syncing the bool");
                return;
            }
            else if (Plugin.instance.activeCam == value)
            {
                Plugin.MoreLogs("Catching extra sync and returning");
                return;
            }

            else
            {
                Plugin.instance.activeCam = value;
                Plugin.MoreLogs($"activeCam set to {value}");
                return;
            }

        }

        [ServerRpc(RequireOwnership = false)]
        internal void SyncTerminalServerRpc(int fromClient, int otherClient)
        {
            if (fromClient == -1 || otherClient == -1)
            {
                Plugin.ERROR($"SyncTerminalServerRpc FATAL ERROR: Invalid client ID detected.\nfromClient: {fromClient}\notherClient: {otherClient}");
                return;
            }

            Plugin.MoreLogs($"Server: Client [{fromClient}] requesting terminalNode from: [{otherClient}]");
            SyncTerminalClientRpc(fromClient, otherClient);
        }

        [ClientRpc]
        internal void SyncTerminalClientRpc(int fromClient, int otherClient)
        {
            if (StartOfRound.Instance == null)
                return;
            if (StartOfRound.Instance.localPlayerController == null)
                return;

            if (((int)StartOfRound.Instance.localPlayerController.playerClientId) == fromClient)
            {
                Plugin.MoreLogs($"This is the client requesting the node");
                return;
            }
            else if (((int)StartOfRound.Instance.localPlayerController.playerClientId) == otherClient)
            {
                Plugin.MoreLogs($"This is the client the node is being requested from");
                //NetHandler.Instance.SyncActiveCamsBoolServerRpc(otherClient, netCamStatus);
                StartofHandling.SyncTerminal(Plugin.instance.Terminal.currentNode);
                return;
            }
            else
            {
                Plugin.MoreLogs("This client is neither the client requesting node status or the client being requested to provide it.");
                return;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        internal void AskUpgradeStatusServerRpc()
        {
            Plugin.MoreLogs($"Server: Client requesting Update of upgrades status for all clients");
            AskUpgradeStatusClientRpc();
        }

        [ClientRpc]
        internal void AskUpgradeStatusClientRpc()
        {
            NetworkManager networkManager = base.NetworkManager;
            if (networkManager.IsHost || networkManager.IsServer)
            {
                foreach (string name in SaveManager.AllUpgradesUnlocked)
                    UpgradeStatusServerRpc(name);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        internal void GetTravelHistoryServerRpc()
        {
            Plugin.MoreLogs($"Server: Client requesting Travel History status update for all clients");
            GetTravelHistoryClientRpc();
        }

        [ClientRpc]
        internal void GetTravelHistoryClientRpc()
        {
            NetworkManager networkManager = base.NetworkManager;
            if (networkManager.IsHost || networkManager.IsServer)
            {
                foreach (string name in MoonsPlus.MoonsVisited)
                    TravelHistoryServerRpc(name);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        internal void TravelHistoryServerRpc(string moonName)
        {
            Plugin.MoreLogs($"Server: Adding {moonName} to travel history for all clients");
            TravelHistoryClientRpc(moonName);
        }

        [ClientRpc]
        internal void TravelHistoryClientRpc(string moonName)
        {
            if (MoonsPlus.MoonsVisited.Contains(moonName))
                return;

            MoonsPlus.MoonsVisited.Add(moonName);

            Plugin.MoreLogs($"Client: Adding {moonName} to travel history for all clients");
            MoonsPlus.UpdateMoonTravelHistory(moonName);
        }

        [ServerRpc(RequireOwnership = false)]
        internal void UpgradeStatusServerRpc(string upgradeName)
        {
            Plugin.MoreLogs($"Server: Updating {upgradeName} upgrade status for all clients");
            UpgradeStatusClientRpc(upgradeName);
        }

        [ClientRpc]
        internal void UpgradeStatusClientRpc(string upgradeName)
        {
            if(!SaveManager.AllUpgradesUnlocked.Contains(upgradeName))
                SaveManager.AllUpgradesUnlocked.Add(upgradeName);

            Plugin.MoreLogs($"Client: Updating {upgradeName} upgrade status for all clients");
            CostCommands.UpdateUnlockStatus();
        }

        [ServerRpc(RequireOwnership = true)]
        internal void SendItemsToAllServerRpc(int[] itemsOrdered)
        {
            Plugin.MoreLogs("Server: Sending itemsOrdered to clients...");
            SendItemsToAllClientRpc(itemsOrdered);
        }
        [ClientRpc]
        internal void SendItemsToAllClientRpc(int[] itemsOrdered)
        {
            NetworkManager networkManager = base.NetworkManager;
            if (!networkManager.IsHost || !networkManager.IsServer)
            {
                Plugin.MoreLogs("Client: Setting storeCart value to host's orderedItemsFromTerminal list");
                List<int> receivedList = new(itemsOrdered);
                CostCommands.storeCart = receivedList;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        internal void SyncCreditsServerRpc(int newCreds, int items)
        {
            Plugin.MoreLogs("Server: syncing credits and items...");
            SyncCreditsClientRpc(newCreds, items);
        }
        [ClientRpc]
        internal void SyncCreditsClientRpc(int newCreds, int items)
        {

            NetworkManager networkManager = base.NetworkManager;
            if (networkManager.IsHost || networkManager.IsServer)
            {
                Plugin.instance.Terminal.SyncGroupCreditsServerRpc(newCreds, items);
                CostCommands.storeCart = Plugin.instance.Terminal.orderedItemsFromTerminal;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        internal void SyncRadarZoomServerRpc(float zoom)
        {
            Plugin.MoreLogs("Server: syncing radar zoom level...");
            SyncRadarZoomClientRpc(zoom);
        }
        [ClientRpc]
        internal void SyncRadarZoomClientRpc(float zoom)
        {
            if (ViewCommands.radarZoom == zoom)
                return;

            ViewCommands.radarZoom = zoom;

            GameStuff.TerminalMapRenderer.cam.orthographicSize = ViewCommands.radarZoom;
            Plugin.MoreLogs($"Radar Zoom set to {ViewCommands.radarZoom}");
        }

        [ServerRpc(RequireOwnership = false)]
        internal void SyncRadarMapServerRpc(int fromClient, int newTarget)
        {
            Plugin.Spam($"Server: Client ({fromClient}) attempting to sync radarmap target to {newTarget}");
            SyncRadarMapClientRpc(fromClient, newTarget);
        }

        [ClientRpc]
        internal void SyncRadarMapClientRpc(int fromClient, int newTarget)
        {
            if (((int)StartOfRound.Instance.localPlayerController.playerClientId) == fromClient)
            {
                Plugin.Spam($"This is the client updating target to {newTarget}");
                return;
            }
            else
            {

                Plugin.Spam("SyncRadarMapClientRpc called from another client");
                GameStuff.TerminalMapRenderer.StartCoroutine(GameStuff.TerminalMapRenderer.updateMapTarget(newTarget, true));
            }

        }

        [ServerRpc(RequireOwnership = false)]
        internal void StartAoDServerRpc(bool aod)
        {
            Plugin.MoreLogs($"Server: syncing alwaysondisplay to {aod}");
            AoDClientRpc(aod);
        }

        [ServerRpc(RequireOwnership = false)]
        internal void AoDServerRpc(bool aod)
        {
            Plugin.MoreLogs($"Server: syncing alwaysondisplay to {aod}");
            AoDClientRpc(aod);
        }
        [ClientRpc]
        internal void AoDClientRpc(bool aod)
        {
            Plugin.MoreLogs($"Client: setting alwaysondisplay to {aod}");
            alwaysOnDisplay = aod;
            if (Plugin.instance.Terminal.terminalInUse == false)
                ToggleScreen(aod);
        }


        //Ship Color changes
        [ServerRpc(RequireOwnership = false)]
        internal void ShipColorALLServerRpc(Color newColor, string target)
        {
            ShipColorALLClientRpc(newColor, target);
        }

        [ClientRpc]
        internal void ShipColorALLClientRpc(Color newColor, string target)
        {
            GameObject.Find("Environment/HangarShip/ShipElectricLights/Area Light (3)").GetComponent<Light>().color = newColor;
            GameObject.Find("Environment/HangarShip/ShipElectricLights/Area Light (4)").GetComponent<Light>().color = newColor;
            GameObject.Find("Environment/HangarShip/ShipElectricLights/Area Light (5)").GetComponent<Light>().color = newColor;
            Plugin.MoreLogs($"Client: Ship Color change for all lights received. Color: {newColor} Name: {target} ");
        }

        [ServerRpc(RequireOwnership = false)]
        internal void ShipColorFRONTServerRpc(Color newColor, string target)
        {
            ShipColorFRONTClientRpc(newColor, target);
        }

        [ClientRpc]
        internal void ShipColorFRONTClientRpc(Color newColor, string target)
        {
            GameObject.Find("Environment/HangarShip/ShipElectricLights/Area Light (3)").GetComponent<Light>().color = newColor;
            Plugin.MoreLogs($"Client: Ship Color change received for front lights. Color: {newColor} Name: {target} ");
        }

        [ServerRpc(RequireOwnership = false)]
        internal void ShipColorMIDServerRpc(Color newColor, string target)
        {
            Plugin.MoreLogs("serverRpc called");
            ShipColorMIDClientRpc(newColor, target);
        }

        [ClientRpc]
        internal void ShipColorMIDClientRpc(Color newColor, string target)
        {
            GameObject.Find("Environment/HangarShip/ShipElectricLights/Area Light (4)").GetComponent<Light>().color = newColor;
            Plugin.MoreLogs($"Client: Ship Color change received for middle lights. Color: {newColor} Name: {target} ");
        }

        [ServerRpc(RequireOwnership = false)]
        internal void ShipColorBACKServerRpc(Color newColor, string target)
        {
            Plugin.MoreLogs("serverRpc called");
            ShipColorBACKClientRpc(newColor, target);
        }

        [ClientRpc]
        internal void ShipColorBACKClientRpc(Color newColor, string target)
        {
            GameObject.Find("Environment/HangarShip/ShipElectricLights/Area Light (5)").GetComponent<Light>().color = newColor;
            Plugin.MoreLogs($"Client: Ship Color change received for back lights. Color: {newColor} Name: {target} ");
        }


        //Flashlights

        [ServerRpc(RequireOwnership = false)]
        internal void FlashColorServerRpc(Color newColor, ulong playerID, string playerName)
        {
            //Plugin.MoreLogs("Fcolor serverRpc called");
            FlashColorClientRpc(newColor, playerID, playerName);
            HelmetLightColorClientRpc(newColor, playerID);
        }

        [ClientRpc]
        internal void FlashColorClientRpc(Color newColor, ulong playerID, string playerName)
        {
            if (StartOfRound.Instance.localPlayerController.playerClientId == playerID)
                return;

            //Plugin.MoreLogs("Fcolor clientRpc called");
            FlashlightItem getFlash = FindFlashlightObject(playerName);
            if (getFlash != null)
            {
                SetFlash(ref getFlash, newColor);
            }
                
            else
                Plugin.WARNING($"Unable to find flashlight of player [ {playerName} ] to set custom color [ {newColor} ]");
        }

        [ServerRpc(RequireOwnership = false)]
        internal void HelmetLightColorServerRpc(Color newColor, ulong playerID)
        {
            //Plugin.MoreLogs("Fcolor serverRpc called");
            HelmetLightColorClientRpc(newColor, playerID);
        }

        [ClientRpc]
        internal void HelmetLightColorClientRpc(Color newColor, ulong playerID)
        {
            if (StartOfRound.Instance.localPlayerController.playerClientId == playerID)
                return;

            SetHelmetLight(newColor, playerID);
        }

        private FlashlightItem FindFlashlightObject(string playerName)
        {
            List<FlashlightItem> allflashlights = [.. FindObjectsByType<FlashlightItem>(sortMode:FindObjectsSortMode.None)];
            FlashlightItem flashlightItem = null;

            if (allflashlights.Count == 0)
                return flashlightItem;

            foreach (FlashlightItem thisFlash in allflashlights)
            {
                if (thisFlash.playerHeldBy != null)
                {
                    if (thisFlash.playerHeldBy.playerUsername == playerName && thisFlash.gameObject.name.Contains("Flashlight"))
                    {
                        flashlightItem = thisFlash.GetComponent<FlashlightItem>();
                        break;
                    }
                }
            }

            return flashlightItem;
        }

        internal static void SetFlash(ref FlashlightItem flashlightItem, Color newColor)
        {

            // Move the null check outside the loop
            if (flashlightItem == null)
                return;

            if (flashlightItem.flashlightBulb != null && flashlightItem.flashlightBulbGlow != null)
            {
                flashlightItem.flashlightBulb.color = newColor;
                flashlightItem.flashlightBulbGlow.color = newColor;
            }
            else
            {
                Plugin.WARNING($"flashlightBulb or flashlightBulbGlow is null");
            }
        }

        internal static void SetHelmetLight(Color newColor, ulong playerID)
        {
            if (StartOfRound.Instance.allPlayerScripts.Length <= (int)playerID)
                return;

            if (StartOfRound.Instance.allPlayerScripts[(int)playerID].helmetLight)
            {
                StartOfRound.Instance.allPlayerScripts[(int)playerID].helmetLight.color = newColor;
            }
        }

        internal void CycleThroughRainbowFlash()
        {
            if (rainbowFlashEnum)
                return;

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
            if (rainbowFlashEnum)
                yield break;

            rainbowFlashEnum = true;
            Plugin.Spam("RainbowFlashCoroutine!");

            FlashlightItem flashlight = FindFlashlightObject(playerName);
            if (flashlight != null)
            {
                flashlight.itemProperties.itemName += "(Rainbow)";

                while (!player.isPlayerDead && !endFlashRainbow)
                {
                    float rainbowSpeed = 0.4f;
                    float hue = Mathf.PingPong(Time.time * rainbowSpeed, 1f);
                    Color flashlightColor = Color.HSVToRGB(hue, 1f, 1f);

                    SetFlash(ref flashlight, flashlightColor);
                    SetHelmetLight(flashlightColor, playerID);
                    Instance.FlashColorServerRpc(flashlightColor, playerID, playerName);

                    // Wait for a short duration before updating the color again
                    yield return new WaitForSeconds(0.05f);

                    if (StartOfRound.Instance.allPlayersDead || flashlight.insertedBattery.empty || !flashlight.isHeld || !flashlight.flashlightBulb.enabled || !ColorCommands.RainbowFlash)
                    {
                        Plugin.MoreLogs("ending flashy rainbow");
                        flashlight.itemProperties.itemName = flashlight.itemProperties.itemName.Replace("(Rainbow)", "");
                        endFlashRainbow = true;
                    }
                }
            }
            else
                Plugin.Log.LogWarning("no flashlights found for rainbow!");

            rainbowFlashEnum = false;
        }

        //QuickRestart RPC
        [ServerRpc(RequireOwnership = true)]
        internal void QuickRestartServerRpc()
        {
            QuickRestartClientRpc();
        }

        [ClientRpc]
        internal void QuickRestartClientRpc()
        {
            GameNetworkManager.Instance.localPlayerController.DropAllHeldItemsAndSync();
            if(GameNetworkManager.Instance.localPlayerController.currentTriggerInAnimationWith == Plugin.instance.Terminal.terminalTrigger)
                Plugin.instance.Terminal.QuitTerminal(); //quit terminal for terminal user

            //DeleteInventory();
            Plugin.instance.Terminal.ClearBoughtItems();

            if (StartOfRound.Instance.IsServer)
            {
                GameNetworkManager.Instance.ResetSavedGameValues();
            }

            StartOfRound.Instance.gameStats.daysSpent = 0;
            StartOfRound.Instance.gameStats.scrapValueCollected = 0;
            StartOfRound.Instance.gameStats.deaths = 0;
            StartOfRound.Instance.gameStats.allStepsTaken = 0;

            StartOfRound.Instance.ResetShip();
            StartOfRound.Instance.currentPlanetPrefab.transform.position = StartOfRound.Instance.planetContainer.transform.position;
            ShipReset.Invoke(); //public event for other mods to listen to and do things on ship reset, This has been added to openlib now

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


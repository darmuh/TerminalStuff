using GameNetcodeStuff;
using OpenLib.Events;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TerminalStuff.CommandHandling;
using TerminalStuff.EventSub;
using TerminalStuff.MoonsTweaks;
using TerminalStuff.Util;
using TerminalStuff.VisualElements;
using Unity.Netcode;
using UnityEngine;
using static OpenLib.CoreMethods.AddingThings;
using static OpenLib.CoreMethods.LogicHandling;
using static TerminalStuff.EventSub.TerminalStart;

#pragma warning disable CA1822

namespace TerminalStuff.Networking;

public class NetHandler : NetworkBehaviour
{
    public static Events.CustomEvent ShipReset = new();
    internal static NetHandler Instance { get; private set; } = null!;
    internal bool EndFlashRainbow = false;
    internal static TerminalNode NetNode = CreateDummyNode("", true, "");
    internal static bool RainbowFlashRoutine = false;

    //Load New Node SYNC
    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    internal void SyncNodesRpc(string topRightText, string nodeName, string nodeText, int nodeNumber = -1)
    {
        if (!TryGetFromAllNodes(nodeName, out TerminalNode node))
        {
            DefaultSync(nodeName, nodeText);
            Loggers.LogInfo($"{nodeName} not matching known nodes. Only displaying text:\n{nodeText}");
            return;
        }
        else
        {
            if (nodeNumber != -1 && nodeNumber <= Configs.Commands.GetSpecialCommands().ConvertAll(x => x.VerySpecialNum).Max())
            {
                TerminalNode viewNode = StartofHandling.FindViewNode(nodeNumber);

                if (viewNode == null)
                    return;

                viewNode.displayText = ViewCommands.SyncViewNodeWithNum(nodeNumber, nodeText);

                if (viewNode.displayText != nodeText)
                    viewNode.displayText = nodeText;

                Plugin.instance.Terminal.LoadNewNode(viewNode);
                //Plugin.instance.Terminal.currentNode.displayText = viewNode.displayText;
                Loggers.LogInfo($"Non terminal user: Attempting to load {nodeName}, ViewNode: {nodeNumber}\n {viewNode.displayText}");
            }
            else if (nodeNumber == 100 && nodeName == "ViewInsideShipCam 1")
            {
                if (viewMonitorVanilla == null)
                    return;

                Plugin.instance.Terminal.LoadNewNode(viewMonitorVanilla);
                Loggers.LogInfo($"Non terminal user: Attempting to load vanilla viewMonitor: {nodeName}");
            }
            else
            {
                DefaultSync(nodeName, nodeText, node);
                Loggers.LogInfo($"Non terminal user: Attempting to load {nodeName}'s displayText:\n {nodeText}");
            }


            Plugin.instance.Terminal.topRightText.text = topRightText;
        }
    }

    private void DefaultSync(string nodeName, string nodeText, TerminalNode newNode = null!)
    {
        if (newNode != null && vanillaNodes.Contains(newNode))
        {
            MoreCamStuff.CheckVisualPersistance(nodeName);
            Plugin.instance.Terminal.LoadNewNode(newNode);
            return;
        }

        MoreCamStuff.CheckVisualPersistance(nodeName);
        NetNode.displayText = nodeText;
        Plugin.instance.Terminal.LoadNewNode(NetNode);

        Loggers.LogInfo($"Only displaying {nodeName} text.");
    }

    [Rpc(SendTo.Owner, RequireOwnership = false)]
    internal void SyncDropShipRpc(bool isRefund)
    {
        NetworkManager networkManager = NetworkManager;
        if (networkManager.IsHost || networkManager.IsServer)
        {
            if (isRefund)
            {
                Loggers.LogInfo("Host received refund command, clearing orderedItems list");
                Plugin.instance.Terminal.orderedItemsFromTerminal.Clear();
                CostCommands.storeCart.Clear();
                return;
            }

            Loggers.LogInfo("Host syncing dropship to storeCart after purchase");
            int[] itemsOrdered = [.. Plugin.instance.Terminal.orderedItemsFromTerminal];
            CostCommands.storeCart = Plugin.instance.Terminal.orderedItemsFromTerminal;
            HostSendItemsToAllRpc(itemsOrdered);
        }
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    internal void SyncMyVideoChoiceToEveryoneRpc(string videoPlaying)
    {
        VideoManager.currentlyPlaying = videoPlaying;
        Loggers.LogInfo($"currentlyPlaying set to {videoPlaying}");
    }

    [Rpc(SendTo.Owner, RequireOwnership = false)]
    internal void GetHostTerminalRpc()
    {
        Loggers.LogInfo($"Server: Syncing current node from host.");
        StartofHandling.SyncTerminal(Plugin.instance.Terminal.currentNode);
    }


    [Rpc(SendTo.Owner, RequireOwnership = false)]
    internal void AskHostUpgradeStatusRpc()
    {
        foreach (string name in SaveManager.AllUpgradesUnlocked)
            UpgradeStatusRpc(name);
    }

    [Rpc(SendTo.Owner, RequireOwnership = false)]
    internal void AskHostTravelHistoryRpc()
    {
        foreach (string name in MoonsPlus.MoonsVisited)
            UpdateTravelHistoryRpc(name);
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    internal void UpdateTravelHistoryRpc(string moonName)
    {
        if (MoonsPlus.MoonsVisited.Contains(moonName))
            return;

        MoonsPlus.MoonsVisited.Add(moonName);

        if (GameNetworkManager.Instance.localPlayerController.IsHost)
            SaveManager.SaveTravelHistory(MoonsPlus.MoonsVisited);

        Loggers.LogInfo($"Client: Adding {moonName} to travel history");
        MoonsPlus.UpdateMoonTravelHistory(moonName);
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    internal void UpgradeStatusRpc(string upgradeName)
    {
        if (!SaveManager.AllUpgradesUnlocked.Contains(upgradeName))
            SaveManager.AllUpgradesUnlocked.Add(upgradeName);

        Loggers.LogInfo($"Client: Updating {upgradeName} upgrade status");
        CostCommands.UpdateUnlockStatus();
    }

    [Rpc(SendTo.NotMe)]
    internal void HostSendItemsToAllRpc(int[] itemsOrdered)
    {
        NetworkManager networkManager = NetworkManager;
        if (!networkManager.IsHost || !networkManager.IsServer)
        {
            Loggers.LogInfo("Client: Setting storeCart value to host's orderedItemsFromTerminal list");
            List<int> receivedList = [.. itemsOrdered];
            CostCommands.storeCart = receivedList;
        }
    }

    [Rpc(SendTo.Owner, RequireOwnership = false)]
    internal void SyncHostCreditsRpc(int newCreds, int items)
    {
        NetworkManager networkManager = NetworkManager;
        if (networkManager.IsHost || networkManager.IsServer)
        {
            Plugin.instance.Terminal.SyncGroupCreditsServerRpc(newCreds, items);
            CostCommands.storeCart = Plugin.instance.Terminal.orderedItemsFromTerminal;
        }
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    internal void SyncRadarZoomRpc(float zoom)
    {
        if (ViewCommands.radarZoom == zoom)
            return;

        ViewCommands.radarZoom = zoom;

        GameStuff.TerminalMapRenderer.cam.orthographicSize = ViewCommands.radarZoom;
        Loggers.LogInfo($"Radar Zoom set to {ViewCommands.radarZoom}");
    }

    [Rpc(SendTo.NotMe)]
    internal void SyncRadarMapRpc(int newTarget)
    {
        Loggers.LogDebug("SyncRadarMapClientRpc called from another client");
        GameStuff.TerminalMapRenderer.StartCoroutine(GameStuff.TerminalMapRenderer.updateMapTarget(newTarget, true));
    }

    [Rpc(SendTo.Owner, RequireOwnership = false)]
    internal void GetHostAlwaysOnStatusRpc()
    {
        AlwaysOnDisplaySyncRpc(AlwaysOnDisplay);
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    internal void AlwaysOnDisplaySyncRpc(bool aod)
    {
        Loggers.LogInfo($"Client: setting alwaysondisplay to {aod}");
        AlwaysOnDisplay = aod;
        if (Plugin.instance.Terminal.terminalInUse == false)
            ToggleScreen(aod);
    }

    //Ship Color changes
    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    internal void ShipColorAllRpc(Color newColor, string target)
    {
        ColorCommands.SetLightColors([ColorCommands.FrontLight1, ColorCommands.FrontLight2, ColorCommands.MidLight1, ColorCommands.MidLight2, ColorCommands.BackLight1, ColorCommands.BackLight2], newColor);
        Loggers.LogInfo($"Ship Color change for all lights received. Color: {newColor} Name: {target} ");
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    internal void ShipColorFrontRpc(Color newColor, string target)
    {
        ColorCommands.SetLightColors([ColorCommands.FrontLight1, ColorCommands.FrontLight2], newColor);
        Loggers.LogInfo($"Ship Color change received for front lights. Color: {newColor} Name: {target} ");
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    internal void ShipColorMidRpc(Color newColor, string target)
    {
        ColorCommands.SetLightColors([ColorCommands.MidLight1, ColorCommands.MidLight2], newColor);
        Loggers.LogInfo($"Ship Color change received for middle lights. Color: {newColor} Name: {target} ");
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    internal void ShipColorBackRpc(Color newColor, string target)
    {
        ColorCommands.SetLightColors([ColorCommands.BackLight1, ColorCommands.BackLight2], newColor);
        Loggers.LogInfo($"Ship Color change received for back lights. Color: {newColor} Name: {target} ");
    }

    //Flashlights
    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    internal void FlashColorRpc(Color newColor, ulong playerID)
    {
        PlayerControllerB player = StartOfRound.Instance.allPlayerScripts.FirstOrDefault(p => p.playerSteamId == playerID);

        if (player == null)
            return;

        player.helmetLight.color = newColor;
        foreach(var item in player.ItemSlots)
        {
            if(item is FlashlightItem flashlight)
            {
                flashlight.flashlightBulb.color = newColor;
                flashlight.flashlightBulbGlow.color = newColor;
            }
        }
    }

    internal void CycleThroughRainbowFlash()
    {
        if (RainbowFlashRoutine)
            return;

        EndFlashRainbow = false;
        ColorCommands.RainbowFlash = true;
        Loggers.LogInfo($"setting flashlight to rainbow mode!");
        StartCoroutine(RainbowFlashCoroutine());
    }

    private IEnumerator RainbowFlashCoroutine()
    {
        if (RainbowFlashRoutine)
            yield break;

        RainbowFlashRoutine = true;
        Loggers.LogDebug("RainbowFlashCoroutine!");

        PlayerControllerB player = StartOfRound.Instance.localPlayerController;

        if(player.ItemSlots[player.currentItemSlot] is FlashlightItem flashlight)
        {
            flashlight.itemProperties.itemName += "(Rainbow)";

            while (!player.isPlayerDead && !EndFlashRainbow)
            {
                float rainbowSpeed = 0.4f;
                float hue = Mathf.PingPong(Time.time * rainbowSpeed, 1f);
                Color flashlightColor = Color.HSVToRGB(hue, 1f, 1f);

                Instance.FlashColorRpc(flashlightColor, player.playerSteamId);

                // Wait for next frame
                yield return new WaitForEndOfFrame();

                if (StartOfRound.Instance.allPlayersDead || flashlight.insertedBattery.empty || !flashlight.isHeld || !ColorCommands.RainbowFlash)
                {
                    Loggers.LogInfo("ending flashy rainbow");
                    flashlight.itemProperties.itemName = flashlight.itemProperties.itemName.Replace("(Rainbow)", "");
                    EndFlashRainbow = true;
                }
            }
        }
        else
            Plugin.Log.LogMessage("Rainbow flashlight did not find a valid flashlight item in the player's hand!");

        RainbowFlashRoutine = false;
    }

    //QuickRestart RPC
    [Rpc(SendTo.Everyone)]
    internal void QuickRestartRpc()
    {
        GameNetworkManager.Instance.localPlayerController.DropAllHeldItemsAndSync();
        if (GameNetworkManager.Instance.localPlayerController.currentTriggerInAnimationWith == Plugin.instance.Terminal.terminalTrigger)
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

    private void Awake()
    {
        Instance = this;
        Plugin.Log.LogInfo("Nethandler Spawned!");
    }

#pragma warning restore CA1822

}


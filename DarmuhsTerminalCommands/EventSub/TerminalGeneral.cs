using GameNetcodeStuff;
using TerminalStuff.Configs;
using TerminalStuff.PluginCore;
using TerminalStuff.SpecialStuff;
using static TerminalStuff.TerminalEvents;


namespace TerminalStuff.EventSub;

internal class TerminalGeneral
{
    internal static TerminalNode lastNodeFormatted = null!;
    internal static bool CancelConfirmation = false;
    internal static TerminalNode GeneralDummy = OpenLib.CoreMethods.AddingThings.CreateDummyNode("", true, "");
    internal static void OnTerminalDisable()
    {
        //Plugin.instance.Config.Reload();
        Plugin.instance.Terminal.terminalNodes.specialNodes[20] = TerminalStart.switchNodeVanilla;
        MoonsPlus.LobbyClose();
        MenuBuild.ClearMyMenustuff();
        //ConfigSettings.TerminalStuffMain.DeleteAll();
        lastText = "";
        //Plugin.ClearLists();
        //Terminal disabled, disabling ESC key listener OnDisable
    }

    internal static void OnLoadNode(TerminalNode node)
    {
        Loggers.LogDebug($"CancelConfirmation: {CancelConfirmation}");

        if (CancelConfirmation)
        {
            CancelConfirmation = false;
            GeneralDummy.displayText = node.displayText;
            Plugin.instance.Terminal.LoadNewNode(GeneralDummy);
            Loggers.LogDebug("Saving terminal user from unnecessary confirmation");
            if (StorePacksInfo.AllPacks.Count > 0)
                StorePacksInfo.CancelConfirmation();
        }


        Loggers.LogInfo($"LoadNewNode patch, nNS: {NetHandler.netNodeSet}");
        Loggers.LogDebug("Line count: " + Plugin.instance.Terminal.screenText.textComponent.textInfo.lineCount.ToString());

        if (Commands.TerminalMoonsPlus.Value && MoonsPlusConfig.OneTimePurchase.Value)
            MoonsPlus.CheckNodePurchase(node);

        if (QoLConfig.TerminalInputMaxChars.Value >= 20 && node.maxCharactersToType >= 20)
            node.maxCharactersToType = QoLConfig.TerminalInputMaxChars.Value;

        if (QoLConfig.TerminalFillEmptyText.Value == "nochange")
            return;

        if (Plugin.instance.Terminal.currentNode == Plugin.instance.Terminal.terminalNodes.specialNodes[1])
            return;

        if (Plugin.instance.Terminal.screenText.textComponent.textInfo.lineCount < 24 && node != lastNodeFormatted)
        {
            int spaceToFill = 24 - Plugin.instance.Terminal.screenText.textComponent.textInfo.lineCount;
            lastNodeFormatted = Plugin.instance.Terminal.currentNode;
            //if configitem >= 0 < 2, filltext with configitem choice
            FillText(QoLConfig.TerminalFillEmptyText.Value, ref lastNodeFormatted, spaceToFill);
        }
    }

    private static void FillText(string formatChoice, ref TerminalNode fixLength, int spaceToFill)
    {

        if (formatChoice == "fillbottom")
        {
            for (int i = 0; i < spaceToFill; i++)
            {
                fixLength.displayText += "\n";
            }
            Loggers.LogDebug("added space to bottom only");
            Plugin.instance.Terminal.LoadNewNode(fixLength);
            Loggers.LogDebug($"added {spaceToFill} lines to displayText, reloading node");
            return;
        }
        else if (formatChoice == "textmiddle")
        {
            for (int i = 0; i < spaceToFill; i++)
            {
                if (i < spaceToFill / 2)
                    fixLength.displayText = fixLength.displayText.Insert(0, "\n");
                else
                    fixLength.displayText += "\n";
            }
            Loggers.LogDebug("pushed text to middle of screen and added space to bottom");
            Plugin.instance.Terminal.LoadNewNode(fixLength);
            Loggers.LogDebug($"added {spaceToFill} lines to displayText, reloading node");
            return;
        }
        else if (formatChoice == "textbottom")
        {
            for (int i = 0; i < spaceToFill; i++)
            {
                fixLength.displayText = fixLength.displayText.Insert(0, "\n");
            }
            Loggers.LogDebug("added space to top only");
            Plugin.instance.Terminal.LoadNewNode(fixLength);
            Loggers.LogDebug($"added {spaceToFill} lines to start of displayText, reloading node");
            return;
        }
        else
        {
            Loggers.LogDebug("invalid FillText formatChoice, returning");
            return;
        }
    }

    internal static void OnLoadAffordable(TerminalNode node)
    {
        if (!Commands.TerminalRefund.Value || !ConfigSettings.ModNetworking.Value)
            return;

        if (node == null)
        {
            Loggers.WARNING("WARNING: node is null at OnLoadAffordable, using early return & dropship will not be synced!");
            return;
        }

        NetHandler.Instance.SyncDropShipServerRpc();
        Loggers.LogDebug($"items: {Plugin.instance.Terminal.orderedItemsFromTerminal.Count}");
    }

    internal static void OnSetTerminalInUse()
    {
        string setting = QoLConfig.TerminalLightBehaviour.Value;

        AlwaysOnStuff.screenSettings ??= new(QoLConfig.TerminalScreen.Value);

        if (AlwaysOnStuff.screenSettings.inUse && InUseCheck(StartOfRound.Instance.localPlayerController))
        {
            AlwaysOnStuff.SetScreenPlus(Plugin.instance.Terminal.placeableObject.inUse);
            Loggers.LogDebug($"OnSetTerminalInUse {Plugin.instance.Terminal.placeableObject.inUse}");
        }

        if (setting == "nochange")
            return;
        else if (setting == "disable")
            ShouldDisableTerminalLight(Plugin.instance.Terminal.placeableObject.inUse, setting);
        else if (setting == "alwayson")
            ShouldDisableTerminalLight(false, setting);
    }

    internal static bool InUseCheck(PlayerControllerB player)
    {
        if (player == null)
            return false;

        if (player.spectatedPlayerScript != null && player.isPlayerDead)
            return player.spectatedPlayerScript.isInHangarShipRoom;
        else
            return player.isInHangarShipRoom;
    }

    internal static void OnTerminalKeyPress()
    {
        if (!Plugin.instance.Terminal.terminalInUse)
            return;

        if (BoolStuff.AnyKeyIsPressed() && BoolStuff.ListenForShortCuts())
        {
            ShortcutBindings.HandleKeyPress(ShortcutBindings.keyBeingPressed);
        }

        if (QoLConfig.WalkieTerm.Value)
        {
            WalkieTerm.WalkieTerminal();
        }
    }
}

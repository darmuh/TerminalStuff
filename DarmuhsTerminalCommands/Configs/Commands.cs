using BepInEx.Configuration;
using OpenLib.CoreMethods;
using System;
using System.Collections.Generic;
using TerminalStuff.CommandHandling;
using TerminalStuff.EventSub;
using TerminalStuff.SpecialStuff;
using static OpenLib.Common.CommonTerminal;
using static OpenLib.ConfigManager.ConfigSetup;
using static TerminalStuff.Configs.KeywordConfigs;

namespace TerminalStuff.Configs;

public class Commands
{
    //NEW SYSTEM
    public static List<CommandManager> AllCommands { get; private set; } = [];
    public static List<CommandManager> GetEnabledCommands()
    {
        if (AllCommands.Count == 0)
            return [];

        return AllCommands.FindAll(x => x.IsCreated);
    }
    public static List<CommandManager> GetSpecialCommands()
    {
        var enabled = GetEnabledCommands();

        if (enabled.Count == 0)
            return [];

        return enabled.FindAll(x => x.VerySpecialNum != -1);
    }

    //OLD SYSTEM
    //public static List<ManagedConfig> ViewConfig = [];
    //public static List<ManagedConfig> TerminalStuffBools = [];

    //command booleans
    public static ConfigEntry<bool> TerminalPurchasePacks { get; internal set; } = null!;
    public static ConfigEntry<bool> TerminalShortcutCommands { get; internal set; } = null!;
    public static ConfigEntry<bool> TerminalLobby { get; internal set; } = null!; //lobby name command
    public static ConfigEntry<bool> TerminalCams { get; internal set; } = null!; //cams command
    public static ConfigEntry<bool> TerminalQuit { get; internal set; } = null!; //quit command
    public static ConfigEntry<bool> TerminalClear { get; internal set; } = null!; //clear command
    public static ConfigEntry<bool> TerminalLoot { get; internal set; } = null!; //loot command
    public static ConfigEntry<bool> TerminalVideo { get; internal set; } = null!; //video command
    public static ConfigEntry<bool> TerminalHeal { get; internal set; } = null!; //heal command
    public static ConfigEntry<bool> TerminalFov { get; internal set; } = null!; //Fov command
    public static ConfigEntry<bool> TerminalGamble { get; internal set; } = null!; //Gamble command
    public static ConfigEntry<bool> TerminalLever { get; internal set; } = null!; //Lever command
    public static ConfigEntry<bool> TerminalDanger { get; internal set; } = null!; //Danger command
    public static ConfigEntry<bool> TerminalVitals { get; internal set; } = null!; //Vitals command
    public static ConfigEntry<bool> TerminalBioScan { get; internal set; } = null!; //BioScan command
    public static ConfigEntry<bool> TerminalBioScanPatch { get; internal set; } = null!; //BioScan Upgrade command
    public static ConfigEntry<bool> TerminalVitalsUpgrade { get; internal set; } = null!; //Vitals Upgrade command
    public static ConfigEntry<bool> TerminalTP { get; internal set; } = null!; //Teleporter command
    public static ConfigEntry<bool> TerminalITP { get; internal set; } = null!; //Inverse Teleporter command
    public static ConfigEntry<bool> TerminalMods { get; internal set; } = null!; //Modlist command
    public static ConfigEntry<bool> TerminalKick { get; internal set; } = null!; //Kick command (host only)
    public static ConfigEntry<bool> TerminalFcolor { get; internal set; } = null!; //Flashlight color command
    public static ConfigEntry<bool> TerminalMap { get; internal set; } = null!; //Map shortcut
    public static ConfigEntry<bool> TerminalMinimap { get; internal set; } = null!; //Minimap command
    public static ConfigEntry<bool> TerminalMinicams { get; internal set; } = null!; //Minicams command
    public static ConfigEntry<bool> TerminalOverlay { get; internal set; } = null!; //Overlay cams command
    public static ConfigEntry<bool> TerminalDoor { get; internal set; } = null!; //Door Toggle command
    public static ConfigEntry<bool> TerminalLights { get; internal set; } = null!; //Light Toggle command
    public static ConfigEntry<bool> TerminalScolor { get; internal set; } = null!; //Light colors command
    public static ConfigEntry<bool> TerminalAlwaysOnCommand { get; internal set; } = null!; //AlwaysOn command
    public static ConfigEntry<bool> TerminalLink { get; internal set; } = null!; //Link command
    public static ConfigEntry<bool> TerminalLink2 { get; internal set; } = null!; //Link2 command
    public static ConfigEntry<bool> TerminalRandomSuit { get; internal set; } = null!; //RandomSuit command
    public static ConfigEntry<bool> TerminalClockCommand { get; internal set; } = null!; //toggle clock command
    public static ConfigEntry<bool> TerminalListItems { get; internal set; } = null!; //List Items Command
    public static ConfigEntry<bool> TerminalLootDetail { get; internal set; } = null!; //List Scrap Command
    public static ConfigEntry<bool> TerminalMirror { get; internal set; } = null!; //mirror command
    public static ConfigEntry<bool> TerminalRefund { get; internal set; } = null!; //refund command
    public static ConfigEntry<bool> TerminalRestart { get; internal set; } = null!; //restart command
    public static ConfigEntry<bool> TerminalPrevious { get; internal set; } = null!; //previous switch command
    public static ConfigEntry<bool> TerminalRouteRandom { get; internal set; } = null!; // route random command
    public static ConfigEntry<bool> TerminalRefreshCustomization { get; internal set; } = null!; //refresh customization command
    public static ConfigEntry<bool> TerminalRadarZoom { get; internal set; } = null!;
    public static ConfigEntry<bool> TerminalMoonsPlus { get; internal set; } = null!; //MoonsPlus enable/disable
    public static ConfigEntry<bool> TerminalStorePlus { get; internal set; } = null!; //StorePlus enable/disable

    //overrides
    public static ConfigEntry<bool> LeverConfirmOverride { get; internal set; } = null!; //disable confirmation check for lever
    public static ConfigEntry<bool> RestartConfirmOverride { get; internal set; } = null!; //disable confirmation check for lever

    //Cost configs
    public static ConfigEntry<int> VitalsCost { get; internal set; } = null!; //Cost of Vitals Command
    public static ConfigEntry<int> VitalsUpgradeCost { get; internal set; } = null!; //Cost of Vitals Upgrade Command
    public static ConfigEntry<int> BioScanUpgradeCost { get; internal set; } = null!; //Cost of Enemy Scan Upgrade Command
    public static ConfigEntry<int> BioScanCost { get; internal set; } = null!; //Cost of Enemy Scan Command


    public static void Init()
    {
        //override configs
        LeverConfirmOverride = MakeGeneric(Plugin.instance.Config, "Controls Configuration", "LeverConfirmOverride", false, "Setting this to true will disable the confirmation check for the <lever> command.");
        RestartConfirmOverride = MakeGeneric(Plugin.instance.Config, "Controls Configuration", "RestartConfirmOverride", false, "Setting this to true will disable the confirmation check for the <restart> command.");

        //Cost configs
        VitalsCost = MakeGeneric(Plugin.instance.Config, "Upgrades", "VitalsCost", 10, "Credits cost to run Vitals Command each time it's run.");
        VitalsUpgradeCost = MakeGeneric(Plugin.instance.Config, "Upgrades", "VitalsUpgradeCost", 200, "Credits cost to upgrade Vitals command to not cost credits anymore.");
        BioScanUpgradeCost = MakeGeneric(Plugin.instance.Config, "Upgrades", "BioScanUpgradeCost", 300, "Credits cost to upgrade Bioscan command to provide detailed information on scanned enemies.");
        BioScanCost = MakeGeneric(Plugin.instance.Config, "Upgrades", "BioScanCost", 15, "Credits cost to run Bioscan command each time it's run. (scans for enemy information)");

        //------------------------------------------------MANAGED BOOLS START------------------------------------------------//

        TerminalShortcutCommands = MakeGeneric(Plugin.instance.Config, "Comfort Commands (On/Off)", "TerminalShortcutCommands", false, "Enable or disable shortCut commands (dependent on TerminalShortcuts)");

        TerminalLobby = MakeGeneric(Plugin.instance.Config, "Comfort Commands (On/Off)", "TerminalLobby", false, "Check for the current lobby name");

        AddLocalCommmand("Lobby", "Comfort", TerminalLobby, LobbyKeywords, MoreCommands.GetLobbyName);
        //AddManagedBool(TerminalLobby, defaultManaged, false, "COMFORT", LobbyKeywords, MoreCommands.GetLobbyName);

        TerminalQuit = MakeGeneric(Plugin.instance.Config, "Comfort Commands (On/Off)", "TerminalQuit", true, "Command to quit terminal");

        AddLocalCommmand("Quit", "Comfort", TerminalQuit, QuitKeywords, TerminalEvents.QuitTerminalCommand);

        TerminalClear = MakeGeneric(Plugin.instance.Config, "Comfort Commands (On/Off)", "TerminalClear", true, "Command to clear terminal text");

        AddLocalCommmand("Clear", "Comfort", TerminalClear, ClearKeywords, ClearText);

        TerminalLoot = MakeGeneric(Plugin.instance.Config, "Extras Commands (On/Off)", "TerminalLoot", true, "Command to show total onboard loot value");
        //AddManagedBool(TerminalLoot, defaultManaged, false, "EXTRAS", LootKeywords, AllTheLootStuff.GetLootSimple, 0, false);
        AddLocalCommmand("Loot", "Extras", TerminalLoot, LootKeywords, AllTheLootStuff.GetLootSimple, false);

        TerminalHeal = MakeGeneric(Plugin.instance.Config, "Comfort Commands (On/Off)", "TerminalHeal", false, "Command to heal yourself");
        //AddManagedBool(TerminalHeal, defaultManaged, false, "COMFORT", HealKeywords, MoreCommands.HealCommand);
        AddLocalCommmand("Heal", "Comfort", TerminalHeal, HealKeywords, MoreCommands.HealCommand);

        TerminalFov = MakeGeneric(Plugin.instance.Config, "Comfort Commands (On/Off)", "TerminalFov", false, "Command to change your FOV");
        //AddManagedBool(TerminalFov, defaultManaged, false, "COMFORT", FovKeywords, DynamicCommands.FovPrompt, 1, true, DynamicCommands.FovConfirm, DynamicCommands.FovDeny, "", "", "fov");
        AddConfirmationCommand(AddLocalCommmand("Fov", "Comfort", TerminalFov, FovKeywords, DynamicCommands.FovPrompt, true, true, true), DynamicCommands.FovConfirm, DynamicCommands.FovDeny);

        TerminalGamble = MakeGeneric(Plugin.instance.Config, "Fun Commands (On/Off)", "TerminalGamble", false, "Command to gamble your credits, by percentage");
        //AddManagedBool(TerminalGamble, defaultManaged, true, "FUN", GambleKeywords, GambaCommands.Ask2Gamble, 1, true, GambaCommands.GambleConfirm, GambaCommands.GambleDeny, "", "", GambleKeywords.Value);
        AddConfirmationCommand(AddNetworkedCommmand("Gamble", "Fun", TerminalGamble, GambleKeywords, GambaCommands.Ask2Gamble, true, true, true), GambaCommands.GambleConfirm, GambaCommands.GambleDeny);


        TerminalLever = MakeGeneric(Plugin.instance.Config, "Controls Commands (On/Off)", "TerminalLever", true, "Pull the lever from terminal");
        //AddManagedBool(TerminalLever, defaultManaged, false, "CONTROLS", LeverKeywords, ShipControls.AskLever, 1, true, ShipControls.LeverControlCommand, ShipControls.DenyLever);
        AddConfirmationCommand(AddLocalCommmand("Lever", "CONTROLS", TerminalLever, LeverKeywords, ShipControls.AskLever), ShipControls.LeverControlCommand, ShipControls.DenyLever);

        TerminalRestart = MakeGeneric(Plugin.instance.Config, "Controls Commands (On/Off)", "TerminalRestart", true, "Command to restart the lobby (skips firing sequence)");
        AddConfirmationCommand(AddNetworkedCommmand("Restart", "CONTROLS", TerminalRestart, RestartKeywords, ShipControls.RestartAsk), ShipControls.RestartAction, ShipControls.RestartDeny);

        TerminalDanger = MakeGeneric(Plugin.instance.Config, "Controls Commands (On/Off)", "TerminalDanger", false, "Check moon danger level");
        //AddManagedBool(TerminalDanger, defaultManaged, false, "CONTROLS", DangerKeywords, MoreCommands.DangerCommand);
        AddLocalCommmand("Danger", "Controls", TerminalDanger, DangerKeywords, MoreCommands.DangerCommand);

        TerminalVitals = MakeGeneric(Plugin.instance.Config, "Extras Commands (On/Off)", "TerminalVitals", false, "Scan player being monitored for their vitals");
        //AddManagedBool(TerminalVitals, defaultManaged, true, "EXTRAS", "vitals", CostCommands.VitalsCommand);
        AddNetworkedCommmand("Vitals", "Extras", TerminalVitals, VitalsKWs, CostCommands.VitalsCommand);

        TerminalBioScan = MakeGeneric(Plugin.instance.Config, "Extras Commands (On/Off)", "TerminalBioScan", false, "Scan for \"non-employee\" lifeforms.");
        //AddManagedBool(TerminalBioScan, defaultManaged, true, "EXTRAS", "bioscan", CostCommands.BioscanCommand);
        AddNetworkedCommmand("BioScan", "Extras", TerminalBioScan, BioScanKWs, CostCommands.BioscanCommand);

        //----------------------------------upgrade managed bools----------------------------------//

        TerminalBioScanPatch = MakeGeneric(Plugin.instance.Config, "Extras Commands (On/Off)", "TerminalBioScanPatch", false, "Purchase-able upgrade patch to bioscan command. The command will provide more precise information after the upgrade.");
        //AddManagedBool(TerminalBioScanPatch, defaultManaged, true, "EXTRAS", "bioscanpatch", CostCommands.AskBioscanUpgrade, 2, true, CostCommands.PerformBioscanUpgrade, null, "", "You have opted out of purchasing the BioScanner 2.0 Upgrade Patch.\n\n", "", -1, "", "", BioScanUpgradeCost.Value, "BioscanPatch", true, 1);
        AddStoreCommand(AddNetworkedCommmandManualWords("Bioscan Patch (Upgrade)", "Extras", TerminalBioScanPatch, ["bioscanpatch"], CostCommands.AskBioscanUpgrade), "Bioscan Patch", BioScanUpgradeCost, CostCommands.PerformBioscanUpgrade, () => "You have opted out of purchasing the BioScanner 2.0 Upgrade Patch.\n\n", 1, true);

        TerminalVitalsUpgrade = MakeGeneric(Plugin.instance.Config, "Extras Commands (On/Off)", "TerminalVitalsUpgrade", false, "Purchase-able upgrade to vitals command to make the cost of each vitals scan free!");
        //AddManagedBool(TerminalVitalsUpgrade, defaultManaged, true, "EXTRAS", "vitalspatch", CostCommands.AskVitalsUpgrade, 2, true, CostCommands.PerformVitalsUpgrade, null, "", "You have opted out of purchasing the Vitals Scanner Upgrade.\n\n", "", -1, "VitalsPatch", "", VitalsUpgradeCost.Value, "VitalsPatch", true, 1);
        AddStoreCommand(AddNetworkedCommmandManualWords("Vitals Patch (Upgrade)", "Extras", TerminalVitalsUpgrade, ["vitalspatch"], CostCommands.AskVitalsUpgrade), "Vitals Patch", VitalsUpgradeCost, CostCommands.PerformVitalsUpgrade, () => "You have opted out of purchasing the Vitals Scanner Upgrade.\n\n", 1, true);

        //----------------------------------upgrade managed bools----------------------------------//


        TerminalMods = MakeGeneric(Plugin.instance.Config, "Comfort Commands (On/Off)", "TerminalMods", false, "Command to see your active mods");

        //AddManagedBool(TerminalMods, defaultManaged, false, "COMFORT", ModsKeywords, MoreCommands.ModListCommand);
        AddLocalCommmand("Mod List", "Comfort", TerminalMods, ModsKeywords, MoreCommands.ModListCommand);

        TerminalKick = MakeGeneric(Plugin.instance.Config, "Comfort Commands (On/Off)", "TerminalKick", false, "Enables kick command for host.");
        //AddManagedBool(TerminalKick, defaultManaged, false, "COMFORT", KickKeywords, AdminCommands.KickPlayersAsk, 1, true, AdminCommands.KickPlayerConfirm, AdminCommands.KickPlayerDeny);
        AddConfirmationCommand(AddLocalCommmand("Kick Player", "Comfort", TerminalKick, KickKeywords, AdminCommands.KickPlayersAsk, true, true, true), AdminCommands.KickPlayerConfirm, AdminCommands.KickPlayerDeny);

        TerminalFcolor = MakeGeneric(Plugin.instance.Config, "Fun Commands (On/Off)", "TerminalFcolor", false, "Command to change flashlight color.");

        CommandManager fcolor = AddNetworkedCommmand("Flashlight Color", "Fun", TerminalFcolor, FcolorKeywords, ColorCommands.FlashColorBase, true, true, true);
        fcolor.SetInfoAction(ColorCommands.FlashColorList);


        TerminalScolor = MakeGeneric(Plugin.instance.Config, "Fun Commands (On/Off)", "TerminalScolor", false, "Command to change ship lights colors.");
        //ManagedConfig scolor = AddManagedBool(TerminalScolor, defaultManaged, true, "FUN", ScolorKeywords, ColorCommands.ShipColorBase, 0, true, null, null, "", "", ScolorKeywords.Value);
        //scolor.InfoAction = ColorCommands.ShipColorList;

        CommandManager scolor = AddNetworkedCommmand("Ship Lights Color", "Fun", TerminalScolor, ScolorKeywords, ColorCommands.ShipColorBase, true, true, true);
        scolor.SetInfoAction(ColorCommands.ShipColorList);

        TerminalDoor = MakeGeneric(Plugin.instance.Config, "Controls Commands (On/Off)", "TerminalDoor", true, "Command to open/close the ship door.");
        //AddManagedBool(TerminalDoor, defaultManaged, false, "CONTROLS", DoorKeywords, ShipControls.BasicDoorCommand);
        AddLocalCommmand("Door Button", "Controls", TerminalDoor, DoorKeywords, ShipControls.BasicDoorCommand);

        TerminalLights = MakeGeneric(Plugin.instance.Config, "Controls Commands (On/Off)", "TerminalLights", true, "Command to toggle the ship lights");
        //AddManagedBool(TerminalLights, defaultManaged, false, "CONTROLS", LightsKeywords, ShipControls.BasicLightsCommand);
        AddLocalCommmand("Lightswitch", "Controls", TerminalLights, LightsKeywords, ShipControls.BasicLightsCommand);

        //----------------------------------termview managed bools----------------------------------//

        TerminalCams = MakeGeneric(Plugin.instance.Config, "Extras Commands (On/Off)", "TerminalCams", true, "Command to toggle displaying cameras in terminal");
        //ManagedConfig cams = AddManagedBool(TerminalCams, TerminalStuffBools, false, "EXTRAS", CamsKeywords, ViewCommands.TermCamsEvent, 0, true, null, null, "", "", "cams", 1, "ViewInsideShipCam 1");
        //ViewConfig.Add(cams);
        AddViewCommand(AddLocalCommmand("Show Cameras", "Extras", TerminalCams, CamsKeywords, ViewCommands.TermCamsEvent), 1);

        TerminalVideo = MakeGeneric(Plugin.instance.Config, "Fun Commands (On/Off)", "TerminalVideo", false, "Play a video from the VideoFolderPath folder <video>");
        //AddManagedBool(TerminalVideo, TerminalStuffBools, false, "FUN", VideoKeywords, ViewCommands.LolVideoPlayerEvent, 0, true, null, null, "", "", "lol", 0, "darmuh's videoPlayer");
        AddViewCommand(AddLocalCommmand("Show Video", "FUN", TerminalVideo, VideoKeywords, ViewCommands.LolVideoPlayerEvent), 0);

        TerminalMap = MakeGeneric(Plugin.instance.Config, "Extras Commands (On/Off)", "TerminalMap", true, "Command to toggle displaying radar in the terminal");
        //ManagedConfig map = AddManagedBool(TerminalMap, TerminalStuffBools, false, "EXTRAS", MapKeywords, ViewCommands.TermMapEvent, 0, true, null, null, "", "", "map", 5, "ViewInsideShipCam 1");
        //ViewConfig.Add(map);
        AddViewCommand(AddLocalCommmand("Show Map", "Extras", TerminalMap, MapKeywords, ViewCommands.TermMapEvent), 5);

        TerminalMinimap = MakeGeneric(Plugin.instance.Config, "Extras Commands (On/Off)", "TerminalMinimap", false, "Command to toggle displaying radar/cam minimap view in the terminal");
        //ManagedConfig minimap = AddManagedBool(TerminalMinimap, TerminalStuffBools, false, "EXTRAS", MinimapKeywords, ViewCommands.MiniMapTermEvent, 0, true, null, null, "", "", "minimap", 3, "ViewInsideShipCam 1");
        //ViewConfig.Add(minimap);
        AddViewCommand(AddLocalCommmand("Show MiniMap", "Extras", TerminalMinimap, MinimapKeywords, ViewCommands.MiniMapTermEvent), 3);

        TerminalMinicams = MakeGeneric(Plugin.instance.Config, "Extras Commands (On/Off)", "TerminalMinicams", true, "Command to toggle displaying radar/cam minicams view in the terminal");
        //ManagedConfig minicams = AddManagedBool(TerminalMinicams, TerminalStuffBools, false, "EXTRAS", MinicamsKeywords, ViewCommands.MiniCamsTermEvent, 0, true, null, null, "", "", "minicams", 4, "ViewInsideShipCam 1");
        //ViewConfig.Add(minicams);
        AddViewCommand(AddLocalCommmand("Show MiniCams", "Extras", TerminalMinicams, MinicamsKeywords, ViewCommands.MiniCamsTermEvent), 4);

        TerminalOverlay = MakeGeneric(Plugin.instance.Config, "Extras Commands (On/Off)", "TerminalOverlay", false, "Command to toggle displaying radar/cam overlay view in the terminal");
        //ManagedConfig overlay = AddManagedBool(TerminalOverlay, TerminalStuffBools, false, "EXTRAS", OverlayKeywords, ViewCommands.OverlayTermEvent, 0, true, null, null, "", "", "overlay", 2, "ViewInsideShipCam 1");
        //ViewConfig.Add(overlay);
        AddViewCommand(AddLocalCommmand("Show Overlay", "Extras", TerminalOverlay, OverlayKeywords, ViewCommands.OverlayTermEvent), 2);

        TerminalMirror = MakeGeneric(Plugin.instance.Config, "Extras Commands (On/Off)", "TerminalMirror", true, "Command to toggle displaying a Mirror Cam in the terminal");
        //AddManagedBool(TerminalMirror, TerminalStuffBools, false, "EXTRAS", MirrorKeywords, ViewCommands.MirrorEvent, 0, true, null, null, "", "", "mirror", 6, "terminalStuff Mirror");
        AddViewCommand(AddLocalCommmand("Show Mirror", "Extras", TerminalMirror, MirrorKeywords, ViewCommands.MirrorEvent), 6);

        //----------------------------------termview managed bools----------------------------------//

        TerminalAlwaysOnCommand = MakeGeneric(Plugin.instance.Config, "Comfort Commands (On/Off)", "TerminalAlwaysOnCommand", false, $"Command to toggle Always-On Display");
        //AddManagedBool(TerminalAlwaysOnCommand, defaultManaged, false, "COMFORT", AlwaysOnKeywords, MoreCommands.AlwaysOnDisplay);
        AddLocalCommmand("Always-On Toggle", "Comfort", TerminalAlwaysOnCommand, AlwaysOnKeywords, MoreCommands.AlwaysOnDisplay);

        TerminalLink = MakeGeneric(Plugin.instance.Config, "Extras Commands (On/Off)", "TerminalLink", true, "Command to link to an external web-page");
        //AddManagedBool(TerminalLink, defaultManaged, false, "EXTRAS", LinkKeywords, MoreCommands.FirstLinkAsk, 1, true, MoreCommands.FirstLinkDo, MoreCommands.FirstLinkDeny);
        AddConfirmationCommand(AddLocalCommmand("Link 1", "Extras", TerminalLink, LinkKeywords, MoreCommands.FirstLinkAsk), MoreCommands.FirstLinkDo, MoreCommands.FirstLinkDeny);

        TerminalLink2 = MakeGeneric(Plugin.instance.Config, "Extras Commands (On/Off)", "TerminalLink2", false, "Command to link to a second external web-page");
        //AddManagedBool(TerminalLink2, defaultManaged, false, "EXTRAS", Link2Keywords, MoreCommands.SecondLinkAsk, 1, true, MoreCommands.SecondLinkDo, MoreCommands.SecondLinkDeny);
        AddConfirmationCommand(AddLocalCommmand("Link 2", "Extras", TerminalLink2, Link2Keywords, MoreCommands.SecondLinkAsk), MoreCommands.SecondLinkDo, MoreCommands.SecondLinkDeny);

        TerminalRandomSuit = MakeGeneric(Plugin.instance.Config, "Fun Commands (On/Off)", "TerminalRandomSuit", true, "Command to switch your suit from a random one off the rack");
        //AddManagedBool(TerminalRandomSuit, defaultManaged, false, "FUN", RandomSuitKeywords, TerminalEvents.RandomSuit);
        AddLocalCommmand("Random Suit", "Fun", TerminalRandomSuit, RandomSuitKeywords, TerminalEvents.RandomSuit);

        TerminalClockCommand = MakeGeneric(Plugin.instance.Config, "Controls Commands (On/Off)", "TerminalClockCommand", false, "Command to toggle the Terminal Clock off/on");
        //AddManagedBool(TerminalClockCommand, defaultManaged, false, "CONTROLS", ClockKeywords, TerminalEvents.ClockToggle);
        AddLocalCommmand("Clock toggle", "Controls", TerminalClockCommand, ClockKeywords, TerminalEvents.ClockToggle);

        TerminalListItems = MakeGeneric(Plugin.instance.Config, "Extras Commands (On/Off)", "TerminalListItems", true, "Command to list all non-scrap & not currently held items on the ship");
        //AddManagedBool(TerminalListItems, defaultManaged, false, "EXTRAS", ListItemsKeywords, MoreCommands.GetItemsOnShip);
        AddLocalCommmand("List Items", "Extras", TerminalListItems, ListItemsKeywords, MoreCommands.GetItemsOnShip);

        TerminalLootDetail = MakeGeneric(Plugin.instance.Config, "Extras Commands (On/Off)", "TerminalLootDetail", true, "Command to display an extensive list of all scrap on the ship");
        //AddManagedBool(TerminalLootDetail, defaultManaged, false, "EXTRAS", ListScrapKeywords, AllTheLootStuff.DetailedLootCommand);
        AddLocalCommmand("Loot (Detailed)", "Extras", TerminalLootDetail, ListScrapKeywords, AllTheLootStuff.DetailedLootCommand);

        TerminalRefund = MakeGeneric(Plugin.instance.Config, "Extras Commands (On/Off)", "TerminalRefund", true, "Command to cancel an undelivered order and get your credits back");
        //AddManagedBool(TerminalRefund, defaultManaged, true, "EXTRAS", RefundKeywords, CostCommands.GetRefund);
        AddLocalCommmand("Refund", "Extras", TerminalRefund, RefundKeywords, CostCommands.GetRefund);

        TerminalPrevious = MakeGeneric(Plugin.instance.Config, "Extras Commands (On/Off)", "TerminalPrevious", true, "Command to switch back to previous radar target");
        //AddManagedBool(TerminalPrevious, defaultManaged, false, "EXTRAS", PreviousKeywords, ViewCommands.HandlePreviousSwitchEvent);
        AddLocalCommmand("Previous", "Extras", TerminalPrevious, PreviousKeywords, ViewCommands.HandlePreviousSwitchEvent);

        TerminalRouteRandom = MakeGeneric(Plugin.instance.Config, "Fun Commands (On/Off)", "TerminalRouteRandom", true, "Command to route to a random planet");
        //AddManagedBool(TerminalRouteRandom, defaultManaged, true, "FUN", RandomRouteKeywords, LevelCommands.RouteRandomCommand);
        AddLocalCommmand("Route Random", "Fun", TerminalRouteRandom, RandomRouteKeywords, LevelCommands.RouteRandomCommand);

        TerminalRefreshCustomization = MakeGeneric(Plugin.instance.Config, "Fun Commands (On/Off)", "TerminalRefreshCustomization", false, "Command to reload the Terminal Customization settings (this will not disable any already applied customizations)");
        //AddManagedBool(TerminalRefreshCustomization, defaultManaged, false, "FUN", RefreshcustomizationKWs, TerminalEvents.RefreshCustomizationCommand);
        AddLocalCommmand("Refresh Customizations", "Fun", TerminalRefreshCustomization, RefreshcustomizationKWs, TerminalEvents.RefreshCustomizationCommand);
        TerminalRadarZoom = MakeGeneric(Plugin.instance.Config, "Controls Commands (On/Off)", "TerminalRadarZoom", false, "Command to cycle through various radar zoom levels.");
        //AddManagedBool(TerminalRadarZoom, defaultManaged, false, "CONTROLS", RadarZoomKWs, ViewCommands.RadarZoomEvent, 0, true, null, null, "", "", "radarZoom");
        AddLocalCommmand("Radar Zoom", "Controls", TerminalRadarZoom, RadarZoomKWs, ViewCommands.RadarZoomEvent, true, true, true);

        //------------------------------------------------MANAGED BOOLS END------------------------------------------------//

        //NOT MANAGED BOOLS THAT ARE COMMANDS, DEFINE THESE COMMANDS LATER THAN TERMINAL AWAKE
        TerminalTP = MakeGeneric(Plugin.instance.Config, "Controls Commands (On/Off)", "TerminalTP", true, "Command to Activate Teleporter <TP>");
        TerminalITP = MakeGeneric(Plugin.instance.Config, "Controls Commands (On/Off)", "TerminalITP", true, "Command to Activate Inverse Teleporter <ITP>");
        TerminalPurchasePacks = MakeGeneric(Plugin.instance.Config, "Comfort Commands (On/Off)", "TerminalPurchasePacks", false, "Use [PurchasePackCommands] to create purchase packs that contain multiple store items in one run of the command");
        TerminalMoonsPlus = MakeGeneric(Plugin.instance.Config, "MoonsPlus", "TerminalMoonsPlus", false, "Enable/Disable the Moons Plus page for an interactive menu to select moons from.\nWARNING: This feature has limited compatibility testing with other mods that modify the vanilla moons page and mods that affect where you can route.");
        TerminalStorePlus = MakeGeneric(Plugin.instance.Config, "StorePlus", "TerminalStorePlus", false, "Enable/Disable the Store Plus page for an interactive menu to select shop items from.\nWARNING: This feature has limited compatibility testing with other mods that modify the vanilla store page and mods that affect what you can buy.");
        //NOT MANAGED BOOLS THAT ARE COMMANDS, DEFINE THESE COMMANDS LATER THAN TERMINAL AWAKE
    }

    internal static void CommandDefinitions()
    {
        //--- Menus
        MoonsTweaks.MoonsPlus.MoonsCommand = AddLocalCommmand("MoonsPlus", "Menus", TerminalMoonsPlus, MoonsPlusConfig.MoonsPlusKeywords, MoonsTweaks.MoonsPlus.EnterMoonsMenu, true, false);
        StoreTweaks.StorePlus.StoreCommand = AddLocalCommmand("StorePlus", "Menus", TerminalStorePlus, StorePlusConfig.StorePlusKeywords, StoreTweaks.StorePlus.EnterStoreMenu, true, false);
        //---

        //--- Teleporters
        Teleporters.Inverse = AddLocalCommmand("Use Inverse Teleporter", "Controls", TerminalITP, ItpKeywords, ShipControls.InverseTeleporterCommand, true, false);
        Teleporters.Regular = AddLocalCommmand("Use Teleporter", "Controls", TerminalTP, TpKeywords, ShipControls.RegularTeleporterCommand, true, false, true);
        //---


        //--- Shortcuts
        StuffForLibrary.Bind = AddLocalCommmandManualWords("bindCommand", TerminalShortcutCommands, ["bind"], DynamicCommands.BindKeyToCommand, "Comfort", true, false, true);
        StuffForLibrary.Unbind = AddLocalCommmandManualWords("Unbind", TerminalShortcutCommands, ["unbind"], DynamicCommands.UnBindKeyToCommand, "Comfort", true, false, true);
        //---

        //--- Delay
        AddLocalCommmand("Delay Start", "Comfort", QoLConfig.TerminalRunDelay, DelayKWs, HandleDelayRun.HandleCommandDelay, true, true, true);
        AddLocalCommmand("Delay Stop", "Comfort", QoLConfig.TerminalRunDelay, StopDelayKWs, HandleDelayRun.StopCommandDelay, true, true, true);
        //---

        //--- Misc
        StuffForLibrary.Switch = AddReplacementCommand("SwitchedCam", SwitchKeywords, ViewCommands.SwitchCommandHandler, string.Empty, false, true);
        //---
    }

    //For use with replacing existing commands
    public static CommandManager AddReplacementCommand(string name, ConfigEntry<string> keywordConfig, Func<string> commandFunc, string category = "", bool addAtAwake = true, bool acceptAdditionalText = false)
    {
        CommandManager command = new(name, keywordConfig, commandFunc)
        {
            AddAtAwake = addAtAwake,
            AcceptAdditionalText = acceptAdditionalText,
            Category = category
        };

        AllCommands.Add(command);
        return command;
    }

    public static CommandManager AddLocalCommmand(string name, string category, ConfigEntry<bool> toggle, ConfigEntry<string> keywordConfig, Func<string> commandFunc, bool clearText = true, bool addAtAwake = true, bool acceptAdditionalText = false)
    {
        CommandManager command = new(name, toggle, keywordConfig, commandFunc)
        {
            AddAtAwake = addAtAwake,
            AcceptAdditionalText = acceptAdditionalText,
            Category = category,
            ClearText = clearText,
        };

        AllCommands.Add(command);
        return command;
    }

    public static CommandManager AddNetworkedCommmand(string name, string category, ConfigEntry<bool> toggle, ConfigEntry<string> keywordConfig, Func<string> commandFunc, bool clearText = true, bool addAtAwake = true, bool acceptAdditionalText = false)
    {
        CommandManager command = new(name, toggle, keywordConfig, commandFunc)
        {
            AddAtAwake = addAtAwake,
            AcceptAdditionalText = acceptAdditionalText,
            Category = category,
            ClearText = clearText,
        };

        command.IsEnabled.NetworkingReq = true;
        command.IsEnabled.networkingConfig = ConfigSettings.ModNetworking;

        AllCommands.Add(command);
        return command;
    }

    public static CommandManager AddNetworkedCommmandManualWords(string name, string category, ConfigEntry<bool> toggle, List<string> manualKeywords, Func<string> commandFunc, bool clearText = true, bool addAtAwake = true, bool acceptAdditionalText = false)
    {
        CommandManager command = new(name, toggle, manualKeywords, commandFunc)
        {
            AddAtAwake = addAtAwake,
            AcceptAdditionalText = acceptAdditionalText,
            Category = category,
            ClearText = clearText,
        };

        command.IsEnabled.NetworkingReq = true;
        command.IsEnabled.networkingConfig = ConfigSettings.ModNetworking;

        AllCommands.Add(command);
        return command;
    }

    //video managed command

    //for use with commands that do not have keyword configurations
    public static CommandManager AddLocalCommmandManualWords(string name, ConfigEntry<bool> toggle, List<string> manualKeywords, Func<string> commandFunc, string cateogy = "", bool clearText = true, bool addAtAwake = true, bool acceptAdditionalText = false)
    {
        CommandManager command = new(name, toggle, manualKeywords, commandFunc)
        {
            AddAtAwake = addAtAwake,
            AcceptAdditionalText = acceptAdditionalText,
            Category = cateogy,
            ClearText = clearText
        };

        AllCommands.Add(command);
        return command;
    }

    public static void AddConfirmationCommand(CommandManager command, Func<string> confirm, Func<string> deny, int commandType = 1)
    {
        command.CommandType = commandType;
        command.SetupConfirmation(confirm, deny);
    }

    public static void AddStoreCommand(CommandManager command, string displayName, ConfigEntry<int> priceConfig, Func<string> confirm, Func<string> deny, int maxStock = 0, bool alwaysInStock = false)
    {
        AddConfirmationCommand(command, confirm, deny, 2);
        command.SetupStore(displayName, priceConfig, maxStock, alwaysInStock);
    }

    public static void AddStoreCommand(CommandManager command, string displayName, int manualPrice, Func<string> confirm, Func<string> deny, int maxStock = 0, bool alwaysInStock = false)
    {
        AddConfirmationCommand(command, confirm, deny, 2);
        command.SetupStore(displayName, manualPrice, maxStock, alwaysInStock);
    }

    public static void AddViewCommand(CommandManager command, int index)
    {
        command.VerySpecialNum = index;
    }

}

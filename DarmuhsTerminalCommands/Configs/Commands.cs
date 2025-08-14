using BepInEx.Configuration;
using OpenLib.ConfigManager;
using System.Collections.Generic;
using TerminalStuff.EventSub;
using TerminalStuff.PluginCore;
using TerminalStuff.SpecialStuff;
using static OpenLib.Common.CommonTerminal;
using static OpenLib.ConfigManager.ConfigSetup;
using static TerminalStuff.Configs.KeywordConfigs;

namespace TerminalStuff.Configs
{
    public class Commands
    {
        public static List<ManagedConfig> ViewConfig = [];
        public static List<ManagedConfig> TerminalStuffBools = [];

        //command booleans
        public static ConfigEntry<bool> TerminalPurchasePacks { get; internal set; }
        public static ConfigEntry<bool> TerminalShortcutCommands { get; internal set; }
        public static ConfigEntry<bool> TerminalLobby { get; internal set; } //lobby name command
        public static ConfigEntry<bool> TerminalCams { get; internal set; } //cams command
        public static ConfigEntry<bool> TerminalQuit { get; internal set; } //quit command
        public static ConfigEntry<bool> TerminalClear { get; internal set; } //clear command
        public static ConfigEntry<bool> TerminalLoot { get; internal set; } //loot command
        public static ConfigEntry<bool> TerminalVideo { get; internal set; } //video command
        public static ConfigEntry<bool> TerminalHeal { get; internal set; } //heal command
        public static ConfigEntry<bool> TerminalFov { get; internal set; } //Fov command
        public static ConfigEntry<bool> TerminalGamble { get; internal set; } //Gamble command
        public static ConfigEntry<bool> TerminalLever { get; internal set; } //Lever command
        public static ConfigEntry<bool> TerminalDanger { get; internal set; } //Danger command
        public static ConfigEntry<bool> TerminalVitals { get; internal set; } //Vitals command
        public static ConfigEntry<bool> TerminalBioScan { get; internal set; } //BioScan command
        public static ConfigEntry<bool> TerminalBioScanPatch { get; internal set; } //BioScan Upgrade command
        public static ConfigEntry<bool> TerminalVitalsUpgrade { get; internal set; } //Vitals Upgrade command
        public static ConfigEntry<bool> TerminalTP { get; internal set; } //Teleporter command
        public static ConfigEntry<bool> TerminalITP { get; internal set; } //Inverse Teleporter command
        public static ConfigEntry<bool> TerminalMods { get; internal set; } //Modlist command
        public static ConfigEntry<bool> TerminalKick { get; internal set; } //Kick command (host only)
        public static ConfigEntry<bool> TerminalFcolor { get; internal set; } //Flashlight color command
        public static ConfigEntry<bool> TerminalMap { get; internal set; } //Map shortcut
        public static ConfigEntry<bool> TerminalMinimap { get; internal set; } //Minimap command
        public static ConfigEntry<bool> TerminalMinicams { get; internal set; } //Minicams command
        public static ConfigEntry<bool> TerminalOverlay { get; internal set; } //Overlay cams command
        public static ConfigEntry<bool> TerminalDoor { get; internal set; } //Door Toggle command
        public static ConfigEntry<bool> TerminalLights { get; internal set; } //Light Toggle command
        public static ConfigEntry<bool> TerminalScolor { get; internal set; } //Light colors command
        public static ConfigEntry<bool> TerminalAlwaysOnCommand { get; internal set; } //AlwaysOn command
        public static ConfigEntry<bool> TerminalLink { get; internal set; } //Link command
        public static ConfigEntry<bool> TerminalLink2 { get; internal set; } //Link2 command
        public static ConfigEntry<bool> TerminalRandomSuit { get; internal set; } //RandomSuit command
        public static ConfigEntry<bool> TerminalClockCommand { get; internal set; } //toggle clock command
        public static ConfigEntry<bool> TerminalListItems { get; internal set; } //List Items Command
        public static ConfigEntry<bool> TerminalLootDetail { get; internal set; } //List Scrap Command
        public static ConfigEntry<bool> TerminalMirror { get; internal set; } //mirror command
        public static ConfigEntry<bool> TerminalRefund { get; internal set; } //refund command
        public static ConfigEntry<bool> TerminalRestart { get; internal set; } //restart command
        public static ConfigEntry<bool> TerminalPrevious { get; internal set; } //previous switch command
        public static ConfigEntry<bool> TerminalRouteRandom { get; internal set; } // route random command
        public static ConfigEntry<bool> TerminalRefreshCustomization { get; internal set; } //refresh customization command
        public static ConfigEntry<bool> TerminalRadarZoom { get; internal set; }
        public static ConfigEntry<bool> TerminalMoonsPlus { get; internal set; } //MoonsPlus enable/disable
        public static ConfigEntry<bool> TerminalStorePlus { get; internal set; } //StorePlus enable/disable

        //overrides
        public static ConfigEntry<bool> LeverConfirmOverride { get; internal set; } //disable confirmation check for lever
        public static ConfigEntry<bool> RestartConfirmOverride { get; internal set; } //disable confirmation check for lever

        //Cost configs
        public static ConfigEntry<int> VitalsCost { get; internal set; } //Cost of Vitals Command
        public static ConfigEntry<int> VitalsUpgradeCost { get; internal set; } //Cost of Vitals Upgrade Command
        public static ConfigEntry<int> BioScanUpgradeCost { get; internal set; } //Cost of Enemy Scan Upgrade Command
        public static ConfigEntry<int> BioScanCost { get; internal set; } //Cost of Enemy Scan Command


        public static void Init()
        {
            //override configs
            LeverConfirmOverride = MakeBool(Plugin.instance.Config, "Controls Configuration", "LeverConfirmOverride", false, "Setting this to true will disable the confirmation check for the <lever> command.");
            RestartConfirmOverride = MakeBool(Plugin.instance.Config, "Controls Configuration", "RestartConfirmOverride", false, "Setting this to true will disable the confirmation check for the <restart> command.");

            //Cost configs
            VitalsCost = MakeInt(Plugin.instance.Config, "Upgrades", "VitalsCost", 10, "Credits cost to run Vitals Command each time it's run.");
            VitalsUpgradeCost = MakeInt(Plugin.instance.Config, "Upgrades", "VitalsUpgradeCost", 200, "Credits cost to upgrade Vitals command to not cost credits anymore.");
            BioScanUpgradeCost = MakeInt(Plugin.instance.Config, "Upgrades", "BioScanUpgradeCost", 300, "Credits cost to upgrade Bioscan command to provide detailed information on scanned enemies.");
            BioScanCost = MakeInt(Plugin.instance.Config, "Upgrades", "BioScanCost", 15, "Credits cost to run Bioscan command each time it's run. (scans for enemy information)");

            //------------------------------------------------MANAGED BOOLS START------------------------------------------------//

            TerminalShortcutCommands = MakeBool(Plugin.instance.Config, "Comfort Commands (On/Off)", "TerminalShortcutCommands", false, "Enable or disable shortCut commands (dependent on TerminalShortcuts)");

            TerminalLobby = MakeBool(Plugin.instance.Config, "Comfort Commands (On/Off)", "TerminalLobby", false, "Check for the current lobby name");
            AddManagedBool(TerminalLobby, defaultManaged, false, "COMFORT", LobbyKeywords, MoreCommands.GetLobbyName);

            TerminalQuit = MakeBool(Plugin.instance.Config, "Comfort Commands (On/Off)", "TerminalQuit", true, "Command to quit terminal");

            AddManagedBool(TerminalQuit, defaultManaged, false, "COMFORT", QuitKeywords, TerminalEvents.QuitTerminalCommand);

            TerminalClear = MakeBool(Plugin.instance.Config, "Comfort Commands (On/Off)", "TerminalClear", true, "Command to clear terminal text");

            AddManagedBool(TerminalClear, defaultManaged, false, "COMFORT", ClearKeywords, ClearText);

            TerminalLoot = MakeBool(Plugin.instance.Config, "Extras Commands (On/Off)", "TerminalLoot", true, "Command to show total onboard loot value");
            AddManagedBool(TerminalLoot, defaultManaged, false, "EXTRAS", LootKeywords, AllTheLootStuff.GetLootSimple, 0, false);

            TerminalHeal = MakeBool(Plugin.instance.Config, "Comfort Commands (On/Off)", "TerminalHeal", false, "Command to heal yourself");
            AddManagedBool(TerminalHeal, defaultManaged, false, "COMFORT", HealKeywords, MoreCommands.HealCommand);

            TerminalFov = MakeBool(Plugin.instance.Config, "Comfort Commands (On/Off)", "TerminalFov", false, "Command to change your FOV");
            AddManagedBool(TerminalFov, defaultManaged, false, "COMFORT", FovKeywords, DynamicCommands.FovPrompt, 1, true, DynamicCommands.FovConfirm, DynamicCommands.FovDeny, "", "", "fov");

            TerminalGamble = MakeBool(Plugin.instance.Config, "Fun Commands (On/Off)", "TerminalGamble", false, "Command to gamble your credits, by percentage");
            AddManagedBool(TerminalGamble, defaultManaged, true, "FUN", GambleKeywords, GambaCommands.Ask2Gamble, 1, true, GambaCommands.GambleConfirm, GambaCommands.GambleDeny, "", "", GambleKeywords.Value);

            if (LeverConfirmOverride.Value)
            {
                TerminalLever = MakeBool(Plugin.instance.Config, "Controls Commands (On/Off)", "TerminalLever", true, "Pull the lever from terminal");
                AddManagedBool(TerminalLever, defaultManaged, false, "CONTROLS", LeverKeywords, ShipControls.LeverControlCommand);
            }
            else
            {
                TerminalLever = MakeBool(Plugin.instance.Config, "Controls Commands (On/Off)", "TerminalLever", true, "Pull the lever from terminal");
                AddManagedBool(TerminalLever, defaultManaged, false, "CONTROLS", LeverKeywords, ShipControls.AskLever, 1, true, ShipControls.LeverControlCommand, ShipControls.DenyLever);
            }

            if (RestartConfirmOverride.Value)
            {
                TerminalRestart = MakeBool(Plugin.instance.Config, "Controls Commands (On/Off)", "TerminalRestart", true, "Command to restart the lobby (skips firing sequence)");
                AddManagedBool(TerminalRestart, defaultManaged, true, "CONTROLS", RestartKeywords, ShipControls.RestartAction);
            }
            else
            {
                TerminalRestart = MakeBool(Plugin.instance.Config, "Controls Commands (On/Off)", "TerminalRestart", true, "Command to restart the lobby (skips firing sequence)");
                AddManagedBool(TerminalRestart, defaultManaged, true, "CONTROLS", RestartKeywords, ShipControls.RestartAsk, 1, true, ShipControls.RestartAction, ShipControls.RestartDeny);
            }

            TerminalDanger = MakeBool(Plugin.instance.Config, "Controls Commands (On/Off)", "TerminalDanger", false, "Check moon danger level");
            AddManagedBool(TerminalDanger, defaultManaged, false, "CONTROLS", DangerKeywords, MoreCommands.DangerCommand);

            TerminalVitals = MakeBool(Plugin.instance.Config, "Extras Commands (On/Off)", "TerminalVitals", false, "Scan player being monitored for their vitals");
            AddManagedBool(TerminalVitals, defaultManaged, true, "EXTRAS", "vitals", CostCommands.VitalsCommand);

            TerminalBioScan = MakeBool(Plugin.instance.Config, "Extras Commands (On/Off)", "TerminalBioScan", false, "Scan for \"non-employee\" lifeforms.");
            AddManagedBool(TerminalBioScan, defaultManaged, true, "EXTRAS", "bioscan", CostCommands.BioscanCommand);

            //----------------------------------upgrade managed bools----------------------------------//

            TerminalBioScanPatch = MakeBool(Plugin.instance.Config, "Extras Commands (On/Off)", "TerminalBioScanPatch", false, "Purchase-able upgrade patch to bioscan command. The command will provide more precise information after the upgrade.");
            AddManagedBool(TerminalBioScanPatch, defaultManaged, true, "EXTRAS", "bioscanpatch", CostCommands.AskBioscanUpgrade, 2, true, CostCommands.PerformBioscanUpgrade, null, "", "You have opted out of purchasing the BioScanner 2.0 Upgrade Patch.\n\n", "", -1, "", "", BioScanUpgradeCost.Value, "BioscanPatch", true, 1);

            TerminalVitalsUpgrade = MakeBool(Plugin.instance.Config, "Extras Commands (On/Off)", "TerminalVitalsUpgrade", false, "Purchase-able upgrade to vitals command to make the cost of each vitals scan free!");
            AddManagedBool(TerminalVitalsUpgrade, defaultManaged, true, "EXTRAS", "vitalspatch", CostCommands.AskVitalsUpgrade, 2, true, CostCommands.PerformVitalsUpgrade, null, "", "You have opted out of purchasing the Vitals Scanner Upgrade.\n\n", "", -1, "VitalsPatch", "", VitalsUpgradeCost.Value, "VitalsPatch", true, 1);

            //----------------------------------upgrade managed bools----------------------------------//


            TerminalMods = MakeBool(Plugin.instance.Config, "Comfort Commands (On/Off)", "TerminalMods", false, "Command to see your active mods");
            AddManagedBool(TerminalMods, defaultManaged, false, "COMFORT", ModsKeywords, MoreCommands.ModListCommand);

            TerminalKick = MakeBool(Plugin.instance.Config, "Comfort Commands (On/Off)", "TerminalKick", false, "Enables kick command for host.");
            AddManagedBool(TerminalKick, defaultManaged, false, "COMFORT", KickKeywords, AdminCommands.KickPlayersAsk, 1, true, AdminCommands.KickPlayerConfirm, AdminCommands.KickPlayerDeny);

            TerminalFcolor = MakeBool(Plugin.instance.Config, "Fun Commands (On/Off)", "TerminalFcolor", false, "Command to change flashlight color.");
            ManagedConfig fcolor = AddManagedBool(TerminalFcolor, defaultManaged, true, "FUN", FcolorKeywords, ColorCommands.FlashColorBase, 0, true, null, null, "", "", FcolorKeywords.Value);
            fcolor.InfoAction = ColorCommands.FlashColorList;

            TerminalScolor = MakeBool(Plugin.instance.Config, "Fun Commands (On/Off)", "TerminalScolor", false, "Command to change ship lights colors.");
            ManagedConfig scolor = AddManagedBool(TerminalScolor, defaultManaged, true, "FUN", ScolorKeywords, ColorCommands.ShipColorBase, 0, true, null, null, "", "", ScolorKeywords.Value);
            scolor.InfoAction = ColorCommands.ShipColorList;


            TerminalDoor = MakeBool(Plugin.instance.Config, "Controls Commands (On/Off)", "TerminalDoor", true, "Command to open/close the ship door.");
            AddManagedBool(TerminalDoor, defaultManaged, false, "CONTROLS", DoorKeywords, ShipControls.BasicDoorCommand);

            TerminalLights = MakeBool(Plugin.instance.Config, "Controls Commands (On/Off)", "TerminalLights", true, "Command to toggle the ship lights");
            AddManagedBool(TerminalLights, defaultManaged, false, "CONTROLS", LightsKeywords, ShipControls.BasicLightsCommand);

            //----------------------------------termview managed bools----------------------------------//

            TerminalCams = MakeBool(Plugin.instance.Config, "Extras Commands (On/Off)", "TerminalCams", true, "Command to toggle displaying cameras in terminal");
            ManagedConfig cams = AddManagedBool(TerminalCams, TerminalStuffBools, false, "EXTRAS", CamsKeywords, ViewCommands.TermCamsEvent, 0, true, null, null, "", "", "cams", 1, "ViewInsideShipCam 1");
            ViewConfig.Add(cams);

            TerminalVideo = MakeBool(Plugin.instance.Config, "Fun Commands (On/Off)", "TerminalVideo", false, "Play a video from the VideoFolderPath folder <video>");
            AddManagedBool(TerminalVideo, TerminalStuffBools, false, "FUN", VideoKeywords, ViewCommands.LolVideoPlayerEvent, 0, true, null, null, "", "", "lol", 0, "darmuh's videoPlayer");

            TerminalMap = MakeBool(Plugin.instance.Config, "Extras Commands (On/Off)", "TerminalMap", true, "Command to toggle displaying radar in the terminal");
            ManagedConfig map = AddManagedBool(TerminalMap, TerminalStuffBools, false, "EXTRAS", MapKeywords, ViewCommands.TermMapEvent, 0, true, null, null, "", "", "map", 5, "ViewInsideShipCam 1");
            ViewConfig.Add(map);

            TerminalMinimap = MakeBool(Plugin.instance.Config, "Extras Commands (On/Off)", "TerminalMinimap", false, "Command to toggle displaying radar/cam minimap view in the terminal");
            ManagedConfig minimap = AddManagedBool(TerminalMinimap, TerminalStuffBools, false, "EXTRAS", MinimapKeywords, ViewCommands.MiniMapTermEvent, 0, true, null, null, "", "", "minimap", 3, "ViewInsideShipCam 1");
            ViewConfig.Add(minimap);

            TerminalMinicams = MakeBool(Plugin.instance.Config, "Extras Commands (On/Off)", "TerminalMinicams", true, "Command to toggle displaying radar/cam minicams view in the terminal");
            ManagedConfig minicams = AddManagedBool(TerminalMinicams, TerminalStuffBools, false, "EXTRAS", MinicamsKeywords, ViewCommands.MiniCamsTermEvent, 0, true, null, null, "", "", "minicams", 4, "ViewInsideShipCam 1");
            ViewConfig.Add(minicams);

            TerminalOverlay = MakeBool(Plugin.instance.Config, "Extras Commands (On/Off)", "TerminalOverlay", false, "Command to toggle displaying radar/cam overlay view in the terminal");
            ManagedConfig overlay = AddManagedBool(TerminalOverlay, TerminalStuffBools, false, "EXTRAS", OverlayKeywords, ViewCommands.OverlayTermEvent, 0, true, null, null, "", "", "overlay", 2, "ViewInsideShipCam 1");
            ViewConfig.Add(overlay);

            TerminalMirror = MakeBool(Plugin.instance.Config, "Extras Commands (On/Off)", "TerminalMirror", true, "Command to toggle displaying a Mirror Cam in the terminal");
            AddManagedBool(TerminalMirror, TerminalStuffBools, false, "EXTRAS", MirrorKeywords, ViewCommands.MirrorEvent, 0, true, null, null, "", "", "mirror", 6, "terminalStuff Mirror");

            //----------------------------------termview managed bools----------------------------------//

            TerminalAlwaysOnCommand = MakeBool(Plugin.instance.Config, "Comfort Commands (On/Off)", "TerminalAlwaysOnCommand", false, $"Command to toggle Always-On Display");
            AddManagedBool(TerminalAlwaysOnCommand, defaultManaged, false, "COMFORT", AlwaysOnKeywords, MoreCommands.AlwaysOnDisplay);

            TerminalLink = MakeBool(Plugin.instance.Config, "Extras Commands (On/Off)", "TerminalLink", true, "Command to link to an external web-page");
            AddManagedBool(TerminalLink, defaultManaged, false, "EXTRAS", LinkKeywords, MoreCommands.FirstLinkAsk, 1, true, MoreCommands.FirstLinkDo, MoreCommands.FirstLinkDeny);

            TerminalLink2 = MakeBool(Plugin.instance.Config, "Extras Commands (On/Off)", "TerminalLink2", false, "Command to link to a second external web-page");
            AddManagedBool(TerminalLink2, defaultManaged, false, "EXTRAS", Link2Keywords, MoreCommands.SecondLinkAsk, 1, true, MoreCommands.SecondLinkDo, MoreCommands.SecondLinkDeny);

            TerminalRandomSuit = MakeBool(Plugin.instance.Config, "Fun Commands (On/Off)", "TerminalRandomSuit", true, "Command to switch your suit from a random one off the rack");
            AddManagedBool(TerminalRandomSuit, defaultManaged, false, "FUN", RandomSuitKeywords, TerminalEvents.RandomSuit);

            TerminalClockCommand = MakeBool(Plugin.instance.Config, "Controls Commands (On/Off)", "TerminalClockCommand", false, "Command to toggle the Terminal Clock off/on");
            AddManagedBool(TerminalClockCommand, defaultManaged, false, "CONTROLS", ClockKeywords, TerminalEvents.ClockToggle);

            TerminalListItems = MakeBool(Plugin.instance.Config, "Extras Commands (On/Off)", "TerminalListItems", true, "Command to list all non-scrap & not currently held items on the ship");
            AddManagedBool(TerminalListItems, defaultManaged, false, "EXTRAS", ListItemsKeywords, MoreCommands.GetItemsOnShip);

            TerminalLootDetail = MakeBool(Plugin.instance.Config, "Extras Commands (On/Off)", "TerminalLootDetail", true, "Command to display an extensive list of all scrap on the ship");
            AddManagedBool(TerminalLootDetail, defaultManaged, false, "EXTRAS", ListScrapKeywords, AllTheLootStuff.DetailedLootCommand);

            TerminalRefund = MakeBool(Plugin.instance.Config, "Extras Commands (On/Off)", "TerminalRefund", true, "Command to cancel an undelivered order and get your credits back");
            AddManagedBool(TerminalRefund, defaultManaged, true, "EXTRAS", RefundKeywords, CostCommands.GetRefund);

            TerminalPrevious = MakeBool(Plugin.instance.Config, "Extras Commands (On/Off)", "TerminalPrevious", true, "Command to switch back to previous radar target");
            AddManagedBool(TerminalPrevious, defaultManaged, false, "EXTRAS", PreviousKeywords, ViewCommands.HandlePreviousSwitchEvent);

            TerminalRouteRandom = MakeBool(Plugin.instance.Config, "Fun Commands (On/Off)", "TerminalRouteRandom", true, "Command to route to a random planet");
            AddManagedBool(TerminalRouteRandom, defaultManaged, true, "FUN", RandomRouteKeywords, LevelCommands.RouteRandomCommand);

            TerminalRefreshCustomization = MakeBool(Plugin.instance.Config, "Fun Commands (On/Off)", "TerminalRefreshCustomization", false, "Command to reload the Terminal Customization settings (this will not disable any already applied customizations)");
            AddManagedBool(TerminalRefreshCustomization, defaultManaged, false, "FUN", RefreshcustomizationKWs, TerminalEvents.RefreshCustomizationCommand);
            TerminalRadarZoom = MakeBool(Plugin.instance.Config, "Controls Commands (On/Off)", "TerminalRadarZoom", false, "Command to cycle through various radar zoom levels.");
            AddManagedBool(TerminalRadarZoom, defaultManaged, false, "CONTROLS", RadarZoomKWs, ViewCommands.RadarZoomEvent, 0, true, null, null, "", "", "radarZoom");

            //------------------------------------------------MANAGED BOOLS END------------------------------------------------//

            //NOT MANAGED BOOLS THAT ARE COMMANDS, DEFINE THESE COMMANDS LATER THAN TERMINAL AWAKE
            TerminalTP = MakeBool(Plugin.instance.Config, "Controls Commands (On/Off)", "TerminalTP", true, "Command to Activate Teleporter <TP>");
            TerminalITP = MakeBool(Plugin.instance.Config, "Controls Commands (On/Off)", "TerminalITP", true, "Command to Activate Inverse Teleporter <ITP>");
            TerminalPurchasePacks = MakeBool(Plugin.instance.Config, "Comfort Commands (On/Off)", "TerminalPurchasePacks", false, "Use [PurchasePackCommands] to create purchase packs that contain multiple store items in one run of the command");
            TerminalMoonsPlus = MakeBool(Plugin.instance.Config, "MoonsPlus", "TerminalMoonsPlus", false, "Enable/Disable the Moons Plus page for an interactive menu to select moons from.\nWARNING: This feature has limited compatibility testing with other mods that modify the vanilla moons page and mods that affect where you can route.");
            TerminalStorePlus = MakeBool(Plugin.instance.Config, "StorePlus", "TerminalStorePlus", false, "Enable/Disable the Store Plus page for an interactive menu to select shop items from.\nWARNING: This feature has limited compatibility testing with other mods that modify the vanilla store page and mods that affect what you can buy.");
            //NOT MANAGED BOOLS THAT ARE COMMANDS, DEFINE THESE COMMANDS LATER THAN TERMINAL AWAKE
        }

        internal static void CommandDefinitions()
        {
            MoonsPlus.MoonsCommand = new("MoonsPlus", TerminalMoonsPlus, MoonsPlusConfig.MoonsPlusKeywords, MoonsPlus.EnterMoonsMenu)
            {
                AddAtAwake = false
            };

            StorePlus.StoreCommand = new("StorePlus", TerminalStorePlus, StorePlusConfig.StorePlusKeywords, StorePlus.EnterStoreMenu)
            {
                AddAtAwake = false
            };

            Teleporters.Inverse = new("Use Inverse Teleporter", TerminalITP, ItpKeywords, ShipControls.InverseTeleporterCommand)
            {
                AddAtAwake = false
            };

            Teleporters.Regular = new("Use Teleporter", TerminalTP, TpKeywords, ShipControls.RegularTeleporterCommand)
            {
                AddAtAwake = false,
                AcceptAdditionalText = true,
            };

            StuffForLibrary.Switch = new("SwitchedCam", SwitchKeywords, ViewCommands.SwitchCommandHandler)
            {
                AddAtAwake = false,
                AcceptAdditionalText = true
            };

            StuffForLibrary.Bind = new("bindCommand", TerminalShortcutCommands, ["bind"], DynamicCommands.BindKeyToCommand)
            {
                
                AddAtAwake = false,
                AcceptAdditionalText = true
            };

            StuffForLibrary.Unbind = new("SwitchedCam", SwitchKeywords, ViewCommands.SwitchCommandHandler)
            {
                AddAtAwake = false,
                AcceptAdditionalText = true
            };
        }
    }
}

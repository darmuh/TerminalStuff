using BepInEx.Configuration;
using OpenLib.ConfigManager;
using OpenLib.CoreMethods;
using System.Collections.Generic;
using static OpenLib.Common.CommonTerminal;
using static OpenLib.ConfigManager.ConfigSetup;

namespace TerminalStuff
{
    public static class ConfigSettings
    {
        public static List<ManagedConfig> TerminalStuffBools = [];
        public static MainListing TerminalStuffMain;
        public static List<ManagedConfig> ViewConfig = [];

        //keybinds
        public static ConfigEntry<string> WalkieTermKey { get; internal set; }
        public static ConfigEntry<string> WalkieTermMB { get; internal set; }
        public static ConfigEntry<string> KeyActionsConfig { get; internal set; }
        public static ConfigEntry<bool> CreateMoreMenus { get; internal set; }
        public static ConfigEntry<bool> FauxMoreMenu { get; internal set; }

        //cams special
        public static ConfigEntry<bool> CamsUseDetectedMods { get; internal set; }
        public static ConfigEntry<bool> ObcRequireUpgrade { get; internal set; }
        public static ConfigEntry<string> ObcResolutionMirror { get; internal set; }
        public static ConfigEntry<string> ObcResolutionBodyCam { get; internal set; }
        public static ConfigEntry<float> MirrorZoom { get; internal set; }
        public static ConfigEntry<bool> Mirror2DStyle { get; internal set; }

        //establish commands that can be turned on or off here
        public static ConfigEntry<bool> TerminalShortcutCommands { get; internal set; }
        public static ConfigEntry<bool> ModNetworking { get; internal set; }
        public static ConfigEntry<bool> NetworkedNodes { get; internal set; } //enable or disable networked terminal nodes (beta)
        public static ConfigEntry<bool> TerminalClock { get; internal set; } //Clock object itself
        public static ConfigEntry<bool> ExtensiveLogging { get; internal set; }
        public static ConfigEntry<bool> DeveloperLogging { get; internal set; }
        public static ConfigEntry<bool> WalkieTerm { get; internal set; } //Use walkie at terminal function
        public static ConfigEntry<bool> TerminalShortcuts { get; internal set; } //adds the bind keyword and the enumerator checking for shortcuts
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

        //features
        public static ConfigEntry<bool> TerminalPurchasePacks { get; internal set; } // purchase packs feature

        //Strings for display messages
        public static ConfigEntry<bool> CanOpenDoorInSpace { get; internal set; } //bool to allow for opening door in space
        public static ConfigEntry<string> DoorOpenString { get; internal set; } //Door String
        public static ConfigEntry<string> DoorCloseString { get; internal set; } //Door String
        public static ConfigEntry<string> DoorSpaceString { get; internal set; } //Door String
        public static ConfigEntry<string> QuitString { get; internal set; } //Quit String
        public static ConfigEntry<string> LeverString { get; internal set; } //Lever String
        public static ConfigEntry<string> VideoStartString { get; internal set; } //lol, start video string
        public static ConfigEntry<string> VideoStopString { get; internal set; } //lol, stop video string
        public static ConfigEntry<string> TpMessageString { get; internal set; } //TP Message String
        public static ConfigEntry<string> ItpMessageString { get; internal set; } //TP Message String
        public static ConfigEntry<string> VitalsPoorString { get; internal set; } //Vitals can't afford string
        public static ConfigEntry<string> VitalsUpgradePoor { get; internal set; } //Vitals Upgrade can't afford string
        public static ConfigEntry<string> HealIsFullString { get; internal set; } //full health string
        public static ConfigEntry<string> HealString { get; internal set; } //healing player string
        public static ConfigEntry<string> CamOnString { get; internal set; } //Cameras on string
        public static ConfigEntry<string> CamOffString { get; internal set; } //Cameras off string
        public static ConfigEntry<string> MapOnString { get; internal set; } //map on string
        public static ConfigEntry<string> MapOffString { get; internal set; } //map off string
        public static ConfigEntry<string> OverlayOnString { get; internal set; } //overlay on string
        public static ConfigEntry<string> OverlayOffString { get; internal set; } //overlay off string
        public static ConfigEntry<string> MiniMapOnString { get; internal set; } //minimap on string
        public static ConfigEntry<string> MiniMapOffString { get; internal set; } //minimap off string
        public static ConfigEntry<string> MiniCamsOnString { get; internal set; } //minicam on string
        public static ConfigEntry<string> MiniCamsOffString { get; internal set; } //minicam off string


        //Cost configs
        public static ConfigEntry<int> VitalsCost { get; internal set; } //Cost of Vitals Command
        public static ConfigEntry<int> VitalsUpgradeCost { get; internal set; } //Cost of Vitals Upgrade Command
        public static ConfigEntry<int> BioScanUpgradeCost { get; internal set; } //Cost of Enemy Scan Upgrade Command
        public static ConfigEntry<int> BioScanCost { get; internal set; } //Cost of Enemy Scan Command

        //Other config items
        public static ConfigEntry<int> GambleMinimum { get; internal set; } //Minimum amount of credits needed to gamble
        public static ConfigEntry<bool> GamblePityMode { get; internal set; } //enable or disable pity for gamblers
        public static ConfigEntry<int> GamblePityCredits { get; internal set; } //Pity Credits for losers
        public static ConfigEntry<string> GamblePoorString { get; internal set; } //gamble credits too low string
        public static ConfigEntry<string> VideoFolderPath { get; internal set; } //Specify a different folder with videos
        public static ConfigEntry<bool> VideoSync { get; internal set; } //Should videos be synced between players (good for AOD)
        public static ConfigEntry<bool> AlwaysUniqueVideo { get; internal set; }
        public static ConfigEntry<bool> LeverConfirmOverride { get; internal set; } //disable confirmation check for lever
        public static ConfigEntry<bool> RestartConfirmOverride { get; internal set; } //disable confirmation check for lever
        public static ConfigEntry<bool> CamsNeverHide { get; internal set; }
        public static ConfigEntry<int> OverlayOpacity { get; internal set; } //Opacity Percentage for Overlay Cams View
        public static ConfigEntry<string> CustomLink { get; internal set; }
        public static ConfigEntry<string> CustomLink2 { get; internal set; }
        public static ConfigEntry<string> CustomLinkHint { get; internal set; }
        public static ConfigEntry<string> CustomLink2Hint { get; internal set; }
        public static ConfigEntry<string> HomeLine1 { get; internal set; }
        public static ConfigEntry<string> HomeLine2 { get; internal set; }
        public static ConfigEntry<string> HomeLine3 { get; internal set; }
        public static ConfigEntry<string> HomeHelpLines { get; internal set; }
        public static ConfigEntry<string> HomeTextArt { get; internal set; }
        public static ConfigEntry<string> MoreMenuText { get; internal set; }
        public static ConfigEntry<string> MoreHintText { get; internal set; }
        public static ConfigEntry<string> TerminalScreen { get; internal set; }
        public static ConfigEntry<bool> ScreenOnWhileDead { get; internal set; }
        public static ConfigEntry<int> ScreenOffDelay { get; internal set; }
        public static ConfigEntry<string> RouteRandomBannedWeather { get; internal set; }
        public static ConfigEntry<int> RouteRandomCost { get; internal set; }
        public static ConfigEntry<bool> RouteOnlyInCurrentConstellation { get; internal set; }
        public static ConfigEntry<string> PurchasePackCommands { get; internal set; }

        //keywords
        public static ConfigEntry<string> AlwaysOnKeywords { get; internal set; } //string to match keyword
        public static ConfigEntry<string> MinimapKeywords { get; internal set; }
        public static ConfigEntry<string> MinicamsKeywords { get; internal set; }
        public static ConfigEntry<string> OverlayKeywords { get; internal set; }
        public static ConfigEntry<string> DoorKeywords { get; internal set; }
        public static ConfigEntry<string> LightsKeywords { get; internal set; }
        public static ConfigEntry<string> ModsKeywords { get; internal set; }
        public static ConfigEntry<string> TpKeywords { get; internal set; }
        public static ConfigEntry<string> ItpKeywords { get; internal set; }
        public static ConfigEntry<string> SwitchKeywords { get; internal set; }
        public static ConfigEntry<string> QuitKeywords { get; internal set; }
        public static ConfigEntry<string> VideoKeywords { get; internal set; }
        public static ConfigEntry<string> ClearKeywords { get; internal set; }
        public static ConfigEntry<string> DangerKeywords { get; internal set; }
        public static ConfigEntry<string> HealKeywords { get; internal set; }
        public static ConfigEntry<string> LootKeywords { get; internal set; }
        public static ConfigEntry<string> CamsKeywords { get; internal set; }
        public static ConfigEntry<string> MapKeywords { get; internal set; }
        public static ConfigEntry<string> MirrorKeywords { get; internal set; }
        public static ConfigEntry<string> RandomSuitKeywords { get; internal set; }
        public static ConfigEntry<string> ClockKeywords { get; internal set; }
        public static ConfigEntry<string> ListItemsKeywords { get; internal set; } //List Items Command
        public static ConfigEntry<string> ListScrapKeywords { get; internal set; } //List Scrap Command
        public static ConfigEntry<string> RandomRouteKeywords { get; internal set; }
        public static ConfigEntry<string> LobbyKeywords { get; internal set; } //show lobby name keywords
        public static ConfigEntry<string> RefreshcustomizationKWs { get; internal set; }
        public static ConfigEntry<string> RadarZoomKWs { get; internal set; }

        public static ConfigEntry<string> FovKeywords { get; internal set; }
        public static ConfigEntry<string> KickKeywords { get; internal set; }
        public static ConfigEntry<string> FcolorKeywords { get; internal set; }
        public static ConfigEntry<string> GambleKeywords { get; internal set; }
        public static ConfigEntry<string> LeverKeywords { get; internal set; }
        public static ConfigEntry<string> ScolorKeywords { get; internal set; }
        public static ConfigEntry<string> LinkKeywords { get; internal set; }
        public static ConfigEntry<string> Link2Keywords { get; internal set; }
        public static ConfigEntry<string> RefundKeywords { get; internal set; }
        public static ConfigEntry<string> PreviousKeywords { get; internal set; }
        public static ConfigEntry<string> RestartKeywords { get; internal set; }

        //Quality of Life features
        public static ConfigEntry<bool> LockCameraInTerminal { get; internal set; }
        public static ConfigEntry<string> TerminalLightBehaviour { get; internal set; }
        public static ConfigEntry<bool> TerminalAutoComplete { get; internal set; }
        public static ConfigEntry<string> TerminalAutoCompleteKey { get; internal set; }
        public static ConfigEntry<int> TerminalAutoCompleteMaxCount { get; internal set; }
        public static ConfigEntry<bool> TerminalHistory { get; internal set; }
        public static ConfigEntry<int> TerminalHistoryMaxCount { get; internal set; }
        public static ConfigEntry<bool> TerminalConflictResolution { get; internal set; }
        public static ConfigEntry<float> TerminalRadarDefaultZoom { get; internal set; }
        public static ConfigEntry<string> TerminalFillEmptyText { get; internal set; }
        public static ConfigEntry<string> TerminalStartPage { get; internal set; }
        public static ConfigEntry<bool> SaveLastInput { get; internal set; }
        public static ConfigEntry<int> TerminalInputMaxChars { get; internal set; }

        public static ConfigEntry<int> StartingCreds { get; internal set; }

        //Terminal Customization
        public static ConfigEntry<bool> TerminalCustomization { get; internal set; }
        public static ConfigEntry<string> TerminalColor { get; internal set; }
        public static ConfigEntry<string> TerminalButtonsColor { get; internal set; }
        public static ConfigEntry<string> TerminalKeyboardColor { get; internal set; }
        public static ConfigEntry<string> TerminalTextColor { get; internal set; }
        public static ConfigEntry<string> TerminalMoneyColor { get; internal set; }
        public static ConfigEntry<string> TerminalMoneyBGColor { get; internal set; }
        public static ConfigEntry<float> TerminalMoneyBGAlpha { get; internal set; }
        public static ConfigEntry<string> TerminalCaretColor { get; internal set; }
        public static ConfigEntry<string> TerminalScrollbarColor { get; internal set; }
        public static ConfigEntry<string> TerminalScrollBGColor { get; internal set; }
        public static ConfigEntry<string> TerminalClockColor { get; internal set; }
        public static ConfigEntry<string> TerminalLightColor { get; internal set; }
        public static ConfigEntry<bool> TerminalCustomBG { get; internal set; }
        public static ConfigEntry<string> TerminalCustomBGColor { get; internal set; }
        public static ConfigEntry<float> TerminalCustomBGAlpha { get; internal set; }

        //font stuff
        public static ConfigEntry<string> CustomFontPath { get; internal set; }
        public static ConfigEntry<string> CustomFontName { get; internal set; }
        public static ConfigEntry<int> CustomFontSizeMain { get; internal set; }
        public static ConfigEntry<int> CustomFontSizeMoney { get; internal set; }
        public static ConfigEntry<int> CustomFontSizeClock { get; internal set; }

        //Cruiser Terminal
        public static ConfigEntry<string> CruiserTerminalFilterType { get; internal set; }
        public static ConfigEntry<string> CruiserKeywordList { get; internal set; }

        public static void BindConfigSettings()
        {
            //Network Configs
            ModNetworking = MakeBool(Plugin.instance.Config, "Networking", "ModNetworking", true, "Disable this if you want to disable networking and use this mod as a Client-sided mod");
            NetworkedNodes = MakeBool(Plugin.instance.Config, "Networking", "NetworkedNodes", true, "Enable networked Always-On Display & displaying synced terminal nodes");

            AddManagedBool(NetworkedNodes, defaultManaged, true, "", "");

            TerminalClock = MakeBool(Plugin.instance.Config, "Quality of Life", "TerminalClock", false, "Enable or Disable the TerminalClock feature in it's entirety");
            WalkieTerm = MakeBool(Plugin.instance.Config, "Quality of Life", "WalkieTerm", false, "Enable or Disable the ability to use a walkie from your inventory at the terminal (vanilla method will work regardless)");
            TerminalShortcuts = MakeBool(Plugin.instance.Config, "Quality of Life", "TerminalShortcuts", false, "Enable this for the ability to bind commands to any valid key (also enables the \"bind\" keyword.");
            ExtensiveLogging = MakeBool(Plugin.instance.Config, "Debug", "ExtensiveLogging", false, "Enable or Disable extensive logging for this mod.");
            DeveloperLogging = MakeBool(Plugin.instance.Config, "Debug", "DeveloperLogging", false, "Enable or Disable developer logging for this mod. (this will fill your log file FAST)");
            KeyActionsConfig = MakeString(Plugin.instance.Config, "Quality of Life", "KeyActionsConfig", "", "Stored keybinds, don't modify this unless you know what you're doing!");
            CreateMoreMenus = MakeBool(Plugin.instance.Config, "Quality of Life", "CreateMoreMenus", true, "Set this to false to remove the More commands menu.\nIf disabled, any command added by this mod will be added to the 'Other' command listing");
            FauxMoreMenu = MakeBool(Plugin.instance.Config, "Quality of Life", "FauxMoreMenu", false, "Set this to true to use Faux Keywords for more menu category & next commands.\nCan be used in the case of certain category commands not being created due to existing keywords (ie. fun moon filter from LLL)");
            PurchasePackCommands = MakeString(Plugin.instance.Config, "Comfort Configuration", "PurchasePackCommands", "Essentials:pro,shov,walkie;PortalPack:teleporter,inverse", "List of purchase pack commands to create. Format is command:item1,item2,etc.;next command:item1,item2");

            Plugin.Spam("network configs section done");

            //keybinds
            WalkieTermKey = MakeString(Plugin.instance.Config, "Quality of Life", "WalkieTermKey", "LeftAlt", "Key used to activate your walkie while at the terminal, see here for valid key names https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/api/UnityEngine.InputSystem.Key.html");
            WalkieTermMB = MakeString(Plugin.instance.Config, "Quality of Life", "WalkieTermMB", "Left", "Mousebutton used to activate your walkie while at the terminal, see here for valid button names https://docs.unity3d.com/Packages/com.unity.inputsystem@1.3/api/UnityEngine.InputSystem.LowLevel.MouseButton.html");

            Plugin.Spam("keybind configs section done");

            //Cams Mod Config
            CamsUseDetectedMods = MakeBool(Plugin.instance.Config, "Extras Configuration", "CamsUseDetectedMods", true, "With this enabled, this mod will detect if another mod that adds player cams is enabled and use the mod's camera for all cams commands. Currently detects the following: Helmet Cameras by Rick Arg, Body Cameras by Solo, OpenBodyCams by Zaggy1024");

            ObcRequireUpgrade = MakeBool(Plugin.instance.Config, "Extras Configuration", "ObcRequireUpgrade", true, "With this enabled (and CamsUseDetectedMods), cams views will not be available until the bodycam upgrade from OpenBodyCams has been unlocked.");

            //override configs
            LeverConfirmOverride = MakeBool(Plugin.instance.Config, "Controls Configuration", "LeverConfirmOverride", false, "Setting this to true will disable the confirmation check for the <lever> command.");
            RestartConfirmOverride = MakeBool(Plugin.instance.Config, "Controls Configuration", "RestartConfirmOverride", false, "Setting this to true will disable the confirmation check for the <restart> command.");

            //Keyword configs (multiple per config item)
            AlwaysOnKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "AlwaysOnKeywords", "alwayson;always on", "This semi-colon separated list is all keywords that can be used in terminal to return <alwayson> command");
            CamsKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "CamsKeywords", "cameras; show cams; cams", "This semi-colon separated list is all keywords that can be used in terminal to return <cams> command");
            MapKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "MapKeywords", "show map; map", "Additional This semi-colon separated list is all keywords that can be used in terminal to return <map> command");
            MinimapKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "MinimapKeywords", "minimap; show minimap", "This semi-colon separated list is all keywords that can be used in terminal to return <minimap> command.");
            MinicamsKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "MinicamsKeywords", "minicams; show minicams", "This semi-colon separated list is all keywords that can be used in terminal to return <minicams> command");
            OverlayKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "OverlayKeywords", "overlay; show overlay", "This semi-colon separated list is all keywords that can be used in terminal to return <overlay> command");
            MirrorKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "MirrorKeywords", "mirror; reflection; show mirror", "This semi-colon separated list is all keywords that can be used in terminal to return <cams> command");
            DoorKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "DoorKeywords", "door; toggle door", "This semi-colon separated list is all keywords that can be used in terminal to return <door> command");
            LightsKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "LightsKeywords", "lights; toggle lights", "This semi-colon separated list is all keywords that can be used in terminal to return <lights> command");
            ModsKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "ModsKeywords", "modlist; show mods", "This semi-colon separated list is all keywords that can be used in terminal to return <mods> command");
            TpKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "TpKeywords", "tp; use teleporter; teleport", "This semi-colon separated list is all keywords that can be used in terminal to return <tp> command");
            ItpKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "ItpKeywords", "itp; use inverse; inverse", "This semi-colon separated list is all keywords that can be used in terminal to return <itp> command");
            SwitchKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "SwitchKeywords", "switch; view next", "This semi-colon separated list is all keywords that can be used in terminal to return <switch> command");
            QuitKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "QuitKeywords", "quit;exit;leave", "This semi-colon separated list is all keywords that can be used in terminal to return <quit> command");
            VideoKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "VideoKeywords", "lol; play video", "This semi-colon separated list is all keywords that can be used in terminal to return <video> command");
            ClearKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "ClearKeywords", "clear;wipe", "This semi-colon separated list is all keywords that can be used in terminal to return <clear> command");
            DangerKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "DangerKeywords", "danger;hazard;show danger; show hazard", "This semi-colon separated list is all keywords that can be used in terminal to return <danger> command");
            HealKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "HealKeywords", "heal me; heal", "This semi-colon separated list is all keywords that can be used in terminal to return <heal> command");
            LootKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "LootKeywords", "loot; shiploot", "This semi-colon separated list is all keywords that can be used in terminal to return <loot> command");
            RandomSuitKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "RandomSuitKeywords", "randomsuit; random suit", "This semi-colon separated list is all keywords that can be used in terminal to return <randomsuit> command");
            ClockKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "ClockKeywords", "clock; show clock; time", "This semi-colon separated list is all keywords that can be used in terminal to toggle Terminal Clock display");
            ListItemsKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "ListItemsKeywords", "show items; get items; list items", "This semi-colon separated list is all keywords that can be used in terminal to return <itemlist> command");
            ListScrapKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "ListScrapKeywords", "loot detail; loot list", "This semi-colon separated list is all keywords that can be used in terminal to return <lootlist> command");
            RandomRouteKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "RandomRouteKeywords", "route random; random moon", "This semi-colon separated list is all keywords that can be used in terminal to return <randomRoute> command");
            LobbyKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "LobbyKeywords", "show lobby; lobby name", "This semi-colon separated list is all keywords that can be used in terminal to return <lobby> command");
            RefreshcustomizationKWs = MakeString(Plugin.instance.Config, "Custom Keywords", "RefreshcustomizationKWs", "refresh colors; paintme; customize", "This semi-colon separated list is all keywords that can be used to run the TerminalRefreshCustomization command");
            RadarZoomKWs = MakeString(Plugin.instance.Config, "Custom Keywords", "RadarZoomKWs", "zoom; enhance; radar zoom", "This semi-colon separated list is all keywords that can be used in terminal to return <lootlist> command");

            Plugin.Spam("keyword configs section done");

            //terminal patcher keywords
            FcolorKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "FcolorKeywords", "fcolor", "This semi-colon separated list is all keywords that can be used in terminal to return <fcolor> command");
            GambleKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "GambleKeywords", "gamble", "This semi-colon separated list is all keywords that can be used in terminal to return <gamble> command");
            LeverKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "LeverKeywords", "lever", "This semi-colon separated list is all keywords that can be used in terminal to return <lever> command");
            ScolorKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "ScolorKeywords", "scolor", "This semi-colon separated list is all keywords that can be used in terminal to return <scolor> command");
            LinkKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "LinkKeywords", "link", "This semi-colon separated list is all keywords that can be used in terminal to return <link> command");
            Link2Keywords = MakeString(Plugin.instance.Config, "Custom Keywords", "Link2Keywords", "link2", "This semi-colon separated list is all keywords that can be used in terminal to return <link2> command");
            KickKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "KickKeywords", "kick; italy", "This semi-colon separated list is all keywords that can be used in terminal to return <kick> command");
            FovKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "FovKeywords", "fov", "This semi-colon separated list is all keywords that can be used in terminal to return <fov> command");
            //refund,previous,restart
            RefundKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "RefundKeywords", "refund; cancel", "This semi-colon separated list is all keywords that can be used in terminal to return <refund> command");
            PreviousKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "PreviousKeywords", "previous; goback", "This semi-colon separated list is all keywords that can be used in terminal to return <previous> command");
            RestartKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "RestartKeywords", "restart; reset", "This semi-colon separated list is all keywords that can be used in terminal to return <restart> command");


            Plugin.Spam("special keyword configs section done");

            RouteRandomBannedWeather = MakeString(Plugin.instance.Config, "Fun Configuration", "RouteRandomBannedWeather", "Eclipsed;Flooded;Foggy", "This semi-colon separated list will be used to exclude moons from the route random command");
            RouteRandomCost = MakeClampedInt(Plugin.instance.Config, "Fun Configuration", "RouteRandomCost", 100, "Flat rate for running the route random command to get a random moon...", 0, 99999);
            RouteOnlyInCurrentConstellation = MakeBool(Plugin.instance.Config, "Fun Configuration", "RouteOnlyInCurrentConstellation", true, "When LethalConstellations mod is present, setting this to true will only choose a random moon within the current constellation");

            //Cost configs
            VitalsCost = MakeInt(Plugin.instance.Config, "Upgrades", "VitalsCost", 10, "Credits cost to run Vitals Command each time it's run.");
            VitalsUpgradeCost = MakeInt(Plugin.instance.Config, "Upgrades", "VitalsUpgradeCost", 200, "Credits cost to upgrade Vitals command to not cost credits anymore.");
            BioScanUpgradeCost = MakeInt(Plugin.instance.Config, "Upgrades", "BioScanUpgradeCost", 300, "Credits cost to upgrade Bioscan command to provide detailed information on scanned enemies.");
            BioScanCost = MakeInt(Plugin.instance.Config, "Upgrades", "BioScanCost", 15, "Credits cost to run Bioscan command each time it's run. (scans for enemy information)");

            Plugin.Spam("cost configs section done");

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
                AddManagedBool(TerminalRestart, defaultManaged, true, "CONTROLS", RestartKeywords, SpecialConfirmationLogic.RestartAction);
            }
            else
            {
                TerminalRestart = MakeBool(Plugin.instance.Config, "Controls Commands (On/Off)", "TerminalRestart", true, "Command to restart the lobby (skips firing sequence)");
                AddManagedBool(TerminalRestart, defaultManaged, true, "CONTROLS", RestartKeywords, SpecialConfirmationLogic.RestartAsk, 1, true, SpecialConfirmationLogic.RestartAction, SpecialConfirmationLogic.RestartDeny);
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
            AddManagedBool(TerminalVitalsUpgrade, defaultManaged, true, "EXTRAS", "vitalspatch", CostCommands.AskVitalsUpgrade, 2, true, CostCommands.PerformVitalsUpgrade, null, "", "You have opted out of purchasing the Vitals Scanner Upgrade.\n\n", "", -1, "   ", "", VitalsUpgradeCost.Value, "VitalsPatch", true, 1);

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
            //NOT MANAGED BOOLS THAT ARE COMMANDS, DEFINE THESE COMMANDS LATER THAN TERMINAL AWAKE

            //String Configs
            DoorOpenString = MakeString(Plugin.instance.Config, "Controls Configuration", "DoorOpenString", "Opening door.", "Message returned on door (open) command.");
            DoorCloseString = MakeString(Plugin.instance.Config, "Controls Configuration", "DoorCloseString", "Closing door.", "Message returned on door (close) command.");
            DoorSpaceString = MakeString(Plugin.instance.Config, "Controls Configuration", "DoorSpaceString", "Can't open doors in space.", "Message returned on door (inSpace) command.");
            CanOpenDoorInSpace = MakeBool(Plugin.instance.Config, "Controls Configuration", "CanOpenDoorInSpace", true, "Allow/Disallow for using the terminal to press the button to open the door in space.\nDoes not change whether the door can actually be opened.");
            QuitString = MakeString(Plugin.instance.Config, "Comfort Configuration", "QuitString", "goodbye!", "Message returned on quit command.");
            LeverString = MakeString(Plugin.instance.Config, "Controls Configuration", "LeverString", "PULLING THE LEVER!!!", "Message returned on lever pull command.");
            VideoStartString = MakeString(Plugin.instance.Config, "Fun Configuration", "VideoStartString", "lol.", "Message displayed when first playing a video.");
            VideoStopString = MakeString(Plugin.instance.Config, "Fun Configuration", "VideoStopString", "No more lol.", "Message displayed if you want to end video playback early.");
            TpMessageString = MakeString(Plugin.instance.Config, "Controls Configuration", "TpMessageString", "Teleport Button pressed.", "Message returned when TP command is run.");
            ItpMessageString = MakeString(Plugin.instance.Config, "Controls Configuration", "ItpMessageString", "Inverse Teleport Button pressed.", "Message returned when ITP command is run.");
            VitalsPoorString = MakeString(Plugin.instance.Config, "Upgrades", "VitalsPoorString", "You can't afford to run this command.", "Message returned when you don't have enough credits to run the <Vitals> command.");
            VitalsUpgradePoor = MakeString(Plugin.instance.Config, "Upgrades", "VitalsUpgradePoor", "You can't afford to upgrade the Vitals Scanner.", "Message returned when you don't have enough credits to unlock the vitals scanner upgrade.");
            HealIsFullString = MakeString(Plugin.instance.Config, "Comfort Configuration", "HealIsFullString", "You are full health!", "Message returned when heal command is run and player is already full health.");
            HealString = MakeString(Plugin.instance.Config, "Comfort Configuration", "HealString", "The terminal healed you?!?", "Message returned when heal command is run and player is healed.");
            CamOnString = MakeString(Plugin.instance.Config, "Extras Configuration", "CamOnString", "(CAMS)", "Message returned when enabling Cams command (cams).");
            CamOffString = MakeString(Plugin.instance.Config, "Extras Configuration", "CamOffString", "Cameras disabled.", "Message returned when disabling Cams command (cams).");
            MapOnString = MakeString(Plugin.instance.Config, "Extras Configuration", "MapOnString", "(MAP)", "Message returned when enabling map command (map).");
            MapOffString = MakeString(Plugin.instance.Config, "Extras Configuration", "MapOffString", "Map View disabled.", "Message returned when disabling map command (map).");
            OverlayOnString = MakeString(Plugin.instance.Config, "Extras Configuration", "OverlayOnString", "(Overlay)", "Message returned when enabling Overlay command (overlay).");
            OverlayOffString = MakeString(Plugin.instance.Config, "Extras Configuration", "OverlayOffString", "Overlay disabled.", "Message returned when disabling Overlay command (overlay).");
            MiniMapOnString = MakeString(Plugin.instance.Config, "Extras Configuration", "MiniMapOnString", "(MiniMap)", "Message returned when enabling minimap command (minimap).");
            MiniMapOffString = MakeString(Plugin.instance.Config, "Extras Configuration", "MiniMapOffString", "MiniMap disabled.", "Message returned when disabling minimap command (minimap).");
            MiniCamsOnString = MakeString(Plugin.instance.Config, "Extras Configuration", "MiniCamsOnString", "(MiniCams)", "Message returned when enabling minicams command (minicams).");
            MiniCamsOffString = MakeString(Plugin.instance.Config, "Extras Configuration", "MiniCamsOffString", "MiniCams disabled.", "Message returned when disabling minicams command (minicams).");

            CustomLink = MakeString(Plugin.instance.Config, "Extras Configuration", "CustomLink", "https://thunderstore.io/c/lethal-company/p/darmuh/darmuhsTerminalStuff/", "URL to send players to when using the \"link\" command.");
            CustomLinkHint = MakeString(Plugin.instance.Config, "Extras Configuration", "CustomLinkHint", "Go to the thunderstore listing for this mod.", "Hint given to players in extras menu for \"link\" command.");
            CustomLink2 = MakeString(Plugin.instance.Config, "Extras Configuration", "CustomLink2", "https://github.com/darmuh/TerminalStuff", "URL to send players to when using the second \"link\" command.");
            CustomLink2Hint = MakeString(Plugin.instance.Config, "Extras Configuration", "CustomLink2Hint", "Go to the github for this mod.", "Hint given to players in extras menu for \"link\" command.");

            //Other configs
            GambleMinimum = MakeInt(Plugin.instance.Config, "Fun Configuration", "GambleMinimum", 0, "Credits needed to start gambling, 0 means you can gamble everything.");
            GamblePityMode = MakeBool(Plugin.instance.Config, "Fun Configuration", "GamblePityMode", false, "Enable Gamble Pity Mode, which gives credits back to those who lose everything.");
            GamblePityCredits = MakeClampedInt(Plugin.instance.Config, "Fun Configuration", "GamblePityCredits", 10, "If Gamble Pity Mode is enabled, specify how much Pity Credits are given to losers. (Max: 60)", 0, 60);
            GamblePoorString = MakeString(Plugin.instance.Config, "Fun Configuration", "GamblePoorString", "You don't meet the minimum credits requirement to gamble.", "Message returned when your credits is less than the GambleMinimum set.");
            VideoFolderPath = MakeString(Plugin.instance.Config, "Fun Configuration", "VideoFolderPath", "darmuh-darmuhsTerminalVideos", "Folder name where videos will be pulled from, needs to be in BepInEx/plugins");
            VideoSync = MakeBool(Plugin.instance.Config, "Fun Configuration", "VideoSync", true, "When networking is enabled, this setting will sync videos being played on the terminal for all players whose terminal screen is on.");
            AlwaysUniqueVideo = MakeBool(Plugin.instance.Config, "Fun Configuration", "AlwaysUniqueVideo", true, "When enabled, this setting will shuffle all of the videos into a list. Each time a video command is run it will play a video from the list until it reaches the end of the list. Then a new re-shuffled list will be created.");
            ObcResolutionMirror = MakeString(Plugin.instance.Config, "Extras Configuration", "ObcResolutionMirror", "1000; 700", "Set the resolution of the Mirror Camera created with OpenBodyCams for darmuhsTerminalStuff");
            ObcResolutionBodyCam = MakeString(Plugin.instance.Config, "Extras Configuration", "ObcResolutionBodyCam", "1000; 700", "Set the resolution of the Body Camera created with OpenBodyCams for darmuhsTerminalStuff");
            MirrorZoom = MakeClampedFloat(Plugin.instance.Config, "Extras Configuration", "MirrorZoom", 3.4f, "Set the mirror zoom level, the higher the value the more zoomed out the mirror will be.\nThis requires [Mirror2DStyle] to be enabled", 0.2f, 9f);
            Mirror2DStyle = MakeBool(Plugin.instance.Config, "Extras Configuration", "Mirror2DStyle", false, "Change whether the mirror will use Orthographic (2D) Styling.\n Old versions of this mod had this enabled by default.");
            CamsNeverHide = MakeBool(Plugin.instance.Config, "Extras Configuration", "CamsNeverHide", false, "Setting this to true will make it so no command will ever auto-hide any cams-type view.");
            OverlayOpacity = Plugin.instance.Config.Bind("Extras Configuration", "OverlayOpacity", 10, new ConfigDescription("Opacity percentage for Overlay View.", new AcceptableValueRange<int>(0, 100)));
            ScreenOnWhileDead = MakeBool(Plugin.instance.Config, "Quality of Life", "ScreenOnWhileDead", false, "Set this to true if you wish to keep the screen on after death when TerminalScreen is set to any mode that keeps the screen on.");
            ScreenOffDelay = MakeClampedInt(Plugin.instance.Config, "Quality of Life", "ScreenOffDelay", -1, "Set this to delay turning the terminal screen off by this many seconds after leaving the ship.", -1, 30);


            //homescreen lines
            HomeLine1 = MakeString(Plugin.instance.Config, "Terminal Customization", "homeline1", $"Welcome to the FORTUNE-9{Plugin.PluginInfo.PLUGIN_VERSION} OS PLUS", "First line of the home command (startup screen)");
            HomeLine2 = MakeString(Plugin.instance.Config, "Terminal Customization", "homeline2", "\tUpgraded by Employee: <color=#e6b800>darmuh</color>", "Second line of the home command (startup screen)");
            HomeLine3 = MakeString(Plugin.instance.Config, "Terminal Customization", "homeline3", "Have a wonderful [currentDay]!", "Last line of the home command (startup screen)");
            HomeHelpLines = MakeString(Plugin.instance.Config, "Terminal Customization", "HomeHelpLines", ">>Type \"Help\" for a list of commands.\r\n>>Type <color=#b300b3>\"More\"</color> for a menu of darmuh's commands.\r\n", "these two lines should generally be used to point to menus of other usable commands. Can also be expanded to more than two lines by using \"\\r\\n\" to indicate a new line");

            HomeTextArt = MakeString(Plugin.instance.Config, "Terminal Customization", "HomeTextArt", "[leadingSpacex4][leadingSpace]<color=#e6b800>^^      .-=-=-=-.  ^^\r\n ^^        (`-=-=-=-=-`)         ^^\r\n         (`-=-=-=-=-=-=-`)  ^^         ^^\r\n   ^^   (`-=-=-=-=-=-=-=-`)   ^^          \r\n       ( `-=-=-=-(@)-=-=-` )      ^^\r\n       (`-=-=-=-=-=-=-=-=-`)  ^^          \r\n       (`-=-=-=-=-=-=-=-=-`)  ^^\r\n        (`-=-=-=-=-=-=-=-`)          ^^\r\n         (`-=-=-=-=-=-=-`)  ^^            \r\n           (`-=-=-=-=-`)\r\n            `-=-=-=-=-`</color>", "ASCII Art goes here");

            MoreMenuText = MakeString(Plugin.instance.Config, "Terminal Customization", "MoreMenuText", "Welcome to darmuh's Terminal Upgrade!\r\n\tSee below Categories for new stuff :)", "This is the header of the more command menu\nRefreshing this text requires a lobby restart.");
            MoreHintText = MakeString(Plugin.instance.Config, "Terminal Customization", "MoreHintText", "<color=#b300b3>>MORE</color>\nTo open a menu of darmuh's commands.\nRefreshing this text requires a lobby restart.", "Text displayed for hints to the more command menu");

            //Quality of Life Stuff
            LockCameraInTerminal = MakeBool(Plugin.instance.Config, "Quality of Life", "LockCameraInTerminal", false, "Enable this to lock the player camera to the terminal when it is in use.");
            TerminalLightBehaviour = MakeClampedString(Plugin.instance.Config, "Quality of Life", "TerminalLightBehaviour", "alwayson", "Use this config item to change how the terminal light behaves. Options are 'nochange' which keeps vanilla behaviour, 'disable' which disables this light whenever you use it, and 'alwayson' which will keep the light on as long as the screen is on", new AcceptableValueList<string>("nochange", "disable", "alwayson"));
            TerminalScreen = MakeClampedString(Plugin.instance.Config, "Quality of Life", "TerminalScreen", "inship", "Use this config item to change how the terminal screen behaves. Options are 'nochange' which keeps vanilla behaviour, 'alwayson' which keeps the screen on at all times, 'inship' which will keep the screen on whenever you are in the ship, and 'inuse' which will keep the screen on whenever anyone is using the terminal.", new AcceptableValueList<string>("nochange", "alwayson", "inship", "inuse"));
            TerminalHistory = MakeBool(Plugin.instance.Config, "Quality of Life", "TerminalHistory", false, "(Requires TerminalShortcuts feature to function) With this feature enabled, uparrow and downarrow will cycle through a list of previously used commands.");
            TerminalHistoryMaxCount = MakeClampedInt(Plugin.instance.Config, "Quality of Life", "TerminalHistoryMaxCount", 9, "Max amount of previous commands to save in TerminalHistory list.", 3, 50);
            TerminalAutoComplete = MakeBool(Plugin.instance.Config, "Quality of Life", "TerminalAutoComplete", false, "(Requires TerminalShortcuts feature to function) With this feature enabled, tab key will cycle through a list of matching commands to the current input.");
            TerminalAutoCompleteKey = MakeString(Plugin.instance.Config, "Quality of Life", "TerminalAutoCompleteKey", "Tab", "Key used to activate TerminalAutoComplete feature https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/api/UnityEngine.InputSystem.Key.html");
            TerminalAutoCompleteMaxCount = MakeClampedInt(Plugin.instance.Config, "Quality of Life", "TerminalAutoCompleteMaxCount", 5, "Max amount of matching commands to store before disabling autocomplete.", 3, 50);
            TerminalConflictResolution = MakeBool(Plugin.instance.Config, "Quality of Life", "TerminalConflictResolution", false, "With this feature enabled, terminal command input will be weighted for conflict resolution using the Levenshtein algorithm.");
            TerminalRadarDefaultZoom = MakeClampedFloat(Plugin.instance.Config, "Quality of Life", "TerminalRadarDefaultZoom", 20f, "The default level zoom for the radar. The lower the number the more zoomed in you'll be.", 5f, 30f);
            TerminalFillEmptyText = MakeClampedString(Plugin.instance.Config, "Quality of Life", "TerminalFillEmptyText", "nochange", "AutoFill any node with empty space depending on your desired formatting", new AcceptableValueList<string>("nochange", "fillbottom", "textmiddle", "textbottom"));
            TerminalStartPage = MakeString(Plugin.instance.Config, "Quality of Life", "TerminalStartPage", "None", "Enter a keyword to load when a player begins using the terminal.");
            SaveLastInput = MakeBool(Plugin.instance.Config, "Quality of Life", "SaveLastInput", true, "Will save the input of the person last using the terminal and add it back when you start using it again");
            StartingCreds = MakeClampedInt(Plugin.instance.Config, "Quality of Life", "StartingCreds", -1, "Change this Quality of Life feature from -1 to set your desired starting credits amount\n-1 will leave starting credits unchanged by this mod.\nCapped at 20K, you shouldn't need more than this", -1, 20000);
            TerminalInputMaxChars = MakeClampedInt(Plugin.instance.Config, "Quality of Life", "TerminalInputMaxChars", -1, "Change this Quality of Life feature from -1 to set your desired Max Input Length to adjust node's max input that accepts at least 20 characters of input. Setting this to any number below 20 will do nothing.", -1, 999);


            //Terminal Customization
            TerminalCustomization = MakeBool(Plugin.instance.Config, "Terminal Customization", "TerminalCustomization", true, "Enable or Disable terminal color customizations");
            TerminalColor = MakeString(Plugin.instance.Config, "Terminal Customization", "TerminalColor", "#CFCFCF", "This changes the color of the physical terminal");
            TerminalButtonsColor = MakeString(Plugin.instance.Config, "Terminal Customization", "TerminalButtonsColor", "#B1D2FF", "This changes the color of the physical buttons on the terminal");
            TerminalKeyboardColor = MakeString(Plugin.instance.Config, "Terminal Customization", "TerminalKeyboardColor", "#878787", "This changes the color of the keyboard on the terminal");
            TerminalTextColor = MakeString(Plugin.instance.Config, "Terminal Customization", "TerminalTextColor", "#03E715", "This changes the color of the main text in the terminal");
            TerminalMoneyColor = MakeString(Plugin.instance.Config, "Terminal Customization", "TerminalMoneyColor", "#03E715", "This changes the color of the current credits text in the top left of the terminal");
            TerminalMoneyBGColor = MakeString(Plugin.instance.Config, "Terminal Customization", "TerminalMoneyBGColor", "#064F00", "This changes the color of the current credits text in the top left of the terminal");
            TerminalMoneyBGAlpha = MakeClampedFloat(Plugin.instance.Config, "Terminal Customization", "TerminalMoneyBGAlpha", 0.1f, "This changes the transparency of the money background color.", 0f, 1f);
            TerminalCaretColor = MakeString(Plugin.instance.Config, "Terminal Customization", "TerminalCaretColor", "#00BC0F", "This changes the color of the text caret in the terminal");
            TerminalScrollbarColor = MakeString(Plugin.instance.Config, "Terminal Customization", "TerminalScrollbarColor", "#075200", "This changes the color of the scrollbar in the terminal");
            TerminalScrollBGColor = MakeString(Plugin.instance.Config, "Terminal Customization", "TerminalScrollBGColor", "#075200", "This changes the color of the background box of the scrollbar in the terminal");
            TerminalClockColor = MakeString(Plugin.instance.Config, "Terminal Customization", "TerminalClockColor", "#03E715", "This changes the color of the clock element that is added to the terminal");
            TerminalLightColor = MakeString(Plugin.instance.Config, "Terminal Customization", "TerminalLightColor", "#DBFFBA", "This changes the color of the light that shines from the terminal");
            TerminalCustomBG = MakeBool(Plugin.instance.Config, "Terminal Customization", "TerminalCustomBG", false, "Enable or Disable custom background for the terminal screen");
            TerminalCustomBGColor = MakeString(Plugin.instance.Config, "Terminal Customization", "TerminalCustomBGColor", "#FFFFFF", "This changes the color of the custom background for the terminal screen");
            TerminalCustomBGAlpha = MakeClampedFloat(Plugin.instance.Config, "Terminal Customization", "TerminalCustomBGAlpha", 0.08f, "This changes the transparency of the custom background for the terminal screen", 0f, 1f);

            //Font Stuff
            CustomFontPath = MakeString(Plugin.instance.Config, "Terminal Customization", "CustomFontPath", "fonts", "If you want to share a profile code that includes the font file, put it in a folder with this name in the config folder. The default example would be \"BepInEx\\config\\fonts\"");
            CustomFontName = MakeString(Plugin.instance.Config, "Terminal Customization", "CustomFontName", "", "Name of the custom font you'd like to use in the terminal, leave blank or set to \"default\" to use the normal terminal font");
            CustomFontSizeMain = MakeClampedInt(Plugin.instance.Config, "Terminal Customization", "CustomFontSizeMain", -1, "Set a custom size for your custom font (main text), leave at -1 if you wish not to change it", -1, 72);
            CustomFontSizeMoney = MakeClampedInt(Plugin.instance.Config, "Terminal Customization", "CustomFontSizeMoney", -1, "Set a custom size for your font (credits at the top left), leave at -1 if you wish not to change it", -1, 72);
            CustomFontSizeClock = MakeClampedInt(Plugin.instance.Config, "Terminal Customization", "CustomFontSizeClock", -1, "Set a custom size for your font (TerminalClock), leave at -1 if you wish not to change it", -1, 72);
            CruiserTerminalConfigs();

            PluginCore.StuffForLibrary.ManualManagedBools(); //add more managedbools that dont come from a specific config item

            Plugin.MoreLogs("end of config setup");

            RemoveOrphanedEntries(Plugin.instance.Config);
            NetworkingCheck(ModNetworking.Value, Plugin.instance.Config, defaultManaged);
        }

        public static void CruiserTerminalConfigs()
        {
            CruiserTerminalFilterType = MakeClampedString(Plugin.instance.Config, "CruiserTerminal", "Cruiser Terminal Filter Type", "Deny", "Use this to set whether the Cruiser Keyword List is a list of keywords to permit or deny", new AcceptableValueList<string>("Deny", "Permit"));
            CruiserKeywordList = MakeString(Plugin.instance.Config, "CruiserTerminal", "Cruiser Terminal Keyword List", "mirror, reflection, show mirror, restart, tp, use teleporter, teleport, itp, use inverse, inverse, door, lights, toggle lights, lol, play video, route random, random moon, refresh colors, paintme, customize, scolor, fcolor, lever, link, link2, fov, restart, reset", "Comma-separated listing of keywords to permit OR deny depending on Cruiser Terminal Filter Type");
        }
    }
}
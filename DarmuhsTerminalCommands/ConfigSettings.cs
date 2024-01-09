using BepInEx.Configuration;
using JetBrains.Annotations;

namespace TerminalStuff
{
    public static class ConfigSettings
    {
        //establish commands that can be turned on or off here
        public static ConfigEntry<bool> ModNetworking;
        public static ConfigEntry<bool> terminalLobby; //lobby name command
        public static ConfigEntry<bool> terminalCams; //cams command
        public static ConfigEntry<bool> terminalQuit; //quit command
        public static ConfigEntry<bool> terminalClear; //clear command
        public static ConfigEntry<bool> terminalLoot; //loot command
        public static ConfigEntry<bool> terminalLol; //lol command
        public static ConfigEntry<bool> terminalHeal; //heal command
        public static ConfigEntry<bool> terminalFov; //Fov command
        public static ConfigEntry<bool> terminalGamble; //Gamble command
        public static ConfigEntry<bool> terminalLever; //Lever command
        public static ConfigEntry<bool> terminalDanger; //Danger command
        public static ConfigEntry<bool> terminalVitals; //Vitals command
        public static ConfigEntry<bool> terminalBioScan; //BioScan command
        public static ConfigEntry<bool> terminalVitalsUpgrade; //Vitals Upgrade command
        public static ConfigEntry<bool> terminalTP; //Teleporter command
        public static ConfigEntry<bool> terminalITP; //Inverse Teleporter command
        public static ConfigEntry<bool> terminalMods; //Modlist command
        public static ConfigEntry<bool> terminalKick; //Kick command (host only)
        public static ConfigEntry<bool> terminalFcolor; //Flashlight color command
        public static ConfigEntry<bool> terminalMap; //Map shortcut
        public static ConfigEntry<bool> terminalMinimap; //Minimap command
        public static ConfigEntry<bool> terminalMinicams; //Minicams command
        public static ConfigEntry<bool> terminalOverlay; //Overlay cams command
        public static ConfigEntry<bool> terminalDoor; //Door Toggle command
        public static ConfigEntry<bool> terminalLights; //Light Toggle command
        public static ConfigEntry<bool> terminalScolor; //Light colors command
        public static ConfigEntry<bool> terminalAlwaysOn; //AlwaysOn command
        public static ConfigEntry<bool> terminalLink; //Link command
        public static ConfigEntry<bool> terminalRandomSuit; //RandomSuit command


        //Strings for display messages
        public static ConfigEntry<string> doorOpenString; //Door String
        public static ConfigEntry<string> doorCloseString; //Door String
        public static ConfigEntry<string> doorSpaceString; //Door String
        public static ConfigEntry<string> quitString; //Quit String
        public static ConfigEntry<string> leverString; //Lever String
        public static ConfigEntry<string> kickString; //Kick String
        public static ConfigEntry<string> kickNoString; //Kick NO string
        public static ConfigEntry<string> kickNotHostString; //Kick not host string
        public static ConfigEntry<string> lolStartString; //lol, start video string
        public static ConfigEntry<string> lolStopString; //lol, stop video string
        public static ConfigEntry<string> tpMessageString; //TP Message String
        public static ConfigEntry<string> itpMessageString; //TP Message String
        public static ConfigEntry<string> vitalsPoorString; //Vitals can't afford string
        public static ConfigEntry<string> vitalsUpgradePoor; //Vitals Upgrade can't afford string
        public static ConfigEntry<string> healIsFullString; //full health string
        public static ConfigEntry<string> healString; //healing player string
        public static ConfigEntry<string> camString; //Cameras on string
        public static ConfigEntry<string> camString2; //Cameras off string
        public static ConfigEntry<string> mapString; //map on string
        public static ConfigEntry<string> mapString2; //map off string
        public static ConfigEntry<string> ovString; //overlay on string
        public static ConfigEntry<string> ovString2; //overlay off string
        public static ConfigEntry<string> mmString; //minimap on string
        public static ConfigEntry<string> mmString2; //minimap off string
        public static ConfigEntry<string> mcString; //minicam on string
        public static ConfigEntry<string> mcString2; //minicam off string


        //Cost configs
        public static ConfigEntry<int> vitalsCost; //Cost of Vitals Command
        public static ConfigEntry<int> vitalsUpgradeCost; //Cost of Vitals Upgrade Command
        public static ConfigEntry<int> bioScanUpgradeCost; //Cost of Enemy Scan Upgrade Command
        public static ConfigEntry<int> enemyScanCost; //Cost of Enemy Scan Command

        //Other config items
        public static ConfigEntry<int> gambleMinimum; //Minimum amount of credits needed to gamble
        public static ConfigEntry<bool> gamblePityMode; //enable or disable pity for gamblers
        public static ConfigEntry<int> gamblePityCredits; //Pity Credits for losers
        public static ConfigEntry<string> gamblePoorString; //gamble credits too low string
        public static ConfigEntry<string> videoFolderPath; //Specify a different folder with videos
        public static ConfigEntry<bool> leverConfirmOverride; //disable confirmation check for lever
        public static ConfigEntry<bool> camsNeverHide;
        public static ConfigEntry<bool> networkedNodes; //enable or disable networked terminal nodes (beta)
        public static ConfigEntry<string> defaultCamsView;
        public static ConfigEntry<int> ovOpacity; //Opacity Percentage for Overlay Cams View
        public static ConfigEntry<string> customLink;
        public static ConfigEntry<string> homeLine1;
        public static ConfigEntry<string> homeLine2;
        public static ConfigEntry<string> homeLine3;

        //keyword strings (terminalapi)
        public static ConfigEntry<string> alwaysOnKeyword; //string to match keyword
        public static ConfigEntry<string> minimapKeyword;
        public static ConfigEntry<string> minicamsKeyword;
        public static ConfigEntry<string> overlayKeyword;
        public static ConfigEntry<string> doorKeyword;
        public static ConfigEntry<string> lightsKeyword;
        public static ConfigEntry<string> modsKeyword2;
        public static ConfigEntry<string> tpKeyword2;
        public static ConfigEntry<string> itpKeyword2;
        public static ConfigEntry<string> quitKeyword2;
        public static ConfigEntry<string> lolKeyword;
        public static ConfigEntry<string> clearKeyword2;
        public static ConfigEntry<string> dangerKeyword;
        public static ConfigEntry<string> healKeyword2;
        public static ConfigEntry<string> lootKeyword2;
        public static ConfigEntry<string> camsKeyword2;
        public static ConfigEntry<string> mapKeyword2;
        public static ConfigEntry<string> randomSuitKeyword;

        //terminal patcher keywords
        public static ConfigEntry<string> fcolorKeyword;
        public static ConfigEntry<string> gambleKeyword;
        public static ConfigEntry<string> leverKeyword;
        public static ConfigEntry<string> scolorKeyword;
        public static ConfigEntry<string> linkKeyword;



        //terminal patcher words 
        //"lobby", "home", "more", "next", "comfort", "controls", "extras", "fun", "kick",
        // "fcolor", "fov", "gamble", "lever", "vitalspatch", "bioscan", "bioscanpatch", "scolor"

        public static void BindConfigSettings()
        {

            Plugin.Log.LogInfo("Binding configuration settings");

            //Network Configs
            ConfigSettings.networkedNodes = Plugin.instance.Config.Bind<bool>("Networked Things", "networkedNodes", false, "Enable networked Always-On Display & displaying synced terminal nodes (BETA)");
            ConfigSettings.ModNetworking = Plugin.instance.Config.Bind<bool>("__General", "ModNetworking", true, "Disable this if you want to disable networking and use this mod as a Client-sided mod");


            //enable or disable
            ConfigSettings.terminalLobby = Plugin.instance.Config.Bind<bool>("Comfort Commands (On/Off)", "terminalLobby", true, "Shows the current lobby name <Lobby Name>");
            ConfigSettings.terminalQuit = Plugin.instance.Config.Bind<bool>("Comfort Commands (On/Off)", "terminalQuit", true, "Command to quit terminal <Quit>");
            ConfigSettings.terminalClear = Plugin.instance.Config.Bind<bool>("Comfort Commands (On/Off)", "terminalClear", true, "Command to clear terminal text <Clear>");
            ConfigSettings.terminalLoot = Plugin.instance.Config.Bind<bool>("Extras Commands (On/Off)", "terminalLoot", true, "Command to show total onboard loot value <Loot>");
            ConfigSettings.terminalCams = Plugin.instance.Config.Bind<bool>("Extras Commands (On/Off)", "terminalCams", true, "Command to toggle displaying cameras in terminal <Cameras>");
            ConfigSettings.terminalLol = Plugin.instance.Config.Bind<bool>("Fun Commands (On/Off)", "terminalLol", true, "Play a funny video <lol>");
            ConfigSettings.terminalHeal = Plugin.instance.Config.Bind<bool>("Comfort Commands (On/Off)", "terminalHeal", true, "Command to heal yourself <Heal>");
            ConfigSettings.terminalFov = Plugin.instance.Config.Bind<bool>("Comfort Commands (On/Off)", "terminalFov", true, "Command to change your FOV <Fov>");
            ConfigSettings.terminalGamble = Plugin.instance.Config.Bind<bool>("Fun Commands (On/Off)", "terminalGamble", true, "Command to gamble your credits, by percentage <Gamble>");
            ConfigSettings.terminalLever = Plugin.instance.Config.Bind<bool>("Controls Commands (On/Off)", "terminalLever", true, "Pull the lever from terminal <Lever>");
            ConfigSettings.terminalDanger = Plugin.instance.Config.Bind<bool>("Controls Commands (On/Off)", "terminalDanger", true, "Check moon danger level <Danger>");
            ConfigSettings.terminalVitals = Plugin.instance.Config.Bind<bool>("Extras Commands (On/Off)", "terminalVitals", true, "Scan player being tracked by monitor for their Health/Weight. <Vitals>");
            ConfigSettings.terminalBioScan = Plugin.instance.Config.Bind<bool>("Extras Commands (On/Off)", "terminalBioScan", true, "Scan player being tracked by monitor for their Health/Weight. <BioScan>");
            ConfigSettings.terminalVitalsUpgrade = Plugin.instance.Config.Bind<bool>("Extras Commands (On/Off)", "terminalVitalsUpgrade", true, "Purchase-able upgrade to vitals to not cost anything each scan. <Vitals>");
            ConfigSettings.terminalTP = Plugin.instance.Config.Bind<bool>("Controls Commands (On/Off)", "terminalTP", true, "Command to Activate Teleporter <TP>");
            ConfigSettings.terminalITP = Plugin.instance.Config.Bind<bool>("Controls Commands (On/Off)", "terminalITP", true, "Command to Activate Inverse Teleporter <ITP>");
            ConfigSettings.terminalMods = Plugin.instance.Config.Bind<bool>("Comfort Commands (On/Off)", "terminalMods", true, "Command to see your active mods <Mods>");
            ConfigSettings.terminalKick = Plugin.instance.Config.Bind<bool>("Comfort Commands (On/Off)", "terminalKick", false, "Enables kick command for host. <Kick>");
            ConfigSettings.terminalFcolor = Plugin.instance.Config.Bind<bool>("Fun Commands (On/Off)", "terminalFcolor", true, "Command to change flashlight color. <Fcolor colorname>");
            ConfigSettings.terminalScolor = Plugin.instance.Config.Bind<bool>("Fun Commands (On/Off)", "terminalScolor", true, "Command to change ship lights colors. <Scolor all,front,middle,back colorname>");
            ConfigSettings.terminalDoor = Plugin.instance.Config.Bind<bool>("Controls Commands (On/Off)", "terminalDoor", true, "Command to open/close the ship door. <Door>");
            ConfigSettings.terminalLights = Plugin.instance.Config.Bind<bool>("Controls Commands (On/Off)", "terminalLights", true, "Command to toggle the ship lights");
            ConfigSettings.terminalMap = Plugin.instance.Config.Bind<bool>("Extras Commands (On/Off)", "terminalMap", true, "Adds 'map' shortcut to 'view monitor' command <Map>");
            ConfigSettings.terminalMinimap = Plugin.instance.Config.Bind<bool>("Extras Commands (On/Off)", "terminalMinimap", true, "Command to view cams with radar at the top right. <Minimap>");
            ConfigSettings.terminalMinicams = Plugin.instance.Config.Bind<bool>("Extras Commands (On/Off)", "terminalMinicams", true, "Command to view radar with cams at the top right. <Minicams>");
            ConfigSettings.terminalOverlay = Plugin.instance.Config.Bind<bool>("Extras Commands (On/Off)", "terminalOverlay", true, "Command to view cams with radar overlayed on top. <Overlay>");
            ConfigSettings.terminalAlwaysOn = Plugin.instance.Config.Bind<bool>("Comfort Commands (On/Off)", "terminalAlwaysOn", true, $"Command to toggle Always-On Display <Alwayson>");
            ConfigSettings.terminalLink = Plugin.instance.Config.Bind<bool>("Extras Commands (On/Off)", "terminalLink", true, "Command to link to an external web-page <Link>");
            ConfigSettings.terminalRandomSuit = Plugin.instance.Config.Bind<bool>("Fun Commands (On/Off)", "terminalRandomSuit", true, "Command to switch your suit from a random one off the rack <RandomSuit>");


            //String Configs
            ConfigSettings.doorOpenString = Plugin.instance.Config.Bind<string>("Door", "doorOpenString", "Opening door.", "Message returned on door (open) command.");
            ConfigSettings.doorCloseString = Plugin.instance.Config.Bind<string>("Door", "doorCloseString", "Closing door.", "Message returned on door (close) command.");
            ConfigSettings.doorSpaceString = Plugin.instance.Config.Bind<string>("Door", "doorSpaceString", "Can't open doors in space.", "Message returned on door (inSpace) command.");
            ConfigSettings.quitString = Plugin.instance.Config.Bind<string>("Quit", "quitString", "goodbye!", "Message returned on quit command.");
            ConfigSettings.leverString = Plugin.instance.Config.Bind<string>("Lever", "leverString", "PULLING THE LEVER!!!", "Message returned on lever pull command.");
            ConfigSettings.kickString = Plugin.instance.Config.Bind<string>("Server Admin", "kickString", "Kicking player now.", "Message returned on kick command.");
            ConfigSettings.kickNoString = Plugin.instance.Config.Bind<string>("Server Admin", "kickNoString", "Unable to kick, player not found.", "Message returned on kick command fail.");
            ConfigSettings.kickNotHostString = Plugin.instance.Config.Bind<string>("Server Admin", "kickNotHostString", "You do not have access to this command.", "Message returned on kick command and you're not host.");
            ConfigSettings.lolStartString = Plugin.instance.Config.Bind<string>("Lol", "lolStartString", "lol.", "Message returned when first running lol.");
            ConfigSettings.lolStopString = Plugin.instance.Config.Bind<string>("Lol", "lolStopString", "No more lol.", "Message returned if you want to end lol early.");
            ConfigSettings.tpMessageString = Plugin.instance.Config.Bind<string>("Teleporters", "tpMessageString", "Teleport Button pressed.", "Message returned when TP command is run.");
            ConfigSettings.itpMessageString = Plugin.instance.Config.Bind<string>("Teleporters", "itpMessageString", "Inverse Teleport Button pressed.", "Message returned when ITP command is run.");
            ConfigSettings.vitalsPoorString = Plugin.instance.Config.Bind<string>("Vitals", "vitalsPoorString", "You can't afford to run this command.", "Message returned when you don't have enough credits to run the <Vitals> command.");
            ConfigSettings.vitalsUpgradePoor = Plugin.instance.Config.Bind<string>("Vitals", "vitalsUpgradePoor", "You can't afford to upgrade the Vitals Scanner.", "Message returned when you don't have enough credits to unlock the vitals scanner upgrade.");
            ConfigSettings.healIsFullString = Plugin.instance.Config.Bind<string>("Heal", "healIsFullString", "You are full health!", "Message returned when heal command is run and player is already full health.");
            ConfigSettings.healString = Plugin.instance.Config.Bind<string>("Heal", "healString", "The terminal healed you?!?", "Message returned when heal command is run and player is healed.");
            ConfigSettings.camString = Plugin.instance.Config.Bind<string>("Cams", "camString", "(CAMS)", "Message returned when enabling Cams command (cams).");
            ConfigSettings.camString2 = Plugin.instance.Config.Bind<string>("Cams", "camString2", "Cameras disabled.", "Message returned when disabling Cams command (cams).");
            ConfigSettings.mapString = Plugin.instance.Config.Bind<string>("Cams", "mapString", "(MAP)", "Message returned when enabling map command (map).");
            ConfigSettings.mapString2 = Plugin.instance.Config.Bind<string>("Cams", "mapString2", "Map View disabled.", "Message returned when disabling map command (map).");
            ConfigSettings.ovString = Plugin.instance.Config.Bind<string>("Cams", "ovString", "(Overlay)", "Message returned when enabling Overlay command (overlay).");
            ConfigSettings.ovString2 = Plugin.instance.Config.Bind<string>("Cams", "ovString2", "Overlay disabled.", "Message returned when disabling Overlay command (overlay).");
            ConfigSettings.mmString = Plugin.instance.Config.Bind<string>("Cams", "mmString", "(MiniMap)", "Message returned when enabling minimap command (minimap).");
            ConfigSettings.mmString2 = Plugin.instance.Config.Bind<string>("Cams", "mmString2", "MiniMap disabled.", "Message returned when disabling minimap command (minimap).");
            ConfigSettings.mcString = Plugin.instance.Config.Bind<string>("Cams", "mcString", "(MiniCams)", "Message returned when enabling minicams command (minicams).");
            ConfigSettings.mcString2 = Plugin.instance.Config.Bind<string>("Cams", "mcString2", "MiniCams disabled.", "Message returned when disabling minicams command (minicams).");

            ConfigSettings.customLink = Plugin.instance.Config.Bind<string>("Link", "customLink", "https://thunderstore.io/c/lethal-company/p/darmuh/darmuhsTerminalStuff/", "URL to send players to when using the \"link\" command.");


            //Cost configs
            ConfigSettings.vitalsCost = Plugin.instance.Config.Bind<int>("Vitals", "vitalsCost", 10, "Credits cost to run Vitals Command each time it's run.");
            ConfigSettings.vitalsUpgradeCost = Plugin.instance.Config.Bind<int>("Vitals", "vitalsUpgradeCost", 200, "Credits cost to upgrade Vitals command to not cost credits anymore.");
            ConfigSettings.bioScanUpgradeCost = Plugin.instance.Config.Bind<int>("BioScan", "bioScanUpgradeCost", 300, "Credits cost to upgrade Bioscan command to provide detailed information on scanned enemies.");
            ConfigSettings.enemyScanCost = Plugin.instance.Config.Bind<int>("BioScan", "enemyScanCost", 15, "Credits cost to run Bioscan command each time it's run.");

            //Other configs
            ConfigSettings.gambleMinimum = Plugin.instance.Config.Bind<int>("Gamble", "gambleMinimum", 0, "Credits needed to start gambling, 0 means you can gamble everything.");
            ConfigSettings.gamblePityMode = Plugin.instance.Config.Bind<bool>("Gamble", "gamblePityMode", false, "Enable Gamble Pity Mode, which gives credits back to those who lose everything.");
            ConfigSettings.gamblePityCredits = Plugin.instance.Config.Bind<int>("Gamble", "gamblePityCredits", 10, "If Gamble Pity Mode is enabled, specify how much Pity Credits are given to losers. (Max: 60)");
            ConfigSettings.gamblePoorString = Plugin.instance.Config.Bind<string>("Gamble", "gamblePoorString", "You don't meet the minimum credits requirement to gamble.", "Message returned when your credits is less than the gambleMinimum set.");
            ConfigSettings.videoFolderPath = Plugin.instance.Config.Bind<string>("Lol", "videoFolderPath", "darmuh-darmuhsTerminalStuff", "Folder name where videos will be pulled from, needs to be in BepInEx/plugins");
            ConfigSettings.leverConfirmOverride = Plugin.instance.Config.Bind<bool>("Lever", "leverConfirmOverride", false, "Setting this to true will disable the confirmation check for the <lever> command.");
            ConfigSettings.camsNeverHide = Plugin.instance.Config.Bind<bool>("Cams", "camsNeverHide", false, "Setting this to true will make it so no command will ever auto-hide any cams command.");
            ConfigSettings.defaultCamsView = Plugin.instance.Config.Bind("Cams", "defaultCamsView", "map", new ConfigDescription("Set the default view switch commands will use when nothing is active.", new AcceptableValueList<string>("map", "cams", "minimap", "minicams", "overlay")));
            ConfigSettings.ovOpacity = Plugin.instance.Config.Bind("Cams", "ovOpacity", 10, new ConfigDescription("Opacity percentage for Overlay View.", new AcceptableValueRange<int>(0, 100)));


            //Keyword configs (terminalapi)
            ConfigSettings.alwaysOnKeyword = Plugin.instance.Config.Bind<string>("Custom Keywords", "alwaysOnKeyword", "alwayson", "Keyword used in terminal to return <alwayson> command.");
            ConfigSettings.camsKeyword2 = Plugin.instance.Config.Bind<string>("Custom Keywords", "camsKeyword2", "cameras", "Additional Keyword used in terminal to return <cams> command");
            ConfigSettings.mapKeyword2 = Plugin.instance.Config.Bind<string>("Custom Keywords", "mapKeyword2", "show map", "Additional Keyword used in terminal to return <map> command");
            ConfigSettings.minimapKeyword = Plugin.instance.Config.Bind<string>("Custom Keywords", "minimapKeyword", "minimap", "Keyword used in terminal to return <minimap> command.");
            ConfigSettings.minicamsKeyword = Plugin.instance.Config.Bind<string>("Custom Keywords", "minicamsKeyword", "minicams", "Keyword used in terminal to return <minicams> command");
            ConfigSettings.overlayKeyword = Plugin.instance.Config.Bind<string>("Custom Keywords", "overlayKeyword", "overlay", "Keyword used in terminal to return <overlay> command");
            ConfigSettings.doorKeyword = Plugin.instance.Config.Bind<string>("Custom Keywords", "doorKeyword", "door", "Keyword used in terminal to return <door> command");
            ConfigSettings.lightsKeyword = Plugin.instance.Config.Bind<string>("Custom Keywords", "lightsKeyword", "lights", "Keyword used in terminal to return <lights> command");
            ConfigSettings.modsKeyword2 = Plugin.instance.Config.Bind<string>("Custom Keywords", "modsKeyword2", "modlist", "Additional Keyword used in terminal to return <mods> command");
            ConfigSettings.tpKeyword2 = Plugin.instance.Config.Bind<string>("Custom Keywords", "tpKeyword2", "teleport", "Additional Keyword used in terminal to return <tp> command");
            ConfigSettings.itpKeyword2 = Plugin.instance.Config.Bind<string>("Custom Keywords", "itpKeyword2", "inverse", "Additional Keyword used in terminal to return <itp> command");
            ConfigSettings.quitKeyword2 = Plugin.instance.Config.Bind<string>("Custom Keywords", "quitKeyword2", "exit", "Additional Keyword used in terminal to return <quit> command");
            ConfigSettings.lolKeyword = Plugin.instance.Config.Bind<string>("Custom Keywords", "lolKeyword", "lol", "Keyword used in terminal to return <lol> command");
            ConfigSettings.clearKeyword2 = Plugin.instance.Config.Bind<string>("Custom Keywords", "clearKeyword2", "wipe", "Additional Keyword used in terminal to return <clear> command");
            ConfigSettings.dangerKeyword = Plugin.instance.Config.Bind<string>("Custom Keywords", "dangerKeyword", "danger", "Keyword used in terminal to return <danger> command");
            ConfigSettings.healKeyword2 = Plugin.instance.Config.Bind<string>("Custom Keywords", "healKeyword2", "healme", "Additional Keyword used in terminal to return <heal> command");
            ConfigSettings.lootKeyword2 = Plugin.instance.Config.Bind<string>("Custom Keywords", "lootKeyword2", "shiploot", "Additional Keyword used in terminal to return <loot> command");
            ConfigSettings.randomSuitKeyword = Plugin.instance.Config.Bind<string>("Custom Keywords", "randomSuitKeyword", "randomsuit", "Keyword used in terminal to return <randomsuit> command");


            //terminal patcher keywords
            ConfigSettings.fcolorKeyword = Plugin.instance.Config.Bind<string>("Custom Keywords", "fcolorKeyword", "fcolor", "Keyword used in terminal to return <fcolor> command");
            ConfigSettings.gambleKeyword = Plugin.instance.Config.Bind<string>("Custom Keywords", "gambleKeyword", "gamble", "Keyword used in terminal to return <gamble> command"); ;
            ConfigSettings.leverKeyword = Plugin.instance.Config.Bind<string>("Custom Keywords", "leverKeyword", "lever", "Keyword used in terminal to return <lever> command"); ;
            ConfigSettings.scolorKeyword = Plugin.instance.Config.Bind<string>("Custom Keywords", "scolorKeyword", "scolor", "Keyword used in terminal to return <scolor> command"); ;
            ConfigSettings.linkKeyword = Plugin.instance.Config.Bind<string>("Custom Keywords", "linkKeyword", "link", "Keyword used in terminal to return <link> command"); ;

            //homescreen lines
            ConfigSettings.homeLine1 = Plugin.instance.Config.Bind<string>("Home Page", "homeline1", "Welcome to the FORTUNE-9 OS PLUS", "First line of the home command (startup screen)");
            ConfigSettings.homeLine2 = Plugin.instance.Config.Bind<string>("Home Page", "homeline2", "\tUpgraded by Employee: darmuh", "Second line of the home command (startup screen)");
            ConfigSettings.homeLine3 = Plugin.instance.Config.Bind<string>("Home Page", "homeline3", "Have a wonderful [currentDay]!", "Last line of the home command (startup screen)");
        }
    }
}
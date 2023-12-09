using BepInEx.Configuration;
using FovAdjust;

namespace TerminalStuff
{
    public static class ConfigSettings
    {
        //establish commands that can be turned on or off here
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
        public static ConfigEntry<bool> terminalTP; //Teleporter command
        public static ConfigEntry<bool> terminalMods; //Modlist command
        public static ConfigEntry<bool> terminalKick; //Kick command (host only)

        //Cost configs
        public static ConfigEntry<int> vitalsCost; //Cost of Vitals Command




        public static void BindConfigSettings()
        {

            Plugin.Log.LogInfo("Binding configuration settings");
            ConfigSettings.terminalLobby = Plugin.instance.Config.Bind<bool>("Lobby", "terminalLobby", true, "Shows the current lobby name <Lobby Name>");
            ConfigSettings.terminalQuit = Plugin.instance.Config.Bind<bool>("Quit", "terminalQuit", true, "Command to quit terminal <Quit>");
            ConfigSettings.terminalClear = Plugin.instance.Config.Bind<bool>("Clear", "terminalClear", true, "Command to clear terminal text <Clear>");
            ConfigSettings.terminalLoot = Plugin.instance.Config.Bind<bool>("Loot", "terminalLoot", true, "Command to show total onboard loot value <Loot>");
            ConfigSettings.terminalCams = Plugin.instance.Config.Bind<bool>("Cams", "terminalCams", true, "Command to toggle displaying cameras in terminal <Cameras>");
            ConfigSettings.terminalLol = Plugin.instance.Config.Bind<bool>("Lol", "terminalLol", true, "Play a funny video <lol>");
            ConfigSettings.terminalHeal = Plugin.instance.Config.Bind<bool>("Heal", "terminalHeal", true, "Command to heal yourself <Heal>");
            ConfigSettings.terminalFov = Plugin.instance.Config.Bind<bool>("Fov", "terminalFov", true, "Command to change your FOV <Fov>");
            ConfigSettings.terminalGamble = Plugin.instance.Config.Bind<bool>("Gamble", "terminalGamble", true, "Command to gamble your credits, by percentage <Gamble>");
            ConfigSettings.terminalLever = Plugin.instance.Config.Bind<bool>("Lever", "terminalLever", true, "Pull the lever from terminal <Lever>");
            ConfigSettings.terminalDanger = Plugin.instance.Config.Bind<bool>("Danger", "terminalDanger", true, "Check moon danger level <Danger>");
            ConfigSettings.terminalVitals = Plugin.instance.Config.Bind<bool>("Vitals", "terminalVitals", true, "Scan player being tracked by monitor for their Health/Weight. <Vitals>");
            ConfigSettings.terminalTP = Plugin.instance.Config.Bind<bool>("Teleporters", "terminalTP", true, "Command to Activate Teleporter <TP>");
            ConfigSettings.terminalMods = Plugin.instance.Config.Bind<bool>("Mod List", "terminalMods", true, "Command to see your active mods <Mods>");
            ConfigSettings.terminalKick = Plugin.instance.Config.Bind<bool>("Server Admin", "terminalKick", false, "Enables kick command for host. <Kick>");

            //Cost configs
            ConfigSettings.vitalsCost = Plugin.instance.Config.Bind<int>("Vitals", "VitalsPrice", 10, "Credits cost to run Vitals Command, default is 10 credits");
        }
    }
}
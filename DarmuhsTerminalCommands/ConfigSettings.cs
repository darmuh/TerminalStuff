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




        public static void BindConfigSettings()
        {

            Plugin.Log.LogInfo("Binding configuration settings");
            ConfigSettings.terminalLobby = Plugin.instance.Config.Bind<bool>("Commands", "terminalLobby", true, "Shows the current lobby name <Lobby Name>");
            ConfigSettings.terminalQuit = Plugin.instance.Config.Bind<bool>("Commands", "terminalQuit", true, "Command to quit terminal <Quit>");
            ConfigSettings.terminalClear = Plugin.instance.Config.Bind<bool>("Commands", "terminalClear", true, "Command to clear terminal text <Clear>");
            ConfigSettings.terminalLoot = Plugin.instance.Config.Bind<bool>("Commands", "terminalLoot", true, "Command to show total onboard loot value <Loot>");
            ConfigSettings.terminalCams = Plugin.instance.Config.Bind<bool>("Commands", "terminalCams", true, "Command to toggle displaying cameras in terminal <Cameras>");
            ConfigSettings.terminalLol = Plugin.instance.Config.Bind<bool>("Commands", "terminalLol", false, "Funny video, (doesn't work atm) <lol>");
            ConfigSettings.terminalHeal = Plugin.instance.Config.Bind<bool>("Commands", "terminalHeal", true, "Command to heal yourself <Heal>");
            ConfigSettings.terminalFov = Plugin.instance.Config.Bind<bool>("Commands", "terminalFov", true, "Command to change your FOV <Fov>");
            ConfigSettings.terminalGamble = Plugin.instance.Config.Bind<bool>("Commands", "terminalGamble", true, "Command to gamble your credits, by percentage <Gamble>");
            ConfigSettings.terminalLever = Plugin.instance.Config.Bind<bool>("Commands", "terminalLever", true, "Pull the lever from terminal <Gamble>");
        }
    }
}
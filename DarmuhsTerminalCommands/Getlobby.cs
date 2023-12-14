using HarmonyLib;

namespace TerminalStuff
{
    public static class Getlobby
    {

        private static string _lastSteamLobbyName; // Variable to store the last steam lobby name after Postfix

        public static string LastSteamLobbyName
        {
            get { return _lastSteamLobbyName; }
            set { _lastSteamLobbyName = value; }
        }

        [HarmonyPatch(typeof(GameNetworkManager), "JoinLobby")]
        public static class JoinLobby_Patch
        {
            // This is a postfix method that gets executed after the original JoinLobby method
            [HarmonyPostfix]
            public static void Postfix(GameNetworkManager __instance)
            {
                // Access the private or protected steamLobbyName variable
                string steamLobbyName = (string)AccessTools.Field(typeof(GameNetworkManager), "steamLobbyName").GetValue(__instance);

                // Assign it to your own string
                LastSteamLobbyName = steamLobbyName;

                // Optionally, you can perform additional actions with the obtained value
                Plugin.Log.LogInfo($"Steam Lobby Name: {steamLobbyName}");
                if (ConfigSettings.terminalLobby.Value)
                {
                    Plugin.AddLobbyKeywords();
                }

                Plugin.Log.LogInfo("---------Lobby & Name Keywords updated!---------");
            }
        }

        [HarmonyPatch(typeof(HostSettings), MethodType.Constructor, typeof(string), typeof(bool))]
        public static class HostSettingsPatch
        {
            private static string _lastLobbyName; // Variable to store the last lobby name after Postfix

            public static string LastLobbyName
            {
                get
                {
                    return _lastLobbyName;
                    //Plugin.Log.LogInfo("get");
                }
                set
                {
                    _lastLobbyName = value;
                    //Plugin.Log.LogInfo("set");
                }
            }

            [HarmonyPostfix]
            public static void Postfix(HostSettings __instance)
            {
                LastLobbyName = __instance.lobbyName;
                Plugin.Log.LogInfo("---------Lobby & Name Keywords updated!---------");

                if (ConfigSettings.terminalLobby.Value)
                {
                    Plugin.AddLobbyKeywords();
                }
            }
        }
    }
}
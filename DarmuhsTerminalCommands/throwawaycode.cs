namespace TerminalStuff
{
    internal class throwawaycode
    {
        /*
         * 
         * 
         * 
         * 
         * 
         * 
         
        internal static void AttemptConfigSync(int fromClient)
        {
            Plugin.Spam("Attempting config sync!");
            List<ConfigEntryBase> configItems = OpenLib.ConfigManager.ConfigSync.PullMyConfig(Plugin.instance.Config);
            Plugin.Spam($"config count - {configItems.Count}");
            Dictionary<string, bool> configBools = OpenLib.ConfigManager.ConfigSync.ConfigBools(configItems);
            foreach(var unlockable in configBools)
            {
                Instance.AttemptConfigSyncServerRpc(fromClient, unlockable.Key, 0, unlockable.Value);
            }

            Dictionary<string, string> configStrings = OpenLib.ConfigManager.ConfigSync.ConfigStrings(configItems);
            foreach (var unlockable in configStrings)
            {
                Instance.AttemptConfigSyncServerRpc(fromClient, unlockable.Key, 0, false, unlockable.Value);
            }

            Dictionary<string, float> configFloats = OpenLib.ConfigManager.ConfigSync.ConfigFloats(configItems);
            foreach (var unlockable in configFloats)
            {
                Instance.AttemptConfigSyncServerRpc(fromClient, unlockable.Key, 0, false, "", -1, unlockable.Value);
            }

            Dictionary<string, int> configInts = OpenLib.ConfigManager.ConfigSync.ConfigInts(configItems);
            foreach (var unlockable in configInts)
            {
                Instance.AttemptConfigSyncServerRpc(fromClient, unlockable.Key, 0, false, "", unlockable.Value);
            }

        }

        [ServerRpc(RequireOwnership = true)]
        internal void AttemptConfigSyncServerRpc(int toClient, string unlockable, int type, bool value = false, string value2 = "", int value3 = -1, float value4 = -1)
        {
            Plugin.Spam($"attempting sync of {unlockable} to {toClient}");
            AttemptConfigSyncClientRpc(toClient, unlockable, type, value, value2, value3, value4);
        }

        [ClientRpc]
        internal void AttemptConfigSyncClientRpc(int toClient, string unlockable, int type, bool value = false, string value2 = "", int value3 = -1, float value4 = -1)
        {
            Plugin.Spam($"attempting sync of {unlockable} to {toClient}");
            if ((int)StartOfRound.Instance.localPlayerController.actualClientId != toClient)
                return;

            Plugin.Spam("We are the client specified!");

            if (type == 0)
                OpenLib.ConfigManager.ConfigSync.UpdateFromHost(unlockable, value, Plugin.instance.Config);
            if(type == 1)
                OpenLib.ConfigManager.ConfigSync.UpdateFromHost(unlockable, value2, Plugin.instance.Config);
            if(type == 2)
                OpenLib.ConfigManager.ConfigSync.UpdateFromHost(unlockable, value3, Plugin.instance.Config);
            if(type == 3)
                OpenLib.ConfigManager.ConfigSync.UpdateFromHost(unlockable, value4, Plugin.instance.Config);

            Plugin.instance.Config.Save();
        }

        [ServerRpc(RequireOwnership = false)]
        internal void AskHostSyncServerRpc(int fromClient)
        {
            Plugin.Spam("Asking host for config sync!");
            Plugin.Spam($"from - {fromClient}");
            AskHostSyncClientRpc(fromClient);
        }

        [ClientRpc]
        internal void AskHostSyncClientRpc(int fromClient)
        {
            if (GameNetworkManager.Instance == null)
                return;

            Plugin.Spam("checking if we are the host");

            if (GameNetworkManager.Instance.isHostingGame && ConfigSettings.SyncConfigs.Value)
                AttemptConfigSync(fromClient);
        }
         * 
         * 
         * 
         * 
         * 
         * 
         * 
         */
    }
}

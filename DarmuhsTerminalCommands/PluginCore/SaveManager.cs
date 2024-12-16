using System.Collections.Generic;
using TerminalStuff.SpecialStuff;
using static TerminalStuff.SpecialStuff.MoonsPlus;

namespace TerminalStuff.PluginCore
{
    internal class SaveManager
    {
        internal static List<string> AllUpgradesUnlocked = [];

        internal static void InitMoonPlusSave()
        {
            //havetraveledto
            if (!ConfigSettings.ModNetworking.Value)
                return;

            if (!ConfigSettings.TerminalMoonsPlus.Value)
                return;

            SetAllMoonsUnvisited();

            if (!GameNetworkManager.Instance.isHostingGame)
            {
                NetHandler.Instance.GetTravelHistoryServerRpc();
                return;
            }

            if (!ES3.KeyExists("darmuhsTerminalStuff_MoonsPlusHistory", GameNetworkManager.Instance.currentSaveFileName))
            {
                Plugin.Spam("Creating save key for darmuhsTerminalStuff_MoonsPlusHistory");
                MoonsVisited.Clear();
                MoonsVisited = GetTravelHistory();
                SaveTravelHistory(MoonsVisited);
                foreach (string name in MoonsVisited)
                    NetHandler.Instance.TravelHistoryServerRpc(name);
                //network to clients
            }
            else
            {
                MoonsVisited = ES3.Load<List<string>>("darmuhsTerminalStuff_MoonsPlusHistory", GameNetworkManager.Instance.currentSaveFileName);
                Plugin.Spam("Updating MoonsPlus Travel History from save key darmuhsTerminalStuff_MoonsPlusHistory");
                Plugin.Spam($"MoonsVisited count: {MoonsVisited.Count}");
                foreach (string name in MoonsVisited)
                    NetHandler.Instance.TravelHistoryServerRpc(name);
                //network to clients
            }
        }

        internal static void InitUnlocks()
        {
            CostCommands.enemyScanUpgradeEnabled = false;
            CostCommands.vitalsUpgradeEnabled = false;

            if (!ConfigSettings.ModNetworking.Value)
                return;

            if (!GameNetworkManager.Instance.isHostingGame)
            {
                NetHandler.Instance.AskUpgradeStatusServerRpc();
                return;
            }

            if (!ES3.KeyExists("darmuhsTerminalStuff_Upgrades", GameNetworkManager.Instance.currentSaveFileName))
            {
                Plugin.Spam("Creating save key for darmuhsTerminalStuff_Upgrades");
                AllUpgradesUnlocked = GetUnlockList();
                SaveUnlocks(AllUpgradesUnlocked);
                foreach(string name in AllUpgradesUnlocked)
                    NetHandler.Instance.UpgradeStatusServerRpc(name);
                //network to clients
            }
            else
            {
                AllUpgradesUnlocked = ES3.Load<List<string>>("darmuhsTerminalStuff_Upgrades", GameNetworkManager.Instance.currentSaveFileName);
                Plugin.Spam("Updating upgrades unlock status from save key darmuhsTerminalStuff_Upgrades");
                Plugin.Spam($"AllUpgrades count: {AllUpgradesUnlocked.Count}");
                foreach (string name in AllUpgradesUnlocked)
                    NetHandler.Instance.UpgradeStatusServerRpc(name);
                //network to clients
            }
        }

        internal static List<string> GetUnlockList()
        {
            List<string> upgradesUnlocked = [];
            if (!GameNetworkManager.Instance.isHostingGame)
                return upgradesUnlocked;

            if (CostCommands.CheckUnlockableStatus("BioscanPatch"))
            {
                CostCommands.enemyScanUpgradeEnabled = true;
                upgradesUnlocked.Add("BioscanPatch");
            }

            if (CostCommands.CheckUnlockableStatus("VitalsPatch"))
            {
                CostCommands.vitalsUpgradeEnabled = true;
                upgradesUnlocked.Add("VitalsPatch");
            }

            return upgradesUnlocked;
        }

        internal static void NewUnlock(string unlockName)
        {
            if (!AllUpgradesUnlocked.Contains(unlockName))
                AllUpgradesUnlocked.Add(unlockName);

            if(GameNetworkManager.Instance.isHostingGame)
                SaveUnlocks(AllUpgradesUnlocked);

            NetHandler.Instance.UpgradeStatusServerRpc(unlockName);
        }

        internal static void SaveUnlocks(List<string> unlockList)
        {
            if (!GameNetworkManager.Instance.isHostingGame)
                return;

            Plugin.Spam("saving darmuhsTerminalStuff_Upgrades");
            ES3.Save<List<string>>("darmuhsTerminalStuff_Upgrades", unlockList, GameNetworkManager.Instance.currentSaveFileName);
        }

        internal static void SaveTravelHistory(List<string> travelHistory)
        {
            if (!GameNetworkManager.Instance.isHostingGame)
                return;

            Plugin.Spam("saving darmuhsTerminalStuff_MoonsPlusHistory");
            ES3.Save<List<string>>("darmuhsTerminalStuff_MoonsPlusHistory", travelHistory, GameNetworkManager.Instance.currentSaveFileName);
        }

        internal static void AddToTravelHistory(MoonInfo moon)
        {
            if(!MoonsVisited.Contains(moon.LevelName))
                MoonsVisited.Add(moon.LevelName);

            if (GameNetworkManager.Instance.isHostingGame)
                SaveTravelHistory(MoonsVisited);

            NetHandler.Instance.TravelHistoryServerRpc(moon.LevelName);
        }
    }
}

using HarmonyLib;
using System.Collections.Generic;
using TerminalStuff.Configs;
using TerminalStuff.MoonsTweaks;
using TerminalStuff.Networking;
using static TerminalStuff.MoonsTweaks.MoonsPlus;

namespace TerminalStuff.Util;

internal class SaveManager
{
    internal static List<string> AllUpgradesUnlocked = [];

    internal static void InitMoonPlusSave()
    {
        //networking disabled
        if (!ConfigSettings.ModNetworking.Value)
            return;

        if (!Commands.TerminalMoonsPlus.Value)
            return;

        if (!GameNetworkManager.Instance.localPlayerController.IsHost)
        {
            NetHandler.Instance.AskHostTravelHistoryRpc();
            return;
        }

        //MoonsPlusHistory
        HistorySaveInit();
    }

    internal static void HistorySaveInit()
    {
        //networking disabled
        if (!ConfigSettings.ModNetworking.Value)
            return;

        if (!ES3.KeyExists("darmuhsTerminalStuff_MoonsPlusHistory", GameNetworkManager.Instance.currentSaveFileName))
        {
            Loggers.LogDebug("Creating save key for darmuhsTerminalStuff_MoonsPlusHistory");
            MoonsVisited.Clear();
            MoonsVisited = GetTravelHistory();
            SaveTravelHistory(MoonsVisited);
            foreach (string name in MoonsVisited)
                NetHandler.Instance.UpdateTravelHistoryRpc(name);
            //network to clients
        }
        else
        {
            MoonsVisited = ES3.Load<List<string>>("darmuhsTerminalStuff_MoonsPlusHistory", GameNetworkManager.Instance.currentSaveFileName);
            Loggers.LogDebug("Updating MoonsPlus Travel History from save key darmuhsTerminalStuff_MoonsPlusHistory");
            Loggers.LogDebug($"MoonsVisited count: {MoonsVisited.Count}");
            foreach (string name in MoonsVisited)
                NetHandler.Instance.UpdateTravelHistoryRpc(name);
            //network to clients
        }

        MoonListing.Do(x => x.OneTimePurchaseLoadIn());
    }

    internal static void ResetUnlocks()
    {
        CostCommands.enemyScanUpgradeEnabled = false;
        CostCommands.vitalsUpgradeEnabled = false;

        AllUpgradesUnlocked = [];
    }

    internal static void InitUnlocks()
    {
        //networking disabled
        if (!ConfigSettings.ModNetworking.Value)
            return;

        CostCommands.enemyScanUpgradeEnabled = false;
        CostCommands.vitalsUpgradeEnabled = false;

        if (!ConfigSettings.ModNetworking.Value)
            return;

        if (!GameNetworkManager.Instance.isHostingGame)
        {
            NetHandler.Instance.AskHostUpgradeStatusRpc();
            return;
        }

        if (!ES3.KeyExists("darmuhsTerminalStuff_Upgrades", GameNetworkManager.Instance.currentSaveFileName))
        {
            Loggers.LogDebug("Creating save key for darmuhsTerminalStuff_Upgrades");
            AllUpgradesUnlocked = GetUnlockList();
            SaveUnlocks(AllUpgradesUnlocked);
            foreach (string name in AllUpgradesUnlocked)
                NetHandler.Instance.UpgradeStatusRpc(name);
            //network to clients
        }
        else
        {
            AllUpgradesUnlocked = ES3.Load<List<string>>("darmuhsTerminalStuff_Upgrades", GameNetworkManager.Instance.currentSaveFileName);
            Loggers.LogDebug("Updating upgrades unlock status from save key darmuhsTerminalStuff_Upgrades");
            Loggers.LogDebug($"AllUpgrades count: {AllUpgradesUnlocked.Count}");
            foreach (string name in AllUpgradesUnlocked)
                NetHandler.Instance.UpgradeStatusRpc(name);
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
        //networking disabled
        if (!ConfigSettings.ModNetworking.Value)
            return;

        if (!AllUpgradesUnlocked.Contains(unlockName))
            AllUpgradesUnlocked.Add(unlockName);

        if (GameNetworkManager.Instance.isHostingGame)
            SaveUnlocks(AllUpgradesUnlocked);

        NetHandler.Instance.UpgradeStatusRpc(unlockName);
    }

    internal static void SaveUnlocks(List<string> unlockList)
    {
        if (!GameNetworkManager.Instance.isHostingGame)
            return;

        Loggers.LogDebug("saving darmuhsTerminalStuff_Upgrades");
        ES3.Save("darmuhsTerminalStuff_Upgrades", unlockList, GameNetworkManager.Instance.currentSaveFileName);
    }

    internal static void SaveTravelHistory(List<string> travelHistory)
    {
        if (!GameNetworkManager.Instance.isHostingGame)
            return;

        Loggers.LogDebug($"saving darmuhsTerminalStuff_MoonsPlusHistory from list ({travelHistory.Count}):");
        foreach (string t in travelHistory)
            Loggers.LogDebug(t);
        ES3.Save("darmuhsTerminalStuff_MoonsPlusHistory", travelHistory, GameNetworkManager.Instance.currentSaveFileName);
    }

    internal static void AddToTravelHistory(MoonInfo moon)
    {
        //networking disabled
        if (!ConfigSettings.ModNetworking.Value)
            return;

        if (GameNetworkManager.Instance.localPlayerController == null)
            return;

        if (!MoonsVisited.Contains(moon.LevelName))
            MoonsVisited.Add(moon.LevelName);

        if (GameNetworkManager.Instance.localPlayerController.IsHost)
            SaveTravelHistory(MoonsVisited);

        NetHandler.Instance.UpdateTravelHistoryRpc(moon.LevelName);
    }
}

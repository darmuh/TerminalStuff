using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TerminalStuff.Configs;
using TerminalStuff.EventSub;
using TerminalStuff.PluginCore;
using TerminalStuff.SpecialStuff.Store;
using UnityEngine;

namespace TerminalStuff;

internal class CostCommands
{
    internal static bool vitalsUpgradeEnabled = false;
    internal static bool enemyScanUpgradeEnabled = false;
    //List<int> items 
    internal static List<int> storeCart = [];
    //internal static string currentPackList;
    //internal static string currentPackName;
    //internal static string buyPackName;
    internal static Dictionary<Item, int> itemsIndexed = [];

    internal static bool CheckUnlockableStatus(string itemName)
    {
        foreach (UnlockableItem item in StartOfRound.Instance.unlockablesList.unlockables)
        {
            //Loggers.LogInfo($"Checking {itemName}");
            if (item.unlockableName == itemName)
            {
                if (item.alreadyUnlocked || item.hasBeenUnlockedByPlayer)
                {
                    Loggers.LogDebug($"Upgrade: {itemName} already unlocked. Setting variable to true");
                    return true;
                }
            }
        }

        Loggers.LogDebug($"Upgrade: {itemName} is NOT unlocked already");
        return false;
    }

    internal static bool CheckUnlockableStatus(int itemID)
    {
        if (itemID >= StartOfRound.Instance.unlockablesList.unlockables.Count)
            return false;

        UnlockableItem item = StartOfRound.Instance.unlockablesList.unlockables[itemID];
        if (!item.alreadyUnlocked && !item.hasBeenUnlockedByPlayer)
        {
            Loggers.LogDebug($"Upgrade ID: {itemID} has not been unlocked. Setting variable to true");
            return true;
        }

        return false;
    }

    internal static void UpdateUnlockStatus()
    {
        enemyScanUpgradeEnabled = false;
        vitalsUpgradeEnabled = false;

        foreach (string name in SaveManager.AllUpgradesUnlocked)
        {
            Loggers.LogDebug($"Updating upgrade status for {name}");

            if (name == "BioscanPatch")
                enemyScanUpgradeEnabled = true;
            else if (name == "VitalsPatch")
                vitalsUpgradeEnabled = true;
            else
                Loggers.WARNING($"Unexpected upgrade unlock name [ {name} ]");
        }

    }

    internal static string BioscanCommand()
    {
        string displayText;

        if (RoundManager.Instance != null)
        {
            int scannedEnemies = RoundManager.Instance.SpawnedEnemies.Count;
            int getCreds = Plugin.instance.Terminal.groupCredits;
            int costCreds = Commands.BioScanCost.Value;

            if (ShouldRunBioscan2(getCreds, costCreds)) //upgraded bioscan
            {
                int newCreds = CalculateNewCredits(getCreds, costCreds, Plugin.instance.Terminal);

                List<EnemyAI> livingEnemies = GetLivingEnemiesList();
                string filteredLivingEnemiesString = FilterLivingEnemies(livingEnemies);

                string bioscanResult = GetBioscanResult(scannedEnemies, costCreds, newCreds, filteredLivingEnemiesString);
                displayText = bioscanResult;
                Loggers.LogInfo($"Living Enemies(filtered): {filteredLivingEnemiesString}");
                return displayText;
            }
            else if (getCreds >= costCreds) //nonupgraded
            {
                int newCreds = CalculateNewCredits(getCreds, costCreds, Plugin.instance.Terminal);
                string bioscanResult = GetBasicBioscanResult(scannedEnemies, costCreds, newCreds);
                displayText = bioscanResult;
                Loggers.LogInfo("v1 scanner utilized, only numbers shown");
                return displayText;
            }
            else
            {
                displayText = "Not enough credits to run Biomatter Scanner.\r\n";
                Loggers.LogInfo("brokeboy detected");
                return displayText;
            }
        }
        else
        {
            displayText = "Cannot scan for Biomatter at this time.\r\n";
            return displayText;
        }
    }

    private static bool ShouldRunBioscan2(int getCreds, int costCreds)
    {
        return enemyScanUpgradeEnabled && getCreds >= costCreds;
    }

    private static string GetBasicBioscanResult(int scannedEnemies, int costCreds, int newCreds)
    {
        return $"Biomatter scanner charged {costCreds} credits and has detected [{scannedEnemies}] non-employee organic objects.\r\n\r\nYour new balance is ■{newCreds} Credits.\r\n";
    }

    private static List<EnemyAI> GetLivingEnemiesList()
    {
        return [.. RoundManager.Instance.SpawnedEnemies.Where(enemy => !enemy.isEnemyDead)];
    }

    private static string FilterLivingEnemies(List<EnemyAI> livingEnemies)
    {
        string livingEnemiesString = string.Join(Environment.NewLine, livingEnemies.Select(enemy => enemy.ToString()));
        string pattern = @"\([^)]*\)";
        return Regex.Replace(livingEnemiesString, pattern, string.Empty);
    }
    private static string GetBioscanResult(int scannedEnemies, int costCreds, int newCreds, string filteredLivingEnemiesString)
    {
        string bioscanResult = $"Biomatter scanner charged {costCreds} credits and has detected [{scannedEnemies}] non-employee organic objects.\r\n\r\n";

        if (!string.IsNullOrEmpty(filteredLivingEnemiesString))
        {
            bioscanResult += $"Your new balance is ■{newCreds} Credits.\r\n\r\nDetailed scan has defined these objects as the following in the registry: \r\n{filteredLivingEnemiesString}\r\n";
        }
        else
        {
            bioscanResult += $"Your new balance is ■{newCreds} Credits.\r\n";
            Loggers.LogInfo("v1 scanner utilized, only numbers shown");
        }

        return bioscanResult;
    }
    internal static string VitalsCommand()
    {
        string displayText;
        PlayerControllerB getPlayerInfo = GameStuff.TerminalMapRenderer.targetedPlayer;

        if (getPlayerInfo == null)
        {
            displayText = $"Vitals command malfunctioning...\n\n";
            return displayText;
        }

        int getCreds = Plugin.instance.Terminal.groupCredits;
        int costCreds = GetCostCreds(vitalsUpgradeEnabled);

        string playername = getPlayerInfo.playerUsername;

        Loggers.LogInfo("playername: " + playername);

        if (ShouldDisplayVitals(getPlayerInfo, getCreds, costCreds))
        {
            int newCreds = CalculateNewCredits(getCreds, costCreds, Plugin.instance.Terminal);

            string vitalsInfo = GetVitalsInfo(getPlayerInfo);
            string creditsInfo = GetCreditsInfo(newCreds);

            if (!vitalsUpgradeEnabled)
            {
                displayText = $"Charged ■{costCreds} Credits. \n{vitalsInfo}\n{creditsInfo}";
                return displayText;
            }
            else
            {
                displayText = $"{vitalsInfo}\n{creditsInfo}";
                return displayText;
            }
        }
        else
        {
            displayText = $"{ConfigSettings.VitalsPoorString.Value}\n";
            return displayText;
        }
    }
    internal static int GetCostCreds(bool upgradeStatus)
    {
        if (!upgradeStatus)
        {
            return Commands.VitalsCost.Value;
        }
        else
        {
            return 0;
        }
    }
    internal static string AskBioscanUpgrade()
    {
        if (enemyScanUpgradeEnabled == false)
        {
            string patchASK = $"Purchase the BioScanner 2.0 Upgrade Patch?\nThis software update is available for {Commands.BioScanUpgradeCost.Value} Credits.\n\n\n\n\n\n\n\n\n\n\nPlease CONFIRM or DENY.\n";
            return patchASK;
        }
        else
        {
            TerminalGeneral.CancelConfirmation = true;
            string displayText = $"BioScanner software has already been updated to the latest patch (2.0).\r\n\r\n";
            return displayText;
        }
    }
    internal static string AskVitalsUpgrade()
    {
        if (vitalsUpgradeEnabled == false)
        {
            string patchASK = $"Purchase the Vitals Scanner 2.0 Patch?\nThis software update is available for {Commands.VitalsUpgradeCost.Value} Credits.\n\n\n\n\n\n\n\n\n\n\nPlease CONFIRM or DENY.\n";
            return patchASK;
        }
        else
        {
            TerminalGeneral.CancelConfirmation = true;
            string displayText = $"Vitals Scanner software has already been updated to the latest patch (2.0).\r\n\r\n";
            return displayText;
        }
    }

    internal static string PerformBioscanUpgrade()
    {
        if (enemyScanUpgradeEnabled == false)
        {
            int newCreds = Plugin.instance.Terminal.groupCredits - Commands.BioScanUpgradeCost.Value;
            string displayText = $"Biomatter Scanner software has been updated to the latest patch (2.0) and now provides more detailed information!\r\n\r\nYour new balance is ■{newCreds} Credits\r\n";
            SaveManager.NewUnlock("BioscanPatch");
            Plugin.instance.Terminal.SyncGroupCreditsServerRpc(newCreds, Plugin.instance.Terminal.numberOfItemsInDropship);
            Plugin.instance.Terminal.PlayTerminalAudioServerRpc(0);
            return displayText;
        }
        else
        {
            string displayText = $"BioScanner software has already been updated to the latest patch (2.0).\r\n\r\n";
            return displayText;
        }
    }

    internal static string GetRefund()
    {
        string displayText;
        int deliverables = Plugin.instance.Terminal.numberOfItemsInDropship;
        Item[] buyables = Plugin.instance.Terminal.buyableItemsList;
        List<string> returnlist = [];
        int refund = 0;

        Loggers.LogInfo($"buyables: {buyables.Length}, deliverables: {deliverables}, storeCart: {storeCart.Count}");

        StoreRefundList.storeRefundItems = [];

        if (deliverables > 0)
        {
            foreach (int num in storeCart)
            {
                if (num <= buyables.Length)
                {
                    StoreRefundItem refundItem = StoreRefundList.storeRefundItems.FirstOrDefault(i => i.item == buyables[num]);
                    if (refundItem != null)
                    {
                        refundItem.count++;
                        Loggers.LogDebug($"Updating refundItem ({refundItem.item.itemName}) count to {refundItem.count}");
                    }

                    else
                    {
                        Loggers.LogDebug($"Creating refundItem for {buyables[num].itemName}!");
                        refundItem = new(buyables[num], 1);
                        refundItem.GetValue(buyables, num);
                        StoreRefundList.storeRefundItems.Add(refundItem);
                    }

                    refund += refundItem.value;
                    Loggers.LogDebug($"Adding {refundItem.item.itemName} ${refundItem.value} to refund list");
                }
                else
                {
                    Loggers.WARNING($"Unable to add item at index {num} to refund list! (out of index)");
                }

            }

            StoreRefundList.storeRefundItems.Do(x => returnlist.Add($"${x.value} {x.item.itemName} x {x.count}\n"));


            Loggers.LogDebug($"old creds: {Plugin.instance.Terminal.groupCredits}");
            int newCreds = Plugin.instance.Terminal.groupCredits + refund;
            Plugin.instance.Terminal.groupCredits = newCreds;
            Loggers.LogDebug($"new creds: {newCreds}");
            Plugin.instance.Terminal.orderedItemsFromTerminal.Clear();
            storeCart.Clear();
            NetHandler.Instance.SyncDropShipServerRpc(true);

            NetHandler.Instance.SyncCreditsServerRpc(newCreds, 0);

            string allitems = ListToStringBuild(returnlist);

            Loggers.LogInfo($"Refund total: ${refund}");
            displayText = $"Cancelling order for:\n{allitems}\nYou have been refunded ■{refund} Credits!\r\n";
            Plugin.instance.Terminal.PlayTerminalAudioServerRpc(0);
            return displayText;
        }
        else
            displayText = "No ordered items detected on the dropship.\n\n";

        return displayText;
    }

    private static string ListToStringBuild(List<string> list)
    {
        StringBuilder sb = new();

        for (int i = 0; i < list.Count; i++)
        {
            sb.Append(list[i]);
        }

        return sb.ToString();
    }

    internal static string PerformVitalsUpgrade()
    {
        if (vitalsUpgradeEnabled == false)
        {
            int newCreds = Plugin.instance.Terminal.groupCredits - Commands.VitalsUpgradeCost.Value;
            SaveManager.NewUnlock("VitalsPatch");
            string displayText = $"Vitals Scanner software has been updated to the latest patch (2.0) and no longer requires credits to scan.\r\n\r\nYour new balance is ■{newCreds} credits\r\n";
            Plugin.instance.Terminal.SyncGroupCreditsServerRpc(newCreds, Plugin.instance.Terminal.numberOfItemsInDropship);
            Plugin.instance.Terminal.PlayTerminalAudioServerRpc(0);
            return displayText;
        }
        else
        {
            string displayText = "Update already purchased.\n";
            return displayText;
        }
    }

    private static bool ShouldDisplayVitals(PlayerControllerB playerInfo, int getCreds, int costCreds)
    {
        return !playerInfo.isPlayerDead && (getCreds >= costCreds || vitalsUpgradeEnabled);
    }

    internal static int CalculateNewCredits(int getCreds, int costCreds, Terminal frompatch)
    {
        int newCreds = getCreds - costCreds;
        frompatch.groupCredits = newCreds;

        NetHandler.Instance.SyncCreditsServerRpc(newCreds, frompatch.numberOfItemsInDropship);
        return newCreds;
    }

    private static string GetVitalsInfo(PlayerControllerB playerInfo)
    {
        int playerHealth = playerInfo.health;
        float playerWeight = playerInfo.carryWeight;
        float playerSanity = playerInfo.insanityLevel;
        bool hasFlash = playerInfo.ItemSlots.Any(item => item is FlashlightItem);
        float realWeight = Mathf.RoundToInt(Mathf.Clamp(playerWeight - 1f, 0f, 100f) * 105f);

        string vitalsInfo = $"{playerInfo.playerUsername} Vitals:\n\n Health: {playerHealth}\n Weight: {realWeight}\n Sanity: {playerSanity}";

        if (hasFlash)
        {
            float flashCharge = Mathf.RoundToInt(playerInfo.pocketedFlashlight.insertedBattery.charge * 100);
            vitalsInfo += $"\n Flashlight Battery Percentage: {flashCharge}%";
        }

        return vitalsInfo;
    }

    private static string GetCreditsInfo(int newCreds)
    {
        return $"Your new balance is ■{newCreds} Credits.\r\n";
    }

}

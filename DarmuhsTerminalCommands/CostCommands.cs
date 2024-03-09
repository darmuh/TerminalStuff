using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TerminalStuff
{
    internal class CostCommands
    {
        public static bool vitalsUpgradeEnabled = false;
        public static bool enemyScanUpgradeEnabled = false;

        internal static void BioscanCommand(out string displayText)
        {
            
            //TerminalNode node = frompatch.currentNode;

            if (RoundManager.Instance != null)
            {
                int scannedEnemies = RoundManager.Instance.SpawnedEnemies.Count;
                int getCreds = Plugin.Terminal.groupCredits;
                int costCreds = ConfigSettings.enemyScanCost.Value;

                if (ShouldRunBioscan2(getCreds, costCreds)) //upgraded bioscan
                {
                    int newCreds = CalculateNewCredits(getCreds, costCreds, Plugin.Terminal);

                    List<EnemyAI> livingEnemies = GetLivingEnemiesList();
                    string filteredLivingEnemiesString = FilterLivingEnemies(livingEnemies);

                    string bioscanResult = GetBioscanResult(scannedEnemies, costCreds, newCreds, filteredLivingEnemiesString);
                    displayText = bioscanResult;
                    Plugin.MoreLogs($"Living Enemies(filtered): {filteredLivingEnemiesString}");
                    return;
                }
                else if (getCreds >= costCreds) //nonupgraded
                {
                    int newCreds = CalculateNewCredits(getCreds, costCreds, Plugin.Terminal);
                    string bioscanResult = GetBasicBioscanResult(scannedEnemies, costCreds, newCreds);
                    displayText = bioscanResult;
                    Plugin.MoreLogs("v1 scanner utilized, only numbers shown");
                    return;
                }
                else
                {
                    displayText = "Not enough credits to run Biomatter Scanner.\r\n";
                    Plugin.MoreLogs("brokeboy detected");
                    return;
                }
            }
            else
            {
                displayText = "Cannot scan for Biomatter at this time.\r\n";
                return;
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
            return RoundManager.Instance.SpawnedEnemies.Where(enemy => !enemy.isEnemyDead).ToList();
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
                Plugin.MoreLogs("v1 scanner utilized, only numbers shown");
            }

            return bioscanResult;
        }

        internal static void VitalsCommand(out string displayText)
        {
            
            //TerminalNode node = frompatch.currentNode;

            PlayerControllerB getPlayerInfo = StartOfRound.Instance.mapScreen.targetedPlayer;

            if (getPlayerInfo == null)
            {
                displayText = $"Vitals command malfunctioning...\n";
                return;
            }

            int getCreds = Plugin.Terminal.groupCredits;
            int costCreds = GetCostCreds(vitalsUpgradeEnabled);

            string playername = getPlayerInfo.playerUsername;

            Plugin.MoreLogs("playername: " + playername);

            if (ShouldDisplayVitals(getPlayerInfo, getCreds, costCreds))
            {
                int newCreds = CalculateNewCredits(getCreds, costCreds, Plugin.Terminal);

                string vitalsInfo = GetVitalsInfo(getPlayerInfo);
                string creditsInfo = GetCreditsInfo(newCreds);

                if (!vitalsUpgradeEnabled)
                {
                    displayText = $"Charged ■{costCreds} Credits. \n{vitalsInfo}\n{creditsInfo}";
                }
                else
                {
                    displayText = $"{vitalsInfo}\n{creditsInfo}";
                }
            }
            else
            {
                displayText = $"{ConfigSettings.vitalsPoorString.Value}\n";
            }
        }

        internal static int GetCostCreds(bool upgradeStatus)
        {
            if (!upgradeStatus)
            {
                return ConfigSettings.vitalsCost.Value;
            }
            else
            {
                return 0;
            }
        }

        internal static void PerformBioscanUpgrade(out string displayText)
        {
            
            //TerminalNode node = frompatch.currentNode;

            if (enemyScanUpgradeEnabled == false)
            {
                int getCreds = Plugin.Terminal.groupCredits;
                int costCreds = ConfigSettings.bioScanUpgradeCost.Value;
                if (getCreds >= costCreds)
                {
                    int newCreds = getCreds - costCreds;
                    Plugin.Terminal.groupCredits = newCreds;
                    NetHandler.Instance.SyncCreditsServerRpc(newCreds, Plugin.Terminal.numberOfItemsInDropship);
                    displayText = $"Biomatter Scanner software has been updated to the latest patch (2.0) and now provides more detailed information!\r\n\r\nYour new balance is ■{newCreds} Credits\r\n";
                    enemyScanUpgradeEnabled = true;
                }
                else
                {
                    displayText = $"You cannot afford this upgrade.\r\n";
                }

            }
            else
                displayText = $"Upgrade already purchased.\r\n";
        }

        internal static void GetRefund(out string displayText)
        {
            
            int deliverables = Plugin.Terminal.numberOfItemsInDropship;
            Item[] buyables = Plugin.Terminal.buyableItemsList;
            List<int> items = Plugin.Terminal.orderedItemsFromTerminal;
            List<string> returnlist = new List<string>();
            int refund = 0;

            Plugin.MoreLogs($"buyables: {buyables.Length}, deliverables: {deliverables}, items: {items.Count}");

            if (deliverables > 0)
            {
                foreach (int num in items)
                {
                    if (num <= buyables.Length)
                    {
                        refund += buyables[num].creditsWorth;
                        string itemname = buyables[num].itemName;
                        returnlist.Add(itemname + "\n");
                        Plugin.MoreLogs($"Adding {itemname} ${buyables[num].creditsWorth} to refund list");
                    }
                    else
                    {
                        string itemname = buyables[num].itemName;
                        Plugin.MoreLogs($"Unable to add {itemname} ${buyables[num].creditsWorth} to refund list");
                    }
                        
                }
                Plugin.MoreLogs($"old creds: {Plugin.Terminal.groupCredits}");
                int newCreds = Plugin.Terminal.groupCredits + refund;
                Plugin.Terminal.groupCredits = newCreds;
                Plugin.MoreLogs($"new creds: {newCreds}");
                Plugin.Terminal.orderedItemsFromTerminal.Clear();

                NetHandler.Instance.SyncCreditsServerRpc(newCreds, 0);

                string allitems = ListToStringBuild(returnlist);
                
                Plugin.MoreLogs($"Refund total: ${refund}");
                displayText = $"Cancelling order for: {allitems}\nYou have been refunded ■{refund} Credits!\r\n";
            }
            else
                displayText = "No ordered items detected on the dropship.\n\n";
        }

        private static string ListToStringBuild(List<string> list)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < list.Count; i++)
            {
                sb.Append(list[i]);
            }

            return sb.ToString();
        }

        internal static void PerformVitalsUpgrade(out string displayText)
        {
            
            //TerminalNode node = getTerm.currentNode;

            if (vitalsUpgradeEnabled == false)
            {
                int getCreds = Plugin.Terminal.groupCredits;
                int vitalsUpgradeCost = ConfigSettings.vitalsUpgradeCost.Value;
                if (getCreds >= vitalsUpgradeCost)
                {
                    int newCreds = getCreds - vitalsUpgradeCost;
                    Plugin.Terminal.groupCredits = newCreds;
                    NetHandler.Instance.SyncCreditsServerRpc(newCreds, Plugin.Terminal.numberOfItemsInDropship);
                    vitalsUpgradeEnabled = true;
                    displayText = $"Vitals Scanner software has been updated to the latest patch (2.0) and no longer requires credits to scan.\r\n\r\nYour new balance is ■{newCreds} credits\r\n";
                    return;
                }
                else
                {
                    displayText = $"{ConfigSettings.vitalsUpgradePoor.Value}\n";
                    return;
                }
            }
            else
                displayText = "Update already purchased.\n";
        }

        private static bool ShouldDisplayVitals(PlayerControllerB playerInfo, int getCreds, int costCreds)
        {
            return !playerInfo.isPlayerDead && (getCreds >= costCreds || vitalsUpgradeEnabled);
        }

        private static int CalculateNewCredits(int getCreds, int costCreds, Terminal frompatch)
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
}

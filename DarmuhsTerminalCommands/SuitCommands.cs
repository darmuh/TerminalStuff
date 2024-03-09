using GameNetcodeStuff;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TerminalStuff
{
    internal class SuitCommands
    {
        private static int GetMyPlayerID()
        {
            string myName = GameNetworkManager.Instance.localPlayerController.playerUsername;
            List<PlayerControllerB> allPlayers = StartOfRound.Instance.allPlayerScripts.ToList();
            allPlayers = allPlayers.OrderBy(player => player.playerClientId).ToList();

            for (int i = 0; i < allPlayers.Count; i++)
            {
                if (allPlayers[i].playerUsername == myName)
                {
                    Plugin.MoreLogs("Found my playerID");
                    return i;
                }
            }

            Plugin.MoreLogs("Failed to find ID");
            return -1;
        }

        internal static void GetRandomSuit(out string displayText)
        {
            List<UnlockableSuit> allSuits = new List<UnlockableSuit>();
            List<UnlockableItem> Unlockables = new List<UnlockableItem>();

            //get allSuits
            allSuits = Resources.FindObjectsOfTypeAll<UnlockableSuit>().ToList();
            displayText = string.Empty;

            if (allSuits.Count > 1)
            {
                // Order the list by syncedSuitID.Value
                allSuits = allSuits.OrderBy((UnlockableSuit suit) => suit.suitID).ToList();
                allSuits.RemoveAll(suit => suit.syncedSuitID.Value < 0); //simply remove bad suit IDs
                Unlockables = StartOfRound.Instance.unlockablesList.unlockables;
                int playerID = GetMyPlayerID();

                if (Unlockables != null)
                {
                    for (int i = 0; i < Unlockables.Count; i++)
                    {
                        // Get a random index
                        int randomIndex = UnityEngine.Random.Range(0, allSuits.Count);
                        string SuitName;

                        // Get the UnlockableSuit at the random index
                        UnlockableSuit randomSuit = allSuits[randomIndex];
                        if (randomSuit != null && Unlockables[randomSuit.syncedSuitID.Value] != null)
                        {
                            SuitName = Unlockables[randomSuit.syncedSuitID.Value].unlockableName;
                            UnlockableSuit.SwitchSuitForPlayer(StartOfRound.Instance.allPlayerScripts[playerID], randomSuit.syncedSuitID.Value, true);
                            randomSuit.SwitchSuitServerRpc(playerID);
                            randomSuit.SwitchSuitClientRpc(playerID);
                            displayText = $"Changing suit to {SuitName}!\r\n";
                            return;
                        }
                        else
                        {
                            displayText = "A suit could not be found.\r\n";
                            Plugin.Log.LogInfo($"Random suit ID was invalid or null");
                            return;
                        }
                    }
                }

                displayText = "A suit could not be found.\r\n";
                Plugin.Log.LogInfo($"Unlockables are null");
                return;
            }
            else
            {
                displayText = "Not enough suits detected.\r\n";
                Plugin.Log.LogInfo($"allsuits count too low");
                return;
            }
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace TerminalStuff;

internal class SuitCommands
{

    internal static void GetRandomSuit(out string displayText)
    {
        List<UnlockableSuit> allSuits = [];
        List<UnlockableItem> Unlockables = [];

        //get AllSuits
        allSuits = [.. Resources.FindObjectsOfTypeAll<UnlockableSuit>()];
        displayText = string.Empty;

        if (allSuits.Count > 1)
        {
            // Order the list by syncedSuitID.Value
            allSuits = [.. allSuits.OrderBy((UnlockableSuit suit) => suit.suitID)];
            allSuits.RemoveAll(suit => suit.syncedSuitID.Value < 0); //simply remove bad suit IDs
            Unlockables = StartOfRound.Instance.unlockablesList.unlockables;
            Random random = new();

            if (Unlockables != null)
            {
                for (int i = 0; i < Unlockables.Count; i++)
                {
                    // Get a random index
                    int randomIndex = random.Next(0, allSuits.Count);
                    string SuitName;

                    // Get the UnlockableSuit at the random index
                    UnlockableSuit randomSuit = allSuits[randomIndex];
                    if (randomSuit != null && Unlockables[randomSuit.syncedSuitID.Value] != null)
                    {
                        SuitName = Unlockables[randomSuit.syncedSuitID.Value].unlockableName;
                        randomSuit.SwitchSuitToThis();
                        displayText = $"Changing suit to {SuitName}!\r\n";
                        return;
                    }
                }

                displayText = "A suit could not be found.\r\n";
                Plugin.Log.LogInfo($"Random suit ID was invalid or null");
                return;
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

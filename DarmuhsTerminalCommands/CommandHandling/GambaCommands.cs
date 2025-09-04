using TerminalStuff.Configs;
using TerminalStuff.EventSub;
using TerminalStuff.PluginCore;
using UnityEngine;
using static TerminalStuff.DynamicCommands;
using static TerminalStuff.StringStuff;

namespace TerminalStuff;

internal class GambaCommands
{
    internal static bool validGambleValue = false;

    internal static string Ask2Gamble()
    {
        Loggers.LogInfo("Ask2Gamble");
        string val = GetAfterKeyword(GetKeywordsPerConfigItem(KeywordConfigs.GambleKeywords.Value));

        if (val.Length < 1)
        {
            TerminalGeneral.CancelConfirmation = true;
            validGambleValue = false;
            string displayText = "Unable to gamble at this time...\r\n\tInvalid input detected, no digits were provided!\r\n\r\n";
            Loggers.WARNING("not enough words for the gamble command!");
            return displayText;
        }

        if (int.TryParse(val, out int parsedValue))
        {
            if (parsedValue < 0 || parsedValue > 100)
                return BadInputGamble(val);
            newParsedValue = true;
            validGambleValue = true;
            Loggers.LogInfo("))))))))))))))))))Integer Established");
            ParsedValue = parsedValue;
            string displayText = $"Gamble {ParsedValue}% of your credits?\n\n\n\n\n\n\n\n\n\n\n\nPlease CONFIRM or DENY.\n";
            //Loggers.LogInfo(displayText);
            return displayText;
        }
        else
        {
            return BadInputGamble(val);
        }
    }

    internal static string BadInputGamble(string val)
    {
        TerminalGeneral.CancelConfirmation = true;
        validGambleValue = false;
        string displayText = $"Unable to gamble at this time...\r\n\tInvalid input detected!\r\n\tInput: {val}\r\n\r\n";
        Loggers.WARNING("there are no digits for the gamble command!");
        return displayText;
    }

    internal static string GambleConfirm()
    {
        if (validGambleValue)
        {
            Loggers.LogInfo("Valid gamble value detected, returning gamble command action");
            BasicGambleCommand(out string displayText);
            return displayText;
        }
        else
        {
            Loggers.LogInfo("attempting to confirm invalid gamble. Returning error");
            string displayText = Plugin.instance.Terminal.terminalNodes.specialNodes[5].displayText;
            return displayText;
        }
    }

    internal static string GambleDeny()
    {
        if (validGambleValue)
        {
            Loggers.LogInfo("Valid gamble value detected, but gamble has been canceled");
            string displayText = $"Gamble for {ParsedValue}% of your credits has been canceled.\r\n\r\n\r\n";
            return displayText;
        }
        else
        {
            Loggers.LogInfo("attempting to confirm invalid gamble. Returning error");
            string displayText = Plugin.instance.Terminal.terminalNodes.specialNodes[5].displayText;
            return displayText;
        }
    }

    internal static void BasicGambleCommand(out string displayText)
    {
        // Example: Get the percentage from the ParsedValue
        float percentage = ParsedValue;

        // Check if the percentage is within the valid range (0-100)
        if (!newParsedValue || percentage < 0 || percentage > 100)
        {
            // Handle the case when percentage is outside the valid range
            Loggers.LogInfo("Invalid percentage value. Telling user.");
            displayText = "Invalid gamble percentage, please input a value between 0 and 100.\n\n";
            return;
        }
        if (Plugin.instance.Terminal.groupCredits <= ConfigSettings.GambleMinimum.Value)
        {
            // Handle the case when groupCredits is lower than minimum required
            Loggers.LogInfo("Invalid percentage value. Telling user.");
            displayText = $"{ConfigSettings.GamblePoorString.Value}\n\n";
            return;
        }
        else
        {
            // Make the gamble and get the result
            var gambleResult = Gamble(Plugin.instance.Terminal.groupCredits, percentage);

            // Assign the result values to appropriate variables
            Plugin.instance.Terminal.groupCredits = gambleResult.newGroupCredits;
            NetHandler.Instance.SyncCreditsServerRpc(gambleResult.newGroupCredits, Plugin.instance.Terminal.numberOfItemsInDropship);
            newParsedValue = false;
            displayText = gambleResult.displayText;
            return;
        }

    }

    private static (int newGroupCredits, string displayText) Gamble(int currentGroupCredits, float percentage)
    {
        // Ensure the percentage is within a valid range (0-100)
        percentage = Mathf.Clamp(percentage, 0, 100);

        // Calculate the gamble amount as a percentage of the total credits
        int gambleAmount = (int)(currentGroupCredits * (percentage / 100.0f));

        // Generate two separate random floats
        float randomValue1 = UnityEngine.Random.value;
        float randomValue2 = UnityEngine.Random.value;

        // Determine the outcome based on a fair comparison of the two random values
        bool isWinner = randomValue1 < randomValue2;

        if (isWinner)
        {
            // Code for winning scenario
            string displayText = $"Congratulations! You won ■{gambleAmount} credits!\r\n\r\nYour new balance is ■{currentGroupCredits + gambleAmount} Credits.\r\n";
            return (currentGroupCredits + gambleAmount, displayText);
        }
        else
        {
            // Code for losing scenario
            int localResult = currentGroupCredits - gambleAmount;
            if (ConfigSettings.GamblePityMode.Value && localResult == 0) //checks for pity mode and 0 credits
            {
                if (ConfigSettings.GamblePityCredits.Value <= 60) //capping pity credits to 60 to avoid abuses of this system.
                {
                    string displayText = $"Sorry, you lost ■{gambleAmount} credits.\n\nHowever, you've received {ConfigSettings.GamblePityCredits.Value} Pity Credits.\r\n\r\nYour new balance is ■{ConfigSettings.GamblePityCredits.Value} Credits.\r\n";
                    return (ConfigSettings.GamblePityCredits.Value, displayText);
                }
                else
                {
                    string displayText = $"Sorry, you lost ■{gambleAmount} credits.\n\nUnfortunately we're also fresh out of Pity Credits due to malicious actors.\r\n\r\nYour new balance is ■{localResult} Credits.\r\n";
                    return (currentGroupCredits - gambleAmount, displayText);
                }

            }
            else
            {
                string displayText = $"Sorry, you lost ■{gambleAmount} credits.\r\n\r\nYour new balance is ■{localResult} Credits.\r\n";
                return (currentGroupCredits - gambleAmount, displayText);
            }
        }
    }
}

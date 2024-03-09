using System.Text.RegularExpressions;
using UnityEngine;
using static TerminalApi.TerminalApi;
using static TerminalStuff.DynamicCommands;

namespace TerminalStuff
{
    internal class GambaCommands
    {
        internal static TerminalNode AskToGamble(string digitsProvided, out TerminalNode outNode)
        {
            if (Regex.IsMatch(digitsProvided, "\\d+"))
            {
                newParsedValue = true;
                Plugin.MoreLogs("))))))))))))))))))Integer Established");
                int parsedValue = int.Parse(digitsProvided);
                ParsedValue = parsedValue;
            }
            else
            {
                Plugin.Log.LogWarning("there are no digits");
                outNode = null;
                return outNode;
            }
            TerminalNode gambleAsk = CreateTerminalNode($"Gamble {ParsedValue}% of your credits?\n\n\n\n\n\n\n\n\n\n\n\nPlease CONFIRM or DENY.\n", true);
            outNode = gambleAsk; //Ask user to confirm or deny

            // Awaiting Confirmation Logic
            Plugin.MoreLogs("waiting for confirm");
            return outNode;
        }

        internal static void BasicGambleCommand(out string displayText)
        {
            
            TerminalNode node = Plugin.Terminal.currentNode;

            node.clearPreviousText = true;
            // Example: Get the percentage from the ParsedValue
            float percentage = DynamicCommands.ParsedValue;

            // Check if the percentage is within the valid range (0-100)
            if (!DynamicCommands.newParsedValue || (percentage < 0 || percentage > 100))
            {
                // Handle the case when percentage is outside the valid range
                Plugin.MoreLogs("Invalid percentage value. Telling user.");
                displayText = "Invalid gamble percentage, please input a value between 0 and 100.\n\n";
            }
            if (Plugin.Terminal.groupCredits <= ConfigSettings.gambleMinimum.Value)
            {
                // Handle the case when groupCredits is lower than minimum required
                Plugin.MoreLogs("Invalid percentage value. Telling user.");
                displayText = $"{ConfigSettings.gamblePoorString.Value}\n\n";
            }
            else
            {
                // Make the gamble and get the result
                var gambleResult = Gamble(Plugin.Terminal.groupCredits, percentage);


                // Assign the result values to appropriate variables
                Plugin.Terminal.groupCredits = gambleResult.newGroupCredits;
                NetHandler.Instance.SyncCreditsServerRpc(gambleResult.newGroupCredits, Plugin.Terminal.numberOfItemsInDropship);
                DynamicCommands.newParsedValue = false;
                displayText = gambleResult.displayText;
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
                if (ConfigSettings.gamblePityMode.Value && localResult == 0) //checks for pity mode and 0 credits
                {
                    if (ConfigSettings.gamblePityCredits.Value <= 60) //capping pity credits to 60 to avoid abuses of this system.
                    {
                        string displayText = $"Sorry, you lost ■{gambleAmount} credits.\n\nHowever, you've received {ConfigSettings.gamblePityCredits.Value} Pity Credits.\r\n\r\nYour new balance is ■{ConfigSettings.gamblePityCredits.Value} Credits.\r\n";
                        return (ConfigSettings.gamblePityCredits.Value, displayText);
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
}

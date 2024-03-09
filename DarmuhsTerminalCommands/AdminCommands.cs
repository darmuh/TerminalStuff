using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using static TerminalApi.TerminalApi;
using Object = UnityEngine.Object;

namespace TerminalStuff
{
    internal class AdminCommands
    {
        //kick nodes
        static TerminalNode kickYes = CreateTerminalNode("Player has been kicked.\n", false);
        static TerminalNode kickNo = CreateTerminalNode($"{ConfigSettings.kickNoString.Value}\n", false);
        static TerminalNode notHost = CreateTerminalNode($"{ConfigSettings.kickNotHostString.Value}\n", false);

        public static int playerObjIdForTerminal; //needed for terminalEvent
        internal static TerminalNode KickPlayerCommand(string[] words, out TerminalNode outNode)
        {
            string targetPlayerName = words[1];
            outNode = null;
            

            if (GameNetworkManager.Instance.localPlayerController.isHostPlayerObject)
            {
                if (targetPlayerName.Length >= 3)
                {

                    // Find the matching player in allPlayerScripts
                    var matchingPlayer = StartOfRound.Instance.allPlayerScripts.FirstOrDefault(player =>
                        player.playerUsername.IndexOf(targetPlayerName, StringComparison.OrdinalIgnoreCase) != -1);

                    if (matchingPlayer != null && matchingPlayer.isHostPlayerObject == false)
                    {
                        int privatePlayerObjId = -1;
                        // Get the player's ID
                        for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Count(); i++)
                        {
                            if (StartOfRound.Instance.allPlayerScripts[i].playerUsername == matchingPlayer.playerUsername)
                            {
                                privatePlayerObjId = i;
                                break;
                            }
                        }

                        // Ensure playerID is valid
                        if (privatePlayerObjId != -1)
                        {
                            ulong getSteamID = matchingPlayer.playerSteamId;

                            playerObjIdForTerminal = privatePlayerObjId; //pass int for kick command in terminalEvent
                            Plugin.MoreLogs($"Kick command detected for player: {matchingPlayer.playerUsername}, Steam ID: {getSteamID}");
                            outNode = kickYes;
                            outNode.displayText = $"{ConfigSettings.kickString.Value}\n";
                            Plugin.Terminal.StartCoroutine(KickYes(Plugin.Terminal));
                            return outNode;
                        }
                        else
                        {
                            Plugin.MoreLogs($"Player {targetPlayerName} not found in the lobby. Object failed.");
                            outNode = kickNo;
                            return outNode;
                            // Invalid playerID or player not found
                        }
                    }
                    else //matchingplayer returning null
                    {
                        Plugin.MoreLogs($"Player {targetPlayerName} not found in the lobby. (null response)");
                        outNode = kickNo;
                        return outNode;
                    }
                }
                else //string (name) given not at least 3 characters
                {
                    Plugin.MoreLogs($"Input must be at least 3 characters long.");
                    outNode = kickNo;
                    return outNode;
                    // Handle case where the input is too short
                }
            }
            else // handles when the person entering the command is not the host
            {
                outNode = notHost;
                return outNode;
            }
        }

        internal static IEnumerator KickYes(Terminal getTerm)
        {

            Plugin.MoreLogs("We made it to the terminalEvent!!");
            int playernum = AdminCommands.playerObjIdForTerminal;
            Plugin.MoreLogs("playerObjIdForTerminal = " + playernum.ToString());

            // Delay for 1 second
            yield return new WaitForSeconds(1);
            Plugin.MoreLogs("Wait 1");
            getTerm.QuitTerminal();
            yield return new WaitForSeconds(1);
            Plugin.MoreLogs("Wait 2");
            StartOfRound.Instance.KickPlayer(AdminCommands.playerObjIdForTerminal);
            Plugin.MoreLogs("kicked");
        }
    }
}

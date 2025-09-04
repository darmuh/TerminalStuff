using GameNetcodeStuff;
using System.Collections;
using System.Text;
using TerminalStuff.Configs;
using TerminalStuff.EventSub;
using TerminalStuff.PluginCore;
using UnityEngine;
using static TerminalStuff.StringStuff;

namespace TerminalStuff;

internal class AdminCommands
{
    internal static PlayerControllerB playerToKick = null!;
    internal static bool kickEnum = false;

    internal static string KickPlayersAsk()
    {
        TerminalGeneral.CancelConfirmation = true;
        playerToKick = null!;
        string val = GetAfterKeyword(GetKeywordsPerConfigItem(KeywordConfigs.KickKeywords.Value));

        if (!AmIHost(out string displayText))
            return displayText;

        if (val.Length < 1)
        {
            string getPlayerNames = PlayerNameAndIDList();
            displayText = $"You must specify a player name or ID to kick them!\r\n\tKickable Players:\r\n(id) PlayerName{getPlayerNames}\r\n\r\n";
            return displayText;
        }

        if (ulong.TryParse(val, out ulong tryPlayerID))
        {
            foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
            {
                //Loggers.LogInfo($"Checking {player.playerUsername} - ID {player.playerClientId}");
                if (player.playerClientId == tryPlayerID && StartOfRound.Instance.localPlayerController != player && player.isPlayerControlled)
                {
                    TerminalGeneral.CancelConfirmation = false;
                    playerToKick = player;
                    displayText = $"Kick {player.playerUsername} from the lobby?\n\n\n\n\n\n\n\n\n\n\nPlease CONFIRM or DENY.\r\n\r\n";
                    Loggers.LogInfo("valid player to kick from id");
                    return displayText;
                }
            }
        }
        else
        {
            Loggers.LogDebug("ulong failed parse");
            string targetPlayerName = val.ToLower();
            foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
            {
                if (OpenLib.Common.Misc.CompareStringsInvariant(player.playerUsername, targetPlayerName))
                {
                    TerminalGeneral.CancelConfirmation = false;
                    playerToKick = player;
                    displayText = $"Kick {player.playerUsername} from the lobby?\n\n\n\n\n\n\n\n\n\n\nPlease CONFIRM or DENY.\r\n\r\n";
                    return displayText;
                }
            }
        }

        displayText = $"Unable to find player to kick by name or id - {val}\r\n\r\n";
        return displayText;
    }

    private static string PlayerNameAndIDList()
    {
        StringBuilder message = new();
        foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
        {
            if (StartOfRound.Instance.localPlayerController != player && player.isPlayerControlled)
            {
                message.Append($"\r\n({player.playerClientId}) {player.playerUsername}\r\n");
            }
        }

        return message.ToString();
    }

    internal static bool AmIHost(out string displayText)
    {
        displayText = "";
        if (GameNetworkManager.Instance.localPlayerController.IsHost)
            return true;
        else
        {
            displayText = $"You do not have permission to kick players from this lobby, you are NOT the host.\r\n\r\n";
            Plugin.Log.LogWarning("Somehow non-host player could try to kick others, error handled.");
            return false;
        }
    }

    internal static string KickPlayerConfirm()
    {
        int playerID = GetPlayerToKickID(playerToKick);
        Plugin.instance.Terminal.StartCoroutine(KickYes(playerID));
        string displayText = $"Kick Player Action Confirmed.\r\n\r\n\tKicking player: {playerToKick.playerUsername}\r\n\r\n";
        return displayText;
    }

    internal static string KickPlayerDeny()
    {
        string displayText = $"Cancelling kick player action for player:{playerToKick.playerUsername}\r\n\r\n";
        playerToKick = null!;
        return displayText;
    }

    internal static int GetPlayerToKickID(PlayerControllerB matchingPlayer)
    {
        int playerID = -1;
        for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++)
        {
            if (StartOfRound.Instance.allPlayerScripts[i].playerUsername == matchingPlayer.playerUsername)
            {
                playerID = i;
                break;
            }
        }

        return playerID;
    }

    internal static IEnumerator KickYes(int playerNum)
    {
        if (kickEnum)
            yield break;

        kickEnum = true;

        Loggers.LogDebug("We made it to the kick event!!");
        Loggers.LogDebug("playerObjIdForTerminal = " + playerNum.ToString());

        Plugin.instance.Terminal.QuitTerminal();
        yield return new WaitForSeconds(0.1f);
        StartOfRound.Instance.KickPlayer(playerNum);
        Loggers.LogDebug("kicked");

        kickEnum = false;
    }
}

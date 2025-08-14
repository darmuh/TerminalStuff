using GameNetcodeStuff;

namespace TerminalStuff
{
    internal class Misc
    {
        internal static bool IsLocalPlayerNull()
        {
            if (StartOfRound.Instance == null)
                return true;
            if (StartOfRound.Instance.localPlayerController == null)
                return true;

            return false;
        }

        internal static PlayerControllerB GetPlayerFromName(string playerName)
        {
            foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
            {
                if (player.playerUsername.ToLower() == playerName.ToLower())
                {
                    return player;
                }
            }

            return null;
        }

        internal static PlayerControllerB GetPlayerUsingTerminal()
        {
            foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
            {
                if (!player.isPlayerDead && player.currentTriggerInAnimationWith == Plugin.instance.Terminal.terminalTrigger)
                {
                    Plugin.MoreLogs($"Player: {player.playerUsername} detected using terminal.");
                    return player;
                }
            }
            return null;
        }

        internal static int HostClientID()
        {
            foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
            {
                if (player.isHostPlayerObject)
                {
                    Plugin.MoreLogs($"Player: {player.playerUsername} is the host, client ID: {player.playerClientId}.");
                    return ((int)player.playerClientId);
                }
            }

            return -1;
        }

    }
}

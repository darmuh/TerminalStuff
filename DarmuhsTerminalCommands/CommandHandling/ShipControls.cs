using GameNetcodeStuff;
using System.Collections;
using TerminalStuff.Configs;
using TerminalStuff.EventSub;
using TerminalStuff.Util;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using static OpenLib.Common.Teleporter;
using static TerminalStuff.Util.Misc;
using static TerminalStuff.Util.StringStuff;
using static TerminalStuff.TerminalEvents;
using static UnityEngine.Object;
using TerminalStuff.Networking;
using TerminalStuff.Compatibility;

namespace TerminalStuff.CommandHandling;

internal class ShipControls
{

    internal static bool leverEnum = false;
    internal static bool DoorSpaceCheck()
    {
        if (ConfigSettings.CanOpenDoorInSpace.Value)
        {
            return true;
        }
        else if (StartOfRound.Instance.shipDoorsEnabled)
            return true;
        else
            return false;
    }

    internal static string BasicDoorCommand()
    {
        string displayText = string.Empty;

        if (DoorSpaceCheck())
        {

            // Determine the button name based on the hangar doors state
            string buttonName = StartOfRound.Instance.hangarDoorsClosed ? "StartButton" : "StopButton";

            // Find the corresponding button GameObject
            GameObject buttonObject = GameObject.Find(buttonName);

            // Get the InteractTrigger component from the button
            InteractTrigger interactTrigger = buttonObject.GetComponentInChildren<InteractTrigger>();

            // Determine the action based on the hangar doors state
            string action = StartOfRound.Instance.hangarDoorsClosed ? "opened" : "closed";

            // Log the door state
            Loggers.LogInfo($"Hangar doors are {action}.");

            // Invoke the onInteract event if the button and event are found
            if (interactTrigger != null)
            {
                if (interactTrigger.onInteract is UnityEvent<PlayerControllerB> onInteractEvent)
                {
                    onInteractEvent.Invoke(GameNetworkManager.Instance.localPlayerController);

                    // Log individual messages for open and close events
                    if (action == "opened")
                    {
                        displayText = $"{ConfigSettings.DoorOpenString.Value}\n";
                        Loggers.LogInfo($"Hangar doors {action} successfully by interacting with button {buttonName}.");
                        return displayText;
                    }
                    else if (action == "closed")
                    {
                        displayText = $"{ConfigSettings.DoorCloseString.Value}\n";
                        Loggers.LogInfo($"Hangar doors {action} successfully by interacting with button {buttonName}.");
                        return displayText;
                    }
                }
                else
                {
                    // Log if onInteractEvent is null
                    Plugin.Log.LogWarning($"Warning: onInteract event is null for button {buttonName}.");
                    displayText = $"Unable to close doors, button could not be found!";
                    return displayText;
                }
            }
            else
            {
                // Log if interactTrigger is null
                Plugin.Log.LogWarning($"Warning: InteractTrigger not found on button {buttonName}.");
                displayText = $"Unable to close doors, button could not be found!";
                return displayText;
            }
        }
        else
        {
            displayText = $"{ConfigSettings.DoorSpaceString.Value}\n";
            return displayText;
        }

        return displayText;
    }

    internal static string BasicLightsCommand()
    {
        string displayText;

        StartOfRound.Instance.shipRoomLights.ToggleShipLights();
        if (StartOfRound.Instance.shipRoomLights.areLightsOn)
            displayText = $"Ship Lights are [ON]\n\n";
        else
            displayText = $"Ship Lights are [OFF]\n\n";
        return displayText;
    }

    internal static string RegularTeleporterCommand()
    {

        string val = GetAfterKeyword(GetKeywordsPerConfigItem(KeywordConfigs.TpKeywords.Value));
        string displayText;
        ShipTeleporter tp = NormalTP;
        if (tp != null)
        {
            float cooldownTime = tp.cooldownTime;
            if (Mathf.Round(cooldownTime) == 0 && tp.buttonTrigger.interactable)
            {
                if (val.Length > 1)
                {
                    Loggers.LogInfo("attempting to tp specific player");
                    string playerName = PlayerNameToTargetString(val, GameStuff.TerminalMapRenderer.radarTargets);
                    PlayerControllerB player = GetPlayerFromName(playerName);
                    if (player != null && player.isPlayerControlled)
                    {
                        tp.StartCoroutine(TeleportSpecificPlayer(player, tp));
                        displayText = $"{ConfigSettings.TpMessageString.Value}\n\tPlayer: {playerName}\n\n";
                        return displayText;
                    }
                    else
                    {
                        BaseUseNormalTP(out displayText, tp);
                        return displayText;
                    }
                }
                else
                {
                    BaseUseNormalTP(out displayText, tp);
                    return displayText;
                }


            }
            else displayText = $"Teleporter has {Mathf.Round(cooldownTime)} seconds remaining on cooldown.\n";
            return displayText;
        }
        else displayText = "Can't teleport at this time.\n Do you even have a teleporter?\n";
        return displayText;
    }

    private static IEnumerator TeleportSpecificPlayer(PlayerControllerB player, ShipTeleporter tp)
    {
        if (player == null)
            yield break;

        int current = GameStuff.TerminalMapRenderer.targetTransformIndex;
        int playerIndex = GameStuff.TerminalMapRenderer.radarTargets.FindIndex(t => t.name == player.playerUsername);

        GameStuff.TerminalMapRenderer.SwitchRadarTargetAndSync(playerIndex);
        Loggers.LogDebug($"Set radartarget to {player.playerUsername} @ [ {playerIndex} ]");
        yield return new WaitForSeconds(0.1f);
        tp.PressTeleportButtonOnLocalClient();
        Loggers.LogDebug($"teleporting {player.playerUsername}");
        yield return new WaitForSeconds(0.1f);
        GameStuff.TerminalMapRenderer.SwitchRadarTargetAndSync(current);
        Loggers.LogDebug($"Set radartarget back to {current}");

    }

    private static string BaseUseNormalTP(out string displayText, ShipTeleporter tp)
    {
        if (Plugin.instance.TwoRadarMapsMod && ViewCommands.AnyActiveMonitoring())
        {
            Loggers.LogInfo("using TP on target from Terminal Radar");
            displayText = TwoRadarMapsCompatibility.TeleportCompatibility();
            return displayText;
        }
        else
        {
            tp.PressTeleportButtonOnLocalClient();
            displayText = $"{ConfigSettings.TpMessageString.Value} (Targeted Player: {GameStuff.TerminalMapRenderer.radarTargets[GameStuff.TerminalMapRenderer.targetTransformIndex].name})\n";
            return displayText;
        }
    }

    internal static string InverseTeleporterCommand()
    {
        string displayText;
        ShipTeleporter tp = InverseTP;
        if (tp != null)
        {
            float cooldownTime = tp.cooldownTime;
            if (!StartOfRound.Instance.inShipPhase && tp.buttonTrigger.interactable)
            {
                tp.PressTeleportButtonOnLocalClient();
                displayText = $"{ConfigSettings.ItpMessageString.Value}\n";
                return displayText;
            }
            else if (Mathf.Round(cooldownTime) > 0)
            {
                displayText = $"Inverse Teleporter has {Mathf.Round(cooldownTime)} seconds remaining on cooldown.\n";
                return displayText;
            }
            else
            {
                displayText = $"Can't Inverse Teleport from space...\n"; //test
                return displayText;
            }


        }
        else displayText = "Can't Inverse Teleport at this time.\n Do you even have an Inverse Teleporter?\n";
        return displayText;
    }

    internal static string LeverControlCommand()
    {
        StartMatchLever leverInstance = FindObjectOfType<StartMatchLever>();
        NetworkManager networkManager = Plugin.instance.Terminal.NetworkManager;
        string getLevelName = StartOfRound.Instance.currentLevel.PlanetName;

        if (CanPullLever(networkManager))
        {
            string displayText = $"{ConfigSettings.LeverString.Value}\n";
            leverInstance.StartCoroutine(LeverPull(leverInstance));
            Loggers.LogInfo("lever pulled");
            return displayText;
        }
        else if (StartOfRound.Instance.travellingToNewLevel)
        {
            string displayText = $"We have not yet arrived to {getLevelName}, please wait.\n";
            return displayText;
        }
        else
        {
            string displayText = "Cannot pull the lever at this time.\n\n\tNOTE: If the game has not been started, only the host can do this.\n\n";
            return displayText;
        }
    }

    internal static string AskLever()
    {
        if (Commands.LeverConfirmOverride.Value)
            return LeverControlCommand();

        string getLevelName = StartOfRound.Instance.currentLevel.PlanetName;
        if (StartOfRound.Instance.inShipPhase)
        {
            string displayText = $"Pull the Lever and land on {getLevelName}?\n\n\n\n\n\n\n\n\n\n\n\nPlease CONFIRM or DENY.\n";
            return displayText;
        }
        else
        {
            string displayText = $"Pull the Lever and leave {getLevelName}?\n\n\n\n\n\n\n\n\n\n\n\nPlease CONFIRM or DENY.\n";
            return displayText;
        }
    }

    internal static string DenyLever()
    {
        string displayText = "Lever pull canceled...\n\n\n";
        return displayText;
    }

    private static bool CanPullLever(NetworkManager networkManager)
    {
        if (!GameNetworkManager.Instance.gameHasStarted &&
               !StartOfRound.Instance.travellingToNewLevel &&
               networkManager is not null &&
               networkManager.IsHost)
            return true;

        if (GameNetworkManager.Instance.gameHasStarted && !StartOfRound.Instance.travellingToNewLevel)
            return true;

        return false;
    }

    static IEnumerator LeverPull(StartMatchLever leverInstance)
    {
        if (leverEnum)
            yield break;

        leverEnum = true;

        if (leverInstance != null)
        {
            yield return new WaitForSeconds(0.3f);
            leverInstance.LeverAnimation();
            yield return new WaitForSeconds(0.3f);
            leverInstance.PullLever();
        }
        else
        {
            Plugin.Log.LogError("StartMatchLever instance not found!");
        }

        leverEnum = false;
    }

    internal static string RestartAsk()
    {
        if (Commands.RestartConfirmOverride.Value)
            return RestartAction();

        string displayText = "Restart Lobby?\n\n\n\n\n\n\n\n\n\n\n\nPlease CONFIRM or DENY.\n";
        return displayText;
    }

    internal static string RestartDeny()
    {
        string displayText = $"Restart lobby cancelled...\n\n";
        return displayText;
    }

    internal static string RestartAction()
    {
        if (!StartOfRound.Instance.inShipPhase || Bools.GameIsLocalPlayerNull())
        {
            string displayText = "This can only be done in orbit...\n\n";
            return displayText;
        }
        else if (!GameNetworkManager.Instance.localPlayerController.IsHost)
        {
            string displayText = "Only the host can do this...\n";
            return displayText;
        }
        else
        {
            string displayText = "Restart lobby confirmed, getting new ship...\n\n";
            NetHandler.Instance.QuickRestartRpc();
            Loggers.LogInfo("restarting lobby");
            return displayText;
        }

    }
}

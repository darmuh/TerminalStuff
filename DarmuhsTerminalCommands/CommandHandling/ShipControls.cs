using GameNetcodeStuff;
using System.Collections;
using TerminalStuff.EventSub;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using static OpenLib.Common.Teleporter;
using static TerminalStuff.Misc;
using static TerminalStuff.StringStuff;
using static TerminalStuff.TerminalEvents;
using static UnityEngine.Object;

namespace TerminalStuff
{
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

            //TerminalNode node = getTerm.currentNode;
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
                Plugin.MoreLogs($"Hangar doors are {action}.");

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
                            Plugin.MoreLogs($"Hangar doors {action} successfully by interacting with button {buttonName}.");
                            return displayText;
                        }
                        else if (action == "closed")
                        {
                            displayText = $"{ConfigSettings.DoorCloseString.Value}\n";
                            Plugin.MoreLogs($"Hangar doors {action} successfully by interacting with button {buttonName}.");
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
                displayText = $"Ship Lights are [ON]\r\n\r\n";
            else
                displayText = $"Ship Lights are [OFF]\r\n\r\n";
            return displayText;
        }

        internal static string RegularTeleporterCommand()
        {
            if (GameStuff.TerminalMapRenderer == null)
                GameStuff.GetMapRenderer();

            string val = GetAfterKeyword(GetKeywordsPerConfigItem(ConfigSettings.TpKeywords.Value));
            string displayText;
            ShipTeleporter tp = NormalTP;
            if (tp != null)
            {
                float cooldownTime = tp.cooldownTime;
                if (Mathf.Round(cooldownTime) == 0 && tp.buttonTrigger.interactable)
                {
                    if (val.Length > 1)
                    {
                        Plugin.MoreLogs("attempting to tp specific player");
                        string playerName = QueryToPlayerName(val, GameStuff.TerminalMapRenderer.radarTargets);
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
                else displayText = $"Teleporter has {Mathf.Round(cooldownTime)} seconds remaining on cooldown.\r\n";
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
            Plugin.Spam($"Set radartarget to {player.playerUsername} @ [ {playerIndex} ]");
            yield return new WaitForSeconds(0.1f);
            tp.PressTeleportButtonOnLocalClient();
            Plugin.Spam($"teleporting {player.playerUsername}");
            yield return new WaitForSeconds(0.1f);
            GameStuff.TerminalMapRenderer.SwitchRadarTargetAndSync(current);
            Plugin.Spam($"Set radartarget back to {current}");

        }

        private static string BaseUseNormalTP(out string displayText, ShipTeleporter tp)
        {
            if (Plugin.instance.TwoRadarMapsMod && ViewCommands.AnyActiveMonitoring())
            {
                Plugin.MoreLogs("using TP on target from Terminal Radar");
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
                if (!(StartOfRound.Instance.inShipPhase) && tp.buttonTrigger.interactable)
                {
                    tp.PressTeleportButtonOnLocalClient();
                    displayText = $"{ConfigSettings.ItpMessageString.Value}\n";
                    return displayText;
                }
                else if (Mathf.Round(cooldownTime) > 0)
                {
                    displayText = $"Inverse Teleporter has {Mathf.Round(cooldownTime)} seconds remaining on cooldown.\r\n";
                    return displayText;
                }
                else
                {
                    displayText = $"Can't Inverse Teleport from space...\r\n"; //test
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
                Plugin.MoreLogs("lever pulled");
                return displayText;
            }
            else if (StartOfRound.Instance.travellingToNewLevel)
            {
                string displayText = $"We have not yet arrived to {getLevelName}, please wait.\r\n";
                return displayText;
            }
            else
            {
                string displayText = "Cannot pull the lever at this time.\r\n\r\n\tNOTE: If the game has not been started, only the host can do this.\r\n\r\n";
                return displayText;
            }
        }

        internal static string AskLever()
        {
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
            string displayText = "Lever pull canceled...\r\n\r\n\r\n";
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
    }
}

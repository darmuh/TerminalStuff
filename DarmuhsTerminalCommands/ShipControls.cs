using GameNetcodeStuff;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.Object;
using Object = UnityEngine.Object;

namespace TerminalStuff
{
    internal class ShipControls
    {
        internal static List<TerminalNode> shipControlNodes = new List<TerminalNode>();

        internal static void BasicDoorCommand(out string displayText)
        {
            
            //TerminalNode node = getTerm.currentNode;
            displayText = string.Empty;

            if (StartOfRound.Instance.shipDoorsEnabled)
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
                    UnityEvent<PlayerControllerB> onInteractEvent = interactTrigger.onInteract as UnityEvent<PlayerControllerB>;

                    if (onInteractEvent != null)
                    {
                        onInteractEvent.Invoke(GameNetworkManager.Instance.localPlayerController);

                        // Log individual messages for open and close events
                        if (action == "opened")
                        {
                            displayText = $"{ConfigSettings.doorOpenString.Value}\n";
                            Plugin.MoreLogs($"Hangar doors {action} successfully by interacting with button {buttonName}.");
                        }
                        else if (action == "closed")
                        {
                            displayText = $"{ConfigSettings.doorCloseString.Value}\n";
                            Plugin.MoreLogs($"Hangar doors {action} successfully by interacting with button {buttonName}.");
                        }
                    }
                    else
                    {
                        // Log if onInteractEvent is null
                        Plugin.Log.LogWarning($"Warning: onInteract event is null for button {buttonName}.");
                    }
                }
                else
                {
                    // Log if interactTrigger is null
                    Plugin.Log.LogWarning($"Warning: InteractTrigger not found on button {buttonName}.");
                }
            }
            else
            {
                displayText = $"{ConfigSettings.doorSpaceString.Value}\n";
            }
        }

        internal static void BasicLightsCommand(out string displayText)
        {
            
            //TerminalNode node = getTerm.currentNode;

            StartOfRound.Instance.shipRoomLights.ToggleShipLights();
            if (StartOfRound.Instance.shipRoomLights.areLightsOn)
                displayText = $"Ship Lights are [ON]\r\n\r\n";
            else
                displayText = $"Ship Lights are [OFF]\r\n\r\n";
        }

        internal static void RegularTeleporterCommand(out string displayText)
        {
            
            //TerminalNode node = getTerm.currentNode;

            ShipTeleporter[] objectsOfType = Object.FindObjectsOfType<ShipTeleporter>();
            ShipTeleporter tp = (ShipTeleporter)null;
            foreach (ShipTeleporter normaltp in objectsOfType)
            {
                if (!normaltp.isInverseTeleporter)
                {
                    tp = normaltp;
                    break;
                }
            }
            if ((Object)tp != (Object)null)
            {
                float cooldownTime = tp.cooldownTime;
                if (Mathf.Round(cooldownTime) == 0 && tp.buttonTrigger.interactable)
                {
                    if(Plugin.instance.TwoRadarMapsMod && ViewCommands.AnyActiveMonitoring())
                    {
                        Plugin.MoreLogs("using TP on target from Terminal Radar");
                        displayText = TwoRadarMapsCompatibility.TeleportCompatibility();
                        return;
                    }
                    else
                    {
                        tp.PressTeleportButtonOnLocalClient();
                        displayText = $"{ConfigSettings.tpMessageString.Value}\n";
                        return;
                    } 
                }
                else displayText = $"Teleporter has {Mathf.Round(cooldownTime)} seconds remaining on cooldown.\r\n";
                return;
            }
            else displayText = "Can't teleport at this time.\n Do you even have a teleporter?\n";
            return;
        }

        internal static void InverseTeleporterCommand(out string displayText)
        {
            
            //TerminalNode node = getTerm.currentNode;

            ShipTeleporter[] objectsOfType = Object.FindObjectsOfType<ShipTeleporter>();
            ShipTeleporter tp = (ShipTeleporter)null;
            foreach (ShipTeleporter inversetp in objectsOfType)
            {
                if (inversetp.isInverseTeleporter)
                {
                    tp = inversetp;
                    break;
                }
            }
            if ((Object)tp != (Object)null)
            {
                float cooldownTime = tp.cooldownTime;
                if (!(StartOfRound.Instance.inShipPhase) && tp.buttonTrigger.interactable)
                {
                    tp.PressTeleportButtonOnLocalClient();
                    displayText = $"{ConfigSettings.itpMessageString.Value}\n";
                    return;
                }
                else if (Mathf.Round(cooldownTime) > 0)
                {
                    displayText = $"Inverse Teleporter has {Mathf.Round(cooldownTime)} seconds remaining on cooldown.\r\n";
                    return;
                }
                else
                {
                    displayText = $"Can't Inverse Teleport from space...\r\n"; //test
                    return;
                }


            }
            else displayText = "Can't Inverse Teleport at this time.\n Do you even have an Inverse Teleporter?\n";
            return;
        }

        internal static void LeverControlCommand(out string displayText)
        {
            
            //TerminalNode node = getTerm.currentNode;

            StartMatchLever leverInstance = FindObjectOfType<StartMatchLever>();
            NetworkManager networkManager = Plugin.Terminal.NetworkManager;
            string getLevelName = StartOfRound.Instance.currentLevel.PlanetName;

            if (CanPullLever(networkManager))
            {
                displayText = $"{ConfigSettings.leverString.Value}\n";
                leverInstance.StartCoroutine(LeverPull(leverInstance));
                Plugin.MoreLogs("lever pulled");
            }
            else if (StartOfRound.Instance.travellingToNewLevel)
            {
                displayText = $"We have not yet arrived to {getLevelName}, please wait.\r\n";
            }
            else if (GameNetworkManager.Instance.gameHasStarted)
            {
                displayText = $"{ConfigSettings.leverString.Value}\n";
                leverInstance.StartCoroutine(LeverPull(leverInstance));
                Plugin.MoreLogs("lever pulled");
            }
            else
            {
                displayText = "Cannot pull the lever at this time.\r\n\r\nNOTE: If the game has not been started, only the host can do this.\r\n\r\n";
            }
        }

        private static bool CanPullLever(NetworkManager networkManager)
        {
            return !GameNetworkManager.Instance.gameHasStarted &&
                   !StartOfRound.Instance.travellingToNewLevel &&
                   (object)networkManager != null &&
                   networkManager.IsHost;
        }

        static IEnumerator LeverPull(StartMatchLever leverInstance)
        {
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
        }
    }
}

using OpenLib.CoreMethods;
using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using static OpenLib.ConfigManager.ConfigSetup;
using static TerminalStuff.EventSub.TerminalStart;
using static TerminalStuff.TerminalEvents;

namespace TerminalStuff.EventSub
{
    internal class TerminalBeginUsing
    {
        public static void OnTerminalBeginUse()
        {
            Plugin.MoreLogs("Start Using Terminal Postfix");

            if (Plugin.instance.Terminal == null)
            {
                Plugin.ERROR("FATAL ERROR: Terminal Instance is NULL");
                return;
            }

            Plugin.instance.Terminal.screenText.ActivateInputField();
            Plugin.instance.Terminal.screenText.interactable = true; //force terminal accept input

            StartUsingTerminalCheck(Plugin.instance.Terminal);

            if (StartOfRound.Instance.localPlayerController != null)
                ShouldLockPlayerCamera(false, StartOfRound.Instance.localPlayerController);

            if (Plugin.instance.Terminal.currentNode == null)
            {
                Plugin.WARNING("WARNING: currentNode is NULL, loading home page node");
                Plugin.instance.Terminal.LoadNewNode(Plugin.instance.Terminal.terminalNodes.specialNodes.ToArray()[1]);
            }


            if (ConfigSettings.TerminalStuffMain.specialListNum.ContainsKey(Plugin.instance.Terminal.currentNode))
                return;

            if (!ViewCommands.isVideoPlaying)
                TerminalParse.NetSync(Plugin.instance.Terminal.currentNode);
        }

        internal static void StartUsingTerminalCheck(Terminal instance)
        {
            TerminalNode nextNode = null;

            if (ConfigSettings.TerminalAutoComplete.Value)
            {
                if (Plugin.instance.removeTab)
                {
                    Plugin.Spam("tab is disabled to quit terminal");
                    instance.playerActions.m_Movement_OpenMenu.Disable();
                    instance.playerActions.m_Movement_OpenMenu.ApplyBindingOverride(new InputBinding { path = "<Keyboard>/tab", overridePath = "" });
                    instance.playerActions.m_Movement_OpenMenu.Enable();
                    HUDManager.Instance.ChangeControlTip(0, "Quit terminal : [Esc]", true);
                }
            }

            //refund init
            if (ConfigSettings.TerminalRefund.Value && ConfigSettings.ModNetworking.Value)
            {
                Plugin.Spam("Syncing items between players for refund command");
                NetHandler.Instance.SyncDropShipServerRpc();
            }

            //AlwaysOn Functions
            if (!alwaysOnDisplay)
            {
                Plugin.Spam("disabling cams views, alwaysOnDisplay is [DISABLED]");

                //Loading specific startpage or nothing at all
                ChooseStartPage(instance, ref nextNode);
            }
            else
            {
                Plugin.MoreLogs("Terminal is Always On, checking for active monitoring or active video");
                if (Plugin.instance.isOnMirror || Plugin.instance.isOnCamera || Plugin.instance.isOnMap || Plugin.instance.isOnMiniCams || Plugin.instance.isOnMiniMap || Plugin.instance.isOnOverlay)
                {
                    Plugin.Spam($"One of the following is true.\nMap: {Plugin.instance.isOnMap} \nCams: {Plugin.instance.isOnCamera} \nMiniMap: {Plugin.instance.isOnMiniMap} \nMiniCams: {Plugin.instance.isOnMiniCams} \nOverlay: {Plugin.instance.isOnOverlay}\nMirror: {Plugin.instance.isOnMirror}");
                    if (lastText.Length > 0 && ConfigSettings.SaveLastInput.Value)
                        LogicHandling.SetTerminalInput(lastText);

                    return;
                }
                else if (ViewCommands.isVideoPlaying && Plugin.instance.Terminal.currentNode == VideoManager.videoPlayerNode)
                {
                    Plugin.Spam($"VideoPlayer: {ViewCommands.isVideoPlaying}\nCurrently Playing: {VideoManager.currentlyPlaying}");
                    return;
                }
                else
                {
                    //Loading specific startpage or nothing at all
                    ChooseStartPage(instance, ref nextNode);
                }
            }

            if (ConfigSettings.NetworkedNodes.Value)
            {
                if (nextNode == null)
                {
                    Plugin.MoreLogs("Failed to grab nextNode for server sync!");
                    return;
                }
                Plugin.Spam("sending current node to other users");
                TerminalParse.NetSync(nextNode);
            }
        }
        //end of void

        internal static void ChooseStartPage(Terminal instance, ref TerminalNode nextNode)
        {
            //Loading specific startpage or nothing at all
            if (terminalSettings.startPage != null)
            {
                SplitViewChecks.DisableSplitView("neither");
                ViewCommands.isVideoPlaying = false;

                List<MainListing> fullListings =
                    [
                        defaultListing, ConfigSettings.TerminalStuffMain
                    ];

                if (LogicHandling.TryGetFuncFromNode(fullListings, ref terminalSettings.startPage, out Func<string> displayTextSupplier))
                {
                    string displayText = displayTextSupplier();
                    Plugin.MoreLogs("running function related to displaytext supplier");
                    terminalSettings.startPage.displayText = displayText;
                }

                nextNode = terminalSettings.startPage;
                instance.LoadNewNode(terminalSettings.startPage);
            }
            else if (Plugin.instance.Terminal.currentNode == null)
            {
                Plugin.WARNING("currentNode is NULL, loading home page as fail-safe");
                instance.LoadNewNode(startNode);
                nextNode = startNode;
            }
            else
            {
                if (ViewCommands.AnyActiveMonitoring() || Plugin.instance.isOnMirror)
                {
                    Plugin.MoreLogs("Entering terminal and enabling any active cameras");
                    SplitViewChecks.ShowCameraView(true);
                }
            }

            if (lastText.Length > 0 && ConfigSettings.SaveLastInput.Value)
                LogicHandling.SetTerminalInput(lastText);
        }
    }
}

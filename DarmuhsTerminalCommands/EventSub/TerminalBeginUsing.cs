using OpenLib.CoreMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using TerminalStuff.Configs;
using TerminalStuff.PluginCore;
using UnityEngine.InputSystem;
using static TerminalStuff.EventSub.TerminalStart;
using static TerminalStuff.TerminalEvents;

namespace TerminalStuff.EventSub;

internal class TerminalBeginUsing
{
    public static void OnTerminalBeginUse()
    {
        Loggers.LogInfo("Start Using Terminal Postfix");

        if (Plugin.instance.Terminal == null)
        {
            Loggers.ERROR("FATAL ERROR: Terminal Instance is NULL");
            return;
        }

        Plugin.instance.Terminal.screenText.ActivateInputField();
        Plugin.instance.Terminal.screenText.interactable = true; //force terminal accept input

        StartUsingTerminalCheck(Plugin.instance.Terminal);

        if (StartOfRound.Instance.localPlayerController != null)
            ShouldLockPlayerCamera(false, StartOfRound.Instance.localPlayerController);

        if (Plugin.instance.Terminal.currentNode == null)
        {
            Loggers.WARNING("WARNING: currentNode is NULL, loading home page node");
            Plugin.instance.Terminal.LoadNewNode(Plugin.instance.Terminal.terminalNodes.specialNodes.ToArray()[1]);
        }

        List<CommandManager> enabled = Commands.GetEnabledCommands();

        if (enabled.Any(x => x.VerySpecialNum != -1 && Plugin.instance.Terminal.currentNode == x.terminalNode))
            return;

        //if (ConfigSettings.TerminalStuffMain.specialListNum.ContainsKey(Plugin.instance.Terminal.currentNode))
        //return;

        if (!ViewCommands.isVideoPlaying)
            TerminalParse.NetSync(Plugin.instance.Terminal.currentNode!);
    }

    internal static void StartUsingTerminalCheck(Terminal instance)
    {
        TerminalNode nextNode = null!;

        if (QoLConfig.TerminalAutoComplete.Value)
        {
            if (Plugin.instance.removeTab)
            {
                Loggers.LogDebug("tab is disabled to quit terminal");
                instance.playerActions.m_Movement_OpenMenu.Disable();
                instance.playerActions.m_Movement_OpenMenu.ApplyBindingOverride(new InputBinding { path = "<Keyboard>/tab", overridePath = "" });
                instance.playerActions.m_Movement_OpenMenu.Enable();
                HUDManager.Instance.ChangeControlTip(0, "Quit terminal : [Esc]", true);
            }
        }

        //refund init
        if (Commands.TerminalRefund.Value && ConfigSettings.ModNetworking.Value)
        {
            Loggers.LogDebug("Syncing items between players for refund command");
            NetHandler.Instance.SyncDropShipServerRpc();
        }

        ChooseStartPage(instance, ref nextNode);

        if (ConfigSettings.NetworkedNodes.Value)
        {
            if (nextNode == null) //not loading any new nodes
                return;

            Loggers.LogDebug("sending current node to other users");
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

            if (LogicHandling.TryGetFuncFromTerminalNode(ref terminalSettings.startPage, out Func<string> supplier))
            {
                string displayText = supplier();
                Loggers.LogInfo("related function has started for terminal start page!");
                terminalSettings.startPage.displayText = displayText;
            }

            nextNode = terminalSettings.startPage;
            instance.LoadNewNode(terminalSettings.startPage);
        }
        else if (Plugin.instance.Terminal.currentNode == null)
        {
            Loggers.WARNING("currentNode is NULL, loading home page as fail-safe");
            instance.LoadNewNode(startNode);
            nextNode = startNode;
        }
        else
        {
            if (ViewCommands.AnyActiveMonitoring() || Plugin.instance.isOnMirror)
            {
                Loggers.LogInfo("Entering terminal and enabling any active cameras");
                ReturnToMonitoring();
            }
        }

        if (lastText.Length > 0 && QoLConfig.SaveLastInput.Value)
            LogicHandling.SetTerminalInput(lastText);
    }

    internal static void ReturnToMonitoring()
    {
        int[] singleViews = [1, 5, 6];
        int[] multiViews = [2, 3, 4];
        int nodeNum = ViewCommands.GetCurrentNodeNum();

        if (multiViews.Contains(nodeNum))
        {
            SplitViewChecks.CheckForSplitView("multi");
        }

        if (singleViews.Contains(nodeNum))
        {
            SplitViewChecks.CheckForSplitView("single");
        }
    }
}

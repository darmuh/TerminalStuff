using OpenLib.InteractiveMenus;
using TerminalStuff.Configs;
using TerminalStuff.VisualElements;
using UnityEngine.InputSystem;
using static TerminalStuff.AdminCommands;
using static TerminalStuff.DynamicCommands;
using static TerminalStuff.EventSub.TerminalStart;
using static TerminalStuff.Networking.NetHandler;
using static TerminalStuff.CommandHandling.ShipControls;
using static TerminalStuff.SpecialStuff.ShortcutBindings;
using static TerminalStuff.TerminalEvents;
using static TerminalStuff.SpecialStuff.WalkieInTerm;
using TerminalStuff.CommandHandling;
using TerminalStuff.Compatibility;

namespace TerminalStuff.Util;

internal class Bools
{
    internal static bool ListenForShortCuts()
    {
        if (!QoLConfig.TerminalShortcuts.Value || keyActions.Count < 1)
            return false;

        if (Plugin.instance.suitsTerminal && SuitsTerminalCompatibility.CheckForSuitsMenu())
            return false;

        if (MenusContainer.AnyMenuActive())
            return false;

        if (!Plugin.instance.Terminal.terminalInUse)
            return false;

        if (!QoLConfig.TerminalShortcuts.Value || stopForAnyReason)
            return false;

        if (ITAPICheck())
            return false;

        return true;
    }

    internal static bool ITAPICheck()
    {
        if (!OpenLib.Plugin.instance.ITAPI)
            return false;

        return OpenLib.Compat.InteractiveTermAPI.ApplicationInUse();
    }

    //check if any key that is bound by this mod is pressed
    internal static bool AnyKeyIsPressed()
    {
        foreach (var keyAction in keyActions)
        {
            if (Keyboard.current[keyAction.Key].isPressed)
            {
                keyBeingPressed = keyAction.Key;
                Loggers.LogInfo($"Key detected in use: {keyAction.Key}");
                return true;
            }
        }
        return false;
    }

    internal static void ResetEnumBools()
    {
        delayStartEnum = false;
        quitTerminalEnum = false;
        leverEnum = false;
        fovEnum = false;
        RainbowFlashRoutine = false;
        kickEnum = false;
        walkieEnum = false;
    }

    internal static bool ShouldAddCamsLogic()
    {
        if (Commands.TerminalCams.Value)
            return true;
        if (Commands.TerminalMap.Value)
            return true;
        if (Commands.TerminalMinicams.Value)
            return true;
        if (Commands.TerminalMinimap.Value)
            return true;
        if (Commands.TerminalOverlay.Value)
            return true;
        return false;
    }

    internal static bool MapCameraUsed()
    {
        if (!StartOfRound.Instance.localPlayerController.isInHangarShipRoom) //not in ship
            return false;

        if (Plugin.instance.isOnMap || Plugin.instance.isOnMiniCams || Plugin.instance.isOnMiniMap || Plugin.instance.isOnOverlay || (bool)Plugin.instance.Terminal.displayingPersistentImage)
            return true;

        return false;
    }

    internal static bool ShouldEnableImage(TerminalNode node)
    {
        Loggers.LogDebug($"ShouldEnableImage? - {node.name}");

        if (Plugin.instance.suitsTerminal)
        {
            if (SuitsTerminalCompatibility.CheckForSuitsMenu())
            {
                SplitViewChecks.ResetPluginInstanceBools();
                SplitViewChecks.DisableSplitView("neither");
                return false;
            }
        }

        if (node.displayVideo != null)
            return true;

        if (node.displayTexture != null)
            return true;

        if (ViewCommands.AnyActiveMonitoring())
            return true;

        if (Plugin.instance.isOnMirror)
            return true;

        if (Plugin.instance.Terminal.currentNode == null)
            return false;

        if (MoreCamStuff.excludedNames.Contains(node.name) && !MoreCamStuff.HideCams() && Plugin.instance.Terminal.terminalImage.enabled)
            return true;

        return false;
    }

    internal static bool AnyMonitoringModesEnabled()
    {
        return Commands.TerminalMap.Value || Commands.TerminalCams.Value || Commands.TerminalOverlay.Value || Commands.TerminalMinimap.Value || Commands.TerminalMinicams.Value;
    }

}

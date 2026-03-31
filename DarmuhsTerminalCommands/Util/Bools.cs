using OpenLib.InteractiveMenus;
using TerminalStuff.CommandHandling;
using TerminalStuff.Compatibility;
using TerminalStuff.Configs;
using TerminalStuff.MoonsTweaks;
using TerminalStuff.VisualElements;
using UnityEngine.InputSystem;
using static TerminalStuff.AdminCommands;
using static TerminalStuff.CommandHandling.ShipControls;
using static TerminalStuff.CommandHandling.ViewCommands;
using static TerminalStuff.DynamicCommands;
using static TerminalStuff.EventSub.TerminalStart;
using static TerminalStuff.Networking.NetHandler;
using static TerminalStuff.SpecialStuff.ShortcutBindings;
using static TerminalStuff.SpecialStuff.WalkieInTerm;
using static TerminalStuff.TerminalEvents;

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

        if(CurrentView == ViewMode.Map)
            return true;

        if (CurrentView == ViewMode.MiniMap)
            return true;

        if (CurrentView == ViewMode.MiniCams)
            return true;

        if (CurrentView == ViewMode.Overlay)
            return true;

        if ((bool)Plugin.instance.Terminal.displayingPersistentImage)
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
                CamEvents.UpdateCamsEvent.Invoke(ViewMode.None);
                return false;
            }
        }

        if (MoonsPlus.IsNodeMoonsPlus(node))
        {
            Plugin.instance.Terminal.displayingPersistentImage = null;
            CurrentView = ViewMode.None;
            bool result = MoonsPlus.HijackTerminalImage();
            MoonsPlus.TerminalReelSetDimensions(result);
            return result;
        }

        if (node.displayVideo != null)
            return true;

        if (node.displayTexture != null)
        {
            if (node.name == "ViewInsideShipCam 1" && StartOfRound.Instance.inShipPhase)
                return false;

            if (CurrentView == ViewMode.Vanilla)
            {
                CamEvents.UpdateCamsEvent.Invoke(ViewMode.None);
                Loggers.LogDebug("Disabling vanilla view monitor image");
                return false;
            }
            else
            {
                Loggers.LogDebug("Something else is using node.displayTexture, keeping it enabled");
                return true;
            }    
        } 

        if (AnyActiveMonitoring())
            return true;

        if (CurrentView == ViewMode.Mirror)
            return true;

        if (MoreCamStuff.DontHideMonitoringNodes.Contains(node.name) && !MoreCamStuff.CanHideCams() && Plugin.instance.Terminal.terminalImage.enabled && CurrentView != ViewMode.None)
            return true;

        return false;
    }

    internal static bool AnyMonitoringModesEnabled()
    {
        return Commands.TerminalMap.Value || Commands.TerminalCams.Value || Commands.TerminalOverlay.Value || Commands.TerminalMinimap.Value || Commands.TerminalMinicams.Value;
    }

}

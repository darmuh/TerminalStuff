using OpenLib.Common;
using TerminalStuff.Configs;
using static TerminalStuff.VisualElements.AlwaysOnStuff;
using static TerminalStuff.EventSub.TerminalStart;
using static TerminalStuff.TerminalEvents;
using TerminalStuff.VisualElements;
using TerminalStuff.Util;
using TerminalStuff.CommandHandling;
using TerminalStuff.Compatibility;

namespace TerminalStuff.EventSub;

internal class TerminalQuit
{
    internal static void OnTerminalQuit()
    {
        if (QoLConfig.SaveLastInput.Value && Plugin.instance.Terminal.currentNode != null && Plugin.instance.Terminal.currentNode.name != "TerminalQuit")
        {
            lastText = CommonStringStuff.GetCleanedScreenText(Plugin.instance.Terminal);
            Loggers.LogDebug("grabbed lastText");
        }

        if (StartOfRound.Instance.localPlayerController != null)
            ShouldLockPlayerCamera(true, StartOfRound.Instance.localPlayerController);

        //Plugin.Log.LogInfo($"terminuse set to {__instance.terminalInUse}");
        if (!AlwaysOnDisplay || screenSettings.inUse)
        {
            HandleRegularQuit();
        }
    }

    internal static void OBCTerminalCameraStatus(bool status)
    {
        if (status == false)
        {
            if (Plugin.instance.suitsTerminal)
            {
                if (SuitsTerminalCompatibility.CheckForSuitsMenu())
                    OpenLib.Compat.OpenBodyCamFuncs.TerminalCameraStatus(status);
                else
                {
                    OpenLib.Compat.OpenBodyCamFuncs.TerminalMirrorStatus(status);
                    OpenLib.Compat.OpenBodyCamFuncs.TerminalCameraStatus(status);
                }
            }
            else
            {
                OpenLib.Compat.OpenBodyCamFuncs.TerminalMirrorStatus(status);
                OpenLib.Compat.OpenBodyCamFuncs.TerminalCameraStatus(status);
            }
        }
        else if (Plugin.instance.isOnMirror)
        {
            OpenLib.Compat.OpenBodyCamFuncs.TerminalMirrorStatus(status);
        }
        else
            OpenLib.Compat.OpenBodyCamFuncs.TerminalCameraStatus(status);
    }

    private static void HandleRegularQuit()
    {
        if (ViewCommands.AnyActiveMonitoring() || Plugin.instance.isOnMirror)
        {
            Loggers.LogInfo("Leaving terminal and disabling any active cameras");
            SplitViewChecks.ShowCameraView(false);
        }
    }
}

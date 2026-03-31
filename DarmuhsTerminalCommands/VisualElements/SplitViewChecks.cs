using OpenLib.Common;
using OpenLib.CoreMethods;
using UnityEngine;
using UnityEngine.Video;
using static TerminalStuff.Util.Bools;
using static TerminalStuff.EventSub.TerminalQuit;
using TerminalStuff.Util;
using TerminalStuff.CommandHandling;
using TerminalStuff.Compatibility;

namespace TerminalStuff.VisualElements;

internal class SplitViewChecks
{
    internal static bool replacedViewMon = false;
    

    internal static CommandManager viewMon = new("View Monitor Not Available", ["view monitor"], ViewCommands.NoVanillaView)
    {
        AddAtAwake = false
    };

    private static void HandleVanillaMap(bool shouldRemove)
    {
        if (shouldRemove)
        {
            ReplaceViewMonitor();
        }
    }

    private static void ReplaceViewMonitor()
    {
        if (!DynamicBools.TryGetKeyword("view monitor"))
        {
            viewMon.RegisterCommand();
            Loggers.LogDebug("Added view monitor warning");
        }
        else
            Loggers.LogDebug("keyword already exists!");
    }

    public static void InitSplitViewObjects()
    {
        if (!ShouldAddCamsLogic())
        {
            HandleVanillaMap(false);
            return;
        }
        else if (Plugin.instance.OpenBodyCamsMod && OpenBodyCamsCompatibility.IsCreatingCommands())
        {
            Loggers.LogInfo("Not replacing 'view monitor' command, as OpenBodyCams is creating terminal commands");
            return;
        }

        else
            HandleVanillaMap(true);


        if (Plugin.instance.Terminal.terminalImage == null || Plugin.instance.splitViewCreated)
        {
            Loggers.LogInfo("Original terminalImage not found or split view already created");
            return;
        }

        CamEvents.MiniScreenImage = Object.Instantiate(Plugin.instance.Terminal.terminalImage, Plugin.instance.Terminal.terminalImage.transform);

        if (CamEvents.MiniScreenImage.gameObject.GetComponent<VideoPlayer>() != null)
        {
            VideoPlayer extraPlayer = CamEvents.MiniScreenImage.GetComponent<VideoPlayer>();
            Object.Destroy(extraPlayer);
            Loggers.LogDebug("extraPlayer deleted");
        }

        CamEvents.MiniScreenImage.gameObject.name = "MiniScreen";
        Plugin.instance.Terminal.terminalImage.gameObject.name = "terminalImage";

        Plugin.instance.splitViewCreated = true;
    }

    internal static void DisableVanillaViewMonitor(bool disableImage = true)
    {
        Loggers.LogDebug("disabling vanilla view monitor");
        Plugin.instance.Terminal.displayingPersistentImage = null;
        Plugin.instance.Terminal.terminalImage.enabled = !disableImage;
    }

    internal static void ShowCameraView(bool state)
    {
        if (ConfigSettings.CamsUseDetectedMods.Value && Plugin.instance.OpenBodyCamsMod)
        {
            OBCTerminalCameraStatus(state);
        }
        else
            CamStuff.HomebrewCameraState(state, ViewCommands.playerCam);
    }
}

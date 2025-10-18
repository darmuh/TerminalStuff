using System.Text;
using TerminalStuff.Configs;
using TerminalStuff.EventSub;
using UnityEngine;
using static TerminalStuff.Patching.AllMyTerminalPatches;
using static TerminalStuff.VisualElements.MoreCamStuff;
using static TerminalStuff.Util.StringStuff;
using static TerminalStuff.VisualElements.CamEvents;
using TerminalStuff.VisualElements;
using TerminalStuff.Util;
using TerminalStuff.Networking;
using TerminalStuff.Compatibility;

namespace TerminalStuff.CommandHandling;

internal class ViewCommands
{
    internal static bool isVideoPlaying = false;
    internal static RenderTexture mycamTexture = null!;
    internal static Camera playerCam = null!;

    internal static float radarZoom;

    internal static string TermMapEvent()
    {
        if (Plugin.instance.OpenBodyCamsMod && OpenBodyCamsCompatibility.IsCreatingCommands())
            return OBCTerminalCommand();

        if (StartOfRound.Instance != null && StartOfRound.Instance.shipDoorsEnabled)
        {
            HandleMapEvent(out string message);
            return message;
        }
        else
        {
            HandleOrbitMapEvent(out string message);
            return message;
        }
    }

    private static void HandleMapEvent(out string displayText)
    {
        if (!Plugin.instance.isOnMap)
        {
            UpdateCamsEvent.Invoke("map");
            DisplayTextUpdater(out string message);
            displayText = message;
            return;
        }
        else
        {
            SplitViewChecks.DisableSplitView("map");
            displayText = $"{ConfigSettings.MapOffString.Value}\n";
            return;
        }
    }

    private static void HandleOrbitMapEvent(out string displayText)
    {
        TerminalNode node = Plugin.instance.Terminal.currentNode;

        Loggers.LogInfo("This should only trigger in orbit");
        node.clearPreviousText = true;
        node.loadImageSlowly = false;
        displayText = "Radar view not available in orbit.\n";
        ResetPluginInstanceBools();
        return;
    }

    internal static string HandlePreviousSwitchEvent()
    {
        Loggers.LogInfo("switching to previous player event detected");
        string displayText = "Ope, this shouldn't show up.... (HandlePreviousSwitchEvent)";
        bool earlyReturn = false;
        if (!AnyActiveMonitoring())
            displayText = AutoMonitor(out earlyReturn);

        if (earlyReturn)
            return displayText;

        int newTarget = GetPrevValidTarget(GameStuff.TerminalMapRenderer.radarTargets, GameStuff.TerminalMapRenderer.targetTransformIndex);
        TargetSwitchCheck(newTarget);
        DisplayTextUpdater(out string message, newTarget);

        return message;
    }

    internal static void TargetSwitchCheck(int target)
    {
        GameStuff.TerminalMapRenderer.SwitchRadarTargetAndSync(target);

        if (Plugin.instance.TwoRadarMapsMod && NetHandler.Instance != null && ConfigSettings.NetworkedNodes.Value)
        {
            Loggers.LogDebug("Second radar requires syncing!");
            NetHandler.Instance.SyncRadarMapRpc(target);
        }
    }

    internal static string SwitchCommandHandler()
    {
        string displayText = "Ope, this shouldn't show up.... (SwitchCommandHandler)";
        bool earlyReturn = false;
        if (!AnyActiveMonitoring())
            displayText = AutoMonitor(out earlyReturn);

        if (earlyReturn)
            return displayText;

        string val = GetAfterKeyword(GetKeywordsPerConfigItem(KeywordConfigs.SwitchKeywords.Value));

        if (val.Length > 1)
        {
            Loggers.LogInfo("switch to specific player command detected");

            int playernum = TerminalEvents.PlayerNameToTargetInt(val, GameStuff.TerminalMapRenderer.radarTargets);
            Loggers.LogDebug($"PlayerNameToTarget determined playernum - {playernum}");
            if (playernum != -1)
            {
                TargetSwitchCheck(playernum);
                DisplayTextUpdater(out displayText, playernum);
                return displayText;
            }

            Loggers.LogInfo("PlayerName returned invalid number");
            displayText = $"Unable to switch to Unknown Player - [ {val} ]";
            return displayText;
        }
        else
        {
            Loggers.LogInfo("switch command detected");
            int newTarget = GetNextValidTarget(GameStuff.TerminalMapRenderer.radarTargets, GameStuff.TerminalMapRenderer.targetTransformIndex);
            TargetSwitchCheck(newTarget);

            DisplayTextUpdater(out displayText, newTarget);
            return displayText;
        }
    }

    internal static string AutoMonitor(out bool earlyReturn)
    {
        earlyReturn = true;

        if (!Plugin.instance.splitViewCreated)
        {
            if (StartOfRound.Instance.inShipPhase)
                return "There is no active monitoring and you are currently in orbit!";

            if (!OpenLib.Common.Misc.CompareStringsInvariant(ConfigSettings.MonitoringDefaultView.Value, "none"))
            {
                if (TerminalStart.viewMonitorVanilla != null)
                {
                    Plugin.instance.Terminal.LoadNewNode(TerminalStart.viewMonitorVanilla);
                    earlyReturn = false;
                    return TerminalStart.viewMonitorVanilla.displayText;
                }
                else
                    return "There is no active monitoring to switch!\n\n";
            }
            else
                return "There is no active monitoring to switch!\n\n";
        }

        if (GetDefaultNodeNum(out int modeNum))
        {
            earlyReturn = false;
            return SyncViewNodeWithNum(modeNum, "");
        }
        else
            return "There is no active monitoring to switch!\n\n";
    }

    internal static int GetCurrentNodeNum()
    {
        if (isVideoPlaying) //VideoPlayer
        {
            return 0;
        }
        else if (Plugin.instance.isOnCamera) // cams
        {
            return 1;
        }
        else if (Plugin.instance.isOnOverlay) //overlay
        {
            return 2;
        }
        else if (Plugin.instance.isOnMiniMap) //minimap
        {
            return 3;
        }
        else if (Plugin.instance.isOnMiniCams) //minicams
        {
            return 4;
        }
        else if (Plugin.instance.isOnMap) //map
        {
            return 5;
        }
        else if (Plugin.instance.isOnMirror) //mirror
        {
            return 6;
        }

        Loggers.LogInfo("No matching views detected");
        return -1;
    }

    internal static bool GetDefaultNodeNum(out int modeNum)
    {
        string config = ConfigSettings.MonitoringDefaultView.Value.ToLower();

        if (config == "map" && Commands.TerminalMap.Value)
        {
            modeNum = 5;
            return true;
        }
        else if (config == "cams" && Commands.TerminalCams.Value)
        {
            modeNum = 1;
            return true;
        }
        else if (config == "overlay" && Commands.TerminalOverlay.Value)
        {
            modeNum = 2;
            return true;
        }
        else if (config == "minimap" && Commands.TerminalMinimap.Value)
        {
            modeNum = 3;
            return true;
        }
        else if (config == "minicams" && Commands.TerminalMinicams.Value)
        {
            modeNum = 4;
            return true;
        }
        else if (config != "none" && Bools.AnyMonitoringModesEnabled())
        {
            if (Commands.TerminalMap.Value)
            {
                modeNum = 5;
                return true;
            }
            else if (Commands.TerminalCams.Value)
            {
                modeNum = 1;
                return true;
            }
            else if (Commands.TerminalOverlay.Value)
            {
                modeNum = 2;
                return true;
            }
            else if (Commands.TerminalMinimap.Value)
            {
                modeNum = 3;
                return true;
            }
            else if (Commands.TerminalMinicams.Value)
            {
                modeNum = 4;
                return true;
            }
        }

        modeNum = -1;
        return false;
    }

    internal static string SyncViewNodeWithNum(int nodeNumber, string nodeText)
    {
        Loggers.LogInfo("---------------- Loading view node triggered by another player ----------------");

        if (nodeNumber == 0) //VideoPlayer
        {
            if (ConfigSettings.VideoSync.Value)
            {
                VideoManager.PlaySyncedVideo();
                return nodeText;
            }
            else
                return LolVideoPlayerEvent();
        }
        else if (nodeNumber == 1) // cams
        {
            return TermCamsEvent();
        }
        else if (nodeNumber == 2) //overlay
        {
            return OverlayTermEvent();
        }
        else if (nodeNumber == 3) //minimap
        {
            return MiniMapTermEvent();
        }
        else if (nodeNumber == 4) //minicams
        {
            return MiniCamsTermEvent();
        }
        else if (nodeNumber == 5) //map
        {
            return TermMapEvent();
        }
        else if (nodeNumber == 6) //mirror
        {
            return MirrorEvent();
        }
        else
            Loggers.LogInfo("No matching views detected");

        return nodeText;
    }

    internal static string MirrorEvent()
    {
        isVideoPlaying = false;
        if (Plugin.instance.OpenBodyCamsMod && OpenBodyCamsCompatibility.IsCreatingCommands())
            return OBCTerminalCommand();

        if (Plugin.instance.isOnMirror == false || (bool)Plugin.instance.Terminal.displayingPersistentImage)
        {
            SetMirrorState(true);
            UpdateCamsEvent.Invoke("mirror");

            Loggers.LogInfo("Mirror added to terminal screen");
            DisplayTextUpdater(out string displayText);
            return displayText;
        }
        else
        {
            SetMirrorState(false);
            SplitViewChecks.DisableSplitView("mirror");
            Loggers.LogInfo("mirror removed");
            return $"\n\n\t>>Mirror Camera removed from terminal.\n\n";
        }
    }

    internal static string TermCamsEvent()
    {
        isVideoPlaying = false;
        if (Plugin.instance.OpenBodyCamsMod && OpenBodyCamsCompatibility.IsCreatingCommands())
            return OBCTerminalCommand();

        if (Plugin.instance.OpenBodyCamsMod && ConfigSettings.CamsUseDetectedMods.Value)
        {
            if (!OpenLib.Compat.OpenBodyCamFuncs.BodyCamIsUnlocked() && ConfigSettings.ObcRequireUpgrade.Value)
                return "\tThis command is currently <color=#ff1a1a>unavailable</color>!\n\nPlease purchase the <color=#ffff66>BodyCam upgrade</color> to use this command.\n\n";
        }

        if (Plugin.instance.isOnCamera == false && Plugin.instance.splitViewCreated)
        {
            UpdateCamsEvent.Invoke("cams");

            // Enable split view and update bools
            SplitViewChecks.EnableSplitView("cams");

            Loggers.LogInfo("Cam added to terminal screen");
            DisplayTextUpdater(out string displayText);
            return displayText;
        }
        else
        {
            SplitViewChecks.DisableSplitView("cams");
            string displayText = $"{ConfigSettings.CamOffString.Value}\n";
            Loggers.LogInfo("Cams removed");
            return displayText;
        }
    }

    internal static string MiniCamsTermEvent()
    {
        isVideoPlaying = false;

        if (Plugin.instance.OpenBodyCamsMod && OpenBodyCamsCompatibility.IsCreatingCommands())
            return OBCTerminalCommand();

        if (Plugin.instance.OpenBodyCamsMod && ConfigSettings.CamsUseDetectedMods.Value)
        {
            if (!OpenLib.Compat.OpenBodyCamFuncs.BodyCamIsUnlocked() && ConfigSettings.ObcRequireUpgrade.Value)
                return "\tThis command is currently <color=#ff1a1a>unavailable</color>!\n\nPlease purchase the <color=#ffff66>BodyCam upgrade</color> to use this command.\n\n";
        }

        if (Plugin.instance.splitViewCreated && !Plugin.instance.isOnMiniCams)
        {
            UpdateCamsEvent.Invoke("minicams");

            DisplayTextUpdater(out string displayText);
            return displayText;
        }
        else
        {
            SplitViewChecks.DisableSplitView("minicams");
            return $"{ConfigSettings.MiniCamsOffString.Value}\n";
        }
    }

    internal static string RadarZoomEvent()
    {
        string val = GetAfterKeyword(GetKeywordsPerConfigItem(KeywordConfigs.RadarZoomKWs.Value));

        if (!AnyActiveMonitoring() && Plugin.instance.splitViewCreated)
        {
            return $"No active monitoring detected, unable to change zoom.\n\n";
        }
        else if (!Plugin.instance.splitViewCreated && !(bool)Plugin.instance.Terminal.displayingPersistentImage)
        {
            return $"No active monitoring detected, unable to change zoom.\n\n";
        }
        else
        {
            if (val.Length < 1)
            {

                GameStuff.TerminalMapRenderer.cam.orthographicSize = GetNewZoom(ref radarZoom);
                Loggers.LogInfo($"Radar Zoom set to {radarZoom}");

                if (ConfigSettings.NetworkedNodes.Value)
                    NetHandler.Instance.SyncRadarZoomRpc(radarZoom);

                if (!Plugin.instance.splitViewCreated)
                    return $"Radar Zoom level adjusted.\n";

                return $"\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\nRadar Zoom level adjusted.\n";
            }
            else
            {
                if (int.TryParse(val, out int newZoomVal))
                {
                    if (newZoomVal >= 5 && newZoomVal <= 60) //max value of 60
                    {
                        radarZoom = newZoomVal;

                        GameStuff.TerminalMapRenderer.cam.orthographicSize = radarZoom;
                        Loggers.LogInfo($"Radar Zoom set to {radarZoom}");

                        if (ConfigSettings.NetworkedNodes.Value)
                            NetHandler.Instance.SyncRadarZoomRpc(radarZoom);

                        if (!Plugin.instance.splitViewCreated)
                            return $"Radar Zoom level adjusted to new value: {val}\n";

                        return $"\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\nRadar Zoom level adjusted to new value: {val}\n";
                    }
                    else
                        return $"Cannot change zoom to value: {val}.\nValue is too high or too low.\n\n";
                }
                else
                    return $"Cannot change zoom to invalid value: {val}.\n";
            }
        }
    }

    internal static float GetNewZoom(ref float currentZoom)
    {
        if (currentZoom >= 10f)
        {
            currentZoom -= 5f;
        }
        else
            currentZoom = 30f;

        return currentZoom;
    }

    internal static string MiniMapTermEvent()
    {
        isVideoPlaying = false;

        if (Plugin.instance.OpenBodyCamsMod && OpenBodyCamsCompatibility.IsCreatingCommands())
            return OBCTerminalCommand();

        if (Plugin.instance.OpenBodyCamsMod && ConfigSettings.CamsUseDetectedMods.Value)
        {
            if (!OpenLib.Compat.OpenBodyCamFuncs.BodyCamIsUnlocked() && ConfigSettings.ObcRequireUpgrade.Value)
                return "\tThis command is currently <color=#ff1a1a>unavailable</color>!\n\nPlease purchase the <color=#ffff66>BodyCam upgrade</color> to use this command.\n\n";
        }

        if (Plugin.instance.splitViewCreated && !Plugin.instance.isOnMiniMap)
        {
            UpdateCamsEvent.Invoke("minimap");

            DisplayTextUpdater(out string displayText);
            return displayText;
        }
        else
        {
            SplitViewChecks.DisableSplitView("minimap");
            return $"{ConfigSettings.MiniMapOffString.Value}\n";
        }
    }

    internal static string OverlayTermEvent()
    {
        isVideoPlaying = false;

        if (Plugin.instance.OpenBodyCamsMod && OpenBodyCamsCompatibility.IsCreatingCommands())
            return OBCTerminalCommand();

        if (Plugin.instance.OpenBodyCamsMod && ConfigSettings.CamsUseDetectedMods.Value)
        {
            if (!OpenLib.Compat.OpenBodyCamFuncs.BodyCamIsUnlocked() && ConfigSettings.ObcRequireUpgrade.Value)
                return "\tThis command is currently <color=#ff1a1a>unavailable</color>!\n\nPlease purchase the <color=#ffff66>BodyCam upgrade</color> to use this command.\n\n";
        }

        if (Plugin.instance.splitViewCreated && !Plugin.instance.isOnOverlay)
        {
            UpdateCamsEvent.Invoke("overlay");

            DisplayTextUpdater(out string displayText);
            return displayText;
        }
        else
        {
            SplitViewChecks.DisableSplitView("overlay");
            return $"{ConfigSettings.OverlayOffString.Value}\n";
        }
    }



    internal static void SetAnyCamsTrue()
    {
        if (!ConfigSettings.NetworkedNodes.Value || NetHandler.Instance == null)
            Plugin.instance.activeCam = true;
        else
            NetHandler.Instance.SyncMyCamsBoolToEveryoneRpc(true);
    }

    internal static string LolVideoPlayerEvent()
    {
        Loggers.LogInfo("Start of LolEvent");

        TerminalNode node = Plugin.instance.Terminal.currentNode;

        SplitViewChecks.CheckForSplitView("neither"); // Disables split view components if enabled

        if (VideoManager.Videos.Count == 0) //if videos failed to load at launch
            return "No videos available to play!\n\nWomp Womp.\n\n";

        node.clearPreviousText = true;
        FixVideoPatch.VideoCheck = true;

        string displayText = VideoManager.PickVideoToPlay();
        return displayText;
    }

    internal static string OBCTerminalCommand()
    {
        StringBuilder message = new();
        message.AppendLine("This command has been <color=#ff1a1a>deactivated</color> to ensure compatibility with <color=#ffff66>OpenBodyCams'</color> \"view bodycam\" command.\n\n");
        message.AppendLine("If you would like to use this command please disable Terminal Commands in OpenBodyCams' config.\n\n");
        return message.ToString();
    }

    internal static string NoVanillaView()
    {
        StringBuilder message = new();
        message.AppendLine("\tThis command has been <color=#ff1a1a>replaced</color>!\n\nPlease use one of the following alternatives:\n");
        /*
        List<TerminalMenuItem> menus = TerminalMenuItems()

        foreach (TerminalMenuItem menuItem in menus)
        {
            if (menuItem.itemKeywords.Count == 0)
                continue;
            message.AppendLine($"> <color=#ffff66>{OpenLib.Common.CommonStringStuff.GetKeywordsForMenuItem(menuItem.itemKeywords)}</color>\n{menuItem.itemDescription}\n");
        }*/

        return message.ToString();
    }

    internal static void DisplayTextUpdater(out string displayText, int givenIndex = -1)
    {

        Loggers.LogInfo("updating displaytext!!!");
        GetCurrentMode(out string mode);

        string playerName = givenIndex == -1
            ? GameStuff.TerminalMapRenderer.radarTargets[GameStuff.TerminalMapRenderer.targetTransformIndex].name
            : GameStuff.TerminalMapRenderer.radarTargets[givenIndex].name;

        if (mode == "Mirror")
            displayText = "\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\nMirror Enabled.\n\n";
        else if (!Plugin.instance.splitViewCreated)
            displayText = $"Monitoring: {playerName} [{mode}]\n\n";
        else
            displayText = $"\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\nMonitoring: {playerName} [{mode}]\n\n";
        return;
    }

    private static void GetCurrentMode(out string mode)
    {
        if (Plugin.instance.isOnCamera)
        {
            mode = ConfigSettings.CamOnString.Value;
            Loggers.LogInfo("cams mode detected");
            return;
        }
        else if (Plugin.instance.isOnMap)
        {
            mode = ConfigSettings.MapOnString.Value;
            Loggers.LogInfo("map mode detected");
            return;
        }
        else if (Plugin.instance.isOnOverlay)
        {
            mode = ConfigSettings.OverlayOnString.Value;
            Loggers.LogInfo("overlay mode detected");
            return;
        }
        else if (Plugin.instance.isOnMiniMap)
        {
            mode = ConfigSettings.MiniMapOnString.Value;
            Loggers.LogInfo("minimap mode detected");
            return;
        }
        else if (Plugin.instance.isOnMiniCams)
        {
            mode = ConfigSettings.MiniCamsOnString.Value;
            Loggers.LogInfo("minicams mode detected");
            return;
        }
        else if (Plugin.instance.isOnMirror)
        {
            mode = "Mirror";
            Loggers.LogInfo("Mirror mode detected");
            return;
        }
        else if (!Plugin.instance.splitViewCreated && (bool)Plugin.instance.Terminal.displayingPersistentImage)
        {
            mode = "View Monitor";
            Loggers.LogInfo("Vanilla \"view monitor\" detected!");
            return;
        }
        else
        {
            Plugin.Log.LogError("Error with mode return, setting to default value");
            mode = "???";
            return;
        }
    }

    internal static bool AnyActiveMonitoring()
    {
        if (Plugin.instance.isOnMap || Plugin.instance.isOnCamera || Plugin.instance.isOnMiniMap || Plugin.instance.isOnMiniCams || Plugin.instance.isOnOverlay || Plugin.instance.activeCam)
            return true;

        if (!Plugin.instance.splitViewCreated && (bool)Plugin.instance.Terminal.displayingPersistentImage)
            return true;

        return false;
    }
}

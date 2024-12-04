using OpenLib.Menus;
using System.Collections.Generic;
using System.Text;
using TerminalStuff.EventSub;
using UnityEngine;
using static OpenLib.Menus.MenuBuild;
using static TerminalStuff.AllMyTerminalPatches;
using static TerminalStuff.MoreCamStuff;
using static TerminalStuff.StringStuff;
using static TerminalStuff.VisualCore.CamEvents;

namespace TerminalStuff
{
    internal class ViewCommands
    {
        internal static bool isVideoPlaying = false;
        internal static RenderTexture mycamTexture;
        internal static Camera playerCam = null;

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
                displayText = $"{ConfigSettings.MapOffString.Value}\r\n";
                return;
            }
        }

        private static void HandleOrbitMapEvent(out string displayText)
        {
            TerminalNode node = Plugin.instance.Terminal.currentNode;

            Plugin.MoreLogs("This should only trigger in orbit");
            node.clearPreviousText = true;
            node.loadImageSlowly = false;
            displayText = "Radar view not available in orbit.\r\n";
            ResetPluginInstanceBools();
            return;
        }

        internal static string HandlePreviousSwitchEvent()
        {
            Plugin.MoreLogs("switching to previous player event detected");
            string displayText = "Ope, this shouldn't show up.... (SwitchCommandHandler)";
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

            if (Plugin.instance.TwoRadarMapsMod && ConfigSettings.ModNetworking.Value && ConfigSettings.NetworkedNodes.Value)
            {
                Plugin.Spam("Second radar requires syncing!");
                NetHandler.Instance.SyncRadarMapServerRpc((int)StartOfRound.Instance.localPlayerController.playerClientId, target);
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

            string val = GetAfterKeyword(GetKeywordsPerConfigItem(ConfigSettings.SwitchKeywords.Value));

            if (val.Length > 1)
            {
                Plugin.MoreLogs("switch to specific player command detected");

                int playernum = TerminalEvents.PlayerNameToTarget(val, GameStuff.TerminalMapRenderer.radarTargets);
                Plugin.Spam($"PlayerNameToTarget determined playernum - {playernum}");
                if (playernum != -1)
                {
                    TargetSwitchCheck(playernum);
                    DisplayTextUpdater(out displayText, playernum);
                    return displayText;
                }

                Plugin.MoreLogs("PlayerName returned invalid number");
                displayText = $"Unable to switch to Unknown Player - [ {val} ]";
                return displayText;
            }
            else
            {
                Plugin.MoreLogs("switch command detected");
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

                if (ConfigSettings.MonitoringDefaultView.Value.ToLower() != "none")
                {
                    if(TerminalStart.viewMonitorVanilla != null)
                    {
                        Plugin.instance.Terminal.LoadNewNode(TerminalStart.viewMonitorVanilla);
                        earlyReturn = false;
                        return TerminalStart.viewMonitorVanilla.displayText;
                    }
                    else
                        return "There is no active monitoring to switch!\r\n\r\n";
                }
                else
                    return "There is no active monitoring to switch!\r\n\r\n";
            }

            if(GetDefaultNodeNum(out int modeNum))
            {
                earlyReturn = false;
                return SyncViewNodeWithNum(modeNum, "");
            }
            else
                return "There is no active monitoring to switch!\r\n\r\n";
        }

        internal static bool GetDefaultNodeNum(out int modeNum)
        {
            string config = ConfigSettings.MonitoringDefaultView.Value.ToLower();

            if(config == "map" && ConfigSettings.TerminalMap.Value)
            {
                modeNum = 5;
                return true;
            }
            else if (config == "cams" && ConfigSettings.TerminalCams.Value)
            {
                modeNum = 1;
                return true;
            }
            else if(config == "overlay" && ConfigSettings.TerminalOverlay.Value)
            {
                modeNum = 2;
                return true;
            }
            else if (config == "minimap" && ConfigSettings.TerminalMinimap.Value)
            {
                modeNum = 3;
                return true;
            }
            else if (config == "minicams" && ConfigSettings.TerminalMinicams.Value)
            {
                modeNum = 4;
                return true;
            }
            else if (config != "none" && BoolStuff.AnyMonitoringModesEnabled())
            {
                if (ConfigSettings.TerminalMap.Value)
                {
                    modeNum = 5;
                    return true;
                }
                else if (ConfigSettings.TerminalCams.Value)
                {
                    modeNum = 1;
                    return true;
                }
                else if (ConfigSettings.TerminalOverlay.Value)
                {
                    modeNum = 2;
                    return true;
                }
                else if (ConfigSettings.TerminalMinimap.Value)
                {
                    modeNum = 3;
                    return true;
                }
                else if (ConfigSettings.TerminalMinicams.Value)
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
            Plugin.MoreLogs("---------------- Loading view node triggered by another player ----------------");

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
                Plugin.MoreLogs("No matching views detected");

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

                Plugin.MoreLogs("Mirror added to terminal screen");
                DisplayTextUpdater(out string displayText);
                return displayText;
            }
            else
            {
                SetMirrorState(false);
                SplitViewChecks.DisableSplitView("mirror");
                Plugin.MoreLogs("mirror removed");
                return $"\n\n\t>>Mirror Camera removed from terminal.\r\n\r\n";
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
                    return "\tThis command is currently <color=#ff1a1a>unavailable</color>!\n\nPlease purchase the <color=#ffff66>BodyCam upgrade</color> to use this command.\r\n\r\n";
            }

            if (Plugin.instance.isOnCamera == false && Plugin.instance.splitViewCreated)
            {
                UpdateCamsEvent.Invoke("cams");

                // Enable split view and update bools
                SplitViewChecks.EnableSplitView("cams");

                Plugin.MoreLogs("Cam added to terminal screen");
                DisplayTextUpdater(out string displayText);
                return displayText;
            }
            else
            {
                SplitViewChecks.DisableSplitView("cams");
                string displayText = $"{ConfigSettings.CamOffString.Value}\r\n";
                Plugin.MoreLogs("Cams removed");
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
                    return "\tThis command is currently <color=#ff1a1a>unavailable</color>!\n\nPlease purchase the <color=#ffff66>BodyCam upgrade</color> to use this command.\r\n\r\n";
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
                return $"{ConfigSettings.MiniCamsOffString.Value}\r\n";
            }
        }

        internal static string RadarZoomEvent()
        {
            string val = GetAfterKeyword(GetKeywordsPerConfigItem(ConfigSettings.RadarZoomKWs.Value));

            if (!AnyActiveMonitoring() && Plugin.instance.splitViewCreated)
            {
                return $"No active monitoring detected, unable to change zoom.\r\n\r\n";
            }
            else if (!Plugin.instance.splitViewCreated && !(bool)Plugin.instance.Terminal.displayingPersistentImage)
            {
                return $"No active monitoring detected, unable to change zoom.\r\n\r\n";
            }
            else
            {
                if (val.Length < 1)
                {

                    GameStuff.TerminalMapRenderer.cam.orthographicSize = GetNewZoom(ref radarZoom);
                    Plugin.MoreLogs($"Radar Zoom set to {radarZoom}");

                    if (ConfigSettings.NetworkedNodes.Value)
                        NetHandler.Instance.SyncRadarZoomServerRpc(radarZoom);

                    if (!Plugin.instance.splitViewCreated)
                        return $"Radar Zoom level adjusted.\r\n";

                    return $"\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\nRadar Zoom level adjusted.\r\n";
                }
                else
                {
                    if (int.TryParse(val, out int newZoomVal))
                    {
                        if (newZoomVal >= 5 && newZoomVal <= 50)
                        {
                            radarZoom = newZoomVal;

                            GameStuff.TerminalMapRenderer.cam.orthographicSize = radarZoom;
                            Plugin.MoreLogs($"Radar Zoom set to {radarZoom}");

                            if (ConfigSettings.NetworkedNodes.Value)
                                NetHandler.Instance.SyncRadarZoomServerRpc(radarZoom);

                            if (!Plugin.instance.splitViewCreated)
                                return $"Radar Zoom level adjusted to new value: {val}\r\n";

                            return $"\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\nRadar Zoom level adjusted to new value: {val}\r\n";
                        }
                        else
                            return $"Cannot change zoom to value: {val}.\nValue is too high or too low.\r\n\r\n";
                    }
                    else
                        return $"Cannot change zoom to invalid value: {val}.\r\n";
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
                    return "\tThis command is currently <color=#ff1a1a>unavailable</color>!\n\nPlease purchase the <color=#ffff66>BodyCam upgrade</color> to use this command.\r\n\r\n";
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
                return $"{ConfigSettings.MiniMapOffString.Value}\r\n";
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
                    return "\tThis command is currently <color=#ff1a1a>unavailable</color>!\n\nPlease purchase the <color=#ffff66>BodyCam upgrade</color> to use this command.\r\n\r\n";
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
                return $"{ConfigSettings.OverlayOffString.Value}\r\n";
            }
        }



        internal static void SetAnyCamsTrue()
        {
            Plugin.instance.activeCam = true;
            NetHandler.SyncMyCamsBoolToEveryone(true);
        }

        internal static string LolVideoPlayerEvent()
        {
            Plugin.MoreLogs("Start of LolEvent");

            TerminalNode node = Plugin.instance.Terminal.currentNode;

            SplitViewChecks.CheckForSplitView("neither"); // Disables split view components if enabled

            if (VideoManager.Videos.Count == 0) //if videos failed to load at launch
                return "No videos available to play!\n\nWomp Womp.\r\n\r\n";

            node.clearPreviousText = true;
            FixVideoPatch.sanityCheckLOL = true;

            string displayText = VideoManager.PickVideoToPlay(Plugin.instance.Terminal.videoPlayer);
            return displayText;
        }

        internal static string OBCTerminalCommand()
        {
            StringBuilder message = new();
            message.AppendLine("This command has been <color=#ff1a1a>deactivated</color> to ensure compatibility with <color=#ffff66>OpenBodyCams'</color> \"view bodycam\" command.\n\n");
            message.AppendLine("If you would like to use this command please disable Terminal Commands in OpenBodyCams' config.\n\r\n");
            return message.ToString();
        }

        internal static string NoVanillaView()
        {
            StringBuilder message = new();
            message.AppendLine("\tThis command has been <color=#ff1a1a>replaced</color>!\n\nPlease use one of the following alternatives:\n");
            List<TerminalMenuItem> menus = TerminalMenuItems(ConfigSettings.ViewConfig);

            foreach (TerminalMenuItem menuItem in menus)
            {
                if (menuItem.itemKeywords.Count == 0)
                    continue;
                message.AppendLine($"> <color=#ffff66>{OpenLib.Common.CommonStringStuff.GetKeywordsForMenuItem(menuItem.itemKeywords)}</color>\r\n{menuItem.itemDescription}\r\n");
            }

            return message.ToString();
        }

        internal static void DisplayTextUpdater(out string displayText, int givenIndex = -1)
        {

            Plugin.MoreLogs("updating displaytext!!!");
            GetCurrentMode(out string mode);

            string playerName = (givenIndex == -1) 
                ? GameStuff.TerminalMapRenderer.radarTargets[GameStuff.TerminalMapRenderer.targetTransformIndex].name
                : GameStuff.TerminalMapRenderer.radarTargets[givenIndex].name;

            if (mode == "Mirror")
                displayText = "\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\nMirror Enabled.\r\n\n";
            else if(!Plugin.instance.splitViewCreated)
                displayText = $"Monitoring: {playerName} [{mode}]\r\n\n";
            else
                displayText = $"\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\nMonitoring: {playerName} [{mode}]\r\n\n";
            return;
        }

        private static void GetCurrentMode(out string mode)
        {
            if (Plugin.instance.isOnCamera)
            {
                mode = ConfigSettings.CamOnString.Value;
                Plugin.MoreLogs("cams mode detected");
                return;
            }
            else if (Plugin.instance.isOnMap)
            {
                mode = ConfigSettings.MapOnString.Value;
                Plugin.MoreLogs("map mode detected");
                return;
            }
            else if (Plugin.instance.isOnOverlay)
            {
                mode = ConfigSettings.OverlayOnString.Value;
                Plugin.MoreLogs("overlay mode detected");
                return;
            }
            else if (Plugin.instance.isOnMiniMap)
            {
                mode = ConfigSettings.MiniMapOnString.Value;
                Plugin.MoreLogs("minimap mode detected");
                return;
            }
            else if (Plugin.instance.isOnMiniCams)
            {
                mode = ConfigSettings.MiniCamsOnString.Value;
                Plugin.MoreLogs("minicams mode detected");
                return;
            }
            else if (Plugin.instance.isOnMirror)
            {
                mode = "Mirror";
                Plugin.MoreLogs("Mirror mode detected");
                return;
            }
            else if(!Plugin.instance.splitViewCreated && (bool)Plugin.instance.Terminal.displayingPersistentImage)
            {
                mode = "View Monitor";
                Plugin.MoreLogs("Vanilla \"view monitor\" detected!");
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

}

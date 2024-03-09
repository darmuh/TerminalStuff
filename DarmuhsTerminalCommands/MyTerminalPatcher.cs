using BepInEx.Bootstrap;
using HarmonyLib;
using System;
using System.Collections;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Video;
using static TerminalApi.TerminalApi;
using static TerminalStuff.TerminalEvents;


namespace TerminalStuff
{
    public class AllMyTerminalPatches : MonoBehaviour
    {
        public static AllMyTerminalPatches Instance;

        [HarmonyPatch(typeof(Terminal), "Awake")]
        public class AwakeTermPatch : Terminal
        {
            static void Postfix(Terminal __instance)
            {
                Plugin.Terminal = __instance;
                Plugin.MoreLogs($"Setting Plugin.Terminal");
                TerminalStartPatch.firstload = false;
            }
        }

            [HarmonyPatch(typeof(Terminal), "LoadNewNode")]
        public class LoadNewNodePatch : Terminal
        {
            static void Postfix(ref TerminalNode node)
            {
                Plugin.MoreLogs($"LoadNewNode patch, nNS: {NetHandler.netNodeSet}");    
            }
        }

        [HarmonyPatch(typeof(Terminal), "QuitTerminal")]
        public class QuitPatch : Terminal
        {
            static void Postfix(ref Terminal __instance)
            {
                TerminalStartPatch.isTermInUse = __instance.terminalInUse;
                //Plugin.Log.LogInfo($"terminuse set to {__instance.terminalInUse}");
                if (TerminalStartPatch.alwaysOnDisplay)
                {
                    __instance.StartCoroutine(__instance.waitUntilFrameEndToSetActive(active: true));
                    Plugin.MoreLogs("Screen set to active");
                    if (ViewCommands.isVideoPlaying)
                    {
                        __instance.videoPlayer.Pause();
                        __instance.StartCoroutine(WaitUntilFrameEndVideo(__instance));
                    }

                    if(ConfigSettings.alwaysOnDynamic.Value)
                        __instance.StartCoroutine(AlwaysOnDynamic(__instance));
                }
                else
                {
                    if (ViewCommands.externalcamsmod && Plugin.instance.OpenBodyCamsMod && Plugin.instance.activeCam)
                        OpenBodyCamsCompatibility.ForceEnableOBC(false);
                }
                

            }

            private static IEnumerator AlwaysOnDynamic(Terminal instance)
            {
                while(!StartOfRound.Instance.localPlayerController.isPlayerDead && !MoreCommands.keepAlwaysOnDisabled)
                {
                    if(!StartOfRound.Instance.localPlayerController.isInHangarShipRoom && instance.terminalUIScreen.gameObject.activeSelf)
                    {
                        instance.terminalUIScreen.gameObject.SetActive(false);

                        if (ViewCommands.externalcamsmod && Plugin.instance.OpenBodyCamsMod && Plugin.instance.activeCam)
                            OpenBodyCamsCompatibility.ForceEnableOBC(false);

                        Plugin.MoreLogs("Disabling terminal screen.");
                    }  
                    else if (StartOfRound.Instance.localPlayerController.isInHangarShipRoom && !instance.terminalUIScreen.gameObject.activeSelf)
                    {
                        instance.terminalUIScreen.gameObject.SetActive(true);

                        if (ViewCommands.externalcamsmod && Plugin.instance.OpenBodyCamsMod && Plugin.instance.activeCam)
                            OpenBodyCamsCompatibility.ForceEnableOBC(true);

                        Plugin.MoreLogs("Enabling terminal screen.");
                    }      

                    yield return new WaitForSeconds(0.5f);
                }

                if(StartOfRound.Instance.localPlayerController.isPlayerDead)
                {
                    instance.terminalUIScreen.gameObject.SetActive(false);
                    if (ViewCommands.externalcamsmod && Plugin.instance.OpenBodyCamsMod && Plugin.instance.activeCam)
                        OpenBodyCamsCompatibility.ForceEnableOBC(false);

                    Plugin.MoreLogs("Player detected dead, disabling terminal screen.");
                }
            }

            private static IEnumerator WaitUntilFrameEndVideo(Terminal instance)
            {
                yield return new WaitForEndOfFrame();
                if (ViewCommands.isVideoPlaying)
                    instance.videoPlayer.Play();
                Plugin.MoreLogs("attemtped to resume videoplayer");
            }

        }

        [HarmonyPatch(typeof(Terminal), "Start")]
        public class TerminalStartPatch : Terminal
        {
            public static bool doesTPexist = false;
            public static bool doesITPexist = false;
            public static bool alwaysOnDisplay = false;
            public static bool isTermInUse = false;
            public static TerminalNode startNode = null;
            public static TerminalNode helpNode = null;

            public static bool firstload = false;

            internal static void TerminalStartGroup()
            {
                Plugin.MoreLogs("Upgrading terminal with my stuff, smile.");
                DynamicCommands.GetConfigKeywordsToUse();
                OverWriteTextNodes();
                TerminalClockStuff.MakeClock();
                MenuBuild.CreateMenus();
                AlwaysOnStart(Plugin.Terminal, startNode);
                ViewCommands.DetermineCamsTargets();
                InitSecondBodyCam();
                ShortcutBindings.InitSavedShortcuts();
            }

            internal static void TerminalStartGroupDelay()
            {
                Plugin.MoreLogs("Starting TerminalDelayStartEnumerator");
                Plugin.Terminal.StartCoroutine(TerminalDelayStartEnumerator());
            }

            internal static IEnumerator TerminalDelayStartEnumerator()
            {
                yield return new WaitForSeconds(3);
                Plugin.MoreLogs("3 Second delay methods starting.");
                CheckForTPatStart();
                StartOfRound.Instance.mapScreen.SwitchRadarTargetAndSync(0); //fix vanilla bug where you need to switch map target at start
                TerminalClockStuff.StartClockCoroutine();

                SplitViewChecks.DisableSplitView("neither");
                Plugin.MoreLogs("disabling cams views");
                ViewCommands.isVideoPlaying = false;
            }

            private static void OverWriteTextNodes()
            {
                Plugin.MoreLogs("updating displaytext for help and home");
                startNode = Plugin.Terminal.terminalNodes.specialNodes.ToArray()[1];
                helpNode = Plugin.Terminal.terminalNodes.specialNodes.ToArray()[13];
                string original = helpNode.displayText;
                Plugin.MoreLogs(original);
                string replacement = original.Replace("To see the list of moons the autopilot can route to.", "List of moons the autopilot can route to.").Replace("To see the company store's selection of useful items.", "Company store's selection of useful items.").Replace("[numberOfItemsOnRoute]", ">MORE\r\nTo see a list of commands added via darmuhsTerminalStuff\r\n\r\n[numberOfItemsOnRoute]");
                Plugin.MoreLogs($"{replacement}");

                //string dontuse = ">MOONS\r\nTo see the list of moons the autopilot can route to.\r\n\r\n>STORE\r\nTo see the company store's selection of useful items.\r\n\r\n>BESTIARY\r\nTo see the list of wildlife on record.\r\n\r\n>STORAGE\r\nTo access objects placed into storage.\r\n\r\n>OTHER\r\nTo see the list of other commands\r\n\r\n>MORE\r\nTo see a list of commands added via darmuhsTerminalStuff\r\n\r\n1 purchased items on route.";
                Plugin.Terminal.terminalNodes.specialNodes.ToArray()[13].displayText = replacement;
                Plugin.MoreLogs(Plugin.Terminal.terminalNodes.specialNodes.ToArray()[13].displayText);
                Plugin.MoreLogs("~~~~~~~~~~~~~~~~~~~~~~~~~~~~ HELP MODIFIED ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");

                //string maskasciiart = "     ._______.\r\n     | \\   / |\r\n  .--|.O.|.O.|______.\r\n__).-| = | = |/   \\ |\r\np__) (.'---`.)Q.|.Q.|--.\r\n      \\\\___// = | = |-.(__\r\n       `---'( .---. ) (__&lt;\r\n             \\\\.-.//\r\n              `---'\r\n\t\t\t  ";
                string asciiArt = ConfigSettings.homeTextArt.Value;
                asciiArt = asciiArt.Replace("[leadingSpace]", " ");
                asciiArt = asciiArt.Replace("[leadingSpacex4]", "    ");

                //no known compatibility issues with home screen
                startNode.displayText = $"{ConfigSettings.homeLine1.Value}\r\n{ConfigSettings.homeLine2.Value}\r\n\r\n>>Type \"Help\" for a list of commands.\r\n>>Type \"More\" for a list of darmuh's commands.\r\n\r\n{asciiArt}\r\n\r\n{ConfigSettings.homeLine3.Value}\r\n\r\n";

                StopPersistingKeywords();
                ChangeVanillaKeywords();
            }

            //change vanilla terminal stuff here
            static void Postfix(ref Terminal __instance)
            {
                doesTPexist = false;
                doesITPexist = false;
                isTermInUse = __instance.terminalInUse;

                TerminalStartGroup();
                TerminalStartGroupDelay();
            }

            private static void InitSecondBodyCam()
            {
                if(!Plugin.instance.OpenBodyCamsMod)
                    return;

                Plugin.MoreLogs("Initializing Second BodyCam");
                OpenBodyCamsCompatibility.CreateTerminalBodyCam();
            }

            public static void ToggleScreen(bool status)
            {
                    Plugin.Terminal.StartCoroutine(Plugin.Terminal.waitUntilFrameEndToSetActive(status));
                    Plugin.MoreLogs($"Screen set to {status}");
            }

            private static void AlwaysOnStart(Terminal thisterm, TerminalNode startNode)
            {

                if (ConfigSettings.alwaysOnAtStart.Value && !firstload)
                {
                    Plugin.MoreLogs("Setting AlwaysOn Display.");
                    if (ConfigSettings.networkedNodes.Value && ConfigSettings.ModNetworking.Value)
                    {
                        Plugin.MoreLogs("network nodes enabled, syncing alwayson status");
                        NetHandler.Instance.StartAoDServerRpc(true);
                        thisterm.LoadNewNode(startNode);
                        StartofHandling.CheckNetNode(startNode);
                        firstload = true;
                    }
                    else
                    {
                        TerminalStartPatch.alwaysOnDisplay = true;
                        ToggleScreen(true);
                        thisterm.LoadNewNode(startNode);
                        firstload = true;
                    }

                }
            }


            private static void StopPersistingKeywords()
            {
                //deletes keywords at game start if they exist from previous plays
                if(ConfigSettings.terminalTP.Value)
                {
                    DeleteKeyword(ConfigSettings.tpKeyword.Value);
                    DeleteKeyword(ConfigSettings.tpKeyword2.Value);
                }
                if(ConfigSettings.terminalITP.Value)
                {
                    DeleteKeyword(ConfigSettings.itpKeyword.Value);
                    DeleteKeyword(ConfigSettings.itpKeyword2.Value);
                }
                
            }

            private static void ChangeVanillaKeywords()
            {
                //deletes keywords at game start if they exist from previous plays

                DeleteKeyword("view monitor");
                AddCommand("view monitor replacement\n", true, ViewCommands.termViewNodes, "view monitor", true, "ViewInsideShipCam 1", "", "", ViewCommands.TermMapEvent);

            }

            private static void CheckForTPatStart()
            {
                if (!ConfigSettings.terminalTP.Value && !ConfigSettings.terminalITP.Value)
                    return;

                //Add TP keywords ONLY if they have already been purchased and exist
                Plugin.MoreLogs("Checking for purchased Teleporter objects");
                ShipTeleporter[] objectsOfType = UnityEngine.Object.FindObjectsOfType<ShipTeleporter>();

                if (!doesTPexist && ConfigSettings.terminalTP.Value)
                {
                    ShipTeleporter tp = (ShipTeleporter)null;

                    foreach (ShipTeleporter tpobject in objectsOfType)
                    {
                        if (!tpobject.isInverseTeleporter)
                        {
                            tp = tpobject;
                            break;
                        }
                    }

                    if (tp != null)
                    {
                        doesTPexist = true;
                        TerminalEvents.AddTeleportKeywords();
                    }
                    else
                    {
                        Plugin.MoreLogs("TP does not exist yet");
                    }
                }
                if (!doesITPexist && ConfigSettings.terminalITP.Value)
                {
                    ShipTeleporter itp = (ShipTeleporter)null;
                    foreach (ShipTeleporter tpobject in objectsOfType)
                    {
                        if (tpobject.isInverseTeleporter)
                        {
                            itp = tpobject;
                            break;
                        }
                    }

                    if (itp != null && ConfigSettings.terminalITP.Value)
                    {
                        doesITPexist = true;
                        TerminalEvents.AddInverseTeleportKeywords();
                    }
                    else
                    {
                        Plugin.MoreLogs("ITP does not exist yet");
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Terminal), "BeginUsingTerminal")]
        public class Terminal_Begin_Patch
        {
            internal static TerminalNode returnCams = CreateTerminalNode("returning to cams", true);
            internal static void StartUsingTerminalCheck(Terminal instance)
            {
                //refund init
                if (ConfigSettings.terminalRefund.Value)
                {
                    NetHandler.Instance.SyncDropShipServerRpc();
                }

                //walkie functions
                if(ConfigSettings.walkieTerm.Value)
                {
                    instance.StartCoroutine(WalkieTerm.TalkinTerm(instance));
                }

                if(ConfigSettings.terminalShortcuts.Value)
                {
                    instance.StartCoroutine(ShortcutBindings.TerminalShortCuts());
                }

                //AlwaysOn Functions
                if (!TerminalStartPatch.alwaysOnDisplay)
                {
                    SplitViewChecks.DisableSplitView("neither");
                    Plugin.MoreLogs("disabling cams views");
                    ViewCommands.isVideoPlaying = false;

                    //Always load to start if alwayson disabled
                    instance.LoadNewNode(instance.terminalNodes.specialNodes.ToArray()[1]);
                }
                else
                {
                    if (Plugin.instance.isOnMirror || Plugin.instance.isOnCamera || Plugin.instance.isOnMap || Plugin.instance.isOnMiniCams || Plugin.instance.isOnMiniMap || Plugin.instance.isOnOverlay)
                    {
                        ViewCommands.HandleReturnCamsEvent(instance, out string displayText);
                        returnCams.displayText = displayText;
                        instance.LoadNewNode(returnCams);
                        Plugin.MoreLogs($"[returning to camera-type node during AOD]\nMap: {Plugin.instance.isOnMap} \nCams: {Plugin.instance.isOnCamera} \nMiniMap: {Plugin.instance.isOnMiniMap} \nMiniCams: {Plugin.instance.isOnMiniCams} \nOverlay: {Plugin.instance.isOnOverlay}\nMirror: {Plugin.instance.isOnMirror}");
                        return;
                    }
                    else
                    {
                        Plugin.MoreLogs($"[no matching camera-type nodes during AOD]\nMap: {Plugin.instance.isOnMap} \nCams: {Plugin.instance.isOnCamera} \nMiniMap: {Plugin.instance.isOnMiniMap} \nMiniCams: {Plugin.instance.isOnMiniCams} \nOverlay: {Plugin.instance.isOnOverlay}\nMirror: {Plugin.instance.isOnMirror}");
                        instance.LoadNewNode(instance.terminalNodes.specialNodes.ToArray()[1]);
                        return;
                    }
                }


            }

            static void Postfix(ref Terminal __instance)
            {
                TerminalStartPatch.isTermInUse = __instance.terminalInUse;
                StartUsingTerminalCheck(__instance);
                StartofHandling.CheckNetNode(__instance.currentNode);
            }
        }

        [HarmonyPatch(typeof(Terminal), "LoadTerminalImage")]
        public class fixVideoPatch : Terminal
        {
            public static bool sanityCheckLOL = false;
            static void Postfix(ref Terminal __instance, TerminalNode node)
            {

                Terminal instanceCopy = __instance;
                if (node.name == "darmuh's videoPlayer" && sanityCheckLOL)
                {
                    Plugin.MoreLogs("testing patch");
                    if (!ViewCommands.isVideoPlaying)
                    {
                        __instance.videoPlayer.enabled = true;
                        __instance.terminalImage.enabled = true;
                        __instance.videoPlayer.loopPointReached += vp => OnVideoEnd(vp, instanceCopy);

                        __instance.videoPlayer.Play();
                        ViewCommands.isVideoPlaying = true;
                        Plugin.MoreLogs("isVideoPlaying set to TRUE");
                        sanityCheckLOL = false;
                        return;
                    }
                }
            }

            public static void OnVideoEnd(VideoPlayer vp, Terminal instance)
            {
                // This method will be called when the video is done playing
                // Disable the video player and terminal image here
                if (ViewCommands.isVideoPlaying)
                {
                    instance.videoPlayer.enabled = false;
                    instance.terminalImage.enabled = false;
                    ViewCommands.isVideoPlaying = false;
                    sanityCheckLOL = false;
                    Plugin.MoreLogs("isVideoPlaying set to FALSE");
                    instance.videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
                    instance.videoPlayer.source = VideoSource.VideoClip;
                    instance.videoPlayer.aspectRatio = VideoAspectRatio.FitHorizontally;
                    instance.videoPlayer.isLooping = true;
                    instance.videoPlayer.playOnAwake = true;

                }
            }

        }

        [HarmonyPatch(typeof(Terminal), "ParsePlayerSentence")]
        public class Terminal_ParsePlayerSentence_Patch
        {
            static void Postfix(Terminal __instance, ref TerminalNode __result)
            {
                if(ConfigSettings.networkedNodes.Value)
                    NetHandler.NetNodeReset(false);

                Plugin.Terminal = __instance;

                StartofHandling.FirstCheck(__instance, ref __result);

                string cleanedText = GetCleanedScreenText(__instance);
                string[] words = cleanedText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (words.Length < 1)
                    return;

                if ((!TerminalStartPatch.doesTPexist && ConfigSettings.terminalTP.Value) || (!TerminalStartPatch.doesITPexist && ConfigSettings.terminalITP.Value))
                    AddTPKeywords.CheckForTP();
                
                StartofHandling.HandleParsed(__instance, words, out TerminalNode parsedNode);
                if (parsedNode != null)
                {
                    if (parsedNode.name == string.Empty)
                    {
                        parsedNode.name = "parsedNode.darm";
                        Plugin.MoreLogs("setting node name");
                    }

                    __result = parsedNode;
                    StartofHandling.CheckNetNode(__result);
                    return;
                }

            }

            private static string GetCleanedScreenText(Terminal __instance)
            {
                string s = __instance.screenText.text.Substring(__instance.screenText.text.Length - __instance.textAdded);
                return RemovePunctuation(s);
            }
        }

        [HarmonyPatch(typeof(Terminal), "LoadNewNodeIfAffordable")]
        public class AffordableNodePatch
        {
            static void Postfix(Terminal __instance)
            {
                if (!ConfigSettings.terminalRefund.Value)
                    return;
                NetHandler.Instance.SyncDropShipServerRpc();
                Plugin.MoreLogs($"items: {__instance.orderedItemsFromTerminal.Count}");
            }
        }

        private static string RemovePunctuation(string s) //copied from game files
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (char c in s)
            {
                if (!char.IsPunctuation(c))
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().ToLower();
        }

    }
}
using System.Collections;
using System.Collections.Generic;
using TerminalStuff.Compatibility;
using TerminalStuff.PluginCore;
using UnityEngine;
using static OpenLib.ConfigManager.ConfigSetup;
using static OpenLib.CoreMethods.LogicHandling;
using static TerminalStuff.TerminalEvents;

namespace TerminalStuff.EventSub
{
    internal class TerminalStart
    {
        internal static bool alwaysOnDisplay = false;
        internal static TerminalNode startNode = null;
        internal static TerminalNode helpNode = null;
        internal static List<TerminalNode> vanillaNodes = [];
        internal static TerminalNode viewMonitorVanilla = null;
        internal static bool delayStartEnum = false;

        internal static void OnTerminalStart()
        {
            TerminalStartGroup();
            TerminalStartGroupDelay();
        }

        internal static void TerminalStartGroup()
        {
            Plugin.MoreLogs("Upgrading terminal with my stuff, smile.");
            Plugin.Allnodes = GetAllNodes();
            OtherModWords();
            OverWriteTextNodes();
            VanillaNodesCache();
            TerminalClockStuff.MakeClock();
            ShortcutBindings.InitSavedShortcuts();
            TerminalCustomizer.TerminalCustomization();
            MenuBuild.CategoryList();
            SaveManager.InitUnlocks(); // sync upgrades status for this save
        }

        private static void OtherModWords()
        {
            if(Plugin.instance.ITAPI)
                InteractiveAPI.GetITAPIWords();
        }


        private static void OverWriteTextNodes()
        {
            Plugin.MoreLogs("updating displaytext for help");
            helpNode = Plugin.instance.Terminal.terminalNodes.specialNodes.ToArray()[13];

            if (!GameStuff.oneTimeOnly)
            {
                string original = helpNode.displayText;
                //Plugin.Spam(original);
                string replacement = original.Replace("To see the list of moons the autopilot can route to.", "List of moons the autopilot can route to.").Replace("To see the company store's selection of useful items.", "Company store's selection of useful items.");
                //Plugin.Spam($"{replacement}");

                Plugin.instance.Terminal.terminalNodes.specialNodes.ToArray()[13].displayText = replacement;
                Plugin.Spam("~~~~~~~~~~~~~~~~~~~~~~~~~~~~ HELP MODIFIED ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
                GameStuff.oneTimeOnly = true;
            }

            OpenLib.CoreMethods.AddingThings.AddKeywordToExistingNode("home", Plugin.instance.Terminal.terminalNodes.specialNodes.ToArray()[1], true); //startNode

        }

        internal static void VanillaNodesCache()
        {
            vanillaNodes.Clear();

            if (OpenLib.CoreMethods.DynamicBools.TryGetKeyword("store", out TerminalKeyword storeWord))
            {
                TerminalNode storeNode = storeWord.specialKeywordResult;
                vanillaNodes.Add(storeNode);
                Plugin.Spam("storeNode cached");
            }

            if (OpenLib.CoreMethods.DynamicBools.TryGetKeyword("moons", out TerminalKeyword moonsWord))
            {
                TerminalNode moonsNode = moonsWord.specialKeywordResult;
                vanillaNodes.Add(moonsNode);
                Plugin.Spam("moonsNode cached");
            }

            if (OpenLib.CoreMethods.DynamicBools.TryGetKeyword("bestiary", out TerminalKeyword bestiaryWord))
            {
                TerminalNode bestiaryNode = bestiaryWord.specialKeywordResult;
                vanillaNodes.Add(bestiaryNode);
                Plugin.Spam("bestiaryNode cached");
            }

            if (!Plugin.instance.splitViewCreated)
            {
                Plugin.Spam("Trying to cache vanilla view monitor");
                if(TryGetFromAllNodes("ViewInsideShipCam 1", out viewMonitorVanilla))
                {
                    vanillaNodes.Add((viewMonitorVanilla));
                    Plugin.Spam("cached vanilla view monitor!");
                }
            }

        }

        internal static void TerminalStartGroupDelay()
        {
            Plugin.MoreLogs("Starting TerminalDelayStartEnumerator");
            Plugin.instance.Terminal.StartCoroutine(TerminalDelayStartEnumerator());
        }

        internal static IEnumerator TerminalDelayStartEnumerator()
        {
            if (delayStartEnum)
                yield break;

            delayStartEnum = true;
            yield return new WaitForSeconds(1);
            Plugin.MoreLogs("1 Second delay methods starting.");
            SplitViewChecks.CheckForSplitView("neither");
            Plugin.MoreLogs("disabling cams views");
            ViewCommands.isVideoPlaying = false;
            //TerminalClockStuff.StartClockCoroutine();
            AlwaysOnStart(Plugin.instance.Terminal, startNode);
            yield return new WaitForSeconds(0.1f);
            Plugin.instance.Terminal.topRightText.text = $"${Plugin.instance.Terminal.groupCredits}"; //fix creds display for alwayson
            StartCheck(Plugin.instance.Terminal, startNode);
            DebugShowInfo();
            delayStartEnum = false;
        }

        public static void ToggleScreen(bool status)
        {
            OpenLib.Common.CommonTerminal.ToggleScreen(status);
        }

        internal static void InitiateTerminalStuff()
        {
            if (Plugin.instance.Terminal == null)
                return;

            terminalSettings.StartPage(ConfigSettings.TerminalStartPage.Value);
        }

        private static void AlwaysOnStart(Terminal thisterm, TerminalNode startNode)
        {

            if (AlwaysOnStuff.screenSettings.AlwaysOn && !AlwaysOnStuff.screenSettings.inUse)
            {
                Plugin.Spam("Setting AlwaysOn Display.");
                if (ConfigSettings.NetworkedNodes.Value && ConfigSettings.ModNetworking.Value)
                {
                    Plugin.Spam("network nodes enabled, syncing alwayson status");
                    NetHandler.Instance.StartAoDServerRpc(true);
                }
                else
                {
                    alwaysOnDisplay = true;
                    ToggleScreen(true);
                    thisterm.LoadNewNode(startNode);
                }

                if (ConfigSettings.TerminalLightBehaviour.Value == "alwayson")
                    ShouldDisableTerminalLight(false, "alwayson");

            }
        }

        private static void StartCheck(Terminal thisterm, TerminalNode startNode)
        {
            if (startNode == null)
                startNode = Plugin.instance.Terminal.terminalNodes.specialNodes.ToArray()[13];

            if (!ConfigSettings.ModNetworking.Value || !ConfigSettings.NetworkedNodes.Value)
            {
                Plugin.Spam("Networking disabled, returning...");
                thisterm.LoadNewNode(startNode);
                return;
            }

            if (GameNetworkManager.Instance.localPlayerController.IsHost)
            {
                GameStuff.TerminalMapRenderer.SwitchRadarTargetAndSync(0); //fix vanilla bug where you need to switch map target at start
                NetHandler.Instance.SyncRadarZoomServerRpc(ConfigSettings.TerminalRadarDefaultZoom.Value); //host only at load-in
                thisterm.LoadNewNode(startNode);
                StartofHandling.CheckNetNode(startNode);
                return;
            }
            else
            {
                Plugin.instance.Terminal.StartCoroutine(ClientStuffDelayed());
                return;
            }
        }

        private static IEnumerator ClientStuffDelayed()
        {
            yield return new WaitForSeconds(1);
            Plugin.Spam("------------ CLIENT JUST LOADED --------------");
            Plugin.Spam("grabbing node from host");
            Plugin.Spam("------------ CLIENT JUST LOADED --------------");

            int hostClient = Misc.HostClientID();
            NetHandler.Instance.SyncTerminalServerRpc(((int)StartOfRound.Instance.localPlayerController.playerClientId), hostClient);
        }

        private static void DebugShowInfo()
        {
            Plugin.Spam($"Terminal Keywords Count: {Plugin.instance.Terminal.terminalNodes.allKeywords.Length}");
            Plugin.Spam($"Plugin.Allnodes: {Plugin.Allnodes.Count}");
            Plugin.Spam($"TerminalStuffBools.Count: {ConfigSettings.TerminalStuffBools.Count}");
            Plugin.Spam($"TerminalStuffMain.Listing.Count: {ConfigSettings.TerminalStuffMain.Listing.Count}");
            Plugin.Spam($"defaultListing.Listing.Count: {defaultListing.Listing.Count}");
            Plugin.Spam($"defaultManaged.Count: {defaultManaged.Count}");

            Plugin.Spam("------------------------ end of darmuh's debug info ------------------------");

        }
    }
}

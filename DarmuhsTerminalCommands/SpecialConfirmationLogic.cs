using HarmonyLib;
using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;
using static TerminalApi.TerminalApi;
using static TerminalStuff.DynamicCommands;

namespace TerminalStuff
{
    internal class SpecialConfirmationLogic
    {
        internal static List<TerminalNode> confCheckNodes = new List<TerminalNode>();
        internal static List<string> confKeywords = new List<string> { "bioscanpatch" };

        //restart
        public static TerminalNode restartAsk = CreateConfirmationNode("Restart Lobby?\n\n\n\n\n\n\n\n\n\n\n\nPlease CONFIRM or DENY.\n", $"{Restart}.ask");
        public static TerminalNode restartDo = CreateConfirmationNode($"Restart lobby confirmed, getting new ship...\n", $"{Restart}.do");
        public static TerminalNode restartDont = CreateConfirmationNode($"Restart lobby cancelled...\n", $"{Restart}.dont");

        //lever
        public static TerminalNode leverAsk = CreateConfirmationNode("Pull the Lever?\n\n\n\n\n\n\n\n\n\n\n\nPlease CONFIRM or DENY.\n", $"{Lever}.ask");
        public static TerminalNode leverDO = CreateConfirmationNode($"Lever pull confirmed, pulling now...\n", $"{Lever}.do");
        public static TerminalNode leverDONT = CreateConfirmationNode($"Lever pull cancelled...\n", $"{Lever}.dont");

        //link
        public static TerminalNode linkDO = CreateConfirmationNode($"externalLink event", $"link.do");
        public static TerminalNode linkDONT = CreateConfirmationNode($"You have cancelled visiting the site: {linktext}\n", "link.dont");
        public static TerminalNode linkAsk = CreateConfirmationNode($"linkask node\n", $"link.ask");

        // Gamble nodes
        public static TerminalNode gambleDO = CreateConfirmationNode("gamba", "gamble.do");
        public static TerminalNode gambleNO = CreateConfirmationNode($"Gamble cancelled.\n", $"{Gamble}.no");
        public static TerminalNode ask2gamble = CreateConfirmationNode("ask to gamble", $"{Gamble}.ask");

        // Vitals nodes
        public static TerminalNode vitalsUpgradeAsk = CreateConfirmationNode($"Purchase the Vitals Scanner 2.0 Patch?\nThis software update is available for {ConfigSettings.vitalsUpgradeCost.Value} Credits.\n\n\n\n\n\n\n\n\n\n\nPlease CONFIRM or DENY.\n", "vitalspatch.ask");
        public static TerminalNode vitalsDoUpgrade = CreateConfirmationNode("\n", "vitalspatch.Do");
        public static TerminalNode vitalsDontUpgrade = CreateConfirmationNode("You have opted out of purchasing the Vitals Scanner Upgrade.\n", "vitalspatch.dont");
        public static TerminalNode vitalsAlreadyUpgraded = CreateConfirmationNode("Vitals Scanner software has already been updated to the latest patch (2.0).\n", "vitalspatch.alreadyupgraded");

        // BioScan nodes
        public static TerminalNode bioScanUpgradeAsk = CreateConfirmationNode($"Purchase the BioScanner 2.0 Upgrade Patch?\nThis software update is available for {ConfigSettings.bioScanUpgradeCost.Value} Credits.\n\n\n\n\n\n\n\n\n\n\nPlease CONFIRM or DENY.\n", "bioscanpatch.ask");
        public static TerminalNode bioScanDoUpgrade = CreateConfirmationNode("", "bioscanpatch.do");
        public static TerminalNode bioScanDontUpgrade = CreateConfirmationNode("You have opted out of purchasing the BioScanner 2.0 Upgrade Patch.\n", "bioscanpatch.dont");
        public static TerminalNode bioScanAlreadyUpgraded = CreateConfirmationNode("BioScanner software has already been updated to the latest patch (2.0).\n", "bioscanpatch.alreadyupgraded");

        internal static Dictionary<int, Action> confirmationCallbacks = new Dictionary<int, Action>();
        internal static Dictionary<string, int> callbackNumber = new Dictionary<string, int>();

        private static TerminalNode localResult = null;

        internal static int ResolveCallbackToInt(string keyWord, out int callback)
        {
            if (callbackNumber.Count > 0)
                callback = callbackNumber.GetValueSafe(keyWord);
            else
                callback = -1;
            return callback;

        }

        internal static void InitKeywords()
        {
            Plugin.MoreLogs("Adding Confirmation keywords");

            if(ConfigSettings.terminalGamble.Value && ConfigSettings.ModNetworking.Value)
                confKeywords.Add(Gamble);
            if(ConfigSettings.terminalLever.Value)
                confKeywords.Add(Lever);
            if (ConfigSettings.terminalVitalsUpgrade.Value && ConfigSettings.ModNetworking.Value)
                confKeywords.Add("vitalspatch");
            if(ConfigSettings.terminalLink.Value)
                confKeywords.Add(Link);
            if(ConfigSettings.terminalLink2.Value) 
                confKeywords.Add(Link2);
            if (ConfigSettings.terminalRestart.Value)
                confKeywords.Add(Restart);

            foreach (string item in confKeywords)
            {
                Plugin.MoreLogs(item);
            }
        }

        internal static void InitCallbackNums()
        {
            if (callbackNumber.Count > 0)
                callbackNumber.Clear(); //need to wipe the dictionary each time

            callbackNumber.Add(Lever, 1);
            callbackNumber.Add(Gamble, 2);
            callbackNumber.Add("vitalspatch", 3);
            callbackNumber.Add("bioscanpatch", 4);
            callbackNumber.Add(Link, 5);
            callbackNumber.Add(Link2, 5);
            callbackNumber.Add(Restart, 6);
        }

        internal static TerminalNode ConfirmationNodes(Terminal terminal, string[] words, out TerminalNode resultNode)
        {
            Plugin.MoreLogs("Confirmation keywords detected, matching to event");

            if (confirmationCallbacks.Count > 0)
                confirmationCallbacks.Clear(); // need to wipe the dictionary each time

            DefineConfirmationCallbacks(terminal, words); // Define confirmation and denial callbacks

            if (confirmationCallbacks.TryGetValue(Plugin.instance.confirmationNodeNum, out Action callback))
            {
                callback.Invoke();
                Plugin.instance.awaitingConfirmation = false; // Remove confirm check
                Plugin.instance.confirmationNodeNum = 0;
                Plugin.MoreLogs("Callback invoked");
            }
            else
                Plugin.Log.LogError($"Unable to get callback from Number {Plugin.instance.confirmationNodeNum}");

            resultNode = localResult;
            return resultNode;
        }

        private static void DefineConfirmationCallbacks(Terminal terminal, string[] words)
        {
            confirmationCallbacks.Add(1, () => HandleConfirmation(terminal, words, ConfirmLever, DenyLever));
            confirmationCallbacks.Add(2, () => HandleConfirmation(terminal, words, ConfirmGamble, DenyGamble));
            confirmationCallbacks.Add(3, () => HandleConfirmation(terminal, words, ConfirmVitalsUpgrade, DenyVitalsUpgrade));
            confirmationCallbacks.Add(4, () => HandleConfirmation(terminal, words, ConfirmBioScanUpgrade, DenyBioScanUpgrade));
            confirmationCallbacks.Add(5, () => HandleConfirmation(terminal, words, ConfirmCustomLink, DenyCustomLink));
            confirmationCallbacks.Add(6, () => HandleConfirmation(terminal, words, ConfirmRestart, DenyRestart));
        }

        // Define confirmation and denial callbacks
        private static void ConfirmLever() => LeverAction();
        internal static void LeverAction()
        {
            ShipControls.LeverControlCommand(out string displayText);
            leverDO.displayText = displayText;
            SetLocalResult(leverDO, "Confirmation received.");
        }
        private static void DenyLever() => SetLocalResult(leverDONT, "Denial received.");

        private static void ConfirmRestart() => RestartAction();
        internal static void RestartAction()
        {
            if (!StartOfRound.Instance.inShipPhase)
            {
                restartDo.displayText = "This can only be done in orbit...\n\n";
            }
            else if (!GameNetworkManager.Instance.localPlayerController.isHostPlayerObject)
                restartDo.displayText = "Only the host can do this...\r\n";
            else
            {
                restartDo.displayText = "Restart lobby confirmed, getting new ship...\n\n";
                StartOfRound.Instance.ResetShip();
                //GameNetworkManager.Instance.ResetSavedGameValues();
                Object.FindObjectOfType<Terminal>().SetItemSales();
                GameNetworkManager.Instance.SaveGameValues();
                Plugin.MoreLogs("restarting lobby");
            }
                
            SetLocalResult(restartDo, "Confirmation received.");
            
        }
        private static void DenyRestart() => SetLocalResult(restartDont, "Denial received.");


        private static void ConfirmGamble() => GambleAction();
        private static void GambleAction()
        {
            GambaCommands.BasicGambleCommand(out string displayText);
            gambleDO.displayText = displayText;
            SetLocalResult(gambleDO, "Confirmation received.");
        }
        private static void DenyGamble() => SetLocalResult(gambleNO, "Denial received.");

        private static void ConfirmVitalsUpgrade() => DoVitalsUpgrade();
        private static void DoVitalsUpgrade()
        {
            CostCommands.PerformVitalsUpgrade(out string displayText);
            vitalsDoUpgrade.displayText = displayText;
            SetLocalResult(vitalsDoUpgrade, "Confirmation received.");
        }
        private static void DenyVitalsUpgrade() => SetLocalResult(vitalsDontUpgrade, "Denial received.");

        private static void ConfirmBioScanUpgrade() => DoBioScanUpgrade();
        private static void DoBioScanUpgrade()
        {
            CostCommands.PerformBioscanUpgrade(out string displayText);
            vitalsDoUpgrade.displayText = displayText;
            SetLocalResult(bioScanDoUpgrade, "Confirmation received.");
        }
        private static void DenyBioScanUpgrade() => SetLocalResult(bioScanDontUpgrade, "Denial received.");

        private static void ConfirmCustomLink() => SendToLink();
        private static void SendToLink()
        {
            MoreCommands.ExternalLink(out string displayText);
            linkDO.displayText = displayText;
            SetLocalResult(linkDO, "Confirmation received.");
        }
        private static void DenyCustomLink() => SetLocalResult(linkDONT, "Denial received.");

        private static void SetLocalResult(TerminalNode result, string logMessage)
        {
            localResult = result;
            Plugin.Log.LogInfo(logMessage);
        }


        internal static TerminalNode CreateConfirmationNode(string confirmationMessage, string nodeName)
        {
            TerminalNode confirmationNode = CreateTerminalNode(confirmationMessage, true);
            confirmationNode.name = nodeName;
            confCheckNodes.Add(confirmationNode);
            return confirmationNode;
        }

        internal static TerminalNode CreateConfirmationNode(string confirmationMessage, string nodeName, string termEvent)
        {
            TerminalNode confirmationNode = CreateTerminalNode(confirmationMessage, true, termEvent);
            confirmationNode.name = nodeName;
            confCheckNodes.Add(confirmationNode);
            return confirmationNode;
        }

        internal static void GetSpecialQuestionStuff(ref string keyWord)
        {
            if (keyWord == Link)
            {
                linktext = ConfigSettings.customLink.Value;
                linkAsk.displayText = $"Would you like to be taken to the following link?\n\n{linktext}\n\n\n\n\n\n\n\n\n\nPlease CONFIRM or DENY.\n";
                linkDONT.displayText = $"You have cancelled visiting the site:\n\n {linktext}\n";

                keyWord = "link";
                Plugin.MoreLogs($"special question stuff [keyWord:{keyWord}] and [customLink:{ConfigSettings.customLink.Value}]");
            }
            else if (keyWord == Link2)
            {
                linktext = ConfigSettings.customLink2.Value;
                linkAsk.displayText = $"Would you like to be taken to the following link?\n\n{linktext}\n\n\n\n\n\n\n\n\n\nPlease CONFIRM or DENY.\n";
                linkDONT.displayText = $"You have cancelled visiting the site:\n\n {linktext}\n";

                keyWord = "link";
                Plugin.MoreLogs($"special question stuff [keyWord:{keyWord}] and [customLink:{ConfigSettings.customLink2.Value}]");
            }
            else
                Plugin.MoreLogs($"KW: {keyWord}");
        }

        internal static TerminalNode HandleQuestion(string keyWord, out TerminalNode terminalNode)
        {
            terminalNode = null;
            GetSpecialQuestionStuff(ref keyWord);
            bool skipconfirm = SkipConfirmCheck(ref keyWord);
            
            if (!skipconfirm)
            {
                for (int i = 0; i < confCheckNodes.Count; i++)
                {
                    if (confCheckNodes[i].name.ToLower() == $"{keyWord}.ask")
                    {
                        terminalNode = confCheckNodes[i];
                        Plugin.MoreLogs($"node found for {keyWord}");
                        break;
                    }
                }
                return terminalNode;
            }
            else
            {
                for (int i = 0; i < confCheckNodes.Count; i++)
                {
                    if (confCheckNodes[i].name.ToLower() == $"{keyWord}.do")
                    {
                        terminalNode = confCheckNodes[i];
                        Plugin.MoreLogs($"node found for {keyWord}");
                        HandleConfirmationSkip(ref terminalNode);
                        break;
                    }
                }
                return terminalNode;
            }

            
        }

        private static bool SkipConfirmCheck(ref string keyWord)
        {
            if (keyWord == Lever && ConfigSettings.leverConfirmOverride.Value)
                return true;
            else if (keyWord == Restart && ConfigSettings.restartConfirmOverride.Value)
                return true;
            else
                return false;
        }

        private static bool HandleConfirmation(Terminal __instance, string[] words, Action confirmCallback, Action denyCallback)
        {
            if (Plugin.instance.awaitingConfirmation)
            {
                if (words.Length == 1 && (words[0].StartsWith("c")))
                {
                    confirmCallback?.Invoke();
                    Plugin.instance.awaitingConfirmation = false;
                    Plugin.instance.confirmationNodeNum = 0;
                    return true; // Confirmation
                }
                else if (words.Length == 1 && (words[0].StartsWith("d")))
                {
                    denyCallback?.Invoke();
                    Plugin.instance.awaitingConfirmation = false;
                    Plugin.instance.confirmationNodeNum = 0;
                    return true; // Denial
                }
                else
                {
                    Plugin.MoreLogs($"{words[0]} doesn't match confirm or deny");
                    return false;
                }


            }
            else
                return false; // No confirmation or denial
        }

        private static void HandleConfirmationSkip(ref TerminalNode outNode)
        {
            if (outNode == leverDO)
            {
                Plugin.MoreLogs("Handling lever confirmation skip");
                LeverAction();
            }
            else if (outNode == restartDo)
            {
                Plugin.MoreLogs("Handling restart confirmation skip");
                RestartAction();
            }
            else
                Plugin.Log.LogError("Confirmation skip detected but no matching nodes!");
        }
    }
}

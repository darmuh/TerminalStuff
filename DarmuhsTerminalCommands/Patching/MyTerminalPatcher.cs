using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using TerminalStuff.CommandHandling;
using TerminalStuff.Configs;
using TerminalStuff.SpecialStuff;
using TerminalStuff.Util;
using TerminalStuff.VisualElements;
using UnityEngine.Video;


namespace TerminalStuff.Patching;

public class AllMyTerminalPatches
{
    public class ConfigGetters
    {
        public static int GetMaxItems()
        {
            Loggers.LogDebug("GetMaxItems!");
            if (Plugin.instance.GenImprovements)
            {
                int other = Compatibility.GenImproves.GetMaxItems();
                if (other != 12)
                {
                    QoLConfig.TerminalMaxOrderedItems.Value = other;
                    return other;
                }
            }

            //Loggers.LogDebug($"GetMaxItems - {ConfigSettings.TerminalMaxOrderedItems.Value}");
            return QoLConfig.TerminalMaxOrderedItems.Value;
        }

        public static float GetMaxItemsFloat()
        {
            //Loggers.LogDebug($"GetMaxItemsFloat - {ConfigSettings.TerminalMaxOrderedItems.Value}");
            return QoLConfig.TerminalMaxOrderedItems.Value;
        }
    }

    [HarmonyPatch(typeof(Terminal), nameof(ParseWord))]
    public class ConflictResolution : Terminal
    {
        static void Postfix(string playerWord, ref TerminalKeyword __result)
        {

            if (!QoLConfig.TerminalConflictResolution.Value)
                return;

            ConflictRes.InitRes(playerWord, ref __result); //should modify the keyword to whatever resolution finds as the best match
        }
    }
    [HarmonyPatch(typeof(Terminal), nameof(TextPostProcess))]
    public class CustomReplacements : Terminal
    {
        static void Postfix(TerminalNode node, ref string __result)
        {
            if (node.name == "ViewInsideShipCam 1" && StartOfRound.Instance.inShipPhase && !Plugin.instance.splitViewCreated)
            {
                __result = "\n\n\nView monitor is not available in orbit!\n\n";
                return;
            }

            __result = __result.Replace("[leadingSpace]", " ");
            __result = __result.Replace("[leadingSpacex4]", "    ");
            if (StartOfRound.Instance != null)
            {
                if (StartOfRound.Instance.localPlayerController != null)
                {
                    __result = __result.Replace("[thisPlayerName]", $"{StartOfRound.Instance.localPlayerController.playerUsername}");
                    __result = __result.Replace("[thisPlayerHealth]", $"{StartOfRound.Instance.localPlayerController.health}");
                }

                __result = __result.Replace("[currentPlanetName]", $"{StartOfRound.Instance.currentLevel.PlanetName}");
            }

            __result = __result.Replace("[GetMaxPossibleItems]", $"{ConfigGetters.GetMaxItems()}");

        }

    }


    [HarmonyPatch(typeof(Terminal), nameof(waitUntilFrameEndToSetActive))]
    public class QuitTerminalPatch : Terminal
    {
        [HarmonyPrefix]
        static void Prefix(ref bool active)
        {
            Loggers.LogDebug("waitUntilFrameEndToSetActive");

            if (EventSub.TerminalStart.AlwaysOnDisplay)
            {
                Loggers.LogDebug("alwaysOnDisplay is TRUE");

                if (!MoreCommands.keepAlwaysOnDisabled)
                    active = true; //turn screen off

                //Loggers.LogDebug("End of Prefix");
                return;
            }
        }
    }

    [HarmonyPatch(typeof(Terminal), nameof(BeginUsingTerminal))]
    [HarmonyPriority(Priority.Last)]
    public class BeginUsingTranspiler : Terminal
    {
        static int replacements = 0;
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> BeginUsingTerminal_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            Plugin.PatchLog("BeginUsingTerminal Transpiler Initialized");
            MethodInfo LoadNewNode = AccessTools.Method("Terminal:LoadNewNode");
            replacements = 0;
            instructions.DoIf(instruction => instruction.Calls(LoadNewNode), Nothing);
            return instructions;
        }

        private static void Nothing(CodeInstruction instruction)
        {
            replacements++;
            Plugin.PatchLog($"BeginUsingTerminal - Transpiler removed matching instruction\n[ {replacements} ] lines changed");
            instruction.opcode = OpCodes.Nop;
        }
    }

    [HarmonyPatch(typeof(Terminal), nameof(BuyItemsServerRpc))]
    [HarmonyPriority(Priority.Last)]
    public class PurchaseLimitPatch1 : Terminal
    {
        static int replacements = 0;
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> BuyItemsServerRpc_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            Plugin.PatchLog("BuyItemsServerRpc Transpiler Initialized");
            replacements = 0;
            CodeInstruction original = new(OpCodes.Ldc_I4_S, 12);
            instructions.DoIf(instruction => instruction.opcode == original.opcode, OrderChange);
            return instructions;
        }
        static void OrderChange(CodeInstruction instruction)
        {
            replacements++;
            CodeInstruction getter = Transpilers.EmitDelegate(ConfigGetters.GetMaxItems);
            instruction.opcode = getter.opcode;
            instruction.operand = getter.operand;
            Plugin.PatchLog($"BuyItemsServerRpc - Transpiler success!\n[ {replacements} ] lines changed");
        }
    }

    [HarmonyPatch(typeof(Terminal), nameof(LoadNewNodeIfAffordable))]
    [HarmonyPriority(Priority.Last)]
    public class PurchaseLimitPatch2 : Terminal
    {
        static int replacements = 0;
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> LoadNewNodeIfAffordable_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            Plugin.PatchLog("LoadNewNodeIfAffordable Transpiler Initialized");
            replacements = 0;
            CodeInstruction original = new(OpCodes.Ldc_I4_S, 12);
            CodeInstruction originalfloat = new(OpCodes.Ldc_R4, 12f); //ldc.r4
            instructions.DoIf(x => x.opcode == original.opcode, ReplaceInt);
            instructions.DoIf(x => x.opcode == originalfloat.opcode, ReplaceFloat);


            return instructions;
        }

        private static void ReplaceFloat(CodeInstruction instruction)
        {
            if (!float.TryParse(instruction.operand.ToString(), out float value))
                return;

            if (value != 12f)
                return;

            CodeInstruction getter = Transpilers.EmitDelegate(ConfigGetters.GetMaxItemsFloat);
            instruction.opcode = getter.opcode;
            instruction.operand = getter.operand;
            replacements++;
            Plugin.PatchLog($"LoadNewNodeIfAffordable replaced float {value} in favor of maxitems config!\n[ {replacements} ] lines changed");
        }

        private static void ReplaceInt(CodeInstruction instruction)
        {
            if (!int.TryParse(instruction.operand.ToString(), out int value))
                return;

            if (value != 12)
                return;

            CodeInstruction getter = Transpilers.EmitDelegate(ConfigGetters.GetMaxItems);
            instruction.opcode = getter.opcode;
            instruction.operand = getter.operand;
            replacements++;
            Plugin.PatchLog($"LoadNewNodeIfAffordable replaced int {value} in favor of maxitems config!\n[ {replacements} ] lines changed");
        }
    }

    [HarmonyPatch(typeof(Terminal), nameof(SyncBoughtItemsWithServer))]
    [HarmonyPriority(Priority.Last)]
    public class PurchaseLimitPatch3 : Terminal
    {
        static int replacements = 0;
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> SyncBoughtItemsWithServer_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            Plugin.PatchLog("SyncBoughtItemsWithServer Transpiler Initialized");
            replacements = 0;
            instructions.DoIf(x => x.opcode == OpCodes.Ldc_I4_S, ReplaceInt);

            return instructions;
        }

        private static void ReplaceInt(CodeInstruction instruction)
        {
            if (!int.TryParse(instruction.operand.ToString(), out int value))
                return;

            if (value != 12)
                return;

            CodeInstruction getter = Transpilers.EmitDelegate(ConfigGetters.GetMaxItems);
            instruction.opcode = getter.opcode;
            instruction.operand = getter.operand;
            replacements++;
            Plugin.PatchLog($"SyncBoughtItemsWithServer replaced {value} in favor of maxitems config!\n[ {replacements} ] lines changed");
        }
    }

    [HarmonyPatch(typeof(Terminal), nameof(ParsePlayerSentence))]
    [HarmonyPriority(Priority.Last)]
    public class PurchaseLimitPatch4 : Terminal
    {
        static int replacements = 0;
        [HarmonyTranspiler]
        private static List<CodeInstruction> ParsePlayerSentence_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            //ldc.i4.s
            CodeInstruction getter = Transpilers.EmitDelegate(ConfigGetters.GetMaxItems);
            FieldInfo playerDefined = typeof(Terminal).GetField(nameof(Terminal.playerDefinedAmount));
            Plugin.PatchLog("ParsePlayerSentence Transpiler Initialized");
            replacements = 0;
            CodeMatcher codeMatcher = new(instructions);
            codeMatcher = codeMatcher.Start();
            Loggers.LogDebug($"codeMatcher at Start! {codeMatcher.Pos}");
            codeMatcher = codeMatcher.SearchForward(x => x.StoresField(playerDefined));
            Loggers.LogDebug($"SearchForward at playerDefined stored! {codeMatcher.Pos}");
            codeMatcher = codeMatcher.SearchBack(x => x.opcode == OpCodes.Ldc_I4_S);
            Loggers.LogDebug($"SearchBack at Float! {codeMatcher.Pos}");
            codeMatcher = codeMatcher.SetInstruction(getter);
            replacements++;
            Plugin.PatchLog($"ParsePlayerSentence patched in favor of maxitems config!\n[ {replacements} ] lines changed");
            return codeMatcher.Instructions();
        }
    }

    [HarmonyPatch(typeof(Terminal), nameof(LoadTerminalImage))]
    public class FixVideoPatch : Terminal
    {
        internal static bool VideoCheck { get; set; } = false;
        static void Postfix(TerminalNode node)
        {
            if (node.name == "darmuh's videoPlayer" && VideoCheck)
            {
                VideoManager.videoPlayerNode = node;

                if (!ViewCommands.isVideoPlaying)
                {
                    Plugin.instance.Terminal.videoPlayer.enabled = true;
                    Plugin.instance.Terminal.terminalImage.enabled = true;
                    Plugin.instance.Terminal.videoPlayer.loopPointReached += vp => OnVideoEnd();

                    Plugin.instance.Terminal.videoPlayer.Play();
                    ViewCommands.isVideoPlaying = true;
                    Loggers.LogInfo("isVideoPlaying set to TRUE");
                    VideoCheck = false;
                    return;
                }
            }
            else
            {
                bool shouldEnable = Bools.ShouldEnableImage(node);
                Loggers.LogDebug($"shouldEnable: {shouldEnable}");

                if ((bool)Plugin.instance.Terminal.displayingPersistentImage)
                {
                    // set current view to vanilla mode and disable any cameras/miniscreens
                    CamEvents.UpdateCamsEvent.Invoke(ViewCommands.ViewMode.Vanilla);
                    Loggers.LogDebug("Vanilla persistent image detected");
                }

                if (Plugin.instance.Terminal.terminalImage.enabled = shouldEnable)
                    return;

                Plugin.instance.Terminal.terminalImage.enabled = shouldEnable;
                //full screen image should always be enabled for cam views
                Loggers.LogDebug($"full screen image set to {shouldEnable}");
            }

        }

        public static void OnVideoEnd()
        {
            // This method will be called when the video is done playing
            // Disable the video player and terminal image here
            if (ViewCommands.isVideoPlaying)
            {
                Plugin.instance.Terminal.videoPlayer.enabled = false;
                Plugin.instance.Terminal.terminalImage.enabled = false;
                ViewCommands.isVideoPlaying = false;
                VideoCheck = false;
                Loggers.LogInfo("isVideoPlaying set to FALSE");
                Plugin.instance.Terminal.videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
                Plugin.instance.Terminal.videoPlayer.source = VideoSource.VideoClip;
                Plugin.instance.Terminal.videoPlayer.aspectRatio = VideoAspectRatio.FitHorizontally;
                Plugin.instance.Terminal.videoPlayer.isLooping = true;
                Plugin.instance.Terminal.videoPlayer.playOnAwake = true;

            }
        }

    }
}
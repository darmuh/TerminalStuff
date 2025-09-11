using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using TerminalStuff.CommandHandling;
using TerminalStuff.Configs;
using TerminalStuff.SpecialStuff;
using TerminalStuff.Util;
using TerminalStuff.VisualElements;
using UnityEngine;
using UnityEngine.Video;


namespace TerminalStuff.Patching;

public class AllMyTerminalPatches : MonoBehaviour
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

    [HarmonyPatch(typeof(Terminal), "ParseWord")]
    public class ConflictResolution : Terminal
    {
        static void Postfix(string playerWord, ref TerminalKeyword __result)
        {

            if (!QoLConfig.TerminalConflictResolution.Value)
                return;

            ConflictRes.InitRes(playerWord, ref __result); //should modify the keyword to whatever resolution finds as the best match
        }
    }
    [HarmonyPatch(typeof(Terminal), "TextPostProcess")]
    public class CustomReplacements
    {
        static void Postfix(ref string __result)
        {
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


    [HarmonyPatch(typeof(Terminal), "waitUntilFrameEndToSetActive")]
    public class QuitTerminalPatch : Terminal
    {
        [HarmonyPrefix]
        static void Prefix(ref bool active)
        {
            Loggers.LogDebug("waitUntilFrameEndToSetActive");

            if (EventSub.TerminalStart.alwaysOnDisplay)
            {
                Loggers.LogDebug("alwaysOnDisplay is TRUE");

                if (!MoreCommands.keepAlwaysOnDisabled)
                    active = true; //turn screen off

                //Loggers.LogDebug("End of Prefix");
                return;
            }
        }
    }

    [HarmonyPatch(typeof(Terminal), "BeginUsingTerminal")]
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

    [HarmonyPatch(typeof(Terminal), "BuyItemsServerRpc")]
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

    [HarmonyPatch(typeof(Terminal), "LoadNewNodeIfAffordable")]
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

    [HarmonyPatch(typeof(Terminal), "SyncBoughtItemsWithServer")]
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

    [HarmonyPatch(typeof(Terminal), "ParsePlayerSentence")]
    [HarmonyPriority(Priority.Last)]
    public class PurchaseLimitPatch4
    {
        static int replacements = 0;
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> ParsePlayerSentence_Transpiler(IEnumerable<CodeInstruction> instructions)
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

    [HarmonyPatch(typeof(Terminal), "LoadTerminalImage")]
    public class FixVideoPatch : Terminal
    {
        public static bool sanityCheckLOL = false;
        static void Postfix(TerminalNode node)
        {
            if (node.name == "darmuh's videoPlayer" && sanityCheckLOL)
            {
                VideoManager.videoPlayerNode = node;

                if (!ViewCommands.isVideoPlaying)
                {
                    Plugin.instance.Terminal.videoPlayer.enabled = true;
                    Plugin.instance.Terminal.terminalImage.enabled = true;
                    Plugin.instance.Terminal.videoPlayer.loopPointReached += vp => OnVideoEnd(Plugin.instance.Terminal);

                    Plugin.instance.Terminal.videoPlayer.Play();
                    ViewCommands.isVideoPlaying = true;
                    Loggers.LogInfo("isVideoPlaying set to TRUE");
                    sanityCheckLOL = false;
                    return;
                }
            }
            else
            {
                bool shouldEnable = Bools.ShouldEnableImage(node);
                Loggers.LogDebug($"shouldEnable: {shouldEnable}");

                if ((bool)Plugin.instance.Terminal.displayingPersistentImage)
                {
                    MoreCamStuff.ResetPluginInstanceBools();
                    Loggers.LogDebug("Vanilla view monitor detected, resetting plugin bools");
                }

                if (Plugin.instance.Terminal.terminalImage.enabled = shouldEnable)
                    return;

                Plugin.instance.Terminal.terminalImage.enabled = shouldEnable;
                //full screen image should always be enabled for cam views
                Loggers.LogDebug($"full screen image set to {shouldEnable}");
            }

        }

        public static void OnVideoEnd(Terminal instance)
        {
            // This method will be called when the video is done playing
            // Disable the video player and terminal image here
            if (ViewCommands.isVideoPlaying)
            {
                instance.videoPlayer.enabled = false;
                instance.terminalImage.enabled = false;
                ViewCommands.isVideoPlaying = false;
                sanityCheckLOL = false;
                Loggers.LogInfo("isVideoPlaying set to FALSE");
                instance.videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
                instance.videoPlayer.source = VideoSource.VideoClip;
                instance.videoPlayer.aspectRatio = VideoAspectRatio.FitHorizontally;
                instance.videoPlayer.isLooping = true;
                instance.videoPlayer.playOnAwake = true;

            }
        }

    }
}
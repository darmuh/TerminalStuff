using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using TerminalStuff.SpecialStuff;
using UnityEngine;
using UnityEngine.Video;


namespace TerminalStuff
{
    public class AllMyTerminalPatches : MonoBehaviour
    {
        [HarmonyPatch(typeof(Terminal), "ParseWord")]
        public class ConflictResolution : Terminal
        {
            static void Postfix(string playerWord, ref TerminalKeyword __result)
            {

                if (!ConfigSettings.TerminalConflictResolution.Value)
                    return;

                ConflictRes.InitRes(playerWord, ref __result); //should modify the keyword to whatever resolution finds as the best match
            }
        }


        [HarmonyPatch(typeof(Terminal), "waitUntilFrameEndToSetActive")]
        public class QuitTerminalPatch : Terminal
        {
            [HarmonyPrefix]
            static void Prefix(ref bool active)
            {
                Plugin.Spam("waitUntilFrameEndToSetActive");

                if (EventSub.TerminalStart.alwaysOnDisplay)
                {
                    Plugin.Spam("alwaysOnDisplay is TRUE");

                    if (!MoreCommands.keepAlwaysOnDisabled)
                        active = true; //turn screen off

                    //Plugin.Spam("End of Prefix");
                    return;
                }
            }
        }

        [HarmonyPatch(typeof(Terminal), "BeginUsingTerminal")]
        public class BeginUsingTranspiler : Terminal
        {
            [HarmonyTranspiler]
            private static IEnumerable<CodeInstruction> BeginUsingTerminal_Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                int replacements = 0;
                CodeInstruction nothing = new(OpCodes.Nop);
                foreach (CodeInstruction instruction in instructions)
                {
                    if (instruction.Calls(AccessTools.Method("Terminal:LoadNewNode")))
                    {
                        replacements++;
                        yield return nothing;
                    }
                    else
                    {
                        //Plugin.Log.LogInfo(instruction.ToString());
                        yield return instruction;
                    }

                }
                if (replacements > 0)
                    Plugin.Log.LogInfo($"BeginUsingTerminal - Transpiler success!\n [ {replacements} ] lines changed");
                else
                    Plugin.Log.LogInfo("BeginUsingTerminal - Transpiler ran with no changes");
            }
        }

        [HarmonyPatch(typeof(Terminal), "TextPostProcess")]
        public class TextPostProcessTranspiler : Terminal
        {
            [HarmonyTranspiler]
            private static IEnumerable<CodeInstruction> TextPostProcess_Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                int replacements = 0;
                CodeInstruction original = new(OpCodes.Ldstr, "\n\n\n\n\n\n\n\n\n\n\n\n\n\nn\n\n\n\n\n\n");
                CodeInstruction myFix = new(OpCodes.Ldstr, "\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n");
                foreach (CodeInstruction instruction in instructions)
                {
                    if (instruction.operand == original.operand)
                    {
                        replacements++;
                        yield return myFix;
                    }
                    else
                    {
                        yield return instruction;
                    }
                }

                if (replacements > 0)
                    Plugin.Log.LogInfo($"TextPostProcess - Transpiler success!\n [ {replacements} ] lines changed");
                else
                    Plugin.Log.LogInfo("TextPostProcess - Transpiler ran with no changes");
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
                        Plugin.MoreLogs("isVideoPlaying set to TRUE");
                        sanityCheckLOL = false;
                        return;
                    }
                }
                else
                {
                    bool shouldEnable = BoolStuff.ShouldEnableImage(node);
                    Plugin.Spam($"shouldEnable: {shouldEnable}");

                    if ((bool)Plugin.instance.Terminal.displayingPersistentImage)
                    {
                        MoreCamStuff.ResetPluginInstanceBools();
                        Plugin.Spam("Vanilla view monitor detected, resetting plugin bools");
                    }

                    if (Plugin.instance.Terminal.terminalImage.enabled = shouldEnable)
                        return;

                    Plugin.instance.Terminal.terminalImage.enabled = shouldEnable;
                    //full screen image should always be enabled for cam views
                    Plugin.Spam($"full screen image set to {shouldEnable}");
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
                    Plugin.MoreLogs("isVideoPlaying set to FALSE");
                    instance.videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
                    instance.videoPlayer.source = VideoSource.VideoClip;
                    instance.videoPlayer.aspectRatio = VideoAspectRatio.FitHorizontally;
                    instance.videoPlayer.isLooping = true;
                    instance.videoPlayer.playOnAwake = true;

                }
            }

        }
    }
}
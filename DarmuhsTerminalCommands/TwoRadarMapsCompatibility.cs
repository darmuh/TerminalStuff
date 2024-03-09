using TwoRadarMaps;
using System;
using UnityEngine;
using static TerminalStuff.ViewCommands;
using static TwoRadarMaps.Plugin;
using Object = UnityEngine.Object;
using System.Collections.Generic;
using GameNetcodeStuff;
using OpenBodyCams.API;
using OpenBodyCams;

namespace TerminalStuff
{
    internal class TwoRadarMapsCompatibility
    {

        internal static Texture RadarCamTexture()
        {
            Plugin.MoreLogs("Getting Radar texture for Zaggy's Unique Radar from TwoRadarMaps");
            Texture ZaggyRadarTexture = TerminalMapRenderer.cam.targetTexture;
            return ZaggyRadarTexture;
        }

        internal static String TargetedPlayerOnSecondRadar()
        {
            Plugin.MoreLogs($"Getting playername from TerminalMapScreenPlayerName.text which is {TerminalMapScreenPlayerName.text}");
            string playerName = TerminalMapScreenPlayerName.text.Replace("MONITORING: ", "");
            return playerName;
        }

        internal static string TeleportCompatibility()
        {
            if (TerminalMapRenderer.targetedPlayer != null)
            {
                TeleportTarget(TerminalMapRenderer.targetTransformIndex);
                Plugin.MoreLogs("Valid player attached to tworadarmaps, teleporting");
                string displayText = $"{ConfigSettings.tpMessageString.Value} (Targeted Player: {TerminalMapRenderer.targetedPlayer.playerUsername})";
                return displayText;
            }
            else
            {
                Plugin.MoreLogs("Not monitoring a valid player");
                string displayText = "Unable to teleport target.";
                return displayText;
            }
        }

        internal static void UpdateTerminalRadarTarget(Terminal terminal, int playerNum = -1)
        {
            if (playerNum == -1)
            {
                int next = GetNextValidTarget(TerminalMapRenderer.radarTargets, TerminalMapRenderer.targetTransformIndex);
                StartTargetTransition(TerminalMapRenderer, next);
                Plugin.MoreLogs("Setting to next player");
                StartofHandling.DelayedUpdateText(terminal);
            }
            else if(playerNum == -2)
            {
                int prev = GetPrevValidTarget(TerminalMapRenderer.radarTargets, TerminalMapRenderer.targetTransformIndex);
                StartTargetTransition(TerminalMapRenderer, prev);
                Plugin.MoreLogs("Setting to next player");
                StartofHandling.DelayedUpdateText(terminal);
            }
            else
            {
                StartTargetTransition(TerminalMapRenderer, playerNum);
                Plugin.MoreLogs("Setting to specific player");
                StartofHandling.DelayedUpdateText(terminal);
            }   
        }

        internal static int CheckForPlayerNameCommand(string firstWord, string secondWord) //vanilla function modified for use with TwoRadarMaps
        {

            if (secondWord.Length <= 2)
            {
                return -1;
            }

            Plugin.MoreLogs("first word: " + firstWord + "; second word: " + secondWord);
            List<string> list = new List<string>();
            for (int i = 0; i < TerminalMapRenderer.radarTargets.Count; i++) //swapped out vanilla mapscreen for TwoRadarMaps'
            {
                if (TargetIsValid(TerminalMapRenderer.radarTargets[i]?.transform))
                {
                    list.Add(TerminalMapRenderer.radarTargets[i].name); //added this to only get valid targets
                    Plugin.MoreLogs($"name {i}: {list[i]}");
                }
                else
                    list.Add(string.Empty); //added this to keep same list length
            }

            secondWord = secondWord.ToLower();
            for (int j = 0; j < list.Count; j++)
            {
                string text = list[j].ToLower();
                if (text == secondWord)
                {
                    return j;
                }
            }

            Plugin.MoreLogs($"Target names length: {list.Count}");
            for (int k = 0; k < list.Count; k++)
            {
                string text = list[k].ToLower();
                Plugin.MoreLogs($"Word #{k}: {text}; length: {text.Length}");
                for (int num = secondWord.Length; num > 2; num--)
                {
                    Plugin.MoreLogs($"c: {num}");
                    Plugin.MoreLogs(secondWord.Substring(0, num));
                    if (text.StartsWith(secondWord.Substring(0, num)))
                    {
                        return k;
                    }
                }
            }
            return -1;
        }


        //copied the below methods as they are not available to be referenced from external sources
        //There is a public method that uses the below methods but it will update BOTH the real radar and the terminal radar
        //I needed to use a method that will only update the terminalmap

        internal static void StartTargetTransition(ManualCameraRenderer mapRenderer, int targetIndex) //copied from TwoRadarMaps, no changes
        {
            if (mapRenderer.updateMapCameraCoroutine != null)
            {
                mapRenderer.StopCoroutine(mapRenderer.updateMapCameraCoroutine);
            }

            mapRenderer.updateMapCameraCoroutine = mapRenderer.StartCoroutine(mapRenderer.updateMapTarget(targetIndex));
        }

        //moved GetNextValidTarget and TargetIsValid to ViewCommands
        //They are useful for more than just TwoRadarsCompatibility and will be used for other functions

    }
}

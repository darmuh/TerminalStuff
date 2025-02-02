using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TerminalStuff.Configs;
using UnityEngine;
using static TerminalStuff.StringStuff;

namespace TerminalStuff.SpecialStuff.Keywords
{
    internal static class HandleDelayRun
    {
        internal static List<DelayRun> DelayRuns = [];

        internal static string HandleCommandDelay()
        {
            List<string> keywords = GetKeywordsPerConfigItem(KeywordConfigs.DelayKWs.Value);
            string failText = $"Invalid usage of {keywords[0]} command! Please follow the following format:\r\n\r\n\t{keywords[0]} <time> <command>\r\n\r\n";
            string command = GetAfterKeyword(keywords);
            if (command.Length < 1)
            {
                Plugin.Spam("No given words after keyword!");
                return failText;
            }
                
            else
            {
                string[] words = command.Split([' '], System.StringSplitOptions.RemoveEmptyEntries);
                if (words.Length < 2)
                {
                    Plugin.Spam($"words count is not enough! [ {words.Length} ]");
                    return failText;
                }

                Plugin.Spam($"words length - {words.Length}");
                string givenCommand = string.Join(" ", words, 1, words.Length -1);

                if (!int.TryParse(words[0], out int delay))
                {
                    Plugin.Spam($"Unable to parse value from [ {words[0]} ]");
                    return failText;
                }    
                else
                {
                    DelayRun delayRun = new(givenCommand, delay);
                    Plugin.instance.Terminal.StartCoroutine(delayRun.CommandRunner());
                    return $"You have initiated a delayed command of {givenCommand} which will run in {delay} seconds...\r\n\r\n";
                }
            }
        }

        internal static string StopCommandDelay()
        {
            List<string> keywords = GetKeywordsPerConfigItem(KeywordConfigs.StopDelayKWs.Value);
            string failText = $"Invalid usage of {keywords[0]} command! Please follow the following format:\r\n\r\n\t{keywords[0]} <time> <command>\r\n\r\n";
            string command = GetAfterKeyword(keywords);
            if (command.Length < 1)
                return failText;
            else
            {
                string[] words = command.Split([' '], System.StringSplitOptions.RemoveEmptyEntries);
                if (words.Length < 2)
                    return failText;

                string givenCommand = string.Join(" ", words, 1, words.Length - 1);

                if (!int.TryParse(words[0], out int delay))
                    return failText;
                else
                {
                    DelayRun delayRun = DelayRuns.FirstOrDefault(x=> x.command == givenCommand && x.delay == delay);

                    if (delayRun != null)
                    {
                        delayRun.enabled = false;
                        DelayRuns.Remove(delayRun);
                        return $"You have canceled the command \"{delayRun.command}\" from running after {delayRun.delay} seconds!";
                    }
                    else
                        return $"Unable to find active command {givenCommand} set to run after {delay} seconds!";
                }
            }
        }
    }

    internal class DelayRun
    {
        internal string command;
        internal int delay;
        internal bool enabled = true;

        internal DelayRun(string words, int time)
        {
            command = words;
            delay = time;
            HandleDelayRun.DelayRuns.Add(this);
        }

        internal IEnumerator CommandRunner()
        {
            bool shipStatus = StartOfRound.Instance.inShipPhase;
            yield return new WaitForSeconds(delay);
            
            if (shipStatus != StartOfRound.Instance.inShipPhase) //do not run previously defined command
            {
                Plugin.MoreLogs($"{command} delayed by {delay} seconds canceled due to detected ship phase change!");
                yield break;
            }

            if (Plugin.instance.Terminal.placeableObject.inUse)
            {
                Plugin.MoreLogs($"{command} delayed by {delay} seconds canceled due to terminal in use at this moment!");
                yield break;
            }

            if (!enabled)
            {
                Plugin.MoreLogs($"{command} delayed by {delay} seconds canceled due to manual disable!");
                yield break;
            }

            OpenLib.CoreMethods.LogicHandling.SetTerminalInput(command);
            TerminalNode newNode = Plugin.instance.Terminal.ParsePlayerSentence();
            TerminalEvents.LoadAndSync(newNode);
            HandleDelayRun.DelayRuns.Remove(this);

        }
    }
}

using System.Collections.Generic;
using TerminalStuff.Configs;
using TerminalStuff.EventSub;
using TerminalStuff.PluginCore;

namespace TerminalStuff;

internal class AutoComplete
{
    internal static int AutoCompleteIndex = -1;
    internal static List<string> AutoCompleteResults = [];
    internal static List<string> GetMatchingKeywords(string input)
    {
        List<string> matching = [];
        matching.Add(input);
        Loggers.LogDebug($"added {input} to automcomplete list");
        foreach (TerminalKeyword word in Plugin.instance.Terminal.terminalNodes.allKeywords)
        {
            if (OpenLib.Common.Misc.StringContainsInvariant(word.word, input))
            {
                matching.Add(word.word);
                Loggers.LogDebug($"adding matching word: {word.word} to autocomplete list");
            }
        }

        if (GameStuff.otherModWords.Count > 0)
        {
            foreach (string modword in GameStuff.otherModWords)
            {
                if (OpenLib.Common.Misc.StringContainsInvariant(modword, input))
                {
                    matching.Add(modword);
                    Loggers.LogDebug($"adding otherModWord: {modword} to autocomplete list");
                }
            }
        }

        return matching;
    }

    internal static bool CheckCurrentInput(List<string> matchList, string input)
    {
        if (!matchList.Contains(input))
        {
            AutoCompleteIndex = -1;
            return false;
        }

        else
            return true;
    }

    internal static string ShowMatchingKeywords(List<string> matchList, ref int currentIndex)
    {
        Loggers.LogDebug($"Matching Words: {matchList.Count}");
        if (matchList.Count < 1)
        {
            Loggers.LogDebug("matchList count is 0");
            currentIndex = 0;
            return string.Empty;
        }
        else if (matchList.Count > QoLConfig.TerminalAutoCompleteMaxCount.Value)
        {
            Loggers.LogDebug("matchList count is too high");
            currentIndex = -1;
            return matchList[0].ToString();
        }

        if (currentIndex == -1)
            currentIndex = 1;

        if (currentIndex > matchList.Count - 1)
        {
            Loggers.LogDebug("setting autocompleteindex to 0");
            currentIndex = -1;
            return matchList[0].ToString();
        }

        string command = matchList[currentIndex].ToString();
        currentIndex++;
        return command;

    }
}

using System.Collections.Generic;
using TerminalStuff.Configs;
using TerminalStuff.Util;

namespace TerminalStuff.SpecialStuff;

internal class TerminalHistory
{
    internal static List<string> CommandHistory = [];
    internal static int historyIndex = 0;

    internal static void AddToCommandHistory(string command)
    {
        if (!QoLConfig.TerminalHistory.Value)
            return;

        if (CommandHistory.Count < QoLConfig.TerminalHistoryMaxCount.Value)
            CommandHistory.Add(command);
        else
        {
            CommandHistory.RemoveAt(0);
            CommandHistory.Add(command);
        }

        Loggers.LogDebug($"Added {command} to CommandHistory List");
    }

    internal static int PreviousIndex()
    {
        if (CommandHistory.Count < 1)
        {
            Loggers.LogDebug("CommandHistory.Count < 1");
            historyIndex = 0;
            return historyIndex;
        }

        if (historyIndex == 0)
            historyIndex = CommandHistory.Count - 1;
        else
            historyIndex--;

        return historyIndex;
    }

    internal static int NextIndex()
    {
        if (CommandHistory.Count < 1)
        {
            Loggers.LogDebug("CommandHistory.Count < 1");
            historyIndex = 0;
            return historyIndex;
        }

        if (historyIndex == CommandHistory.Count - 1)
            historyIndex = 0;
        else
            historyIndex++;

        return historyIndex;
    }

    internal static string GetFromCommandHistory(ref int currentIndex)
    {

        string command = string.Empty;
        if (CommandHistory.Count < 1)
            return command;
        else
        {
            if (CommandHistory.Count > currentIndex)
                command = CommandHistory[currentIndex];
            else
            {
                command = CommandHistory[0];
                currentIndex = 0;
            }
            return command;
        }
    }


}

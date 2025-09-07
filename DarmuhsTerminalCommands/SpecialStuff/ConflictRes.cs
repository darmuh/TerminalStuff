using System.Collections.Generic;
using TerminalStuff.Configs;
using TerminalStuff.Util;


namespace TerminalStuff.SpecialStuff;


internal class ConflictRes
{
    readonly TerminalKeyword word;
    readonly int distance;
    readonly int bonus;
    readonly int priority;

    internal ConflictRes(TerminalKeyword word, int distance, int bonus, string verb = "none")
    {
        this.word = word;
        this.distance = distance;
        this.bonus = bonus;
        if (verb == "route")
            priority = 1;
        else if (verb == "buy")
            priority = 2;
        else
            priority = 0;

        Loggers.LogDebug($"{word.word} registered.\ndistance: {distance}\nbonus: {bonus}\nverb: {verb}");
    }

    internal static bool ContainsWord(List<ConflictRes> conflictList, TerminalKeyword query)
    {
        foreach (ConflictRes word in conflictList)
        {
            if (word.word == query)
                return true;
        }
        return false;

    }

    internal static void InitRes(string playerWord, ref TerminalKeyword word)
    {
        if (word == null || !QoLConfig.TerminalConflictResolution.Value)
            return;

        if (playerWord.Length < 0)
            return;

        if (TryGetBestMatchingKeyword(playerWord, out TerminalKeyword returnWord))
        {
            word = returnWord;
            Loggers.LogDebug("returning conflictresolution word");
        }
    }

    internal static void QueryKeywords(ref List<ConflictRes> matching, string input)
    {
        List<string> highPriorityVerbs = ["buy", "route"];
        foreach (TerminalKeyword word in Plugin.instance.Terminal.terminalNodes.allKeywords)
        {
            if (OpenLib.Common.Misc.StringContainsInvariant(word.word, input) && !ContainsWord(matching, word))
            {
                if (word.word.Length < 2)
                    continue;
                int bonus = Levenshtein.MatchingStart(word.word, input);
                int score = Levenshtein.Distance(word.word, input);

                Loggers.LogDebug($"Word {word.word} noted with score {score} for input {input} with bonus {bonus}");
                if (word.defaultVerb == null)
                {
                    matching.Add(new(word, score, bonus));
                    continue;
                }

                if (highPriorityVerbs.Contains(word.defaultVerb.word.ToLower()))
                {
                    Loggers.LogDebug($"Word - {word.word} given high priority attribute due to matching high priority verb: {word.defaultVerb.word}");
                    matching.Add(new(word, score, bonus, word.defaultVerb.word.ToLower()));
                }
                else
                    matching.Add(new(word, score, bonus));
            }
            else if (word.compatibleNouns != null)
            {
                foreach (CompatibleNoun noun in word.compatibleNouns)
                {
                    if (input.Contains(noun.noun.word) && !ContainsWord(matching, noun.noun))
                    {
                        if (noun.noun.word.Length < 3 || noun.noun.defaultVerb == null)
                            continue;
                        Loggers.LogDebug($"{word.word} has compatible noun: {noun.noun.word} which is in {input}");
                        int bonus = Levenshtein.MatchingStart(noun.noun.word, input);
                        int score = Levenshtein.Distance(noun.noun.word, input);
                        Loggers.LogDebug($"score: {score}");

                        if (highPriorityVerbs.Contains(noun.noun.defaultVerb.word.ToLower()))
                        {
                            Loggers.LogDebug($"Word - {noun.noun.word} given high priority attribute due to matching high priority verb: {noun.noun.defaultVerb.word}");
                            matching.Add(new(noun.noun, score, bonus, noun.noun.defaultVerb.word.ToLower()));
                        }
                        else
                            matching.Add(new(noun.noun, score, bonus));
                    }
                }
            }
        }
    }

    internal static bool TryGetBestMatchingKeyword(string query, out TerminalKeyword returnWord)
    {
        List<ConflictRes> resolutionList = [];
        List<ConflictRes> highPri = [];

        QueryKeywords(ref resolutionList, query);

        if (resolutionList.Count < 1)
        {
            Plugin.Log.LogWarning($"No matching keywords could be found for input: {query}");
            returnWord = null!;
            return false;
        }

        foreach (ConflictRes resolution in resolutionList)
        {
            if (resolution.priority > 0)
                highPri.Add(resolution);
        }

        ConflictRes bestMatch = null!;

        if (highPri.Count > 0)
        {
            GetResolution(highPri, ref bestMatch);
        }
        else
        {
            GetResolution(resolutionList, ref bestMatch);
        }

        if (bestMatch == null)
        {
            returnWord = null!;
            return false;
        }

        if (bestMatch.word == null)
        {
            Plugin.Log.LogWarning($"No matching keywords found for input: {query}");
            returnWord = null!;
            return false;
        }
        else
        {
            Loggers.LogDebug($"Conflict resolution has determined word: {bestMatch.word.word} is the best match with distance: {bestMatch.distance} and starswith bonus: {bestMatch.bonus}");
            returnWord = bestMatch.word;
            return true;
        }

    }

    internal static bool TryGetHighestPriority(List<ConflictRes> resolutionList, ConflictRes currentRes)
    {
        bool isHighest = false;
        foreach (ConflictRes resolution in resolutionList)
        {
            if (currentRes.priority >= resolution.priority)
                isHighest = true;
        }

        return isHighest;
    }

    internal static void GetResolution(List<ConflictRes> resolutionList, ref ConflictRes bestMatch)
    {
        int highestBonus = 0;

        foreach (ConflictRes resolution in resolutionList)
        {
            if (resolution.bonus > highestBonus && TryGetHighestPriority(resolutionList, resolution))
            {
                Loggers.LogDebug($"Highest priority: {resolution.priority}");
                bestMatch = resolution;
                highestBonus = resolution.bonus;
                Loggers.LogDebug($"highestBonus updated to {highestBonus} - from {bestMatch.word}");
            }
            else if (resolution.bonus == highestBonus && bestMatch != null)
            {
                Loggers.LogDebug($"matching highestBonus found for word {resolution.word.word}");
                if (resolution.distance < bestMatch.distance && resolution.priority >= bestMatch.priority)
                {
                    Loggers.LogDebug($"distance for {resolution.word.word} is lower than {bestMatch.word.word}");
                    Loggers.LogDebug($"setting bestMatch to {resolution.word.word}");
                    bestMatch = resolution;
                }
            }
        }
    }
}

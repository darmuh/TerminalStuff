//https://github.com/DanHarltey/Fastenshtein/blob/master/src/Fastenshtein/StaticLevenshtein.cs
//algorithm used for comparing string diff distance
//has been tweaked slightly for use in LethalCompany

using System.Text.RegularExpressions;

namespace TerminalStuff.SpecialStuff
{
    /// <summary>
    /// Measures the difference between two strings.
    /// Uses the Levenshtein string difference algorithm.
    /// </summary>
    public partial class Levenshtein
    {
        /// <summary>
        /// Compares the two values to find the minimum Levenshtein distance. 
        /// Thread safe.
        /// </summary>
        /// <returns>Difference. 0 complete match.</returns>
        public static int Distance(string value1, string value2)
        {
            value1 = Regex.Replace(value1, @"\s", "");
            value2 = Regex.Replace(value2, @"\s", ""); //remove empty space in strings for only alphanumeric comparison

            if (value2.Length == 0)
            {
                return value1.Length;
            }

            int[] costs = new int[value2.Length];

            // Add indexing for insertion to first row
            for (int i = 0; i < costs.Length;)
            {
                costs[i] = ++i;
            }

            for (int i = 0; i < value1.Length; i++)
            {
                // cost of the first index
                int cost = i;
                int previousCost = i;

                // cache value for inner loop to avoid index lookup and bonds checking, profiled this is quicker
                char value1Char = value1[i];

                for (int j = 0; j < value2.Length; j++)
                {
                    int currentCost = cost;

                    // assigning this here reduces the array reads we do, improvement of the old version
                    cost = costs[j];

                    if (value1Char != value2[j])
                    {
                        if (previousCost < currentCost)
                        {
                            currentCost = previousCost;
                        }

                        if (cost < currentCost)
                        {
                            currentCost = cost;
                        }

                        ++currentCost;
                    }

                    /* 
                        * Improvement on the older versions.
                        * Swapping the variables here results in a performance improvement for modern intel CPU’s, but I have no idea why?
                        */

                    costs[j] = currentCost;
                    previousCost = currentCost;
                }
            }

            int score = costs[costs.Length - 1];

            return score;
        }

        public static int MatchingStart(string value1, string value2)
        {
            int bonus = 0; //distance with bonus modifier for characters that start the same

            for (int i = 0; i < value2.Length; i++)
            {
                if (i > value1.Length - 1)
                    break; //catch query being longer than word
                if (value2.Substring(i, 1).Equals(value1.Substring(i, 1)))
                {
                    Plugin.Spam($"({i + 1}x multiplier) - matching start character, adding to bonus modifier");
                    bonus++;
                    Plugin.Spam($"bonus is now {bonus}");
                }
                else
                    break;
            }
            return bonus;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace TerminalStuff
{
    internal class StringStuff
    {

        internal static string[] GetWords()
        {
            string cleanedText = Plugin.instance.Terminal.screenText.text.Substring(Plugin.instance.Terminal.screenText.text.Length - Plugin.instance.Terminal.textAdded);
            string[] words = cleanedText.Split([' '], StringSplitOptions.RemoveEmptyEntries);
            return words;
        }

        internal static string GetAfterKeyword(List<string> keywords)
        {
            string cleanedText = Plugin.instance.Terminal.screenText.text.Substring(Plugin.instance.Terminal.screenText.text.Length - Plugin.instance.Terminal.textAdded);
            foreach (string item in keywords)
            {
                if (cleanedText.StartsWith(item, true, null))
                {
                    string val = cleanedText.Replace(item, "");
                    return val;
                }
            }

            return "";
        }

        internal static List<string> GetKeywordsPerConfigItem(string configItem)
        {
            List<string> keywordsInConfig = [];
            if (configItem.Length > 0)
            {
                keywordsInConfig = configItem.Split(';')
                                      .Select(item => item.TrimStart())
                                      .ToList();
                //Plugin.MoreLogs("GetKeywordsPerConfigItem split complete");
            }

            return keywordsInConfig;
        }

        internal static List<int> GetNumberListFromStringList(List<string> stringList)
        {
            List<int> numbersList = [];
            foreach (string item in stringList)
            {
                if (int.TryParse(item, out int number))
                {
                    numbersList.Add(number);
                }
                else
                    Plugin.WARNING($"Could not parse {item} to integer");
            }

            return numbersList;
        }

        internal static List<string> GetItemList(string rawList)
        {
            List<string> itemList = [];
            if (rawList.Length > 0)
            {
                itemList = rawList.Split(',')
                                      .Select(item => item.TrimStart())
                                      .ToList();
            }

            return itemList;
        }

        internal static Dictionary<string, string> GetKeywordAndItemNames(string configItem)
        {
            Dictionary<string, string> KeywordAndNames = [];
            if (configItem.Length > 0)
            {
                KeywordAndNames = configItem
                   .Split(';')
                   .Select(item => item.Trim())
                   .Select(item => item.Split(':'))
                   .ToDictionary(pair => pair[0].Trim(), pair => pair[1].Trim());
            }

            return KeywordAndNames;
        }

        internal static List<string> GetListToLower(List<string> stringList)
        {
            List<string> itemsToLower = [];

            foreach (string item in stringList)
            {
                itemsToLower.Add(item.ToLower());
            }
            return itemsToLower;
        }

    }
}

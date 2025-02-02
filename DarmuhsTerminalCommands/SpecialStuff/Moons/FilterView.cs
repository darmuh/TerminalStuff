using System.Collections.Generic;
using System.Linq;
using System;

namespace TerminalStuff.SpecialStuff
{
    internal class FilterView
    {
        internal bool Weather = true;
        internal bool Price = true;
        internal bool Difficulty = false;
        internal bool RemoveBadWeather = false;
        internal bool RemoveTooExpensive = false;

        internal string Sorting = "LevelID";
        internal int SortType = 0;

        internal void SetDefaults(string config)
        {
            Weather = false;
            Price = false;
            Difficulty = false;

            if (config.Length < 1)
                return;

            List<string> entries = [.. config.Split(',')];
            foreach (string entry in entries)
            {
                entry.Trim();

                if (entry == "weather")
                    Weather = true;

                if(entry == "price")
                    Price = true;

                if(entry == "difficulty")
                    Difficulty = true;
            }
        }

        internal void GetMoonsToDisplay(ref int activeIndex)
        {
            MoonOnTopCheck();
            MoonsPlus.MoonsDisplayed = MoonsPlus.MoonListing.FindAll(x => x.ShowInListing());
            ResolveHideList(ref MoonsPlus.MoonsDisplayed);

            List<MoonInfo> FilteredWeather = [];

            if (RemoveBadWeather && MoonsPlus.AcceptableWeathers.Count > 0)
            {
                
                foreach(MoonInfo moon in MoonsPlus.MoonsDisplayed)
                {
                    string currentWeather = MoonsPlus.GetWeatherName(moon.Level);
                    Plugin.Spam($"Checking {moon.LevelName} weather - {currentWeather}");
                    if (currentWeather.Length < 1)
                        FilteredWeather.Add(moon);
                    else if (MoonsPlus.AcceptableWeathers.Any(x => currentWeather.IndexOf(x, StringComparison.InvariantCultureIgnoreCase) != -1))
                        FilteredWeather.Add(moon);
                    else
                        Plugin.Spam($"Not displaying {moon.LevelName}");
                }

                if(FilteredWeather.Count > 0)
                    MoonsPlus.MoonsDisplayed = FilteredWeather;
            }
                
            if (RemoveTooExpensive)
                MoonsPlus.MoonsDisplayed = [.. MoonsPlus.MoonsDisplayed.FindAll(x => x.DisplayPrice <= Plugin.instance.Terminal.groupCredits)];

            MoonsPlus.UpdateMoonsDisplayed.Invoke(MoonsPlus.MoonsDisplayed); //should allow for external mods to filter as needed

        }

        internal static void MoonOnTopCheck()
        {
            if (MoonsPlusConfig.ThisAlwaysOnTop.Value.Length < 1)
                return;

            MoonInfo thismoon = MoonsPlus.MoonListing.Find(x => x.LevelName.ToLower() == MoonsPlusConfig.ThisAlwaysOnTop.Value.ToLower());
            if (thismoon == null)
                return;

            int thisIndex = MoonsPlus.MoonListing.IndexOf(thismoon);
            Plugin.Spam($"{MoonsPlusConfig.ThisAlwaysOnTop.Value} = {thisIndex}");
            if (thisIndex > 0)
            {
                MoonsPlus.MoonListing.Remove(thismoon);
                MoonsPlus.MoonListing.Insert(0, thismoon);

                Plugin.Spam($"{MoonsPlusConfig.ThisAlwaysOnTop.Value} is now first in list!");
            }
        }

        internal static void ResolveHideList(ref List<MoonInfo> moonsList)
        {
            if (MoonsPlusConfig.AlwaysHideList.Value.Length < 1)
                return;

            List<MoonInfo> HideList = [];

            Plugin.MoreLogs("AlwaysHideList Resolution starting...");
            List<string> moons = OpenLib.Common.CommonStringStuff.GetKeywordsPerConfigItem(MoonsPlusConfig.AlwaysHideList.Value, ',');
            Plugin.MoreLogs($"AlwaysHideList Count: {moons.Count}");
            bool hidecompany = moons.Any(x => x.ToLower() == "thismoon");
            foreach (MoonInfo moon in moonsList)
            {
                Plugin.MoreLogs($"Checking Moon: {moon.LevelName}");
                if (moons.Any(x => x.ToLower() == moon.LevelName.ToLower()))
                {
                    Plugin.MoreLogs($"Matching moon name found! Hiding {moon.LevelName}");
                    HideList.Add(moon);
                }
                else if(hidecompany && moon.IsCompany)
                {
                    Plugin.MoreLogs($"Matching moon name found! Hiding {moon.LevelName} (Company)");
                    HideList.Add(moon);
                }
                else
                    Plugin.MoreLogs($"{moon.LevelName} does not match any configuration entries for AlwaysHideList");

            }

            moonsList.RemoveAll(x => HideList.Contains(x));
        }

        internal void AssignSorting(string config)
        {
            if (config == "id")
            {
                SortType = 0;
                Sorting = "LevelID";
            }
            else if (config == "alphabetical")
            {
                SortType = 1;
                Sorting = "Alphabetical";
            }
            else if (config == "price")
            {
                SortType = 2;
                Sorting = "Price";
            }
            else if (config == "weather")
            {
                SortType = 3;
                Sorting = "Weather";
            }
            else if (config == "difficulty")
            {
                SortType = 4;
                Sorting = "Difficulty";
            }
            else
                Plugin.WARNING("Failed to assign DefaultSorting value from config!");
        }

        internal void ChangeSort(int type)
        {
            if (type == 0)
            {
                SortType = type;
                Sorting = "LevelID";
                MoonsPlus.MoonListing = [.. MoonsPlus.MoonListing.OrderBy(x => x.Level.levelID)];
            }
            else if (type == 1)
            {
                SortType = type;
                Sorting = "Alphabetical";
                MoonsPlus.MoonListing = [.. MoonsPlus.MoonListing.OrderBy(x => x.LevelName)];
            }
            else if (type == 2)
            {
                SortType = type;
                Sorting = "Price";
                MoonsPlus.MoonListing = [.. MoonsPlus.MoonListing.OrderBy(x => x.DisplayPrice)];
            }
            else if (type == 3)
            {
                SortType = type;
                Sorting = "Weather";
                MoonsPlus.MoonListing = [.. MoonsPlus.MoonListing.OrderBy(x => x.Level.currentWeather)];
            }
            else if (type == 4)
            {
                SortType = type;
                Sorting = "Difficulty";
                MoonsPlus.MoonListing = [.. MoonsPlus.MoonListing.OrderBy(x => x.Level.riskLevel)];
            }
        }

    }
}

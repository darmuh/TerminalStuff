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

        internal void GetMoonsToDisplay()
        {
            MoonsPlus.MoonsDisplayed = MoonsPlus.MoonListing.FindAll(x => x.ShowInListing());
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
                MoonsPlus.MoonsDisplayed = [.. MoonsPlus.MoonsDisplayed.FindAll(x => x.price <= Plugin.instance.Terminal.groupCredits)];

            MoonsPlus.UpdateMoonsDisplayed.Invoke(MoonsPlus.MoonsDisplayed); //should allow for external mods to filter as needed

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
                MoonsPlus.MoonListing = [.. MoonsPlus.MoonListing.OrderBy(x => x.price)];
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


            for (int i = 0; i < MoonsPlus.MoonListing.Count; i++)
                MoonsPlus.MoonListing[i].indexNum = i;
        }

    }
}

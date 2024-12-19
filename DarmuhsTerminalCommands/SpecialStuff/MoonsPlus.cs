using Key = UnityEngine.InputSystem.Key;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Collections;
using TerminalStuff.EventSub;
using TerminalStuff.PluginCore;
using static TerminalStuff.EventSub.TerminalStart;
using static OpenLib.ConfigManager.ConfigSetup;
using static OpenLib.CoreMethods.AddingThings;
using OpenLib.CoreMethods;
using UnityEngine.InputSystem;
using System.Linq;
using TerminalStuff.Compatibility;

namespace TerminalStuff.SpecialStuff
{
    public class MoonsPlus
    {
        public static OpenLib.Events.Events.CustomEvent<List<MoonInfo>> UpdateMoonsDisplayed = new();

        internal static List<MoonInfo> MoonListing = [];
        public static List<MoonInfo> MoonsDisplayed = [];
        internal static List<MoonInfo> MoonsUnhidden = [];
        internal static List<string> AcceptableWeathers = [];
        internal static List<string> FilterMenu = [];
        internal static List<string> MoonsVisited = [];

        //menu stuff
        internal static int activeSelection = 0;
        internal static int currentPage = 1;
        internal static bool inMoonsMenu = false;
        internal static bool inFilterMenu = false;
        internal static TerminalNode MoonsMenu = null!;
        internal static FilterView MoonsFilter = new();


        internal static Color transparent = new(0, 0, 0, 0);

        internal static void SetToVanilla()
        {
            if (OriginalMoonsPage == null)
                return;

            if (DynamicBools.TryGetKeyword("moons", out TerminalKeyword Moons))
            {
                Moons.specialKeywordResult = OriginalMoonsPage;
                Plugin.Spam("Moons keyword set back to original");
            }
        }

        internal static void GetFilters()
        {
            FilterMenu.Clear();
            FilterMenu.Insert(0, $"Show Weather for Moon");
            FilterMenu.Insert(1, $"Show Price for Moon:");
            FilterMenu.Insert(2, $"Show Difficulty for Moon:");
            FilterMenu.Insert(3, $"Sort by LevelID:");
            FilterMenu.Insert(4, $"Sort Alphabetically:");
            FilterMenu.Insert(5, $"Sort by Price:");
            FilterMenu.Insert(6, $"Sort by Weather:");
            FilterMenu.Insert(7, $"Sort by Difficulty:");
            FilterMenu.Insert(8, $"[Filter by Affordability]");
            FilterMenu.Insert(9, $"[Filter by Weather]");

        }

        internal static void GetMoons()
        {
            if (!ConfigSettings.TerminalMoonsPlus.Value)
            {
                SetToVanilla();
                return;
            }
                
            if (StartOfRound.Instance.levels.Length == 0) 
                return;

            MoonListing.Clear();
            MoonsDisplayed.Clear();

            for(int i = 0; i < StartOfRound.Instance.levels.Length; i++)
            {
                if (StartOfRound.Instance.levels[i].name == "LiquidationLevel")
                    continue;

                MoonInfo moon = new(StartOfRound.Instance.levels[i], i, StartOfRound.Instance.levels[i].PlanetName, StartOfRound.Instance.levels[i] == StartOfRound.Instance.currentLevel);
                MoonListing.Add(moon);
            }

            MoonsFilter.ChangeSort(MoonsFilter.SortType);

            MoonsDisplayed = MoonListing.FindAll(x => x.ShowInListing());

            GetFilters();
            SaveManager.InitMoonPlusSave();

            if (!MoonsPlusConfig.MoonsPlusKeywords.Value.Split(';').Any(x=>x.Trim().ToLower() == "moons"))
            {
                MoonsMenu = AddNodeManual("MoonsPlus", MoonsPlusConfig.MoonsPlusKeywords, EnterMoonsMenu, true, 0, ConfigSettings.TerminalStuffMain, defaultManaged, "EXTRAS", "Open the Moons Plus Page");
                return;
            }

            if (DynamicBools.TryGetKeyword("moons", out TerminalKeyword Moons))
            {
                MoonsMenu = AddNodeManual("MoonsPlus", MoonsPlusConfig.MoonsPlusKeywords, EnterMoonsMenu, true, 0, ConfigSettings.TerminalStuffMain, defaultManaged, "EXTRAS", "Open the Moons Plus Page");
                
                OriginalMoonsPage = Moons.specialKeywordResult;
                MoonsMenu.displayText = OriginalMoonsPage.displayText;
                Moons.specialKeywordResult = MoonsMenu;
                Plugin.Log.LogMessage("Moons page replaced with MoonsPlus page!");
            }
            else
                Plugin.ERROR("UNABLE TO GET MOONS KEYWORD FOR MENU!\nUNABLE TO GET MOONS KEYWORD FOR MENU!\nUNABLE TO GET MOONS KEYWORD FOR MENU!");
        }

        internal static string GetMoonPage(int activeIndex, int pageSize, ref int currentPage)
        {
            Plugin.Spam($"activeIndex: {activeIndex}\npageSize: {pageSize}\ncurrentPage: {currentPage}");
            if (MoonListing == null)
            {
                Plugin.ERROR("darmuhsTerminalStuff FATAL ERROR: MoonListing is NULL");
                return "suitsTerminal FATAL ERROR: suitListing is NULL";
            }

            MoonsFilter.GetMoonsToDisplay();

            int listing = MoonsDisplayed.Count;

            if (listing == 0)
            {
                Plugin.Spam("MoonsDisplayed empty!!");
                return "Empty Moon Listing :(\r\n";
            }

            Plugin.Spam($"listing count: {listing}");

            // Ensure currentPage is within valid range
            currentPage = Mathf.Clamp(currentPage, 1, Mathf.CeilToInt((float)listing / pageSize));

            // Calculate the start and end indexes for the current page
            int startIndex = (currentPage - 1) * pageSize;
            int endIndex = Mathf.Min(startIndex + pageSize, listing);
            int totalItems = 0;
            int emptySpace;

            string currentLevel;
            MoonInfo currentMoon = MoonListing.FirstOrDefault(x => x.Level == StartOfRound.Instance.currentLevel);

            if (currentMoon != null)
            {
                if (currentMoon.isHidden && !MoonsPlusConfig.RevealHiddenOnRoute.Value)
                    currentLevel = "?????";
                else if (currentMoon.isCompany)
                    currentLevel = "71 Gordion (Company)";
                else
                    currentLevel = currentMoon.Level.PlanetName;
            }
            else
                currentLevel = StartOfRound.Instance.currentLevel.PlanetName;

            StringBuilder message = new();

            message.Append($"================= Moons Plus  =================\r\n");
            message.Append("\r\n");

            // Recalculate activeIndex based on the current page
            // Ensure activeIndex is within the range of items on the current page
            activeIndex = Mathf.Clamp(activeIndex, startIndex, endIndex - 1);
            Plugin.Spam($"activeSelection: {activeSelection} activeIndex: {activeIndex}");
            Plugin.Spam("matching values");
            activeSelection = activeIndex;

            // Iterate through each item in the current page
            for (int i = startIndex; i < endIndex; i++)
            {

                // Prepend ">" to the active item and append "[EQUIPPED]" line if applicable
                string menuItem;

                MoonInfo moon = MoonsDisplayed[i];

                if (moon != null)
                {
                    menuItem = (i == activeIndex)
                    ? $"> "
                    : $"";

                    if (moon.isCompany && moon.isCurrent)
                        menuItem += $">>Company<<";
                    else if (moon.isCompany)
                        menuItem += $"Company";
                    else if (moon.isLocked)
                        menuItem += $"[ROUTE LOCKED]";
                    else
                    {
                        if (MoonsFilter.Price)
                            menuItem += $"${moon.price} ";

                        if (moon.isHidden && moon.isCurrent)
                            menuItem += $">>[ ??? ]<<";
                        else if (moon.isHidden && !moon.isCurrent)
                            menuItem += $"[ ??? ]";
                        else if (moon.isCurrent)
                            menuItem += $">>{moon.LevelName}<<";
                        else
                            menuItem += $"{moon.LevelName}";

                        if (MoonsFilter.Weather && GetWeatherName(moon.Level).Length > 1)
                            menuItem += $" ({GetWeatherName(moon.Level)})";

                        if (MoonsFilter.Difficulty)
                            menuItem += $" ({moon.Level.riskLevel})";

                        if (moon.AdditionalInfo.Length > 0) //add any additional stuff from other mods accessing this attribute
                            menuItem += moon.AdditionalInfo;
                    }

                }
                else
                {
                    menuItem = (i == activeIndex)
                    ? $"> {i} - **MISSING MOON INFO**"
                    : $"{i} - **MISSING MOON INFO**";
                    Plugin.WARNING($"Unable to find moon at index [ {i} ] of MoonListing!");
                }

                // Display the menu item
                message.Append(menuItem + "\n");
                totalItems++;
            }

            emptySpace = pageSize - totalItems;

            for (int i = 0; i < emptySpace; i++)
            {
                message.Append("\r\n");
            }

            // Display pagination information
            //Page [LeftArrow] < 6/10 > [RightArrow]
            message.Append("\r\n\r\n");
            message.Append($"Filters Menu: [F]\r\n");
    
            message.Append($"Currently Orbiting: {currentLevel}\r\n\r\n");
            message.Append($"Page [LeftArrow] < {currentPage}/{Mathf.CeilToInt((float)listing / pageSize)} > [RightArrow]\r\n");
            message.Append($"Leave Menu: [BackSpace]    Select Moon: [Enter]\r\n\r\n");
            return message.ToString();
        }

        internal static string GetWeatherName(SelectableLevel level)
        {
            string levelWeather = "";
            if (Plugin.instance.WeatherTweaks)
                levelWeather = WeatherTweaksCompat.GetWeather(level);
            else
                levelWeather = level.currentWeather.ToString();

            if (levelWeather.ToLower() == "none")
                levelWeather = "";

            return levelWeather;
        }

        internal static string GetFiltersPage(int activeIndex)
        {
            StringBuilder message = new();

            message.Append($"================= Moons Plus  =================\r\n");
            message.Append("\r\nFilters Menu:\n");


            // Prepend ">" to the active item and append "[EQUIPPED]" line if applicable
            string menuItem;
            int[] sortChangers = [3, 4, 5, 6, 7];

            // Recalculate activeIndex based on the current page
            // Ensure activeIndex is within the range of items on the current page
            activeIndex = Mathf.Clamp(activeIndex, 0, FilterMenu.Count - 1);
            Plugin.Spam($"activeSelection: {activeSelection} activeIndex: {activeIndex}");
            Plugin.Spam("matching values");
            activeSelection = activeIndex;

            for (int i = 0; i < FilterMenu.Count; i++)
            {
                menuItem = (i == activeIndex)
                   ? $"\t> {FilterMenu[i]}"
                   : $"\t{FilterMenu[i]}";
                if (i == 0)
                    menuItem += $" [{FilterMenuBools(MoonsFilter.Weather)}]";
                else if (i == 1)
                    menuItem += $" [{FilterMenuBools(MoonsFilter.Price)}]";
                else if (i == 2)
                    menuItem += $" [{FilterMenuBools(MoonsFilter.Difficulty)}]\r\n";
                else if (sortChangers.Contains(i))
                    menuItem += $" [{FilterMenuBools(MoonsFilter.SortType == i - 3)}]";
                else if (i == 8)
                    menuItem += $" [{FilterMenuBools(MoonsFilter.RemoveTooExpensive)}]";
                else if (i == 9)
                    menuItem += $" [{FilterMenuBools(MoonsFilter.RemoveBadWeather)}]";
                message.Append(menuItem + "\n");
            }

            message.Append($"\r\n\r\nBack to MoonsPlus Menu: [F]\r\n");
            message.Append($"Currently Sorting by: {MoonsFilter.Sorting}\r\n");
            message.Append($"Leave Menu: [BackSpace]    Toggle Filter: [Enter]\r\n\r\n");
            return message.ToString();
        }

        internal static string FilterMenuBools(bool enabled)
        {
            if (enabled)
                return "<color=#00ab66>Active</color>";
            else
                return "<color=#b22222>Disabled</color>";
        }

        internal static void HandleInput()
        {
            if (Keyboard.current[Key.UpArrow].isPressed)
                UpMenu();

            if (Keyboard.current[Key.DownArrow].isPressed)
                DownMenu();

            if (Keyboard.current[Key.Backspace].isPressed)
                ExitMenu(true);

            if (Keyboard.current[Key.LeftArrow].isPressed)
                PrevPage();

            if (Keyboard.current[Key.RightArrow].isPressed)
                NextPage();

            if (Keyboard.current[Key.Enter].isPressed)
                SelectInMenu();

            if (Keyboard.current[Key.F].isPressed)
                ToggleMenus();

        }

        internal static void LoadPage()
        {
            Plugin.instance.Terminal.StartCoroutine(DelayUpdateText());
        }

        internal static IEnumerator DelayUpdateText()
        {
            yield return new WaitForEndOfFrame();
            if (!inFilterMenu)
                MoonsMenu.displayText = GetMoonPage(activeSelection, 10, ref currentPage);
            else
                MoonsMenu.displayText = GetFiltersPage(activeSelection);


            if (StartOfRound.Instance.currentLevel.videoReel != null && !inFilterMenu)
                ShowReel(true);
            else
                ShowReel(false);

            yield return new WaitForEndOfFrame();
            Plugin.instance.Terminal.LoadNewNode(MoonsMenu);
            TerminalParse.NetSync(MoonsMenu);
            yield return new WaitForEndOfFrame();
            
        }

        internal static void ShowReel(bool show)
        {
            if (show)
            {
                MoonInfo currentMoon = MoonListing.FirstOrDefault(x => x.Level == StartOfRound.Instance.currentLevel);

                if(currentMoon != null)
                {
                    if(currentMoon.isHidden)
                    {
                        HideReel();
                        return;
                    }
                }
               
                Plugin.instance.Terminal.terminalImage.rectTransform.sizeDelta = new Vector2(200, 150);
                Plugin.instance.Terminal.terminalImage.rectTransform.anchoredPosition = new Vector2(80, 0);
                MoonsMenu.displayVideo = StartOfRound.Instance.currentLevel.videoReel;
            }
            else
                HideReel();
        }

        internal static void HideReel()
        {
            VisualCore.CamEvents.SetRawImageDimensions(Plugin.instance.Terminal.terminalImage.rectTransform, isFullScreen: true);
            MoonsMenu.displayVideo = null;
        }

        internal static void HideLevelFromMapScreen()
        {
            MoonInfo currentMoon = MoonListing.FirstOrDefault(x => x.Level == StartOfRound.Instance.currentLevel);
            if (currentMoon == null)
                return;

            if (!currentMoon.isHidden)
                return;
            
            StartOfRound.Instance.screenLevelVideoReel.enabled = false;
            StartOfRound.Instance.screenLevelVideoReel.gameObject.SetActive(value: false);
            StartOfRound.Instance.screenLevelDescription.text = "\t????????";
        }

        internal static void ToggleMenus()
        {
            activeSelection = 0;
            inFilterMenu = !inFilterMenu;
            LoadPage();
        }

        internal static void SelectInMenu()
        {
            int[] sortChangers = [3, 4, 5, 6, 7];

            if(inFilterMenu)
            {
                if (activeSelection == 0)
                    ToggleWeatherDisplay();
                else if (activeSelection == 1)
                    TogglePriceDisplay();
                else if (activeSelection == 2)
                    ToggleRiskDisplay();
                else if (sortChangers.Contains(activeSelection))
                    MoonsFilter.ChangeSort(activeSelection - 3);
                else if (activeSelection == 8)
                    MoonsFilter.RemoveTooExpensive = !MoonsFilter.RemoveTooExpensive;
                else if (activeSelection == 9)
                    MoonsFilter.RemoveBadWeather = !MoonsFilter.RemoveBadWeather;
                else
                    Plugin.WARNING("ACTIVE SELECTION OUTSIDE BOUNDS OF FILTER MENU");

                LoadPage();

                return;
            }

            if (MoonsDisplayed[activeSelection].Level == null)
            {
                Plugin.ERROR($"Active Moons Selection is NULL! - {activeSelection}");
                return;
            }

            if (StartOfRound.Instance.travellingToNewLevel || !StartOfRound.Instance.inShipPhase || StartOfRound.Instance.currentLevel == MoonsDisplayed[activeSelection].Level)
                return;

            if (MoonsDisplayed[activeSelection].price > Plugin.instance.Terminal.groupCredits)
                return;

            int newCreds = CostCommands.CalculateNewCredits(Plugin.instance.Terminal.groupCredits, MoonsDisplayed[activeSelection].price, Plugin.instance.Terminal);

            StartOfRound.Instance.ChangeLevelServerRpc(MoonsDisplayed[activeSelection].Level.levelID, newCreds);
            StartOfRound.Instance.SetMapScreenInfoToCurrentLevel();

            LoadPage();
        }

        internal static void UpMenu()
        {
            if (activeSelection > 0)
                activeSelection--;

            LoadPage();
        }

        internal static void DownMenu()
        {
            activeSelection++;
            LoadPage();
        }

        internal static void NextPage()
        {
            if (inFilterMenu)
                return;

            currentPage++;
            LoadPage();
        }

        internal static void PrevPage()
        {
            if (inFilterMenu)
                return;

            if(currentPage > 1)
                currentPage--;

            LoadPage();
        }

        internal static void ExitMenu(bool enableInput)
        {
            Plugin.instance.Terminal.StartCoroutine(MenuClose(enableInput));
        }

        internal static void ToggleWeatherDisplay()
        {
            MoonsFilter.Weather = !MoonsFilter.Weather;
        }

        internal static void TogglePriceDisplay()
        {
            MoonsFilter.Price = !MoonsFilter.Price;
        }

        internal static void ToggleRiskDisplay()
        {
            MoonsFilter.Difficulty = !MoonsFilter.Difficulty;
        }

        internal static string EnterMoonsMenu()
        {
            if (!StartOfRound.Instance.inShipPhase)
                return "Return to orbit to see the moons listing!\r\n\r\n";

            currentPage = 1;
            Plugin.instance.Terminal.StartCoroutine(MenuStart());
            return GetMoonPage(0, 10, ref currentPage);
        }

        internal static IEnumerator MenuStart()
        {
            if (inMoonsMenu)
                yield break;

            yield return new WaitForEndOfFrame();
            Plugin.instance.Terminal.screenText.caretColor = transparent;
            inMoonsMenu = true;
            yield return new WaitForEndOfFrame();
            Plugin.instance.Terminal.screenText.DeactivateInputField();
            Plugin.instance.Terminal.screenText.interactable = false;
            yield return new WaitForEndOfFrame();

            if (StartOfRound.Instance.currentLevel.videoReel != null)
            {
                ShowReel(true);
                Plugin.instance.Terminal.LoadNewNode(MoonsMenu);
                TerminalParse.NetSync(MoonsMenu);
            }

            yield break;
        }

        internal static IEnumerator MenuClose(bool enableInput)
        {
            yield return new WaitForEndOfFrame();
            inMoonsMenu = false;
            yield return new WaitForEndOfFrame();

            ShowReel(false);

            TerminalNode nextNode = null;
            TerminalBeginUsing.ChooseStartPage(Plugin.instance.Terminal, ref nextNode);
            
            if(nextNode == null)
                nextNode = startNode;
            

            Plugin.instance.Terminal.LoadNewNode(nextNode);
            TerminalParse.NetSync(nextNode);

            yield return new WaitForEndOfFrame();
            Plugin.instance.Terminal.screenText.caretColor = TerminalCustomizer.SetColorFor(ConfigSettings.TerminalCaretColor.Value, CustomTerminalStuff.TextCaret);

            if (enableInput)
            {
                Plugin.instance.Terminal.screenText.ActivateInputField();
                Plugin.instance.Terminal.screenText.interactable = true;
            }

            yield break;
        }

        internal static void UpdateMoonTravelHistory(string levelName)
        {
            if(TryGetMoon(levelName, out MoonInfo moon))
            {
                moon.haveVisited = true;
                moon.UpdateHistory();
            }
        }

        internal static List<string> GetTravelHistory()
        {
            Plugin.Spam("GetTravelHistory");
            MoonInfo currentMoon = MoonListing.FirstOrDefault(x => x.isCurrent);

            if (currentMoon != null)
                return [currentMoon.LevelName];
            else
                return [];
        }

        internal static void SetAllMoonsUnvisited()
        {
            Plugin.Spam("SETALLMOONSUNVISITED");
            foreach(MoonInfo moon in MoonListing)
            {
                if (MoonsUnhidden.Contains(moon))
                {
                    moon.haveVisited = false;
                    moon.Hide(true);
                }
                else
                    moon.haveVisited = false;
            }

            MoonsUnhidden.Clear();
                
        }

        //public stuff

        public static bool TryGetMoon(string levelName, out MoonInfo moon)
        {
            moon = null!;
            if (MoonListing.Count == 0)
                return false;

            moon = MoonListing.FirstOrDefault(x => x.LevelName.Equals(levelName, System.StringComparison.InvariantCultureIgnoreCase));
            if (moon == null)
                return false;

            return true;
        }

        //Get selectable level's MoonInfo
        public static bool TryGetMoon(SelectableLevel level, out MoonInfo moon)
        {
            moon = null!;
            if(MoonListing.Count == 0)
                return false;

            moon = MoonListing.FirstOrDefault(x => x.Level == level);
            if(moon == null) 
                return false;

            return true;   
        }

        //Try to get a selectable level from the currently displayed moons
        public static bool TryGetDisplayMoon(SelectableLevel level, out MoonInfo moon)
        {
            moon = null!;
            if (MoonsDisplayed.Count == 0)
                return false;

            moon = MoonsDisplayed.FirstOrDefault(x => x.Level == level);
            if (moon == null)
                return false;

            return true;
        }
    }
}

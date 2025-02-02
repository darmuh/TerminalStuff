using Key = UnityEngine.InputSystem.Key;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Collections;
using TerminalStuff.PluginCore;
using static TerminalStuff.EventSub.TerminalStart;
using static TerminalStuff.TerminalEvents;
using static OpenLib.ConfigManager.ConfigSetup;
using static OpenLib.CoreMethods.AddingThings;
using OpenLib.CoreMethods;
using System.Linq;
using TerminalStuff.Compatibility;
using HarmonyLib;
using TerminalStuff.Configs;
using TerminalStuff.VisualCore;

namespace TerminalStuff.SpecialStuff
{
    public class MoonsPlus
    {
        public static OpenLib.Events.Events.CustomEvent<List<MoonInfo>> UpdateMoonsDisplayed = new();
        internal static InteractiveMenu MoonsPlusMenu = new("moonsMenu", LoadPage, SelectInMenu, ExitInTerminal);
        internal static TerminalNode OriginalMoonsPage = null!;

        internal static List<MoonInfo> MoonListing = [];
        public static List<MoonInfo> MoonsDisplayed = [];
        internal static List<string> AcceptableWeathers = [];
        internal static List<string> FilterMenu = [];
        internal static List<string> MoonsVisited = [];
        internal static List<string> MoonsPurchased = [];

        //menu stuff
        internal static bool inFilterMenu = false;
        internal static TerminalNode MoonsMenu = null!;
        internal static FilterView MoonsFilter = new();

        //
        internal static Camera ExternalShipCam;

        internal static void SetToVanilla()
        {
            MoonsPlusMenu.isMenuEnabled = false;

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

        internal static void ExitInTerminal()
        {
            ExitMenu(true);
        }

        internal static void GetMoons()
        {
            if (!Commands.TerminalMoonsPlus.Value)
            {
                SetToVanilla();
                return;
            }
                
            if (StartOfRound.Instance.levels.Length == 0) 
                return;

            MoonsDisplayed.Clear();

           

            List<TerminalNode> allNodes = LogicHandling.GetAllNodes();

            for (int i = 0; i < StartOfRound.Instance.levels.Length; i++)
            {
                Plugin.Spam($"GetMoons - {StartOfRound.Instance.levels[i].name}");
                if (StartOfRound.Instance.levels[i].name == "LiquidationLevel")
                    continue;

                MoonInfo moon = MoonListing.FirstOrDefault(m => m.LevelID == StartOfRound.Instance.levels[i].levelID);

                if (moon != null)
                    moon.Reload(StartOfRound.Instance.levels[i], allNodes);
                else
                {
                    moon = new(StartOfRound.Instance.levels[i], allNodes);
                    MoonListing.Add(moon);
                }
                    
            }
            
            MoonsPlusMenu.isMenuEnabled = true;
            MoonsPlusMenu.activeSelection = 0;
            MoonsPlusMenu.currentPage = 1;
            MoonsPlusMenu.AddToOtherActions(Key.F, ToggleMenus);

            MoonsFilter.ChangeSort(MoonsFilter.SortType);

            MoonsDisplayed = MoonListing.FindAll(x => x.ShowInListing());

            GetFilters();
            SaveManager.InitMoonPlusSave();

            if (!MoonsPlusConfig.MoonsPlusKeywords.Value.Split(';').Any(x => x.Trim().ToLower() == "moons"))
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

            MoonsFilter.GetMoonsToDisplay(ref activeIndex);

            int listing = MoonsDisplayed.Count;

            if (listing == 0)
            {
                Plugin.WARNING("MoonsDisplayed empty!!");
                return "Empty Moon Listing :(\r\n\r\nPlease press [BackSpace] or [Escape] to exit...\r\n";
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
                if (currentMoon.IsHidden && !MoonsPlusConfig.RevealHiddenOnRoute.Value)
                    currentLevel = "?????";
                else if (currentMoon.IsCompany)
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
            Plugin.Spam($"activeSelection: {MoonsPlusMenu.activeSelection} activeIndex: {activeIndex}");
            Plugin.Spam("matching values");
            MoonsPlusMenu.activeSelection = activeIndex;

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

                    if (moon.IsCompany && moon.IsCurrent)
                        menuItem += $">>Company<<";
                    else if (moon.IsCompany)
                        menuItem += $"Company";
                    else if (moon.IsLocked)
                        menuItem += $"[ROUTE LOCKED]";
                    else
                    {
                        if (MoonsFilter.Price)
                            menuItem += $"${moon.DisplayPrice} ";

                        if (moon.IsHidden && moon.IsCurrent)
                            menuItem += $">>[ ??? ]<<";
                        else if (moon.IsHidden && !moon.IsCurrent)
                            menuItem += $"[ ??? ]";
                        else if (moon.IsCurrent)
                            menuItem += $">>{moon.LevelName}<<";
                        else
                            menuItem += $"{moon.LevelName}";

                        if (MoonsFilter.Weather && GetWeatherName(moon.Level).Length > 1)
                            menuItem += $" ({GetWeatherName(moon.Level)})";

                        if (MoonsFilter.Difficulty)
                            menuItem += $" ({moon.Level.riskLevel})";

                        if (moon.AdditionalInfo.Length > 0) //add any additional stuff from other mods accessing this attribute
                            menuItem += moon.AdditionalInfo;

                        if (moon.DisplayPrice <= Plugin.instance.Terminal.groupCredits && MoonsPlusConfig.AffordableColor.Value.Length > 0)
                        {
                            menuItem = menuItem.Insert(0, $"<color={MoonsPlusConfig.AffordableColor.Value}>");
                            menuItem += "</color>";
                        }

                        if (moon.DisplayPrice > Plugin.instance.Terminal.groupCredits && MoonsPlusConfig.NotEnoughCredsColor.Value.Length > 0)
                        {
                            menuItem = menuItem.Insert(0, $"<color={MoonsPlusConfig.NotEnoughCredsColor.Value}>");
                            menuItem += "</color>";
                        }
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
            string levelWeather;
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
            Plugin.Spam($"activeSelection: {MoonsPlusMenu.activeSelection} activeIndex: {activeIndex}");
            Plugin.Spam("matching values");
            MoonsPlusMenu.activeSelection = activeIndex;

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

        internal static void LoadPage()
        {
            Plugin.instance.Terminal.StartCoroutine(DelayUpdateText());
        }

        internal static IEnumerator DelayUpdateText()
        {
            yield return new WaitForEndOfFrame();
            if (!inFilterMenu)
                MoonsMenu.displayText = GetMoonPage(MoonsPlusMenu.activeSelection, 10, ref MoonsPlusMenu.currentPage);
            else
                MoonsMenu.displayText = GetFiltersPage(MoonsPlusMenu.activeSelection);

            MoonInfo current = MoonsDisplayed[MoonsPlusMenu.activeSelection];

            if (current.Level.videoReel != null && !inFilterMenu)
                ShowReel(true);
            else
                ShowReel(false);

            yield return new WaitForEndOfFrame();
            LoadAndSync(MoonsMenu);
            yield return new WaitForEndOfFrame();
            
        }

        internal static void ShowReel(bool show)
        {
            if (!MoonsPlusConfig.ShowVideoReels.Value)
            {
                Plugin.Spam($"Video Reels Disabled! (ShowVideoReels is {MoonsPlusConfig.ShowVideoReels.Value})");
                HideReel();
                return;
            }
                

            if (show)
            {
                MoonInfo currentMoon = MoonsDisplayed[MoonsPlusMenu.activeSelection];

                if(currentMoon != null)
                {
                    if(currentMoon.IsHidden && MoonsPlusConfig.ObscureHiddenInfo.Value)
                    {
                        ExternalShipCam = GameObject.Find("Environment/HangarShip/Cameras/FrontDoorSecurityCam/SecurityCamera")?.GetComponent<Camera>();
                        if (ExternalShipCam != null)
                        {
                            MoonsMenu.displayVideo = null!;
                            Plugin.instance.Terminal.terminalImage.rectTransform.sizeDelta = new Vector2(200, 150);
                            Plugin.instance.Terminal.terminalImage.rectTransform.anchoredPosition = new Vector2(80, 0);
                            MoonsMenu.displayTexture = ExternalShipCam.targetTexture;
                            return;
                        }

                        HideReel();
                        return;
                    }
                }
 
                Plugin.instance.Terminal.terminalImage.rectTransform.sizeDelta = new Vector2(200, 150);
                Plugin.instance.Terminal.terminalImage.rectTransform.anchoredPosition = new Vector2(80, 0);
                MoonsMenu.displayVideo = currentMoon.Level.videoReel;
            }
            else
                HideReel();
        }

        internal static void HideReel()
        {
            MoonsMenu.displayVideo = null!;
            MoonsMenu.displayTexture = null!;
            CamEvents.SetRawImageDimensions(Plugin.instance.Terminal.terminalImage.rectTransform, isFullScreen: true);
        }

        internal static void HideLevelFromMapScreen()
        {
            if (!MoonsPlusConfig.ObscureHiddenInfo.Value)
                return;

            MoonInfo currentMoon = MoonListing.FirstOrDefault(x => x.Level == StartOfRound.Instance.currentLevel);
            if (currentMoon == null)
                return;

            if (!currentMoon.IsHidden)
                return;
            
            StartOfRound.Instance.screenLevelVideoReel.enabled = false;
            StartOfRound.Instance.screenLevelVideoReel.gameObject.SetActive(value: false);
            StartOfRound.Instance.screenLevelDescription.text = "\t????????";
        }

        internal static void ToggleMenus()
        {
            MoonsPlusMenu.activeSelection = 0;
            inFilterMenu = !inFilterMenu;
            LoadPage();
        }

        internal static void SelectInMenu()
        {
            int[] sortChangers = [3, 4, 5, 6, 7];

            if(inFilterMenu)
            {
                if (MoonsPlusMenu.activeSelection == 0)
                    ToggleWeatherDisplay();
                else if (MoonsPlusMenu.activeSelection == 1)
                    TogglePriceDisplay();
                else if (MoonsPlusMenu.activeSelection == 2)
                    ToggleRiskDisplay();
                else if (sortChangers.Contains(MoonsPlusMenu.activeSelection))
                    MoonsFilter.ChangeSort(MoonsPlusMenu.activeSelection - 3);
                else if (MoonsPlusMenu.activeSelection == 8)
                    MoonsFilter.RemoveTooExpensive = !MoonsFilter.RemoveTooExpensive;
                else if (MoonsPlusMenu.activeSelection == 9)
                    MoonsFilter.RemoveBadWeather = !MoonsFilter.RemoveBadWeather;
                else
                    Plugin.WARNING("ACTIVE SELECTION OUTSIDE BOUNDS OF FILTER MENU");

                LoadPage();

                return;
            }

            if (MoonsDisplayed[MoonsPlusMenu.activeSelection].Level == null)
            {
                Plugin.ERROR($"Active Moons Selection is NULL! - {MoonsPlusMenu.activeSelection}");
                return;
            }

            if (StartOfRound.Instance.travellingToNewLevel || !StartOfRound.Instance.inShipPhase || StartOfRound.Instance.currentLevel == MoonsDisplayed[MoonsPlusMenu.activeSelection].Level)
            {
                Plugin.instance.Terminal.PlayTerminalAudioServerRpc(1);
                return;
            }
                

            if (MoonsDisplayed[MoonsPlusMenu.activeSelection].DisplayPrice > Plugin.instance.Terminal.groupCredits)
            {
                Plugin.instance.Terminal.PlayTerminalAudioServerRpc(1);
                return;
            }

            if (MoonsPlusConfig.UseVanillaPurchaseNodes.Value && MoonsDisplayed[MoonsPlusMenu.activeSelection].purchaseNode != null)
            {
                ExitMenu(true, MoonsDisplayed[MoonsPlusMenu.activeSelection].purchaseNode);
                return;
            }

            int newCreds = CostCommands.CalculateNewCredits(Plugin.instance.Terminal.groupCredits, MoonsDisplayed[MoonsPlusMenu.activeSelection].DisplayPrice, Plugin.instance.Terminal);

            StartOfRound.Instance.ChangeLevelServerRpc(MoonsDisplayed[MoonsPlusMenu.activeSelection].Level.levelID, newCreds);
            StartOfRound.Instance.SetMapScreenInfoToCurrentLevel();

            LoadPage();
        }

        internal static void CheckNodePurchase(TerminalNode node)
        {
            MoonInfo moon = MoonListing.FirstOrDefault(x => x.resultNode == node);
            if (moon != null)
            {
                moon.OTP = true;
                moon.resultNode.itemCost = 0;
                moon.purchaseNode.itemCost = 0;

                moon.UpdateInfo();
            }
        }

        internal static void LobbyClose()
        {
            ClearMoonsVisited();
            MoonListing.Do(x => x.Reset());
            SetToVanilla();
        }

        internal static void ShipReset()
        {
            ClearMoonsVisited();
            MoonListing.Do(x => x.Reset());
        }

        internal static void ExitMenu(bool enableInput, TerminalNode newPage = null!)
        {
            Plugin.instance.Terminal.StartCoroutine(MenuClose(enableInput, newPage));
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

            MoonsPlusMenu.currentPage = 1;
            inFilterMenu = false;

            int current = MoonsDisplayed.FindIndex(x => x.IsCurrent);

            if (current == -1)
                current = 0;

            Plugin.instance.Terminal.StartCoroutine(MenuStart());
            return GetMoonPage(current, 10, ref MoonsPlusMenu.currentPage);
        }

        internal static IEnumerator MenuStart()
        {
            if (MoonsPlusMenu.inMenu)
                yield break;

            yield return new WaitForEndOfFrame();
            Plugin.instance.Terminal.screenText.caretColor = transparent;
            MoonsPlusMenu.inMenu = true;
            yield return new WaitForEndOfFrame();
            Plugin.instance.Terminal.screenText.DeactivateInputField();
            Plugin.instance.Terminal.screenText.interactable = false;
            yield return new WaitForEndOfFrame();

            if (StartOfRound.Instance.currentLevel.videoReel != null)
            {
                ShowReel(true);
                LoadAndSync(MoonsMenu);
            }

            yield break;
        }

        internal static IEnumerator MenuClose(bool enableInput, TerminalNode newPage = null)
        {
            yield return new WaitForEndOfFrame();
            MoonsPlusMenu.inMenu = false;
            yield return new WaitForEndOfFrame();

            ShowReel(false);

            TerminalNode nextNode = startNode;

            if (terminalSettings.startPage != null)
                nextNode = terminalSettings.startPage;

            if (newPage != null)
                nextNode = newPage;

            LoadAndSync(nextNode);

            yield return new WaitForEndOfFrame();
            Plugin.instance.Terminal.screenText.caretColor = TerminalCustomizer.SetColorFor(CustomizeConfig.TerminalCaretColor.Value, CustomTerminalStuff.TextCaret);

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
                if (MoonsPlusConfig.OneTimePurchase.Value)
                    moon.OTP = true;
                moon.UpdateHistory();
            }
        }

        internal static List<string> GetTravelHistory()
        {
            Plugin.Spam("GetTravelHistory");
            MoonInfo currentMoon = MoonListing.FirstOrDefault(x => x.IsCurrent);

            if (currentMoon != null)
                return [currentMoon.LevelName];
            else
                return [];
        }

        internal static void ClearMoonsVisited()
        {
            MoonsVisited.Clear();
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

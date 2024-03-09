using System;
using System.Collections.Generic;
using System.Text;
using static TerminalApi.TerminalApi;
using static TerminalStuff.DynamicCommands;

namespace TerminalStuff
{

    internal class MenuBuild
    {
        internal static StringBuilder extraLinesForInfoCommands = new StringBuilder("");
        internal static bool isNextEnabled = false;
        internal static int nextCount = 1; //start at same value as pages
        internal static string currentCategory = string.Empty;
        internal static List<string> keywordList = new List<string> { "comfort", "extras", "controls", "fun" };
        internal static List<TerminalNode> menuNodes = new List<TerminalNode>();

        internal static Dictionary<string, StringBuilder> categoryBuilders = new Dictionary<string, StringBuilder>();
        internal static Dictionary<string, int> getCategoryPageCount = new Dictionary<string, int>();

        internal static TerminalNode HandleMenus(string keyWord, out TerminalNode outNode)
        {
            outNode = null;
            nextCount = 1;
            if (keywordList.Contains(keyWord))
            {
                for (int i = 0; i < keywordList.Count; i++)
                {
                    if (keywordList[i] == keyWord)
                    {
                        currentCategory = keywordList[i];
                        GetMenuNode(keyWord, out outNode);
                        if (outNode != null)
                            Plugin.MoreLogs("outNode found!");
                        if (outNode != null && outNode.displayText.Contains("Use command \"next\" to see the next page!"))
                            isNextEnabled = true;
                        break;
                    }
                }
            }
            return outNode;

        }

        internal static TerminalNode GetMenuNode(string name, out TerminalNode outNode)
        {
            outNode = null;
            for (int i = 0; i < menuNodes.Count; i++)
            {
                if (menuNodes[i].name.Contains(name))
                {
                    outNode = menuNodes[i];
                    break;
                }
            }

            return outNode;
        }

        internal static int GetMaxPages(out int maxPages)
        {
            maxPages = 0;
            foreach (var category in getCategoryPageCount.Keys)
            {
                if (category == currentCategory)
                {
                    Plugin.MoreLogs($"Setting maxPages to {getCategoryPageCount[category]} from {category}");
                    maxPages = getCategoryPageCount[category];
                    break;
                }
                else
                    Plugin.Log.LogWarning($"Category is not {category}");
            }
            if (maxPages == 0)
                Plugin.Log.LogWarning("Unable to get max pages");
            return maxPages;
        }


        internal static TerminalNode HandleNext(out TerminalNode outNode)
        {
            outNode = null;

            if (!isNextEnabled)
                return outNode;

            nextCount++;
            int maxPages = GetMaxPages(out maxPages);
            Plugin.MoreLogs($"maxpages: {maxPages}");

            for (int i = 0; i < keywordList.Count; i++)
            {
                if (keywordList[i] == currentCategory && nextCount <= maxPages)
                {
                    GetMenuNode($"{keywordList[i]}.{nextCount}", out outNode);
                    if (outNode != null)
                        Plugin.MoreLogs("outNode found!");
                    if (outNode != null && outNode.displayText.Contains("Use command \"next\" to see the next page!"))
                        isNextEnabled = true;
                    else
                        isNextEnabled = false;

                    Plugin.MoreLogs($"Attempting to show page {nextCount} of {keywordList[i]}");
                    break;
                }
            }
            return outNode;
        }

        internal static void CreateMoreCommand()
        {
            TerminalNode moreText = CreateTerminalNode("Welcome to darmuh's Terminal Upgrade!\r\n\tSee below Categories for new stuff :)\r\n\r\n[COMFORT]\r\nImproves the terminal user experience.\r\n\r\n[EXTRAS]\r\nAdds extra functionality to the ship terminal.\r\n\r\n[CONTROLS]\r\nGives terminal more control of the ship's systems.\r\n\r\n[FUN]ctionality\r\nType \"fun\" for a list of these FUNctional commands.\r\n\r\n", true);
            TerminalKeyword moreKeyword = CreateTerminalKeyword("more", true, moreText);
            moreText.name = "More Command by Darmuh";
            AddTerminalKeyword(moreKeyword);
        }

        internal static void CreateMenus()
        {
            categoryBuilders["comfort"] = new StringBuilder();
            categoryBuilders["extras"] = new StringBuilder();
            categoryBuilders["controls"] = new StringBuilder();
            categoryBuilders["fun"] = new StringBuilder();



            foreach (var category in categoryBuilders.Keys)
            {
                StringBuilder categoryStringBuilder = categoryBuilders[category];

                // Add commands to StringBuilder based on the category
                switch (category)
                {
                    case "comfort":
                        AddComfortCommands(ref categoryStringBuilder);
                        break;

                    case "extras":
                        AddExtrasCommands(ref categoryStringBuilder);
                        break;

                    case "controls":
                        AddControlsCommands(ref categoryStringBuilder);
                        break;

                    case "fun":
                        AddFunCommands(ref categoryStringBuilder);
                        break;

                    default:
                        break;
                }

                // Split the category text into pages
                List<PageBuilder> pages = PageSplitter.SplitTextIntoPages(categoryStringBuilder.ToString(), 11);

                foreach (var page in pages)
                {
                    // Add the header for each page
                    page.Content.Insert(0, $"=== {category.ToUpper()} [Page:{page.PageNumber}] ===\r\n\r\n");

                    // Create a TerminalNode for each page
                    if (page.PageNumber == 1)
                    {
                        TerminalNode categoryNode = CreateTerminalNode($"{page.Content}", true);
                        categoryNode.name = $"{category}";
                        menuNodes.Add(categoryNode);
                        Plugin.MoreLogs("First page: [" + page.PageNumber.ToString() + "]");
                    }
                    else
                    {
                        TerminalNode categoryNode = CreateTerminalNode($"{page.Content}", true);
                        categoryNode.name = $"{category}.{page.PageNumber}";
                        menuNodes.Add(categoryNode);

                        Plugin.MoreLogs("Page Number: [" + page.PageNumber.ToString() + "]");
                    }

                    Plugin.MoreLogs($"Number of lines in {category} Page {page.PageNumber}: {page.Content.ToString().Split(new[] { ".\r\n" }, StringSplitOptions.None).Length}");
                    if (page.PageNumber < pages.Count)
                    {
                        extraLinesForInfoCommands.AppendLine($"{category} {page.PageNumber}"); // Add a marker for 'next' command
                    }
                }

                getCategoryPageCount[category] = pages.Count;
            }
        }



        // Additional methods to add category-specific commands
        static void AddComfortCommands(ref StringBuilder comfortStringBuilder)
        {

            if (ConfigSettings.terminalClear.Value)
                comfortStringBuilder.AppendLine("> clear\r\nClear the terminal of any existing text.\r\n");

            if (ConfigSettings.terminalAlwaysOn.Value)
                comfortStringBuilder.AppendLine($"> {ConfigSettings.alwaysOnKeyword.Value}\r\nToggle the Always-On Terminal Screen mode.\r\n");
            if (ConfigSettings.terminalFov.Value)
                comfortStringBuilder.AppendLine($"> fov <value>\r\nUpdate your in-game Field of View.\r\n");
            if (ConfigSettings.terminalHeal.Value)
                comfortStringBuilder.AppendLine($"> heal, {ConfigSettings.healKeyword2.Value}\r\nHeal yourself from any damage.\r\n");
            if (ConfigSettings.terminalKick.Value && (GameNetworkManager.Instance != null && GameNetworkManager.Instance.isHostingGame))
                comfortStringBuilder.AppendLine($"> kick\r\nKick another employee from your group.\r\n");
            if (ConfigSettings.terminalLobby.Value)
                comfortStringBuilder.AppendLine($"> lobby\r\nDisplay current lobby name.\r\n");
            if (ConfigSettings.terminalMods.Value)
                comfortStringBuilder.AppendLine($"> mods, {ConfigSettings.modsKeyword2.Value}\r\nDisplay your currently loaded Mods.\r\n");
            if (ConfigSettings.terminalQuit.Value)
                comfortStringBuilder.AppendLine($"> quit, {ConfigSettings.quitKeyword2.Value}\r\nLeave the terminal.\r\n");

            comfortStringBuilder.AppendLine("> home\r\nReturn to start screen.\r\n");

            // Add more comfort commands...
        }

        static void AddExtrasCommands(ref StringBuilder extrasStringBuilder)
        {
            if (ConfigSettings.terminalLink.Value)
                extrasStringBuilder.AppendLine($"> {ConfigSettings.linkKeyword.Value}\r\n {ConfigSettings.customLinkHint.Value}\r\n");

            if (ConfigSettings.terminalLink2.Value)
                extrasStringBuilder.AppendLine($"> {ConfigSettings.link2Keyword.Value}\r\n {ConfigSettings.customLink2Hint.Value}\r\n");

            if (ConfigSettings.terminalCams.Value)
                extrasStringBuilder.AppendLine($"> cams, {ConfigSettings.camsKeyword2.Value}\r\nToggle displaying cameras in terminal.\r\n");

            if (ConfigSettings.terminalMap.Value)
                extrasStringBuilder.AppendLine($"> map, {ConfigSettings.mapKeyword2.Value}\r\nShortcut to toggle radar map on terminal.\r\n");

            if (ConfigSettings.terminalMinimap.Value)
                extrasStringBuilder.AppendLine($"> {ConfigSettings.minimapKeyword.Value}\r\nToggle cameras and radar map via MiniMap Mode.\r\n");

            if (ConfigSettings.terminalOverlay.Value)
                extrasStringBuilder.AppendLine($"> {ConfigSettings.overlayKeyword.Value}\r\nToggle cameras and radar map via Overlay Mode.\r\n");

            if (ConfigSettings.terminalMirror.Value)
                extrasStringBuilder.AppendLine($"> mirror\r\nToggle a camera on screen to see yourself.\r\n");

            if (ConfigSettings.terminalLoot.Value)
                extrasStringBuilder.AppendLine($"> loot, {ConfigSettings.lootKeyword2.Value}\r\nDisplay total value of all loot on-board.\r\n");

            if (ConfigSettings.terminalLootDetail.Value)
                extrasStringBuilder.AppendLine($"> lootlist, {ConfigSettings.ListScrapKeyword.Value}\r\nDisplay a detailed list of all loot on-board.\r\n");

            if (ConfigSettings.terminalListItems.Value)
                extrasStringBuilder.AppendLine($"> itemlist, {ConfigSettings.ListItemsKeyword.Value}\r\nDisplay a detailed list of all non-scrap items on-board that are not being held.\r\n");

            if (ConfigSettings.terminalVitals.Value && ConfigSettings.ModNetworking.Value)
                extrasStringBuilder.AppendLine($"> vitals\r\nDisplay vitals of employee being tracked on radar.\r\n");

            if (ConfigSettings.terminalVitalsUpgrade.Value && ConfigSettings.ModNetworking.Value)
                extrasStringBuilder.AppendLine($"> vitalspatch\r\nPurchase upgrade to Vitals Software Patch 2.0\r\n");

            if (ConfigSettings.terminalBioScan.Value && ConfigSettings.ModNetworking.Value)
                extrasStringBuilder.AppendLine($"> bioscan\r\n Use Ship BioScanner to search for non-employee lifeforms.\r\n");

            if (ConfigSettings.terminalBioScan.Value && ConfigSettings.ModNetworking.Value)
                extrasStringBuilder.AppendLine($"> bioscanpatch\r\n Purchase upgrade to BioScanner Software Patch 2.0\r\n");

            if (ConfigSettings.terminalRefund.Value && ConfigSettings.ModNetworking.Value)
                extrasStringBuilder.AppendLine($"> refund \r\nCancel any purchase that has yet to be delivered from the dropship.\r\n");
            
            if (ConfigSettings.terminalPrevious.Value)
                extrasStringBuilder.AppendLine($"> previous \r\nUse this command to switch to previous radar target during any cams view.\r\n");


            // Add more extras commands...
        }

        static void AddControlsCommands(ref StringBuilder controlsStringBuilder)
        {
            if (ConfigSettings.terminalDanger.Value)
                controlsStringBuilder.AppendLine($"> {ConfigSettings.dangerKeyword.Value} \r\nDisplays the danger level once the ship has landed.\r\n");
            if (ConfigSettings.terminalLever.Value)
                controlsStringBuilder.AppendLine($"> {Lever}\r\nRemotely pull the ship lever.\r\n");
            if (ConfigSettings.terminalDoor.Value)
                controlsStringBuilder.AppendLine($"> {ConfigSettings.doorKeyword.Value}\r\nRemotely open/close the ship doors.\r\n");
            if (ConfigSettings.terminalLights.Value)
                controlsStringBuilder.AppendLine($"> {ConfigSettings.lightsKeyword.Value}\r\nRemotely toggle the ship lights.\r\n");
            if (ConfigSettings.terminalTP.Value)
                controlsStringBuilder.AppendLine($"> {ConfigSettings.tpKeyword.Value}, {ConfigSettings.tpKeyword2.Value}\r\nRemotely push the Teleporter button.\r\n");
            if (ConfigSettings.terminalITP.Value)
                controlsStringBuilder.AppendLine($"> {ConfigSettings.itpKeyword.Value}, {ConfigSettings.itpKeyword2.Value}\r\nRemotely push the Inverse Teleporter button.\r\n");
            if (ConfigSettings.terminalClockCommand.Value)
                controlsStringBuilder.AppendLine($"> clock, {ConfigSettings.clockKeyword2.Value}\r\nToggle Terminal Clock display on/off.\r\n");
            if (ConfigSettings.terminalRestart.Value)
                controlsStringBuilder.AppendLine($"> restart\r\nRestart the lobby and get a new ship (skips firing sequence)");

            // Add more controls commands...
        }

        static void AddFunCommands(ref StringBuilder funStringBuilder)
        {
            if (ConfigSettings.terminalFcolor.Value && ConfigSettings.ModNetworking.Value)
            {
                funStringBuilder.AppendLine($"> {fColor} <color>\r\nUpgrade your flashlight with a new color.\r\n");
                funStringBuilder.AppendLine($"> {fColor} list\r\nView available colors for flashlight.\r\n");
            }

            if (ConfigSettings.terminalScolor.Value && ConfigSettings.ModNetworking.Value)
            {
                funStringBuilder.AppendLine($"> {sColor} <all,front,middle,back> <color>\r\nChange the color of the ship's lights.\r\n");
                funStringBuilder.AppendLine($"> {sColor} list\r\nView available colors to change ship lights.\r\n");
            }

            if (ConfigSettings.terminalRandomSuit.Value)
                funStringBuilder.AppendLine($"> {ConfigSettings.randomSuitKeyword.Value} \r\nPut on a random suit.\r\n");

            if (ConfigSettings.terminalGamble.Value && ConfigSettings.ModNetworking.Value)
                funStringBuilder.AppendLine($"> {Gamble} <percentage>\r\nGamble a percentage of your credits.\r\n");
            if (ConfigSettings.terminalLol.Value)
                funStringBuilder.AppendLine($"> {ConfigSettings.lolKeyword.Value}\r\nPlay a silly video on the terminal.\r\n");

            // Add more fun commands...
        }
    }
}

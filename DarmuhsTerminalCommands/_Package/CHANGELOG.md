# Change Log

All notable changes to this project will be documented in this file.
 
The format is based on [Keep a Changelog](http://keepachangelog.com/).
This project does NOT adhere to perfect Semantic Versioning. Mostly because I don't have the time to learn how to use it.

## [3.10.2]
 - Update DawnLib version for compat with V80 recompile.
 - Fixed unreported issue with restart command that was introduced by V80
 - Fixed MoonsPlus issue that caused Dawnlib transition animation not to display on ship screen.
 - Added extra NRE handling for commonly used local player reference

## [3.10.1]
 - Fixed MoonsPlus replacing the vanilla "moons" keyword and then deleting it after lobby reload. (bug introduced in 3.10.0)

## [3.10.0] (Long awaited fixes, Compatible with both v73 and v80!)
 - The system behind the various monitoring view types (map, minicams, overlay, etc.) has been reworked entirely.
	- This was mostly a detangling of this system.
 - MoonsPlus mini preview fixes, including fixing compatibility with vanilla ``view monitor`` command.
	- Thanks glacialstage aka Sily Wawa for their help identifying/testing issues relating to this.
 - ShipInventory compatibility for ship loot commands has been updated to [version 2.0.8](https://thunderstore.io/c/lethal-company/p/LethalCompanyModding/ShipInventoryUpdated/)
 - MoonsPlus & StorePlus menu size configuration items have been fixed (thanks xCore for reminding me to fix this)
 - StorePlus new configuration item - DontAddToOther
	- This was a requested config item to identify keywords that should *not* be added to StorePlus' Other section
 - StorePlus new configuration item - HideFromStorePlus
	- This was a highly requested config item to remove certain shop items from showing up in the StorePlus menus.
	- Simply enter each store item you wish to hide followed by a comma for it to register the item as hidden. (This does not disable the item's keyword)
 - Fixed MoonsPlus bug where unhiding a moon would not actually update the menu item name to show the level name.
 - Added compat for Dawnlib moon lock, hide, price, and disabled status.
	- Will use LLL when ``Allow LLL to Override Vanilla Moon Locked/Hidden Status`` is enabled.
 - OpenLib version 0.4.2 and 0.4.3 contains generic command-related fixes that affect this mod.
 - This version is compatible with v73 by using Openlib version 0.4.1 and compatible with v80 by using newer Openlib versions!

## [3.9.0] (V73 Update)
 - Networking updated for Lethal Company v73
	- This included many optimizations/changes on the backend of this mod.
	- Now utilizing Openlib networker.
	- Networking is now entirely dependent on host.
 - MoonsPlus updates
	- Menu now has separate pages for affordable moons & good weather only moons
	- Sorting logic has been modified and a new config item has been provided to change the default
	- Overall code overhaul should fix many issues from v3.8.5
 - StorePlus updates
	- New default config items for sorting style/savings
	- Overall code overhaul should fix many issues from v3.8.5
 - Removed hard-coded spaces from HomePage text. Config defaults have been updated with this in mind.
 - Overhauled commands to use OpenLib's newer command addition system. 
	- This is a backend change and should not make a notice-able difference but did take me some time to implement.
 - Many more undocumented changes.

## [3.8.5]
 - Fixed some issues with MoonsPlus when networking is disabled.
	- Added notes for all config items that require networking
	- Removed credit calculating method that requires networking
	- Added more networking disabled checks throughout all mooninfo methods
 - Fixed IsHidden check for moons when LLL is not present

## [3.8.4]
 - Fixed NRE being thrown on player respawn that would cause the pov to be inside the player's head
	- Moved MoonsPlus initiation to delayed terminal start instead of on player spawn (not sure why I had it that way to begin with)
 - Fixed another NRE thrown at OneTimePurchaseLoadIn due to the company building having been visited in a save
 - Fixed StorePlus settings menu that was broken after adding the ``DontAddToOtherList`` config item
 - Updated readme to include a list of all text variables that can be used to replace text values in your custom strings

## [3.8.3]
 - Fixed null error being thrown on load of a lobby with moonsplus disabled

## [3.8.2]
 - Added new video reel for hidden moons. Thanks & Credit to @duskwise for providing the new clip!
 - Added new config item ``DontAddToOtherList`` to StorePlus section that will allow you to filter out any external menus from showing up in the "Other" listing of StorePlus
 - Added more checks for if networking is enabled in SaveManager to prevent errors (the features that require savemanager all require networking enabled)
 - Fixed issue with MoonsPlus OTP only working when using vanilla purchase nodes
 - Hopefully fixed some inconsistencies when using ``RevealHiddenOnRoute`` with MoonsPlus
 - Fixed error being thrown when leaving the lobby due to Company building specifically having a null terminalNode
 - Fixed incorrect store prices due to a wrong assumption of using the terminalNode's itemCost value instead of the Item's creditsWorth value.
 - Added handling for terminalnodes that are not buyable items having a buyitemindex above -1
	- This caused some irregularities with prices as well
 - Added handling to always use the terminalnode with the higher price
 - Fixed issue where items added by LethalLib that were disabled still showed in the store
 - Added null checks for TextPostProcess postfix
 - Readme finally updated
	- tried to add an accurate credits section but I may have missed some peeps. Sorry if I did

## [3.8.1]
 - Fixed error being thrown in certain profiles when CruiserTerminal was not present.
 - Fixed Cruiser price getting overridden by enemy terminalnodes (football lol) and being set to 0

## [3.8.0]
 - Switched some common moonsplus menu stuff to openlib
 - Added TerminalMaxOrderedItems config item and related transpiler patches.
	- Also added compatibility support for GeneralImprovements which has a similar config item. When present, will use GeneralImprovement's config item value
 - Improved performance of all terminal transpiler patches
	- should fix the noticeable delay where the console would pause during the first transpiler 
 - Attempted to optimize the networking code from Evasia's netcode patcher.
 - Fixed travel history save key for MoonsPlus functions that need to remember your previous travel history
 - Fixed issue with MoonsPlus LLL compatibility that would break LLL's ``IsRouteHidden`` property in certain circumstances
 - Added Upgrade Unlocks reset at ship reset for Bioscan/Vitals upgrades
 - Added new MoonsPlus config items:
	- ``ShowVideoReels`` - set to false to disable the video previews for moons
	- ``AlwaysHideList`` - add a moon's numberless name to always hide it from the moonsplus menu (will not disable the route keyword)
	- ``UseVanillaPurchaseNodes`` - enable this in order to improve compatibility with mods like LethalMoonUnlocks that have patches related to loading the moon's route node. (NOTE: For some reason my code does not detect the company's route node, but I don't think this *should* be an issue)
	- ``OneTimePurchase`` - enable this to remove the route cost after routing to a moon once. *Should* reset to it's correct price after getting fired or loading a different save (Requires networking)
	- ``AffordableColor`` - set to a hexcode of your choice to set the color of moon routes that you CAN afford, leave blank to not change the color of this text.
	- ``NotEnoughCredsColor``  - set to a hexcode of your choice to set the color of moon routes that you CANNOT afford, leave blank to not change the color of this text.
 - Added StorePlus page (beta)
	- This page includes some different safeguards from purchasing too many items and should stop you from "over-purchasing" more than what the dropship can handle
	- Added direct support for purchase packs
	- Added direct support for refund command
	- Added direct support for other mod's store pages/menus (Late Game Upgrades, Too Many Emotes, Ship Inventory, etc.)
	- Added settings submenu
	- Added personal savings feature to prevent spending up to a certain credits amount
	- Settings submenu also has different sorting features
	- RespectStoreRotation configuration item determines whether or not to check the store rotation before displaying furniture/upgrades/suits in the listing
	- AffordableColor and NotEnoughCredsColor allows you to set custom colors for a store item's price
 - PurchasePacks has been updated to have less defined at run-time of the command
	- Improved purchase result page formatting
 - Updated refund command logic to use new class, should be a bit more optimized
 - In an effort to clean up the code, broke some of the larger config sections out into their own classes.
	- Should not be a noticeable difference in any way
 - Fixed SColor normal/default color names not working.
 - Fixed bind command not working for any commands that accepted input after the keyword
 - Added AutoResizeMoneyBG config item to resize the credits background box to fit the credits text inside it.
	- Will change size/position based on the credits text length.
 - Fixed MoonsPlus adding moons that were disabled in LLL's config
 - Fixed custom unlocks from this mod not resetting on ship reset
 - Fixed shortcuts working in any Interactive Terminal API menu
 - Fixed money background customizations not updating after initial load into the lobby
 - Updated terminal patch for whether or not to enable the terminal image to always enable if the node has a displayTexture property
	- I'm surprised this didn't cause issues for people before, might cause some odd issue that I didnt notice so please let me know if things act weirdly
 - Added new feature ``TerminalRunDelay`` which allows you to set a command to run on a delay.
	- The command will run after the delay is counted in seconds unless:
		- the terminal is in use
		- ship phase has changed since the delay started
		- the delayed command has been cancelled by the person who started it running a command to stop it (this feature is not networked, but networked nodes will update the node for everyone)
 - Removed system font information from debug spam
 - Patched in some additional replace-able text strings in TextPostProcess to grab game values
	- ``[leadingSpace]`` a single space
	- ``[leadingSpacex4]`` 4 spaces
	- ``[thisPlayerName]`` local player name
	- ``[thisPlayerHealth]`` local player health
	- ``[currentPlanetName]`` Moon you are currently orbiting or landed on
	- ``[GetMaxPossibleItems]`` Gets max possible items as defined by ``TerminalMaxOrderedItems`` config item 

 <details open>
 <summary>Historical Patch Notes</summary>

## [3.7.7]
 - Fixed rare issue where a moon's (dine) price was inaccurate in a specific profile.
	- This profile had LLL so I am leveraging getting the route price from LLL when it is present.
	- If anyone else experiences this issue without LLL present please report it when possible.
 - Fixed MoonsMenu not syncing for other players with networkednodes enabled
	- the moon previews will remain disabled for players not using the terminal

## [3.7.6]
 - Added a missing early return for when MoonsPlus is disabled which would still create the keyword.

## [3.7.5]
 - Added new experimental feature, MoonsPlus.
	- When enabled can either completely replace the moons catalogue or be used as a separate keyword to display an interactive listing of moons available for routing.
	- Compatibility has been added for LethalLevelLoader, LethalConstellations, and Weather Tweaks/Registry.
	- Has many configurable features for filtering/sorting. Can also be adjusted in the menus.
	- This feature is still very experimental and is default disabled.
 - publicized ShortcutBindings so that other mods can assign stopForAnyReason to stop/start the shortcut listening logic

## [3.7.4]
 - Fixed compatibility with [Route Random](https://thunderstore.io/c/lethal-company/p/stormytuna/RouteRandom/) and [RandomRouteOnly](https://thunderstore.io/c/lethal-company/p/Index154/RandomRouteOnly/)
	- issue was due to the node containing a null displayText property. Should fix compatibility with any mod that handles their terminal nodes in this manor.
	- Thanks zhenrong for troubleshooting this issue and helping identify the cause
 - Fixed issue where sometimes the Terminal's image would get enabled when it shouldn't be
	- Thanks thundershocker1 for the in-depth report
 - Fixed issue with bestiary animation videos not playing
	- Thanks crutled for the report!
 - Added handling for NRE reported by jk_5857
	- Will need to look into why this code was getting called when the miniScreen image was null, but for now it early returns if it is.
 - Simplified TerminalBeginUsing stuff (when you start using the terminal)
	- Should hopefully fix the issue reported by agitatio where the camera in minicams was getting disabled upon re-entering the terminal.

## [3.7.3]
 - Fixed CruiserTerminal ``Quit`` command compatibility
 - Fixed issue with kick command getting stuck due to confirmation logic when not actively trying to kick anyone.
 - Renamed ``CamsNeverHide`` to ``MonitoringNeverHide`` to make more sense as to what it does.
 - Added ``MonitoringDefaultView`` to set a default monitoring view to enable when the switch or previous command is used and no active monitoring is detected.
	- Set to ``None`` to display the "No Active Monitoring" message.
	- If using the vanilla "view monitor" command, setting this to anything except ``None`` will enable the view monitor radar map.
	- If set to a monitoring mode that is not currently enabled, will use the next available mode.
 - Fixed issue caused by recent updates to switch command that was intermittently causing other mods to throw NRE on lobby reload.
	- Thank you @zhenrong on discord for assisting me with troubleshooting this issue as I was unable to reproduce it myself.
	- Issue was due to the fact that I am replacing terminalNodes.specialNodes[20] with my own node that is deleted each lobby reload. Which would result in a null node in the listing at Terminal Awake.
	- To resolve the issue I am setting terminalNodes.specialNodes[20] to it's vanilla TerminalNode on terminaldisable, until it is replaced again at Terminal Start with my node.
 - Added automated formatting where, if a node's displaytext is detected without new line entries at the end, new lines will be appended so that you are typing on a new line.

## [3.7.2]
 - Switched mapscreen radar cache logic to use a getter/setter to avoid rare NREs where the cached radar has not been updated with the current game state.
	- I unforuntately was unable to replicate the error that was provided to me but this should fix it in theory.

## [3.7.1]
 - Added better handling for error that would occur in a specific user's profile when trying to reset customization for the MoneyBackground.
	- This object's color cannot be updated with the refresh customizations button in LethalConfig when this issue is encountered.
 - With LLL's recent update, you can now modify the main font size and see it update when playing with LLL!
	- Thanks IAmBatby for publicizing the cached font size variable in LLL!
 - Added support for suitsTerminal's recent terminal caret color variable.
	- Whenever this mod changes the color for the caret it will update suitsTerminal's reference to it.
 - Simplified LethalConstellations compat
 - Added a line to try and fix the vanilla issue of the terminal not being interactable (accepting input) when you start using it

## [3.7.0]
 - Reworked TwoRadarMaps compatibility and removed lots of redundant code
	- In order to use switch/previous with this mod and have it synced with other players, you will need to enable networking in *this* mod.
	- I've not found a way to get TwoRadarMaps' terminal radar to sync with other players on it's own (and to be honest I dont think it was every intended to)
 - Cached the mapscreen radar, whether it's tworadarmaps' terminal radar or the vanilla one to remove redundant code/logic
 - Added MeetsCameraEnabledConditions patch per request from Zaggy to enable the terminal radar when it is in use.
	- Once Zaggy's mods are updated along-side this, it's supposed to help with performance.
 - Updated updateMapTarget patch to invoke a subscribe-able event
 - Added/Fixed networked nodes for vanilla ``view monitor`` command
	- it should now sync between players when enabled/disabled.
	- this feature is very new and intertwined with a lot of conditional logic so if you experience issues please provide a detailed bug report with steps to reproduce
  - Fixed issue with vanilla ``view monitor`` command where it would hide after door code commands
	- Thanks pkpenguin, glacialstage, and Lunxara for the reports.
	- This was due to the terminal node for door code commands being null, causing the current node to be reloaded (which would hide the map)
	- Fix is in Openlib specifically as part of the ParsePlayerSentence patch, to not invoke the event for null terminalnodes
 - Fixed issue where TerminalStartPage config item was ignored due to an early return in codebase.
	- Thanks kaplumbava for the report!
 - Fixed issue with bind/unbind commands not being added regardless of config setup due to an unnecessary early return in codebase.
	- Thanks 6rag0n for the report!
 - Reworked key detection for shortcuts, autocomplete, history, and walkie terminal use while at the terminal
	- Now uses the update patch from OpenLib as opposed to a coroutine constantly running in the background while in the terminal.
		- Openlib has a new keydetection event that is subscribed to in this mod
	- May see some performance benefits, however, this has not been profiled in any way.
	- Holding a key down will not cause the function to repeat now. Key presses will also feel much more responsive than before.
 - Removed TerminalClock coroutine in favor of event-based system.
	- TerminalClock will now update at the same rate as the hud clock
	- Should be a performance gain now that this feature is not a continuous coroutine.
	- Clock is hidden at the start of a new day (after ship returns to orbit)
	- While in orbit if you try to run the associated clock command, it will let you know the clock cannot determine time zones from orbit
	- If for whatever reason the clock was not created, the command will let you know the component couldnt be found.
	- Tested with CruiserTerminal, unsure if this feature had worked via the prior implementation with cruiserterminal...
 - Fixed issue where upgrades (like furniture) were being added to the item count that would be sent to the dropship.
	- Thanks Lunxara for the report!
 - Delayed client networked methods to get things like the current node on join to deal with rare error (likely latency related)
	- Thanks Lunxara for the report!
 - Added handling for rare error occuring with flashlight color change logic where the player's client id was out of range
	- if your flashlight doesnt change color sometimes this is likely the reason why
 - Updated client sync methods to grab the current radar target and zoom from the host after joining.
 - Updated screen disable/enable logic to include enabling/disabling other components of the screen that may be added by this mod (clock/bodycams/custom map radar/etc.)
 - Updated some portions of the networked nodes syncing logic
 - Updated some portions of the screen settings logic
 - Fixed rare issue with previous command not going to the correct previous target
 - Updated restart command logic to reset persistant game save data
	- This should fix the issue of day counts persisting after running the command
 - Added ``FauxMoreMenu`` config item to allow for the use of faux keywords (only work when entered from a specific page) for the MoreMenu sections
	- With this addition the menu creation logic has been updated slightly to handle faux keywords as an option.
	- Thanks paradox75831004 for the reported keyword conflict issues, hope this setting helps!
 - Added ``SwitchKeywords`` config item for the switch command. If ``switch`` does not exist in this config item it will be auto-added.
 - Added compatibility for Auto-Complete and History features with commands added via LateGameUpgrades and InteractiveTerminalAPI
	- Thanks Whitespike for the help!
	- If there are other mods that are adding commands in an odd way that do not use InteractiveTerminalAPI that you would like supported by this feature please comment the mod's name and a link in Issue #37 on this mod's github.
 - Updated compatibility for CruiserTerminal for future version 1.1.0
	- If you do not have this version installed the compatibility features this mod adds will be disabled.
 - Removed failed commands from Terminal History listing.

## [3.6.8]
 - Fixed issue with vitals and bioscan upgrades persisting to different game saves. They are now added to a key in the game save.
 - Fixed issue with credits not being reduced for upgrades (bioscan/vitals) purchased.
 - Added handling for screen logic when spectating for the different possible modes.
 - Improved flashlight color handling. You no longer need to be holding a flashlight to run the command and it will now save your preference.
	- Already modified flashlights will not be changed and your helmet light color will update to any flashlight you are holding.
	- Rainbow flashlight coroutine will stop and start depending on if you have an active flashlight.
	- Setting flashlight color preference to "default" or "normal" will return the color to it's default.
 - Fixed issue with CruiserTerminal blocked command page still running the blocked command's added logic. (ie. tp still teleporting the player)

## [3.6.7]
 - Fixed issue with gamble command where input failures would cause the terminal to get stuck waiting for a confirm/deny
	- Thanks @weoneguy for the report on discord
 - Fixed/Updated tp name matching to properly take input for a player name and tp the best matching name
 - Added handling for when no names match the input provided to the better name matching used by switch & tp

## [3.6.6]
 - Added Soft-Compatibility for CruiserTerminal.
	- Commands that quit the terminal now properly quit from the CruiserTerminal
	- Added config items to permit/deny certain keywords from the cruiserterminal. Use the CruiserKeywordList as either a permit or deny list.

## [3.6.5]
 - Added config item [TerminalInputMaxChars] to adjust maximum character input for the majority of terminal nodes. Leave at -1 to make no changes.
 - Updated handling for switch command matching to a player name per request from @thephxrises on the modding discord
	- Now uses a similar method to the conflict resolution feature. 
	- I was unable to test this as much as I'd like so let me know if there's any issues.
 - Completely reworked Always-On Display handling for the different TerminalScreen settings.
	- Should have fixed issue reported by @aglitchednpc and @moroxide which was causing clients not to be able to type in the terminal sometimes.
	- Now utilizes new events in OpenLib that are called from changes detected in different Update patches.
	- Performance difference should be negligible or only positive in theory, but please report if you experience any issues.
 - Fixed issue reported by @thundershocker1 on the modding discord where the map radar would disappear on reloading "view bodycam" from OpenBodyCams in specific configurations.
 - Fixed issue in OpenLib with confirmation keywords reported by @_stormyy on the modding discord.
 - Fixed issue reported by @mmiinnaa11 on the modding discord with Home text not being customizable without enabling TerminalCustomization setting (this is only intended to disable colors)

## [3.6.4]
 - Fixed issue discovered with certain mod configurations that would brick the terminal.
	- This was related to the recent changes in 3.6.0 that added a transpiler for BeginUsingTerminal
 - Fixed various issues with cams implementations, including compatibility with suitsTerminal when both mods are using OpenBodyCams.
	- Thank you @moroxide for the help in troubleshooting this.

## [3.6.3]
 - Moved Homebrew cam stuff to OpenLib.
	- Various fixes within OpenLib for the homebrew cams.
 - Additional Compatibility with suitsTerminal (requires OpenLib update)
 - Updated to utilize latest version of OpenLib (0.2.3)
 - Additional suitsTerminal built-in compatibility
 - Added home page text customization to be refreshed with terminal color customizations

## [3.6.2]
 - Hotfix for fcolor command (thanks @chrono57 for the report and testing)

## [3.6.1]
 - Fixed issue discovered with purchase packs where if you had exactly the amount it costed you could not buy it (thanks @frostycirno for the report)
 - Added config item to disable more menu creation. When more menus is disabled any new commands will be added directly to the "Other" command listing.
 - Replaced TextPostProcess postfix for zeekers' typo with a transpiler to search for the typo and fix it only once
 - Fixed issue discovered with Store listing not updating when networkednodes is enabled. (thanks pacoito for the report & testing)
 - Updated handling for homebrew cams to use new functions in OpenLib (this will probably be moved to OpenLib in a future update)
	- Also improved visibility to remove local player arms in homebrew cams views whenever they are visible (please report any issues with this)
 - Added handling for when OpenBodyCams' view bodycam command is set to be added to the terminal.
	- This view does not have built in compatibility with my cams views so they will be disabled when this command is enabled.
 - Updated OpenLib version requirement

## [3.6.0]
 - Fixed issue of causing errors when typing nothing but spaces in the terminal.
 - Removed default videos from the mod. They will be reuploaded under their own video pack as darmuhsTerminalVideos
 - Formatting of config items has been standardized, this will result in many items resetting to default for this update
 - Automatic info commands added with OpenLib 0.2.0
 - Moved OpenBodyCams stuff to OpenLib for better standardization
	- fixed issue where mirror cam positioning would break
	- Added new config item [Mirror2DStyle] to switch between mirror camera styles
	- default mirror style is a new style, switch to 2d for previous patch version
 - Added ShipInventory compatibility for loot commands added by this mod
 - All view commands (cams, minicams, overlay, etc.) have been refactored completely
 - Updated handling of all commands to allow for multiple configured keywords
 - Removed CachedTerminalPages config item in favor of new [TerminalStartPage] config option
	- you can put any keyword here to load when entering the terminal
	- Leave blank or set to "None" to not change terminal page when entering the terminal
 - Saving last terminal input feature has been associated to new config item [SaveLastInput]
 - Switched random functions from UnityEngine to System as I've read System random is a bit better
 - Created ShipReset public event for other mods to subscribe to when lobby is fast reset (fired without cutscene)
 - Added error handling for Conflict Resolution throwing null values
 - General code cleanup and optimizations

## [3.5.10]
 - Hotfix for issue where having cached pages disabled would make it seem like they are still enabled when screen was set to always be on.
	- Issue was just that when you started using the terminal, *nothing* would load. So the last page you had would be what you'd see.
	- May adjust/simplify this feature in the future.

## [3.5.9]
 - Hotfix for default video folder config not working following 3.5.7
 - Also added new result page for when no videos are present in the folder specified by [videoFolderPath] 

## [3.5.8]
 - Added null check in Conflict Resolution for nouns that do not have a defaultVerb property which was throwing errors for some people.

## [3.5.7]
 - Added better handling of default "view monitor" command.
	- Will now display a new page for users when the keyword is not already replaced.
 - Added new config item [obcRequireUpgrade] which will disable any commands that use a bodycam until the unlock has been purchased.
	- Commands that will show a new message telling the user the bodycam upgrade is required: terminalCams, terminalMinimap, terminalMinicams, and terminalOverlay.
 - Updated handling for keeping the screen on. There shouldnt be any noticeable difference but it helps on the backend.
 - Added a simple transpiler patch to disable the vanilla behaviour of loading different nodes when you start using the terminal.
	- This mod is now fully responsible for loading a node when you first start using the terminal.
 - Fixed some issues with terminal video handling such as:
	- extra (unnecessary) folder being created for video packs
	- videos restarting when leaving/reentering the terminal causing desyncs with other players.
 - Hopefully fixed some minor desync issues with networked nodes.
 - Added config item [alwaysUniqueVideo] which will ensure a unique video is always played when running the videoplayer command.
	- When enabled, this setting will shuffle all of the videos into a list. Each time a video command is run it will play a video from the list until it reaches the end of the list. Then a new re-shuffled list will be created.
 - Added priority handling so that Store commands will always take priority over Moons routing commands with ConflictResolution enabled.
	- I can add this as a configurable in the future if requested.
 - Removed alwaysOnAtStart & alwaysOnDynamic in favor of new Quality of Life setting - [TerminalScreen]
	- this can be set to one of the following modes:
		- nochange (screen will follow vanilla behavior and remain untouched)
		- alwayson (screen will stay on at all times)
		- inship (screen will stay on while you are in the ship)
		- inuse (screen will only stay on when someone is using the terminal)
 - Renamed alwaysOnWhileDead and aodOffDelay config items to ScreenOnWhileDead and ScreenOffDelay

## [3.5.6]
 - Hotfix for minor issue with cached terminal pages

## [3.5.5]
 - Added config item [routeOnlyInCurrentConstellation] for LethalConstellations support.
	- Enabling this will make it so route random will only choose a moon from the current constellation.
 - Fixed Lever command only ever working once.
	- Thanks @slonzyjr on discord for the report.
 - Hopefully fixed cached terminal pages issues such as
	- blank page on lobby reload
	- incorrect moons/store pages when other mods modify the text in post process
 - Updated networked nodes to hopefully better handle when other mods modify a terminal page's text in post process
 - Added [StartingCreds] configuration item to modify a new game's starting credits.
	- leave at -1 to leave the game untouched, will not revert any changes that were already made.
 - Updated default TerminalCustomization colors to use the game's default values.
	- You can either revert to the default config item, leave it blank, or type "default" to revert any changes on next customization refresh.
 - Updated conflict resolution to prioritize moon/store items over any other keyword.
	- This will mean if you want to access a bestiary item or any other non-store/moon word it will likely require a more precise query.
	- If you are having any further issues with conflict resolution after this update please report them to me with logs (enable all logging configuration options and reproduce the issue)
 - Compiled for the latest version of LethalCompany.

 
## [3.5.4]
 - Updated cached terminal pages for compatibility with TerminalFormatter & other mods that change text at TextPostProcess()

## [3.5.3]
 - Added config option to disable new cached terminal pages feature (CacheLastTerminalPage) per request
 - Added config option to change Mirror zoom scale (mirrorZoom) per request
 - Updated cached terminal pages handling to not replay any sounds that were associated with the page you left

## [3.5.2]
 - Re-added method from 3.5.0 to disable view monitor command.
	- Thanks Zaggy for providing a fix for the issue on your end :)
 - Added some more configurable strings per request, (moreMenuText/moreHintText)
 - Adjusted terminal background graphic to better fill in the gaps on the screen

## [3.5.1]
 - Hotfix for compatibility with TwoRadarMaps/OpenBodyCams removing a problem method that was added in 3.5.0
	- This method was disabling the vanilla "view monitor" command. But in doing-so breaking the other mods in their entirety.
	- For the best cams experience either have all cams commands disabled to use the vanilla "view monitor" 
	- OR ensure you have the map command enabled with "view monitor" as one of it's keywords to replace the vanilla one

## [3.5.0]
 - Completely reworked terminal cams logic.
	- Adding one less image object for better optimization.
	- Disabling all cams-type commands will leave (or re-enable) the vanilla "view monitor" command
	- overall lots of optimizations with this.
 - Reworked custom background Terminal Customization.
	- Will now completely fill the screen space at all times
	- No longer covers the cams feeds from any of the cams-type commands
 - Added return to last node handling.
	- The terminal will now return to the last node you were viewing when you start using it.
	- It will also save any un-submitted text for you.
	- If another terminal user updates the terminal screen to another node your last cached node will update with it.
 - Added new config item [TerminalFillEmptyText] which allows you to add a base level of formatting from a few options for any terminalnode with text that does not fill the screen.
	- default value is nochange and will do nothing.
 - Added patch to fix zeekers typo in TextPostProcess()
 - Updated radar zoom command to allow for input. You should now be able to perform a command such as "zoom 30" to set the zoom to 30
	- this probably wont work with a multiword keyword like "radar zoom 10", I may add a fix for this later
 - Added some more compatibility stuff for suitsTerminal.
 - Added new config item [aodOffDelay] to determine seconds delay after a player is detected off the ship where it will turn off the screen
	- If a player returns to the ship within this time the screen will not turn off

## [3.4.1]
 - Updated base custom font path to look for folders in the Bepinex/Config folder.
	- This is because custom content packs cannot make folders in the base Bepinex folder.
	- See [Minecraft TerminalFont](https://thunderstore.io/c/lethal-company/p/darmuh/Minecraft_TerminalFont/) as an example for proper folder structure for a font pack.
 - Fixed issue with Terminal Resolution bonus calculator throwing errors due to bad math.
	- Thanks to nickham13 for the report and sorry for the mistake :)
 - Fixed minor visual bug with alwaysonatstart showing the incorrect credits amount until the terminal was used

## [3.4.0]
 - Updated to OpenLib v0.1.6
 - Added License file to package
 - Adjusted conflict resolution feature for better matching.
	- Will now prioritize matching that start the same as your input with a bonus score system.
	- This will fix issues like typing "ass" and getting lasso instead of assurance.
 - Fixed issue with dropship syncing (as part of the refund feature) where clients would get an extra item from their order.
	- Thank you for the report nickham13!
 - Fixed unreported issue with refund command that would let a client get more credits than the cost of the items they originally purchased.
	- This was related to the above bugfix for dropship syncing.
 - Added new Terminal Customization features:
	- You can now change the terminal font to a font specified in the [CustomFontName] config item.
	- There are 3 different ways of getting the specified font:
		- system font on your OS
		- custom fonts you've installed on your system (which go to "%LocalAppData%\Microsoft\Windows\Fonts")
		- specify a folder within the bepinex root folder space that contains font files via the [CustomFontPath] config item.
	- You can also change the font **size** for the main text, the money text at the top left, and the terminalClock text if it's enabled.
	- HUGE thank you to DiFFoZ for pointing me in the right direction for this feature and helping me debug when I ran into issues.
 - Now using OpenLib SoftCompatibility checker for soft dependencies.
 - Potentially fixed issue with AlwaysOnDynamic (untested)

## [3.3.7]
 - Fixed conflict resolution trying to return keywords with less than 3 characters.
	- another mod was adding a blank keyword (likely for an enemy bestiary registry) that was causing the terminal to resolve most shorthand input to this keyword.

## [3.3.6]
 - Fixed issue with conflict resolution trying to add the same terminalKeyword causing keyword matching to break
	- Thanks mina for the report and steps to reproduce!
 - Added more visible logging statements to players by default.
 - Added more null reference handling with better log messaging explaining what is happening
	- Attempting to load home page now if a node is detected null when starting to use the terminal

## [3.3.5]
 - Updated mod to use OpenLib version 0.1.5
	- Offloaded more game patches to OpenLib events
 - Updated incorrect config option descriptions
 - Fixed TerminalHistory issue of not creating the history keybind shortcuts
	- Thanks to pacoito for the report!
 - Improved TerminalConflictResolution keyword handling
	- Thanks again to pacoito for the reported issues!

## [3.3.4]
 - Updated mod to use OpenLib version 0.1.4
	- This should fix the issue of the NetworkingCheck failing, causing vanilla compatibility to fail.
 - Added configuration item - terminalShortcutCommands to allow for disabling the bind/unbind keywords while keeping shortcut functionality.
	- You will only be able to modify shortcuts via the configuration item, and they will only refresh between lobby loads.
 - Fixed issue where bind/unbind was getting created regardless of whether or not the terminalShortcuts feature was enabled.

## [3.3.3]
 - Fixed Manifest to target correct version of OpenLib

## [3.3.2]
 ### Fixed/Changed
 - Fixed newly introduced issue for TerminalHistory in version 3.3.0 which caused the feature to not work at all.
	- Commands were not being added to the command history list so the keybinds were not doing anything in particular.
 - Fixed issue where, with certain config files, the category menus would not update their displayText properly.
	- This issue was resolved by updating OpenLib to 0.1.3

## [3.3.1]
 ### Fixed/Changed
 - Fixed critical error that would brick the terminal with the TerminalHistory configuration item enabled.
	- Apologies for the inconvenience and thankyou to all in the modding discord who helped troubleshoot
 - Adjust more menu category to not display a category if no commands exist for it.

## [3.3.0]
 ### Added
 - More Customization!
	- Added configuration options for:
		- Money background color/alpha
		- A new custom screen background, color/alpha
		- Terminal Buttons Color
		- Terminal Keyboard Color
 - New Quality of Life Feature, Conflict Resolution
	- Similar to TerminalConflictFix, will use string comparison to provide the best matching keyword
	- Uses the Fastenshtein implementation of the Levenshtein algorithm for fast/efficient string comparison.
 - Added More command information to "other" command output.
 - Added customization refresh command to the terminal. Run this command after you modify any of the customization settings in your config.
 - Added radarZoom command to change the zoom level of the terminal radar.
	- Built-in compatibility with tworadarmaps
	- currently rotates through a range of possible zoom levels, each time you run the command.
	- may add further configurables to this in the future.
	- zoom levels will be synced with the rest of the lobby if networkedNodes is enabled.
		- Without tworadarmaps this WILL affect the mapscreen in the terminal.
 - Also added [TerminalRadarDefaultZoom] as a quality of life config setting to change the default radarzoom.
	- This will be synced at each client load-in if networkedNodes are enabled.
	- While the radarZoom command is recommended to be used alongside this feature, it is not required.
	

 ### Fixed/Changed
 - Command creation/deletion system has been offloaded to my new library "OpenLib"
	- This will allow my other mods to quickly utilize the same system without the need for continuous porting.
	- OpenLib will be open to anyone to utilize/contribute to. My only request is that it continues to support my own mods.
	- OpenLib adds new backend features such as
		- Config watching with the ManagedConfig class.
			- Allows for easily checking networking required config items, checking for features being disabled/enabled between lobbies, and associating separate config items to eachother.
		- Event system which allows my mods to not require patching into the game.
			- The library will do the patching and create events for my mods to subscribe to.
		- Extensive Terminal Keyword/Node/Store/etc. system.
			- Will ensure no conflicting keywords exist, utilize similar terminalNodes whenever possible, and clear any items added utilizing this library on lobby close.
	- While I have extensively tested the move to OpenLib, there may be some new bugs introduced with this move. If you experience any issues please let me know.
 - Fixed issue with return to cams that would throw null errors and break the terminal if only certain cams types were enabled.
	- Thank you to mmiinnaa11 on discord for the report and assistance in troubleshooting.
 - Fixed issue of throwing LogError messages when the message was more suited to be a warning or below level log message.
 - Fixed issue of not syncing cams views between players with TwoRadarMaps present.
	- while this added a networking method for switch/previous, this new method will only run when networkedNodes are enabled.
 - Fixed issue of previous not being able to cycle to the end of the player list with TwoRadarMaps present
 - Adjusted binding logic for TerminalHistory/TerminalAutocomplete binds to not throw a log error message at each load-in
 - Cleaned TerminalClock creation method up a bit
 - Adjusted restart command to sync between players. This feature will now require networking.
 - Fixed issue with StorePacks returning a null node if you could not afford them and use any keyword besides deny/confirm

## [3.2.5]
 ### Added
 - New Quality of Life feature which will keep the terminalLight on whenever the screen is on.
	- This has been added as an option for the [TerminalLightBehaviour] configuration option.
	- set [TerminalLightBehaviour] to alwayson to try this new feature!
 - Added customization items for the following:
	- [TerminalScrollbarColor] change the color of the scrollbar
	- [TerminalScrollBGColor] change the color of the background portion of the scrollbar
	- [TerminalClockColor] change the color of the clock text item that is added via [terminalClock]
	- [TerminalLightColor] change the color of the Terminal Light

 ### Fixed/Changed
 - Moved [terminalClock] to Quality of Life section and updated description.
 - Fixed recently introduced issue where alwaysondynamic was not actually disabling the terminal screen when leaving the ship if you had [alwaysOnWhileDead] disabled

	
## [3.2.4]
 ### Fixed/Changed
 - Recompiled for compatibility with v55.
 - Moved more configuration options around. Sections have been condensed.
 - Updated config option name [enemyScanCost] to [bioScanScanCost] for clarity as to what this is

## [3.2.3]
 ### Added
 - Added new Quality of Life features
	- [TerminalHistory], will store up to [TerminalHistoryMaxCount] commands that can be accessed with the up/down arrows.
		- Requires [terminalShortcuts] to function.
		- [TerminalHistoryMaxCount] is the max amount of historical commands remembered by the terminal.
	- [TerminalAutoComplete], will allow you to auto-complete to a matching keyword in the terminal by pressing [TerminalAutoCompleteKey]
		- Requires [terminalShortcuts] to function.
		- If more than one command matches the input, you can press tab to cycle through all matching commands
		- If [TerminalAutoCompleteKey] is set to Tab, will disable the Tab key quitting the terminal and update the HUD hint.
		- [TerminalAutoCompleteMaxCount] is the max amount of matching commands that autocomplete will cycle through
			- if more commands match than this number, auto-complete will not change your input to any other command.
 ### Fixed/Changed
 - Moved some config options around.
 - Updated key detection press delays to be more in-line with the new suitsTerminal defaults

## [3.2.2]
 ### Added
 - Added support for LobbyCompatibility mod.
	- Will set flags on load-in based on whether network is enabled or disabled.
		- These flags do not refresh if the networking config item is changed after loading.
 - New Section "Quality of Life"
	- This section will have simple client-side changes to the terminal.
	- Currently the two new configuration items for this section are [LockCameraInTerminal] and [DisableTerminalLight]
 - New Section "Terminal Customization"
	- This section will have client-side customization of the Terminal.
	- [TerminalCustomization] will enable or disable this entire section of customizations.
	- [TerminalColor] changes the color of the actual physical Terminal Object.
	- [TerminalTextColor] changes the color of the main text in the terminal.
	- [TerminalMoneyColor] changes the color of the current credits text in the top left of the terminal
	- [TerminalCaretColor] changes the color of the text caret in the terminal
	- Currently, the defaults are set as examples of some fun colors to change things to.
	- Host and client can have different customization settings depending on config settings.

 ### Fixed/Changed
 - Updated project to utilize plugin version number wherever possible, including compiling the assembly with the version number.
	- Now when I update the version number in one spot, it'll update it everywhere (except the changelog, keeping that manual)

## [3.2.1]
 ### Fixed
 - Clients were having their commands deleted when joining. This made the mod basically un-usable for any clients.
	- Fixed this by moving the command deletion method so that commands are only deleted when you leave a game, not when you are first loading in.
	- Thank you to @_rafael_barreto_ for the report on the discord thread for this mod.

## [3.2.0] 
 ### Added
 - Added new feature called Purchase Packs (enabled via the [terminalPurchasePacks] configuration item)
	- With this you can add customized purchase packs to the store.
	- Purchase packs and their keywords are set in the [purchasePackCommands] configuration item.
	- Syntax for this configuration item is "command:item1,item2,etc.;next command:item1,item2"
		- The ":" separates the command from the items being purchased.
		- Each item is separated by a "," just type what you would type in the terminal to buy it (ie. shov for shovel)
		- Ship Upgrades can also be added to purchase packs, like my example "PortalPack"
			- These are matched differently from standard items and will match to an upgrade that starts the same
			- so in my example "inverse" matches with "inverse teleporter" and "teleporter" matches with "teleporter"
		- Then each purchase pack is separated by a ";"
		- Failure to adhere to this syntax can result in errors
	- If you list more than 12 items in one purchase pack, the terminal will tell you the dropship can only hold 12 items.
		- From my own testing, I've been able to get 24 items from a dropship, however I do suggest keeping this warning in mind.
 - Added new configuration option [developerLogging] for my more spammy log messages.
	- This is a retroactive change following the development of the purchase packs feature having a LOT of logs.
	- If you find any log messages that show in extensiveLogging that should probably be in developerLogging please let me know :)
 - Added configurable keywords for the [terminalLobby] command to avoid conflicts with the LobbyControl mod by mattymatty
	- This will be set via the [lobbyKeywords] configuration option
 - Added the ability to configure the resolution for both potential cameras created with OpenBodyCams via this mod
	- see [obcResolutionMirror] and [obcResolutionBodyCam]
 - Added soft dependency for TerminalFormatter to detect if this mod is present and avoid double adding stuff to the store

 ### Fixed/Changed
 - Added some more null reference handling, especially when handling configuration items that can have incorrect format
 - Added "SpawnPlayerAnimation" patch for AlwaysOnDynamic coroutine rather than attempting to run it every time the ship is landed.
	- This may resolve some issues of the monitor not being on for someone who died on the last moon.
 - Fixed error being thrown when buying something quickly and this mod is paired with TerminalFormatter
 - Fixed Route Random being able to send you into debt with negative credits
	- Now if you dont have enough credits you will be told lol
 - Changed store names for VitalsPatch and BioscanPatch as they were much too long.
	- I may make this configurable in the future but for now they are the above names in the store.
 - Fixed some issues with the cameras created via OpenBodyCams for this mod.
	- This was practically a complete rework of how OpenBodyCams is utilized in this mod.
	- Added a function to remove persisting cameras between lobbys.
 - Fixed some issues with the home-brew cameras created with this mod.
 - Adjusted refund command to be more accurate, including trying to get current sales price for each item.
 - Fixed issue with StoreRotation that caused anything I added to the store to break the store with this mod enabled.
 - Fixed visual bug with Vitals Upgrade displaying wrong credits after performing the upgrade (the correct credits amount was being deducted still)
 - Updated configuration items for more consistency/clarity
	- extensiveLogging has been moved to "__Debug" section
	- Moved alwayson config options [alwaysOnAtStart], [alwaysOnDynamic], and [alwaysOnWhileDead] to new section "Always On"
	- updated [canOpenDoorInSpace] description to explain that this setting does not change actual game logic for the door in space
	- Clamped [routeRandomCost] so that you can't set it to a negative value and give yourself credits...
 - Added the rest of the home page as configuration options to allow full configuration of the home page.
	- The new configuration option is [homeHelpLines]
 - Changed some core functions in how this mod adds command to the terminal.
	- This mod will now delete any pre-existing keywords/nodes if you have closed and opened another lobby.
		- This will now make it possible to enable/disable commands between sessions without having to close the game to remove a command's functionality.
		- This will also fix an issue I noticed where multiple keywords/nodes for the same command were being created due to hosting a new lobby.
	- For multiple keywords, I am now reusing the same node rather than making a node for each keyword.
	- While I don't expect a performance difference from this optimization, it was bothering me once I noticed the issue which delayed this update a bit.


## [3.1.0]
 ### Added
 - Mod is now completely self-reliant, with only soft dependencies if you so choose to enable certain features (fovAdjust, OpenBodyCams, etc.)
	- This mod no longer uses TerminalAPI to create/implement commands. I have created my own home-brew command adding logic which I first used with ghostCodes' terminal commands.
	- This was also a decision I made to give myself more freedom in what kind of commands I can create with this mod; including adding items to the vanilla store.
	- My own command creation logic is HEAVILY influenced by TerminalAPI, so I still would like to give Atomic a huge thank you for their wonderful API.
	- If for whatever reason you experience issues with this version of the mod failing to create commands consistently, I suggest returning to version 3.0.4 which still relies on TerminalAPI.
	- Confirmation logic has also been reworked and built into the base-game terminal confirmation logic, rather than being it's own separate system.
	- This mod's command help menu (more) has also been built in as regular terminal commands.
 - You can now have as many keywords as you want for each command! (well, most of them)
	- In the default configuration for each keyword you will see examples of multiple keywords for each command.
	- Format is "keyword1; keyword two; anotherkeyword; etc"
	- I have not set a hard limit on keywords per command, however use caution when adding so many words for one action.
	- At the moment, this will also attempt to delete any other duplicate keywords. So I'd avoid making any keywords such as "store" for example.
	- For certain commands, this feature has not been implemented yet. This may change in a future update.
	- If you find any issues with this new feature please reach out on the modding discord or my github
 - Added better syncing of the terminal between players when networked terminal nodes is enabled
	- You should now have completely synced cams between players on the terminal when always-on is enabled.
	- Cams will even sync when joining another lobby where the host has enabled a cam on the terminal already.
	- The video player can also be synced if you choose, with the [videoSync] config option.
 - Improved Always On Display handling
	- Added better handling for Always On Dynamic, may see slight performance increase.
	- Adding config item to keep monitors on after death [alwaysOnWhileDead]
 - Improved Terminal Clock feature, will now display on terminal when screen is on (additional compatibility with alwayson feature introduced)
	- Improved handling of terminal 
	- feature, may see slight performance increase.
 - Improved home-brew camera creation/handling
	- Home-brew camera mask should now be up to standards with most other mods that use cameras. Thanks again to Zaggy for all the pointers.
	- Targeting system for both mirror/bodycams has been improved as well.
 - Improved teleport command handling
	- Removed check every store purchase in favor of detecting the specific teleporter instance.
 - Added teleport by name feature per request.
	- This is built on-top of the terminalTP configuration option and does not currently have it's own configuration option.
	- simply type your teleport keyword, such as "tp", followed by the player's name.
		- if a player matches the name listed, they will be teleported.
		- if no matching player is found, the command falls back to just pressing the tp button.
 - Added Route Random command per request. [terminalRouteRandom]
	- This command is part of the "fun" section
	- For your configuration fun, this command can choose from all moons or filter out moons with specific weather types via [routeRandomBannedWeather]
		- Valid weather options: None, DustClouds, Rainy, Stormy, Foggy, Flooded, Eclipsed
		- Separate each weather option by a semi-colon in the same way you would do the multiple keywords option
		- Default config is filtering out flooded, eclipsed, and foggy weather from potential routing options
	- To prevent command "abuse" where someone would keep rolling random till they got a high value moon I have added a flat rate price via [routeRandomCost]
		- You can configure this to 0 if you want this command to be free, the default is set to 100 credits.
		- You will be charged this amount of credits no matter what moon you get, even free ones.
	- Another idea I've had regarding this command, but not implemented, is making this command a one time use that refreshes every time you land.
		- If there is interest in this option, I can add a config option for this later.
 - Added networking check on mod startup. Checks if ModNetworking configuration is set to disabled (indicating this mod should work with vanilla clients)
	- Any other config items that requires networking will automatically be disabled to ensure compatibility with vanilla clients.

 ### Fixed/Changed
 - Terminal Upgrades such as bioscanpatch and vitalspatch are now properly integrated into the store.
 - Upgrades should also be synced between all players now, rather than per player individually.
 - Optimized code handling, might see slightly improved performance in longer sessions pertaining to this mod specifically.
 - Lol command has been renamed to Video for better clarity.
	- This includes any config option that referenced "lol" such as terminaLol will now say "video" ie. terminalVideo
	- It remains in the Fun section of commands
 - Updated compatibility with OpenBodyCams' latest update.
	- Resolution is now set to 1000x700 by default. Expect a configuration option for this in the future.
	- Mirror now properly utilizes OpenBodyCams to create a camera when it is detected (and with camsUseDetectedMods enabled)
	- Improved overall handling for this mods' OpenBodyCams cameras.
 - Improved Cams hiding logic for better consistency
	- Have updated the list of commands that will not hide cams views after being run
 - Reorganized Code into sectional folders for my own sanity...

	
## [3.0.4]

 ### Fixed
 - Added bioscanpatch config option to disable the patch but not bioscan itself.
	- Fixed bioscanpatch always being enabled.
 - Added compatibility for latest update of suitsTerminal
 - Added fix for this mod's commands that was found while working on suitsTerminal.
 - Added culling mask fix for mirror command that was found during suitsTerminal development.

## [3.0.3]

 ### Fixed
 - Fixed an issue when using OpenBodyCams to create a camera and having AlwaysOn disabled, the camera would freeze after exiting the terminal.
	- Camera should now be properly disabled when leaving the terminal.
	- Added extensiveLogging statements to show when the camera is disabled or enabled.

 ### Note
 - Since this update took so long I'm probably forgetting some things that have been changed/fixed/updated.
 - If you have any questions regarding this update feel free to post on github, discord, etc. I may not see it right away but I will answer once I have the time.

## [3.0.2]

 ### Added
 - Added support for multiword terminal shortcuts.
	- Multiword shortcuts will be stored regardless of whether they are valid commands or not.
	- Proper usage is: bind <key> <multiple word command>
	- Handling for multi-word shortcuts differs from single-word shortcuts.
 - Added terminal shortcuts commands to controls commands menu.

 ### Fixed
 - (hopefully) Fixed issue where pressing switch button on ship monitors without any active monitoring mode would cause odd visual bugs of seeing other player's POVs instead of your own.
 - Fixed minor compatibility issue with monitoring commands and only having the TwoRadarMaps mod on where the cams views would not update to the right target.
 - Removed *most* references to AdvancedCompany due to it being removed from Thunderstore. Compatibility functions will be added back if it does ever make it back to thunderstore.


## [3.0.1]

 ### Fixed
 - Fixed issue with menus where next page would get stuck after using any of the view commands (cams/map/minicams/minimap/overlay)
 - Fixed compatibility integration issues with OpenBodyCams
 - Fixed certain patching issues that would break the terminal. This was caused by some other mods for some people and shouldn't occur anymore.

## [3.0.0]

 ### Added/Changed
 - COMPATIBILITY UPDATES FOR: LethalLevelLoader, OpenBodyCams, and TwoRadarMaps.
 - Mod has been **completely reworked** from the ground up. Command handling is now 100% integrated within TerminalAPI, with exception to a handful of dynamic input-based commands.
 - Networked Nodes received a substantial upgrade due to this rework and now much more reliably update for all clients.
 - Keyboard shortcut functionality has been added via terminalShortcuts configuration option!
 - Shortcuts can be set using the bind command and removed using the unbind command.
 - Shortcuts are saved in config under keyActionsConfig (dont touch this unless you know what you're doing).
 - Added the ability for this mod to produce it's own camera when no bodycams mods are present.
 - Added configuration option camsUseDetectedMods to utilize any known bodycam mod's camera when they are present.
 - Currently OpenBodyCams has the most integration built into this mod, however both SolosBodyCams and Helmet_Cameras by Rick Arg should also continue to work.
 - Big performance updates for Always-On mode and other continously running mod functions. Moved as much logic out of patches as possible, to improve compatibility with other mods.
 - Added configuration option alwaysOnDynamic that will turn the screen off when you are not on the ship.
 - Majority of logging statements have been moved to a separate function. You can see these messages by enabling the extensiveLogging config option.
 - New commands that can be enabled or disabled: terminalMirror, terminalRefund, terminalRestart, terminalPrevious
 - terminalMirror is a new mirror command that uses this mod's own camera (no external mod needed!)
 - terminalRefund allows for refunding items purhcased that have yet to be delivered.
 - terminalRestart will restart the lobby without triggering the firing sequence. Will ask for confirmation unless restartConfirmOverride is set to true.
 - Added further home page customization with homeTextArt. Use this configurable string to set your own ascii art.
 - Added tpKeyword and itpKeyword options to change the shorthand keywords.
 - All commands that affect credits now require networking. This is to make credit changes more consistent and prevent any future abuse.
 
 ### Fixed
 - Fixed issue with more command not hiding cams when it should.
 - Fixed compatibility issues with LethalLevelLoader by completely moving command handling off of custom terminal events
 - Fixed numerous compatibility issues with OpenBodyCams and TwoRadarMaps. Thanks Zaggy1024 for all the help!
 - Fixed issue where teleport keywords were still being deleted even with this section of the mod disabled.
 - Fixed issue where sometimes typing "extras" for the menu option would bring up buying an extension ladder.
 
## [2.2.4] **CURRENT VERSION**

 ### Added
 - Configuration option to start with Always On Enabled (alwaysOnAtStart)
 - Added Terminal Clock feature, this can be disabled via configuration option (terminalClock) and will be on by default (does not show in orbit)
 - Added clock toggle command to toggle the new Terminal Clock feature on/off (terminalClockCommand)
 - Added new feature to use the walkie-talkie item while at the terminal. This can be disabled via configuration option (walkieTerm)
 - walkieTerm feature comes with configuration binds for both a keyboard key (walkieTermKey) and a mousebutton (walkieTermMB)
 - walkieTerm keybindings will only do something while at the terminal with a walkie in your inventory
 - Added new detailed loot command which will display all scrap onboard and their worth (terminalLootDetail)
 - Added new List Items command which will display all non-scrap items that are not currently being held on the ship (terminalListItems)

## [2.2.3]

 ### Fixed
 - Cleaned up command adding logic to be more streamlined (this is more for me than anyone else lol)
 - Adding further null exception handling when dealing with spawning the nethandler (thanks Zuploader for reporting this on GitHub)
 - Fixed fcolor to also apply to non-pro flashlights
 - No real fix action here but, the previously determined incompatible Glowsticks mod actually just needs a specific [configuration setting](https://github.com/darmuh/TerminalStuff/issues/13) in LC_API.

 ### Added
 - Added secondary link command per request
 - Removed Fov_Adjust hard dependency (this mod will still be needed for the Fov command)
 - Added the ability to use hexcodes for both fcolor and scolor (ex: fcolor 19C3A7)
 - Added rainbow flashlight command "fcolor rainbow" (could affect performance, drop your flashlight to kill the rainbow effect)
 - Added configurable hint strings (displayed in extras) for the new link commands.

## [2.2.2]

 ### Fixed
 - Fixed map command only ever saying the ship was in orbit.


## [2.2.1]
 ### Added/Changed
 - (1) NETWORKING IS NOW TOGGLE-ABLE VIA ModNetworking CONFIG OPTION
 - (1) This means you can continue to use this mod as a client-side only mod as in the past.
 - (1) I've disabled a number of commands that require networking as well as commands I dont believe should be client-side only commands when networking is disabled.
 - (2) New randomsuits command to pick a random suit to wear off the rack (ported from my suitsTerminal mod, disable this if you have that)
 - (3) Made main page you open to when opening the terminal the home page rather than help.
 - (4) Added configurable strings to the home page, leave your crewmates a message to see every time they open the terminal!
 - (5) Added link command for linking to an external web page from the terminal.
 - (6) Added configurable keywords for fcolor, scolor, gamble, lever, link, and randomsuits commands.
 - (7) As with the command, added a configurable option for what link you want to display with the link command.
 
 ### Fixed
 - (1) As mentioned above, mod can now be used as a non-host player again!
 - (2) Updated cams views to not hide cams whenever an invalid command is inputted (thanks to Sp1rit for the bug report on GitHub)
 - (3) Improved compatibility with Advanced Company & LateGame Upgrades.
 - (3) Specifically in how we all modify the help command of the terminal. It should now look much cleaner
 - (4) Fixed longstanding bug with FOV mod that would not adjust the visor with your FOV.
 - (4 Cont.) My mistake for leaving this in, i'd assumed it was an issue with the Fov mod itself and not my own implementation of it.


## [2.2.0] 
 ### Added/Changed
 - (1) NETWORKING HAS BEEN FIGURED OUT (sorta lol)
 - (1 cont.) Thanks to Evaisa for their UnityNetcodeWeaver NetcodePatcher tool and Xilophor for the very helpful wiki!
 - (2) Fcolor/Scolor commands should now sync between all clients
 - (3) Added 'networkedNodes' config option to try out the synced network nodes BETA feature
 - (3) (networkedNodes) clients will still have differing terminal instances and overall this mode mostly just tries to copy whatever the terminal user is seeing and display it on everyone else's terminal screen.
 - (3) (networkedNodes) I've labeled this as a BETA feature because i've done limited testing on it so it's bound to be buggy.
 - (4) Refactored code that has to do with switching cams pov and returning to camera screen after an event to be more modular.
 - (5) Overlay: Added configuration for Overlay Opacity. This will let you change how faded the map is on the cams view.

 ### Fixed
 - As mentioned above, flashlight colors and shiplight colors are no longer client-side only!
 - Fixed issue where switch command without a target player was overwriting switch command with a target player.
 - (hopefully) fixed an issue where switch command would bring up the wrong view.

## [2.1.1]
 ### Added/Changed
 - NEW Configuration option "camsNeverHide" to set cams views to not hide once another command is entered.
 - (camsNeverHide) if you want to be able to see the store list, help list, etc. and keep the cams view active you should set this to true.
 - (camsNeverHide) keeping this false (disabled) will hide cams views when any large text is entered but will keep them active for door codes, ping/flash radarbooster, and switch views commands.
 - I've also added a list of commands that will never hide the cams views despite either setting. I may make this configurable in the future. If interested please let me know.
 - Changed how cams/map display their views to use the same method as minimap/minicams/overlay. I can explain this in depth if anyone has questions.
 - Changed some excessive logging messages to hopefully clean up the log window a bit. There is still a lot of log messages but they are helpful when troubleshooting issues.
 - Changed when keywords get added. Shouldn't be noticeable to the average user.
 - TP/ITP: Added cooldown timer to text displayed when trying to run one of these commands while the TP itself is on cooldown.

 ### Fixed
 - Switch command should now work properly and not hide any cams/map views
 - Improved interopability with FastSwitchPlayerViewInRadar and removed a noticeable delay of their mod working only after a couple switch commands have been used.


## [2.1.0]
 ### Added/Changed
 - CHANGED CONFIG STRUCTURE AGAIN: Please delete your old config upon updating and let the mod generate a new one.
 - (Config) The update config now has which command belongs to which in-game category (if you type more).
 - CUSTOM KEYWORDS: Added a handful of configurable keywords. Take a look at the new config file to see what you can change!
 - lol: Reworked lol command to use the terminal's built in videoplayer and it will now display videos directly on the terminal!
 - scolor: added scolor command to change the color of the base lights in the ship.
 - (scolor) this command changes 3 separate light colors which are labeled "front", "middle", and "back"
 - (scolor) when using the command you specify what lights you want to change or simply type all for your selection.
 - (scolor) the third argument in this command is the color name. Like with fcolor you can see a list of these using "scolor list"
 - proview/minimap: changed keyword/name of the proview command to "minimap"
 - minicams: added inverse command of minimap where the cam is the small screen.
 - lights: finally added a command to toggle the lights in the ship
 - Always On Display: Added alwayson command that allows you to disable leaving the terminal screen on all the time or not.
 - lobby: As this was the oldest command in this mod I've updated it to be a bit more streamlined and pull from the same variable for both host/client.

 ### Fixed
 - TP/ITP: Fixed issue where you would be trying to buy either an inverse teleporter or a regular teleporter and these command would trigger instead.
 - (TP/ITP) You now won't be able to use these commands until you have a teleporter of the type required to run it.
 - From my own testing if everyone is using this mod the radar switch will be synced between players, so I've removed this as a known issue.
 - fcolor: Fixed the issue where this command would accept any color name instead of returning invalid for incorrect colors.


## [2.0.3]

 ### Added
 - fcolor: added command "fcolor list" to see what colors are available to choose from per request.
 - (fcolor) also added normal/default color keywords to set back to "normal" flashlight color (white)

 ### Fixed
 - switch: This base command was having weird interactions with all the different cams commands
 - (switch) Also a popular mod "FastSwitchPlayerViewInRadarMOD" was having some compatability issues with the new cams hooking logic
 - (switch) I've fixed all of these issues and hopefully this mod should remain compatible with any other mods that utilize the vanilla "ViewInsideShipCam 1" object name.
 - lever: Pretty sure I fixed having to pull the lever twice.
 - (lever) Turns out the reason for this was that the game requires a wait between changing levels (moons) and starting the game
 - fcolor: Reworked command so that it works without errors. As long as you have a flashlight in your inventory it will change the color.
 - (fcolor) If you have two flashlights on you it will only change the color for one. This is intended.
 

## [2.0.2]

 ### Fixed
 - heal/healme: Fixed typo in command causing heal function to not properly heal.
 - door: Fixed door command not opening the door for non-host players.

## [2.0.1] 

 ### Fixed
 - devtest: Removed my devtest command from active released patch. Luckily was linked to door command in 2.0.0 so no real damage could be done.
 - fcolor: Fixed missing null check handling case where player has never held a flashlight.
 - (fcolor) Also fixed flashlight colors not updating for your specific flashlight, was originally just looking for ANY flashlight.
 - config: Updated config description for enemyScanCost to actively reflect that it is for the "bioscan" command to avoid confusion.


## [2.0.0] 

### Added
 - MAJOR REWORK OF COMMAND HANDLING: Removed some convuluted logic in handling digits, added confirmation checks, and overall improved command to command interactions.
 - (MAJOR REWORK) This is the main reason for updating version number to 2.0.0, rather than calling this anything in 1.X.X
 - Terminal Menus: Updated start screen & help command with my own information.
 - (Terminal Menus) Added (more) command to see commands added from this mod
 - (Terminal Menus) (more) This will list 4 separate category commands to choose from (comfort) (extras) (controls) (fun)
 - (Terminal Menus) (comfort) Lists all the quality of life commands you have added.
 - (Terminal Menus) (extras) Lists all enabled commands that add extra functionality to the ship terminal. 
 - (Terminal Menus) (controls) Lists all enabled commands that give terminal more control of the ship's systems.
 - (Terminal Menus) (fun) Lists all the for-fun commands that are enabled.
 - (Terminal Menus) (comfort)(extras)(controls)(fun) All of these are also dynamic based on config options set.
 - (Terminal Menus) Added (home) command to go back to the start screen you see when first using the terminal.
 - Configurable Strings: Updated a majority of commands to have configurable strings.
 - (Configruable Strings) This will allow you to change the messages returned for most commands. Some commands will not have this feature implemented, some just haven't yet.
 - Gamble Update: Updated various elements of the gamble command
 - (gamble) Added a minimum credits requirement Config option. Set this to 0 if you want to be able to gamble it all!
 - (gamble) Added configurable Pity Mode. If enabled, this will give the biggest losers a configurable amount of "Pity Credits" (Max 60 to avoid abuse)
 - lever: Added a configuration variable for overriding new confirmation check.
 - lol: Added the option to configure a different folder to play videos from than the main plugin folder. (Credit to )
 - (lol) make sure when using your own folder it is in the Bepinex/Plugins folder.
 - itp: Added Inverse Teleport command to control the inverse teleporter from the terminal.
 - door: Added door command to control the Ship Doors from the terminal. 
 - (door) Credit to NavarroTech as I used their code for reference.
 - map: Added map keyword for "view monitor" command and removed the loadimageslowly setting so it shows instantly.
 - (map) I can add the loadimageslowly as a config option later if there is demand for it.
 - (map) Also should interface better with the other set of cams commands as of this patch.
 - proview: Added command that shows BOTH cams and radar map at the same time.
 - (proview) radarmap is put in the top right corner in a smaller box on top of the cams feed.
 - overlay: Added command that shows BOTH cams and radar map on top of eachother.
 - (overlay) Idea from @usb. on discord. Radar feed is superimposed onto the camera feed.
 - (proview/overlay) Both accomplish relatively the same thing. I may make these purchaseable upgrades in upcoming patches, for now they are only enable/disable in the config.
 - bioscan: Added a bioscanner that scans for biomatter (enemies).
 - (bioscan) Has a default configured cost of 15 credits per scan.
 - (bioscan) Can also be upgraded via bioscanpatch command.
 - bioscanpatch: Upgrades the default bioscan "software" to 2.0. Which gives more detailed information on biomatter (enemies that are alive)
 - (bioscanpatch) default configured cost of 300 credits, seemed fair to me but could use testing.
 - (bioscanpatch) Does not remove the cost requirement to run the bioscan command. I can add this option in future updates if requested enough.
 - (bioscanpatch) Like vitals, balancing is just abritralily set by me. Could use actual game testing to see what values are best.
 - vitalspatch: Purchasable Vitals Software patch that allows for vitals to be run anytime with no further cost.
 - (vitalspatch) You can configure the cost for this upgrade to whatever you want, 200 credits made sense to me.
 - (vitalspatch) If you have your original vitals command set to charge 0 credits this really doesn't need to be enabled.
 - fcolor: Added command to change flashlight color. Usage: fcolor <colorname>
 - (fcolor) there is a limited number of colors available. More can be added by request.
 - (fcolor) if requesting colors be added, please provide the rgb value of the color.
 - (fcolor) Current list of available colors: blue, cyan, green, lime, magenta, pink, purple, red, yellow, samstro, and sasstro.

 ### Changed
  - gamble: Now has a confirmation check so you can't blame me if you gamble it all away lol
  - lever: Now has a configuration check
  - heal: Updated heal logic to now say you were healed if you have less than 100 health but more than 10.

 ### Fixed
 - kick: Base game pushed out a hotfix for the kick function and I believe this has resolved any issues with the kick command in current mod.
 - (kick) will leave disabled by default as there doesn't seem to be too much demand for this feature anymore.
 - modlist: Config was not set up properly and this command was always enabled regardless of config option.

## [1.2.1] 
 ### Fixed
 - kick: Completely reworked command, was not kicking players and throwing errors in previous patch.  (only works in game version v40 and earlier)
 - (kick) Will accept as little as 3 characters of any name in the lobby and kick them now. 
 - (kick) May need to create another couple commands to kick via playerID# as players without english names are hard to kick through terminal.  
 - (kick) Also, I believe the current iteration of this command will not allow the player to join again until the lobby is remade. Requires testing. 

## [1.2.0] 
 ### Added
 - danger: checks Hazard level of moon once ship has landed.
 - vitals: checks health, weight, sanity, and flashlight charge of player being tracked by map radar.
 - vitals cost: configuration for how much credits the "vitals" command will cost to run.
 - tp/teleport: presses the teleport button from the terminal.
 - modlist: returns mods that were loaded by Bepinex and the associated version numbers.
 - kick (EXPERIMENTAL): allows the host to kick players from the lobby via terminal command. WARNING: This command is untested and therefore defaults to off at this time.
 
### Changed
 - lol: Made this command functional. Selects from an array of found videos in the plugin folder and plays a random video. 
 - (lol) You can type lol again to toggle the video off if you want to end it early. 
 - (lol) Video files should go in the "darmuh-darmuhsTerminalStuff" folder. 
 - (lol) Thanks to flipf17 for their work on TelevisionVideo, used their code for reference. 
 - Config: format has been changed, each command has it's own category now in case I want to add more variable specific commands like vitals' cost. 
 - Updated readme.md for better readability 
 
### Fixed
 - Fixed typo in lever command configuration values, thank you @glitched4772 on discord for reporting this.
 
## [1.1.2]
 
### Fixed
 - gamble: results were not updating for all players. Fixed this with setting both client/server credit values.
 - lever: added host check for when the game hasn't been started.
 
## [1.1.1]
 
 ### Fixed
 - Plugin version number was set incorrectly in 1.1.0, fixed in 1.1.1
 
## [1.1.0]
 
### Added
 - gamble: command to gamble percentage of ship credits out of 100%.
 - fov: command to change Fov from the terminal, requires Fov_Adjust mod by Rozebud.
 - lever: command to "pull the lever" from the terminal (start ship/game).
  
## [1.0.0]
 
### Added
 - lol: plays a funny video, doesn't work.
 - lobby: displays current lobby name.
 - cams: displays ship cameras to terminal screen, compatible with helmetcams/bodycams mods. Thanks to RickArg as I used their code for reference.
 - quit: quits terminal
 - loot: shows loot available on ship, thanks to tinyhoot for their ShipLoot plugin. I used their code for reference however their plugin is not required.
 - clear: clears terminal of existing text.
 - heal: Command to heal self, thanks to Thorlar for their HealthStation mod. Used their code for reference (not required for this mod).
 - Configuration system to enable/disable above commands.

  </details>
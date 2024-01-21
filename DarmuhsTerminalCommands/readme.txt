# darmuhsTerminalStuff

## Completely configurable Terminal expansion that adds up to 38 commands to the terminal!

## **For client-side only use, disable ModNetworking in config**

### Features:
 - Customizable keywords for a majority of commands
 - (BETA) Networked Terminal nodes with Always On Display will let you see what your coworkers are doing on the terminal!
 - Customizable pricing, strings, upgrades, etc.
 - Customizable home (startup) page!
 - Expanded compatibility with Advanced Company & Lategame Upgrades!
 - [NEW in 2.2.4] Configurable Terminal Clock feature that will always be displayed alongside credits!
 - [NEW in 2.2.4] Configurable Use-Walkie at Terminal feature that will allow you to bind one key and one mousebutton to use any powered walkie from your inventory!

### Comfort Commands:
 - Lobby: Display the current lobby name.
 - Quit: Exit the terminal via command rather than hitting escape or tab.
 - Clear: Clear the terminal of all text.
 - Heal: Heal yourself at the terminal.
 - Fov: Change your Fov. (requires Fov_Adjust mod by Rozebud)
 - Mods: Display a list of all loaded mods.
 - Kick (host only): Kick another player from the ship (from the terminal)
 - AlwaysOn: Keep the terminal screen on at all times.

### Controls Commands:
 - Lever: Pull the lever that controls the ship (take off/land).
 - Danger: Show current hazard/danger level of the moon you are on.
 - TP: Activate Teleporter
 - ITP: Activate Inverse Teleporter
 - Door: Control ship doors (open/closed) from terminal
 - Lights: Control ship lights (on/off) from terminal
 - Clock: Toggle Custom Terminal Clock display on/off

### Extras Commands:
 - Loot: Scans ship for all loot and gives you the total value.
 - Cams: View from the terminal that shows player pov (if helmetcams, bodycams, or another similar mod is installed)
 - Vitals: Scan player being tracked by monitor for their Health/Weight.
 - Bioscan: Scan for "non-employee" lifeforms.
 - VitalsUpgrade: Upgrades Vitals command to cost 0 credits.
 - Map: Shortened keyword for "view monitor"
 - Minimap: View from the terminal that shows camera pov with a smaller radar in the top right
 - Minicams: View from the terminal that shows radar with a smaller camera pov in the top right
 - Overlay: View from the terminal that shows camera pov with radar overlayed on top (configurable opacity)
 - Link: If enabled, allows for linking to an external website
 - Link2: If enabled, allows for linking to an external website (2)
 - Lootlist: Detailed loot command which displays all scrap items onboard and their worth.
 - Itemlist: Display a detailed list of all non-scrap items on-board that are not being held.

### Fun Commands:
 - Lol (videoplayer): Plays a random video from all videos located in the configured videoFolderPath folder.
 - Gamble: Gamble a percentage of your ship credits
 - Fcolor: Change the color of the light coming from your flashlight (networking required)
 - Scolor: Change the color of the lights inside the ship (networking required)
 - RandomSuit: Change your suit to a random suit from all suits available.

### Information Commands:
 - More: main menu of darmuhsTerminalStuff
 - Comfort: menu listing of all enabled commands in the comfort category
 - Controls: menu listing of all enabled commands in the controls category
 - Extras: menu listing of all enabled commands in the extras category
 - Fun: menu listing of all enabled commands in the fun category
 - Fcolor list: list of predefined fcolor names and usage examples
 - Scolor list: list of predefined scolor names and usage examples
 - Home: Terminal start screen

## FYI Section

### NOTES:
 - When updating this mod DELETE THE OLD CONFIG FILE and let it generate a new one on first launch with the update.
 - Please feel free to request changes or new features at my github [here](https://github.com/darmuh/TerminalStuff)
 - Also please report any bugs you find there as well.
 - If you're not a fan of GitHub, I'm also fairly active on the LethalCompany Modding Discord managed by Thunderstore. I post updates on WIP features there whenever i'm messing with something new.
 - Please report compatibility issues when you find them, it's not difficult for me to resolve these issues but I have to know about them.

### Work for future updates
 - ~~keybind to use radio while at the terminal (might run into issues with other mods here)~~
 - Integrate keybind config options into the controls menu of the game (but I don't want to add InputUtils as a dependency)
 - Help/Info command to show info on other commands, eg. "info fcolor"
 - ~~minimap/minicams/overlay: Configuration options to make these purchasable upgrades. Not highly desired but I think it'd be nice to have the option.~~ (Not going to put effort into this unless it is requested)
 - ~~minimap/minicams/overlay: Configurable Opacity levels for these.~~
 - more custom configurable keywords
 - more door commands: Purchasable upgrade for the door open/close system (maybe, this might only be possible for the host player)
 - Terminal customization (colors, etc.)
 - More fun-type commands such as roll-the-dice, expanded gambling games, etc. (maybe even tie in a wager system with the game itself)
 - ~~Add networking to the mod for various commands...~~
 

### Requested Features from Community
 - Terminal customization (colors, etc.)
 - Control terminal via chat commands (Need to identify which commands I'd want to allow through this feature, ie. no free minimap)
 - Custom home page with custom image rather than the current mask ascii art
 - ~~Option to have the terminal on at load-in~~
 - Option to have the terminal on & play a video at load-in
 - ~~Networked color commands~~
 - ~~Networked Always-On Display so everyone could see what you're doing on the terminal~~ (Now in BETA)
 - ~~An option to remove networking from the mod~~
 - ~~integrate the time of the day on the monitor~~
 - ~~feature like STOCKoverview which let you see the equipement you have in ship~~
 - ~~Make FovAdjust a soft dependency~~
 - a terminal radar zoom in/out option would be quite nice, something like zoom [value], and perhaps w/ a configurable default value
 - config option to have alwayson by default

///////////////////////////////////////////////////////////////////////

# Change Log

All notable changes to this project will be documented in this file.
 
The format is based on [Keep a Changelog](http://keepachangelog.com/).
This project does not adhere to Semantic Versioning at this time. Mostly because I refuse to learn about it.

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
 
 ### Known issues/bugs
 - (1) switch: switch command text sometimes does not update properly on first run.
 - (2) If another mod has a function that keeps the terminal screen enabled, alwayson will not be able to disable it.
 - (2 Cont.) Recommend if you run into this issue to set alwaysOnAtStart to true. 
 - (3) Picking up eachother's colored flashlights will not automatically change any colors. 
 - (3 Cont.) So your helmetlight will stay as-is and the flashlight you pick up will have the color the other player selected.
 - (3 Cont.) I'm exploring some ways to fix this without causing any potential performance issues.
 - (4) More command is not hiding cams views (and should).

 <details open>
 <summary>Historical Patch Notes</summary>

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
# darmuhsTerminalStuff

## List of Commands (current version):
 -Home (home) *info command*

 -More (more) *info command*

 -Comfort (comfort) *info command*

 -Extras (extras) *info command*

 -Tools (tools) *info command*

 -Functionality (fun, functionality) *info command*
 
 -Lobby Name (lobby, name, lobby name) *info command*
  
 -Quit (quit)

 -Door (door)
 
 -Loot (shiploot, loot)
 
 -Cams (cameras, cams)

 -ProView (proview)

 -Overlay (overlay)
 
 -lol (lol)
 
 -Clear (clear)
 
 -Heal (heal, healme)
 
 -Fov (fov <#>)
 
 -Gamble (gamble <#>)
 
 -Lever (lever)
 
 -Danger (danger) 
 
 -Vitals (vitals <playername>)
 
 -VitalsPatch (vitalspatch)

 -Inverse Teleport (itp, inverse)

 -Teleport (tp, teleport)
 
 -Modlist (modlist) *info command*
 
 -Kick (kick <playername>)

 -BioScan (bioscan)

 -BioScanPatch (bioscanpatch)

 -Flashlight Color (fcolor "colorname")

 -Flashlight Color List (fcolor list) *info command*


# Change Log

All notable changes to this project will be documented in this file.
 
The format is based on [Keep a Changelog](http://keepachangelog.com/).
This project does not adhere to Semantic Versioning at this time.
 
 
## FYI Section

### NOTES:
 - Highly recommend when updating this mod deleting the old config file and letting it generate a new one on first launch with the update.
 - Please feel free to request changes or new features at my github [here](https://github.com/darmuh/TerminalStuff)
 - Also please do report any bugs you find there as well.
 - If you're not a fan of GitHub, I'm also fairly active on the LethalCompany Modding Discord managed by Thunderstore. I post updates on WIP features there whenever i'm messing with something new.
 - Flashlight colors are CLIENT SIDE ONLY. You will not see other player's color change.
 - If your flashlight color isn't changing try the following. (1) Make sure you have a flashlight in your inventory. (2) Turn on the flashlight and switch to something else to also tell the game you have a helmet light.

### Work for future updates
 - lol: think I might revist trying to get videos to display on the terminal itself after some findings in developing the splitview commands.
 - proview/overlay: Configuration options to make these purchasable upgrades
 - more door commands: Purchasable upgrade for the door open/close system (maybe, this might only be possible for the host player)
 - Ship Light controls and color change commands
 - Terminal customization (colors, etc.)
 - Terminal screen Always-On (does not turn black after leaving)
 - Custom keywords for commands (need to see if this is possible first)
 - Networked flashlight colors if I ever figure out custom networking for this mod :)

 

### Requested Features from Community
 - ~~late join command: a feature similar to latecompany that can be toggled~~
 - Ship Lights (on/off)
 - Terminal customization (colors, etc.)
 - Terminal Always-On display
 - Custom keywords for commands
 - Control terminal via chat, believe this has already been done by another mod

## [2.0.3] *CURRENT VERSION*

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


 ### Known issues/bugs
 - fcolor: Flashlight colors are only client side. Fixing this would require adding networking to this mod.
 - (fcolor networking) This probably won't be implemented any time soon. I've done some internal testing but still running into issues with this implementation.
 - radar switch in terminal is not synced with other players (client only)
 <details open>
 <summary>Historical Patch Notes</summary>

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

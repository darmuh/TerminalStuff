# darmuhsTerminalStuff

## List of Commands (current version):
 -Lobby Name (lobby, name, lobby name)
  
 -Quit (quit)
 
 -Loot (shiploot, loot)
 
 -Cams (cameras, cams)
 
 -lol (lol)
 
 -Clear (clear)
 
 -Heal (heal, healme)
 
 -Fov (fov <#>)
 
 -Gamble (gamble <#>)
 
 -Lever (lever)
 
 -Danger (danger)
 
 -Vitals (vitals <playername>)
 
 -Teleport (tp, teleport)
 
 -Modlist (modlist)
 
 -Kick (kick <playername>) *Must be host*

# Change Log
All notable changes to this project will be documented in this file.
 
The format is based on [Keep a Changelog](http://keepachangelog.com/).
This project does not adhere to Semantic Versioning at this time.
 
## Planned features/ideas

### Work for future updates
 - Split view for cams/radar to show both simultaneously? Maybe impossible?
 - Enemies: Scan for enemy count, may implement config values for varying degrees of information availability.
 - Change flashlight Color: Still haven't looked into this yet, seems possible but may run into issues.
 - Control ship doors from terminal
 - Update lol command to allow configuration of videos folder path.

### Requested Features from Community
 - flashlight colors 
      part of my planned features to look into above.
 - configuration for the Gamble Command. Like win/lose chances. Limitations for how much you can gamble or floors.
      this will likely require reworking the gamble commmand a bit. may work on & implement this in a future update.
 - a feature similar to latecompany that can be toggled? 
      have not looked into this whatsoever, probably not possible.
 

## [1.2.0] **CURRENT VERSION** 
 ### Added
 - danger: checks Hazard level of moon once ship has landed.
 - vitals: checks health, weight, sanity, and flashlight charge of player being tracked by map radar.
 - vitals cost: configuration for how much credits the "vitals" command will cost to run.
 - tp/teleport: presses the teleport button from the terminal.
 - modlist: returns mods that were loaded by Bepinex and the associated version numbers.
 - kick (EXPERIMENTAL): allows the host to kick players from the lobby via terminal command. WARNING: This command is untested and therefore defaults to off at this time.
 
### Changed
 - lol: Made this command functional. Selects from an array of found videos in the plugin folder and plays a random video.
       You can type lol again to toggle the video off if you want to end it early.
	   Video files should go in the "darmuh-darmuhsTerminalStuff" folder.
	   Thanks to flipf17 for their work on TelevisionVideo, used their code for reference.
 - Config format has been changed, each command has it's own category now in case I want to add more variable specific commands like vitals' cost.
 -Updated readme.md for better readability
 
### Fixed
 - Fixed typo in lever command configuration values, thank you @glitched4772 on discord for reporting this.

### Known issues/bugs
 - Certain commands could use a confirm/deny feature such as gamble, lever, and vitals.
      I have not figured how how to get the terminal to request confirm/deny from the user just yet.
 - Lever seems to require two pulls from terminal to start the game in some online lobbies, not sure why this happened need info from log events.
 - WARNING: Kick is an untested command. There is a chance this command does not actually kick anyone. There is also a chance once you kick someone they'll be unable to join back. Treat this command with caution and please feel free to report bugs to either the github link or in the Lethal Company modding discord. 


 
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

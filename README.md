# TerminalStuff
Mod for Lethal Company that adds my terminal commands

# darmuhsTerminalStuff

1.1.2 Update: 
-Fixed Gamble to update gamble results to everyone in the server.
-Fixed Lever command behavior prior to game start, only host should be able to pull it.
-Updated Gamble logic to be more random. Was originally a static coinflip.


List of current Commands:
Lobby Name
Quit
Loot
Cams
lol (not working)
Clear
Heal
Fov (requires Fov_Adjust mod by Rozebud)
Gamble
Lever

Detailed List of current commands and their compatible nouns:
**Lobby Name**: Get's the Name of the current lobby
-lobby
-lobby name
-name lobby (not sure about this combination, both words are designated as verbs)

**Quit**: Quit's terminal in the same way esc or tab will
-exit
-quit

**Loot**: Get's current value of all loot on ship (thanks to tinyhoot for their ShipLoot plugin. I used their code for reference however their plugin is not required.)
-loot
-shiploot

**Cams**: Displays the Camera View of the other monitor in the ship. (Works with the Helmet Cameras mod by Rick Arg. I also used this code for reference so thank you!)
-cams
-cameras
*Note, works in the same way as view monitor. 
*FYI works with both currently available player cam mods. Both Helmet Cameras by Rick Arg and Solo's Body Cameras

**lol**: Command to display a funny video from the web on the monitor, doesn't work at the moment.
-lol
-hampter
*Just a fun thing I wanted to get working to surprise some friends. Having issues with Unity's play video from URL at the moment. Might try loading it as an asset and seeing if that works.
*disabled in config by default as this doesn't work.

**Clear**: Command to clear screen.
-clear

**Heal**: Command to heal self.
-heal
-healme
*Thanks to Thorlar for their HealthStation mod. Used their code for reference (not required for this mod)

**Fov**: Command to change FOV from terminal
-fov
*Usage: Fov <number between 66 and 130.
**Requires Fov_Adjust mod by Rozebud

**Gamble**: Command to gamble credits by percentage
*Usage: gamble <number between 1 and 100>
**Does not count towards quota, this is only credits that you can use to buy things

**Lever**: Command to pull the lever from terminal.
**WARNING: This does not ask the user to confirm whether they want to pull the lever or not. I want to add this later but haven't quite figured it out yet.




**Planned command not yet implemented:
Split view for cams/radar to show both simultaneously? Maybe impossible?
Teleport: I know this has already been done but wouldn't hurt to create this for my plugin anyway?
Enemies: Scan for enemy count, maybe makes the game too easy?
Ally Status: Check health/weight/battery status of all players, also might make the game too easy? Maybe implement a scan only for target player and range within vicinity?
Danger Level: Game tells you this at the beginning so I don't think this is too bad to add.
Change flashlight Color: Unsure how i'd implement this, might not be possible?

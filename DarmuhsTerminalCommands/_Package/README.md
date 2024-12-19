# darmuhsTerminalStuff

## **For client-side only use, disable ModNetworking in config (found in the "Networking" section)**

### Completely configurable Terminal expansion mod! 40+ new commands, 5+ new features, QoL fixes, and much more

### Features:
 - **Near infinite** customizable keywords for a majority of commands
 - MoonsPlus (experimental): Interactive moons catalogue menu within the terminal. Compatible with LethalLevelLoader & LethalConstellations
 - Networked Terminal nodes with Always On Display will let you see what your coworkers are doing on the terminal! **NOW WORKS WITH CAMS AND VIDEOS**
 - Customizable pricing, strings, upgrades, etc.
 - Customizable home (startup) page! **NOW WITH THE ABILITY TO ADD YOUR OWN ASCII ART IN UPDATE 3.0.0**
 - Configurable Terminal Clock feature that will always be displayed alongside credits!
 - Configurable Use-Walkie at Terminal feature that will allow you to bind one key and one mousebutton to use any powered walkie from your inventory!
 - Terminal Shortcuts can be set using the bind command and removed using the unbind command! These binds will be saved in the config for continued use.
	- Bind/Unbind commands can be disabled via terminalShortcutCommands
 - Configurable "Purchase Packs" that let you buy multiple items from the store with one command!
 - Client-side Terminal Customization, change the color of the terminal and the different text types!
 - Client-side Quality of Life features, make the terminal experience how you want it to be!
 - Set the terminal screen/light to stay on all the time, only when you are in the ship, or only when someone is using the terminal!
 - Terminal Autocomplete and History features to make typing in the terminal feel more like a real terminal!
 - Terminal Conflict Resolution will compare your input to all keywords to give you the best result.
	- Using the [Fastenshtein string comparison algorithm](https://github.com/DanHarltey/Fastenshtein)
 - Change the terminal font and font size!
	- Added support for custom font packs, see [Minecraft TerminalFont](https://thunderstore.io/c/lethal-company/p/darmuh/Minecraft_TerminalFont/) as an example
 - Your last terminal session will save the page you were on and any text you had typed in.
 - Adjust starting credits to any value from 0 to 20000! (Host Only)
 - Cams commands can now show player povs even without a separate bodycam mod present.
	- Still compatibile with every major bodycam mod; OpenBodyCams, Helmet Cameras, and Solos Bodycams are all compatible!
 - Expanded compatibility with LethalLevelLoader, OpenBodyCams, and TwoRadarMaps!
	- Will now check if BodyCam upgrade has been purchased when using OpenBodyCams cameras!
 - Loot commands have added compatiblility with ShipInventory
 - Special config items for CruiserTerminal compatibility!
	- Choose which commands you'd like to work with the CruiserTerminal!

### Comfort Commands:
 - Lobby: Display the current lobby name.
 - Quit: Exit the terminal via command rather than hitting escape or tab.
 - Clear: Clear the terminal of all text.
 - Heal: Heal yourself at the terminal.
 - Fov: Change your Fov. (requires Fov_Adjust mod by Rozebud)
 - Mods: Display a list of all loaded mods.
 - Kick (host only): Kick another player from the ship (from the terminal)
 - AlwaysOn: Adjust whether the terminal screen should stay on (will not change TerminalScreen QoL setting)

### Controls Commands:
 - Bind/Unbind: Commands used to set terminal shortcuts (enabled/disabled by terminalShortcutCommands)
 - Lever: Pull the lever that controls the ship (take off/land).
 - Danger: Show current hazard/danger level of the moon you are on.
 - TP: Activate Teleporter, can also target a specific player by typing their name after
 - ITP: Activate Inverse Teleporter
 - Door: Control ship doors (open/closed) from terminal
 - Lights: Control ship lights (on/off) from terminal
 - Clock: Toggle Custom Terminal Clock display on/off
 - Restart: Reset lobby without triggering firing sequence (networking required)
 - Radar Zoom: Change zoom level of the map from view monitor (will affect Map on the monitor if not using TwoRadarMaps)

### Extras Commands:
 - Loot: Scans ship for all loot and gives you the total value.
 - Cams: View from the terminal that shows player pov
 - Vitals: Scan player being tracked by monitor for their Health/Weight. (networking required)
 - Bioscan: Scan for "non-employee" lifeforms. (networking required)
 - VitalsPatch: Upgrades Vitals command to cost 0 credits. (networking required) **Store Item
 - BioscanPatch: Upgrades Bioscan command to display even more information about enemies in the facility (networking required) **Store Item
 - Map: Reworked version of "view monitor", cooperates better with the other cams views and can be any configurable keyword
 - Minimap: View from the terminal that shows camera pov with a smaller radar in the top right
 - Minicams: View from the terminal that shows radar with a smaller camera pov in the top right
 - Overlay: View from the terminal that shows camera pov with radar overlayed on top (configurable opacity)
 - Link: If enabled, allows for linking to an external website
 - Link2: If enabled, allows for linking to an external website (2)
 - Itemlist: Display a detailed list of all non-scrap items on-board that are not being held.
 - Lootlist: Detailed loot command which displays all scrap items onboard and their worth.
 - Mirror: View your surroundings from the terminal (uses camera internal to this mod)
 - Refund: Cancel an undelivered order and get your credits back. (networking required)
 - Previous: As opposed to the switch command, will switch to the previous target.

### Fun Commands:
 - VideoPlayer: Plays a random video on the terminal from all videos located in the configured videoFolderPath folder.
	- As of 3.6.0, requires a separate TerminalVideoPack.
 - Gamble: Gamble a percentage of your ship credits (networking required)
 - Fcolor: Change the color of the light coming from your flashlight (networking required)
 - Scolor: Change the color of the lights inside the ship (networking required)
 - RandomSuit: Change your suit to a random suit from all suits available.
 - Route Random: Route to a random moon for a flat rate! Completely configurable.
 - Refresh Customization: Refresh terminal customizations from config settings.

### Information Commands:
 - More: main menu of darmuhsTerminalStuff
 - Comfort: menu listing of all enabled commands in the comfort category
 - Controls: menu listing of all enabled commands in the controls category
 - Extras: menu listing of all enabled commands in the extras category
 - Fun: menu listing of all enabled commands in the fun category
 - Fcolor list: list of predefined fcolor names and usage examples
 - Scolor list: list of predefined scolor names and usage examples
 - Home: Terminal start screen (the one with the art)

## FYI Section

### NOTES:
 - Please feel free to request changes or new features at my github [here](https://github.com/darmuh/TerminalStuff)
 - Also please report any bugs you find there as well.
 - If you're not a fan of GitHub, I'm also fairly active on the [LethalCompany Modding Discord](https://discord.gg/XeyYqRdRGC) managed by Thunderstore.
 - Please report compatibility issues when you find them, it's not difficult for me to resolve these issues but I do have to know about them.
 - This is the first of many of my mods to start using my Open-Source libary: OpenLib. This will be a dependency for version 3.3.0 and forward.

### Work for future updates & Requested Features from Community
 - Find an up-to-date listing [here](https://thunderstore.io/c/lethal-company/p/darmuh/darmuhsTerminalStuff/wiki/1277-planned-work-community-suggestions/)
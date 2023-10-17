## Universal Portals
This mod allows the user to use the portals without the need to link them by name, creating the possibility to link any portals, there are two settings, dynamic portals, and static portals. In the Details tab of this mod is well explained

Compatible with multiplayer and dedicated servers **(mod required in client and server side)**

Requires [BepInEx](https://github.com/BepInEx/BepInEx) or [InSlimVML](https://github.com/PJninja/InSlimVML)

## Installation:
* Download and extract the zip file
* Copy the contents of the zip in "steamapps\common\Valheim\BepInEx\plugins" or "steamapps\common\Valheim\InSlimVML\Mods"

## Usage

Currently there are two settings:
* Dynamic portals: Every time upon entering a portal you will be asked to select a destination portal on the map
* Static portals: Static portals won't ask for destination upon entering, unless they have no target

You can also use this shortcuts:
* Shift + E: To select a default destination of a portal
* Alt + E: To remove the default destination of a portal

## Configuration:
Alongside with the mod (the dll file), there is a json file with the configuration of the mod, the current possible setting are these:
* ShowMarkersOnMapSelection (default false): 
  * When this value is set to true the map will always show the player map markers, even when selecting a portal destination. This can be set to false to have no map markers when selecting a portal destination, and have a clearer view of the map
* SaveLastSelection (default false):
  * When this value is set to true all the portals will be static portals, the ones that have no target will ask the first time for a destination portal, and then save it

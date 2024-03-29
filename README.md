# What's Chaos Mode?
Chaos Mode is a Risk of Rain 2 Artifact mod that adds, well... Chaos. Enabling the mod causes bosses and swarms to spawn on a timer. Items are randomly dispersed, chests are randomized and events can be triggered that drastically change the way the game is experienced. Server-side and vanilla-compatible (in most cases).

# Compatibility
As of patch 2.1.0 Chaos Mode works with the `Survivors of The Void Expansion!`

# Multiplayer
As of patch 2.2.0 multiplayer is a bit weird. If you want to use this mod in multiplayer you essentially have two options.
+ Make sure everyone is running a modded client with the same version of the mod installed. Everything should work correctly.
+ If anyone is playing unmodded, make sure the host has the mod enabled and set the config setting 'ArtifactMode' to 'False.' The mod will automatically run in the background.
	+ You cannot currently enable/disable the mod this way without resetting the game (Working on a fix for this).

# Configuration
All settings are intended to work at their default. Raising them can cause unnecessary lag or crashing (especially when combined with the Survivors of The Void DLC). Adjust carefully.

### General Settings
Setting | Range | Default | Effect
---|---:|---:|---
ArtifactMode | True/False | True | Enables/disables the mod as an artifact. Turn off if playing multiplayer with unmodded clients.
StartingItems | True/False | True | Gives items to all players at the start of each Run.
GiveItems | True/False | True | Enables or disables free items when an event triggers.
RandomShrines | True/False | True | Enables or disables all drop types for Shrines of Chance.
RandomSpecialChests | True/False | False | Enables or disables all drop types for Equipment Barrels, Lunar Pods, Legendary Chests and Void Chests.
IncludeLunar | True/False | False | Enables/disables Lunar items being included in the pool of random items during events.

### Run Settings
Setting | Range | Default | Effect
---|---:|---:|---
ChaosSpeed | 15 - 6000 | 60 | Time in in-game seconds between enemy and event triggers. Limited to every 15 seconds.

### Event Settings
Setting | Range | Default | Effect
---|---:|---:|---
EventRate | 0 - 100 | 15 | Raises the likelyhood that special "Chaos Events" are triggered.
EnableOrder | True/False | False | Enables or disables the order event wherein your items are redistributed.
PurgeRate | 0 - 10 | 5 | Limits how many items a Purge Event can take from players. Always leaves you with at least 3 items.

### Enemy Settings
Setting | Range | Default | Effect
---|---:|---:|---
SpawnLimit | True/False | True | Limits the amount of enemies that the mod can spawn. WARNING! TURNING THIS OFF CAN CAUSE LAG OR CRASHES!
MaxEnemies | 1 - 50 | 20 | Maximum amount of enemies that can spawn at once. Ignored if SpawnLimit is 'False'.
SwarmAggression | 1 - 5 | 1 | Multiplies the average number of enemies spawned in swarms.
SwarmRate | 0 - 100 | 35 | Raises the likelyhood of spawning low-tier swarms.
AmbushRate | 0 - 100 | 30 | Raises the likelyhood of spawning high-tier boss enemies.
EliteRate | 0 - 100 | 50 | Raises the likelyhood of spawning elite enemies.

### Item Rates
Setting | Range | Default | Effect
---|---:|---:|---
CommonRate | 0 - 100 | 20 | Raises the likelyhood that a common item will be rolled.
UncommonRate | 0 - 100 | 15 | Raises the likelyhood that an uncommon item will be rolled.
LegendRate | 0 - 100 | 10 | Raises the likelyhood that a legendary item will be rolled.
BossRate | 0 - 100 | 10 | Raises the likelyhood that a boss item will be rolled.
LunarRate | 0 - 100 | 10 | Raises the likelyhood that a lunar item will be rolled.
CorruptRate | 0 - 100 | 10 | Raises the likelyhood that a corrupted item will be rolled. (Requires Survivors of The Void)

# Contact
If you have questions or problems, you can leave them in the [issues](https://github.com/bryantBaumgartner/chaosMode/issues) or reach out to me on Discord at `Pocket#4156` either through DMs or the [RoR2 Modding Discord](https://discord.gg/JDbYRZCGbs). I'm pretty good about responding to messages.

# Changelog
2.5.0
+ Primarily cleaned up code.
+ Prep work for inevitable Seekers of the Storm update.

2.4.0
+ Balanced difficulty to make the early-game more fair.
+ Added a setting to toggle randomized drops for special chest types.
+ Added a setting to control whether Lunar items can be added to your inventory from events.

2.3.0
+ Randomized Shrines of Chance to include all drop types (based on drop settings). Guarantees drops when interacting. (May cause conflicts with other Shrine mods)
+ Added weighting to Elite types.
+ Added a setting to toggle randomized Shrines of Chance.
+ Added a setting to toggle random starting items.

2.2.4
+ Optimized multiplayer for vanilla clients.
+ Fixed a bug causing infinite spawns in the bazaar.
+ Added a new setting to enable/disable item dispersal during events.

2.2.2
+ Added a new setting to enable/disable Artifact mode.

2.2.1 
+ Added a new setting to enable/disable the Sequencing event.

2.2.0 
+ ChaosMode is now an Artifact (Artifact of ChaosMode)!

2.1.1
+ Fixed an issue with one of the events.
+ Changed drop table for Corrupted items.
+ Reworked Elite system and added new Elite types: Mending, Void. (required Survivors of the Void)
+ Optimized code. (it was very old lol)

2.1.0
+ Vanilla-compatible.
+ Fixed an indexing issue with the item system. 
+ Added new enemies and events. 
+ Reworked enemy spawn system. 
+ Changed and added new settings (this may require deleting and remaking your config file).

Earlier version details can be found on the [Github page](https://github.com/bryantBaumgartner/chaosMode).

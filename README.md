# What's Chaos Mode?
Chaos Mode is a Risk of Rain 2 Artifact mod that adds, well... Chaos. Enabling the mod causes bosses and swarms to spawn on a timer. Items are randomly dispersed, chests are randomized and events can be triggered that drastically change the way the game is experienced. Server-side and vanilla-compatible (in most cases).

# Compatibility
As of patch 3.0.0 Chaos Mode works with the `Seekers of the Storm Expansion!`
As of patch 2.1.0 Chaos Mode works with the `Survivors of The Void Expansion!`

# Multiplayer
In order to use the mod in multiplayer you can either
+ Ensure all clients in the lobby have matching versions of the mod.
+ Set the config setting `ArtifactMode` to `False` on the host client. The mod will automatically run in the background.
	+ You cannot currently disable the mod with this method without resetting the game.

# Configuration
All settings are intended to work at their default. Raising them can cause unnecessary lag or crashing (especially when combined with the DLC Expansions). Adjust at your own risk.

### General Settings
Setting | Range | Default | Effect
---|---:|---:|---
ArtifactMode | True/False | True | Enables/disables the mod as an artifact. Set to False if playing multiplayer using any unmodded clients.
StartingItems | True/False | True | Whether to give 5 items to all players at the start of each run.
GiveItems | True/False | True | Whether to give a random item whenever the mod triggers events or enemies.
RandomShrines | True/False | True | Whether Shrines of Chance can drop any tier of item (configurable).
RandomSpecialChests | True/False | False | Whether Equipment Barrels, Lunar Pods, Legendary Chests and Void Cradles can drop any tier of item (configurable).
IncludeLunar | True/False | False | Whether Lunar items are included in the pool of random items.

### Run Settings
Setting | Range | Default | Effect
---|---:|---:|---
ChaosSpeed | 15 - 6000 | 60 | Interval that enemy spawns and events trigger. Limited to at least 15 seconds.

### Event Settings
Setting | Range | Default | Effect
---|---:|---:|---
EventRate | 0 - 100 | 15 | The weighted likelyhood that special events are triggered.
EnableOrder | True/False | False | Whether an event can occur that redistributes all your items like the Shrine of Order.
PurgeRate | 0 - 10 | 5 | How many items can be taken from each player during a Purge event. It will never leave you with less than 3 items.

### Enemy Settings
Setting | Range | Default | Effect
---|---:|---:|---
SpawnLimit | True/False | True | Limits the amount of enemies that the mod can spawn. Recommended to be True if playing on a slower machine.
MaxEnemies | 1 - 50 | 20 | The maximum amount of enemies that the mod can spawn. Will stop once this limit is reached. Ignored if SpawnLimit is `False`.
SwarmAggression | 1 - 5 | 1 | Multiplies the average number of enemies spawned in swarms.
SwarmRate | 0 - 100 | 35 | The weighted likelyhood that swarms will spawn.
AmbushRate | 0 - 100 | 30 | The weighted likelyhood that bosses will spawn.
EliteRate | 0 - 100 | 50 | The weighted likelyhood that elite enemies will spawn.

### Item Rates
Setting | Range | Default | Effect
---|---:|---:|---
CommonRate | 0 - 100 | 20 | The weighted likelyhood that a common item will be rolled.
UncommonRate | 0 - 100 | 15 | The weighted likelyhood that a uncommon item will be rolled.
LegendRate | 0 - 100 | 10 | The weighted likelyhood that a legendary item will be rolled.
BossRate | 0 - 100 | 10 | The weighted likelyhood that a boss item will be rolled.
LunarRate | 0 - 100 | 10 | The weighted likelyhood that a lunar item will be rolled.
CorruptRate | 0 - 100 | 10 | The weighted likelyhood that a void item will be rolled. (Requires Survivors of The Void)

# Contact
If you have questions or problems, you can leave them in the [issues](https://github.com/bryantBaumgartner/chaosMode/issues) or reach out to me on Discord at username `pocket_squid`, display name `Pocket`, formerly `Pocket#4156` either through DMs or the [RoR2 Modding Discord](https://discord.gg/JDbYRZCGbs). I'm pretty good about responding to messages.

# Changelog
3.0.0
+ Seekers of the Storm Compatibility! 
+ New enemies can spawn.
+ New events added.
+ Removed Voidtouched Mithrix event due to bug.

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

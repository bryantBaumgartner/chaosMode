# Compatibility
As of patch 2.1.0 Chaos Mode works with the `Survivors of The Void Expansion` AND is vanilla-compatible!

# What's Chaos Mode?
Chaos Mode is a Risk of Rain 2 mod that adds, well... Chaos. Bosses and swarms spawn on a timer and items are randomly dispersed all the time. Chest are randomized and events can be triggered that drastically change the way the game is experienced. Only required by the host in multiplayer (with the right mod installed).

# Configuration
All settings are intended to work at their default. Raising them can cause unnecessary lag or crashing (especially when combined with the Survivors of The Void DLC). Adjust carefully.

Setting | Range | Default | Effect
---|---:|---:|---
ChaosSpeed | 15 - 60 | 60 | Time in seconds before Chaos enemies spawn. Caps at every 15 seconds.
CommonRate | 0 - 100 | 20 | Raises the likelyhood that a common item will be rolled.
UncommonRate | 0 - 100 | 15 | Raises the likelyhood that an uncommon item will be rolled.
LegendRate | 0 - 100 | 10 | Raises the likelyhood that a legendary item will be rolled.
BossRate | 0 - 100 | 10 | Raises the likelyhood that a boss item will be rolled.
LunarRate | 0 - 100 | 10 | Raises the likelyhood that a lunar item will be rolled.
CorruptRate | 0 - 100 | 10 | Raises the likelyhood that a corrupted item will be rolled. (Requires Survivors of The Void)
SwarmRate | 0 - 100 | 35 | Likelyhood of spawning low-tier swarms.
EventRate | 0 - 100 | 15 | Rate at which special "Chaos events" are triggered.
AmbushRate | 0 - 100 | 30 | Likelyhood of spawning high-tier boss enemies.
EliteRate | 0 - 100 | 50 | Likelyhood of spawning elite enemies.
SpawnLimit| True/False | True | Limits the amount of enemies that the mod can spawn. WARNING! TURNING THIS OFF CAN CAUSE LAG OR CRASHES!
MaxEnemies | 1 - 50 | 20 | Maximum amount of enemies that can spawn at once. Ignored if SpawnLimit is 'False'.
SwarmAggression | 1 - 5 | 1 | Multiplies the average number of enemies spawned in swarms.
PurgeRate| 0 - 10 | 5 | Limits how many items a Purge Event can take from players. Always keeps at least 3 items.

# Contact
If you have questions or problems, you can leave them in the [issues](https://github.com/bryantBaumgartner/chaosMode/issues) or reach out to me on Discord at `Pocket#4156` either in DMs or the [RoR2 Modding Discord](https://discord.gg/JDbYRZCGbs).

# Changelog
2.1.1 - Fixed an issue with one of the events. Changed drop table for void items. Reworked Elite system and added new Elite types: Mending, Void. Cleaned up behind-the-scenes code.

2.1.0 - Vanilla-compatible. Fixed an indexing issue with the item system. Added new enemies and events. Reworked enemy spawn system. Changed and added new settings (this may require deleting and remaking your config file).

2.0.5 - Fixed an issue that broke the mod when distributing Equipment to players.

2.0.4 - Fixed an issue where items were distributed 0 times.

2.0.3 - Updated dependancies (Whoops).

2.0.2 - Fixed a bug that prevented Elite enemies from spawning. Added new Elite type: Perfected. Fixed a bug that activated multiple events at once. Reworked chest drop system. Added new settings.

2.0.1 - Compatibility with `Survivors of The Void` DLC. Items can spawn as Corrupted Items whenever rolled. No longer vanilla-compatible.

1.0.3 - Fixed elite enemy bug. Chest drops are balanced and randomized to include Boss and Lunar items. Added new settings.

1.0.2 - Enemies can now spawn as Elites.   

1.0.1 - Initial Release

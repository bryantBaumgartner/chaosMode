# Now Compatible with `Survivors of The Void Expansion`

# What's Chaos Mode?
Chaos Mode is a Risk of Rain 2 mod that adds, well... Chaos. Bosses and swarms spawn on a timer and items are randomly dispersed all the time. Events can be triggered that drastically change the way the game is experienced. Only required by the host in multiplayer (Assuming you have HookGenPatcher installed).

# Configuration
All settings are intended to work at their default. Raising them can cause unnecessary bloat and lag (especially when combined with the Survivors of The Void DLC). Adjust carefully.

Setting | Range | Default | Effect
---|---:|---:|---
ChaosRate | 1 - 10 | 1 | Raises the overall difficulty of Chaos Mode. [Mostly deprecated]
ChaosSpeed | 1 - 60 | 1 | Time in seconds before Chaos enemies spawn. (61 - ChaosSpeed) Caps at every 15 seconds.
CorruptRate | 0 - 100 | 10 | Raises the likelyhood that a corrupted item will be rolled. (Requires Survivors of The Void)
SwarmRate | 0 - 100 | 35 | Likelyhood of spawning low-tier swarms.
AmbushRate | 1 - 10 | 5 | Likelyhood of spawning high-tier boss enemies.
EliteRate | 0 - 100 | 50 | Likelyhood of spawning elite enemies.
MaxEnemies | 1 - 50 | 20 | Maximum amount of enemies that can spawn at once.
EventRate | 0 - 100 | 15 | Rate at which special "Chaos events" are triggered.
PurgeRate| 0 - 10 | 5 | Limits how many items a Purge Event can take from players. Always keeps at least 3 items.

# Contact
If you have questions or problems, you can leave them in the [issues](https://github.com/bryantBaumgartner/chaosMode/issues) or reach out to me on Discord at `Pocket#4156`.

# Changelog
2.0.2 - Fixed a bug that prevented Elite enemies from spawning (added new elite types). Fixed a bug that activated multiple events at once (*needs more testing). Reworked chest drop rates. Added new settings.

2.0.1 - Compatibility with `Survivors of The Void` DLC. Items can spawn as Corrupted Items whenever rolled. The mod may get confused if Survivors of The Void is not installed (to be fixed in a later patch).

1.0.3 - Fixed elite enemy bug. Chest drops are balanced and randomized to include Boss and Lunar items. Added new settings.

1.0.2 - Enemies can now spawn as Elites.   

1.0.1 - Initial Release

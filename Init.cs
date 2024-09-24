using BepInEx;
using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

namespace ChaosMode
{
    class Init
    {
        public static void Initialize(ConfigFile Config)
        {
            InitConfig(Config);
            switch (artifactMode.Value)
            {
                case true:
                    InitArtifact();
                    break;

                case false:
                    AddHooks();
                    break;
            }
        }

        public static ArtifactDef ChaosArtifact;
        public static bool multiplayerMode = true;

        public static void InitArtifact()
        {
            ///ISSUE WITH ARTIFACT INITIALIZATION AT THE MOMENT
            ///ORIGINAL ERROR WAS WITH AssetBundle.LoadFromStream
            ///CAN'T FIND ASSET LOCATIONS?

            //Create the new Artifact
            ChaosArtifact = ScriptableObject.CreateInstance<ArtifactDef>();
            ChaosArtifact.cachedName = "ARTIFACT_ChaosMode";
            ChaosArtifact.nameToken = "Artifact of Chaos Mode";
            ChaosArtifact.descriptionToken = "Randomizes chest drops, spawns bosses on a timer, manipulates items and causes ABSOLUTE CHAOS!";

            //AssetBundle Icons
            AssetBundle chaosBundle = AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("chaos.chaosmode_assets"));
            ChaosArtifact.smallIconSelectedSprite = chaosBundle.LoadAsset<Sprite>("Assets/ChaosMode/Selected.png");
            ChaosArtifact.smallIconDeselectedSprite = chaosBundle.LoadAsset<Sprite>("Assets/ChaosMode/Unselected.png");

            ContentAddition.AddArtifactDef(ChaosArtifact);

            RunArtifactManager.onArtifactEnabledGlobal += OnArtifactEnabled;
            RunArtifactManager.onArtifactDisabledGlobal += OnArtifactDisabled;
        }

        public static void OnArtifactEnabled(RunArtifactManager runArtifactManager, ArtifactDef artifactDef)
        {
            if (artifactDef == ChaosArtifact)
                if (NetworkServer.active)
                    AddHooks();
        }
        public static void OnArtifactDisabled(RunArtifactManager runArtifactManager, ArtifactDef artifactDef)
        {
            if (artifactDef == ChaosArtifact)
                if (NetworkServer.active)
                    RemoveHooks();
        }
        public static void AddHooks()
        {
            On.RoR2.ChestBehavior.ItemDrop += new On.RoR2.ChestBehavior.hook_ItemDrop(Hooks.ChestBehavior_ItemDrop);
            if (randomShrines.Value) On.RoR2.ShrineChanceBehavior.AddShrineStack += new On.RoR2.ShrineChanceBehavior.hook_AddShrineStack(Hooks.ShrineChanceBehavior_AddShrineStack);
            On.RoR2.Run.Start += new On.RoR2.Run.hook_Start(Hooks.Run_Start);
        }
        public static void RemoveHooks()
        {
            On.RoR2.ChestBehavior.ItemDrop -= new On.RoR2.ChestBehavior.hook_ItemDrop(Hooks.ChestBehavior_ItemDrop);
            if (randomShrines.Value) On.RoR2.ShrineChanceBehavior.AddShrineStack -= new On.RoR2.ShrineChanceBehavior.hook_AddShrineStack(Hooks.ShrineChanceBehavior_AddShrineStack);
            On.RoR2.Run.Start -= new On.RoR2.Run.hook_Start(Hooks.Run_Start);
        }

        //Used if the artifact mode is turned off - still in testing!
        //Add a conCommand to enable/disable the non-artifact mod mode
        [ConCommand(commandName = "cm_EnableMultiplayer", flags = ConVarFlags.SenderMustBeServer, helpText = "Use this command to enable the non-artifact version of the mod")]
        private static void EnableMultiplayer(ConCommandArgs args)
        {
            if (!multiplayerMode) AddHooks();
            multiplayerMode = true;
        }
        [ConCommand(commandName = "cm_DisableMultiplayer", flags = ConVarFlags.SenderMustBeServer, helpText = "Use this command to disable the non-artifact version of the mod")]
        private static void DisableMultiplayer(ConCommandArgs args)
        {
            if (multiplayerMode) RemoveHooks();
            multiplayerMode = false;
        }

        public static ConfigEntry<bool> artifactMode { get; set; }
        public static ConfigEntry<int> chaosSpeed { get; set; }
        public static ConfigEntry<bool> startingItems { get; set; }
        public static ConfigEntry<bool> giveItems { get; set; }
        public static ConfigEntry<bool> randomShrines { get; set; }
        public static ConfigEntry<bool> randomSpecialChests { get; set; }
        public static ConfigEntry<bool> includeLunar { get; set; }
        public static ConfigEntry<int> commonRate { get; set; }
        public static ConfigEntry<int> uncommonRate { get; set; }
        public static ConfigEntry<int> legendRate { get; set; }
        public static ConfigEntry<int> bossRate { get; set; }
        public static ConfigEntry<int> lunarRate { get; set; }
        public static ConfigEntry<int> corruptRate { get; set; }
        public static ConfigEntry<int> swarmRate { get; set; }
        public static ConfigEntry<int> eventRate { get; set; }
        public static ConfigEntry<int> ambushRate { get; set; }
        public static ConfigEntry<int> eliteRate { get; set; }
        public static ConfigEntry<bool> spawnLimit { get; set; }
        public static ConfigEntry<int> maxEnemies { get; set; }
        public static ConfigEntry<int> swarmAggression { get; set; }
        public static ConfigEntry<bool> enableOrder { get; set; }
        public static ConfigEntry<int> purgeRate { get; set; }

        public static void InitConfig(ConfigFile Config)
        {
            //Tooling
            artifactMode = Config.Bind<bool>(
                "Mode Settings",
                "ArtifactMode",
                true,
                "Enable or disable the mod as an artifact.\nTurn this off if playing multiplayer with unmodded clients."
            );

            //Chaos
            chaosSpeed = Config.Bind<int>(
                "Chaos Settings",
                "ChaosSpeed",
                60,
                "Raises the speed that Chaos Mode activates.\nIndicates how many seconds to wait before each Event or Spawn."
            );

            startingItems = Config.Bind<bool>(
                "Chaos Settings",
                "StartingItems",
                true,
                "Start each run with 3 Common items, 2 Uncommon items and 1 Legendary item.\nApplies to all players."
            );

            giveItems = Config.Bind<bool>(
                "Chaos Settings",
                "GiveItems",
                true,
                "Turn random item distribution on or off.\nItems either will or won't be added to your inventory when an event triggers."
            );

            randomShrines = Config.Bind<bool>(
                "Chaos Settings",
                "RandomShrines",
                true,
                "Randomizes drop pools for Shrines of Chance.\nItems either will or won't be added to your inventory when an event triggers."
            );

            randomSpecialChests = Config.Bind<bool>(
                "Chaos Settings",
                "RandomSpecialChests",
                false,
                "Randomizes drop pools for Special Chest types.\nItems dropped from Legendary Chests, Lunar Pods, Equipment Barrels, and Void Chests are randomized."
            );

            //Items
            includeLunar = Config.Bind<bool>(
                "Item Settings",
                "IncludeLunar",
                false,
                "Include or exclude Lunar items with randomly dispersed items.\nLunar items can still be found in chests."
            );
            commonRate = Config.Bind<int>(
                "Item Settings",
                "CommonRate",
                20,
                "Raises the likelyhood that a common item will be rolled.\nRoughly CommonRate% of items."
            );
            uncommonRate = Config.Bind<int>(
                "Item Settings",
                "UncommonRate",
                15,
                "Raises the likelyhood that an uncommon item will be rolled.\nRoughly UncommonRate% of items."
            );
            legendRate = Config.Bind<int>(
                "Item Settings",
                "LegendRate",
                10,
                "Raises the likelyhood that a legendary item will be rolled.\nRoughly LegendRate% of items."
            );
            bossRate = Config.Bind<int>(
                "Item Settings",
                "BossRate",
                10,
                "Raises the likelyhood that a boss item will be rolled.\nRoughly BossRate% of items."
            );
            lunarRate = Config.Bind<int>(
                "Item Settings",
                "LunarRate",
                10,
                "Raises the likelyhood that a lunar item will be rolled.\nRoughly LunarRate% of items."
            );
            corruptRate = Config.Bind<int>(
                "Item Settings",
                "CorruptRate",
                10,
                "Raises the likelyhood that a corrupted item will be rolled.\nRoughly CorruptRate% of items. (Requires Survivors of The Void)"
            );

            //Spawns
            swarmRate = Config.Bind<int>(
                "Spawn Settings",
                "SwarmRate",
                15,
                "Boosts the lilelyhood of enemies being spawned in swarms.\nRoughly SwarmRate% of spawns."
            );
            eventRate = Config.Bind<int>(
                "Spawn Settings",
                "EventRate",
                20,
                "Boosts how often events are triggered.\nRoughly EventRate% of spawns."
            );
            ambushRate = Config.Bind<int>(
                "Spawn Settings",
                "AmbushRate",
                15,
                "Boosts how often higher-tier boss enemies are spawned (not during swarms).\nRoughly AmbushRate% of enemies."
            );
            eliteRate = Config.Bind<int>(
                "Spawn Settings",
                "EliteRate",
                50,
                "Percent likelyhood that enemies spawned will be Elite.\nRoughly EliteRate% of enemies."
            );

            //Spawn Limits
            spawnLimit = Config.Bind<bool>(
                "Spawn Settings",
                "SpawnLimit",
                true,
                "Places or removes an internal cap on enemy spawns.\n*WARNING! EXPERIMENTAL AND CAN CAUSE MASSIVE LAG!*"
            );
            maxEnemies = Config.Bind<int>(
                "Spawn Settings",
                "MaxEnemies",
                20,
                "Maximum amount of enemies that *CAN* be spawned during a swarm.\n*Disabling SpawnLimit will cause the mod to ignore this value.*"
            );
            swarmAggression = Config.Bind<int>(
                "Spawn Settings",
                "SwarmAggression",
                1,
                "Multiplies the amount of enemies that are spawned during swarms!"
            );

            //Events
            enableOrder = Config.Bind<bool>(
                "Event Settings",
                "EnableOrder",
                false,
                "Enable or disable the Sequencing order event.\nThe order event takes your items and redistributes them similar to the Shrine of Order."
            );
            purgeRate = Config.Bind<int>(
                "Event Settings",
                "PurgeRate",
                5,
                "Limits how many items a Purge can take (limited to all but 3).\nPurge will remove *UP TO* PurgeRate of your items. (Set to 0 to disable Purge events.)"
            );

            //I swear I will keep adding config settings until the day I die.
        }
    }
}

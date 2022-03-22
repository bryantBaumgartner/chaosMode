using BepInEx;
using BepInEx.Configuration;
using RoR2;
using RoR2.ExpansionManagement;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;

namespace ChaosMode
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.Pocket.ChaosMode", "ChaosMode", "2.1.1")]
    internal class ChaosMode : BaseUnityPlugin
    {
        public static ConfigEntry<int> chaosSpeed { get; set; }
        
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

        public static ConfigEntry<int> purgeRate { get; set; }

        public void Awake()
        {
            //Chaos
            chaosSpeed = Config.Bind<int>(
                "Chaos Settings",
                "ChaosSpeed",
                60,
                "Raises the speed that Chaos Mode activates.\nIndicates how many seconds to wait before each Event or Spawn."
            );

            //Items
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
                35,
                "Boosts the lilelyhood of enemies being spawned in swarms.\nRoughly SwarmRate% of spawns."
            );
            eventRate = Config.Bind<int>(
                "Spawn Settings",
                "EventRate",
                15,
                "Boosts how often events are triggered.\nRoughly EventRate% of spawns."
            );
            ambushRate = Config.Bind<int>(
                "Spawn Settings",
                "AmbushRate",
                30,
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
            purgeRate = Config.Bind<int>(
                "Event Settings",
                "PurgeRate",
                5,
                "Limits how many items a Purge can take (limited to all but 3).\nPurge will remove *UP TO* PurgeRate of your items. (Set to 0 to disable Purge events.)"
            );

            //Randomize chest drops using our drop table method
            On.RoR2.ChestBehavior.ItemDrop += (orig, self) =>
            {
                PropertyInfo dropPickupField = typeof(ChestBehavior).GetProperty("dropPickup", BindingFlags.Instance | BindingFlags.Public);
                System.Console.WriteLine("[Chaos Log] DropPickup is null? {0}", dropPickupField == null);

                List<PickupIndex> newRoll = RollType(GetDropTable());
                if (!Run.instance.IsPickupAvailable(newRoll[0])) newRoll = RollType(GetDropTable(true)); //Prevent an undroppable item

                PickupIndex item = newRoll[random.Next(0, newRoll.Count)];
                dropPickupField.SetValue(self, item);

                orig(self);
                return;
            };
        }
        public void Update()
        {
            //Chaos Mode Functions
            Initialize();
            TimedSummon();

            //Debug Tools
        }

        //Game Management Methods       
        private void Initialize()
        {
            //Legacy Version?
            //Wait until we leave the non-game screens, otherwise cancel all coroutines
            string scene = SceneManager.GetActiveScene().name;
            //if (scene != "splash" & scene != "intro" & scene != "title" & scene != "lobby" & scene != "loadingbasic")
            if (Run.instance & scene != "lobby")
            {
                if (!initialize)
                {
                    initialize = true;
                    currentRunInstance = Intro();
                    StartCoroutine(Intro());
                }
                else if (currentRunInstance == null)
                    initialize = false; //Patch that prevents errors from totally shutting down the mod
            }
            else
            {
                StopAllCoroutines();
                spawning = false;
                initialize = false;
            }
        }
        private IEnumerator Intro()
        {
            yield return new WaitForSeconds(1f);

            //Set a class wide value telling us if dlc1 is enabled on this run
            try
            {
                expansion1 = Run.instance.IsExpansionEnabled(ExpansionCatalog.expansionDefs[0]);
                System.Console.WriteLine("[Chaos Log] Expansion1 loaded: {0}", expansion1);
            }
            catch
            {
                System.Console.WriteLine("[Chaos Log] ExpansionDef not found! Are you using the current version of RoR2?");
            }

            //Use the current seed of the game for consistency
            int seed = (int)Run.instance.seed;
            random = new System.Random(seed);
            System.Console.WriteLine("[Chaos Log] Run seed converted to random seed: {0}", seed);

            //Give initial items - Update this?
            List<PickupIndex> newRoll = RollType(0);
            PickupIndex item = newRoll[random.Next(0, newRoll.Count)];

            for (int i = 0; i < 3; i++)
            {
                item = newRoll[random.Next(0, newRoll.Count)];
                GiveToAllPlayers(item);
            }
            newRoll = RollType(1);
            for (int i = 0; i < 2; i++)
            {
                item = newRoll[random.Next(0, newRoll.Count)];
                GiveToAllPlayers(item);
            }
            newRoll = RollType(2);
            item = newRoll[random.Next(0, newRoll.Count)];
            GiveToAllPlayers(item);

            Chat.SendBroadcastChat(new Chat.SimpleChatMessage
            {
                baseToken = "<color=#bb0011>[CHAOS] Welcome to CHAOS MODE!"
            });
        }

        //Chaos Mode Methods
        private void TimedSummon()
        {
            if (initialize)
            {
                float t = Run.instance.GetRunStopwatch();
                if (Mathf.FloorToInt(t) % (Mathf.Clamp(chaosSpeed.Value, 15, 60)) == 0 & t > 5 & t != oldTimer)
                {
                    oldTimer = (int)t;
                    if (!spawning)
                    {
                        spawning = true;
                        SpawnEveryMinute();
                        StartCoroutine(FailSafeDelay());
                    }
                }
            }
        }
        private void SpawnEveryMinute()
        {
            List<SpawnCardData> normalEnemies = new List<SpawnCardData>
            {
                ADBeetleGuard, ADGreaterWisp, ADGolem, ADTitan, ADParent, ADBigLemurian, ADNullifier,
                ADRoboBall, ADTemplar, ADArchWisp, ADBeetleQueen, ADLunarGolem, ADLunarWisp
            };
            List<SpawnCardData> heavyEnemies =  new List<SpawnCardData>
            {
                ADBeetleQueen, ADTitan, ADTitanGold, ADOverlord, ADMagmaWorm, ADOverWorm,
                ADDunestrider, ADGhibli, ADGrandparent, ADMagmaWorm, ADBrother, ADScav
            };
            List<SpawnCardData> swarmEnemies = new List<SpawnCardData>
            {
                ADBeetle, ADWisp, ADGreaterWisp, ADBell, ADBison, ADImp, ADGolem, ADLemurian,
                ADJellyfish, ADHermitCrab, ADParent, ADVulture, ADLunarBomb, ADMushroom
            };

            //Add the first expansion's enemies to the normal pool of enemies
            if (expansion1) {
                List<SpawnCardData> ex1NormalEnemies = new List<SpawnCardData> { ADGup, ADJailer, ADApothecary, ADBarnacle };
                List<SpawnCardData> ex1HeavyEnemies = new List<SpawnCardData> { ADJailer, ADMegaCrab, ADMega, ADMajor, ADVoidling };
                List<SpawnCardData> ex1SwarmEnemies = new List<SpawnCardData> { ADLarva, ADPest, ADVermin, ADMinor, ADApothecary, ADAssassin, ADInfestor, ADBarnacle };
                normalEnemies.AddRange(ex1NormalEnemies);
                heavyEnemies.AddRange(ex1HeavyEnemies);
                swarmEnemies.AddRange(ex1SwarmEnemies);
            }

            SpawnCardData enemy = null;
            List<PickupIndex> newRoll = null;
            int type = 0, number = 1;

            switch (GetInstanceTable())
            {
                case 0:
                    //Swarm Spawn
                    enemy = swarmEnemies[random.Next(0, swarmEnemies.Count)];
                    number = Mathf.Clamp(Run.instance.GetDifficultyScaledCost(random.Next(5, 10)) * Mathf.Clamp(swarmAggression.Value, 1, spawnLimit.Value ? 3 : 1024),
                        5, spawnLimit.Value ? maxEnemies.Value : 65536);
                    SummonEnemy(enemy, number);

                    type = GetDropTable();
                    newRoll = RollType(GetDropTable(restrictEquipment: true));
                    GiveToAllPlayers(newRoll[random.Next(0, newRoll.Count)]);
                    break;

                case 1:
                    //Spawn Single Enemy
                    int difficultyBase = Mathf.Clamp(Run.instance.GetDifficultyScaledCost(1) - 1, 0, ambushRate.Value); //This should scale the rate of higher tier enemies over time
                    enemy = (random.Next(0, 100 - difficultyBase) < ambushRate.Value) ? heavyEnemies[random.Next(0, heavyEnemies.Count)] : normalEnemies[random.Next(0, normalEnemies.Count)];
                    number = Mathf.Clamp(Run.instance.GetDifficultyScaledCost(random.Next(1, 3)),
                        1, spawnLimit.Value ? maxEnemies.Value : 65536);
                    SummonEnemy(enemy, number);

                    type = GetDropTable();
                    newRoll = RollType(GetDropTable(restrictEquipment: true));
                    GiveToAllPlayers(newRoll[random.Next(0, newRoll.Count)]);
                    break;

                case 2:
                    //Event
                    List<IEnumerator> events = new List<IEnumerator>() { JellyfishEvent(), EliteParentEvent(), FinalEncounter(), GainFriend(), SequenceEvent() };
                    if (purgeRate.Value > 0) events.Add(PurgeAllItems());
                    if (expansion1) events.AddRange(new List<IEnumerator>() { Corruption(), VoidEncounter() });

                    StartCoroutine(events[random.Next(0, events.Count)]);
                    break;
            }
        }
        private IEnumerator FailSafeDelay()
        {
            yield return new WaitForSeconds(1f);
            spawning = false;
        }

        //Inventory Methods      
        private void GiveToAllPlayers(PickupIndex pickupIndex, int count = 1)
        {
            //Loop through players and give them each the same pickupindex
            foreach (PlayerCharacterMasterController playerCharacterMasterController in PlayerCharacterMasterController.instances)
            {
                string nameOut = playerCharacterMasterController.GetDisplayName();
                CharacterMaster master = playerCharacterMasterController.master;
                master.inventory.GiveItem(PickupCatalog.GetPickupDef(pickupIndex).itemIndex, count);
                MethodInfo method = typeof(GenericPickupController).GetMethod("SendPickupMessage", BindingFlags.Static | BindingFlags.NonPublic);
                method.Invoke(null, new object[]
                {
                    master,
                    pickupIndex
                });
            }
        }
        private void GiveToOnePlayer(PlayerCharacterMasterController playerCharacterMasterController, PickupIndex pickupIndex, int count = 1)
        {
            string nameOut = playerCharacterMasterController.GetDisplayName();
            CharacterMaster master = playerCharacterMasterController.master;
            master.inventory.GiveItem(PickupCatalog.GetPickupDef(pickupIndex).itemIndex, count);
            MethodInfo method = typeof(GenericPickupController).GetMethod("SendPickupMessage", BindingFlags.Static | BindingFlags.NonPublic);
            method.Invoke(null, new object[]
            {
                    master,
                    pickupIndex
            });
        }
        private void EquipAllPlayers(int pickupIndex)
        {
            //Loop through each player and equip the same equipmentindex
            foreach (PlayerCharacterMasterController playerCharacterMasterController in PlayerCharacterMasterController.instances)
            {
                string nameOut = playerCharacterMasterController.GetDisplayName();
                CharacterMaster master = playerCharacterMasterController.master;
                master.inventory.SetEquipmentIndex((EquipmentIndex)pickupIndex);
                MethodInfo method = typeof(GenericPickupController).GetMethod("SendPickupMessage", BindingFlags.Static | BindingFlags.NonPublic);
                method.Invoke(null, new object[]
                {
                    master,
                    pickupIndex
                });
            }
        } //Fix this maybe?
        private void EquipOneElite(Inventory inventory, EliteEquipment eliteType) //Addressable Resource Loading
        {
            EquipmentDef elite = null;
            elite = Addressables.LoadAssetAsync<EquipmentDef>(eliteType.addressable).WaitForCompletion();

            inventory.SetEquipmentIndex(elite.equipmentIndex);
        }

        //Chaos Event Coroutines
        private IEnumerator JellyfishEvent()
        {
            Chat.SendBroadcastChat(new Chat.SimpleChatMessage
            {
                baseToken = "<color=#bb0011>[CHAOS] <color=#ff0000>Jellyfish event! Zzzap!</color>"
            });

            //Addressable Resource loading
            CharacterSpawnCard spawnCard = null;
            spawnCard = Addressables.LoadAssetAsync<CharacterSpawnCard>(ADJellyfish.location).WaitForCompletion();
            for (int i = 0; i < 50; i++)
            {
                foreach (PlayerCharacterMasterController player in PlayerCharacterMasterController.instances)
                {
                    GameObject spawnedInstance = SpawnEnemy(spawnCard, player.master.GetBody().transform.position).spawnedInstance;
                    StartCoroutine(CheckIfEnemyDied(spawnedInstance));              
                }

                yield return new WaitForSeconds(0.5f - (i / 100));
            }
        }
        private IEnumerator EliteParentEvent()
        {
            Chat.SendBroadcastChat(new Chat.SimpleChatMessage
            {
                baseToken = "<color=#bb0011>[CHAOS] <color=#ff0000>Elite Parent event! The council will see you now!</color>"
            });

            PlayerCharacterMasterController[] players = FindObjectsOfType<PlayerCharacterMasterController>();
            PlayerCharacterMasterController chosen = players[random.Next(0, players.Length)];

            //Addressable Resource loading
            CharacterSpawnCard spawnCard = null;
            spawnCard = Addressables.LoadAssetAsync<CharacterSpawnCard>(ADParent.location).WaitForCompletion();
            GameObject spawnedInstance = SpawnEnemy(spawnCard, chosen.master.GetBody().transform.position).spawnedInstance;
            EquipOneElite(spawnedInstance.GetComponent<CharacterMaster>().inventory, ADFire);
            StartCoroutine(CheckIfEnemyDied(spawnedInstance));

            spawnedInstance = SpawnEnemy(spawnCard, chosen.master.GetBody().transform.position).spawnedInstance;
            EquipOneElite(spawnedInstance.GetComponent<CharacterMaster>().inventory, ADIce);
            StartCoroutine(CheckIfEnemyDied(spawnedInstance));

            spawnedInstance = SpawnEnemy(spawnCard, chosen.master.GetBody().transform.position).spawnedInstance;
            EquipOneElite(spawnedInstance.GetComponent<CharacterMaster>().inventory, ADLightning);
            StartCoroutine(CheckIfEnemyDied(spawnedInstance));

            spawnedInstance = SpawnEnemy(spawnCard, chosen.master.GetBody().transform.position).spawnedInstance;
            EquipOneElite(spawnedInstance.GetComponent<CharacterMaster>().inventory, ADGhost);
            StartCoroutine(CheckIfEnemyDied(spawnedInstance));

            spawnedInstance = SpawnEnemy(spawnCard, chosen.master.GetBody().transform.position).spawnedInstance;
            EquipOneElite(spawnedInstance.GetComponent<CharacterMaster>().inventory, ADPoison);
            StartCoroutine(CheckIfEnemyDied(spawnedInstance));

            spawnedInstance = SpawnEnemy(spawnCard, chosen.master.GetBody().transform.position).spawnedInstance;
            EquipOneElite(spawnedInstance.GetComponent<CharacterMaster>().inventory, ADEcho);
            StartCoroutine(CheckIfEnemyDied(spawnedInstance));

            spawnedInstance = SpawnEnemy(spawnCard, chosen.master.GetBody().transform.position).spawnedInstance;
            EquipOneElite(spawnedInstance.GetComponent<CharacterMaster>().inventory, ADEarth);
            StartCoroutine(CheckIfEnemyDied(spawnedInstance));

            yield return null;
        }
        private IEnumerator FinalEncounter()
        {
            Chat.SendBroadcastChat(new Chat.SimpleChatMessage
            {
                baseToken = "<color=#bb0011>[CHAOS] <color=#ff0000>Mutated event!\nThe King of Nothing loses control!</color>"
            });

            foreach (PlayerCharacterMasterController player in PlayerCharacterMasterController.instances)
                StartCoroutine(Purge(player));

            PlayerCharacterMasterController[] players = FindObjectsOfType<PlayerCharacterMasterController>();
            PlayerCharacterMasterController chosen = players[random.Next(0, players.Length)];

            //Addressable Resource loading
            CharacterSpawnCard spawnCard = null;
            spawnCard = Addressables.LoadAssetAsync<CharacterSpawnCard>(ADBrother.location).WaitForCompletion();
            GameObject spawnedInstance = SpawnEnemy(spawnCard, chosen.master.GetBody().transform.position).spawnedInstance;
            EquipOneElite(spawnedInstance.GetComponent<CharacterMaster>().inventory, ADVoid);
            StartCoroutine(CheckIfEnemyDied(spawnedInstance));

            yield return null;
        }
        private IEnumerator VoidEncounter()
        {
            Chat.SendBroadcastChat(new Chat.SimpleChatMessage
            {
                baseToken = "<color=#bb0011>[CHAOS] <color=#ff0000>[VO?ID E??VEN?T][E?SCA?PE!]</color>"
            });

            PlayerCharacterMasterController[] players = FindObjectsOfType<PlayerCharacterMasterController>();
            PlayerCharacterMasterController chosen = players[random.Next(0, players.Length)];

            //Addressable Resource loading
            CharacterSpawnCard spawnCard = null;
            spawnCard = Addressables.LoadAssetAsync<CharacterSpawnCard>(ADVoidling.location).WaitForCompletion();
            GameObject spawnedInstance = SpawnEnemy(spawnCard, chosen.master.GetBody().transform.position).spawnedInstance;
            StartCoroutine(CheckIfEnemyDied(spawnedInstance));

            yield return null;
        }
        private IEnumerator PurgeAllItems()
        {
            Chat.SendBroadcastChat(new Chat.SimpleChatMessage
            {
                baseToken = "<color=#bb0011>[Chaos] <color=#ff0000>Purge event!\nYou don't need these, right?</color>"
            });

            foreach (PlayerCharacterMasterController player in PlayerCharacterMasterController.instances)
                StartCoroutine(Purge(player));

            yield return null;
        }
        private IEnumerator Purge(PlayerCharacterMasterController player)
        {
            CharacterMaster master = player.master;
            List<ItemIndex> inventory = master.inventory.itemAcquisitionOrder;
            int cap = random.Next(3, inventory.Count - 2), j = 0;
            for (int i = inventory.Count - 1; i >= cap; i--)
            {
                //No loner removes in obtained order
                ItemIndex slot = inventory[random.Next(0, inventory.Count)];
                master.inventory.RemoveItem(slot, Mathf.Min(master.inventory.GetItemCount(slot), 2147483647));
                yield return new WaitForSeconds(1f);

                j++;
                if (j > purgeRate.Value) break;
            }
        }
        private IEnumerator Corruption()
        {
            Chat.SendBroadcastChat(new Chat.SimpleChatMessage
            {
                baseToken = "<color=#bb0011>[CHAOS] <color=#ff0000>[VO?ID E??VEN?T][B?EC?OME O??NE OF U?S]</color>"
            });

            int corrupt = random.Next(1, 5);
            for (int i = 0; i < corrupt; i++)
            {
                List<PickupIndex> corruption = RollType(6);
                GiveToAllPlayers(corruption[random.Next(0, corruption.Count)]);
                yield return new WaitForSeconds(1f);
            }
        }
        private IEnumerator GainFriend()
        {
            //Addressable Resource loading
            List<SpawnCardData> allies = new List<SpawnCardData>() { ADBeetleGuard, ADBrother, ADNullifier, ADTitanGold, ADLunarGolem, ADVagrant };
            if (expansion1) allies.Add(ADGup);
            SpawnCardData ally = allies[random.Next(0, allies.Count)];
            CharacterSpawnCard spawnCard = null;
            spawnCard = Addressables.LoadAssetAsync<CharacterSpawnCard>(ally.location).WaitForCompletion();

            Chat.SendBroadcastChat(new Chat.SimpleChatMessage
            {
                baseToken = "<color=#bb0011>[CHAOS] <color=#ff0000>Friendly event! " + ally.name + " wants to help you this stage!</color>"
            });

            PlayerCharacterMasterController[] players = FindObjectsOfType<PlayerCharacterMasterController>();
            PlayerCharacterMasterController chosen = players[random.Next(0, players.Length)];
            GameObject spawnedInstance = SpawnEnemy(spawnCard, chosen.master.GetBody().transform.position, ally: true).spawnedInstance;

            yield return null;
        }
        private IEnumerator SequenceEvent()
        {
            foreach (PlayerCharacterMasterController player in PlayerCharacterMasterController.instances)
            {
                CharacterMaster master = player.master;

                //Get all item tier counts
                int[] tiers = new int[] { 0, 0, 0, 0, 0, 0 };
                tiers[0] = master.inventory.GetTotalItemCountOfTier(ItemTier.Tier1);
                tiers[1] = master.inventory.GetTotalItemCountOfTier(ItemTier.Tier2);
                tiers[2] = master.inventory.GetTotalItemCountOfTier(ItemTier.Tier3);
                tiers[3] = master.inventory.GetTotalItemCountOfTier(ItemTier.Boss);
                tiers[4] = master.inventory.GetTotalItemCountOfTier(ItemTier.Lunar);
                if (expansion1) {
                    tiers[5] = master.inventory.GetTotalItemCountOfTier(ItemTier.VoidTier1);
                    tiers[5] += master.inventory.GetTotalItemCountOfTier(ItemTier.VoidTier2);
                    tiers[5] += master.inventory.GetTotalItemCountOfTier(ItemTier.VoidTier3);
                    tiers[5] += master.inventory.GetTotalItemCountOfTier(ItemTier.VoidBoss);
                }

                System.Console.WriteLine("{0}, {1}, {2}, {3}, {4}, {5}", tiers[0], tiers[1], tiers[2], tiers[3], tiers[4], tiers[5]);

                //Reset all your items
                List<ItemIndex> inventory = master.inventory.itemAcquisitionOrder;
                while(master.inventory.itemAcquisitionOrder.Count > 0)
                {
                    ItemIndex slot = inventory[0];
                    master.inventory.RemoveItem(slot, Mathf.Min(master.inventory.GetItemCount(slot), 2147483647));
                }

                //Dish out an item of each tier for each old item
                for (int i = 0; i < 5; i++)
                {
                    if (tiers[i] <= 0) continue;
                    List<PickupIndex> tieredItem = RollType(i);
                    GiveToOnePlayer(player, tieredItem[random.Next(0, tieredItem.Count)], tiers[i]);
                }
                if (expansion1)
                {
                    if (tiers[5] > 0)
                    {
                        List<PickupIndex> tieredItem = RollType(6);
                        GiveToOnePlayer(player, tieredItem[random.Next(0, tieredItem.Count)], tiers[5]);
                    }
                }

                //This is an inside joke btw
                Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                {
                    baseToken = "<color=#bb0011>[CHAOS] <color=#ff0000>Order event! Your body has been sequenced!</color>"
                });
            }

            yield return null;
        }

        //Enemy Spawn Methods
        private void SummonEnemy(SpawnCardData enemyType, int reps)
        {
            int loop = 0;
            string elementName = "";
            float difficulty = 0, threshold = 1;

            //Elite types
            List<EliteEquipment> eliteTypes = new List<EliteEquipment>() { ADFire, ADIce, ADLightning, ADGhost, ADPoison, ADEcho };
            if (expansion1) eliteTypes.AddRange(new List<EliteEquipment>() { ADEarth, ADVoid });
  
            //Threshold gets lower, until it's at 0.5f
            threshold =  1.9f - (((float)eliteRate.Value / 100f) * 1.5f);
            difficulty = Mathf.Clamp(2 - Mathf.Clamp((Run.instance.GetDifficultyScaledCost(reps) * enemyType.difficultyBase * Random.Range(0.7f, 1.3f)) / Run.instance.GetDifficultyScaledCost(reps), 0.5f, 2), 0.5f, 2);
            System.Console.WriteLine("[Chaos Log] Difficulty is {0} >= Elite Threshold is {1}", difficulty, threshold);

            //Get an element and bool based on the difficulty
            bool getElement = difficulty >= threshold ? random.Next(0, 2) == 0 ? true : false : false;
            EliteEquipment elite = eliteTypes[Random.Range(0, eliteTypes.Count)];

            //Failsafe in case the SpawnCard doesn't exist
            try
            {
                //Check to see if this needs to scale anymore
                int count = reps;

                //Addressable Resource loading
                CharacterSpawnCard spawnCard = null;
                spawnCard = Addressables.LoadAssetAsync<CharacterSpawnCard>(enemyType.location).WaitForCompletion();

                for (int i = 0; i < count; i++)
                {
                    foreach (PlayerCharacterMasterController player in PlayerCharacterMasterController.instances)
                    {
                        //Legacy spawn system
                        GameObject spawnedInstance = SpawnEnemy(spawnCard, player.master.GetBody().transform.position).spawnedInstance;

                        if (getElement & spawnedInstance)
                        {
                            EquipOneElite(spawnedInstance.GetComponent<CharacterMaster>().inventory, elite);
                            elementName = elite.prefix;
                            count = Mathf.Clamp(count - 1, 1, 50);
                        }
                        StartCoroutine(CheckIfEnemyDied(spawnedInstance, (int)enemyType.rewardBase));
                        loop++;
                    }
                }
                Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                {
                    baseToken = "<color=#bb0011>[CHAOS] <color=#ff0000>Summoning " + loop + (getElement ? " " + elementName + " " : " ") + enemyType.name + (loop > 1 ? "s" : "") + "!</color>"
                });

            }
            catch
            {
                System.Console.WriteLine("[Chaos Log] Can't find SpawnCard: {0}!", enemyType.name);
                return;
            }

            return;
        }
        private SpawnCard.SpawnResult SpawnEnemy(CharacterSpawnCard spawnCard, Vector3 center, bool ally = false)
        {
            spawnCard.noElites = false;
            spawnCard.eliteRules = SpawnCard.EliteRules.Default;

            DirectorPlacementRule placementRule = new DirectorPlacementRule
            {
                placementMode = DirectorPlacementRule.PlacementMode.Direct,
                preventOverhead = true
            };
            DirectorSpawnRequest spawnRequest = new DirectorSpawnRequest(spawnCard, placementRule, new Xoroshiro128Plus((ulong)Run.instance.stageRng.nextUint))
            {
                teamIndexOverride = new TeamIndex?(!ally ? TeamIndex.Monster : TeamIndex.Player),
                ignoreTeamMemberLimit = true
            };
            return spawnCard.DoSpawn(center + new Vector3(random.Next(-25, 25), 10f, random.Next(-25, 25)), Quaternion.identity, spawnRequest);
        }
        private IEnumerator CheckIfEnemyDied(GameObject enemy, int reward = 20)
        {
            while (enemy != null)
                yield return new WaitForSeconds(0.1f);

            foreach (PlayerCharacterMasterController player in PlayerCharacterMasterController.instances)
                player.GetComponent<CharacterMaster>().GiveMoney((uint)Run.instance.GetDifficultyScaledCost(reward)); //Add a reward value for each enemy?
        }

        //Tables
        private List<PickupIndex> RollType(int item)
        {
            //Return the corresponding roll type per item tier
            List<PickupIndex> rollType = Run.instance.availableTier1DropList;
            switch (item)
            {
                case 1:
                    rollType = Run.instance.availableTier2DropList;
                    break;

                case 2:
                    rollType = Run.instance.availableTier3DropList;
                    break;

                case 3:
                    rollType = Run.instance.availableBossDropList;
                    break;

                case 4:
                    rollType = Run.instance.availableLunarItemDropList;
                    break;

                case 5:
                    rollType = Run.instance.availableEquipmentDropList;
                    break;

                //Include all void items in one tier so as not to bloat the drop table
                case 6:
                    List<List<PickupIndex>> corruptedList = new List<List<PickupIndex>>() {
                        Run.instance.availableVoidTier1DropList, Run.instance.availableVoidTier2DropList, 
                        Run.instance.availableVoidTier3DropList, Run.instance.availableVoidBossDropList
                    };
                    rollType = corruptedList[GetVoidDropTable()];
                    break;

                default:
                    break;
            }
            return rollType;
        }
        private int GetDropTable(bool restrictVoid = false, bool restrictEquipment = false)
        {
            //Whoops, actually          W > G > R > B > L > E > C
            int[] weights = new int[] {
                commonRate.Value,
                uncommonRate.Value,
                legendRate.Value,
                bossRate.Value,
                lunarRate.Value,
                restrictEquipment ? 0 : 15,
                expansion1 ? restrictVoid ? 0 : corruptRate.Value : 0
            };
            //int[] weights = new int[] { 20, 15, 10, 10, 10, restrictEquipment ? 0 : 15, expansion1 ? restrictVoid ? 0 : corruptRate.Value : 0 };
            int strength = 0, check = 0;
            foreach (int i in weights) strength += i;
            int roll = random.Next(0, strength);

            //Pick subset based on weight and order
            for (int i = 0; i < weights.Length; i++)
            {
                if (weights[i] == 0) continue;

                check += weights[i];
                if (roll < check)
                {
                    return i;
                }
            }
            return 0;
        }
        private int GetVoidDropTable()
        {
            //In order,                 W > G > R > B
            int[] weights = new int[] { 40, 30, 25, 5 };
            int strength = 0, check = 0;
            foreach (int i in weights) strength += i;
            int roll = random.Next(0, strength);

            //Pick subset based on weight and order
            for (int i = 0; i < weights.Length; i++)
            {
                if (weights[i] == 0) continue;

                check += weights[i];
                if (roll < check)
                {
                    return i;
                }
            }
            return 0;
        }
        private int GetInstanceTable()
        {
            //In order,                 Swarm > Boss > Event
            int[] weights = new int[] { swarmRate.Value, Mathf.Clamp(100 - swarmRate.Value - eventRate.Value, 0, 100), eventRate.Value };
            int strength = 0, check = 0;
            foreach (int i in weights) strength += i;
            int roll = random.Next(0, strength);

            for (int i = 0; i < weights.Length; i++)
            {
                check += weights[i];
                if (roll < check) return i;
            }
            return 0;
        }

        //Debug Tools

        //Class wide variables
        private IEnumerator currentRunInstance;
        private System.Random random = new System.Random();     
        private bool initialize = false, spawning = false, expansion1;
        private int oldTimer = 0;

        //Addressable Resource Loading
        //New Elite Equipment Locations
        private EliteEquipment ADFire      = new EliteEquipment() { prefix = "Blazing",     addressable = "RoR2/Base/EliteFire/EliteFireEquipment.asset"           };
        private EliteEquipment ADIce       = new EliteEquipment() { prefix = "Glacial",     addressable = "RoR2/Base/EliteIce/EliteIceEquipment.asset"             };
        private EliteEquipment ADLightning = new EliteEquipment() { prefix = "Overloading", addressable = "RoR2/Base/EliteLightning/EliteLightningEquipment.asset" };
        private EliteEquipment ADGhost     = new EliteEquipment() { prefix = "Celestine",   addressable = "RoR2/Base/EliteHaunted/EliteHauntedEquipment.asset"     };
        private EliteEquipment ADPoison    = new EliteEquipment() { prefix = "Malachite",   addressable = "RoR2/Base/ElitePoison/ElitePoisonEquipment.asset"       };
        private EliteEquipment ADEcho      = new EliteEquipment() { prefix = "Perfected",   addressable = "RoR2/Base/EliteLunar/EliteLunarEquipment.asset"         };
        private EliteEquipment ADEarth     = new EliteEquipment() { prefix = "Mending",     addressable = "RoR2/DLC1/EliteEarth/EliteEarthEquipment.asset"         };
        private EliteEquipment ADVoid      = new EliteEquipment() { prefix = "Voidtouched", addressable = "RoR2/DLC1/EliteVoid/EliteVoidEquipment.asset"           };
        private EliteEquipment ADSpeed     = new EliteEquipment() { prefix = "Speedy?",     addressable = "RoR2/DLC1/EliteSecretSpeedEquipment.asset"              };
        private EliteEquipment ADGold      = new EliteEquipment() { prefix = "Golden?",     addressable = "RoR2/Junk/EliteGold/EliteGoldEquipment.asset"           };
        private EliteEquipment ADYellow    = new EliteEquipment() { prefix = "Yellow?",     addressable = "RoR2/Junk/EliteYellow/EliteYellowEquipment.asset"       };

        //Weaker Enemies
        private SpawnCardData ADBeetle      = new SpawnCardData() { name = "Beetle",             location = "RoR2/Base/Beetle/cscBeetle.asset",                     difficultyBase = 0.2f, rewardBase = 5f  };
        private SpawnCardData ADBeetleGuard = new SpawnCardData() { name = "Beetle Guard",       location = "RoR2/Base/Beetle/cscBeetleGuard.asset",                difficultyBase = 0.5f, rewardBase = 12f };
        private SpawnCardData ADBeetleQueen = new SpawnCardData() { name = "Beetle Queen",       location = "RoR2/Base/Beetle/cscBeetleQueen.asset",                difficultyBase = 0.9f, rewardBase = 23f };
        private SpawnCardData ADLemurian    = new SpawnCardData() { name = "Lemurian",           location = "RoR2/Base/Lemurian/cscLemurian.asset",                 difficultyBase = 0.9f, rewardBase = 23f };
        private SpawnCardData ADBigLemurian = new SpawnCardData() { name = "Elder Lemurian",     location = "RoR2/Base/LemurianBruiser/cscLemurianBruiser.asset",   difficultyBase = 0.9f, rewardBase = 23f };
        private SpawnCardData ADBell        = new SpawnCardData() { name = "Brass Contraption",  location = "RoR2/Base/Bell/cscBell.asset",                         difficultyBase = 0.7f, rewardBase = 16f };
        private SpawnCardData ADBison       = new SpawnCardData() { name = "Bison",              location = "RoR2/Base/Bison/cscBison.asset",                       difficultyBase = 0.4f, rewardBase = 9f  };
        private SpawnCardData ADTemplar     = new SpawnCardData() { name = "Clay Templar",       location = "RoR2/Base/ClayBruiser/cscClayBruiser.asset",           difficultyBase = 1.0f, rewardBase = 21f };
        private SpawnCardData ADApothecary  = new SpawnCardData() { name = "Clay Apothecary",    location = "RoR2/DLC1/ClayGrenadier/cscClayGrenadier.asset",       difficultyBase = 0.9f, rewardBase = 18f };
        private SpawnCardData ADGolem       = new SpawnCardData() { name = "Stone Golem",        location = "RoR2/Base/Golem/cscGolem.asset",                       difficultyBase = 0.5f, rewardBase = 10f };
        private SpawnCardData ADWisp        = new SpawnCardData() { name = "Lesser Wisp",        location = "RoR2/Base/Wisp/cscLesserWisp.asset",                   difficultyBase = 0.2f, rewardBase = 4f  };
        private SpawnCardData ADGreaterWisp = new SpawnCardData() { name = "Greater Wisp",       location = "RoR2/Base/GreaterWisp/cscGreaterWisp.asset",           difficultyBase = 0.8f, rewardBase = 14f };
        private SpawnCardData ADJellyfish   = new SpawnCardData() { name = "Jellyfish",          location = "RoR2/Base/Jellyfish/cscJellyfish.asset",               difficultyBase = 0.3f, rewardBase = 7f  };
        private SpawnCardData ADMushroom    = new SpawnCardData() { name = "Mini Mushroom",      location = "RoR2/Base/MiniMushroom/cscMiniMushroom.asset",         difficultyBase = 1.4f, rewardBase = 19f };
        private SpawnCardData ADVulture     = new SpawnCardData() { name = "Alloy Vulture",      location = "RoR2/Base/Vulture/cscVulture.asset",                   difficultyBase = 0.7f, rewardBase = 14f };
        private SpawnCardData ADImp         = new SpawnCardData() { name = "Imp",                location = "RoR2/Base/Imp/cscImp.asset",                           difficultyBase = 0.6f, rewardBase = 16f };
        private SpawnCardData ADParent      = new SpawnCardData() { name = "Parent",             location = "RoR2/Base/Parent/cscParent.asset",                     difficultyBase = 1.2f, rewardBase = 23f };
        private SpawnCardData ADLunarGolem  = new SpawnCardData() { name = "Lunar Chimera",      location = "RoR2/Base/LunarGolem/cscLunarGolem.asset",             difficultyBase = 1.1f, rewardBase = 25f };
        private SpawnCardData ADLunarWisp   = new SpawnCardData() { name = "Lunar Chimera",      location = "RoR2/Base/LunarWisp/cscLunarWisp.asset",               difficultyBase = 1.3f, rewardBase = 27f };
        private SpawnCardData ADLunarBomb   = new SpawnCardData() { name = "Lunar Chimera",      location = "RoR2/Base/LunarExploder/cscLunarExploder.asset",       difficultyBase = 0.8f, rewardBase = 19f };
        private SpawnCardData ADNullifier   = new SpawnCardData() { name = "Void Reaver",        location = "RoR2/Base/Nullifier/cscNullifier.asset",               difficultyBase = 1.5f, rewardBase = 32f };
        private SpawnCardData ADArchWisp    = new SpawnCardData() { name = "Archaic Wisp",       location = "RoR2/Junk/ArchWisp/cscArchWisp.asset",                 difficultyBase = 0.9f, rewardBase = 23f };
        private SpawnCardData ADHermitCrab  = new SpawnCardData() { name = "Hermit Crab",        location = "RoR2/Base/HermitCrab/cscHermitCrab.asset",             difficultyBase = 0.4f, rewardBase = 8f  };

        //Boss Tier Enemies
        private SpawnCardData ADTitan       = new SpawnCardData() { name = "Stone Titan",        location = "RoR2/Base/Titan/cscTitanBlackBeach.asset",             difficultyBase = 1.2f, rewardBase = 24f };
        private SpawnCardData ADVagrant     = new SpawnCardData() { name = "Wandering Vagrant",  location = "RoR2/Base/Vagrant/cscVagrant.asset",                   difficultyBase = 0.7f, rewardBase = 17f };
        private SpawnCardData ADOverlord    = new SpawnCardData() { name = "Imp Overlord",       location = "RoR2/Base/ImpBoss/cscImpBoss.asset",                   difficultyBase = 1.3f, rewardBase = 19f };
        private SpawnCardData ADTitanGold   = new SpawnCardData() { name = "Aurelionite",        location = "RoR2/Base/Titan/cscTitanGold.asset",                   difficultyBase = 1.4f, rewardBase = 30f };
        private SpawnCardData ADDunestrider = new SpawnCardData() { name = "Clay Dunestrider",   location = "RoR2/Base/ClayBoss/cscClayBoss.asset",                 difficultyBase = 1.0f, rewardBase = 22f };
        private SpawnCardData ADGrandparent = new SpawnCardData() { name = "Grandparent",        location = "RoR2/Base/Grandparent/cscGrandparent.asset",           difficultyBase = 1.6f, rewardBase = 34f };
        private SpawnCardData ADGhibli      = new SpawnCardData() { name = "Grovetender",        location = "RoR2/Base/Gravekeeper/cscGravekeeper.asset",           difficultyBase = 1.3f, rewardBase = 31f };
        private SpawnCardData ADMagmaWorm   = new SpawnCardData() { name = "Magma Worm",         location = "RoR2/Base/MagmaWorm/cscMagmaWorm.asset",               difficultyBase = 1.5f, rewardBase = 32f };
        private SpawnCardData ADOverWorm    = new SpawnCardData() { name = "Overloading Worm",   location = "RoR2/Base/ElectricWorm/cscElectricWorm.asset",         difficultyBase = 1.8f, rewardBase = 36f };
        private SpawnCardData ADRoboBall    = new SpawnCardData() { name = "Solus Control Unit", location = "RoR2/Base/RoboBallBoss/cscRoboBallBoss.asset",         difficultyBase = 1.4f, rewardBase = 23f };
        private SpawnCardData ADScav        = new SpawnCardData() { name = "Scavenger",          location = "RoR2/Base/Scav/cscScav.asset",                         difficultyBase = 1.6f, rewardBase = 37f };

        //DLC
        private SpawnCardData ADLarva       = new SpawnCardData() { name = "Acid Larva",         location = "RoR2/DLC1/AcidLarva/cscAcidLarva.asset",               difficultyBase = 0.6f, rewardBase = 10f };
        private SpawnCardData ADAssassin    = new SpawnCardData() { name = "Assassin",           location = "RoR2/DLC1/Assassin2/cscAssassin2.asset",               difficultyBase = 0.5f, rewardBase = 14f };
        private SpawnCardData ADPest        = new SpawnCardData() { name = "Blind Pest",         location = "RoR2/DLC1/FlyingVermin/cscFlyingVermin.asset",         difficultyBase = 0.7f, rewardBase = 16f };
        private SpawnCardData ADVermin      = new SpawnCardData() { name = "Blind Vermin",       location = "RoR2/DLC1/Vermin/cscVermin.asset",                     difficultyBase = 0.6f, rewardBase = 12f };
        private SpawnCardData ADBarnacle    = new SpawnCardData() { name = "Void Barnacle",      location = "RoR2/DLC1/VoidBarnacle/cscVoidBarnacle.asset",         difficultyBase = 1.0f, rewardBase = 20f };
        private SpawnCardData ADJailer      = new SpawnCardData() { name = "Void Jailer",        location = "RoR2/DLC1/VoidJailer/cscVoidJailer.asset",             difficultyBase = 1.8f, rewardBase = 38f };
        private SpawnCardData ADMegaCrab    = new SpawnCardData() { name = "Void Devastator",    location = "RoR2/DLC1/VoidMegaCrab/cscVoidMegaCrab.asset",         difficultyBase = 2.0f, rewardBase = 43f };
        private SpawnCardData ADGup         = new SpawnCardData() { name = "Gup",                location = "RoR2/DLC1/Gup/cscGupBody.asset",                       difficultyBase = 1.0f, rewardBase = 20f };
        private SpawnCardData ADInfestor    = new SpawnCardData() { name = "Void Infestor",      location = "RoR2/DLC1/EliteVoid/cscVoidInfestor.asset",            difficultyBase = 0.6f, rewardBase = 13f };
        private SpawnCardData ADMajor       = new SpawnCardData() { name = "??? Construct",  location = "RoR2/DLC1/MajorAndMinorConstruct/cscMajorConstruct.asset", difficultyBase = 1.0f, rewardBase = 20f };
        private SpawnCardData ADMinor       = new SpawnCardData() { name = "Alpha Construct",location = "RoR2/DLC1/MajorAndMinorConstruct/cscMinorConstruct.asset", difficultyBase = 0.5f, rewardBase = 11f };
        private SpawnCardData ADMega        = new SpawnCardData() { name = "Xi Construct",   location = "RoR2/DLC1/MajorAndMinorConstruct/cscMajorConstruct.asset", difficultyBase = 1.0f, rewardBase = 20f };

        //Special Enemies
        private SpawnCardData ADBrother     = new SpawnCardData() { name = "Mithrix",            location = "RoR2/Base/Brother/cscBrother.asset",                   difficultyBase = 2.0f, rewardBase = 40f };
        private SpawnCardData ADVoidling    = new SpawnCardData() { name = "Voidling",           location = "RoR2/DLC1/VoidRaidCrab/cscMiniVoidRaidCrabBase.asset", difficultyBase = 2.0f, rewardBase = 45f };

    }

    class SpawnCardData
    {
        public string name { get; set; }
        public string location { get; set; }
        public float difficultyBase { get; set; }
        public float rewardBase { get; set; }
    }
    class EliteEquipment
    {
        public string prefix { get; set; }
        public string addressable { get; set; }
    }
}

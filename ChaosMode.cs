using BepInEx;
using BepInEx.Configuration;
using RoR2;
using EntityStates;
using EntityStates.TimedChest;
using RoR2.ExpansionManagement;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ChaosMode
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.Pocket.ChaosMode", "ChaosMode", "2.0.4")]
    internal class ChaosMode : BaseUnityPlugin
    {
        public static ConfigEntry<int> chaosRate { get; set; }
        public static ConfigEntry<int> chaosSpeed { get; set; }
        public static ConfigEntry<int> corruptRate { get; set; }
        public static ConfigEntry<int> swarmRate { get; set; }
        public static ConfigEntry<int> ambushRate { get; set; }
        public static ConfigEntry<int> eventRate { get; set; }
        public static ConfigEntry<int> maxEnemies { get; set; }
        public static ConfigEntry<int> eliteRate { get; set; }
        public static ConfigEntry<int> purgeRate { get; set; }

        public void Awake()
        {
            chaosRate = Config.Bind<int>(
                "Chaos Settings",
                "ChaosRate",
                1,
                "Raises the difficulty of Chaos Mode on a scale.\nBoosts the likelyhood and maximum spawn rates for enemies."
            );
            chaosSpeed = Config.Bind<int>(
                "Chaos Settings",
                "ChaosSpeed",
                1,
                "Raises the speed that Chaos Mode enemies spawn.\nEvery (61 - ChaosSpeed) seconds, or every minute when ChaosSpeed is 1."
            );
            corruptRate = Config.Bind<int>(
                "Item Settings",
                "CorruptRate",
                10,
                "Raises the likelyhood that a corrupted item will be rolled.\nRoughly CorruptRate%. (Requires Survivors of The Void)"
            );
            swarmRate = Config.Bind<int>(
                "Spawn Settings",
                "SwarmRate",
                35,
                "Boosts the lilelyhood of enemies being spawned in swarms.\nRoughly a 1 in (10 - SwarmRate) chance."
            );
            ambushRate = Config.Bind<int>(
                "Spawn Settings",
                "AmbushRate",
                5,
                "Boosts how often higher tier boss enemies are spawned.\nRoughly every (10 - AmbushRate) enemies."
            );
            eliteRate = Config.Bind<int>(
                "Spawn Settings",
                "EliteRate",
                50,
                "Percent likelyhood that enemies spawned will be Elite. 100 will guaruntee that enemies spawned are elites (but why not just use the Artifact for that)"
            );
            maxEnemies = Config.Bind<int>(
                "Spawn Settings",
                "MaxEnemies",
                20,
                "Maximum amount of enemies that *CAN* be spawned at one time.\nDoes not factor enemies that have already been spawned.\nBe careful - this can cause game overload and lag."
            );
            eventRate = Config.Bind<int>(
                "Spawn Settings",
                "EventRate",
                15,
                "Boosts how often events are triggered.\nEventRate% of the time."
            );
            purgeRate = Config.Bind<int>(
                "Event Settings",
                "PurgeRate",
                5,
                "Limits how many items a Purge can take.\nWon't take more than PurgeRate and will always leave at least 3. (Set to 0 to disable Purge events.)"
            );

            //Randomize chest drops using our drop table method
            On.RoR2.ChestBehavior.ItemDrop += (orig, self) =>
            {
                try
                {
                    PropertyInfo dropPickupField = typeof(ChestBehavior).GetProperty("dropPickup", BindingFlags.Instance | BindingFlags.Public);
                    System.Console.WriteLine("ChaosMode Log: DropPickup is null? {0}", dropPickupField == null);

                    List<PickupIndex> newRoll = RollType(GetDropTable());
                    if (!Run.instance.IsPickupAvailable(newRoll[0])) newRoll = RollType(GetDropTable(true)); //Prevent an undroppable item

                    PickupIndex item = newRoll[random.Next(0, newRoll.Count)];
                    dropPickupField.SetValue(self, item);
                }
                catch
                {
                    System.Console.WriteLine("Error in ChestBehaviour.ItemDrop Hook");
                }

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
            //ReadSpawnCards();
            //SpawnGrandparent();
            //GiveLuck();
        }

        //Game Management     
        public IEnumerator Intro()
        {
            yield return new WaitForSeconds(1f);

            //For now we assume the expansion is installed and active

            //Give initial items
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
                baseToken = "<color=#bb0011>Welcome to CHAOS MODE!\nA Mod By Pocket"
            });
        }
        public void Initialize()
        {
            //Wait until we leave the non-game screens, otherwise cancel all coroutines
            string scene = SceneManager.GetActiveScene().name;
            if (scene != "splash" & scene != "intro" & scene != "title" & scene != "lobby" & scene != "loadingbasic")
            {
                if (!initialize)
                {
                    initialize = true;
                    StartCoroutine(Intro());
                }
            }
            else
            {
                StopAllCoroutines();
                spawning = false;
                initialize = false;
                iteration = 0;
            }
        }

        //Item Methods
        public int GetDropTable(bool restrict1 = false)
        {
            //In order,                 W > G > R > E > L > B > C 
            int[] weights = new int[] { 20, 15, 10, 15, 10, 10, restrict1 ? 0 : corruptRate.Value };
            int strength = 0, check = 0;
            foreach (int i in weights) strength += i;
            int roll = random.Next(0, strength);

            //Pick subset based on weight and order
            for (int i = 0; i < weights.Length; i++)
            {
                check += weights[i];
                if (roll < check)
                {
                    return i;
                }
            }
            return 0;
        }
        public List<PickupIndex> RollType(int item)
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
                    List<PickupIndex>[] corruptedList = new List<PickupIndex>[4] {
                        Run.instance.availableVoidTier1DropList, Run.instance.availableVoidTier2DropList,
                        Run.instance.availableVoidTier3DropList, Run.instance.availableVoidBossDropList
                    };
                    rollType = corruptedList[random.Next(0, corruptedList.Length)];
                    break;

                default:
                    break;
            }
            return rollType;
        }
        public void GiveToAllPlayers(PickupIndex pickupIndex, int count = 1)
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
        public void EquipAllPlayers(int pickupIndex)
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
        }

        //Chaos Mode Methods
        public void TimedSummon()
        {
            if (initialize)
            {
                float t = Run.instance.GetRunStopwatch();
                if (Mathf.FloorToInt(t) % (60 - Mathf.Clamp(chaosSpeed.Value - 1, 0, 45)) == 0 & t > 5)
                {
                    if (!spawning)
                    {
                        spawning = true;
                        SpawnEveryMinute();
                        StartCoroutine(FailSafeDelay());
                    }
                }
                //else
                //    spawning = false;
            }
        }
        public IEnumerator FailSafeDelay()
        {
            yield return new WaitForSeconds(1f);
            spawning = false;
        }
        public int GetInstanceTable()
        {
            //In order,                 S > B > E
            int[] weights = new int[] { swarmRate.Value, Mathf.Clamp(100 - swarmRate.Value - eventRate.Value, 0, 100), eventRate.Value };
            int strength = 0, check = 0;           
            foreach (int i in weights) strength += i;
            int roll = random.Next(0, strength);

            for (int i = 0; i < weights.Length; i++)
            {
                check += weights[i];
                if (roll < check)
                {
                    return i;
                }
            }
            return 0;
        }
        public void SpawnEveryMinute()
        {
            SpawnCardData[] normalEnemies = new SpawnCardData[] { Vagrant, BeetleQueen, GreaterWisp, RoboBallBoss, ClayBruiser, Parent, TitanPlains };
            SpawnCardData[] heavyEnemies = new SpawnCardData[] { TitanGold, GraveKeeper, MagmaWorm, ArchWisp, Brother, Nullifier, ClayBoss, LunarGolem, LunarWisp, ImpBoss, Grandparent };
            SpawnCardData[] swarmEnemies = new SpawnCardData[] { Beetle, BeetleGuard, LesserWisp, GreaterWisp, Bell, Bison, Imp, Golem, Lemurian, Jellyfish, HermitCrab, Parent, Vulture };
            SpawnCardData[] voidDLCEnemies = new SpawnCardData[] { Larva, Grenadier, Pest, Gup, Major, Vermin, Barnacle, Jailer, MegaCrab, Voidling, Infestor };

            //int[] itemOrder = new int[] { 0, 0, 1, 0, 0, 0, 1, 0, 2, 4, 0, 0, 1, 0, 1, 3, 0, 1, 0, 0, 1, 0, 1, 0, 4, 0, 0 };
            int[] enemyOrder = new int[] { 1, 1, 1, 1, 2, 1, 1, 2, 2, 1, 1, 1, 1, 1, 3, 2, 1, 1, 1, 1, 1, 3, 1, 1, 3, 1, 2 };

            SpawnCardData enemy = null;
            List<PickupIndex> newRoll = null;
            int type = 0;

            switch (GetInstanceTable())
            {
                case 0:
                    //Swarm Spawn
                    enemy = swarmEnemies[random.Next(0, swarmEnemies.Length)];
                    SummonEnemy(enemy, random.Next(5, 10) * Mathf.Clamp(Mathf.FloorToInt((float)Run.instance.GetDifficultyScaledCost(1) / 20f), 1, 5));

                    type = GetDropTable();
                    newRoll = RollType(GetDropTable());
                    if (type != 5) GiveToAllPlayers(newRoll[random.Next(0, newRoll.Count)]); else EquipAllPlayers(newRoll[random.Next(0, newRoll.Count)].value);
                    break;

                case 1:
                    //Spawn Single Enemy
                    //enemy = voidDLCEnemies[random.Next(0, voidDLCEnemies.Length)];
                    enemy = (iteration + 1) % (10 - Mathf.Clamp(ambushRate.Value, 0, 9)) == 0 ? heavyEnemies[random.Next(0, heavyEnemies.Length)] : normalEnemies[random.Next(0, normalEnemies.Length)];
                    SummonEnemy(enemy, enemyOrder[iteration] * Mathf.Clamp(Mathf.FloorToInt((float)Run.instance.GetDifficultyScaledCost(5) / 5f), 1, 10));

                    type = GetDropTable();
                    newRoll = RollType(GetDropTable());
                    if (type != 5) GiveToAllPlayers(newRoll[random.Next(0, newRoll.Count)]); else EquipAllPlayers(newRoll[random.Next(0, newRoll.Count)].value);
                    break;

                case 2:
                    //Event
                    List<IEnumerator> events = false ? new List<IEnumerator>() { JellyfishEvent(), PurgeAllItems(), EliteParentEvent(), FinalEncounter(), Corruption() } :
                                                       new List<IEnumerator>() { JellyfishEvent(), purgeRate.Value > 0 ? PurgeAllItems() : JellyfishEvent(), EliteParentEvent(), FinalEncounter() };
                    StartCoroutine(events[random.Next(0, events.Count)]);
                    break;
            }

            if (iteration >= enemyOrder.Length) { iteration = 0; }
        }

        //Chaos Events
        public IEnumerator JellyfishEvent()
        {
            Chat.SendBroadcastChat(new Chat.SimpleChatMessage
            {
                baseToken = "<color=#bb0011>CHAOS MODE:\n<color=#ff0000>Jellyfish event! Zzzap!</color>"
            });

            CharacterSpawnCard spawnCard = Resources.Load<CharacterSpawnCard>(Jellyfish.location);
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
        public IEnumerator EliteParentEvent()
        {
            Chat.SendBroadcastChat(new Chat.SimpleChatMessage
            {
                baseToken = "<color=#bb0011>CHAOS MODE:\n<color=#ff0000>Elite Parent event! Let Chaos reign!</color>"
            });

            CharacterSpawnCard spawnCard = Resources.Load<CharacterSpawnCard>(Parent.location);
            foreach (PlayerCharacterMasterController player in PlayerCharacterMasterController.instances)
            {
                GameObject spawnedInstance = SpawnEnemy(spawnCard, player.master.GetBody().transform.position).spawnedInstance;
                spawnedInstance.GetComponent<CharacterMaster>().inventory.SetEquipmentIndex((EquipmentIndex)Fire);
                StartCoroutine(CheckIfEnemyDied(spawnedInstance));

                spawnedInstance = SpawnEnemy(spawnCard, player.master.GetBody().transform.position).spawnedInstance;
                spawnedInstance.GetComponent<CharacterMaster>().inventory.SetEquipmentIndex((EquipmentIndex)Ice);
                StartCoroutine(CheckIfEnemyDied(spawnedInstance));

                spawnedInstance = SpawnEnemy(spawnCard, player.master.GetBody().transform.position).spawnedInstance;
                spawnedInstance.GetComponent<CharacterMaster>().inventory.SetEquipmentIndex((EquipmentIndex)Lightning);
                StartCoroutine(CheckIfEnemyDied(spawnedInstance));
            }

            yield return null;
        }
        public IEnumerator PurgeAllItems()
        {
            Chat.SendBroadcastChat(new Chat.SimpleChatMessage
            {
                baseToken = "<color=#bb0011>CHAOS MODE:\n<color=#ff0000>Purge event!\nYou don't need these, right?</color>"
            });

            foreach (PlayerCharacterMasterController player in PlayerCharacterMasterController.instances)
            {
                StartCoroutine(Purge(player));
            }

            yield return null;
        }
        public IEnumerator FinalEncounter()
        {
            Chat.SendBroadcastChat(new Chat.SimpleChatMessage
            {
                baseToken = "<color=#bb0011>CHAOS MODE:\n<color=#ff0000>Final event!\nTime to face the King of Nothing!</color>"
            });

            foreach (PlayerCharacterMasterController player in PlayerCharacterMasterController.instances)
            {
                StartCoroutine(Purge(player));
            }

            CharacterSpawnCard spawnCard = Resources.Load<CharacterSpawnCard>(Brother.location);
            PlayerCharacterMasterController origin = PlayerCharacterMasterController.instances[0];
            GameObject spawnedInstance = SpawnEnemy(spawnCard, origin.master.GetBody().transform.position).spawnedInstance;
            StartCoroutine(CheckIfEnemyDied(spawnedInstance));

            yield return null;
        }
        public IEnumerator Purge(PlayerCharacterMasterController player)
        {
            CharacterMaster master = player.master;
            List<ItemIndex> inventory = master.inventory.itemAcquisitionOrder;
            int cap = random.Next(3, inventory.Count - 2), j = 0;
            for (int i = inventory.Count - 1; i >= cap; i--)
            {
                master.inventory.RemoveItem(inventory[i], Mathf.Min(master.inventory.GetItemCount(inventory[i]), 2147483647));
                yield return new WaitForSeconds(1f);

                j++;
                if (j > purgeRate.Value)
                    break;
            }
        }
        public IEnumerator Corruption()
        {
            Chat.SendBroadcastChat(new Chat.SimpleChatMessage
            {
                baseToken = "<color=#bb0011>CHAOS MODE:\n<color=#ff0000>[T?he V??oid to?uc?hes y?ou!]</color>"
            });

            int corrupt = random.Next(1, 5);
            for (int i = 0; i < corrupt; i--)
            {
                List<PickupIndex> corruption = RollType(random.Next(6, 10));
                GiveToAllPlayers(corruption[random.Next(0, corruption.Count)]);
                yield return new WaitForSeconds(1f);
            }
        }

        //Enemy Spawns
        public void SummonEnemy(SpawnCardData enemyType, int reps)
        {
            System.Console.WriteLine(enemyType.name);

            int loop = 0;
            //int[] elements, elementsEasy = new int[] { Lightning, Fire, Ice }, 
            int[] elementsHard = new int[] { Lightning, Fire, Ice, Ghost, Poison, Echo };
            float difficulty = 0, threshold = 1;
            string elementName = "";

            //Threshold gets lower, until it's at 0.5f
            threshold =  1.9f - (((float)eliteRate.Value / 100f) * 1.5f);
            difficulty = 2 - Mathf.Clamp((Run.instance.GetDifficultyScaledCost(reps) * enemyType.difficultyBase * Random.Range(0.7f, 1.3f)) / Run.instance.GetDifficultyScaledCost(reps), 0.5f, 2);
            System.Console.WriteLine("Difficulty is {0} > Elite Threshold is {1}", difficulty, threshold);

            //Get an element and bool based on the difficulty
            bool getElement = difficulty > threshold ? random.Next(0, 2) == 0 ? true : false : false;
            int element = elementsHard[Random.Range(0, elementsHard.Length)]; //Just use full element list for now

            //If the difficulty is lower than 1-2 based on the threshold, then use harder elite types (weaker enemies are more likely to be elite
            //elements = difficulty < threshold ? elementsHard : difficulty < (threshold * 1.1f) ? elementsEasy : elementsEasy;
            //int element = elements[Random.Range(0, elements.Length)];
            //bool getElement = eliteRate.Value == 0 ? false : difficulty < (threshold * 1.5f) & Random.Range(0f, difficulty * 3) <= (difficulty / 2);

            CharacterSpawnCard spawnCard = Resources.Load<CharacterSpawnCard>(enemyType.location);
            int count = Mathf.Clamp(Mathf.FloorToInt(reps * Mathf.FloorToInt(1 + (chaosRate.Value / 10))), 1, Mathf.Clamp(maxEnemies.Value, 1, 50));
            for (int i = 0; i < count; i++)
            {
                foreach (PlayerCharacterMasterController player in PlayerCharacterMasterController.instances)
                {
                    GameObject spawnedInstance = SpawnEnemy(spawnCard, player.master.GetBody().transform.position).spawnedInstance;
                    if (getElement)
                    {
                        spawnedInstance.GetComponent<CharacterMaster>().inventory.SetEquipmentIndex((EquipmentIndex)element);
                        elementName = 
                            element == Fire  ? " Blazing"   : element == Ice    ? " Glacial"   : element == Lightning ? " Overloading" :
                            element == Ghost ? " Celestine" : element == Poison ? " Malachite" : element == Echo      ? "Perfected"    : "";
                        count = Mathf.Clamp(count - 1, 1, 50);
                    }
                    StartCoroutine(CheckIfEnemyDied(spawnedInstance));
                    loop++;
                }
            }
            Chat.SendBroadcastChat(new Chat.SimpleChatMessage
            {
                baseToken = "<color=#bb0011>CHAOS MODE:\n<color=#ff0000>Summoning " + loop + " " + elementName + " " + enemyType.name + (loop > 1 ? "s" : "") + "!</color>"
            });

            return;
        }
        public SpawnCard.SpawnResult SpawnEnemy(CharacterSpawnCard spawnCard, Vector3 center)
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
                teamIndexOverride = new TeamIndex?(TeamIndex.Monster),
                ignoreTeamMemberLimit = true
            };
            return spawnCard.DoSpawn(center + new Vector3(random.Next(-25, 25), 10f, random.Next(-25, 25)), Quaternion.identity, spawnRequest);
        }
        public IEnumerator CheckIfEnemyDied(GameObject enemy)
        {
            while (enemy != null)
                yield return new WaitForSeconds(0.1f);

            foreach (PlayerCharacterMasterController player in PlayerCharacterMasterController.instances)
                player.GetComponent<CharacterMaster>().GiveMoney((uint)Run.instance.GetDifficultyScaledCost(20));
        }

        //Admin Extras
        //Ease of Use
        /*
        public void ReadSpawnCards()
        {
            if (Input.GetKeyDown(KeyCode.F1))
                /foreach (SpawnCard s in Resources.LoadAll<SpawnCard>("SpawnCards/CharacterSpawnCards/"))
                //foreach (ExpansionDef s in Resources.LoadAll<ExpansionDef>("Common"))
                    Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                    {
                        baseToken = s.name
                    });
        }
        public void SpawnGrandparent()
        {
            if (Input.GetKeyDown(KeyCode.F1))
                SummonEnemy(Grandparent, 1);
        }
        int itemAdminSetindex = 0, itemAdminSubsetIndex = 0;
        public void GiveLuck()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                itemAdminSetindex++;
                Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                {
                    baseToken = "<color=#bb0011>Index " + itemAdminSetindex + "</color>"
                });
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                itemAdminSetindex--;
                Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                {
                    baseToken = "<color=#bb0011>Index " + itemAdminSetindex + "</color>"
                });
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                itemAdminSubsetIndex++;
                Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                {
                    baseToken = "<color=#bb0011>Item Set " + itemAdminSubsetIndex + "</color>"
                });
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                itemAdminSubsetIndex--;
                Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                {
                    baseToken = "<color=#bb0011>Item Set " + itemAdminSubsetIndex + "</color>"
                });
            }

            if (Input.GetKeyDown(KeyCode.F2))
            {
                switch (itemAdminSubsetIndex)
                {
                    default:
                        GiveToAllPlayers(RollType(itemAdminSubsetIndex)[itemAdminSetindex], 0);
                        break;

                    case 5:
                        EquipAllPlayers(itemAdminSetindex);
                        break;
                }
            }
        }
        public void GiveRadioScanner()
        {

        }
        */

        System.Random random = new System.Random();
        bool initialize = false, spawning = false;
        int iteration = 0;
        int Fire = 12, Ice = 15, Lightning = 16, Ghost = 14, Poison = 18, Echo = 17, Yellow = 8, Gold = 2;

        public SpawnCardData ArchWisp = new SpawnCardData() { name = "Arch Wisp", location = "SpawnCards/CharacterSpawnCards/cscArchWisp", difficultyBase = 1.1f };
        public SpawnCardData BackupDrone = new SpawnCardData() { name = "Backup Drone", location = "SpawnCards/CharacterSpawnCards/cscBackupDrone" };
        public SpawnCardData Beetle = new SpawnCardData() { name = "Beetle", location = "SpawnCards/CharacterSpawnCards/cscBeetle", difficultyBase = 0.2f };
        public SpawnCardData BeetleCrystal = new SpawnCardData() { name = "Crystal Beetle", location = "SpawnCards/CharacterSpawnCards/cscBeetleCrystal" };
        public SpawnCardData BeetleGuardAlly = new SpawnCardData() { name = "Beetle Guard", location = "SpawnCards/CharacterSpawnCards/cscBeetleGuardAlly" };
        public SpawnCardData BeetleGuard = new SpawnCardData() { name = "Beetle Guard", location = "SpawnCards/CharacterSpawnCards/cscBeetleGuard", difficultyBase = 0.4f };
        public SpawnCardData BeetleQueen = new SpawnCardData() { name = "Beetle Queen", location = "SpawnCards/CharacterSpawnCards/cscBeetleQueen", difficultyBase = 0.9f };
        public SpawnCardData Bell = new SpawnCardData() { name = "Brass Contraption", location = "SpawnCards/CharacterSpawnCards/cscBell", difficultyBase = 0.6f };
        public SpawnCardData Bison = new SpawnCardData() { name = "Bison", location = "SpawnCards/CharacterSpawnCards/cscBison", difficultyBase = 0.4f };
        public SpawnCardData Brother = new SpawnCardData() { name = "Mithrix", location = "SpawnCards/CharacterSpawnCards/cscBrother", difficultyBase = 2f };
        public SpawnCardData ClayBoss = new SpawnCardData() { name = "Clay Dunestrider", location = "SpawnCards/CharacterSpawnCards/cscClayBoss", difficultyBase = 1.4f };
        public SpawnCardData ClayBruiser = new SpawnCardData() { name = "Clay Templar", location = "SpawnCards/CharacterSpawnCards/cscClayBruiser", difficultyBase = 1.1f };
        public SpawnCardData ElectricWorm = new SpawnCardData() { name = "Overloading Worm", location = "SpawnCards/CharacterSpawnCards/cscElectricWorm", difficultyBase = 1.7f };
        public SpawnCardData Golem = new SpawnCardData() { name = "Stone Golem", location = "SpawnCards/CharacterSpawnCards/cscGolem", difficultyBase = 0.5f };
        public SpawnCardData GraveKeeper = new SpawnCardData() { name = "Grovetender", location = "SpawnCards/CharacterSpawnCards/cscGraveKeeper", difficultyBase = 1.3f };
        public SpawnCardData GreaterWisp = new SpawnCardData() { name = "Greater Wisp", location = "SpawnCards/CharacterSpawnCards/cscGreaterWisp", difficultyBase = 0.8f };
        public SpawnCardData HermitCrab = new SpawnCardData() { name = "Hermit Crab", location = "SpawnCards/CharacterSpawnCards/cscHermitCrab", difficultyBase = 0.1f };
        public SpawnCardData Imp = new SpawnCardData() { name = "Imp", location = "SpawnCards/CharacterSpawnCards/cscImp", difficultyBase = 0.6f };
        public SpawnCardData ImpBoss = new SpawnCardData() { name = "Imp Overlord", location = "SpawnCards/CharacterSpawnCards/cscImpBoss", difficultyBase = 1.1f };
        public SpawnCardData Jellyfish = new SpawnCardData() { name = "Jellyfish", location = "SpawnCards/CharacterSpawnCards/cscJellyfish", difficultyBase = 0.1f };
        public SpawnCardData Lemurian = new SpawnCardData() { name = "Lemurian", location = "SpawnCards/CharacterSpawnCards/cscLemurian", difficultyBase = 0.2f };
        public SpawnCardData LemurianBruiser = new SpawnCardData() { name = "Elder Lemurian", location = "SpawnCards/CharacterSpawnCards/cscLemurianBruiser", difficultyBase = 0.8f };
        public SpawnCardData LesserWisp = new SpawnCardData() { name = "Lesser Wisp", location = "SpawnCards/CharacterSpawnCards/cscLesserWisp", difficultyBase = 0.2f };
        public SpawnCardData LunarGolem = new SpawnCardData() { name = "Lunar Chimera", location = "SpawnCards/CharacterSpawnCards/cscLunarGolem", difficultyBase = 0.9f };
        public SpawnCardData LunarWisp = new SpawnCardData() { name = "Lunar Chimera", location = "SpawnCards/CharacterSpawnCards/cscLunarWisp", difficultyBase = 0.9f };
        public SpawnCardData MagmaWorm = new SpawnCardData() { name = "Magma Worm", location = "SpawnCards/CharacterSpawnCards/cscMagmaWorm", difficultyBase = 1.6f };
        public SpawnCardData MiniMushroom = new SpawnCardData() { name = "Mini Mushroom", location = "SpawnCards/CharacterSpawnCards/cscMiniMushroom", difficultyBase = 0.6f };
        public SpawnCardData Nullifier = new SpawnCardData() { name = "Void Reaver", location = "SpawnCards/CharacterSpawnCards/cscNullifier", difficultyBase = 1.2f };
        public SpawnCardData Parent = new SpawnCardData() { name = "Parent", location = "SpawnCards/CharacterSpawnCards/cscParent", difficultyBase = 0.9f };
        public SpawnCardData RoboBallBoss = new SpawnCardData() { name = "Solus Control Unit", location = "SpawnCards/CharacterSpawnCards/cscRoboBallBoss", difficultyBase = 1.2f };
        public SpawnCardData Vagrant = new SpawnCardData() { name = "Wandering Vagrant", location = "SpawnCards/CharacterSpawnCards/cscVagrant", difficultyBase = 1f };
        public SpawnCardData TitanPlains = new SpawnCardData() { name = "Stone Titan", location = "SpawnCards/CharacterSpawnCards/titan/cscTitanGolemPlains", difficultyBase = 1f };
        public SpawnCardData TitanGold = new SpawnCardData() { name = "Aurelionite", location = "SpawnCards/CharacterSpawnCards/titan/cscTitanGold", difficultyBase = 1.5f };
        public SpawnCardData Vulture = new SpawnCardData() { name = "Alloy Vulture", location = "SpawnCards/CharacterSpawnCards/cscVulture", difficultyBase = 0.8f };
        public SpawnCardData Grandparent = new SpawnCardData() { name = "Grandparent", location = "SpawnCards/CharacterSpawnCards/titan/cscGrandParent", difficultyBase = 1.6f };

        //DLC
        public SpawnCardData Larva = new SpawnCardData() { name = "Acid Larva", location = "SpawnCards/CharacterSpawnCards/cscAcidLarva", difficultyBase = 0.3f };
        public SpawnCardData Grenadier = new SpawnCardData() { name = "Clay Apothecary", location = "SpawnCards/CharacterSpawnCards/cscClayGrenadier", difficultyBase = 1.2f };
        public SpawnCardData Pest = new SpawnCardData() { name = "Blind Pest", location = "SpawnCards/CharacterSpawnCards/FlyingVermin/cscFlyingVermin", difficultyBase = 0.6f };
        public SpawnCardData Gup = new SpawnCardData() { name = "Gup", location = "SpawnCards/CharacterSpawnCards/Gup/cscGupBody", difficultyBase = 1f };
        public SpawnCardData Major = new SpawnCardData() { name = "Xi Construct", location = "SpawnCards/CharacterSpawnCards/MajorAndMinorConstruct/cscMajorConstruct", difficultyBase = 1.4f };
        public SpawnCardData Vermin = new SpawnCardData() { name = "Blind Vermin", location = "SpawnCards/CharacterSpawnCards/Vermin/cscVermin", difficultyBase = 0.3f };
        public SpawnCardData Barnacle = new SpawnCardData() { name = "Void Barnacle", location = "SpawnCards/CharacterSpawnCards/Vermin/cscVoidBarnacle", difficultyBase = 0.6f };
        public SpawnCardData Jailer = new SpawnCardData() { name = "Void Jailer", location = "SpawnCards/CharacterSpawnCards/Vermin/cscVoidJailer", difficultyBase = 1.6f };
        public SpawnCardData MegaCrab = new SpawnCardData() { name = "Void Devastator", location = "SpawnCards/CharacterSpawnCards/Vermin/cscVoidMegaCrab", difficultyBase = 1.8f };
        public SpawnCardData Voidling = new SpawnCardData() { name = "Voidling", location = "SpawnCards/CharacterSpawnCards/Vermin/cscVoidRaidCrab", difficultyBase = 3f };
        public SpawnCardData Infestor = new SpawnCardData() { name = "Void Infestor", location = "SpawnCards/CharacterSpawnCards/Vermin/cscVoidInfestor", difficultyBase = 0.9f };
    }

    class SpawnCardData {
        public string name { get; set; }
        public string location { get; set; }
        public float difficultyBase { get; set; }
    }
}

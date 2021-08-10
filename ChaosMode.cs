using BepInEx;
using BepInEx.Configuration;
using EntityStates;
using EntityStates.TimedChest;
using RoR2;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ChaosMode
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.Pocket.ChaosMode", "ChaosMode", "1.0.2")]
    internal class ChaosMode : BaseUnityPlugin
    {
        public static ConfigEntry<int> chaosRate { get; set; }
        public static ConfigEntry<int> chaosSpeed { get; set; }
        public static ConfigEntry<int> swarmRate { get; set; }
        public static ConfigEntry<int> ambushRate { get; set; }
        public static ConfigEntry<int> eventRate { get; set; }
        public static ConfigEntry<int> maxEnemies { get; set; }
        public static ConfigEntry<int> eliteRate { get; set; }

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
            swarmRate = Config.Bind<int>(
                "Spawn Settings",
                "SwarmRate",
                3,
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
                "Event Settings",
                "EventRate",
                1,
                "Boosts how often events are triggered.\nRoughly every 1 in (10 - EventRate) cycle."
            );
           
            On.RoR2.ChestBehavior.ItemDrop += (orig, self) =>
            {
                FieldInfo dropPickup = self.GetType().GetField("dropPickup", BindingFlags.Instance | BindingFlags.NonPublic);
                PickupIndex pickupIndex = (PickupIndex)dropPickup.GetValue(self);
                if (pickupIndex != PickupIndex.none)
                {
                    EntityStateMachine component = self.GetComponent<EntityStateMachine>();
                    if (component)
                    {
                        //component.SetNextState(new SerializableEntityStateType(typeof(Opening)));
                        //EntityState chestState = new SerializableEntityStateType(typeof(Opening));
                        //component.SetNextState(Instantiate<EntityState>(new SerializableEntityStateType(typeof(Opening)));

                        List<PickupIndex> newRoll = RollType(Random.Range(1, 6));
                        dropPickup.SetValue(self, newRoll[random.Next(0, newRoll.Count)]);
                        orig(self);
                        return;
                    }
                    dropPickup.SetValue(self, PickupIndex.none);
                    orig(self);
                }
            };
            On.RoR2.ChestBehavior.Open += (orig, self) =>
            {
                if ((PickupIndex)self.GetType().GetField("dropPickup", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(self) == PickupIndex.none)
                {
                    self.ItemDrop();
                    return;
                }
                orig.Invoke(self);
            };
        }
        public void Update()
        {
            //Chaos Mode Functions
            Initialize();
            TimedSummon();
        }

        //Admin Management     
        public IEnumerator Intro()
        {
            yield return new WaitForSeconds(1f);

            //Give initial items
            List<PickupIndex> newRoll = RollType(0);
            PickupIndex item = newRoll[random.Next(0, newRoll.Count)];

            for (int i = 0; i < 3; i++)
            {
                item = newRoll[random.Next(0, newRoll.Count)];
                GiveToAllPlayers(item, 0);
            }
            newRoll = RollType(1);
            for (int i = 0; i < 2; i++)
            {
                item = newRoll[random.Next(0, newRoll.Count)];
                GiveToAllPlayers(item, 0);
            }
            newRoll = RollType(2);
            item = newRoll[random.Next(0, newRoll.Count)];
            GiveToAllPlayers(item, 0);

            Chat.SendBroadcastChat(new Chat.SimpleChatMessage
            {
                baseToken = "<color=#bb0011>Welcome to CHAOS MODE!\nA Mod By Pocket"
            });
        }
        public void Initialize()
        {
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
                initialize = false;
                iteration = 0;
            }
        }

        //Items 0-Everyone, 1-OnlyHost
        public List<PickupIndex> RollType(int item)
        {
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
                    rollType = Run.instance.availableLunarDropList;
                    break;

                case 5:
                    rollType = Run.instance.availableEquipmentDropList;
                    break;

                default:
                    break;
            }
            return rollType;
        }
        public void GiveToAllPlayers(PickupIndex pickupIndex, int playerPreference, int count = 1)
        {
            foreach (PlayerCharacterMasterController playerCharacterMasterController in PlayerCharacterMasterController.instances)
            {
                string nameOut = playerCharacterMasterController.GetDisplayName();
                if (playerPreference == 0 ? true : playerPreference == 1 ? nameOut == "Pocket" : nameOut != "Pocket")
                {
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
        }

        //Chaos Mode
        public void TimedSummon()
        {
            if (initialize)
            {
                float t = Run.instance.GetRunStopwatch();
                if (Mathf.FloorToInt(t) % (60 - Mathf.Clamp(chaosSpeed.Value - 1, 0, 45)) == 0 & t != 0)
                {
                    if (!spawning)
                    {
                        spawning = true;
                        SpawnEveryMinute();
                    }
                }
                else
                    spawning = false;
            }
        }
        public void SpawnEveryMinute()
        {
            SpawnCardData[] normalEnemies = new SpawnCardData[] { Vagrant, BeetleQueen, GreaterWisp, RoboBallBoss, ClayBruiser, Parent };
            SpawnCardData[] heavyEnemies = new SpawnCardData[] { TitanGold, GraveKeeper, MagmaWorm, ArchWisp, Brother, Nullifier, ClayBoss, LunarGolem, LunarWisp, ImpBoss };
            SpawnCardData[] swarmEnemies = new SpawnCardData[] { Beetle, BeetleGuard, LesserWisp, GreaterWisp, Bell, Bison, Imp, Golem, Lemurian, Jellyfish, HermitCrab, Parent, Vulture };

            int[] itemOrder = new int[] { 0, 0, 1, 0, 0, 0, 1, 0, 2, 0, 0, 0, 1, 0, 1, 3, 0, 1, 0, 0, 1, 0, 1, 0, 4, 0, 0 };
            int[] enemyOrder = new int[] { 1, 1, 1, 1, 2, 1, 1, 2, 2, 1, 1, 1, 1, 1, 3, 2, 1, 1, 1, 1, 1, 3, 1, 1, 3, 1, 2 };

            if (random.Next(0, Mathf.Clamp(10 - eventRate.Value, 2, 10)) == 0)
            {
                //Event
                List<IEnumerator> events = new List<IEnumerator>() { JellyfishEvent(), PurgeAllItems(), EliteParentEvent(), FinalEncounter() };
                StartCoroutine(events[random.Next(0, events.Count)]);
            }
            else if (random.Next(0, 10 - Mathf.Clamp(swarmRate.Value, 0, 9)) != 0)
            {
                //Spawn Single Enemy
                SpawnCardData enemy = (iteration + 1) % (10 - Mathf.Clamp(ambushRate.Value, 0, 9)) == 0 ? heavyEnemies[random.Next(0, heavyEnemies.Length)] : normalEnemies[random.Next(0, normalEnemies.Length)];
                SummonEnemy(enemy, enemyOrder[iteration] * Mathf.Clamp(Mathf.FloorToInt((float)Run.instance.GetDifficultyScaledCost(5) / 5f), 1, 10));

                List<PickupIndex> newRoll = RollType(itemOrder[iteration++]);
                GiveToAllPlayers(newRoll[random.Next(0, newRoll.Count)], 0);
            }
            else
            {
                //Swarm Spawn
                SpawnCardData enemy = swarmEnemies[random.Next(0, swarmEnemies.Length)];
                SummonEnemy(enemy, random.Next(5, 10) * Mathf.Clamp(Mathf.FloorToInt((float)Run.instance.GetDifficultyScaledCost(1) / 20f), 1, 5));

                List<PickupIndex> newRoll = RollType(itemOrder[iteration++]);
                GiveToAllPlayers(newRoll[random.Next(0, newRoll.Count)], 0);
            }

            if (iteration >= itemOrder.Length) { iteration = 0; }
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
                baseToken = "<color=#bb0011>CHAOS MODE:\n<color=#ff0000>Final event!\nTime to face the king of nothing!</color>"
            });

            foreach (PlayerCharacterMasterController player in PlayerCharacterMasterController.instances)
            {
                StartCoroutine(Purge(player));
            }

            CharacterSpawnCard spawnCard = Resources.Load<CharacterSpawnCard>(Parent.location);
            PlayerCharacterMasterController origin = PlayerCharacterMasterController.instances[0];
            GameObject spawnedInstance = SpawnEnemy(spawnCard, origin.master.GetBody().transform.position).spawnedInstance;
            spawnedInstance.GetComponent<CharacterMaster>().inventory.SetEquipmentIndex((EquipmentIndex)Fire);
            StartCoroutine(CheckIfEnemyDied(spawnedInstance));

            spawnedInstance = SpawnEnemy(spawnCard, origin.master.GetBody().transform.position).spawnedInstance;
            spawnedInstance.GetComponent<CharacterMaster>().inventory.SetEquipmentIndex((EquipmentIndex)Ice);
            StartCoroutine(CheckIfEnemyDied(spawnedInstance));

            spawnedInstance = SpawnEnemy(spawnCard, origin.master.GetBody().transform.position).spawnedInstance;
            spawnedInstance.GetComponent<CharacterMaster>().inventory.SetEquipmentIndex((EquipmentIndex)Lightning);
            StartCoroutine(CheckIfEnemyDied(spawnedInstance));

            yield return null;
        }
        public IEnumerator Purge(PlayerCharacterMasterController player)
        {
            CharacterMaster master = player.master;
            List<ItemIndex> inventory = master.inventory.itemAcquisitionOrder;
            int cap = random.Next(0, inventory.Count - 2);
            for (int i = inventory.Count - 1; i >= cap; i--)
            {
                master.inventory.RemoveItem(inventory[i], Mathf.Min(master.inventory.GetItemCount(inventory[i]), 2147483647));
                yield return new WaitForSeconds(1f);
            }
        }

        //Enemy Spawns
        public void SummonEnemy(SpawnCardData enemyType, int reps)
        {
            int loop = 0;
            int[] elements;
            int[] elementsEasy = new int[] { Lightning, Fire, Ice }, elementsHard = new int[] { Lightning, Fire, Ice, Ghost, Poison };
            float difficulty = 0, threshold = 1;
            string elementName = "";

            difficulty = (Run.instance.GetDifficultyScaledCost(reps) * enemyType.difficultyBase * Random.Range(0.8f, 1.2f)) / Run.instance.GetDifficultyScaledCost(reps);
            threshold = 1 - ((float)eliteRate.Value / 100f);
            System.Console.WriteLine("Difficulty is {0}", difficulty);

            elements = difficulty < threshold ? elementsHard : difficulty < (threshold * 1.5f) ? elementsEasy : elementsEasy;
            int element = elements[Random.Range(0, elements.Length)];
            bool getElement = eliteRate.Value == 0 ? false : difficulty < (threshold * 1.5f) & Random.Range(0f, difficulty * 3) <= (difficulty / 2);

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
                        elementName = element == 6 ? " Blazing" : element == 7 ? " Glacial" : element == 0 ? " Overloading"
                            : element == 3 ? " Celestine" : element == 5 ? " Malachite" : "";
                        count--;
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
            spawnCard.eliteRules = SpawnCard.EliteRules.Lunar;

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
            {
                yield return new WaitForSeconds(0.1f);
            }
            foreach (PlayerCharacterMasterController player in PlayerCharacterMasterController.instances)
            {
                player.GetComponent<CharacterMaster>().GiveMoney((uint)Run.instance.GetDifficultyScaledCost(20));
            }
        }

        System.Random random = new System.Random();
        bool initialize = false, spawning = false;
        int iteration = 0;
        int Fire = 6, Ice = 7, Lightning = 0, Ghost = 3, Poison = 5, Echo = 1, Yellow = 8, Gold = 2;

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
        public SpawnCardData TitanPlains = new SpawnCardData() { name = "Stone Titan", location = "SpawnCards/CharacterSpawnCards/cscTitanGolemPlains", difficultyBase = 1f };
        public SpawnCardData TitanGold = new SpawnCardData() { name = "Aurelionite", location = "SpawnCards/CharacterSpawnCards/cscTitanGold", difficultyBase = 1.5f };
        public SpawnCardData Vulture = new SpawnCardData() { name = "Alloy Vulture", location = "SpawnCards/CharacterSpawnCards/cscVulture", difficultyBase = 0.8f };
        public SpawnCardData Grandparent = new SpawnCardData() { name = "Grandparent", location = "SpawnCards/CharacterSpawnCards/cscGrandparent", difficultyBase = 1.6f };

        /*
        public static readonly string ArchWisp = "SpawnCards/CharacterSpawnCards/cscArchWisp";
        public static readonly string BackupDrone = "SpawnCards/CharacterSpawnCards/cscBackupDrone";
        public static readonly string Beetle = "SpawnCards/CharacterSpawnCards/cscBeetle";
        public static readonly string BeetleCrystal = "SpawnCards/CharacterSpawnCards/cscBeetleCrystal";
        public static readonly string BeetleGuard = "SpawnCards/CharacterSpawnCards/cscBeetleGuard";
        public static readonly string BeetleGuardAlly = "SpawnCards/CharacterSpawnCards/cscBeetleGuardAlly";
        public static readonly string BeetleGuardCrystal = "SpawnCards/CharacterSpawnCards/cscBeetleGuardCrystal";
        public static readonly string BeetleQueen = "SpawnCards/CharacterSpawnCards/cscBeetleQueen";
        public static readonly string Bell = "SpawnCards/CharacterSpawnCards/cscBell";
        public static readonly string Bison = "SpawnCards/CharacterSpawnCards/cscBison";
        public static readonly string Brother = "SpawnCards/CharacterSpawnCards/cscBrother";
        public static readonly string BrotherGlass = "SpawnCards/CharacterSpawnCards/cscBrotherGlass";
        public static readonly string BrotherHurt = "SpawnCards/CharacterSpawnCards/cscBrotherHurt";
        public static readonly string ClayBoss = "SpawnCards/CharacterSpawnCards/cscClayBoss";
        public static readonly string ClayBruiser = "SpawnCards/CharacterSpawnCards/cscClayBruiser";
        public static readonly string ElectricWorm = "SpawnCards/CharacterSpawnCards/cscElectricWorm";
        public static readonly string Golem = "SpawnCards/CharacterSpawnCards/cscGolem";
        public static readonly string GraveKeeper = "SpawnCards/CharacterSpawnCards/cscGraveKeeper";
        public static readonly string GreaterWisp = "SpawnCards/CharacterSpawnCards/cscGreaterWisp";
        public static readonly string HermitCrab = "SpawnCards/CharacterSpawnCards/cscHermitCrab";
        public static readonly string Imp = "SpawnCards/CharacterSpawnCards/cscImp";
        public static readonly string ImpBoss = "SpawnCards/CharacterSpawnCards/cscImpBoss";
        public static readonly string Jellyfish = "SpawnCards/CharacterSpawnCards/cscJellyfish";
        public static readonly string Lemurian = "SpawnCards/CharacterSpawnCards/cscLemurian";
        public static readonly string LemurianBruiser = "SpawnCards/CharacterSpawnCards/cscLemurianBruiser";
        public static readonly string LesserWisp = "SpawnCards/CharacterSpawnCards/cscLesserWisp";
        public static readonly string LunarGolem = "SpawnCards/CharacterSpawnCards/cscLunarGolem";
        public static readonly string LunarWisp = "SpawnCards/CharacterSpawnCards/cscLunarWisp";
        public static readonly string MagmaWorm = "SpawnCards/CharacterSpawnCards/cscMagmaWorm";
        public static readonly string MiniMushroom = "SpawnCards/CharacterSpawnCards/cscMiniMushroom";
        public static readonly string Nullifier = "SpawnCards/CharacterSpawnCards/cscNullifier";
        public static readonly string Parent = "SpawnCards/CharacterSpawnCards/cscParent";
        public static readonly string ParentPod = "SpawnCards/CharacterSpawnCards/cscParentPod";
        public static readonly string RoboBallBoss = "SpawnCards/CharacterSpawnCards/cscRoboBallBoss";
        public static readonly string RoboBallMini = "SpawnCards/CharacterSpawnCards/cscRoboBallMini";
        public static readonly string Scav = "SpawnCards/CharacterSpawnCards/cscScav";
        public static readonly string ScavLunar = "SpawnCards/CharacterSpawnCards/cscScavLunar";
        public static readonly string SquidTurret = "SpawnCards/CharacterSpawnCards/cscSquidTurret";
        public static readonly string SuperRoboBallBoss = "SpawnCards/CharacterSpawnCards/cscSuperRoboBallBoss";
        public static readonly string TitanGold = "SpawnCards/CharacterSpawnCards/cscTitanGold";
        public static readonly string TitanGoldAlly = "SpawnCards/CharacterSpawnCards/cscTitanGoldAlly";
        public static readonly string Vagrant = "SpawnCards/CharacterSpawnCards/cscVagrant";
        public static readonly string Vulture = "SpawnCards/CharacterSpawnCards/cscVulture";
        public static readonly string Grandparent = "SpawnCards/CharacterSpawnCards/cscGrandparent";
        public static readonly string TitanBlackBeach = "SpawnCards/CharacterSpawnCards/cscTitanBlackBeach";
        public static readonly string TitanDampCave = "SpawnCards/CharacterSpawnCards/cscTitanDampCave";
        public static readonly string TitanGolemPlains = "SpawnCards/CharacterSpawnCards/cscTitanGolemPlains";
        public static readonly string TitanGooLake = "SpawnCards/CharacterSpawnCards/cscTitanGooLake";
        */
    }

    class SpawnCardData {
        public string name { get; set; }
        public string location { get; set; }
        public float difficultyBase { get; set; }
    }
}

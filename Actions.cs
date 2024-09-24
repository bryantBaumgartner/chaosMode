using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static ChaosMode.ChaosMode;
using static ChaosMode.Init;
using static ChaosMode.Models;
using static ChaosMode.Tables;

namespace ChaosMode
{
    static class Actions
    {
        public static Events eventing = new Events();

        public static void StartingItems()
        {
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

            //System.Console.WriteLine("[CHAOS] Test successful", expansion1);
        }

        public static void SpawnEveryMinute()
        {
            List<SpawnCardData> normalEnemies = new List<SpawnCardData>
            {
                ADBeetleGuard, ADGreaterWisp, ADGolem, ADTitan, ADParent, ADBigLemurian,
                ADRoboBall, ADTemplar, ADArchWisp, ADBeetleQueen, ADLunarGolem, ADLunarWisp
            };
            List<SpawnCardData> heavyEnemies = new List<SpawnCardData>
            {
                ADBeetleQueen, ADTitan, ADTitanGold, ADOverlord, ADMagmaWorm, ADOverWorm, ADNullifier,
                ADDunestrider, ADGhibli, ADGrandparent, ADMagmaWorm, ADBrother, ADScav
            };
            List<SpawnCardData> swarmEnemies = new List<SpawnCardData>
            {
                ADBeetle, ADWisp, ADGreaterWisp, ADBell, ADBison, ADImp, ADGolem, ADLemurian,
                ADJellyfish, ADHermitCrab, ADParent, ADVulture, ADLunarBomb, ADMushroom
            };

            //Add the first expansion's enemies to the normal pool of enemies
            if (expansion1)
            {
                List<SpawnCardData> ex1NormalEnemies = new List<SpawnCardData> { ADGup, ADJailer, ADApothecary, ADBarnacle };
                List<SpawnCardData> ex1HeavyEnemies = new List<SpawnCardData> { ADJailer, ADMegaCrab, ADMega, ADMajor, ADVoidling };
                List<SpawnCardData> ex1SwarmEnemies = new List<SpawnCardData> { ADLarva, ADPest, ADVermin, ADMinor, ADApothecary, ADAssassin, ADInfestor, ADBarnacle };
                normalEnemies.AddRange(ex1NormalEnemies);
                heavyEnemies.AddRange(ex1HeavyEnemies);
                swarmEnemies.AddRange(ex1SwarmEnemies);
            }

            //Add the second expansion's enemies to the normal pool of enemies
            if (expansion2)
            {
                List<SpawnCardData> ex2NormalEnemies = new List<SpawnCardData> { ADScorchling };
                List<SpawnCardData> ex2HeavyEnemies = new List<SpawnCardData> { ADFalseSon };
                List<SpawnCardData> ex2SwarmEnemies = new List<SpawnCardData> { ADChild };
                normalEnemies.AddRange(ex2NormalEnemies);
                heavyEnemies.AddRange(ex2HeavyEnemies);
                swarmEnemies.AddRange(ex2SwarmEnemies);
            }

            SpawnCardData enemy = null;
            List<PickupIndex> newRoll = null;
            int type = 0, number = 1;

            //Increment the pseudo-director value for spawning higher level enemies (this is so scuffed)
            directorValue += Run.instance.GetDifficultyScaledCost(random.Next(1, 3)) / (Mathf.Clamp(10f - ((ambushRate.Value / 100f) * 8f), 2f, 10f));
            System.Console.WriteLine("[CHAOS] Director Aggro Value: {0}", directorValue);

            switch (SummonDropTable()) //New generic drop table system (shouldn't change much)
            {
                case 0:
                    //Swarm Spawn
                    System.Console.WriteLine("[CHAOS] spawn swarm");
                    enemy = swarmEnemies[random.Next(0, swarmEnemies.Count)];
                    number = Mathf.Clamp(Run.instance.GetDifficultyScaledCost(random.Next(2, 4)) * Mathf.Clamp(swarmAggression.Value, 1, spawnLimit.Value ? 3 : 1024),
                        5, spawnLimit.Value ? maxEnemies.Value : 65536);

                    SummonEnemy(enemy, number);
                    ItemEveryMinute();
                    break;

                case 1:
                    //Spawn Single Enemy
                    System.Console.WriteLine("[CHAOS] spawn single enemy");
                    //int difficultyBase = Mathf.Clamp(Run.instance.GetDifficultyScaledCost(1) - 1, 0, ambushRate.Value); //This should scale the rate of higher tier enemies over time
                    //enemy = (random.Next(0, 100 - difficultyBase) < ambushRate.Value) ? heavyEnemies[random.Next(0, heavyEnemies.Count)] : normalEnemies[random.Next(0, normalEnemies.Count)];
                    enemy = normalEnemies[random.Next(0, normalEnemies.Count)];
                    number = Mathf.Clamp(Run.instance.GetDifficultyScaledCost(random.Next(1, 3)), 1, spawnLimit.Value ? maxEnemies.Value : 65536);

                    SummonEnemy(enemy, number);
                    ItemEveryMinute();
                    break;

                case 2:
                    //Event
                    System.Console.WriteLine("[CHAOS] spawn event");
                    List<IEnumerator> events = new List<IEnumerator>() { eventing.JellyfishEvent(), eventing.EliteParentEvent(),
                        eventing.FinalEncounter(), eventing.GainFriend(),
                        eventing.GoldEvent() };
                    if (purgeRate.Value > 0) events.Add(eventing.PurgeAllItems());
                    if (enableOrder.Value) events.Add(eventing.SequenceEvent());
                    if (expansion1) events.AddRange(new List<IEnumerator>() { eventing.Corruption(), eventing.VoidEncounter() });

                    instance.StartCoroutine(events[EventDropTable()]); //Uses our new drop table system to weigh events
                    break;

                case 3:
                    //Spawn Boss Enemy
                    System.Console.WriteLine("[CHAOS] spawn boss");
                    //int difficultyBase = Mathf.Clamp(Run.instance.GetDifficultyScaledCost(1) - 1, 0, ambushRate.Value); //This should scale the rate of higher tier enemies over time
                    enemy = heavyEnemies[random.Next(0, heavyEnemies.Count)];
                    number = Mathf.Clamp(Run.instance.GetDifficultyScaledCost(1), 1, spawnLimit.Value ? maxEnemies.Value : 65536);
                    SummonEnemy(enemy, number);

                    if (giveItems.Value)
                    {
                        type = GetDropTable(restrictEquipment: true);
                        newRoll = RollType(GetDropTable()); //New generic drop table system (shouldn't change much)
                        GiveToAllPlayers(newRoll[random.Next(0, newRoll.Count)]);
                        //if (type != Equipment) GiveToAllPlayers(newRoll[random.Next(0, newRoll.Count)]);
                        //else EquipAllPlayers(random.Next(0, newRoll.Count));
                    }
                    break;
            }
        }
        private static void ItemEveryMinute()
        {
            if (!giveItems.Value) return;

            int type = GetDropTable(restrictEquipment: true, restrictLunar: includeLunar.Value);
            List<PickupIndex> newRoll = RollType(type);
            GiveToAllPlayers(newRoll[random.Next(0, newRoll.Count)]);

            //if (type != Equipment) GiveToAllPlayers(newRoll[random.Next(0, newRoll.Count)]);
            //else EquipAllPlayers(random.Next(0, newRoll.Count));
        }


        //Give items to all network players
        public static void GiveToAllPlayers(PickupIndex pickupIndex, int count = 1)
        {
            //Loop through players and give them each the same pickupindex
            foreach (PlayerCharacterMasterController playerCharacterMasterController in PlayerCharacterMasterController.instances)
            {
                try
                {
                    string nameOut = playerCharacterMasterController.GetDisplayName();
                    System.Console.WriteLine(nameOut);
                    CharacterMaster master = playerCharacterMasterController.master;
                    master.inventory.GiveItem(PickupCatalog.GetPickupDef(pickupIndex).itemIndex, count);
                    MethodInfo method = typeof(GenericPickupController).GetMethod("SendPickupMessage", BindingFlags.Static | BindingFlags.NonPublic);
                    method.Invoke(null, new object[]
                    {
                    master,
                    pickupIndex
                    });
                }
                catch (Exception e)
                {
                    System.Console.WriteLine(e.Message);
                }
            }
        }
        public static void GiveToOnePlayer(PlayerCharacterMasterController playerCharacterMasterController, PickupIndex pickupIndex, int count = 1)
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
        public static void EquipAllPlayers(int pickupIndex)
        {
            //This code worked for like one day and then started crashing every subsequent version of this mod.

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
        public static void EquipOneElite(Inventory inventory, EliteEquipment eliteType)
        {
            EquipmentDef elite = null;
            elite = Addressables.LoadAssetAsync<EquipmentDef>(eliteType.addressable).WaitForCompletion();

            inventory.SetEquipmentIndex(elite.equipmentIndex);
        }

        //Enemy methods
        public static void SummonEnemy(SpawnCardData enemyType, int reps)
        {
            reps = Mathf.Clamp(reps, 1, spawnLimit.Value ? maxEnemies.Value : 50);
            string elementName = "";
            float difficulty = 0, threshold = 1, roll = 0;

            //Elite types
            List<EliteEquipment> eliteTypes = new List<EliteEquipment>() { ADFire, ADIce, ADLightning, ADGhost, ADPoison, ADEcho };
            if (expansion1) eliteTypes.AddRange(new List<EliteEquipment>() { ADEarth, ADVoid });

            //Threshold gets lower, until it's at 0.5f
            
            threshold = Mathf.Clamp((float)eliteRate.Value / 100f, 0f, 1f);
            difficulty = Mathf.Clamp(Run.instance.GetDifficultyScaledCost(1) / 100f, 0f, 1f - threshold);
            roll = random.Next(0, (int)Mathf.Clamp(threshold + Run.instance.GetDifficultyScaledCost(1), 0, 100)) / 100f;
            //threshold = 0.5f + (((float)eliteRate.Value / Mathf.Clamp(100f - Run.instance.GetDifficultyScaledCost(1), 0, 50)) * 1.9f);
            //difficulty = Mathf.Clamp(2 - Mathf.Clamp((Run.instance.GetDifficultyScaledCost(reps) * enemyType.difficultyBase * (random.Next(7, 13) / 10f)) / Run.instance.GetDifficultyScaledCost(reps), 0.5f, 2), 0.5f, 2);
            System.Console.WriteLine("[Chaos Log] Roll is {0} >= Elite Threshold is {1}", roll, threshold + difficulty);

            //Get an element and bool based on the difficulty
            bool getElement = roll >= threshold + difficulty ? true : false;
            //bool getElement = threshold + difficulty >= roll ? random.Next(0, 2) == 0 ? true : false : false;
            EliteEquipment elite = eliteTypes[EliteDropTable()];

            //Failsafe in case the SpawnCard doesn't exist
            try
            {
                //Check to see if this needs to scale anymore
                int count = Mathf.Clamp(reps / (getElement ? 2 : 1), 1, reps);
                var players = PlayerCharacterMasterController.instances;

                //Addressable Resource loading
                CharacterSpawnCard spawnCard = null;
                spawnCard = Addressables.LoadAssetAsync<CharacterSpawnCard>(enemyType.location).WaitForCompletion();

                int loop = 0;
                for (int i = 0; i < Mathf.Clamp(count / players.Count, 1, reps); i++)
                {
                    foreach (PlayerCharacterMasterController player in players)
                    {
                        //Legacy spawn system
                        GameObject spawnedInstance = SpawnEnemy(spawnCard, player.master.GetBody().transform.position).spawnedInstance;

                        if (getElement & spawnedInstance)
                        {
                            EquipOneElite(spawnedInstance.GetComponent<CharacterMaster>().inventory, elite);
                            elementName = elite.prefix;
                        }
                        instance.StartCoroutine(eventing.CheckIfEnemyDied(spawnedInstance, (int)enemyType.rewardBase));
                    }
                    loop = i + 1;
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
        public static SpawnCard.SpawnResult SpawnEnemy(CharacterSpawnCard spawnCard, Vector3 center, bool ally = false)
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
                //Without this code Huntress' boomerang won't bounce between mobs like Grovetender wisps which make enemies like Grovetenders actual nightmares
                teamIndexOverride = new TeamIndex?(!ally ? TeamIndex.Monster : TeamIndex.Player),
                ignoreTeamMemberLimit = true
            };

            //Create a range around the player where enemies spawn to protect from instant deaths
            Vector3 randomizedVector = new Vector3(random.Next(-25, 25), 0, random.Next(-25, 25));
            Vector3 position = center + new Vector3(
                randomizedVector.x >= 0 ? Mathf.Clamp(randomizedVector.x, 5, 25) : Mathf.Clamp(randomizedVector.x, -5, -25),
                10f,
                randomizedVector.z >= 0 ? Mathf.Clamp(randomizedVector.z, 5, 25) : Mathf.Clamp(randomizedVector.z, -5, -25));
            return spawnCard.DoSpawn(position, Quaternion.identity, spawnRequest);
        }

        //Rolls and drop tables (mostly obsolete)
        public static List<PickupIndex> RollType(int item)
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
                    rollType = corruptedList[VoidDropTable()]; //Apparently I've only been giving out WHITE CORRUPTED ITEMS THIS WHOLE TIME AHGFHHHGHGHSGHGH
                    break;

                default:
                    break;
            }
            return rollType;
        }
    }

    public class Tables
    {
        public static int GetDropTable(bool restrictVoid = false, bool restrictEquipment = false, bool restrictLunar = false)
        {
            //Whoops, actually          W > G > R > B > L > E > C
            int[] weights = new int[] {
                commonRate.Value,
                uncommonRate.Value,
                legendRate.Value,
                bossRate.Value,
                restrictLunar ? 0 : lunarRate.Value,
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
        public static int GetInstanceTable()
        {
            //In order,                 Swarm > Boss > Event
            int[] weights = new int[] { swarmRate.Value, Mathf.Clamp(100 - swarmRate.Value - eventRate.Value, 0, 100), eventRate.Value };
            int strength = 0, check = 0;
            foreach (int i in weights) strength += i;
            int roll = random.Next(0, strength);

            for (int i = 0; i < weights.Length; i++)
            {
                if (weights[i] == 0) continue;
                check += weights[i];
                if (roll < check) return i;
            }
            return 0;
        }

        //Generic drop table system
        public static int ItemDropTable(bool restrictVoid = false, bool restrictEquipment = false, bool restrictLunar = false)
        {
            //Whoops, actually          W > G > R > B > L > E > C
            int[] weights = new int[] {
                commonRate.Value,
                uncommonRate.Value,
                legendRate.Value,
                bossRate.Value,
                restrictLunar ? 0 : lunarRate.Value,
                restrictEquipment ? 0 : 15,
                expansion1 ? restrictVoid ? 0 : corruptRate.Value : 0
            };
            return CreateDropTable(weights);
        }
        public static int VoidDropTable()
        {
            //In order,                 W > G > R > B
            int[] weights = new int[] { 40, 35, 20, 5 };
            return CreateDropTable(weights);
        }
        public static int SummonDropTable()
        {
            //In order,                 Swarm > Normal > Event > Boss
            int[] weights = new int[] {
                Mathf.Clamp(swarmRate.Value - (int)directorValue / 3, 0, 100),
                Mathf.Clamp(50 - (int)directorValue / 3, 0, 100),
                Mathf.Clamp(eventRate.Value, 0, 100),
                Mathf.Clamp(5 + (int)directorValue, 0, 100) };
            return CreateDropTable(weights);
        }
        public static int EventDropTable()
        {
            //In order, Jly > ElParent > Mith > Friend > Transport > Teleporter > Gold > Portal
            //Purge > Order > Corrupted > Voidling
            List<int> weights = new List<int>() { 10, 10, 5, 20, 5, 15 }; // Basic weights
            if (purgeRate.Value > 0) weights.Add(3);
            if (enableOrder.Value) weights.Add(2);
            if (expansion1) weights.AddRange(new List<int>() { 5, 5 });

            int response = CreateDropTable(weights.ToArray());
            System.Console.WriteLine("[CHAOS] event return is {0}", response);
            return response;
        }
        public static int EliteDropTable()
        {
            //In order,                 F > I > L > G > Ma > P > Me > V  
            List<int> weights = new List<int>() { 22, 22, 21, 15, 10, 5 }; // Basic weights
            if (expansion1) weights.AddRange(new List<int>() { 25, 5 });
            return CreateDropTable(weights.ToArray());
        }
        public static int CreateDropTable(int[] weights)
        {
            double strength = 0, check = 0;
            double[] percentage = new double[weights.Length];

            foreach (int i in weights) strength += i; //Calc the strength
            for (int i = 0; i < weights.Length; i++) percentage[i] = weights[i] / strength; //Calc the weight
            double roll = random.NextDouble() * 100; //Should work correctly

            //Pick subset based on weight and order
            for (int i = 0; i < percentage.Length; i++)
            {
                check += percentage[i] * 100; //We want to use percentages

                if (percentage[i] == 0) continue;
                if (roll < check) return i;
            }
            return 0;
        }
    }
}

using BepInEx;
using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static ChaosMode.Actions;
using static ChaosMode.ChaosMode;
using static ChaosMode.Init;
using static ChaosMode.Models;

namespace ChaosMode
{
    //Mostly used for IEnumerator's called by ChaosMode.Actions.cs
    public class Events
    {
        //Chaos Event Coroutines
        public void JellyfishEventVoid() => instance.StartCoroutine(JellyfishEvent());
        public IEnumerator JellyfishEvent()
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
                    instance.StartCoroutine(CheckIfEnemyDied(spawnedInstance));
                }

                yield return new WaitForSeconds(0.5f - (i / 100));
            }
        }
        public IEnumerator EliteParentEvent()
        {
            Chat.SendBroadcastChat(new Chat.SimpleChatMessage
            {
                baseToken = "<color=#bb0011>[CHAOS] <color=#ff0000>Elite Parent event! The council will see you now!</color>"
            });

            PlayerCharacterMasterController[] players = ChaosMode.FindObjectsOfType<PlayerCharacterMasterController>();
            PlayerCharacterMasterController chosen = players[random.Next(0, players.Length)];

            //Addressable Resource loading
            CharacterSpawnCard spawnCard = null;
            spawnCard = Addressables.LoadAssetAsync<CharacterSpawnCard>(ADParent.location).WaitForCompletion();
            GameObject spawnedInstance = SpawnEnemy(spawnCard, chosen.master.GetBody().transform.position).spawnedInstance;
            EquipOneElite(spawnedInstance.GetComponent<CharacterMaster>().inventory, ADFire);
            instance.StartCoroutine(CheckIfEnemyDied(spawnedInstance));

            spawnedInstance = SpawnEnemy(spawnCard, chosen.master.GetBody().transform.position).spawnedInstance;
            EquipOneElite(spawnedInstance.GetComponent<CharacterMaster>().inventory, ADIce);
            instance.StartCoroutine(CheckIfEnemyDied(spawnedInstance));

            spawnedInstance = SpawnEnemy(spawnCard, chosen.master.GetBody().transform.position).spawnedInstance;
            EquipOneElite(spawnedInstance.GetComponent<CharacterMaster>().inventory, ADLightning);
            instance.StartCoroutine(CheckIfEnemyDied(spawnedInstance));

            spawnedInstance = SpawnEnemy(spawnCard, chosen.master.GetBody().transform.position).spawnedInstance;
            EquipOneElite(spawnedInstance.GetComponent<CharacterMaster>().inventory, ADGhost);
            instance.StartCoroutine(CheckIfEnemyDied(spawnedInstance));

            spawnedInstance = SpawnEnemy(spawnCard, chosen.master.GetBody().transform.position).spawnedInstance;
            EquipOneElite(spawnedInstance.GetComponent<CharacterMaster>().inventory, ADPoison);
            instance.StartCoroutine(CheckIfEnemyDied(spawnedInstance));

            spawnedInstance = SpawnEnemy(spawnCard, chosen.master.GetBody().transform.position).spawnedInstance;
            EquipOneElite(spawnedInstance.GetComponent<CharacterMaster>().inventory, ADEcho);
            instance.StartCoroutine(CheckIfEnemyDied(spawnedInstance));

            spawnedInstance = SpawnEnemy(spawnCard, chosen.master.GetBody().transform.position).spawnedInstance;
            EquipOneElite(spawnedInstance.GetComponent<CharacterMaster>().inventory, ADEarth);
            instance.StartCoroutine(CheckIfEnemyDied(spawnedInstance));

            yield return null;
        }
        public IEnumerator FinalEncounter()
        {
            if (expansion1)
            {
                Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                {
                    baseToken = "<color=#bb0011>[CHAOS] <color=#ff0000>Mutated event! The King of Nothing loses control!</color>"
                });
            }
            else
            {
                Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                {
                    baseToken = "<color=#bb0011>[CHAOS] <color=#ff0000>Empty event! The King of Nothing invades!</color>"
                });
            }

            foreach (PlayerCharacterMasterController player in PlayerCharacterMasterController.instances)
                instance.StartCoroutine(Purge(player));

            PlayerCharacterMasterController[] players = FindObjectsOfType<PlayerCharacterMasterController>();
            PlayerCharacterMasterController chosen = players[random.Next(0, players.Length)];

            //Addressable Resource loading
            CharacterSpawnCard spawnCard = null;
            spawnCard = Addressables.LoadAssetAsync<CharacterSpawnCard>(ADBrother.location).WaitForCompletion();
            GameObject spawnedInstance = SpawnEnemy(spawnCard, chosen.master.GetBody().transform.position).spawnedInstance;
            if (expansion1) EquipOneElite(spawnedInstance.GetComponent<CharacterMaster>().inventory, ADVoid);
            instance.StartCoroutine(CheckIfEnemyDied(spawnedInstance));

            yield return null;
        }
        public IEnumerator VoidEncounter()
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
            instance.StartCoroutine(CheckIfEnemyDied(spawnedInstance));

            yield return null;
        }
        public IEnumerator PurgeAllItems()
        {
            Chat.SendBroadcastChat(new Chat.SimpleChatMessage
            {
                baseToken = "<color=#bb0011>[Chaos] <color=#ff0000>Purge event! You don't need these, right?</color>"
            });

            foreach (PlayerCharacterMasterController player in PlayerCharacterMasterController.instances)
                instance.StartCoroutine(Purge(player));

            yield return null;
        }
        public IEnumerator Purge(PlayerCharacterMasterController player)
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
        public IEnumerator Corruption()
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
        public IEnumerator GainFriend()
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
        public IEnumerator SequenceEvent()
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
                if (expansion1)
                {
                    tiers[5] = master.inventory.GetTotalItemCountOfTier(ItemTier.VoidTier1);
                    tiers[5] += master.inventory.GetTotalItemCountOfTier(ItemTier.VoidTier2);
                    tiers[5] += master.inventory.GetTotalItemCountOfTier(ItemTier.VoidTier3);
                    tiers[5] += master.inventory.GetTotalItemCountOfTier(ItemTier.VoidBoss);
                }

                System.Console.WriteLine("{0}, {1}, {2}, {3}, {4}, {5}", tiers[0], tiers[1], tiers[2], tiers[3], tiers[4], tiers[5]);

                //Reset all your items
                List<ItemIndex> inventory = master.inventory.itemAcquisitionOrder;
                while (master.inventory.itemAcquisitionOrder.Count > 0)
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

        public IEnumerator CheckIfEnemyDied(GameObject enemy, int reward = 20)
        {
            //I think I fixed this problem a long time ago but either way this is a scuffed way to get money from enemies

            while (enemy != null)
                yield return new WaitForSeconds(0.1f);

            foreach (PlayerCharacterMasterController player in PlayerCharacterMasterController.instances)
                player.GetComponent<CharacterMaster>().GiveMoney((uint)Run.instance.GetDifficultyScaledCost(reward)); //Add a reward value for each enemy?
        }
    }
}

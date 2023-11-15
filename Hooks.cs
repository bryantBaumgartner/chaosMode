using R2API.Utils;
using RoR2;
using System;
using System.Collections.Generic;
using System.Reflection;
using static ChaosMode.Actions;
using static ChaosMode.ChaosMode;
using static ChaosMode.Tables;

namespace ChaosMode
{
    class Hooks
    {
        //Randomize chest drops using our drop table method
        public static void ChestBehavior_ItemDrop(On.RoR2.ChestBehavior.orig_ItemDrop orig, ChestBehavior self)
        {
            if (!CheckChestTypeIsValid(self)) { orig.Invoke(self); return; }

            PropertyInfo dropPickupField = typeof(ChestBehavior).GetProperty("dropPickup", BindingFlags.Instance | BindingFlags.Public);
            if ((PickupIndex)dropPickupField.GetValue(self) == PickupIndex.none) { return; }

            List<PickupIndex> newRoll = RollType(ItemDropTable());
            PickupIndex item = newRoll[random.Next(0, newRoll.Count)];
            dropPickupField.SetValue(self, item);

            orig.Invoke(self);
            return;
        }
        public static void ShrineChanceBehavior_AddShrineStack(On.RoR2.ShrineChanceBehavior.orig_AddShrineStack orig, ShrineChanceBehavior self, Interactor activator)
        {
            //Increase the shrine purchase count
            int purchaseCount = self.GetFieldValue<int>("successfulPurchaseCount") + 1;
            self.SetFieldValue<int>("successfulPurchaseCount", purchaseCount);

            //Create a pickupindex
            List<PickupIndex> newRoll = RollType(GetDropTable(restrictEquipment: true));
            PickupIndex item = newRoll[random.Next(0, newRoll.Count)];
            PickupDropletController.CreatePickupDroplet(item, self.dropletOrigin.position, self.dropletOrigin.forward * 20f);

            //Send message and add actions
            Chat.SendBroadcastChat(new Chat.SubjectFormatChatMessage
            {
                subjectAsCharacterBody = activator.GetComponent<CharacterBody>(),
                baseToken = "SHRINE_CHANCE_SUCCESS_MESSAGE"
            });
            Action<bool, Interactor> action = self.GetFieldValue<Action<bool, Interactor>>("onShrineChancePurchaseGlobal");
            if (action != null)
            {
                action(true, activator);
            }
            self.SetFieldValue<bool>("waitingForRefresh", true);
            self.SetFieldValue<float>("refreshTimer", 2f);
            /*EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ShrineUseEffect"), new EffectData
            {
                origin = self.transform.localPosition,
                rotation = Quaternion.identity,
                scale = 1f,
                color = self.GetFieldValue<Color>("shrineColor")
            }, true); */
            if (self.GetFieldValue<int>("successfulPurchaseCount") >= self.maxPurchaseCount)
            {
                self.symbolTransform.gameObject.SetActive(false);
            }

            //orig.Invoke(self, activator);
            return;
        }
        public static bool CheckChestTypeIsValid(ChestBehavior self)
        {
            String chestType = self.gameObject.name.Replace("(Clone)", "").Trim();
            System.Console.WriteLine("[CHAOS] ChestBehavior gameobject name: {0}", chestType);
            return (chestType != "EquipmentBarrel") && (chestType != "LunarChest") && (chestType != "VoidChest") && (chestType != "GoldChest");
        }
        //Initialize and start the Chaos Loop
        public static void Run_Start(On.RoR2.Run.orig_Start orig, Run self)
        {
            //Set initial run variables
            ChaosMode.Reset();

            orig.Invoke(self);
            return;
        }
    }
}

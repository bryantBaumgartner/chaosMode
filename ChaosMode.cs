using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.ExpansionManagement;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using static ChaosMode.Actions;
using static ChaosMode.Init;

namespace ChaosMode
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.Pocket.ChaosMode", "ChaosMode", "3.0.0")]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.EveryoneNeedSameModVersion)]
    internal class ChaosMode : BaseUnityPlugin
    {
        public static ChaosMode instance;
        public static System.Random random = new System.Random();

        private static int oldTimer;
        public static float directorValue;
        private static bool initialized, spawning, expansion1, expansion2;

        public void Awake()
        {
            Initialize(Config);
            instance = this;
        }

        public static void Reset()
        {
            directorValue = oldTimer = 0;
            spawning = initialized = false;

            instance.StartCoroutine(instance.GameLoop());
        }

        private IEnumerator GameLoop()
        {
            yield return null;
            if (!Run.instance) yield break;
            string scene = SceneManager.GetActiveScene().name;

            //First step setup
            if (!initialized)
            {
                //Skip step if not in game yet
                if (scene == "lobby") { StartCoroutine(GameLoop()); }

                initialized = true;
                instance.StartCoroutine(instance.RunInit());
            }

            //Perform action every Timer step
            float t = Run.instance.GetRunStopwatch();
            bool onTimer = Mathf.FloorToInt(t) % (Mathf.Clamp(chaosSpeed.Value, 5, 600)) == 0;
            bool timeParam = t > 5 && t != oldTimer;
            if (onTimer && timeParam && scene != "bazaar")
            {
                oldTimer = (int)t;
                if (!spawning)
                {
                    spawning = true;
                    SpawnEveryMinute();
                    instance.StartCoroutine(instance.FailSafeDelay());
                }
            }
            else
                spawning = false;

            StartCoroutine(GameLoop());
        }
        private IEnumerator RunInit()
        {
            //Is DLC1 enabled?
            expansion1 = Run.instance.IsExpansionEnabled(ExpansionCatalog.expansionDefs[0]);
            expansion2 = Run.instance.IsExpansionEnabled(ExpansionCatalog.expansionDefs[1]);
            System.Console.WriteLine("[CHAOS] Expansion1 loaded: {0}", expansion1);
            System.Console.WriteLine("[CHAOS] Expansion2 loaded: {0}", expansion2);

            //Use the current seed of the game for consistency
            random = new System.Random((int)Run.instance.seed);

            if (startingItems.Value)
                StartingItems();

            //Broadcast confirmation messsage
            yield return new WaitForSeconds(20);
            Chat.SendBroadcastChat(new Chat.SimpleChatMessage { baseToken = "<color=#bb0011>[CHAOS] The Avatar of Chaos invades!" });
        }
        private IEnumerator FailSafeDelay()
        {
            yield return new WaitForSeconds(1f);
            spawning = false;
        }

        //Needed so we ChaosMode.Events.cs doesn't need to inherit from BaseUnityPlugin and can instead be instanced
        new public static T FindObjectOfType<T>() => FindObjectOfType<T>();
        new public static T[] FindObjectsOfType<T>() => FindObjectsOfType<T>();
    }
}

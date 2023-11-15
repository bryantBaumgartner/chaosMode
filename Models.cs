namespace ChaosMode
{
    static class Models
    {
        //Addressable Resource Loading
        //New Elite Equipment Locations
        public static EliteEquipment ADFire = new EliteEquipment() { prefix = "Blazing", addressable = "RoR2/Base/EliteFire/EliteFireEquipment.asset" };
        public static EliteEquipment ADIce = new EliteEquipment() { prefix = "Glacial", addressable = "RoR2/Base/EliteIce/EliteIceEquipment.asset" };
        public static EliteEquipment ADLightning = new EliteEquipment() { prefix = "Overloading", addressable = "RoR2/Base/EliteLightning/EliteLightningEquipment.asset" };
        public static EliteEquipment ADGhost = new EliteEquipment() { prefix = "Celestine", addressable = "RoR2/Base/EliteHaunted/EliteHauntedEquipment.asset" };
        public static EliteEquipment ADPoison = new EliteEquipment() { prefix = "Malachite", addressable = "RoR2/Base/ElitePoison/ElitePoisonEquipment.asset" };
        public static EliteEquipment ADEcho = new EliteEquipment() { prefix = "Perfected", addressable = "RoR2/Base/EliteLunar/EliteLunarEquipment.asset" };
        public static EliteEquipment ADEarth = new EliteEquipment() { prefix = "Mending", addressable = "RoR2/DLC1/EliteEarth/EliteEarthEquipment.asset" };
        public static EliteEquipment ADVoid = new EliteEquipment() { prefix = "Voidtouched", addressable = "RoR2/DLC1/EliteVoid/EliteVoidEquipment.asset" };
        public static EliteEquipment ADSpeed = new EliteEquipment() { prefix = "Speedy?", addressable = "RoR2/DLC1/EliteSecretSpeedEquipment.asset" };
        public static EliteEquipment ADGold = new EliteEquipment() { prefix = "Golden?", addressable = "RoR2/Junk/EliteGold/EliteGoldEquipment.asset" };
        public static EliteEquipment ADYellow = new EliteEquipment() { prefix = "Yellow?", addressable = "RoR2/Junk/EliteYellow/EliteYellowEquipment.asset" };

        //Weaker Enemies
        //I will never let the "Archaic Wisp" die. He will always be one of my favorite enemy types.
        public static SpawnCardData ADBeetle = new SpawnCardData() { name = "Beetle", location = "RoR2/Base/Beetle/cscBeetle.asset", difficultyBase = 0.1f, rewardBase = 5f };
        public static SpawnCardData ADBeetleGuard = new SpawnCardData() { name = "Beetle Guard", location = "RoR2/Base/Beetle/cscBeetleGuard.asset", difficultyBase = 0.4f, rewardBase = 12f };
        public static SpawnCardData ADBeetleQueen = new SpawnCardData() { name = "Beetle Queen", location = "RoR2/Base/Beetle/cscBeetleQueen.asset", difficultyBase = 1f, rewardBase = 23f };
        public static SpawnCardData ADLemurian = new SpawnCardData() { name = "Lemurian", location = "RoR2/Base/Lemurian/cscLemurian.asset", difficultyBase = 0.2f, rewardBase = 23f };
        public static SpawnCardData ADBigLemurian = new SpawnCardData() { name = "Elder Lemurian", location = "RoR2/Base/LemurianBruiser/cscLemurianBruiser.asset", difficultyBase = 0.9f, rewardBase = 23f };
        public static SpawnCardData ADBell = new SpawnCardData() { name = "Brass Contraption", location = "RoR2/Base/Bell/cscBell.asset", difficultyBase = 0.8f, rewardBase = 16f };
        public static SpawnCardData ADBison = new SpawnCardData() { name = "Bison", location = "RoR2/Base/Bison/cscBison.asset", difficultyBase = 0.3f, rewardBase = 9f };
        public static SpawnCardData ADTemplar = new SpawnCardData() { name = "Clay Templar", location = "RoR2/Base/ClayBruiser/cscClayBruiser.asset", difficultyBase = 0.9f, rewardBase = 21f };
        public static SpawnCardData ADApothecary = new SpawnCardData() { name = "Clay Apothecary", location = "RoR2/DLC1/ClayGrenadier/cscClayGrenadier.asset", difficultyBase = 0.8f, rewardBase = 18f };
        public static SpawnCardData ADGolem = new SpawnCardData() { name = "Stone Golem", location = "RoR2/Base/Golem/cscGolem.asset", difficultyBase = 0.4f, rewardBase = 10f };
        public static SpawnCardData ADWisp = new SpawnCardData() { name = "Lesser Wisp", location = "RoR2/Base/Wisp/cscLesserWisp.asset", difficultyBase = 0.1f, rewardBase = 4f };
        public static SpawnCardData ADGreaterWisp = new SpawnCardData() { name = "Greater Wisp", location = "RoR2/Base/GreaterWisp/cscGreaterWisp.asset", difficultyBase = 0.7f, rewardBase = 14f };
        public static SpawnCardData ADJellyfish = new SpawnCardData() { name = "Jellyfish", location = "RoR2/Base/Jellyfish/cscJellyfish.asset", difficultyBase = 0.2f, rewardBase = 7f };
        public static SpawnCardData ADMushroom = new SpawnCardData() { name = "Mini Mushroom", location = "RoR2/Base/MiniMushroom/cscMiniMushroom.asset", difficultyBase = 0.9f, rewardBase = 19f };
        public static SpawnCardData ADVulture = new SpawnCardData() { name = "Alloy Vulture", location = "RoR2/Base/Vulture/cscVulture.asset", difficultyBase = 0.7f, rewardBase = 14f };
        public static SpawnCardData ADImp = new SpawnCardData() { name = "Imp", location = "RoR2/Base/Imp/cscImp.asset", difficultyBase = 0.6f, rewardBase = 16f };
        public static SpawnCardData ADParent = new SpawnCardData() { name = "Parent", location = "RoR2/Base/Parent/cscParent.asset", difficultyBase = 0.9f, rewardBase = 23f };
        public static SpawnCardData ADLunarGolem = new SpawnCardData() { name = "Lunar Chimera", location = "RoR2/Base/LunarGolem/cscLunarGolem.asset", difficultyBase = 1.1f, rewardBase = 25f };
        public static SpawnCardData ADLunarWisp = new SpawnCardData() { name = "Lunar Chimera", location = "RoR2/Base/LunarWisp/cscLunarWisp.asset", difficultyBase = 1.3f, rewardBase = 27f };
        public static SpawnCardData ADLunarBomb = new SpawnCardData() { name = "Lunar Chimera", location = "RoR2/Base/LunarExploder/cscLunarExploder.asset", difficultyBase = 0.8f, rewardBase = 19f };
        public static SpawnCardData ADNullifier = new SpawnCardData() { name = "Void Reaver", location = "RoR2/Base/Nullifier/cscNullifier.asset", difficultyBase = 1.5f, rewardBase = 32f };
        public static SpawnCardData ADArchWisp = new SpawnCardData() { name = "Archaic Wisp", location = "RoR2/Junk/ArchWisp/cscArchWisp.asset", difficultyBase = 0.8f, rewardBase = 23f };
        public static SpawnCardData ADHermitCrab = new SpawnCardData() { name = "Hermit Crab", location = "RoR2/Base/HermitCrab/cscHermitCrab.asset", difficultyBase = 0.5f, rewardBase = 8f };

        //Boss Tier Enemies
        public static SpawnCardData ADTitan = new SpawnCardData() { name = "Stone Titan", location = "RoR2/Base/Titan/cscTitanBlackBeach.asset", difficultyBase = 1.2f, rewardBase = 24f };
        public static SpawnCardData ADVagrant = new SpawnCardData() { name = "Wandering Vagrant", location = "RoR2/Base/Vagrant/cscVagrant.asset", difficultyBase = 0.7f, rewardBase = 17f };
        public static SpawnCardData ADOverlord = new SpawnCardData() { name = "Imp Overlord", location = "RoR2/Base/ImpBoss/cscImpBoss.asset", difficultyBase = 1.3f, rewardBase = 19f };
        public static SpawnCardData ADTitanGold = new SpawnCardData() { name = "Aurelionite", location = "RoR2/Base/Titan/cscTitanGold.asset", difficultyBase = 1.4f, rewardBase = 30f };
        public static SpawnCardData ADDunestrider = new SpawnCardData() { name = "Clay Dunestrider", location = "RoR2/Base/ClayBoss/cscClayBoss.asset", difficultyBase = 1.0f, rewardBase = 22f };
        public static SpawnCardData ADGrandparent = new SpawnCardData() { name = "Grandparent", location = "RoR2/Base/Grandparent/cscGrandparent.asset", difficultyBase = 1.6f, rewardBase = 34f };
        public static SpawnCardData ADGhibli = new SpawnCardData() { name = "Grovetender", location = "RoR2/Base/Gravekeeper/cscGravekeeper.asset", difficultyBase = 1.3f, rewardBase = 31f };
        public static SpawnCardData ADMagmaWorm = new SpawnCardData() { name = "Magma Worm", location = "RoR2/Base/MagmaWorm/cscMagmaWorm.asset", difficultyBase = 1.5f, rewardBase = 32f };
        public static SpawnCardData ADOverWorm = new SpawnCardData() { name = "Overloading Worm", location = "RoR2/Base/ElectricWorm/cscElectricWorm.asset", difficultyBase = 1.8f, rewardBase = 36f };
        public static SpawnCardData ADRoboBall = new SpawnCardData() { name = "Solus Control Unit", location = "RoR2/Base/RoboBallBoss/cscRoboBallBoss.asset", difficultyBase = 1.4f, rewardBase = 23f };
        public static SpawnCardData ADScav = new SpawnCardData() { name = "Scavenger", location = "RoR2/Base/Scav/cscScav.asset", difficultyBase = 1.6f, rewardBase = 37f };

        //DLC - Survivors of the Void
        public static SpawnCardData ADLarva = new SpawnCardData() { name = "Acid Larva", location = "RoR2/DLC1/AcidLarva/cscAcidLarva.asset", difficultyBase = 0.6f, rewardBase = 10f };
        public static SpawnCardData ADAssassin = new SpawnCardData() { name = "Assassin", location = "RoR2/DLC1/Assassin2/cscAssassin2.asset", difficultyBase = 0.5f, rewardBase = 14f };
        public static SpawnCardData ADPest = new SpawnCardData() { name = "Blind Pest", location = "RoR2/DLC1/FlyingVermin/cscFlyingVermin.asset", difficultyBase = 0.7f, rewardBase = 16f };
        public static SpawnCardData ADVermin = new SpawnCardData() { name = "Blind Vermin", location = "RoR2/DLC1/Vermin/cscVermin.asset", difficultyBase = 0.6f, rewardBase = 12f };
        public static SpawnCardData ADBarnacle = new SpawnCardData() { name = "Void Barnacle", location = "RoR2/DLC1/VoidBarnacle/cscVoidBarnacle.asset", difficultyBase = 1.0f, rewardBase = 20f };
        public static SpawnCardData ADJailer = new SpawnCardData() { name = "Void Jailer", location = "RoR2/DLC1/VoidJailer/cscVoidJailer.asset", difficultyBase = 1.8f, rewardBase = 38f };
        public static SpawnCardData ADMegaCrab = new SpawnCardData() { name = "Void Devastator", location = "RoR2/DLC1/VoidMegaCrab/cscVoidMegaCrab.asset", difficultyBase = 2.0f, rewardBase = 43f };
        public static SpawnCardData ADGup = new SpawnCardData() { name = "Gup", location = "RoR2/DLC1/Gup/cscGupBody.asset", difficultyBase = 1.0f, rewardBase = 20f };
        public static SpawnCardData ADInfestor = new SpawnCardData() { name = "Void Infestor", location = "RoR2/DLC1/EliteVoid/cscVoidInfestor.asset", difficultyBase = 0.6f, rewardBase = 13f };
        public static SpawnCardData ADMajor = new SpawnCardData() { name = "??? Construct", location = "RoR2/DLC1/MajorAndMinorConstruct/cscMajorConstruct.asset", difficultyBase = 1.0f, rewardBase = 20f };
        public static SpawnCardData ADMinor = new SpawnCardData() { name = "Alpha Construct", location = "RoR2/DLC1/MajorAndMinorConstruct/cscMinorConstruct.asset", difficultyBase = 0.5f, rewardBase = 11f };
        public static SpawnCardData ADMega = new SpawnCardData() { name = "Xi Construct", location = "RoR2/DLC1/MajorAndMinorConstruct/cscMegaConstruct.asset", difficultyBase = 1.0f, rewardBase = 20f };

        //Primary Bosses
        public static SpawnCardData ADBrother = new SpawnCardData() { name = "Mithrix", location = "RoR2/Base/Brother/cscBrother.asset", difficultyBase = 2.0f, rewardBase = 40f };
        public static SpawnCardData ADVoidling = new SpawnCardData() { name = "Voidling", location = "RoR2/DLC1/VoidRaidCrab/cscMiniVoidRaidCrabBase.asset", difficultyBase = 2.0f, rewardBase = 45f };

    }

    class SpawnCardData
    {
        public string name { get; set; }
        public string location { get; set; }
        public float difficultyBase { get; set; }
        public float rewardBase { get; set; }
        public int spawnCost { get; set; }

        public SpawnCardData()
        {
            name = "Test Enemy";
            location = "RoR2/Base/Beetle/cscBeetle.asset";
            difficultyBase = 0.1f;
            rewardBase = 5f;
            spawnCost = 1;
        }
    }
    class EliteEquipment
    {
        public string prefix { get; set; }
        public string addressable { get; set; }
    }
}
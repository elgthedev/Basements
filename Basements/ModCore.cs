using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using PieceManager;
using ServerSync;
using UnityEngine;

namespace Basements
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class BasementsMod : BaseUnityPlugin
    {
        private const string ModName = "Basement";
        internal const string ModVersion = "1.1.0";
        private const string ModGUID = "com.rolopogo.Basement";
        private static Harmony harmony = null!;
        ConfigSync configSync = new(ModGUID) 
            { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion};
        internal static ConfigEntry<bool> ServerConfigLocked = null!;
        ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description, bool synchronizedSetting = true)
        {
            ConfigEntry<T> configEntry = Config.Bind(group, name, value, description);

            SyncedConfigEntry<T> syncedConfigEntry = configSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }
        ConfigEntry<T> config<T>(string group, string name, T value, string description, bool synchronizedSetting = true) => config(group, name, value, new ConfigDescription(description), synchronizedSetting);
       
        internal static ConfigEntry<int> MaxNestedLimit = null!;
        private static GameObject basementPrefab { get; set; }
        
        public void Awake()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            harmony = new(ModGUID);
            harmony.PatchAll(assembly);
            ServerConfigLocked = config("1 - General", "Lock Configuration", true, "If on, the configuration is locked and can be changed by server admins only.");
            configSync.AddLockingConfigEntry(ServerConfigLocked);
            
            MaxNestedLimit = config("General", "Max nested basements", 1,
                "The maximum number of basements you can incept into each other");

            BuildPiece buildPiece = new BuildPiece("basement", "Basement");
            buildPiece.Name.English("Basement");
            buildPiece.Name.Russian("Подвал");
            buildPiece.Description.Russian("Хороший и прохладный подвал для ваших вещей");
            buildPiece.Description.English("A nice cool underground storage room for your things");
            buildPiece.RequiredItems.Add("Stone", 200, recover: false);
            buildPiece.RequiredItems.Add("Wood", 100, recover: false);
            buildPiece.Category.Add(BuildPieceCategory.Misc);
            buildPiece.Crafting.Set(CraftingTable.StoneCutter);
            basementPrefab = buildPiece.Prefab.gameObject;
            MaterialReplacer.RegisterGameObjectForMatSwap(basementPrefab);
            //buildPiece.SpecialProperties = new SpecialProperties() { AdminOnly = true, NoConfig = true}; // You can declare multiple properties in one line           
        }
    }
}

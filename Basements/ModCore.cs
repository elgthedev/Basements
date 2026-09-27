using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using JetBrains.Annotations;
using PieceManager;
using ServerSync;
using UnityEngine;

namespace Basements
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class BasementsMod : BaseUnityPlugin
    {
        internal const string ModName = "Basements";
        internal const string ModVersion = "2.0.0";
        private const string ModGUID = "com.rolopogo.Basement"; // GUID kept
        internal static ManualLogSource _basementLogger = new ManualLogSource(ModName);
        private static readonly string _configFileName = ModGUID + ".cfg";
        private static readonly string _configFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + _configFileName;
        private readonly Harmony _harmony = new(ModGUID);
        private FileSystemWatcher? _configWatcher;

        internal static ConfigEntry<bool> ServerConfigLocked = null!;       
        internal static ConfigEntry<int> MaxNestedLimit = null!;
        [SerializeField] private static GameObject? _basementPrefab;
        private static bool _legacyMaterialsReplaced;
        private static Material? _basementStoneFloorMaterial;

        internal static GameObject? BasementPrefab
        {
            get => _basementPrefab;
            set => _basementPrefab = value;
        }

        /// <summary>
        /// The bundled prefab was authored before Valheim 1.0's Unity upgrade. Its materials
        /// have the same names as game materials but retain legacy shader serialization, which
        /// PieceManager's _REPLACE_ naming convention cannot detect.
        /// </summary>
        internal static void ReplaceLegacyPrefabMaterials()
        {
            if (_legacyMaterialsReplaced || BasementPrefab == null) return;

            var prefabMaterials = new HashSet<Material>();
            foreach (var renderer in BasementPrefab.GetComponentsInChildren<Renderer>(true))
            {
                foreach (var material in renderer.sharedMaterials)
                {
                    if (material != null) prefabMaterials.Add(material);
                }
            }

            var gameMaterials = new Dictionary<string, Material>(StringComparer.Ordinal);
            foreach (var material in Resources.FindObjectsOfTypeAll<Material>())
            {
                if (material == null || prefabMaterials.Contains(material) || material.shader == null) continue;
                if (string.Equals(material.shader.name, "Standard", StringComparison.Ordinal)) continue;
                gameMaterials[material.name.Replace(" (Instance)", "")] = material;
            }

            var replacements = new Dictionary<Material, Material>();
            foreach (var material in prefabMaterials)
            {
                var name = material.name.Replace(" (Instance)", "");
                var isLegacyStandardMaterial = material.shader != null && string.Equals(material.shader.name, "Standard", StringComparison.Ordinal);
                var isLegacyHeightmapMaterial = string.Equals(name, "Heightmap_basematerial", StringComparison.Ordinal);
                if (isLegacyStandardMaterial && gameMaterials.TryGetValue(name, out var replacement))
                {
                    replacements[material] = replacement;
                }
                else if (isLegacyHeightmapMaterial)
                {
                    // The old bundle applies the terrain Heightmap shader to static meshes.
                    // In Valheim 1.0 that shader requires terrain-specific data the meshes do
                    // not own, so use the live stone floor material instead.
                    if (TryGetBasementStoneFloorMaterial(gameMaterials, out var stoneFloorMaterial))
                    {
                        replacements[material] = stoneFloorMaterial;
                    }
                }
            }

            foreach (var renderer in BasementPrefab.GetComponentsInChildren<Renderer>(true))
            {
                var materials = renderer.sharedMaterials;
                var changed = false;
                for (var index = 0; index < materials.Length; index++)
                {
                    if (materials[index] != null && replacements.TryGetValue(materials[index], out var replacement))
                    {
                        materials[index] = replacement;
                        changed = true;
                    }
                }

                if (changed) renderer.sharedMaterials = materials;
            }

            _legacyMaterialsReplaced = true;
            _basementLogger.LogInfo($"Replaced {replacements.Count} legacy basement material(s) with Valheim 1.0 materials.");
        }

        private static bool TryGetBasementStoneFloorMaterial(IReadOnlyDictionary<string, Material> gameMaterials, out Material material)
        {
            if (_basementStoneFloorMaterial != null)
            {
                material = _basementStoneFloorMaterial;
                return true;
            }

            if (!gameMaterials.TryGetValue("stonefloor", out var sourceMaterial))
            {
                material = null!;
                return false;
            }

            _basementStoneFloorMaterial = new Material(sourceMaterial)
            {
                name = "Basements_StoneFloor"
            };
            if (_basementStoneFloorMaterial.HasProperty("_Cull")) _basementStoneFloorMaterial.SetFloat("_Cull", 0f);

            material = _basementStoneFloorMaterial;
            return true;
        }

        public void Awake()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
            ServerConfigLocked = config("1 - General", "Lock Configuration", true, "If on, the configuration is locked and can be changed by server admins only.");
            ConfigSync.AddLockingConfigEntry(ServerConfigLocked);
            
            MaxNestedLimit = config("1 - General", "Max nested basements", 5,
                "The maximum number of basements you can incept into each other");

            BuildPiece buildPiece = new BuildPiece("basement", "Basement");
            buildPiece.Name.English("Basement");
            buildPiece.Name.Russian("Подвал");
            buildPiece.Name.Portuguese_Brazilian("Porão");
            buildPiece.Description.Russian("Хороший и прохладный подвал для ваших вещей");
            buildPiece.Description.English("A nice cool underground storage room for your things");
            buildPiece.Description.Portuguese_Brazilian("Um compacto e seguro depósito subterrâneo");
            buildPiece.RequiredItems.Add("Stone", 200, recover: true);
            buildPiece.RequiredItems.Add("Wood", 100, recover: true);
            buildPiece.Category.Set(BuildPieceCategory.Misc);
            buildPiece.Crafting.Set(CraftingTable.StoneCutter);

            _basementLogger = Logger;

            BasementPrefab = buildPiece.Prefab.gameObject;
            MaterialReplacer.RegisterGameObjectForMatSwap(BasementPrefab);

            SetupWatcher();
        }

        private void OnDestroy()
        {
            _configWatcher?.Dispose();
            _configWatcher = null;
            _harmony.UnpatchSelf();
            Config.Save();
        }

        private void SetupWatcher()
        {
            _configWatcher = new FileSystemWatcher(Paths.ConfigPath, _configFileName)
            {
                IncludeSubdirectories = false,
                SynchronizingObject = ThreadingHelper.SynchronizingObject,
                EnableRaisingEvents = true
            };
            _configWatcher.Changed += ReadConfigValues;
            _configWatcher.Created += ReadConfigValues;
            _configWatcher.Renamed += ReadConfigValues;
        }

        private void ReadConfigValues(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(_configFileFullPath)) return;
            try
            {
                _basementLogger.LogDebug("ReadConfigValues called");
                Config.Reload();
            }
            catch
            {
                _basementLogger.LogError($"There was an issue loading your {_configFileName}");
                _basementLogger.LogError("Please check your config entries for spelling and format!");
            }
        }

        internal static void WriteLog(string text, WarnLevel level)
        {
            switch (level)
            {
                case WarnLevel.All:
                    System.Console.BackgroundColor = ConsoleColor.DarkGray;
                    _basementLogger.LogMessage(text);
                    System.Console.ResetColor();
                    break;
                case WarnLevel.Error:
                    System.Console.BackgroundColor = ConsoleColor.DarkRed;
                    _basementLogger.LogMessage(text);
                    System.Console.ResetColor();
                    break;
                case WarnLevel.Warn:
                    System.Console.BackgroundColor = ConsoleColor.Yellow;
                    _basementLogger.LogMessage(text);
                    System.Console.ResetColor();
                    break;
                case WarnLevel.Info:
                    System.Console.BackgroundColor = ConsoleColor.Black;
                    _basementLogger.LogMessage(text);
                    System.Console.ResetColor();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
           
        }

        private ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description,
            bool synchronizedSetting = true)
        {
            ConfigDescription extendedDescription =
                new(
                    description.Description + (synchronizedSetting
                        ? " [Synced with Server]"
                        : " [Not Synced with Server]"), description.AcceptableValues,
                    description.Tags);
            var configEntry = Config.Bind(group, name, value, extendedDescription);

            var syncedConfigEntry = ConfigSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }

        private ConfigEntry<T> config<T>(string group, string name, T value, string description,
            bool synchronizedSetting = true)
        {
            return config(group, name, value, new ConfigDescription(description), synchronizedSetting);
        }

        private class ConfigurationManagerAttributes
        {
            [UsedImplicitly] public int? Order;
            [UsedImplicitly] public bool? Browsable;
            [UsedImplicitly] public string? Category;
            [UsedImplicitly] public Action<ConfigEntryBase>? CustomDrawer;
        }

        private static readonly ConfigSync ConfigSync = new(ModGUID)
        { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };
    }

    enum WarnLevel
    {
        All,
        Error,
        Warn,
        Info
    }
}

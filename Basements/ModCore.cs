using System;
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
        internal const string ModVersion = "1.3.0";
        private const string ModGUID = "com.rolopogo.Basement"; // GUID kept
        internal static ManualLogSource _basementLogger = new ManualLogSource(ModName);
        private static string _configFileName = ModGUID + ".cfg";
        private static string _configFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + _configFileName;
        private readonly Harmony _harmony = new(ModGUID);

        internal static ConfigEntry<bool> ServerConfigLocked = null!;       
        internal static ConfigEntry<int> MaxNestedLimit = null!;
        [SerializeField] private static GameObject _basementPrefab;

        internal static GameObject BasementPrefab
        {
            get => _basementPrefab;
            set => _basementPrefab = value;
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
            Config.Save();
        }

        private void SetupWatcher()
        {
            FileSystemWatcher watcher = new(Paths.ConfigPath, _configFileName);
            watcher.Changed += ReadConfigValues;
            watcher.Created += ReadConfigValues;
            watcher.Renamed += ReadConfigValues;
            watcher.IncludeSubdirectories = true;
            watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            watcher.EnableRaisingEvents = true;
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
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
//EnvMan.instance.CheckInteriorBuildingOverride())
namespace Basements.Patches
{
    [HarmonyPatch(typeof(ZNetScene), "Awake")]
    static class ZNetScene_Patches
    {
        static void Postfix()
        {
            BasementsMod.ReplaceLegacyPrefabMaterials();
        }
    }

    [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.GenerateLocationsIfNeeded))]
    static class ZoneSystem_Patches
    {

        static void Postfix(ZoneSystem __instance)
        {
                BasementsMod.ReplaceLegacyPrefabMaterials();

                if (__instance.m_locationsGenerated && EnvMan.instance != null && BasementsMod.BasementPrefab != null &&
                    !EnvMan.instance.m_environments.Exists(x => x.m_name == "Basement"))
                {
                    EnvSetup? cryptEnvironment = EnvMan.instance.m_environments.Find(x => x.m_name == "Crypt");
                    if (cryptEnvironment == null) return;
                    EnvSetup basementEnv = cryptEnvironment.Clone();
                    basementEnv.m_name = "Basement";
                    basementEnv.m_ambientVol = 0;
                    basementEnv.m_windMax = 0;
                    basementEnv.m_windMin = 0;
                    basementEnv.m_rainCloudAlpha = 0;
                    basementEnv.m_fogDensityDay = 0;
                    basementEnv.m_fogDensityEvening = 0;
                    basementEnv.m_fogDensityMorning = 0;
                    basementEnv.m_fogDensityNight = 0;
                    basementEnv.m_fogColorDay = Color.clear;
                    basementEnv.m_fogColorEvening = Color.clear;
                    basementEnv.m_fogColorMorning = Color.clear;
                    basementEnv.m_fogColorNight = Color.clear;
                    basementEnv.m_fogColorSunDay = Color.clear;
                    basementEnv.m_fogColorSunEvening = Color.clear;
                    basementEnv.m_fogColorSunMorning = Color.clear;
                    basementEnv.m_fogColorSunNight = Color.clear;
                    basementEnv.m_ambientList = "Basement";
                    basementEnv.m_psystems = Array.Empty<GameObject>();
                    EnvMan.instance.m_environments.Add(basementEnv);
                    
                    if (ZNetScene.instance == null) return;
                    var sfxstone = ZNetScene.instance.GetPrefab("sfx_build_hammer_stone");
                    var vfxstone = ZNetScene.instance.GetPrefab("vfx_Place_stone_wall_2x1");
                 
                    EffectList buildStone = new EffectList { m_effectPrefabs = new EffectList.EffectData[2] { new EffectList.EffectData { m_prefab = sfxstone }, new EffectList.EffectData { m_prefab = vfxstone } } };
               
                    var pc = BasementsMod.BasementPrefab.GetComponent<Piece>();
                    pc.m_placeEffect = buildStone;
                    if (!EnvMan.instance.m_interiorBuildingOverrideEnvironments.Contains("Basement"))
                        EnvMan.instance.m_interiorBuildingOverrideEnvironments.Add("Basement");
                }

           
        }
    }
}

using System;
using HarmonyLib;
using UnityEngine;
//EnvMan.instance.CheckInteriorBuildingOverride())
namespace Basements.Patches
{
    [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.GenerateLocationsIfNeeded))]
    static class ZoneSystem_Patches
    {

        static void Postfix(ZoneSystem __instance)
        {
                if(__instance.m_locationsGenerated)
                {
                    EnvSetup basementEnv = EnvMan.instance.m_environments.Find(x => x.m_name == "Crypt").Clone();
                    basementEnv.m_name = "Basement";
                    basementEnv.m_psystems = Array.Empty<GameObject>();
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
                    basementEnv.m_alwaysDark = false;
                    basementEnv.m_ambientList = "Basement";
                    EnvMan.instance.m_environments.Add(basementEnv);
                }

           
        }
    }

    [HarmonyPatch(typeof(EnvMan), nameof(EnvMan.CheckInteriorBuildingOverride))]
    static class EnvMan_Patches
    {
        public static void Postfix(EnvMan __instance, ref bool __result)
        {
            if (__instance.GetCurrentEnvironment().m_name.StartsWith("Basement"))
            {
                __result = true;
                Player.m_localPlayer.m_placementStatus = Player.PlacementStatus.Valid;
            }
        }
    }
}
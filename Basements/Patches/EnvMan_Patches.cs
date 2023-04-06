using System;
using HarmonyLib;
using UnityEngine;

namespace Basements.Patches
{
    [HarmonyPatch(typeof(EnvMan), nameof(EnvMan.Awake))]
    static class EnvMan_Patches
    {
        static void Postfix(EnvMan __instance)
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
            EnvMan.instance.m_environments.Add(basementEnv);
        }
    }
}
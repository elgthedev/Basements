using System;
using HarmonyLib;
using UnityEngine;

namespace Basements.Patches
{
    [HarmonyPatch(typeof(Character), nameof(Character.InInterior), typeof(Transform))]
    static class Character_Patches
    {
        static void Postfix(Character __instance, ref bool __result)
        {
            if (Player.m_localPlayer != null)
            {
                if (__instance != Player.m_localPlayer) return;
                if (EnvMan.instance.GetCurrentEnvironment().m_name == "Basement")
                    __result = false;
            }
        }
    }
}
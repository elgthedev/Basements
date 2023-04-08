using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace Basements.Patches
{
    [HarmonyPatch(typeof(Player), nameof(Player.UpdatePlacementGhost))]
    static class Player_UpdatePlacementGhost_Patch
    {
        const float overlapRadius = 60;

        static void Postfix(Player __instance, GameObject ___m_placementGhost)
        {
            if (!___m_placementGhost) return;
            var basementComponent = ___m_placementGhost.GetComponent<Basement>();
            if (!basementComponent) return;
            if (Basement.allBasements.Count <= 0) return;
            Type type = typeof(Player).Assembly.GetType("Player+PlacementStatus");
            object moreSpace = type.GetField("MoreSpace").GetValue(__instance);
            FieldInfo statusField = __instance.GetType()
                .GetField("m_placementStatus", BindingFlags.NonPublic | BindingFlags.Instance);
            var ol = Basement.allBasements
                .Where(x => Vector3.Distance(x.transform.position, ___m_placementGhost.transform.position) <
                            overlapRadius).Where(x => x.gameObject != ___m_placementGhost);
            if (ol.Any(x => x.GetComponentInParent<Basement>()) || ___m_placementGhost.transform.position.y >
                2500 * Mathf.Max(BasementsMod.MaxNestedLimit.Value, 0) + 2000)
            {
                statusField.SetValue(__instance, moreSpace);
            }
        }

        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> UpdatePlacementGhostTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(
                    useEnd: false,
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(Piece), nameof(Piece.m_groundPiece))),
                    new CodeMatch(OpCodes.Brfalse),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldnull),
                    new CodeMatch(
                        OpCodes.Call,
                        AccessTools.Method(
                            typeof(UnityEngine.Object), "op_Equality",
                            new Type[] { typeof(UnityEngine.Object), typeof(UnityEngine.Object) })))
                .Advance(offset: 5)
                .InsertAndAdvance(Transpilers.EmitDelegate<Func<bool, bool>>(HeightmapIsNullBasemementDelegate))
                .MatchForward(
                    useEnd: false,
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(Piece), nameof(Piece.m_groundOnly))),
                    new CodeMatch(OpCodes.Brfalse),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldnull),
                    new CodeMatch(
                        OpCodes.Call,
                        AccessTools.Method(
                            typeof(UnityEngine.Object), "op_Equality",
                            new Type[] { typeof(UnityEngine.Object), typeof(UnityEngine.Object) })))
                .Advance(offset: 5)
                .InsertAndAdvance(Transpilers.EmitDelegate<Func<bool, bool>>(HeightmapIsNullBasemementDelegate))
                .InstructionEnumeration();
        }

        static bool HeightmapIsNullBasemementDelegate(bool isEqual)
        {
            if (EnvMan.m_instance.GetCurrentEnvironment().m_name == "Basement")
            {
                return false;
            }

            return isEqual;
        }
    }
}
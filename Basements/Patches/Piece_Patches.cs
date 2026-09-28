using HarmonyLib;

namespace Basements.Patches
{
    [HarmonyPatch(typeof(Piece), "Awake")]
    static class Basement_Piece_Awake_Patches
    {
        static void Postfix(Piece __instance)
        {
            if (__instance.GetComponentInChildren<Basement>(true) == null) return;
            BasementsMod.ReplaceLegacyMaterials(__instance.gameObject);
        }
    }

    [HarmonyPatch(typeof(Piece), nameof(Piece.CanBeRemoved))]
    static class Piece_Patches
    {
        static void Postfix(Piece __instance, ref bool __result)
        {
            var basement = __instance.GetComponent<Basement>();
            if (basement)
            {
                if (!basement.CanBeRemoved())
                    __result = false;
            }
        }
    }
}

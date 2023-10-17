using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;

namespace UniversalPortalsMod
{
    [HarmonyPatch(typeof(Game), "Awake")]
    public static class Game_Awake
    {
        public static void Postfix(Game __instance)
        {
            UniversalPortalsConfig.instance.LoadFromFile();
        }
    }

    [HarmonyPatch(typeof(Game), "Shutdown")]
    public static class Game_Shutdown
    {
        public static void Prefix()
        {
            UniversalPortalsConfig.instance.SaveToFile();

            if (Minimap_Patch.saved)
            {
                Minimap_Patch.ChangePins(Minimap_Patch.savedPins.Where(x => !Minimap_Patch.excludedPinTypes.Contains(x.m_type)).ToList());
            }

            if (UniversalPortalsConfig.instance.ShowMarkersOnMapSelection)
            {
                Minimap_Patch.ChangePins(Minimap_Patch.GetPins().Where(x => !Minimap_Patch.excludedPinTypes.Contains(x.m_type) && x.m_type != Minimap_Patch.portalPinType).ToList());
            }
        }
    }
    
    [HarmonyPatch(typeof(Game))]
    [HarmonyPatch("ConnectPortals")]
    public static class Game_ConnectPortals_Patch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => Utils.ClearMethod(instructions);
    }

    [HarmonyPatch(typeof(TeleportWorld))]
    [HarmonyPatch("UpdatePortal")]
    public static class TeleportWorld_UpdatePortal_Patch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => Utils.ClearMethod(instructions);
    }

    [HarmonyPatch(typeof(TeleportWorld))]
    [HarmonyPatch("Update")]
    public static class TeleportWorld_Update_Patch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => Utils.ClearMethod(instructions);
    }

    [HarmonyPatch(typeof(TeleportWorld))]
    [HarmonyPatch("Teleport")]
    public static class TeleportWorld_Teleport_Patch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => Utils.ClearMethod(instructions);
    }

    [HarmonyPatch(typeof(Minimap))]
    [HarmonyPatch("RemovePin", new Type[] { typeof(Vector3), typeof(float) })]
    public static class Minimap_RemovePin_Patch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => Utils.ClearMethod(instructions, OpCodes.Ldc_I4_0);
    }
}

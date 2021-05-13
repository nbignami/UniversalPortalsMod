using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Threading;
using UnityEngine;
using OpCode = System.Reflection.Emit.OpCode;

namespace UniversalPortalsMod
{
    public class UniversalPortalsMod
    {
        public static void Main(string[] args)
        {
            new Thread(() =>
            {
                Harmony harmony = new Harmony("com.valheim.universal_portals");

                while (AccessTools.Method(typeof(TeleportWorld), "UpdatePortal") == null)
                {
                    Thread.Sleep(100);
                }

                harmony.PatchAll();
            }).Start();
        }
    }

    [HarmonyPatch(typeof(Game))]
    [HarmonyPatch("Start")]
    public static class Game_Start_Patch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var done = false;
            var index = 0;

            var codeToRemove = new List<CodeInstruction>();

            do
            {
                var code = instructions.ElementAt(index);

                if (code.ToString() == "ldstr \"ConnectPortals\"")
                {
                    codeToRemove.Add(code);
                }
                else if (codeToRemove.Count > 0)
                {
                    if (code.opcode == OpCodes.Ldstr)
                    {
                        done = true;
                    }
                    else
                    {
                        codeToRemove.Add(code);
                    }
                }

                index++;
            } while (!done);

            var instructionsReturn = instructions.ToList();

            foreach (var code in codeToRemove)
            {
                instructionsReturn.Remove(code);
            }

            return instructionsReturn;
        }
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

    public static class Utils
    {
        public static IEnumerable<CodeInstruction> ClearMethod(IEnumerable<CodeInstruction> instructions, OpCode defaultReturn = default, bool logOriginalInstructions = false)
        {
            var codes = new List<CodeInstruction>(instructions);

            if (logOriginalInstructions)
            {
                FileLog.Log("---");
                foreach (var code in codes)
                {
                    FileLog.Log(code.ToString());
                }
                FileLog.Log("---");
            }

            codes.RemoveRange(0, codes.Count - 1);

            if (defaultReturn != default)
            {
                codes.Insert(0, new CodeInstruction(defaultReturn));
            }

            return codes.AsEnumerable();
        }
    }
}
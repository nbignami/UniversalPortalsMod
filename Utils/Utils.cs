using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace UniversalPortalsMod
{
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

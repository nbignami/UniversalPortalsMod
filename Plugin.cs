using BepInEx;
using HarmonyLib;
using System.Threading;

namespace UniversalPortalsMod
{
    [BepInPlugin("com.valheim.universal_portals", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            new Thread(() =>
            {
                Harmony harmony = new("com.valheim.universal_portals");

                while (AccessTools.Method(typeof(TeleportWorld), "UpdatePortal") == null)
                {
                    Thread.Sleep(100);
                }

                harmony.PatchAll();
            }).Start();
        }
    }
}

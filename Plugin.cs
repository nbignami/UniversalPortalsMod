using BepInEx;
using BepInEx.Configuration;
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

            UniversalPortalsConfig.instance.ShowMarkersOnMapSelection = Config.Bind("Universal Portals", "Show markers on map selection", false, new ConfigDescription("Shows the map markers when you are selecting a portal"));
            UniversalPortalsConfig.instance.SaveLastSelection = Config.Bind("Universal Portals", "Save last selection", false, new ConfigDescription("Saves the last destination of the portal used"));

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

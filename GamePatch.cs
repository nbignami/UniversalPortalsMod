using HarmonyLib;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

    [HarmonyPatch(typeof(ZNet), "Start")]
    public static class ZNet_Start
    {
        public static void Postfix()
        {
            if (ZNet.instance.IsServer())
            {
                Task.Run(() =>
                {
                    while (true)
                    {
                        TeleportWorld_Patch.UpdatePortalsServer();

                        Thread.Sleep(1000);
                    }
                });
            }
        }
    }

    [HarmonyPatch(typeof(ZNet), "OnNewConnection")]
    public static class ZnetPatchOnNewConnection
    {
        private static void Postfix(ZNetPeer peer, ZNet __instance)
        {
            if (__instance.IsServer())
            {
                peer.m_rpc.Invoke("SetConfig", new object[]
                {
                    new ZPackage().FromObject(UniversalPortalsConfig.instance)
                });
            }
            else
            {
                peer.m_rpc.Register("UpdatePortals", new Action<ZRpc, ZPackage>(TeleportWorld_Patch.UpdatePortalsClient));
                peer.m_rpc.Register("SetConfig", new Action<ZRpc, ZPackage>(UniversalPortalsConfig.SetConfig));
            }
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
}

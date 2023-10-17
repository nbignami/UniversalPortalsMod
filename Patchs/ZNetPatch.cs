using HarmonyLib;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace UniversalPortalsMod
{
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
            if (!__instance.IsServer())
            {
                peer.m_rpc.Register("UpdatePortals", new Action<ZRpc, ZPackage>(TeleportWorld_Patch.UpdatePortalsClient));
            }
        }
    }
}
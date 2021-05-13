using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UniversalPortalsMod
{
    public class TeleportWorld_Patch
    {
        public static void ShowMapPortalSelection()
        {
            if (HasActiveTeleport)
            {
                return;
            }

            InventoryGui.instance.Hide();

            HasActiveTeleport = !IsSelectingPortal;

            //Todo: Filter portals without permisssion
            var portals = GetPortals();

            var m_pins = Minimap_Patch.GetPins();

            var portalsPins = new Dictionary<Minimap.PinData, ZDO>();

            var pinsToRemove = m_pins.Where(x => !Minimap_Patch.excludedPinTypes.Contains(x.m_type) && x.m_type != Minimap_Patch.portalPinType).ToList();

            foreach (var portal in portals)
            {
                var pin = Minimap.instance.AddPin(portal.GetPosition(), Minimap_Patch.portalPinType, $"<{portal.GetString("tag", "")}>", true, false);
                portalsPins.Add(pin, portal);
            }

            Minimap_Patch.portalsPins = portalsPins;

            if (!UniversalPortalsConfig.instance.ShowMarkersOnMapSelection)
            {
                if (!Minimap_Patch.saved)
                {
                    Minimap_Patch.savedPins = pinsToRemove;

                    Minimap_Patch.saved = true;
                }

                foreach (var pin in pinsToRemove)
                {
                    Minimap.instance.RemovePin(pin);
                }
            }

            if (Minimap_Patch.GetMapMode() != Minimap_Patch.MapMode.Large)
            {
                Minimap_Patch.SetMapMode(Minimap_Patch.MapMode.Large);
            }

            Player.m_localPlayer.Message(MessageHud.MessageType.Center, !IsSelectingPortal ? "Right click on any portal to teleport" : "Right click on any portal to select as default target", 0, null);
        }

        public static void HideMapPortalSelection()
        {
            HasActiveTeleport = false;
            IsSelectingPortal = false;

            var pins = Minimap_Patch.GetPins().Where(x => x.m_type == Minimap_Patch.portalPinType).ToList();

            foreach (var pin in pins)
            {
                Minimap.instance.RemovePin(pin);
            }

            if (!UniversalPortalsConfig.instance.ShowMarkersOnMapSelection)
            {
                if (Minimap_Patch.saved)
                {
                    foreach (var pin in Minimap_Patch.savedPins)
                    {
                        Minimap.instance.AddPin(pin.m_pos, pin.m_type, pin.m_name, pin.m_save, pin.m_checked);
                    }

                    Minimap_Patch.savedPins.Clear();
                    Minimap_Patch.saved = false;
                }
            }

            Minimap_Patch.SetMapMode(Minimap_Patch.MapMode.Small);
        }

        public static List<ZDO> GetPortals()
        {
            var portals = new List<ZDO>();

            if (ZNet.instance.IsServer())
            {
                var m_portalPrefab = AccessTools.FieldRefAccess<Game, GameObject>(Game.instance, "m_portalPrefab");

                ZDOMan.instance.GetAllZDOsWithPrefab(m_portalPrefab.name, portals);
            }
            else
            {
                portals = PortalsZDOIDs.Select(x => ZDOMan.instance.GetZDO(x)).ToList();
            }

            return portals;
        }

        public static void TeleportLocalPlayer(ZDO portal)
        {
            var portalRotation = portal.GetRotation();
            var portalPosition = portal.GetPosition();

            portalPosition = portalPosition + portalRotation * Vector3.forward * 2f + Vector3.up;

            if (Player.m_localPlayer.IsTeleportable())
            {
                Player.m_localPlayer.TeleportTo(portalPosition, portalRotation, true);
            }
            else
            {
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$msg_noteleport", 0, null);
            }
        }

        public static ZDO GetZDO(TeleportWorld teleport)
        {
            var m_nview = AccessTools.FieldRefAccess<TeleportWorld, ZNetView>(teleport, "m_nview");

            return m_nview.GetZDO();
        }

        public static ZDO GetTarget(TeleportWorld teleport)
        {
            var zdo = GetZDO(teleport);
            var zdoid = zdo.GetZDOID("target");

            if (zdoid.IsNone())
            {
                return null;
            }

            return ZDOMan.instance.GetZDO(zdoid);
        }

        public static void SetTarget(TeleportWorld sourceTeleport, TeleportWorld targetTeleport)
        {
            var zdoTarget = GetZDO(targetTeleport);

            SetTarget(sourceTeleport, zdoTarget);
        }

        public static void SetTarget(TeleportWorld sourceTeleport, ZDO zdoTarget)
        {
            SetTarget(sourceTeleport, zdoTarget.m_uid);
        }

        public static void SetTarget(TeleportWorld sourceTeleport, ZDOID zdoid)
        {
            var zdoSource = GetZDO(sourceTeleport);

            zdoSource.Set("target", zdoid);
        }

        public static void UpdatePortalsServer()
        {
            var package = new ZPackage();

            var portals = GetPortals();

            package.Write(portals.Count);

            foreach (var portal in portals)
            {
                ZDOMan.instance.ForceSendZDO(portal.m_uid);

                package.Write(portal.m_uid);
            }

            foreach (ZNetPeer znetPeer in (Traverse.Create(Traverse.Create(typeof(ZNet)).Field("m_instance").GetValue() as ZNet).Field("m_peers").GetValue() as List<ZNetPeer>))
            {
                if (znetPeer.IsReady() && znetPeer.m_rpc != ZNet.instance.GetServerRPC())
                {
                    znetPeer.m_rpc.Invoke("UpdatePortals", new object[]
                    {
                        package
                    });
                }
            }
        }

        public static void UpdatePortalsClient(ZRpc sender, ZPackage package)
        {
            PortalsZDOIDs.Clear();

            var total = package.ReadInt();

            for (int i = 0; i < total; i++)
            {
                PortalsZDOIDs.Add(package.ReadZDOID());
            }
        }

        public static bool HasActiveTeleport;
        public static bool IsSelectingPortal;
        public static TeleportWorld SourceTeleport;
        public static List<ZDOID> PortalsZDOIDs = new List<ZDOID>();
    }

    [HarmonyPatch(typeof(TeleportWorld), "Awake")]
    public class TeleportWorld_Awake
    {
        [HarmonyPostfix]
        public static void Postfix(TeleportWorld __instance)
        {
            var m_connected = AccessTools.FieldRefAccess<TeleportWorld, EffectList>(__instance, "m_connected");
            var m_colorTargetfound = AccessTools.FieldRefAccess<TeleportWorld, Color>(__instance, "m_colorTargetfound");
            var m_model = AccessTools.FieldRefAccess<TeleportWorld, MeshRenderer>(__instance, "m_model");

            m_connected.Create(__instance.transform.position, __instance.transform.rotation, null, 1f);
            m_model.material.SetColor("_EmissionColor", m_colorTargetfound);
        }
    }

    [HarmonyPatch(typeof(TeleportWorld), "UpdatePortal")]
    public class TeleportWorld_UpdatePortal
    {
        [HarmonyPostfix]
        public static void Postfix(TeleportWorld __instance)
        {
            var m_nview = AccessTools.FieldRefAccess<TeleportWorld, ZNetView>(__instance, "m_nview");
            var m_proximityRoot = AccessTools.FieldRefAccess<TeleportWorld, Transform>(__instance, "m_proximityRoot");
            var m_activationRange = AccessTools.FieldRefAccess<TeleportWorld, float>(__instance, "m_activationRange");
            var m_target_found = AccessTools.FieldRefAccess<TeleportWorld, EffectFade>(__instance, "m_target_found");

            if (!m_nview.IsValid())
            {
                return;
            }

            Player closestPlayer = Player.GetClosestPlayer(m_proximityRoot.position, m_activationRange);

            m_target_found.SetActive(closestPlayer);
        }
    }

    [HarmonyPatch(typeof(TeleportWorld), "Interact")]
    public class TeleportWorld_Interact
    {
        public static void Prefix(Humanoid human, ref bool hold, ref TeleportWorld __instance)
        {
            var shiftPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            var altPressed = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) || Input.GetKey(KeyCode.AltGr);

            if (shiftPressed || altPressed)
            {
                if (!hold)
                {
                    if (shiftPressed)
                    {
                        TeleportWorld_Patch.IsSelectingPortal = true;

                        TeleportWorld_Patch.SourceTeleport = __instance;

                        TeleportWorld_Patch.ShowMapPortalSelection();
                    }
                    else if (altPressed)
                    {
                        TeleportWorld_Patch.SetTarget(__instance, ZDOID.None);
                    }
                }
                hold = true;
            }
        }
    }

    [HarmonyPatch(typeof(TeleportWorld), "GetHoverText")]
    public class TeleportWorld_GetHoverText
    {
        [HarmonyPostfix]
        public static void Postfix(ref string __result, TeleportWorld __instance)
        {
            var m_nview = AccessTools.FieldRefAccess<TeleportWorld, ZNetView>(__instance, "m_nview");

            var target = TeleportWorld_Patch.GetTarget(__instance);
            var targetName = (string)null;

            if (target != null)
            {
                targetName = target.GetString("tag", "");
            }

            __result = Localization.instance.Localize(string.Concat(new string[]
            {
                "Portal: \"",
                $"<color=yellow>{m_nview.GetZDO().GetString("tag", "")}{(targetName == null ? "" : $" - {targetName}")}</color>\"",
                "\n[<color=yellow><b>$KEY_Use</b></color>] $piece_portal_settag"
            }));
        }
    }

    [HarmonyPatch(typeof(TeleportWorld), "Teleport")]
    public class TeleportWorld_Teleport
    {
        [HarmonyPostfix]
        public static void PostFix(TeleportWorld __instance, Player player)
        {
            if (player != Player.m_localPlayer)
            {
                return;
            }

            TeleportWorld_Patch.SourceTeleport = __instance;

            var target = TeleportWorld_Patch.GetTarget(__instance);

            if (target == null)
            {
                TeleportWorld_Patch.ShowMapPortalSelection();
            }
            else
            {
                TeleportWorld_Patch.TeleportLocalPlayer(target);
            }
        }
    }
}

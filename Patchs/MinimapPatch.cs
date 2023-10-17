using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using UnityEngine;

namespace UniversalPortalsMod
{
    public class Minimap_Patch
    {
        public static ref List<Minimap.PinData> GetPins()
        {
            return ref AccessTools.FieldRefAccess<Minimap, List<Minimap.PinData>>(Minimap.instance, "m_pins");
        }

        public static void ChangePins(List<Minimap.PinData> pins)
        {
            var m_pins = GetPins().ToList();

            foreach (var pin in m_pins)
            {
                Minimap.instance.RemovePin(pin);
            }

            foreach (var pin in pins)
            {
                Minimap.instance.AddPin(pin.m_pos, pin.m_type, pin.m_name, pin.m_save, pin.m_checked);
            }
        }

        public static void SetMapMode(Minimap.MapMode mode)
        {
            Minimap.instance.SetMapMode(mode);
        }

        public static Minimap.MapMode GetMapMode()
        {
            return Minimap.instance.m_mode;
        }

        public static bool[] GetVisibleIconTypes()
        {
            return AccessTools.FieldRefAccess<Minimap, bool[]>(Minimap.instance, "m_visibleIconTypes");
        }

        public static void GetVisibleIconTypes(bool[] value)
        {
            Traverse.Create(Minimap.instance).Field("m_visibleIconTypes").SetValue(value);
        }

        public static Minimap.PinType[] excludedPinTypes = new Minimap.PinType[] {
                        Minimap.PinType.EventArea,
                        Minimap.PinType.None,
                        Minimap.PinType.Ping,
                        Minimap.PinType.Player,
                        Minimap.PinType.RandomEvent,
                        Minimap.PinType.Shout
        };
        public static Minimap.PinType portalPinType = (Minimap.PinType)17;
        public static bool saved = false;
        public static List<Minimap.PinData> savedPins = new List<Minimap.PinData>();
        public static Dictionary<Minimap.PinData, ZDO> portalsPins = new Dictionary<Minimap.PinData, ZDO>();
    }

    [HarmonyPatch(typeof(Minimap), "Start")]
    public class Minimap_Start
    {
        public static void Postfix()
        {
            var sprite = default(Minimap.SpriteData);
            sprite.m_name = Minimap_Patch.portalPinType;
            sprite.m_icon = Minimap.instance.m_icons.Find((Minimap.SpriteData x) => x.m_name == Minimap.PinType.Icon3).m_icon;
            Minimap.instance.m_icons.Add(sprite);

            var visibleIconTypes = Minimap_Patch.GetVisibleIconTypes();

            Minimap_Patch.GetVisibleIconTypes(visibleIconTypes.ToList().AddItem(true).ToArray());
        }
    }

    [HarmonyPatch(typeof(Minimap), "Update")]
    public class Minimap_Update
    {
        public static void Postfix()
        {
            bool flag = (Chat.instance == null || !Chat.instance.HasFocus()) && !global::Console.IsVisible() && !TextInput.IsVisible() && !Menu.IsVisible() && !InventoryGui.IsVisible();
            if (flag)
            {
                if ((ZInput.GetButtonDown("Map") || Input.GetKeyDown(KeyCode.Escape)) && (TeleportWorld_Patch.IsSelectingPortal || TeleportWorld_Patch.HasActiveTeleport))
                {
                    TeleportWorld_Patch.HideMapPortalSelection();

                    TeleportWorld_Patch.IsSelectingPortal = false;

                    TeleportWorld_Patch.SourceTeleport = null;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Minimap), "RemovePin", new Type[] { typeof(Vector3), typeof(float) })]
    public class Minimap_RemovePin
    {
        public static void Postfix(Vector3 pos, float radius, ref bool __result)
        {
            var GetClosestPinInfo = AccessTools.Method(typeof(Minimap), "GetClosestPin", new Type[] { typeof(Vector3), typeof(float) });

            Minimap.PinData closestPin = (Minimap.PinData)GetClosestPinInfo.Invoke(Minimap.instance, new object[] { pos, radius });

            if (closestPin != null)
            {
                if (closestPin.m_type == Minimap_Patch.portalPinType)
                {
                    var portal = Minimap_Patch.portalsPins.FirstOrDefault(x => x.Key == closestPin).Value;

                    if (UniversalPortalsConfig.instance.SaveLastSelection.Value)
                    {
                        TeleportWorld_Patch.SetTarget(TeleportWorld_Patch.SourceTeleport, portal);
                    }

                    if (TeleportWorld_Patch.IsSelectingPortal)
                    {
                        if (TeleportWorld_Patch.SourceTeleport != null)
                        {
                            TeleportWorld_Patch.SetTarget(TeleportWorld_Patch.SourceTeleport, portal);

                            TeleportWorld_Patch.IsSelectingPortal = false;
                        }
                    }
                    else
                    {
                        TeleportWorld_Patch.TeleportLocalPlayer(portal);
                    }

                    TeleportWorld_Patch.SourceTeleport = null;

                    TeleportWorld_Patch.HideMapPortalSelection();
                }
                else
                {
                    Minimap.instance.RemovePin(closestPin);
                }

                __result = true;
                return;
            }

            __result = false;
        }
    }
}

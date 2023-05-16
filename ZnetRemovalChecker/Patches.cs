using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace ZnetRemovalChecker
{
    public class Patches
    {
        [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.RemoveObjects))]
        public static class RemoveChecker
        {
            // ReSharper disable once InconsistentNaming
            public static bool Prefix(ZNetScene __instance, List<ZDO> currentNearObjects,
                List<ZDO> currentDistantObjects)
            {
                byte num = (byte) (Time.frameCount & (int) byte.MaxValue);
                    foreach (ZDO currentNearObject in currentNearObjects)
                    {
                        currentNearObject.TempRemoveEarmark = num;
                    }
                    foreach (ZDO currentDistantObject in currentDistantObjects)
                    {
                        currentDistantObject.TempRemoveEarmark = num;
                    }
                    __instance.m_tempRemoved.Clear();
                    foreach (var znetView in __instance.m_instances.Values.Where(znetView => (int) znetView.GetZDO().TempRemoveEarmark != (int) num))
                    {
                        __instance.m_tempRemoved.Add(znetView);
                    }

                    if (__instance.m_tempRemoved.Contains(null))
                    {
                        Debug.LogError("Null entry found in m_tempRemoved.");
                        __instance.m_tempRemoved.Remove(null);
                    }

                    for (int index = 0; index < __instance.m_tempRemoved.Count; ++index)
                    {
                        ZNetView znetView = __instance.m_tempRemoved[index];
                        ZDO zdo = znetView.GetZDO();
                        znetView.ResetZDO();
                        UnityEngine.Object.Destroy((UnityEngine.Object) znetView.gameObject);
                        if (!zdo.Persistent && zdo.IsOwner())
                        {
                            ZDOMan.instance.DestroyZDO(zdo);
                        }
                        __instance.m_instances.Remove(zdo);
                    }

                    return false;
            }
        }
    }
}
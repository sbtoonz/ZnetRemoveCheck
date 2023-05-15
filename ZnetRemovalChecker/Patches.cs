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
            public static bool Prefix(ZNetScene __instance, List<ZDO> currentNearObjects,
                List<ZDO> currentDistantObjects)
            {
                int frameCount = Time.frameCount;
                if (currentNearObjects == null)
                {
                    throw new ArgumentNullException("currentNearObjects");
                }

                if (currentDistantObjects == null)
                {
                    throw new ArgumentNullException("currentDistantObjects");
                }
                if (currentNearObjects.Count == 0)
                {
                    Debug.Log("currentNearObjects is empty");
                    return false;
                }

                if (currentDistantObjects.Count == 0)
                {
                    Debug.Log("currentDistantObjects is empty");
                    return false;
                }
                foreach (ZDO currentNearObject in currentNearObjects)
                {
                    if (currentNearObject == null)
                    {
                        Debug.Log("currentNearObjects contains a null object");
                        return false;
                    }

                    currentNearObject.m_tempRemoveEarmark = Time.frameCount;
                }

                foreach (ZDO currentDistantObject in currentDistantObjects)
                {
                    if (currentDistantObject == null)
                    {
                        Debug.Log("currentDistantObjects contains a null object");
                        return false;
                    }

                    currentDistantObject.m_tempRemoveEarmark = Time.frameCount;
                }
                __instance.m_tempRemoved.Clear();
                foreach (var znetView in __instance.m_instances.Values.Where(znetView => znetView == null || znetView.GetZDO() == null || znetView.GetZDO().m_tempRemoveEarmark != frameCount))
                {
                    __instance.m_tempRemoved.Add(znetView);
                }
                foreach (var znetView in __instance.m_tempRemoved)
                {
                    if (znetView == null)
                    {
                        Debug.Log("m_tempRemoved contains a null object");
                        continue;
                    }

                    ZDO zdo = znetView.GetZDO();
                    if (zdo == null)
                    {
                        Debug.Log("znetView.GetZDO() returned a null object");
                        continue;
                    }

                    znetView.ResetZDO();
                    UnityEngine.Object.Destroy((UnityEngine.Object) znetView.gameObject);
                    if (!zdo.m_persistent && zdo.IsOwner())
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
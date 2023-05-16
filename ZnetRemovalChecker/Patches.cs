using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace ZnetRemovalChecker
{
    public class Patches
    {
        [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.RemoveObjects))]
        public static class RemoveChecker
        {
            public static bool Prefix(ZNetScene __instance, List<ZDO> currentNearObjects, List<ZDO> currentDistantObjects)
            {
                var th = ThreadingHelper.Instance;
                byte num = (byte)(Time.frameCount & (int)byte.MaxValue);

                
                currentNearObjects.RemoveAll(obj => obj == null);
               

                th.StartSyncInvoke(() =>
                {
                    foreach (ZDO currentNearObject in currentNearObjects)
                    {
                        currentNearObject.TempRemoveEarmark = num;
                    }
                });

                
                currentDistantObjects.RemoveAll(obj => obj == null);
                

                th.StartSyncInvoke(() =>
                {
                    foreach (ZDO currentDistantObject in currentDistantObjects)
                    {
                        currentDistantObject.TempRemoveEarmark = num;
                    }
                });

                __instance.m_tempRemoved.Clear();
                List<ZNetView?> tempRemoved = new List<ZNetView?>();

                th.StartSyncInvoke(() =>
                {
                    foreach (var znetView in __instance.m_instances.Values)
                    {
                        if (znetView.GetZDO()?.TempRemoveEarmark != num)
                        {
                            tempRemoved.Add(znetView);
                        }
                    }
                });

                if (tempRemoved.Contains(null))
                {
                    Debug.LogError("Null entry found in m_tempRemoved.");
                    tempRemoved.RemoveAll(view => view == null);
                }

                th.StartSyncInvoke(() =>
                {
                    foreach (ZNetView? znetView in tempRemoved)
                    {
                        if(znetView == null) continue;
                        ZDO zdo = znetView.GetZDO();
                        znetView.ResetZDO();
                        UnityEngine.Object.Destroy((UnityEngine.Object)znetView.gameObject);
                        if (!zdo.Persistent && zdo.IsOwner())
                        {
                            ZDOMan.instance.DestroyZDO(zdo);
                        }
                        __instance.m_instances.Remove(zdo);
                    }
                });

                return false;
            }
        }
    }
}
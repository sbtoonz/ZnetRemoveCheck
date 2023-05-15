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
            /*[HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> RemoveObjectsFix(IEnumerable<CodeInstruction> instructions)
            {
                var t = instructions.ToList();
                var removestart = -1;
                var removeend = -1;
                for (int i = 0; i < t.Count(); i++)
                {
                    if (t[i].opcode == OpCodes.Stloc_0)
                    {
                        removestart = i-1;
                    }

                    if (t[i].opcode == OpCodes.Ret) removeend = i;
                }

                if (removestart == -1 || removeend == -1)
                {
                    Debug.LogWarning("Failed to find instructions");
                }
                t.RemoveRange(removestart, removeend);
                t.Insert(++removestart, new CodeInstruction(OpCodes.Ldarg_0));
                t.Insert(++removestart, new CodeInstruction(OpCodes.Ldarg_1));
                t.Insert(++removestart, new CodeInstruction(OpCodes.Ldarg_2));
                t.Insert(++removestart, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patches), nameof(TestHook))));
                return t.AsEnumerable();
            }*/
            // ReSharper disable once InconsistentNaming
            public static bool Prefix(ZNetScene __instance, List<ZDO> currentNearObjects,
                List<ZDO> currentDistantObjects)
            {
                var frameCount = Time.frameCount;
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
                    ZLog.Log("currentNearObjects is empty");
                    return false;
                }

                if (currentDistantObjects.Count == 0)
                {
                    ZLog.Log("currentDistantObjects is empty");
                    return false;
                }
                foreach (ZDO currentNearObject in currentNearObjects)
                {
                    if (currentNearObject == null)
                    {
                        ZLog.Log("currentNearObjects contains a null object");
                        return false;
                    }

                    currentNearObject.m_tempRemoveEarmark = Time.frameCount;
                }

                foreach (ZDO currentDistantObject in currentDistantObjects)
                {
                    if (currentDistantObject == null)
                    {
                        ZLog.Log("currentDistantObjects contains a null object");
                        currentDistantObjects.RemoveAll(x => x == null);
                        return false;
                    }

                    currentDistantObject.m_tempRemoveEarmark = Time.frameCount;
                }
                __instance.m_tempRemoved.Clear();
                foreach (var znetView in __instance.m_instances.Values.Where(znetView => znetView == null || znetView.GetZDO() == null || znetView.GetZDO().m_tempRemoveEarmark != frameCount))
                {
                    __instance.m_tempRemoved.Add(znetView);
                }
                __instance.m_tempRemoved.RemoveAll(x => x == null);
                foreach (var znetView in __instance.m_tempRemoved)
                {
                    if (znetView == null)
                    {
                        ZLog.Log("m_tempRemoved contains a null object");
                        continue;
                    }

                    var zdo = znetView.GetZDO();
                    if (zdo == null)
                    {
                        ZLog.Log("znetView.GetZDO() returned a null object");
                        continue;
                    }

                    znetView.ResetZDO();
                    UnityEngine.Object.Destroy(znetView.gameObject);
                    if (!zdo.m_persistent && zdo.IsOwner())
                    {
                        ZDOMan.instance.DestroyZDO(zdo);
                    }
                    __instance.m_instances.Remove(zdo);
                }

                return false;
            }
        }
            /*public static bool TestHook(ZNetScene __instance, List<ZDO> currentNearObjects,
                List<ZDO> currentDistantObjects)
            {
                var frameCount = Time.frameCount;
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
                    ZLog.Log("currentNearObjects is empty");
                    return false;
                }

                if (currentDistantObjects.Count == 0)
                {
                    ZLog.Log("currentDistantObjects is empty");
                    return false;
                }
                foreach (ZDO currentNearObject in currentNearObjects)
                {
                    if (currentNearObject == null)
                    {
                        ZLog.Log("currentNearObjects contains a null object");
                        return false;
                    }

                    currentNearObject.m_tempRemoveEarmark = Time.frameCount;
                }

                foreach (ZDO currentDistantObject in currentDistantObjects)
                {
                    if (currentDistantObject == null)
                    {
                        ZLog.Log("currentDistantObjects contains a null object");
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
                        ZLog.Log("m_tempRemoved contains a null object");
                        continue;
                    }

                    var zdo = znetView.GetZDO();
                    if (zdo == null)
                    {
                        ZLog.Log("znetView.GetZDO() returned a null object");
                        continue;
                    }

                    znetView.ResetZDO();
                    UnityEngine.Object.Destroy(znetView.gameObject);
                    if (!zdo.m_persistent && zdo.IsOwner())
                    {
                        ZDOMan.instance.DestroyZDO(zdo);
                    }
                    __instance.m_instances.Remove(zdo);
                }

                return false;
            }*/
    }
}
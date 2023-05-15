using System.Reflection;
using BepInEx;
using HarmonyLib;

namespace ZnetRemovalChecker
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class ZnetRemovalCheckerMod : BaseUnityPlugin
    {
        private const string ModName = "ZnetRemovalCheckerMod";
        private const string ModVersion = "1.0";
        private const string ModGUID = "ZnetRemovalCheckerMod";
        private static Harmony harmony = null!;
       
        public void Awake()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            harmony = new(ModGUID);
            harmony.PatchAll(assembly);
        }
    }
}

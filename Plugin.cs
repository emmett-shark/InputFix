using System.Linq;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace InputFix;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    private void Awake()
    {
        Instance = this;
        Log = Logger;
        new Harmony(PluginInfo.PLUGIN_GUID).PatchAll();
    }

    public static void LogDebug(string message) => Log.LogDebug(message);

    public static Plugin Instance;
    public static ManualLogSource Log;
    
    [HarmonyPatch(typeof(GameController))]
    public static class Patches
    {
        [HarmonyPatch(nameof(GameController.isNoteButtonPressed))]
        public static bool Prefix(GameController __instance, ref bool __result)
        {
            bool isKeyDown = Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) ||
                (Input.anyKeyDown && __instance.toot_keys.Any(key => Input.GetKeyDown(key)));
            __result = !isKeyDown && (Input.GetMouseButton(0) || Input.GetMouseButton(1) ||
                (Input.anyKey && __instance.toot_keys.Any(key => Input.GetKey(key))));
            if (__result && !__instance.quitting && !__instance.enteringlyrics && __instance.curtainc.controller_ready_state == 1)
            {
                __instance.curtainc.controller_ready_state = 2;
                __instance.curtainc.openCurtains();
                __instance.startSong(false);
            }
            return false;
        }
    }
}


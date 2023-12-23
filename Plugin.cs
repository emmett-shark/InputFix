using System.Collections.Generic;
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

    public static Plugin Instance;
    public static ManualLogSource Log;
    
    [HarmonyPatch(typeof(GameController))]
    public static class Patches
    {
        private static bool previousFrameTooting = false;
        [HarmonyPatch(nameof(GameController.isNoteButtonPressed))]
        public static bool Prefix(GameController __instance, ref bool __result)
        {
            bool enableMouseTooting = !GlobalVariables.localsettings.disable_mouse_tooting;
            bool isMouseDown = enableMouseTooting && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1));
            bool isMouse = enableMouseTooting && (Input.GetMouseButton(0) || Input.GetMouseButton(1));

            bool isKeyDown = Input.anyKeyDown && __instance.toot_keys.Any(key => Input.GetKeyDown(key));
            bool isKey = Input.anyKey && __instance.toot_keys.Any(key => Input.GetKey(key));

            __result = (!(isMouseDown || isKeyDown) || !previousFrameTooting) && (isMouse || isKey);
            previousFrameTooting = isKey;
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

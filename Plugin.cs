using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace InputFix
{
    [HarmonyPatch]
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake(){
            Instance = this;
            new Harmony(PluginInfo.PLUGIN_GUID).PatchAll();
        }
        public static void LogDebug(string message){
            Instance.Log(LogLevel.Debug, message);
        }

        public void Log(LogLevel logLevel, string message){
            Logger.Log(logLevel, message);
        }

        private static Plugin Instance;
    }
    

}
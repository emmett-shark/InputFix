using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace InputFix
{
    [HarmonyPatch]
    [BepInPlugin("InputFix", "InputFix", "1.0.3")]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake(){
            Instance = this;
            new Harmony("InputFix").PatchAll();
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
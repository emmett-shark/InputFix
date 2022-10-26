using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Audio;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using UnityEngine.Events;
using System.IO;
using BepInEx.Configuration;

namespace InputFix
{
    [HarmonyPatch]
    [BepInPlugin("InputFix", "InputFix", "1.0.1")]
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
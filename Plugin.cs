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
using TrombSettings;

namespace InputFix
{
    [HarmonyPatch]
    [BepInPlugin("InputFix", "InputFix", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake(){
            new Harmony("InputFix").PatchAll();
        }
    }

}
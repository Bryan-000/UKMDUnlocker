namespace UKMDUnlocker.Compat;

using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using AngryLevelLoader.Fields;
using AngryUiComponents;
using BepInEx.Bootstrap;
using HarmonyLib;
using PluginConfig.API.Fields;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class AngryFix
{
    public static DifficultyField angryDifficultyField => hasAngry? ALLPlugin.difficultyField : null;
    public static bool hasAngry => Chainloader.PluginInfos.ContainsKey(ALLPlugin.PLUGIN_GUID);

    public static void Init()
    {
        if (!hasAngry) return;

        Plugin.Log.LogInfo($"Detected {ALLPlugin.PLUGIN_GUID}");
        Plugin.Harmony.PatchAll(typeof(Patches));
    }

    public static class Patches
    {
        [HarmonyPrefix] [HarmonyPatch(typeof(ALLPlugin), "Start")]
        public static void AddUKMDToDifficultyList(ref List<string> ___difficultyList)
        {
            ___difficultyList.Add("UKMD");
            Plugin.Log.LogInfo($"Added UKMD to Angry's Difficulty List");
        }
    }
}
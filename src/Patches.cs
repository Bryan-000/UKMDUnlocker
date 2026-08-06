namespace UKMDUnlocker;

using HarmonyLib;
using plog;
using System;
using System.Collections.Generic;

[HarmonyPatch]
public static class Patches
{
    [HarmonyPostfix] [HarmonyPatch(typeof(PrefsManager), MethodType.Constructor)]
    public static void AllowUKMD(Logger ___Log, ref Dictionary<string, Func<object, object>> ___propertyValidators)
    {
        if (!___propertyValidators.ContainsKey("difficulty")) return; // Just in case

        // remove the old difficulty check
        ___propertyValidators.Remove("difficulty");

        // add a new one that forces difficulty to be in the range 0..5 instead of 0..4
        ___propertyValidators.Add("difficulty", (value) =>
        {
            if (value is not int)
            {
                ___Log.Warning("Difficulty value is not an int");
                return GameDifficulty.Standard;
            }

            var difficulty = (int)value;
            if (difficulty < 0 || difficulty > 5)
            {
                ___Log.Warning("Difficulty validation error");
                return GameDifficulty.UKMD;
            }

            // use the passed in value
            return value;
        });
    }
}

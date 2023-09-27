using HarmonyLib;
using Reptile;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace CustomGraffiti.Patches
{
    [HarmonyPatch(typeof(GraffitiArtInfo))]
    internal class PatchGraffitiArtInfo
    {
        // GraffitiArtInfo

        // GetAvailableTargets
        // Goal: Add more target combos to the game -- allowing for custom graffiti entirely
        [HarmonyPatch("GetAvailableTargets")]
        [HarmonyPrefix]
        static bool Prefix(GraffitiArtInfo __instance, List<GraffitiArt> unlockedGraffitiArt, List<int> targetSequence, GraffitiSize grafSize, ref int __result)
        {
            // this is mostly decomp aka unreadable!!!

            int sequence = 0;
            int maxTargets = (int)(grafSize + (int)GraffitiSize.XL);
            int count = targetSequence.Count;
            if (grafSize == GraffitiSize.S || count == maxTargets)
            {
                __result = int.MaxValue;
                return false;
            }

            // original unlocked art

            for (int i = 0; i < unlockedGraffitiArt.Count; i++)
            {
                int combos = (int)unlockedGraffitiArt[i].combos;
                int num4 = (int)Mathf.Pow(10f, (float)(maxTargets - 1 - count));
                int num5 = combos / num4 % 10;
                int num6 = 1;
                for (int j = 0; j < count; j++)
                {
                    num4 = (int)Mathf.Pow(10f, (float)(maxTargets - 1 - (count - j - 1)));
                    int num7 = combos / num4 % 10;
                    num6 &= Convert.ToInt32(num7 == targetSequence[count - j - 1]);
                }
                sequence |= num6 << num5;
            }

            // injection

            List<CustomGraffiti> customGraffiti = CustomGraffitiMod.GetCustomGraffitiBySize(grafSize);

            for (int i = 0; i < customGraffiti.Count; i++)
            {
                int combos = (int)customGraffiti[i].Combo;
                int num4 = (int)Mathf.Pow(10f, (float)(maxTargets - 1 - count));
                int num5 = combos / num4 % 10;
                int num6 = 1;
                for (int j = 0; j < count; j++)
                {
                    num4 = (int)Mathf.Pow(10f, (float)(maxTargets - 1 - (count - j - 1)));
                    int num7 = combos / num4 % 10;
                    num6 &= Convert.ToInt32(num7 == targetSequence[count - j - 1]);
                }
                sequence |= num6 << num5;
            }

            __result = sequence;
            return false;
        }
    }
}

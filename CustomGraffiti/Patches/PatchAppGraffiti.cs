using HarmonyLib;
using Reptile.Phone;
using Reptile;
using System.Collections.Generic;

namespace CustomGraffiti.Patches
{
    [HarmonyPatch(typeof(AppGraffiti))]
    internal class PatchAppGraffiti
    {
        // AppGraffiti

        // RefreshList
        // Goal: Add app entries.
        [HarmonyPatch("RefreshList")]
        [HarmonyPostfix]
        static void Postfix(AppGraffiti __instance)
        {
            Traverse traverse = Traverse.Create(__instance);

            List<GraffitiAppEntry> graffitiList = traverse.Property("GraffitiArt").GetValue<List<GraffitiAppEntry>>();

            GraffitiScrollView m_ScrollView = traverse.Field("m_ScrollView").GetValue<GraffitiScrollView>();

            List<CustomGraffiti> customGraffiti = CustomGraffitiMod.LoadedGraffiti;

            for (int i = 0; i < customGraffiti.Count; i++)
            {
                CustomGraffiti graffiti = customGraffiti[i];

                if (graffiti.Size == GraffitiSize.S)
                    continue;

                if (graffiti.AppEntry == null)
                {
                    CustomGraffitiMod.Log.LogWarning($"Cannot create app entry for {graffiti.Name}.");
                    continue;
                }

                graffitiList.Add(graffiti.AppEntry);
            }

            traverse.Property("GraffitiArt").SetValue(graffitiList);

            m_ScrollView.SetListContent(graffitiList.Count);

            traverse.Field("m_ScrollView").SetValue(m_ScrollView);
        }
    }
}

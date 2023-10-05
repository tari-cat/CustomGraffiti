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

            Traverse<List<GraffitiAppEntry>> graffitiList = traverse.Property<List<GraffitiAppEntry>>("GraffitiArt");

            Traverse<GraffitiScrollView> m_ScrollView = traverse.Field<GraffitiScrollView>("m_ScrollView");

            List<CustomGraffiti> customGraffiti = CustomGraffitiMod.LoadedGraffiti;

            List<GraffitiAppEntry> graffitiAppEntries = graffitiList.Value;

            for (int i = 0; i < graffitiAppEntries.Count; i++)
            {
                for (int j = 0; j < customGraffiti.Count; j++)
                {
                    if (graffitiAppEntries[i].Combos == customGraffiti[j].combos)
                    {
                        graffitiAppEntries.RemoveAt(i);
                        i--;
                        break;
                    }
                }
            }

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

                graffitiAppEntries.Add(graffiti.AppEntry);
            }

            graffitiList.Value = graffitiAppEntries;

            m_ScrollView.Value.SetListContent(graffitiAppEntries.Count);
        }
    }
}

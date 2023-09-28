using HarmonyLib;
using Reptile;
using static Reptile.GraffitiGame;
using System.Collections.Generic;
using System.Reflection;
using System;
using UnityEngine;

namespace CustomGraffiti.Patches
{
    [HarmonyPatch(typeof(GraffitiGame))]
    internal class PatchGraffitiGame
    {
        // GraffitiGame

        // SetState
        // Goal: Apply custom graffiti texture to graffiti spot upon spray.
        [HarmonyPatch("SetState")]
        [HarmonyPrefix]
        static bool Prefix(GraffitiGame __instance, GraffitiGameState setState)
        {
            if (setState != GraffitiGameState.SHOW_PIECE)
                return true;

            Traverse traverse = Traverse.Create(__instance);

            GraffitiGameState state = traverse.Field("state").GetValue<GraffitiGameState>();
            float stateTimer = traverse.Field("stateTimer").GetValue<float>();

            state = setState;
            stateTimer = 0f;

            traverse.Field("state").SetValue(state);
            traverse.Field("stateTimer").SetValue(stateTimer);

            GraffitiSpot gSpot = traverse.Field("gSpot").GetValue<GraffitiSpot>();
            GraffitiArt grafArt = traverse.Field("grafArt").GetValue<GraffitiArt>();
            GraffitiArtInfo graffitiArtInfo = traverse.Field("graffitiArtInfo").GetValue<GraffitiArtInfo>();
            Player player = traverse.Field("player").GetValue<Player>();
            LineRenderer line = traverse.Field("line").GetValue<LineRenderer>();

            FieldInfo playerCharacterField = player.GetType().GetField("character", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
            Characters character = (Characters)playerCharacterField.GetValue(player);
            List<int> targetsHitSequence = traverse.Field("targetsHitSequence").GetValue<List<int>>();

            object targetsObjectArray = traverse.Field("targets").GetValue();

            object[] targetArray = targetsObjectArray as object[];

            if (gSpot.size == GraffitiSize.S)
            {
                List<CustomGraffiti> graffiti = CustomGraffitiMod.GetCustomGraffitiBySize(gSpot.size);

                if (graffiti.Count > 0)
                {
                    CustomGraffiti random = graffiti[UnityEngine.Random.Range(0, graffiti.Count)];

                    grafArt = random.Art;
                }
                else
                {
                    grafArt = graffitiArtInfo.FindByCharacter(character);
                }

                traverse.Field("grafArt").SetValue(grafArt);

                line.gameObject.SetActive(false);
                foreach (object target in targetArray)
                {
                    if (target != null)
                    {
                        Type t = target.GetType();
                        PropertyInfo transformField = t.GetProperty("Transform", BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public);
                        object transformObject = transformField.GetValue(target);
                        Transform transform = transformObject as Transform;
                        UnityEngine.Object.Destroy(transform.gameObject);
                    }
                }
            }
            else
            {
                if (targetsHitSequence.Count > (int)(gSpot.size + (int)GraffitiSize.XL))
                    targetsHitSequence.RemoveAt(targetsHitSequence.Count - 1);

                // grafArt == null if custom

                List<CustomGraffiti> graffiti = CustomGraffitiMod.GetCustomGraffitiByCombo(ParseComboBySequence(targetsHitSequence));

                if (graffiti.Count > 0)
                {
                    CustomGraffiti random = graffiti[UnityEngine.Random.Range(0, graffiti.Count)];

                    grafArt = random.Art;
                }

                if (grafArt == null)
                {
                    grafArt = graffitiArtInfo.FindBySequence(targetsHitSequence);
                }

                traverse.Field("grafArt").SetValue(grafArt);
            }

            Player.TrickType type = Player.TrickType.GRAFFITI_S;
            if (gSpot.size == GraffitiSize.M)
            {
                type = Player.TrickType.GRAFFITI_M;
            }
            if (gSpot.size == GraffitiSize.L)
            {
                type = Player.TrickType.GRAFFITI_L;
            }
            if (gSpot.size == GraffitiSize.XL)
            {
                type = Player.TrickType.GRAFFITI_XL;
            }

            player.DoTrick(type, grafArt.title);
            bool oldBottomWasClaimedByPlayableCrew = gSpot.bottomCrew == Crew.PLAYERS || gSpot.bottomCrew == Crew.ROGUE;

            //gSpot.Paint(Crew.PLAYERS, grafArt, null);
            MethodInfo paintMethod = gSpot.GetType().GetMethod("Paint", BindingFlags.NonPublic | BindingFlags.Instance);
            paintMethod.Invoke(gSpot, new object[] { Crew.PLAYERS, grafArt, null });

            gSpot.GiveRep(false, oldBottomWasClaimedByPlayableCrew);

            MethodInfo setStateVisualMethod = __instance.GetType().GetMethod("SetStateVisual", BindingFlags.NonPublic | BindingFlags.Instance);
            setStateVisualMethod.Invoke(__instance, new object[] { state });
            //SetStateVisual(state);
            return false;
        }

        static GraffitiArt.Combos ParseComboBySequence(List<int> sequence)
        {
            int sequenceInt = 0;
            int count = sequence.Count;
            for (int i = 0; i < count; i++)
            {
                sequenceInt += sequence[i] * (int)Mathf.Pow(10f, (float)(count - i - 1));
            }
            GraffitiArt.Combos seq = (GraffitiArt.Combos)sequenceInt;
            return seq;
        }
    }
}

using HarmonyLib;
using Reptile;
using static Reptile.GraffitiGame;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

namespace CustomGraffiti
{
    internal class Patches
    {
        // GraffitiGame

        // SetState
        // Goal: Apply custom graffiti texture to graffiti spot upon spray.
        [HarmonyPatch(typeof(GraffitiGame), "SetState")]
        class Patch_GraffitiGame_SetState
        {
            static bool Prefix(GraffitiGame __instance, GraffitiGameState setState)
            {
                if (setState != GraffitiGameState.SHOW_PIECE)
                    return true;

                GraffitiGameState state = Traverse.Create(__instance).Field("state").GetValue<GraffitiGameState>();
                float stateTimer = Traverse.Create(__instance).Field("stateTimer").GetValue<float>();

                state = setState;
                stateTimer = 0f;

                Traverse.Create(__instance).Field("state").SetValue(state);
                Traverse.Create(__instance).Field("stateTimer").SetValue(stateTimer);

                GraffitiSpot gSpot = Traverse.Create(__instance).Field("gSpot").GetValue<GraffitiSpot>();
                GraffitiArt grafArt = Traverse.Create(__instance).Field("grafArt").GetValue<GraffitiArt>();
                GraffitiArtInfo graffitiArtInfo = Traverse.Create(__instance).Field("graffitiArtInfo").GetValue<GraffitiArtInfo>();
                Player player = Traverse.Create(__instance).Field("player").GetValue<Player>();
                LineRenderer line = Traverse.Create(__instance).Field("line").GetValue<LineRenderer>();

                FieldInfo playerCharacterField = player.GetType().GetField("character", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
                Characters character = (Characters)playerCharacterField.GetValue(player);
                List<int> targetsHitSequence = Traverse.Create(__instance).Field("targetsHitSequence").GetValue<List<int>>();

                object targetsObjectArray = Traverse.Create(__instance).Field("targets").GetValue();

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

                    Traverse.Create(__instance).Field("grafArt").SetValue(grafArt);

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

                    Traverse.Create(__instance).Field("grafArt").SetValue(grafArt);
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

        // GraffitiArtInfo

        // GetAvailableTargets
        // Goal: Add more target combos to the game -- allowing for custom graffiti entirely
        [HarmonyPatch(typeof(GraffitiArtInfo), "GetAvailableTargets")]
        class Patch_GraffitiArtInfo_GetAvailableTargets
        {
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
}

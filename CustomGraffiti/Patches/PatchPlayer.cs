
using HarmonyLib;
using Reptile;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static Reptile.Player;

namespace CustomGraffiti.Patches
{
    [HarmonyPatch(typeof(Player))]
    internal class PatchPlayer
    {
        // Player
        // DoTrick

        // Goal: To avoid this mod being banned in SlopCrew/Multiplayer, don't give extra points for custom graffiti.
        // No other way to approach this at the moment, but may be updated when the API updates.

        [HarmonyPatch("DoTrick")]
        [HarmonyPrefix]
        static bool Prefix(Player __instance, TrickType type, string trickName = "", int trickNum = 0)
        {
            Traverse traverseInstance = Traverse.Create(__instance);

            MethodInfo isGraffitiTypeTrickMethod = __instance.GetType().GetMethod("IsGraffitiTypeTrick", BindingFlags.NonPublic | BindingFlags.Instance);
            bool isGraffiti = (bool) isGraffitiTypeTrickMethod.Invoke(__instance, new object[] { type });

            if (!isGraffiti)
                return true;

            int currentTrickPoints = traverseInstance.Field("currentTrickPoints").GetValue<int>();
            int newGraffitiBonusMulti = traverseInstance.Field("newGraffitiBonusMulti").GetValue<int>();
            List<string> graffitiTitlesDone = traverseInstance.Field("graffitiTitlesDone").GetValue<List<string>>();

            if (type == TrickType.GRAFFITI_S)
            {
                currentTrickPoints = 500;

                // BUG: This is a bug in the base game, try it out for yourself
                // Basically, if you get a new graffiti bonus multiplier by using a larger graffiti
                // and then spray a small graffiti, this multiplier below is never reset to 1

                // this unfortunately needs to be patched on my end to prevent a ban
                newGraffitiBonusMulti = 1;
            }
            else
            {
                // To avoid this mod being banned in SlopCrew/Multiplayer, don't give extra points for custom graffiti.
                // No other way to approach this at the moment, but may be updated when the API updates.
                List<CustomGraffiti> list = CustomGraffitiMod.GetCustomGraffitiByName(trickName);
                bool isCustom = list.Count > 0;
                CustomGraffitiMod.Log.LogInfo($"Is custom: {isCustom}");
                newGraffitiBonusMulti = (isCustom || graffitiTitlesDone.Contains(trickName) ? 1 : 2);
                graffitiTitlesDone.Add(trickName);
                traverseInstance.Field("graffitiTitlesDone").SetValue(graffitiTitlesDone);
                if (type == TrickType.GRAFFITI_M)
                {
                    currentTrickPoints = 1000 * newGraffitiBonusMulti;
                }

                if (type == TrickType.GRAFFITI_L)
                {
                    currentTrickPoints = 1600 * newGraffitiBonusMulti;
                }

                if (type == TrickType.GRAFFITI_XL)
                {
                    currentTrickPoints = 2000 * newGraffitiBonusMulti;
                }
            }

            CustomGraffitiMod.Log.LogInfo($"{currentTrickPoints} / {newGraffitiBonusMulti}");

            traverseInstance.Field("currentTrickPoints").SetValue(currentTrickPoints);
            traverseInstance.Field("newGraffitiBonusMulti").SetValue(newGraffitiBonusMulti);

            // default code (nightmare)

            __instance.ResetComboTimeOut();

            float moveInScoreTimer = traverseInstance.Field("moveInScoreTimer").GetValue<float>();
            TrickType currentTrickType = traverseInstance.Field("currentTrickType").GetValue<TrickType>();
            int tricksInCombo = traverseInstance.Field("tricksInCombo").GetValue<int>();
            GameplayUI ui = traverseInstance.Field("ui").GetValue<GameplayUI>();
            AnimationCurve gainBoostChargeCurve = traverseInstance.Field("gainBoostChargeCurve").GetValue<AnimationCurve>();
            float showAddCharge = traverseInstance.Field("showAddCharge").GetValue<float>();
            float scoreMultiplier = traverseInstance.Field("scoreMultiplier").GetValue<float>();
            string currentTrickName = traverseInstance.Field("currentTrickName").GetValue<string>();
            bool currentTrickOnFoot = traverseInstance.Field("currentTrickOnFoot").GetValue<bool>();
            bool usingEquippedMovestyle = traverseInstance.Field("usingEquippedMovestyle").GetValue<bool>();
            float scoreFactor = traverseInstance.Field("scoreFactor").GetValue<float>();
            bool didAbilityTrick = traverseInstance.Field("didAbilityTrick").GetValue<bool>();
            float baseScore = traverseInstance.Field("baseScore").GetValue<float>();

            moveInScoreTimer = 1f;
            if ((currentTrickType == TrickType.SLIDE && type == TrickType.SLIDE) || (currentTrickType == TrickType.SOFT_CORNER && type == TrickType.SOFT_CORNER))
            {
                moveInScoreTimer = 0f;
            }
            else
            {
                if (tricksInCombo == 0 && ui != null)
                {
                    ui.SetTrickingChargeBarActive(setActive: true);
                }

                tricksInCombo++;
                if (tricksInCombo >= 5)
                {
                    __instance.AddBoostCharge(showAddCharge = gainBoostChargeCurve.Evaluate(Mathf.Min(tricksInCombo, 50f) / 50f));
                    traverseInstance.Field("showAddCharge").SetValue(showAddCharge);
                }
                traverseInstance.Field("tricksInCombo").SetValue(tricksInCombo);
            }
            traverseInstance.Field("moveInScoreTimer").SetValue(moveInScoreTimer);

            if (type != TrickType.GRIND_START && type != TrickType.SLIDE && !isGraffiti && type != TrickType.SOFT_CORNER)
            {
                bool flag = type == TrickType.AIR_BOOST || type == TrickType.GRIND_BOOST || type == TrickType.GROUND_BOOST || type == TrickType.SPECIAL_AIR;

                __instance.TryToRemoveCuff((!flag) ? 1 : 5);
            }

            if (scoreMultiplier == 0f)
            {
                scoreMultiplier = 1f;
                traverseInstance.Field("scoreMultiplier").SetValue(scoreMultiplier);
            }

            if (trickName == "")
            {
                trickName = type.ToString();
            }

            currentTrickType = type;
            traverseInstance.Field("currentTrickType").SetValue(currentTrickType);
            currentTrickName = trickName;
            traverseInstance.Field("currentTrickName").SetValue(currentTrickName);
            currentTrickOnFoot = !usingEquippedMovestyle;
            traverseInstance.Field("currentTrickOnFoot").SetValue(currentTrickOnFoot);
            baseScore += (int)((float)currentTrickPoints * scoreFactor);
            traverseInstance.Field("baseScore").SetValue(baseScore);
            didAbilityTrick = true;
            traverseInstance.Field("didAbilityTrick").SetValue(didAbilityTrick);

            return false;
        }
    }
}

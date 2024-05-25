﻿using System.Collections.Generic;
using HarmonyLib;
using IPA.Utilities;

namespace BeatSaber5.HarmonyPatches {
    // TODO: should prob combine this, GameplayModifiersData and SetMultipliers into one file
    [HarmonyPatch(typeof(GameplayModifiersModelSO))]
    public class GameplayModifiersModel {
        [HarmonyPostfix]
        [HarmonyPatch("OnEnable")]
        static void PrintExclusives(Dictionary<GameplayModifierParamsSO, GameplayModifiersModelSO.GameplayModifierBoolGetter> ____gameplayModifierGetters) {
            foreach (var pair in ____gameplayModifierGetters) {
                Plugin.Log.Info($"{pair.Key.name}:");
                foreach (var gmpso in pair.Key.mutuallyExclusives) {
                    Plugin.Log.Info(gmpso.name);
                }
                Plugin.Log.Info("");
            }
        }

        /*[HarmonyPatch(nameof(GameplayModifiersModelSO.GetTotalMultiplier))]
        static void Prefix(List<GameplayModifierParamsSO> modifierParams) {
            foreach (var mod in modifierParams) {
                Plugin.Log.Info($"{mod.name} {mod.multiplier} {mod.multiplierConditionallyValid} {mod.isInBeta}");
            }
        }*/

        /*[HarmonyPatch(nameof(GameplayModifiersModelSO.CreateModifierParamsList))]
        static void Prefix(Dictionary<GameplayModifierParamsSO, GameplayModifiersModelSO.GameplayModifierBoolGetter> ____gameplayModifierGetters) {
            foreach (var p in ____gameplayModifierGetters) {
                Plugin.Log.Info($"PRE {p.Key.modifierNameLocalizationKey}");
            }
        }*/

        /* very cool footgun
         * because of the patch forcing smallcubes and na to always be false, we have to add them manually here,
         * otherwise their multiplier will never be counted. They have to be added back in like this because we want
         * to overwrite the mod; if we don't force the bool then they'll just activate like normal along with the
         * added modifier that we're adding over.
         */
        [HarmonyPostfix]
        [HarmonyPatch(nameof(GameplayModifiersModelSO.CreateModifierParamsList))]
        static void ReEnableModifiers(ref List<GameplayModifierParamsSO> __result, GameplayModifiersModelSO __instance) {
            // TODO: move modifier disabling to here, otherwise it messes up when you exit level
            // I can't remember why I needed to do this and now it isn't happening anymore
            if (!Config.Instance.Enabled) return;
            
            if (Config.Instance.SameColor) {
                __result.Add(__instance.GetField<GameplayModifierParamsSO, GameplayModifiersModelSO>("_smallCubes"));
            }
            if (Config.Instance.EasyMode) {
                __result.Add(__instance.GetField<GameplayModifierParamsSO, GameplayModifiersModelSO>("_noArrows"));
            }
        }
    }
}
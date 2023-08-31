﻿using BeatSaberMarkupLanguage;
using HarmonyLib;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace BeatSaber5.HarmonyPatches {
    // force pro mode
    [HarmonyPatch(typeof(BeatmapObjectsInstaller), nameof(BeatmapObjectsInstaller.InstallBindings))]
    public class ProModePatch {
        static void Prefix(ref GameNoteController ____normalBasicNotePrefab, ref GameNoteController ____proModeNotePrefab) {
            if (!Config.Instance.Enabled) return;
            ____normalBasicNotePrefab = ____proModeNotePrefab;
        }
    }

    // COMMENTED OUT CODE IN MASTER :20:
    /*[HarmonyPatch(typeof(SphereCuttableBySaber), nameof(SphereCuttableBySaber.Awake))]
    static class BigBombPatch {
        static void Prefix(ref SphereCollider ____collider) {
            if (ModifierTogglePatch.BigBombs) ____collider.radius = 10.25f;
        }
    }

    [HarmonyPatch(typeof(GameplayModifierToggle), "get_toggle")]
    static class ModifierTogglePatch {
        public static bool BigBombs;
        static void Postfix(ref Toggle __result, ref GameplayModifierParamsSO ____gameplayModifier) {
            switch (____gameplayModifier.modifierNameLocalizationKey) {
                case "MODIFIER_DISAPPEARING_ARROWS": BigBombs = __result.isOn; 
                    __result.isOn = false; break;
            }
        }
    }*/


    
    /*[HarmonyPatch(typeof(BoxCuttableBySaber), nameof(BoxCuttableBySaber.Awake))]
    static class ColliderBruhPatch {
        public
            static void Prefix(ref BoxCollider ____collider, BoxCuttableBySaber __instance) {
            ____collider.size = new Vector3(5f, 5f, 5f);
        }
    }*/



    [HarmonyPatch(typeof(BeatmapObjectSpawnController.InitData), MethodType.Constructor, typeof(float), typeof(int), typeof(float), typeof(BeatmapObjectSpawnMovementData.NoteJumpValueType), typeof(float))]
    public class NjsPatch {
        static void Postfix(ref float ___noteJumpMovementSpeed) {
            if (SongSpeedPatch.SongSpeed <= 1) return;
            ___noteJumpMovementSpeed *= Multiplier(SongSpeedPatch.SongSpeed) / SongSpeedPatch.SongSpeed;
        }

        private static float Multiplier(float speed) {
            switch (speed) {
                case 1.2f: return 1.1f;
                case 1.5f: return 1.2f;
                default: return 1f;
            }
        }
    }

    [HarmonyPatch(typeof(GameplayModifiers), "get_songSpeedMul")]
    public class SongSpeedPatch {
        public static float SongSpeed;

        static void Postfix(ref float __result) {
            if (__result < 0.9f) __result = 0.75f;
            SongSpeed = __result;
        }
    }

    // looks like something with this is bugged if u start the game with the mod off
    [HarmonyPatch(typeof(GameplayModifierParamsSO), "get_multiplier")]
    public class MultiplierPatch {
        static void Postfix(ref float __result, GameplayModifierParamsSO __instance) {
            switch (__instance.modifierNameLocalizationKey) {
                case "MODIFIER_SLOWER_SONG":     __result = -0.5f; break;
                case "MODIFIER_FASTER_SONG":     __result = 0.07f; break;
                case "MODIFIER_SUPER_FAST_SONG": __result = 0.15f; break;
                case "MODIFIER_STRICT_ANGLES":   __result = 0.11f; break;
                case "MODIFIER_GHOST_NOTES":     __result = 0.05f; break;
            }

            /* MODIFIER_NO_FAIL_ON_0_ENERGY
             * MODIFIER_NO_BOMBS
             * MODIFIER_GHOST_NOTES
             * MODIFIER_PRO_MODE
             * MODIFIER_SLOWER_SONG
             * MODIFIER_ONE_LIFE
             * MODIFIER_NO_OBSTACLES
             * MODIFIER_DISAPPEARING_ARROWS
             * MODIFIER_STRICT_ANGLES
             * MODIFIER_FASTER_SONG
             * MODIFIER_FOUR_LIVES
             * MODIFIER_NO_ARROWS
             * MODIFIER_SMALL_CUBES
             * MODIFIER_ZEN_MODE
             * MODIFIER_SUPER_FAST_SONG
             */
        }
    }


    
    /*[HarmonyPatch(typeof(GameplayModifierToggle), nameof(GameplayModifierToggle.Start))]
    public class ModifierNamesPatch {
        static void Postfix(ref TextMeshProUGUI ____nameText, ref GameplayModifierParamsSO ____gameplayModifier) {
            switch (____gameplayModifier.modifierNameLocalizationKey) {
                case "MODIFIER_FOUR_LIVES":          ____nameText.text = "1 Health"; break;
                case "MODIFIER_DISAPPEARING_ARROWS": ____nameText.text = "Full Size Bombs"; break;
            }
        }
    }*/
}
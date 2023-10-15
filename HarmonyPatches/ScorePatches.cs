﻿using System;
using HarmonyLib;
using IPA.Utilities;
using TMPro;

namespace BeatSaber5.HarmonyPatches {
    [HarmonyPatch(typeof(ScoreController), nameof(ScoreController.DespawnScoringElement))]
    static class AccScorePatch {
        public static int TotalCutScore;
        public static int TotalNotes;

        static void Postfix(ScoringElement scoringElement) {
            if (scoringElement.noteData.gameplayType == NoteData.GameplayType.Bomb) return;

            TotalCutScore += scoringElement.cutScore;
            TotalNotes++;
        }
    }



    [HarmonyPatch(typeof(ScoreController), nameof(ScoreController.Start))]
    static class ScoreControllerStartPatch {
        public static ScoreController Controller = null;
        static void Postfix(ScoreController __instance) {
            Controller = __instance;

            AccScorePatch.TotalCutScore = 0;
            AccScorePatch.TotalNotes = 0;
        }
    }



    [HarmonyPatch(typeof(RelativeScoreAndImmediateRankCounter), "get_relativeScore")]
    static class ScoreDisplayPatch {
        static bool Prefix(ref float __result) {
            float relativeScore = AccScorePatch.TotalCutScore / (AccScorePatch.TotalNotes * 75f);
            __result = AccScorePatch.TotalNotes == 0 ? 1 : relativeScore;
            return false;
        }
    }



    [HarmonyPatch(typeof(ScoreUIController), nameof(ScoreUIController.UpdateScore))]
    static class PtsScoreDisplayPatch {
        private static PropertyAccessor<ScoreController, int>.Getter ScoreGetter =
            PropertyAccessor<ScoreController, int>.GetGetter("modifiedScore");

        private static PropertyAccessor<ScoreController, int>.Getter MaxScoreGetter =
            PropertyAccessor<ScoreController, int>.GetGetter("immediateMaxPossibleModifiedScore");

        static void Postfix(ref TextMeshProUGUI ____scoreText) {
            if (ScoreControllerStartPatch.Controller == null) return;

            double acc = ((double)AccScorePatch.TotalCutScore / ((double)AccScorePatch.TotalNotes*75d))*100d;
            int noteCount = TotalNotesPatch.CuttableNotesCount + 1; // +1 ???? why is it one less
            int misses = EnergyPatch.TotalMisses;
            int maxCombo = EnergyPatch.HighestCombo;

            double missCountCurve = noteCount / (50 * Math.Pow(misses, 2) + noteCount) * ((50d * noteCount + 1) / (50d * noteCount)) - 1 / (50d * noteCount);
            double maxComboCurve = Math.Pow(noteCount / ((1 - Math.Sqrt(0.5)) * maxCombo - noteCount), 2) - 1;
            const double j = 1d / 1020734678369717893d;
            double accCurve = (6.7 * Math.Pow(acc, 0.25) + j * Math.Pow(acc, 9.8) + Math.Pow(acc, 0.8)) * 0.01;

            int score = AccScorePatch.TotalNotes == 0 ? 0 : (int)(1_000_000d * ((missCountCurve * 0.3) + (maxComboCurve * 0.3) + (accCurve * 0.4)) * ((double)AccScorePatch.TotalNotes / (double)noteCount));

            ____scoreText.text = !Config.Instance.ShowComboPercent ? 
                score.ToString("N0").Replace(",", " ") : 
                (score / (1_000_000d * ((double)AccScorePatch.TotalNotes / (double)noteCount))).ToString("P");
        }
    }

    [HarmonyPatch(typeof(BeatmapData), "get_cuttableNotesCount")]
    static class TotalNotesPatch {
        internal static int CuttableNotesCount;
        static void Postfix(int __result) {
            CuttableNotesCount = __result;
        }
    }



    [HarmonyPatch(typeof(RankModel), nameof(RankModel.GetRankForScore))]
    static class RankPatch {
        static bool Prefix(ref RankModel.Rank __result) {
            float relativeScore = AccScorePatch.TotalCutScore / (AccScorePatch.TotalNotes * 75f);

            if (relativeScore == 1f || AccScorePatch.TotalNotes == 0) __result = RankModel.Rank.SSS;
            if (relativeScore > 0.9) { __result = RankModel.Rank.SS; return false; }
            if (relativeScore > 0.8) { __result = RankModel.Rank.S; return false; }
            if (relativeScore > 0.65) { __result = RankModel.Rank.A; return false; }
            if (relativeScore > 0.5) { __result = RankModel.Rank.B; return false; }
            if (relativeScore > 0.35) { __result = RankModel.Rank.C; return false; }
            if (relativeScore > 0.2) { __result = RankModel.Rank.D; return false; }
            __result = RankModel.Rank.E; return false;
        }
    }
}

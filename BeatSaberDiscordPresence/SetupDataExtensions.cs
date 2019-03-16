using System;
using System.Collections.Generic;
using System.Linq;

namespace BeatSaberDiscordPresence
{
    static class SetupDataExtensions
    {
        private static readonly List<Func<GameplayCoreSceneSetupData, string>> mapping = new List<Func<GameplayCoreSceneSetupData, string>>
        {
            s => s.difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.hintText,
            s => s.practiceSettings != null ? "Practice" : "",
            s => s.difficultyBeatmap.level.levelID.Contains('∎') ? "Custom" : "",
            s => s.gameplayModifiers.batteryEnergy ? "Battery" : "",
            s => s.gameplayModifiers.disappearingArrows ? "Disappearing Arrows" : "",
            s => s.gameplayModifiers.ghostNotes ? "Ghost Notes" : "",
            s => s.gameplayModifiers.instaFail ? "InstaFail" : "",
            s => s.gameplayModifiers.songSpeed == GameplayModifiers.SongSpeed.Faster ? "Faster" : "",
            s => s.gameplayModifiers.songSpeed == GameplayModifiers.SongSpeed.Faster ? "Slower" : "",
            s => s.gameplayModifiers.noObstacles ? "No Walls" : "",
            s => s.gameplayModifiers.noBombs ? "No Bombs" : "",
            s => s.gameplayModifiers.noFail ? "No Fail" : ""
        };

        internal static string GetModifiers(this GameplayCoreSceneSetupData setup)
        {
            return string.Join(" | ", mapping.Select(fn => fn(setup)).Where(s => s.Length > 0));
        }

        internal static string GetGameplayModeImage(this GameplayCoreSceneSetupData setup)
        {
            switch (setup.difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.hintText)
            {
                case "No Arrows":
                    return "no_arrows";
                case "One Saber":
                    return "one_saber";
                default:
                    return "solo";
            }
        }
    }
}

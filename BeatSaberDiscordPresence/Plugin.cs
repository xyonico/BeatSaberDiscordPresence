using IllusionPlugin;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BeatSaberDiscordPresence
{
    public class Plugin : IPlugin
    {
        private static readonly FieldInfo _gameplayCoreSceneSetupDataField = typeof(SceneSetup<GameplayCoreSceneSetupData>).GetField("_sceneSetupData", BindingFlags.NonPublic | BindingFlags.Instance);
        private const string DiscordAppID = "445053620698742804";
        private bool _pluginInit;
        private bool _discordInit;

        public string Name => "Discord Presence";
        public string Version => "v2.1.0";

        #region IPlugin Lifecycle

        public void OnApplicationStart()
        {
            if (_pluginInit)
            {
                return;
            }

            _pluginInit = true;

            try
            {
                var handlers = new DiscordRpc.EventHandlers();
                Log("Connecting to Discord...");
                DiscordRpc.Initialize(DiscordAppID, ref handlers, false, string.Empty);
                _discordInit = true;
            }
            catch (Exception ex)
            {
                Log("Plugin setup failed: {0}", ex);
                return;
            }

            SceneManager.activeSceneChanged += SceneManagerOnActiveSceneChanged;
        }

        public void OnApplicationQuit()
        {
            SceneManager.activeSceneChanged -= SceneManagerOnActiveSceneChanged;

            if (_discordInit)
            {
                DiscordRpc.Shutdown();
            }
        }


        public void OnLevelWasLoaded(int level)
        {
        }

        public void OnLevelWasInitialized(int level)
        {
        }

        public void OnUpdate()
        {
            if (_discordInit)
            {
                DiscordRpc.RunCallbacks();
            }
        }

        public void OnFixedUpdate()
        {
        }

        #endregion

        #region Scene tracking

        private void SceneManagerOnActiveSceneChanged(Scene oldScene, Scene newScene)
        {
            if (newScene.name == "MenuCore")
            {
                var presence = new DiscordRpc.RichPresence
                {
                    state = string.Empty,
                    details = "In Menu",
                    startTimestamp = default(long),
                    largeImageKey = "default",
                    largeImageText = "Beat Saber"
                };

                Publish(presence);
            }

            if (newScene.name == "GameCore")
            {
                var sceneManager = Resources.FindObjectsOfTypeAll<GameScenesManager>().FirstOrDefault();

                if (sceneManager != null)
                {
                    sceneManager.transitionDidFinishEvent -= GameSceneWasLoaded;
                    sceneManager.transitionDidFinishEvent += GameSceneWasLoaded;
                }
            }
        }

        private void GameSceneWasLoaded()
        {
            var sceneManager = Resources.FindObjectsOfTypeAll<GameScenesManager>().FirstOrDefault();
            if (sceneManager != null)
            {
                sceneManager.transitionDidFinishEvent -= GameSceneWasLoaded;
            }

            var setup = Resources.FindObjectsOfTypeAll<GameplayCoreSceneSetup>().FirstOrDefault();
            var data = _gameplayCoreSceneSetupDataField.GetValue(setup) as GameplayCoreSceneSetupData;

            var diff = data.difficultyBeatmap;
            var level = diff.level;

            var presence = new DiscordRpc.RichPresence
            {
                state = data.GetModifiers(),
                details = $"{level.songName} | {diff.difficulty.Name()}",
                largeImageKey = "default",
                largeImageText = "Beat Saber",
                startTimestamp = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds,
                smallImageKey = data.GetGameplayModeImage(),
                smallImageText = data.difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.hintText
            };

            Publish(presence);
        }

        #endregion

        #region Utilities

        private void Log(string format, params object[] args)
        {
            Console.WriteLine("[DiscordPresence] " + format, args);
        }

        private void Publish(DiscordRpc.RichPresence presence)
        {
            if (_discordInit)
            {
                DiscordRpc.UpdatePresence(presence);
            }
        }

        #endregion
    }
}

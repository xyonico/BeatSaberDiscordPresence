using System;
using System.Linq;
using IllusionPlugin;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BeatSaberDiscordPresence
{
	public class Plugin : IPlugin
	{
		private const string MenuSceneName = "Menu";
		private const string GameSceneName = "StandardLevel";
		private const string DiscordAppID = "445053620698742804";
		public static readonly DiscordRpc.RichPresence Presence = new DiscordRpc.RichPresence();
		private MainGameSceneSetupData _mainSetupData;
		private bool _init;
		
		public string Name
		{
			get { return "Discord Presence"; }
		}

		public string Version
		{
			get { return "v2.0.2"; }
		}
		
		public void OnApplicationStart()
		{
			if (_init) return;
			_init = true;
			SceneManager.sceneLoaded += SceneManagerOnSceneLoaded;
			
			var handlers = new DiscordRpc.EventHandlers();
			DiscordRpc.Initialize(DiscordAppID, ref handlers, false, string.Empty);
		}

		public void OnApplicationQuit()
		{
			SceneManager.sceneLoaded -= SceneManagerOnSceneLoaded;
			DiscordRpc.Shutdown();
		}

		private void SceneManagerOnSceneLoaded(Scene newScene, LoadSceneMode mode)
		{
			if (newScene.name == MenuSceneName)
			{
				//Menu scene loaded
				Presence.details = "In Menu";
				Presence.state = string.Empty;
				Presence.startTimestamp = default(long);
				Presence.largeImageKey = "default";
				Presence.largeImageText = "Beat Saber";
				Presence.smallImageKey = "solo";
				Presence.smallImageText = "Solo Standard";
				DiscordRpc.UpdatePresence(Presence);
			}
			else if (newScene.name == GameSceneName)
			{
				_mainSetupData = Resources.FindObjectsOfTypeAll<MainGameSceneSetupData>().FirstOrDefault();
				if (_mainSetupData == null)
				{
					Console.WriteLine("Discord Presence: Error finding the scriptable objects required to update presence.");
					return;
				}
                //Main game scene loaded;

                var diff = _mainSetupData.difficultyLevel;
                var level = diff.level;

				Presence.details = $"{level.songName} | {diff.difficulty.Name()}";
				Presence.state = "";
				if (level.levelID.Contains('∎'))
				{
					Presence.state = "Custom | ";
				}

				var gameplayModeText = GetGameplayModeName(_mainSetupData.gameplayMode);
				Presence.state += gameplayModeText;
				if (_mainSetupData.gameplayOptions.noEnergy)
				{
					Presence.state += " [No Fail]";
				}

				if (_mainSetupData.gameplayOptions.mirror)
				{
					Presence.state += " [Mirrored]";
				}
				
				Presence.largeImageKey = "default";
				Presence.largeImageText = "Beat Saber";
				Presence.smallImageKey = GetGameplayModeImage(_mainSetupData.gameplayMode);
				Presence.smallImageText = gameplayModeText;
				Presence.startTimestamp = (long) DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
				DiscordRpc.UpdatePresence(Presence);
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
			DiscordRpc.RunCallbacks();
		}

		public void OnFixedUpdate()
		{
			
		}

		public static string GetGameplayModeName(GameplayMode gameplayMode)
		{
			switch (gameplayMode)
			{
				case GameplayMode.SoloStandard:
					return "Solo Standard";
				case GameplayMode.SoloOneSaber:
					return "One Saber";
				case GameplayMode.SoloNoArrows:
					return "No Arrows";
				case GameplayMode.PartyStandard:
					return "Party";
				default:
					return "Solo Standard";
			}
		}

		private static string GetGameplayModeImage(GameplayMode gameplayMode)
		{
			switch (gameplayMode)
			{
				case GameplayMode.SoloStandard:
					return "solo";
				case GameplayMode.SoloOneSaber:
					return "one_saber";
				case GameplayMode.SoloNoArrows:
					return "no_arrows";
				case GameplayMode.PartyStandard:
					return "party";
				default:
					return "solo";
			}
		}
    }
}

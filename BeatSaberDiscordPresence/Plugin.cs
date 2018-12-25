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
		private const string GameSceneName = "GameCore";
		private const string DiscordAppID = "445053620698742804";
		public static readonly DiscordRpc.RichPresence Presence = new DiscordRpc.RichPresence();
		private StandardLevelSceneSetupDataSO _mainSetupData;
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
			SceneManager.activeSceneChanged += SceneManagerOnActiveSceneChanged;
			
			var handlers = new DiscordRpc.EventHandlers();
			DiscordRpc.Initialize(DiscordAppID, ref handlers, false, string.Empty);
		}

		private void SceneManagerOnActiveSceneChanged(Scene oldScene, Scene newScene) {
			Console.WriteLine("Discord Presence : " + newScene.name);
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
				_mainSetupData = Resources.FindObjectsOfTypeAll<StandardLevelSceneSetupDataSO>().FirstOrDefault();
				if (_mainSetupData == null)
				{
					Console.WriteLine("Discord Presence: Error finding the scriptable objects required to update presence.");
					return;
				}
				//Main game scene loaded;

				var diff = _mainSetupData.difficultyBeatmap;
				var level = diff.level;

				Presence.details = $"{level.songName} | {diff.difficulty.Name()}";
				Presence.state = "";
				if (level.levelID.Contains('∎'))
				{
					Presence.state = "Custom ";
				}

				//var gameplayModeText = GetGameplayModeName(_mainSetupData.gameplayCoreSetupData);
				//Presence.state += gameplayModeText;
				if (_mainSetupData.gameplayCoreSetupData.gameplayModifiers.noFail)
				{
					Presence.state += " [No Fail]";
				}
				
				Presence.largeImageKey = "default";
				Presence.largeImageText = "Beat Saber";
				//Presence.smallImageKey = GetGameplayModeImage(_mainSetupData.gameplayMode);
				//Presence.smallImageText = gameplayModeText;
				Presence.startTimestamp = (long) DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
				DiscordRpc.UpdatePresence(Presence);
			}
		}

		public void OnApplicationQuit()
		{
			SceneManager.sceneLoaded -= SceneManagerOnSceneLoaded;
			DiscordRpc.Shutdown();
		}

		private void SceneManagerOnSceneLoaded(Scene newScene, LoadSceneMode mode)
		{
			if (newScene.name == "GameCore") {
				foreach (GameObject obj in newScene.GetRootGameObjects()) {
					PrintChildren(obj);
				}

				var scripts = newScene.GetRootGameObjects().First().transform.Find("S").GetComponents<MonoBehaviour>();
				foreach (var m in scripts) {
					Console.WriteLine(m.GetType());
				}
			}
		}

		void PrintChildren(GameObject obj) {
			if (obj.transform.childCount != 0) {
				foreach (Transform child in obj.transform) {
					Console.WriteLine("Discord Presence : " + obj.gameObject.name + " | " + child.gameObject.name);
					PrintChildren(child.gameObject);
				}
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

		public static string GetGameplayModeName(SoloModeSelectionViewController.MenuType gameplayMode)
		{
			switch (gameplayMode)
			{
				case SoloModeSelectionViewController.MenuType.FreePlayMode:
					return "Solo Standard";
				case SoloModeSelectionViewController.MenuType.OneSaberMode:
					return "One Saber";
				case SoloModeSelectionViewController.MenuType.NoArrowsMode:
					return "No Arrows";
				default:
					return "Solo Standard";
			}
		}

		private static string GetGameplayModeImage(SoloModeSelectionViewController.MenuType gameplayMode)
		{
			switch (gameplayMode)
			{
				case SoloModeSelectionViewController.MenuType.FreePlayMode:
					return "solo";
				case SoloModeSelectionViewController.MenuType.OneSaberMode:
					return "one_saber";
				case SoloModeSelectionViewController.MenuType.NoArrowsMode:
					return "no_arrows";
				default:
					return "solo";
			}
		}
    }
}

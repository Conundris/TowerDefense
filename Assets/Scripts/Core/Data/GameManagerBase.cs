﻿using System;
using System.Collections.Generic;
using Core.Utilities;
using GoogleMobileAds.Api;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine;
using UnityEngine.Audio;

namespace Core.Data
{
	/// <summary>
	/// Base game manager
	/// </summary>
	public abstract class GameManagerBase<TGameManager, TDataStore> : PersistentSingleton<TGameManager>
		where TDataStore : GameDataStoreBase, new()
		where TGameManager : GameManagerBase<TGameManager, TDataStore>
	{
		/// <summary>
		/// File name of saved game
		/// </summary>
		const string k_SavedGameFile = "save";

		/// <summary>
		/// Reference to audio mixer for volume changing
		/// </summary>
		public AudioMixer gameMixer;

		/// <summary>
		/// Master volume parameter on the mixer
		/// </summary>
		public string masterVolumeParameter;

		/// <summary>
		/// SFX volume parameter on the mixer
		/// </summary>
		public string sfxVolumeParameter;

		/// <summary>
		/// Music volume parameter on the mixer
		/// </summary>
		public string musicVolumeParameter;

		/// <summary>
		/// The serialization implementation for persistence 
		/// </summary>
		protected JsonSaver<TDataStore> m_DataSaver;

		/// <summary>
		/// The object used for persistence
		/// </summary>
		protected TDataStore m_DataStore;

		// list of achievements we know we have unlocked (to avoid making repeated calls to the API)
		protected Dictionary<string,bool> mUnlockedAchievements = new Dictionary<string, bool>();

		// achievement increments we are accumulating locally, waiting to send to the games API
		protected Dictionary<string,int> mPendingIncrements = new Dictionary<string, int>();
		
		public bool mAuthenticating = false;
		
		// what is the highest score we have posted to the leaderboard?
		public int mHighestPostedScore = 0;
		
		public RewardBasedVideoAd rewardBasedVideo { get; set; }
		
		public bool Authenticated
		{
			get
			{
				return Social.Active.localUser.authenticated;
			}
		}
		
		/// <summary>
		/// Retrieve volumes from data store
		/// </summary>
		public virtual void GetVolumes(out float master, out float sfx, out float music)
		{
			master = m_DataStore.masterVolume;
			sfx = m_DataStore.sfxVolume;
			music = m_DataStore.musicVolume;
		}

		/// <summary>
		/// Set and persist game volumes
		/// </summary>
		public virtual void SetVolumes(float master, float sfx, float music, bool save)
		{
			// Early out if no mixer set
			if (gameMixer == null)
			{
				return;
			}
			
			// Transform 0-1 into logarithmic -80-0
			if (masterVolumeParameter != null)
			{
				gameMixer.SetFloat(masterVolumeParameter, LogarithmicDbTransform(Mathf.Clamp01(master)));
			}
			if (sfxVolumeParameter != null)
			{
				gameMixer.SetFloat(sfxVolumeParameter, LogarithmicDbTransform(Mathf.Clamp01(sfx)));
			}
			if (musicVolumeParameter != null)
			{
				gameMixer.SetFloat(musicVolumeParameter, LogarithmicDbTransform(Mathf.Clamp01(music)));
			}

			if (save)
			{
				// Apply to save data too
				m_DataStore.masterVolume = master;
				m_DataStore.sfxVolume = sfx;
				m_DataStore.musicVolume = music;
				SaveData();
			}
		}

		/// <summary>
		/// Load data
		/// </summary>
		protected override void Awake()
		{
			base.Awake();
			LoadData();
		}

		/// <summary>
		/// Initialize volumes. We cannot change mixer params on awake
		/// </summary>
		protected virtual void Start()
		{
			SetVolumes(m_DataStore.masterVolume, m_DataStore.sfxVolume, m_DataStore.musicVolume, false);
			//Authenticate();
		}
		
		public void Authenticate()
		{
			if (Authenticated || mAuthenticating)
			{
				Debug.LogWarning("Ignoring repeated call to Authenticate().");
				return;
			}

			// Enable/disable logs on the PlayGamesPlatform
			//PlayGamesPlatform.DebugLogEnabled = GameConsts.PlayGamesDebugLogsEnabled;
			PlayGamesPlatform.DebugLogEnabled = true;
			
			PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
				//.EnableSavedGames()
				.Build();
			PlayGamesPlatform.InitializeInstance(config);

			// Activate the Play Games platform. This will make it the default
			// implementation of Social.Active
			PlayGamesPlatform.Activate();

			// Set the default leaderboard for the leaderboards UI
			((PlayGamesPlatform)Social.Active).SetDefaultLeaderboardForUI(GPGSIds.leaderboard_level1);

			// Sign in to Google Play Games
			mAuthenticating = true;
			Social.localUser.Authenticate((bool success) =>
			{
				mAuthenticating = false;
				if (success)
				{
					// if we signed in successfully, load data from cloud
					Debug.Log("Login successful!");
				}
				else
				{
					// no need to show error message (error messages are shown automatically
					// by plugin)
					Debug.LogWarning("Failed to sign in with Google Play Games.");
				}
			});
		}
		
		/// <summary>
		/// Set up persistence
		/// </summary>
		protected void LoadData()
		{
			// If it is in Unity Editor use the standard JSON (human readable for debugging) otherwise encrypt it for deployed version
#if UNITY_EDITOR
			m_DataSaver = new JsonSaver<TDataStore>(k_SavedGameFile);
#else
			m_DataSaver = new EncryptedJsonSaver<TDataStore>(k_SavedGameFile);
#endif

			try
			{
				if (!m_DataSaver.Load(out m_DataStore))
				{
					m_DataStore = new TDataStore();
					SaveData();
				}
			}
			catch (Exception)
			{
				Debug.Log("Failed to load data, resetting");
				m_DataStore = new TDataStore();
				SaveData();
			}
		}

		/// <summary>
		/// Saves the gamme
		/// </summary>
		protected virtual void SaveData()
		{
			m_DataSaver.Save(m_DataStore);
		}

		/// <summary>
		/// Transform volume from linear to logarithmic
		/// </summary>
		protected static float LogarithmicDbTransform(float volume)
		{
			volume = (Mathf.Log(89 * volume + 1) / Mathf.Log(90)) * 80;
			return volume - 80;
		}
	}
}
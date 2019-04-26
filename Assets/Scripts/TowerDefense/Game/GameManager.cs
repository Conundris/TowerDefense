using System;
using Core.Data;
using Core.Game;
using GoogleMobileAds.Api;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TowerDefense.Game
{
	/// <summary>
	/// Game Manager - a persistent single that handles persistence, and level lists, etc.
	/// This should be initialized when the game starts.
	/// </summary>
	public class GameManager : GameManagerBase<GameManager, GameDataStore>
	{
		/// <summary>
		/// Scriptable object for list of levels
		/// </summary>
		public LevelList levelList;

		/// <summary>
		/// Set sleep timeout to never sleep
		/// </summary>
		protected override void Awake()
		{
			Screen.sleepTimeout = SleepTimeout.NeverSleep;
			base.Awake();
		}

		/// <summary>
		/// Method used for completing the level
		/// </summary>
		/// <param name="levelId">The levelId to mark as complete</param>
		/// <param name="starsEarned"></param>
		public void CompleteLevel(string levelId, int starsEarned)
		{
			if (!levelList.ContainsKey(levelId))
			{
				Debug.LogWarningFormat("[GAME] Cannot complete level with id = {0}. Not in level list", levelId);
				return;
			}

			m_DataStore.CompleteLevel(levelId, starsEarned);
			SaveData();
		}

		/// <summary>
		/// Gets the id for the current level
		/// </summary>
		public LevelItem GetLevelForCurrentScene()
		{
			string sceneName = SceneManager.GetActiveScene().name;

			return levelList.GetLevelByScene(sceneName);
		}

		/// <summary>
		/// Determines if a specific level is completed
		/// </summary>
		/// <param name="levelId">The level ID to check</param>
		/// <returns>true if the level is completed</returns>
		public bool IsLevelCompleted(string levelId)
		{
			if (!levelList.ContainsKey(levelId))
			{
				Debug.LogWarningFormat("[GAME] Cannot check if level with id = {0} is completed. Not in level list", levelId);
				return false;
			}

			return m_DataStore.IsLevelCompleted(levelId);
		}

		/// <summary>
		/// Gets the stars earned on a given level
		/// </summary>
		/// <param name="levelId"></param>
		/// <returns></returns>
		public int GetStarsForLevel(string levelId)
		{
			if (!levelList.ContainsKey(levelId))
			{
				Debug.LogWarningFormat("[GAME] Cannot check if level with id = {0} is completed. Not in level list", levelId);
				return 0;
			}

			return m_DataStore.GetNumberOfStarForLevel(levelId);
		}
		
        public void ShowLeaderboardUI()
        {
            if (Authenticated)
            {
                Social.ShowLeaderboardUI();
            }
        }
        
        public string PostToLeaderboard(int score)
        {
            if (Authenticated && score > mHighestPostedScore)
            {
                // post score to the leaderboard
                Social.ReportScore(score, GPGSIds.leaderboard_test, (bool success) =>
                    {
                    });
                mHighestPostedScore = score;
                return "success, Reported Score: " + score;
            }
            else
            {
                Debug.LogWarning("Not reporting score, auth = " + Authenticated + " " +
                    score + " <= " + mHighestPostedScore);
                return "Not reporting score, auth = " + Authenticated + " " +
                       score + " <= " + mHighestPostedScore;
            }
        }

        
        
        public void ShowAchievementsUI()
        {
	        if (Authenticated)
	        {
		        Social.ShowAchievementsUI();
	        }
        }

        // Returns an ad request with custom ad targeting.
        private AdRequest CreateAdRequest()
        {
	        return new AdRequest.Builder()
		        .AddTestDevice(AdRequest.TestDeviceSimulator)
		        .AddTestDevice("0123456789ABCDEF0123456789ABCDEF")
		        .AddKeyword("game")
		        .SetGender(Gender.Male)
		        .SetBirthday(new DateTime(1985, 1, 1))
		        .TagForChildDirectedTreatment(false)
		        .AddExtra("color_bg", "9B30FF")
		        .Build();
        }
        
        public void ShowBannerAd()
        {
#if UNITY_EDITOR
	        string adUnitId = "unused";
#elif UNITY_ANDROID
        string adUnitId = AdMobIds.mainMenuBanner_ID;
#endif
	        
	        // Clean up banner ad before creating a new one.
	        if (this.bannerView != null)
	        {
		        this.bannerView.Destroy();
	        }

	        // Create a 320x50 banner at the top of the screen.
	        this.bannerView = new BannerView(adUnitId, AdSize.SmartBanner, AdPosition.Bottom);

	        // Register for ad events.
	        /*this.bannerView.OnAdLoaded += this.HandleAdLoaded;
	        this.bannerView.OnAdFailedToLoad += this.HandleAdFailedToLoad;
	        this.bannerView.OnAdOpening += this.HandleAdOpened;
	        this.bannerView.OnAdClosed += this.HandleAdClosed;
	        this.bannerView.OnAdLeavingApplication += this.HandleAdLeftApplication;*/

	        // Load a banner ad.
	        this.bannerView.LoadAd(new AdRequest.Builder().Build());
        }
	}
}
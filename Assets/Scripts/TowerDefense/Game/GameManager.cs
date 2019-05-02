using System;
using Core.Data;
using Core.Game;
using GoogleMobileAds.Api;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using TowerDefense.Level;
using TowerDefense.UI.HUD;
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

		public BannerView bannerView;
		public InterstitialAd interstitial;
		public RewardBasedVideoAd rewardedAd;
		private float deltaTime = 0.0f;
		private static string outputMessage = string.Empty;
		
		public int enemiesKilled { get; set; }
		
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
			InitAdMob();
			RequestRewardedAd();
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
                Social.ReportScore(score, GPGSIds.leaderboard_level1, (bool success) =>
                    {
                    });
                mHighestPostedScore = score;
                return "success, : " + Social.localUser.userName + ", " + Social.localUser.authenticated;
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

        public void ReportAllProgress()
        {
	        FlushAchievements();
        }
        
        public void FlushAchievements()
        {
	        if (Authenticated)
	        {
		        foreach (string ach in mPendingIncrements.Keys)
		        {
			        // incrementing achievements by a delta is a feature
			        // that's specific to the Play Games API and not part of the
			        // ISocialPlatform spec, so we have to break the abstraction and
			        // use the PlayGamesPlatform rather than ISocialPlatform
			        PlayGamesPlatform p = (PlayGamesPlatform)Social.Active;
			        p.IncrementAchievement(ach, mPendingIncrements[ach], (bool success) =>
			        {
			        });
		        }
		        mPendingIncrements.Clear();
	        }
        }
        
        public void IncrementAchievement(string achId, int steps)
        {
	        if (mPendingIncrements.ContainsKey(achId))
	        {
		        steps += mPendingIncrements[achId];
	        }
	        mPendingIncrements[achId] = steps;
        }
        
        //Only for Test purposes
        public void UnlockAchievement(string achID)
        {
	        if (Authenticated && !mUnlockedAchievements.ContainsKey(achID))
	        {
		        Social.ReportProgress(achID, 100.0f, (bool success) =>
		        {
		        });
		        mUnlockedAchievements[achID] = true;
	        }
        }
        
        private void UnlockProgressBasedAchievements()
        {
	        if (enemiesKilled > 500)
	        {
		        UnlockAchievement(GPGSIds.achievement_rampage);
	        }
        }

        private void InitAdMob()
        {
#if UNITY_ANDROID
	        string appId = AdMobIds.app_ID;
#elif UNITY_IPHONE
                    string appId = "ca-app-pub-3940256099942544~1458002511";
#else
                    string appId = "unexpected_platform";
#endif
            
	        MobileAds.SetiOSAppPauseOnBackground(true);
            
	        // Initialize the Google Mobile Ads SDK.
	        MobileAds.Initialize(appId);
            
	        // Get singleton reward based video ad reference.
	        this.rewardBasedVideo = RewardBasedVideoAd.Instance;
                    
	        // RewardBasedVideoAd is a singleton, so handlers should only be registered once.
	        this.rewardBasedVideo.OnAdLoaded += this.HandleRewardBasedVideoLoaded;
	        this.rewardBasedVideo.OnAdFailedToLoad += this.HandleRewardBasedVideoFailedToLoad;
	        this.rewardBasedVideo.OnAdOpening += this.HandleRewardBasedVideoOpened;
	        this.rewardBasedVideo.OnAdStarted += this.HandleRewardBasedVideoStarted;
	        this.rewardBasedVideo.OnAdRewarded += this.HandleRewardBasedVideoRewarded;
	        this.rewardBasedVideo.OnAdClosed += this.HandleRewardBasedVideoClosed;
	        this.rewardBasedVideo.OnAdLeavingApplication += this.HandleRewardBasedVideoLeftApplication;
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
        
        
		public void RequestRewardedAd()
		{
#if UNITY_EDITOR
			string adUnitId = "unused";
#elif UNITY_ANDROID
        string adUnitId = "ca-app-pub-3940256099942544/5224354917";
#elif UNITY_IPHONE
        string adUnitId = "ca-app-pub-3940256099942544/1712485313";
#else
        string adUnitId = "unexpected_platform";
#endif
			Debug.LogWarning("Requesting Reward Ad");	
			
			rewardBasedVideo.LoadAd(new AdRequest.Builder().Build(), adUnitId);
		}
		
		public void ShowRewardBasedVideo()
		{
			if (rewardBasedVideo.IsLoaded())
			{
				rewardBasedVideo.Show();
			}
			else
			{
				print("Reward based video ad is not ready yet");
			}
		}
		
		
		#region RewardBasedVideo callback Handlers

		public void HandleRewardBasedVideoLoaded(object sender, EventArgs args)
		{
			print("HandleRewardBasedVideoLoaded event received");
		}

		public void HandleRewardBasedVideoFailedToLoad(object sender, AdFailedToLoadEventArgs args)
		{
			print(
				"HandleRewardBasedVideoFailedToLoad event received with message: " + args.Message);
		}

		public void HandleRewardBasedVideoOpened(object sender, EventArgs args)
		{
			print("HandleRewardBasedVideoOpened event received");
		}

		public void HandleRewardBasedVideoStarted(object sender, EventArgs args)
		{
			print("HandleRewardBasedVideoStarted event received");
			if (GameUI.instanceExists)
			{
				GameUI.instance.Pause();
			}
		}

		public void HandleRewardBasedVideoClosed(object sender, EventArgs args)
		{
			print("HandleRewardBasedVideoClosed event received");
			if (GameUI.instanceExists)
			{
				GameUI.instance.Unpause();
			}
			
			RequestRewardedAd();
		}

		public void HandleRewardBasedVideoRewarded(object sender, Reward args)
		{
			if (LevelManager.instanceExists)
			{
				LevelManager.instance.currency.AddCurrency(10);
			}

			if (GameUI.instanceExists)
			{
				GameUI.instance.Unpause();
			}
		}

		public void HandleRewardBasedVideoLeftApplication(object sender, EventArgs args)
		{
			print("HandleRewardBasedVideoLeftApplication event received");
		}

		#endregion
	}
}
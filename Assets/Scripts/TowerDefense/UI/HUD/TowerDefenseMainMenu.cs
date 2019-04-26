using System;
using Core.UI;
using GoogleMobileAds.Api;
using TowerDefense.Game;
using UnityEngine;
using UnityEngine.UI;

namespace TowerDefense.UI.HUD
{
	/// <summary>
	/// Main menu implementation for tower defense
	/// </summary>
	public class TowerDefenseMainMenu : MainMenu
	{
		/// <summary>
		/// Reference to options menu
		/// </summary>
		public OptionsMenu optionsMenu;
		
		/// <summary>
		/// Reference to title menu
		/// </summary>
		public SimpleMainMenuPage titleMenu;
		
		/// <summary>
		/// Reference to level select menu
		/// </summary>
		public LevelSelectScreen levelSelectMenu;

		public Text textBox;
		public Text debugBannerAdTextBox;
		public Text debugInterstitalAdTextBox;
		public Text rewardedAdTextBox;
		
		public BannerView bannerView;
		public InterstitialAd interstitial;
		
		
		/// <summary>
		/// Bring up the options menu
		/// </summary>
		public void ShowOptionsMenu()
		{
			ChangePage(optionsMenu);
		}
		
		/// <summary>
		/// Bring up the options menu
		/// </summary>
		public void ShowLevelSelectMenu()
		{
			ChangePage(levelSelectMenu);
		}
		
		/// <summary>
		/// Returns to the title screen
		/// </summary>
		public void ShowTitleScreen()
		{
			Back(titleMenu);
		}

		/// <summary>
		/// Set initial page
		/// </summary>
		protected virtual void Awake()
		{
			ShowTitleScreen();
			Debug.LogWarning("Calling Banner Stuff");
			DisplayBannerAd();
			RequestInterstitial();
		}

		public void ShowLeaderBoard()
		{
			GameManager.instance.ShowLeaderboardUI();
		}

		public void ShowAchievements()
		{
			GameManager.instance.ShowAchievementsUI();
		}

		/*private void InitAdMob()
		{
#if UNITY_ANDROID
			//string appId = AdMobIds.app_ID;
			string appId = "ca-app-pub-3940256099942544~3347511713";
#elif UNITY_IPHONE
                    string appId = "ca-app-pub-3940256099942544~1458002511";
#else
                    string appId = "unexpected_platform";
#endif
            
			// Initialize the Google Mobile Ads SDK.
			MobileAds.Initialize(appId);
            
			// Get singleton reward based video ad reference.
			//this.rewardBasedVideo = RewardBasedVideoAd.Instance;
                    
			// RewardBasedVideoAd is a singleton, so handlers should only be registered once.
			/*this.rewardBasedVideo.OnAdLoaded += this.HandleRewardBasedVideoLoaded;
			this.rewardBasedVideo.OnAdFailedToLoad += this.HandleRewardBasedVideoFailedToLoad;
			this.rewardBasedVideo.OnAdOpening += this.HandleRewardBasedVideoOpened;
			this.rewardBasedVideo.OnAdStarted += this.HandleRewardBasedVideoStarted;
			this.rewardBasedVideo.OnAdRewarded += this.HandleRewardBasedVideoRewarded;
			this.rewardBasedVideo.OnAdClosed += this.HandleRewardBasedVideoClosed;
			this.rewardBasedVideo.OnAdLeavingApplication += this.HandleRewardBasedVideoLeftApplication;
		}*/
		
		public void DisplayBannerAd()
		{
			//InitAdMob();
			
			//GameManager.instance.ShowBannerAd();
#if UNITY_EDITOR
			string adUnitId = "unused";
			
#elif UNITY_ANDROID
			//string adUnitId = AdMobIds.mainMenuBanner_ID;
			string adUnitId = "ca-app-pub-3940256099942544/6300978111";
#endif
	        
			// Clean up banner ad before creating a new one.
			if (this.bannerView != null)
			{
				this.bannerView.Destroy();
			}

			// Create a 320x50 banner at the top of the screen.
			this.bannerView = new BannerView(adUnitId, AdSize.SmartBanner, AdPosition.Bottom);

			// Register for ad events.
			this.bannerView.OnAdLoaded += this.HandleAdLoaded;
			this.bannerView.OnAdFailedToLoad += this.HandleAdFailedToLoad;
			this.bannerView.OnAdOpening += this.HandleAdOpened;
			this.bannerView.OnAdClosed += this.HandleAdClosed;
			this.bannerView.OnAdLeavingApplication += this.HandleAdLeftApplication;

			// Load a banner ad.
			this.bannerView.LoadAd(new AdRequest.Builder().Build());
		}
		
		private void RequestInterstitial()
		{
			// These ad units are configured to always serve test ads.
#if UNITY_EDITOR
			string adUnitId = "unused";
#elif UNITY_ANDROID
        string adUnitId = "ca-app-pub-3940256099942544/1033173712";
#elif UNITY_IPHONE
        string adUnitId = "ca-app-pub-3940256099942544/4411468910";
#else
        string adUnitId = "unexpected_platform";
#endif

			// Clean up interstitial ad before creating a new one.
			if (this.interstitial != null)
			{
				this.interstitial.Destroy();
			}

			// Create an interstitial.
			this.interstitial = new InterstitialAd(adUnitId);

			// Register for ad events.
			this.interstitial.OnAdLoaded += this.HandleInterstitialLoaded;
			this.interstitial.OnAdFailedToLoad += this.HandleInterstitialFailedToLoad;
			this.interstitial.OnAdOpening += this.HandleInterstitialOpened;
			this.interstitial.OnAdClosed += this.HandleInterstitialClosed;
			this.interstitial.OnAdLeavingApplication += this.HandleInterstitialLeftApplication;

			// Load an interstitial ad.
			this.interstitial.LoadAd(new AdRequest.Builder().Build());


		}
		
		public void ShowInterstitial()
		{
			if (this.interstitial.IsLoaded())
			{
				this.interstitial.Show();
			}
			else
			{
				MonoBehaviour.print("Interstitial is not ready yet");
				debugInterstitalAdTextBox.text = "Interstital is not ready yet.";
			}
		}
		
		/*private void RequestRewardBasedVideo()
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

			this.rewardBasedVideo.LoadAd(new AdRequest.Builder().Build(), adUnitId);
		}*/

		public void DisplayInterstitalAd()
		{
			GameManager.instance.ShowBannerAd();
		}
		
		public void DisplayRewardedAd()
		{
			GameManager.instance.ShowBannerAd();
		}
		
		/// <summary>
		/// Escape key input
		/// </summary>
		protected virtual void Update()
		{
			textBox.text = GameManager.instance.Authenticated ? "Authenticated BITCH" : "Dumbass. Feck off";
			
			if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
			{
				if ((SimpleMainMenuPage)m_CurrentPage == titleMenu)
				{
					Application.Quit();
				}
				else
				{
					Back();
				}
			}
		}
		
		    #region Banner callback handlers

    public void HandleAdLoaded(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleAdLoaded event received");
        debugBannerAdTextBox.text = "HandleAdLoaded event received";
    }

    public void HandleAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        MonoBehaviour.print("HandleFailedToReceiveAd event received with message: " + args.Message);
        debugBannerAdTextBox.text = "HandleFailedToReceiveAd: " + args.Message;
    }

    public void HandleAdOpened(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleAdOpened event received");
        debugBannerAdTextBox.text = "HandleAdOpened event received";
    }

    public void HandleAdClosed(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleAdClosed event received");
        debugBannerAdTextBox.text = "HandleAdClosed event received";
    }

    public void HandleAdLeftApplication(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleAdLeftApplication event received");
        debugBannerAdTextBox.text = "HandleAdLeftApplication event received";
    }

    #endregion

    #region Interstitial callback handlers

    public void HandleInterstitialLoaded(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleInterstitialLoaded event received");
        debugBannerAdTextBox.text = "HandleInterstitialLoaded event received";
    }

    public void HandleInterstitialFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        MonoBehaviour.print(
            "HandleInterstitialFailedToLoad event received with message: " + args.Message);
        debugBannerAdTextBox.text = "HandleInterstitialFailedToLoad: " + args.Message;
    }

    public void HandleInterstitialOpened(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleInterstitialOpened event received");
        debugBannerAdTextBox.text = "HandleInterstitialOpened event received";
    }

    public void HandleInterstitialClosed(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleInterstitialClosed event received");
        debugBannerAdTextBox.text = "HandleInterstitialClosed event received";
    }

    public void HandleInterstitialLeftApplication(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleInterstitialLeftApplication event received");
        debugBannerAdTextBox.text = "HandleInterstitialLeftApplication event received";
    }

    #endregion

    #region RewardBasedVideo callback handlers

    public void HandleRewardBasedVideoLoaded(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleRewardBasedVideoLoaded event received");
    }

    public void HandleRewardBasedVideoFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        MonoBehaviour.print(
            "HandleRewardBasedVideoFailedToLoad event received with message: " + args.Message);
    }

    public void HandleRewardBasedVideoOpened(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleRewardBasedVideoOpened event received");
    }

    public void HandleRewardBasedVideoStarted(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleRewardBasedVideoStarted event received");
    }

    public void HandleRewardBasedVideoClosed(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleRewardBasedVideoClosed event received");
    }

    public void HandleRewardBasedVideoRewarded(object sender, Reward args)
    {
        string type = args.Type;
        double amount = args.Amount;
        MonoBehaviour.print(
            "HandleRewardBasedVideoRewarded event received for " + amount.ToString() + " " + type);
    }

    public void HandleRewardBasedVideoLeftApplication(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleRewardBasedVideoLeftApplication event received");
    }

    #endregion
	}
}
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System;
using SgLib;

#if EASY_MOBILE
using EasyMobile;
#endif

public class UIManager : MonoBehaviour
{
    [Header("Object References")]
    public GameObject blackPanel;
    public GameObject header;
    public GameObject scoreArea;
    public GameObject title;
    public Text score;
    public Text bigScore;
    public Text bestScore;
    public Text coinText;
    public GameObject coinImg;
    public GameObject newBestScore;
    public GameObject mainButtons;
    public GameObject menuButtons;
    public GameObject dailyRewardBtn;
    public Text dailyRewardBtnText;
    public GameObject rewardUI;
    public GameObject settingsUI;
    public GameObject hardModeToggle;
    public GameObject easyModeToggle;
    public GameObject soundOnBtn;
    public GameObject soundOffBtn;
    public GameObject speedDownBtn;
    public GameObject speedUpBtn;

    [Header("Premium Features Buttons")]
    public GameObject watchRewardedAdBtn;
    public GameObject leaderboardBtn;
    public GameObject achievementBtn;
    public GameObject iapPurchaseBtn;
    public GameObject removeAdsBtn;
    public GameObject restorePurchaseBtn;

    [Header("In-App Purchase Store")]
    public GameObject storeUI;

    [Header("Sharing-Specific")]
    public GameObject shareUI;
    public Image sharedImage;

    GameManager gameManager;
    Animator scoreAnimator;
    Animator dailyRewardAnimator;
    bool isWatchAdsForCoinBtnActive;

    void OnEnable()
    {
        GameManager.GameStateChanged += GameManager_GameStateChanged;
        ScoreManager.ScoreUpdated += OnScoreUpdated;
    }

    void OnDisable()
    {
        GameManager.GameStateChanged -= GameManager_GameStateChanged;
        ScoreManager.ScoreUpdated -= OnScoreUpdated;
    }

    // Use this for initialization
    void Start()
    {
        gameManager = GameManager.Instance;
        scoreAnimator = score.GetComponent<Animator>();
        dailyRewardAnimator = dailyRewardBtn.GetComponent<Animator>();

        Reset();

        // Show appropriate UI elements
        if (!GameManager.IsRestart)
            ShowStartUI();  // First launch
    }

    // Update is called once per frame
    void Update()
    {
        score.text = ScoreManager.Instance.Score.ToString();
        bestScore.text = ScoreManager.Instance.HighScore.ToString();
        coinText.text = CoinManager.Instance.Coins.ToString();

        if (!DailyRewardController.Instance.disable && dailyRewardBtn.gameObject.activeSelf)
        {
            if (DailyRewardController.Instance.CanRewardNow())
            {
                dailyRewardBtnText.text = "REWARD!";
                dailyRewardAnimator.SetTrigger("activate");
            }
            else
            {
                TimeSpan timeToReward = DailyRewardController.Instance.TimeUntilReward;
                dailyRewardBtnText.text = string.Format("{0:00}:{1:00}:{2:00}", timeToReward.Hours, timeToReward.Minutes, timeToReward.Seconds);
                dailyRewardAnimator.SetTrigger("deactivate");
            }
        }

        if (settingsUI.activeSelf)
        {
            UpdateMuteButtons();
        }
    }

    void GameManager_GameStateChanged(GameState newState, GameState oldState)
    {
        if (newState == GameState.Playing)
        {              
            ShowGameUI();
        }
        else if (newState == GameState.PreGameOver)
        {
            // Before game over, i.e. game potentially will be recovered
        }
        else if (newState == GameState.GameOver)
        {
            Invoke("ShowGameOverUI", 1f);
        } 
    }

    void OnScoreUpdated(int newScore)
    {
        scoreAnimator.Play("NewScore");
    }

    void Reset()
    {
        blackPanel.SetActive(false);
        header.SetActive(false);
        title.SetActive(false);
        bigScore.gameObject.SetActive(false);
        newBestScore.SetActive(false);  
        mainButtons.SetActive(false);
        menuButtons.SetActive(false);
        dailyRewardBtn.SetActive(false);
        speedDownBtn.SetActive(false);
        speedUpBtn.SetActive(false);

        // Enable or disable premium stuff
        bool enablePremium = IsPremiumFeaturesEnabled();
        leaderboardBtn.SetActive(enablePremium);
        achievementBtn.SetActive(enablePremium);
        iapPurchaseBtn.SetActive(enablePremium);
        removeAdsBtn.SetActive(enablePremium);
        restorePurchaseBtn.SetActive(enablePremium);

        // Show game mode
        UpdateGameModeToggle();

        // Hidden by default
        storeUI.SetActive(false);
        settingsUI.SetActive(false);
        shareUI.SetActive(false);

        // These premium feature buttons are hidden by default
        // and shown when certain criteria are met (e.g. rewarded ad is loaded)
        watchRewardedAdBtn.gameObject.SetActive(false);
    }

    bool IsPremiumFeaturesEnabled()
    {
        return PremiumFeaturesManager.Instance != null && PremiumFeaturesManager.Instance.enablePremiumFeatures;  
    }

    public void HandlePlayButton()
    {
        if (gameManager.GameState == GameState.Prepare)
            gameManager.StartGame();
        else if (gameManager.GameState == GameState.GameOver)
            gameManager.RestartGame(0.5f);
    }

    public void RestartGame()
    {
        GameManager.Instance.RestartGame(0.2f);
    }

    public void ShowStartUI()
    {
        settingsUI.SetActive(false); 

        blackPanel.SetActive(true);
        header.SetActive(true);
        scoreArea.SetActive(false);
        title.SetActive(true);
        mainButtons.SetActive(true);
        menuButtons.SetActive(true);

        ShowDailyRewardBtn();

        if (IsPremiumFeaturesEnabled())
        {
            ShowWatchAdButton();
        }
    }

    public void ShowGameUI()
    {
        title.SetActive(false);
        dailyRewardBtn.SetActive(false);
        watchRewardedAdBtn.SetActive(false);
        mainButtons.SetActive(false);
        menuButtons.SetActive(false);

        blackPanel.SetActive(false);
        header.SetActive(true);
        scoreArea.SetActive(true);
        speedDownBtn.SetActive(true);
        speedUpBtn.SetActive(true);
    }

    public void ShowGameOverUI()
    {
        blackPanel.SetActive(true);
        speedDownBtn.SetActive(false);
        speedUpBtn.SetActive(false);
        scoreArea.SetActive(false);

        mainButtons.SetActive(true);    
        menuButtons.SetActive(true);

        bigScore.text = ScoreManager.Instance.Score.ToString();
        bigScore.gameObject.SetActive(true);

        if (ScoreManager.Instance.HasNewHighScore)
            newBestScore.SetActive(true);
                   
        ShowDailyRewardBtn();

        // Show premium-feature buttons
        if (IsPremiumFeaturesEnabled())
        {
            ShowWatchAdButton();
            ShowShareUI();
        }
    }

    public void ToggleGameMode()
    {
        if (gameManager.GameMode == GameMode.Easy)
            gameManager.GameMode = GameMode.Hard;
        else if (gameManager.GameMode == GameMode.Hard)
            gameManager.GameMode = GameMode.Easy;

        UpdateGameModeToggle();
    }

    void UpdateGameModeToggle()
    {
        easyModeToggle.SetActive(gameManager.GameMode == GameMode.Easy);
        hardModeToggle.SetActive(gameManager.GameMode != GameMode.Easy);    
    }

    void ShowWatchAdButton()
    {
        // Only show "watch ad button" if a rewarded ad is loaded and premium features are enabled.
        // In the editor, it's always shown for testing purpose.
        #if UNITY_EDITOR
        watchRewardedAdBtn.SetActive(true);
        watchRewardedAdBtn.GetComponent<Animator>().SetTrigger("activate");
        #elif EASY_MOBILE
        if (IsPremiumFeaturesEnabled() && AdDisplayer.Instance.CanShowRewardedAd() && AdDisplayer.Instance.watchAdToEarnCoins)
        {
            watchRewardedAdBtn.SetActive(true);
            watchRewardedAdBtn.GetComponent<Animator>().SetTrigger("activate");
        }
        else
        {
            watchRewardedAdBtn.SetActive(false);
        }
        #endif
    }

    void ShowDailyRewardBtn()
    {
        // Not showing the daily reward button if the feature is disabled
        if (!DailyRewardController.Instance.disable)
        {
            dailyRewardBtn.SetActive(true);
        }
    }

    public void ShowSettingsUI()
    {
        settingsUI.SetActive(true);
    }

    public void HideSettingsUI()
    {
        settingsUI.SetActive(false);
    }

    public void ShowStoreUI()
    {
        storeUI.SetActive(true);
    }

    public void HideStoreUI()
    {
        storeUI.SetActive(false);
    }

    public void WatchRewardedAd()
    {
        #if EASY_MOBILE && UNITY_EDITOR
        Debug.Log("Watch ad is not enabled in the editor. Please test on a real device.");
        #elif EASY_MOBILE
        // Hide the button
        watchRewardedAdBtn.SetActive(false);

        AdDisplayer.CompleteRewardedAdToEarnCoins += OnCompleteRewardedAdToEarnCoins;
        AdDisplayer.Instance.ShowRewardedAdToEarnCoins();
        #else
        Debug.Log("Watch ad is not available.");
        #endif
    }

    void OnCompleteRewardedAdToEarnCoins()
    {
        #if EASY_MOBILE
        // Unsubscribe
        AdDisplayer.CompleteRewardedAdToEarnCoins -= OnCompleteRewardedAdToEarnCoins;

        // Give the coins!
        ShowRewardUI(AdDisplayer.Instance.rewardedCoins);
        #endif
    }

    public void GrabDailyReward()
    {
        if (DailyRewardController.Instance.CanRewardNow())
        {
            dailyRewardBtn.SetActive(false);

            float value = UnityEngine.Random.value;
            int reward = DailyRewardController.Instance.GetRandomRewardCoins();

            // Round the number and make it mutiplies of 5 only.
            int roundedReward = (reward / 5) * 5;
            // Show the reward UI
            ShowRewardUI(roundedReward);

            // Update next time for the reward
            DailyRewardController.Instance.ResetNextRewardTime();
        }
    }

    public void ShowRewardUI(int reward)
    {
        rewardUI.SetActive(true);
        rewardUI.GetComponent<RewardUIController>().Reward(reward);

        RewardUIController.RewardUIClosed += OnRewardUIClosed;
    }

    void OnRewardUIClosed()
    {
        RewardUIController.RewardUIClosed -= OnRewardUIClosed;
        dailyRewardBtn.SetActive(true);
    }

    public void HideRewardUI()
    {
        rewardUI.GetComponent<RewardUIController>().Close();
    }

    public void ShowLeaderboardUI()
    {
        #if EASY_MOBILE
        if (GameServiceManager.IsInitialized())
        {
            GameServiceManager.ShowLeaderboardUI();
        }
        else
        {
        #if UNITY_IOS
            MobileNativeUI.Alert("Service Unavailable", "The user is not logged in to Game Center.");
        #elif UNITY_ANDROID
            GameServiceManager.Init();
        #endif
        }
        #endif
    }

    public void ShowAchievementsUI()
    {
        #if EASY_MOBILE
        if (GameServiceManager.IsInitialized())
        {
            GameServiceManager.ShowAchievementsUI();
        }
        else
        {
        #if UNITY_IOS
            MobileNativeUI.Alert("Service Unavailable", "The user is not logged in to Game Center.");
        #elif UNITY_ANDROID
            GameServiceManager.Init();
        #endif
        }
        #endif
    }

    public void PurchaseRemoveAds()
    {
        #if EASY_MOBILE
        InAppPurchaser.Instance.Purchase(InAppPurchaser.Instance.removeAds);
        #endif
    }

    public void RestorePurchase()
    {
        #if EASY_MOBILE
        InAppPurchaser.Instance.RestorePurchase();
        #endif
    }

    public void ShowShareUI()
    {
        if (ScreenshotSharer.Instance != null)
        {
            Texture2D texture = ScreenshotSharer.Instance.GetScreenshotTexture();

            if (texture != null)
            {
                Sprite sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                Transform imgTf = sharedImage.transform;
                Image imgComp = imgTf.GetComponent<Image>();
                float scaleFactor = imgTf.GetComponent<RectTransform>().rect.width / sprite.rect.width;
                imgComp.sprite = sprite;
                imgComp.SetNativeSize();
                imgTf.localScale = imgTf.localScale * scaleFactor;
                imgComp.color = Color.white;

                shareUI.SetActive(true);
            }
        }
    }

    public void HideShareUI()
    {
        shareUI.SetActive(false);
    }

    public void ShareScreenshot()
    {
        #if EASY_MOBILE
        ScreenshotSharer.Instance.ShareScreenshot();
        #endif
    }

    public void ShowCharacterSelectionScene()
    {
        GameManager.Instance.LoadSelectCarScene();
    }

    public void ToggleSound()
    {
        SoundManager.Instance.ToggleMute();
    }

    public void ToggleMusic()
    {
        SoundManager.Instance.ToggleMusic();
    }

    public void RateApp()
    {
        Utilities.RateApp();
    }

    public void OpenTwitterPage()
    {
        Utilities.OpenTwitterPage();
    }

    public void OpenFacebookPage()
    {
        Utilities.OpenFacebookPage();
    }

    public void ButtonClickSound()
    {
        Utilities.ButtonClickSound();
    }

    void UpdateMuteButtons()
    {
        if (SoundManager.Instance.IsMuted())
        {
            soundOnBtn.gameObject.SetActive(false);
            soundOffBtn.gameObject.SetActive(true);
        }
        else
        {
            soundOnBtn.gameObject.SetActive(true);
            soundOffBtn.gameObject.SetActive(false);
        }
    }
}

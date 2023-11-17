using UnityEngine;
using System.Collections;

#if EASY_MOBILE
using EasyMobile;
#endif

namespace SgLib
{
    public class AdDisplayer : MonoBehaviour
    {
        #if EASY_MOBILE
        public static AdDisplayer Instance { get; private set; }

        public static event System.Action CompleteRewardedAdToRecoverLostGame;
        public static event System.Action CompleteRewardedAdToEarnCoins;

        [Header("Banner Ad Display Config")]
        [Tooltip("Whether or not to show banner ad")]
        public bool showBannerAd = true;
        public BannerAdPosition bannerAdPosition = BannerAdPosition.Bottom;

        [Header("Interstitial Ad Display Config")]
        [Tooltip("Whether or not to show interstitial ad")]
        public bool showInterstitialAd = true;
        [Tooltip("Show interstitial ad every [how many] games")]
        public int gamesPerInterstitial = 3;
        [Tooltip("How many seconds after game over that interstitial ad is shown")]
        public float showInterstitialDelay = 2f;

        [Header("Rewarded Ad Display Config")]
        [Tooltip("Check to allow watching ad to earn coins")]
        public bool watchAdToEarnCoins = true;
        [Tooltip("How many coins the user earns after watching a rewarded ad")]
        public int rewardedCoins = 50;

        private static int gameCount = 0;

        void OnEnable()
        {
            GameManager.GameStateChanged += OnGameStateChanged;
        }

        void OnDisable()
        {
            GameManager.GameStateChanged -= OnGameStateChanged;
        }

        void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        void Start()
        {
            // Show banner ad
            if (showBannerAd && !AdManager.IsAdRemoved() && !AdManager.IsShowingBannerAd())
            {
                AdManager.ShowBannerAd(bannerAdPosition);
            }
        }

        void OnGameStateChanged(GameState newState, GameState oldState)
        {       
            if (newState == GameState.GameOver)
            {
                // Show interstitial ad
                if (showInterstitialAd && !AdManager.IsAdRemoved())
                {
                    gameCount++;

                    if (gameCount >= gamesPerInterstitial)
                    {
                        if (AdManager.IsInterstitialAdReady())
                        {
                            // Show default ad after some optional delay
                            StartCoroutine(ShowInterstitial(showInterstitialDelay));

                            // Reset game count
                            gameCount = 0;
                        }
                    }
                }
            }
        }

        IEnumerator ShowInterstitial(float delay = 0f)
        {        
            if (delay > 0)
                yield return new WaitForSeconds(delay);

            AdManager.ShowInterstitialAd();
        }

        public bool CanShowRewardedAd()
        {
            return AdManager.IsRewardedAdReady();
        }

        public void ShowRewardedAdToRecoverLostGame()
        {
            if (CanShowRewardedAd())
            {
                AdManager.RewardedAdCompleted += OnCompleteRewardedAdToRecoverLostGame;
                AdManager.ShowRewardedAd();
            }
        }

        void OnCompleteRewardedAdToRecoverLostGame(RewardedAdNetwork adNetwork, AdLocation location)
        {
            // Unsubscribe
            AdManager.RewardedAdCompleted -= OnCompleteRewardedAdToRecoverLostGame;

            // Fire event
            if (CompleteRewardedAdToRecoverLostGame != null)
            {
                CompleteRewardedAdToRecoverLostGame();
            }
        }

        public void ShowRewardedAdToEarnCoins()
        {
            if (CanShowRewardedAd())
            {
                AdManager.RewardedAdCompleted += OnCompleteRewardedAdToEarnCoins;
                AdManager.ShowRewardedAd();
            }
        }

        void OnCompleteRewardedAdToEarnCoins(RewardedAdNetwork adNetwork, AdLocation location)
        {
            // Unsubscribe
            AdManager.RewardedAdCompleted -= OnCompleteRewardedAdToEarnCoins;

            // Fire event
            if (CompleteRewardedAdToEarnCoins != null)
            {
                CompleteRewardedAdToEarnCoins();
            }
        }
        #endif
    }
}

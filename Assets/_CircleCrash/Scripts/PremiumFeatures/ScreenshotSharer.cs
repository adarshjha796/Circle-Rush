using UnityEngine;
using System.Collections;
using System.IO;

#if EASY_MOBILE
using EasyMobile;
#endif

namespace SgLib
{
    public class ScreenshotSharer : MonoBehaviour
    {
        [Header("Sharing Config")]
        [Tooltip("Any instances of [score] will be replaced by the actual score achieved in the last game, [AppName] will be replaced by the app name declared in AppInfo")]
        [TextArea(3, 3)]
        public string shareMessage = "Awesome! I've just scored [score] in [AppName]! [#AppName]";
        public string screenshotFilename = "screenshot.png";

        public static ScreenshotSharer Instance { get; private set; }

        // On Android, we use a RenderTexture to take screenshot for better performance.
        #if UNITY_ANDROID && !UNITY_EDITOR
        RenderTexture screenshotRT;    
        Texture2D capturedScreenshot;
        #endif

        void OnEnable()
        {
            PlayerController.PlayerDied += PlayerController_PlayerDied;
        }

        void OnDisable()
        {
            PlayerController.PlayerDied -= PlayerController_PlayerDied;
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
            #if UNITY_ANDROID && !UNITY_EDITOR
            screenshotRT = new RenderTexture(Screen.width, Screen.height, 24);
            #endif
        }

        void PlayerController_PlayerDied()
        {
            if (PremiumFeaturesManager.Instance != null && PremiumFeaturesManager.Instance.enablePremiumFeatures)
            {
                StartCoroutine(CRCaptureScreenshot(.5f));
            }
        }

        IEnumerator CRCaptureScreenshot(float delay = 0f)
        {
            yield return new WaitForSeconds(delay);

            // Wait for right timing to take screenshot
            yield return new WaitForEndOfFrame();

            #if UNITY_EDITOR
            ScreenCapture.CaptureScreenshot(Path.Combine(Application.dataPath, screenshotFilename));
            #elif UNITY_ANDROID
            if (screenshotRT != null)
            {
                // Temporarily render the camera content to our screenshotRenderTexture.
                // Later we'll share the screenshot from this rendertexture.
                Camera.main.targetTexture = screenshotRT;
                Camera.main.Render();
                yield return null;
                Camera.main.targetTexture = null;
                yield return null;

                // Read the rendertexture contents
                RenderTexture.active = screenshotRT;

                capturedScreenshot = new Texture2D(screenshotRT.width, screenshotRT.height, TextureFormat.RGB24, false);
                capturedScreenshot.ReadPixels(new Rect(0, 0, screenshotRT.width, screenshotRT.height), 0, 0);
                capturedScreenshot.Apply();

                RenderTexture.active = null;
            }
            #else
            Application.CaptureScreenshot(screenshotFilename);
            #endif
        }

        public Texture2D GetScreenshotTexture()
        {
            #if UNITY_EDITOR
            string path = Path.Combine(Application.dataPath, screenshotFilename);
            return ImageToTexture2D(path);
            #elif UNITY_ANDROID
            return capturedScreenshot;
            #else
            string path = Path.Combine(Application.persistentDataPath, screenshotFilename);
            return ImageToTexture2D(path);
            #endif
        }

        public Texture2D ImageToTexture2D(string filePath)
        {
            Texture2D tex = new Texture2D(2, 2);

            if (File.Exists(filePath))
            {
                byte[] fileData = File.ReadAllBytes(filePath);
                tex.LoadImage(fileData);
            }

            return tex;
        }

        public void ShareScreenshot()
        {
            #if UNITY_EDITOR
            Debug.Log("Sharing is not available in editor.");
            #elif EASY_MOBILE

            string msg = shareMessage;
            msg = msg.Replace("[score]", ScoreManager.Instance.Score.ToString());
            msg = msg.Replace("[AppName]", AppInfo.Instance.APP_NAME);
            msg = msg.Replace("[#AppName]", "#" + AppInfo.Instance.APP_NAME.Replace(" ", ""));

            #if UNITY_ANDROID
            if (capturedScreenshot == null)
            {
                Debug.Log("ShareScreenshot: FAIL. No captured screenshot.");
                return;
            } 
            MobileNativeShare.ShareTexture2D(capturedScreenshot, screenshotFilename, msg);
            #else
            MobileNativeShare.ShareImage(Path.Combine(Application.persistentDataPath, screenshotFilename), msg);
            #endif

            #endif
        }
    }
}

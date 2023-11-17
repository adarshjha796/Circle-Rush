using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;
using SgLib;

[System.Serializable]
public struct Environment
{
    public GameObject environment;
    public Color groundColor;
}

public enum GameState
{
    Prepare,
    Playing,
    Paused,
    PreGameOver,
    GameOver
}

public enum DayNight
{
    Day,
    Night,
    Random
}

public enum GameMode
{
    Easy,
    Hard,
    CollectCoins
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public static event System.Action<GameState, GameState> GameStateChanged = delegate { };

    public GameState GameState
    {
        get
        {
            return _gameState;
        }
        private set
        {
            if (value != _gameState)
            {
                GameState oldState = _gameState;
                _gameState = value;

                GameStateChanged(_gameState, oldState);
            }
        }
    }

    private GameState _gameState = GameState.Prepare;

    public static int GameCount
    {
        get { return _gameCount; }
        private set { _gameCount = value; }
    }

    private static int _gameCount = 0;

    public GameMode GameMode
    {
        get
        {
            string mode = PlayerPrefs.GetString(GAME_MODE_PPKEY, EASY_MODE);
            switch (mode)
            {
                case EASY_MODE:
                    return GameMode.Easy;
                case HARD_MODE:
                    return GameMode.Hard;
                case COLLECT_COINS_MODE:
                    return GameMode.CollectCoins;
                default:
                    return GameMode.Easy;
            }
        }
        set
        {
            switch (value)
            {
                case GameMode.Easy:
                    hardTrack.SetActive(false);
                    easyTrack.SetActive(true);
                    PlayerPrefs.SetString(GAME_MODE_PPKEY, EASY_MODE);
                    break;
                case GameMode.Hard:
                    hardTrack.SetActive(true);
                    easyTrack.SetActive(false);
                    PlayerPrefs.SetString(GAME_MODE_PPKEY, HARD_MODE);
                    break;
                case GameMode.CollectCoins:
                    PlayerPrefs.SetString(GAME_MODE_PPKEY, COLLECT_COINS_MODE);
                    break;
            }
        }
    }

    public static bool IsRestart { get; private set; }

    [Header("Set the target frame rate for this game")]
    [Tooltip("Use 60 for games requiring smooth quick motion, set -1 to use platform default frame rate")]
    public int targetFrameRate = 30;

    // List of public variable for gameplay tweaking
    [Header("Gameplay Config")]
    public float firstWaitTime = 8f;
    public float minObstacleWaitTime = 5f;
    public float maxObstacleWaitTime = 10f;
    [Range(1, 100)]
    public int scoreToAddCar = 5;
    [HideInInspector]
    public float minCoinWaitTime = 3f;
    [HideInInspector]
    public float maxCoinWaitTime = 5f;
    [HideInInspector]
    public float minCoinSpeed = 4f;
    [HideInInspector]
    public float maxCoinSpeed = 7f;

    // List of public variables referencing other objects
    [Header("Object References")]
    public GameObject mainCamera;
    public PlayerController playerController;
    public GameObject easyTrack;
    public GameObject hardTrack;
    public Transform carsRotatePoint;
    public Transform playerRotatePoint;
    public GameObject plane;
    public GameObject coinPrefab;
    public GameObject bigCoinPrefab;
    public Transform carRandomPos;
    public Light directionalLight;
    public ParticleSystem burnParticle;
    public DayNight dayNight;
    public List<Environment> environments = new List<Environment>();
    public List<GameObject> opponentCars = new List<GameObject>();
    [HideInInspector]
    public List<GameObject> animals = new List<GameObject>();

    private Vector3 obstacleLeftPoint;
    //The create point of obstacles on left
    private Vector3 obstacleRightPoint;
    //The create point of obstacles on right

    private const string GAME_MODE_PPKEY = "SGLIB_GAME_MODE";
    private const string HARD_MODE = "SGLIB_GAME_MODE_HARD";
    private const string EASY_MODE = "SGLIB_GAME_MODE_EASY";
    private const string COLLECT_COINS_MODE = "SGLIB_GAME_MODE_COLLECT_COINS";

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
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (Instance == this)
        {
            if (dayNight == DayNight.Random)
                dayNight = (Random.value <= 0.5f) ? DayNight.Day : DayNight.Night;

            directionalLight.intensity = dayNight == DayNight.Day ? 1 : 0.05f;
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    // Use this for initialization
    void Start()
    {
        // Initial setup
        Application.targetFrameRate = targetFrameRate;
        ScoreManager.Instance.Reset();        
        PrepareGame();
    }

    // Listens to the event when player dies and call GameOver
    void PlayerController_PlayerDied()
    {
        StartCoroutine(CRGameOver());
    }

    // Make initial setup and preparations before the game can be played
    public void PrepareGame()
    {
        GameState = GameState.Prepare;

        //Get evironment and ground color
        Environment en = environments[Random.Range(0, environments.Count)];
        Instantiate(en.environment, Vector3.zero, Quaternion.identity);
        plane.GetComponent<Renderer>().material.SetColor("_Color", en.groundColor);

        //Get obstacle create point
        Vector3 trackSize = hardTrack.GetComponent<Renderer>().bounds.size;
        obstacleLeftPoint = hardTrack.transform.position + Vector3.left * trackSize.x / 2;
        obstacleRightPoint = hardTrack.transform.position + Vector3.right * trackSize.x / 2;

        //Set up the track
        if (GameMode == GameMode.Easy)
        {
            easyTrack.SetActive(true);
            hardTrack.SetActive(false);
        }
        else
        {
            easyTrack.SetActive(false);
            hardTrack.SetActive(true);         
        }
            
        // Start game automatically if this is a restart.
        if (IsRestart)
        {
            IsRestart = false;
            StartGame();
        }

        if (SoundManager.Instance.MusicState != SoundManager.PlayingState.Playing)
            SoundManager.Instance.PlayMusic(SoundManager.Instance.backgroundMusic);
    }

    // A new game official starts
    public void StartGame()
    {
        GameState = GameState.Playing;

        SoundManager.Instance.StopMusic();

        SoundManager.Instance.PlayMusic(SoundManager.Instance.traffic);

        StartCoroutine(WaitAndCreateCar(2f));

        if (GameMode == GameMode.Hard)
            StartCoroutine(WaitAndCreateMiddleLaneObstacle());
        else if (GameMode == GameMode.CollectCoins)
            StartCoroutine(WaitAndCreateCoins());
    }

    // Called when the player died
    IEnumerator CRGameOver()
    {
        SoundManager.Instance.StopMusic();

        SoundManager.Instance.PlaySound(SoundManager.Instance.gameOver);
        GameState = GameState.GameOver;
        GameCount++;

        yield return new WaitForSeconds(1.5f);

        SoundManager.Instance.PlayMusic(SoundManager.Instance.backgroundMusic);
    }

    // Start a new game
    public void RestartGame(float delay = 0)
    {
        StartCoroutine(CRRestartGame(delay));
    }

    IEnumerator CRRestartGame(float delay = 0)
    {
        yield return new WaitForSeconds(delay);
        IsRestart = true;
        SceneManager.LoadScene("Main");
    }

    public void LoadSelectCarScene()
    {
        StartCoroutine(CRLoadSelectCarScene());
    }

    IEnumerator CRLoadSelectCarScene()
    {
        yield return SceneManager.LoadSceneAsync("CharacterSelection", LoadSceneMode.Additive);
        directionalLight.enabled = false;
        mainCamera.SetActive(false);
    }

    public void UnloadSelectCarScene()
    {
        StartCoroutine(CRUnloadSelectCarScene());
    }

    IEnumerator CRUnloadSelectCarScene()
    {       
        
        yield return SceneManager.UnloadSceneAsync("CharacterSelection");
        mainCamera.SetActive(true);
        directionalLight.enabled = false;
    }

    void CreateCirclingCar()
    {
        //Create car
        Transform pos = carRandomPos.GetChild(Random.Range(0, carRandomPos.childCount)).transform;
        GameObject temp = opponentCars[Random.Range(0, opponentCars.Count)];
        GameObject car = (GameObject)Instantiate(temp, pos.position, Quaternion.identity);
        ObstacleController carControl = car.GetComponent<ObstacleController>();
        carControl.moveType = ObstacleController.MovingType.Circle;

        //Random moving direction of the car
        if (Random.value >= 0.5f)
        {
            car.transform.eulerAngles = new Vector3(pos.eulerAngles.x, 
                pos.eulerAngles.y, pos.eulerAngles.z);

            carControl.moveDir = Vector3.up;
        }
        else
        {
            car.transform.eulerAngles = new Vector3(pos.eulerAngles.x,
                pos.eulerAngles.y + 180, pos.eulerAngles.z);
            carControl.moveDir = Vector3.down;
        }  

        carControl.Run();
    }


    IEnumerator WaitAndCreateMiddleLaneObstacle()
    {
        yield return new WaitForSeconds(firstWaitTime);
        while (GameState != GameState.GameOver)
        {
            if (Random.value <= 0.5f) //Create obstacle on left
            {
                CreateMiddleLaneObstacle(Vector3.right);
            }
            else //Create obstacle on right
            {
                CreateMiddleLaneObstacle(Vector3.left);
            }
            yield return new WaitForSeconds(Random.Range(minObstacleWaitTime, maxObstacleWaitTime));
        }       
    }

    void CreateMiddleLaneObstacle(Vector3 dir)
    {
        GameObject obstacle;
        ObstacleController obController;

        if (Random.value < 0.5f && animals != null && animals.Count > 0)
        {
            // Create animals
            obstacle = (GameObject)Instantiate(
                animals[Random.Range(0, animals.Count)],
                obstacleLeftPoint, Quaternion.identity
            );
            obController = obstacle.GetComponent<ObstacleController>();
        }
        else
        {
            // Create cars
            obstacle = (GameObject)Instantiate(
                opponentCars[Random.Range(0, opponentCars.Count)],
                obstacleLeftPoint, Quaternion.identity
            );
            obController = obstacle.GetComponent<ObstacleController>();
        }

        obController.moveType = ObstacleController.MovingType.Straight;
        obController.moveDir = dir;
        obController.isOnLeft = dir == Vector3.right;

        obController.Run();
    }

    IEnumerator WaitAndCreateCoins()
    {
        Vector3 coinPos = obstacleRightPoint + new Vector3(0, 1.5f, 0);
        while (GameState != GameState.GameOver)
        {
            yield return new WaitForSeconds(Random.Range(minCoinWaitTime, maxCoinWaitTime));
            GameObject coin = (GameObject)Instantiate(bigCoinPrefab, coinPos, Quaternion.identity);
            CoinController coinControl = coin.GetComponent<CoinController>();
            coinControl.speed = Random.Range(minCoinSpeed, maxCoinSpeed);
            coinControl.MoveCoin();
        }
    }

    IEnumerator WaitAndCreateCar(float firstDelay = 0f)
    {
        yield return new WaitForSeconds(firstDelay);

        // Create first one
        CreateCirclingCar();

        int lastScore = 0;

        while (GameState != GameState.GameOver)
        {
            if (ScoreManager.Instance.Score > lastScore && ScoreManager.Instance.Score % scoreToAddCar == 0)
            {
                CreateCirclingCar();
                lastScore = ScoreManager.Instance.Score;
            }
                
            yield return new WaitForSeconds(0.1f);
        } 
    }

    public void ChangeMode(GameMode mode)
    {
        switch (mode)
        {
            case GameMode.Easy:
                hardTrack.SetActive(false);
                easyTrack.SetActive(true);
                PlayerPrefs.SetString(GAME_MODE_PPKEY, EASY_MODE);
                break;
            case GameMode.Hard:
                hardTrack.SetActive(true);
                easyTrack.SetActive(false);
                PlayerPrefs.SetString(GAME_MODE_PPKEY, HARD_MODE);
                break;
            case GameMode.CollectCoins:
                break;
        }
    }

    public void HandleHardMode()
    {
        hardTrack.SetActive(true);
        easyTrack.SetActive(false);
    }

    public void HandleEasyMode()
    {
        hardTrack.SetActive(false);
        easyTrack.SetActive(true);
    }

    public void HandleForShowForLuckySpin()
    {
        playerController.gameObject.SetActive(false);
        easyTrack.SetActive(false);
        hardTrack.SetActive(false);
        ObstacleController[] carsControl = FindObjectsOfType<ObstacleController>();
        foreach (ObstacleController o in carsControl)
        {
            o.gameObject.SetActive(false);
        }

        Camera.main.transform.eulerAngles = Vector3.zero;
    }

    public void HandleForHideLuckySpin()
    {
        playerController.gameObject.SetActive(true);
        easyTrack.SetActive(GameMode == GameMode.Easy);
        hardTrack.SetActive(GameMode == GameMode.Hard);
        ObstacleController[] carsControl = FindObjectsOfType<ObstacleController>();
        foreach (ObstacleController o in carsControl)
        {
            o.gameObject.SetActive(true);
        }

        Camera.main.transform.eulerAngles = new Vector3(45, 0, 0);
    }
}

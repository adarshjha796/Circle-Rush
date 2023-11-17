using UnityEngine;
using System.Collections;
using SgLib;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{

    public static event System.Action PlayerDied;

    [Header("Objects References")]
    public ParticleSystem smoke;
    public GameObject tailLight;
    public GameObject spotLight;
    [HideInInspector]
    public float speed;

    [Header("Gameplay Config")]
    public float normalSpeed;
    public float maxSpeed;
    public float minSpeed;

    private Rigidbody rigid;
    private MeshCollider meshCollider;
    private Vector3 moveDir;

    void OnEnable()
    {
        GameManager.GameStateChanged += OnGameStateChanged;
    }

    void OnDisable()
    {
        GameManager.GameStateChanged -= OnGameStateChanged;
    }

    void Start()
    {
        meshCollider = GetComponent<MeshCollider>();

        // Changing the character to the selected one
        GameObject currentCharacter = CharacterManager.Instance.characters[CharacterManager.Instance.CurrentCharacterIndex];
        Mesh charMesh = currentCharacter.GetComponent<MeshFilter>().sharedMesh;
        Material charMaterial = currentCharacter.GetComponent<Renderer>().sharedMaterial;

        GetComponent<MeshFilter>().mesh = charMesh;
        meshCollider.sharedMesh = charMesh;
        GetComponent<Renderer>().material = charMaterial;

        rigid = GetComponent<Rigidbody>();        
        speed = normalSpeed;
        smoke.Play();
        var en = smoke.emission;        
        en.enabled = false;
        tailLight.SetActive(false);
        spotLight.SetActive((GameManager.Instance.dayNight == DayNight.Day) ? (false) : (true));
        if (Random.value >= 0.5f)
            moveDir = Vector3.down;
        else
        {
            moveDir = Vector3.up;
            transform.eulerAngles = new Vector3(transform.eulerAngles.x,
                -transform.eulerAngles.y, transform.eulerAngles.z);
        }
    }
	
    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.GameState == GameState.Prepare)
        {
            // Run with normal speed
            transform.RotateAround(GameManager.Instance.playerRotatePoint.position, moveDir, normalSpeed * Time.deltaTime);   
        }
        else if (GameManager.Instance.GameState == GameState.Playing)
        {
            if (Input.GetMouseButton(0))
            {
                Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition);

                if (pos.x >= 0.5f)
                {
                    speed = maxSpeed;
                    var en = smoke.emission;
                    en.enabled = true;
                    //smoke.emission.enabled = false;
                    tailLight.SetActive(false);
                }
                else
                {
                    speed = minSpeed;
                    var en = smoke.emission;
                    en.enabled = false;
                    tailLight.SetActive(true);

                    // First touch to slow down.
                    if (Input.GetMouseButtonDown(0))
                        SoundManager.Instance.PlaySound(SoundManager.Instance.carBreak);
                }
            }



            if (Input.GetMouseButtonUp(0))
            {
                speed = normalSpeed;
                var en = smoke.emission;
                en.enabled = false;
                tailLight.SetActive(false);
            }

            transform.RotateAround(GameManager.Instance.playerRotatePoint.position, moveDir, speed * Time.deltaTime);   
        }
    }

    // Listens to changes in game state
    void OnGameStateChanged(GameState newState, GameState oldState)
    {
        if (newState == GameState.Playing)
        {
            // Do whatever necessary when a new game starts
        }      
    }

    // Calls this when the player dies and game over
    public void Die()
    {
        // Fire event
        PlayerDied();
    }

    void OnTriggerEnter(Collider other)
    {
        if (GameManager.Instance.GameState == GameState.Playing)
        {
            if (other.CompareTag("Car") || other.CompareTag("Obstacle"))
            {
                Camera.main.GetComponent<CameraController>().ShakeCamera();
                SoundManager.Instance.PlaySound(SoundManager.Instance.gameOver);

                if (GameManager.Instance.GameMode == GameMode.CollectCoins)
                    GameManager.Instance.GameMode = GameMode.Hard;

                tailLight.SetActive(false);
                smoke.Stop();
                meshCollider.isTrigger = false;

                if (other.CompareTag("Car"))
                {
                    rigid.isKinematic = false;
                }

                Die();
            }        
        }
    }

    void Burn()
    {
        ParticleSystem burn = (ParticleSystem)Instantiate(GameManager.Instance.burnParticle, transform.position + new Vector3(0, 0.5f, 0), Quaternion.identity);
        burn.gameObject.SetActive(true);
        burn.transform.SetParent(transform);
        burn.Play(true);
    }
}

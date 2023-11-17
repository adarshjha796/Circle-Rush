using UnityEngine;
using System.Collections;

public class ObstacleController : MonoBehaviour
{
    public enum MovingType
    {
        Circle,
        Straight
    }

    [HideInInspector]
    public MovingType moveType = MovingType.Circle;
    public float minCircularSpeed = 30;
    public float maxCircularSpeed = 90;
    public float minStraightSpeed = 3;
    public float maxStraightSpeed = 5;

    [HideInInspector]
    public Vector3 moveDir;
    [HideInInspector]
    public float speed = 3;
    [HideInInspector]
    public bool isOnLeft;

    private Rigidbody rigid;
    private BoxCollider boxCollider;
    private MeshCollider meshCollider;
    private bool isAnimal;
    private bool stop = true;

    // Use this for initialization
    void Start()
    {
        rigid = GetComponent<Rigidbody>();

        if (GetComponent<Animator>() == null) //Car 
        {
            isAnimal = false;

            if (moveType == MovingType.Straight)
            {
                transform.eulerAngles = (transform.position.x > 0) ?
                    (new Vector3(0, 270, 0)) :
                    (new Vector3(0, 90, 0));
            }

            meshCollider = GetComponent<MeshCollider>();
            transform.Find("SpotLight").gameObject.SetActive((GameManager.Instance.dayNight == DayNight.Day) ? (false) : (true));
        }
        else //Animal
        {
            isAnimal = true;

            boxCollider = GetComponent<BoxCollider>();

            if (moveType == MovingType.Straight)
            {
                transform.eulerAngles = (transform.position.x > 0) ?
                    (new Vector3(0, 180, 0)) :
                    (new Vector3(0, 0, 0));
            }
        }
    }
	

    // Update is called once per frame
    void Update()
    {
        if (!stop)
        {
            if (moveType == MovingType.Circle)
            {
                if (GameManager.Instance.GameState != GameState.GameOver)
                    transform.RotateAround(GameManager.Instance.carsRotatePoint.position, moveDir, speed * Time.deltaTime);
            }
            else
            {
                Vector3 pos = Camera.main.WorldToViewportPoint(transform.position);
                if (isOnLeft)
                {
                    if (pos.x >= 1.1f)
                    {
                        Destroy(gameObject);
                    }
                    else
                        transform.position += moveDir * speed * Time.deltaTime;
                }
                else
                {
                    if (pos.x <= -0.1f)
                    {
                        Destroy(gameObject);
                    }
                    else
                        transform.position += moveDir * speed * Time.deltaTime;
                }
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            stop = true;
            rigid.isKinematic = false;

            if (isAnimal)
            {
                boxCollider.isTrigger = false;
                GetComponent<Animator>().enabled = false;
            }
            else
            { 
                meshCollider.isTrigger = false;
            }
        }
    }

    public void Run()
    {
        if (moveType == MovingType.Circle)
            speed = Random.Range(minCircularSpeed, maxCircularSpeed);
        else
            speed = Random.Range(minStraightSpeed, maxStraightSpeed);

        stop = false;   
    }

    void Burn()
    {
        ParticleSystem burn = (ParticleSystem)Instantiate(GameManager.Instance.burnParticle, transform.position + new Vector3(0, 0.5f, 0), Quaternion.identity);
        burn.gameObject.SetActive(true);
        burn.transform.SetParent(transform);
        burn.Play(true);
    }
}

using UnityEngine;
using System.Collections;
using SgLib;

public class CoinController : MonoBehaviour
{
    [HideInInspector]
    public float speed;
    private MeshCollider meshCollider;
    private bool stopMoving;

    // Use this for initialization
    void Start()
    {
        meshCollider = GetComponent<MeshCollider>();     
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.GameState == GameState.Playing)
        {
            Vector3 pos = Camera.main.WorldToViewportPoint(transform.position);
            if (pos.x <= -0.2f)
                Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        StartCoroutine(Rotate());
    }

    IEnumerator Rotate()
    {
        while (true)
        {
            transform.Rotate(Vector3.up * 5f);
            yield return null;
        }
    }

    public void FirstMove(float time, float endY, bool disableAfterMove)
    {
        StartCoroutine(Move(time, endY, disableAfterMove));
    }

    IEnumerator Move(float time, float endY, bool disableAfterMove)
    {
        float startY = transform.position.y;
        float t = 0;
        while (t < time)
        {
            t += Time.deltaTime;
            float fraction = t / time;
            float newY = Mathf.Lerp(startY, endY, fraction);
            Vector3 pos = transform.position;
            pos.y = newY;
            transform.position = pos;
            yield return null;
        }

        if (disableAfterMove)
        {
            if (tag.Equals("BigCoin"))
                Destroy(gameObject);
            else
            {
                gameObject.SetActive(false);
                meshCollider.enabled = true;
                transform.rotation = Quaternion.identity;
                transform.SetParent(CoinManager.Instance.transform);
            }       
        }
    }

    public void MoveCoin()
    {
        StartCoroutine(Moving());
    }

    IEnumerator Moving()
    {
        while (!stopMoving)
        {
            transform.position += Vector3.left * speed * Time.deltaTime;
            yield return null;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && GameManager.Instance.GameState == GameState.Playing)
        {
            if (tag.Equals("BigCoin"))
                CoinManager.Instance.AddCoins(10);
            else
                CoinManager.Instance.AddCoins(1);
            
            SoundManager.Instance.PlaySound(SoundManager.Instance.coin);
            meshCollider.enabled = false;
            stopMoving = true;
            StartCoroutine(Move(0.5f, 10f, true));
        }
    }
}

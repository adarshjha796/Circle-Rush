using UnityEngine;
using System.Collections;
using SgLib;

public class CoinCreater : MonoBehaviour
{

    private GameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject coin = Instantiate(gameManager.coinPrefab, transform.GetChild(i).position, Quaternion.identity) as GameObject;
            coin.SetActive(false);
            coin.transform.SetParent(CoinManager.Instance.transform);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && GameManager.Instance.GameState == GameState.Playing)
        {
            if (gameManager.GameMode == 0)
                CreateCoin(transform.GetChild(0).position);
            else
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    CreateCoin(transform.GetChild(i).position);
                }
            }         
        }
    }

    void CreateCoin(Vector3 position)
    {
        GameObject coin = CoinManager.Instance.transform.GetChild(0).gameObject;
        coin.SetActive(true);
        coin.transform.SetParent(null);
        coin.transform.position = position;
        coin.GetComponent<CoinController>().FirstMove(0.2f, 1f, false);
    }
}

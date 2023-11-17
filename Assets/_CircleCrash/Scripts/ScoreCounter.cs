using UnityEngine;
using SgLib;

public class ScoreCounter : MonoBehaviour
{

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && GameManager.Instance.GameState == GameState.Playing)
        {
            ScoreManager.Instance.AddScore(1);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BottomBorder : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ball"))
        {
            Ball ballScript = other.GetComponent<Ball>();
            if (ballScript != null && !ballScript.HasCollided())
            {
                ballScript.MarkAsCollided();
                
                GameManager gameManager = Object.FindAnyObjectByType<GameManager>();
                if (gameManager != null)
                {
                    gameManager.BallReturned(other.gameObject);
                }
                else
                {
                    Destroy(other.gameObject);
                }
            }
        }
    }
}
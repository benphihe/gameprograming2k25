using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BottomBorder : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        // Vérifier si c'est une balle
        if (other.CompareTag("Ball"))
        {
            Ball ballScript = other.GetComponent<Ball>();
            if (ballScript != null && !ballScript.HasCollided())
            {
                // Marquer la balle comme ayant collisionné
                ballScript.MarkAsCollided();
                
                // Informer le GameManager que la balle est sortie de la scène
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
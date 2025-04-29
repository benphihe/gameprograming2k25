using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    private GameManager gameManager;
    private bool hasCollided = false;

    public void Initialize(Vector2 launchDirection, GameManager manager)
    {
        gameManager = manager;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = launchDirection * 10f; // Réduction de la vitesse
            Debug.Log("Ball.Initialize() - Velocity set to: " + rb.linearVelocity); // [DEBUG]
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!hasCollided)
        {
            if (collision.gameObject.CompareTag("Block") || collision.gameObject.CompareTag("ReturnZone"))
            {
                hasCollided = true;
                if (gameManager != null)
                {
                    gameManager.BallReturned(gameObject);
                }
                else
                {
                    Debug.LogError("GameManager is null in Ball.cs!");
                    Destroy(gameObject);
                }
            }
        }
    }

    public bool HasCollided()
    {
        return hasCollided;
    }

    void OnDestroy()
    {
        Debug.Log("Ball.OnDestroy() called. Ball destroyed: " + gameObject.name);
    }
}
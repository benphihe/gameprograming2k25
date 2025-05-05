using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    private GameManager gameManager;
    private bool hasCollided = false;
    private Rigidbody2D rb;
    public float minVelocityThreshold = 5f;
    private float currentSpeed;
    private Vector2 lastVelocity;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D manquant sur la balle!");
        }
        
        // Ajouter un tag à la balle pour la détection
        gameObject.tag = "Ball";
    }

    public void Initialize(Vector2 launchDirection, GameManager manager)
    {
        gameManager = manager;
        if (rb != null)
        {
            // Configuration de la physique pour un mouvement plus naturel
            rb.gravityScale = 0.5f; // Réduire légèrement la gravité pour un mouvement plus fluide
            rb.linearDamping = 0f; // Réduire la résistance à l'air
            rb.angularDamping = 0f; // Réduire la résistance à la rotation
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // Éviter les passages à travers des objets rapides
            
            currentSpeed = gameManager.ballSpeed;
            rb.AddForce(launchDirection * currentSpeed, ForceMode2D.Impulse);
            Debug.Log("Ball.Initialize() - Force appliquée: " + (launchDirection * currentSpeed));
        }
    }

    void FixedUpdate()
    {
        if (rb != null)
        {
            lastVelocity = rb.linearVelocity;

            // Maintenir une vitesse minimale
            if (rb.linearVelocity.magnitude < gameManager.minBallSpeed)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * gameManager.minBallSpeed;
            }
            
            // Limiter la vitesse maximale
            if (rb.linearVelocity.magnitude > gameManager.maxBallSpeed)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * gameManager.maxBallSpeed;
            }

            // Appliquer une légère perte d'énergie lors des rebonds
            if (rb.linearVelocity.magnitude > currentSpeed)
            {
                rb.linearVelocity *= (1f - gameManager.bounceEnergyLoss);
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Si la balle touche un bloc, on lui inflige des dégâts
        if (collision.gameObject.CompareTag("Block"))
        {
            Block blockScript = collision.gameObject.GetComponent<Block>();
            if (blockScript != null)
            {
                blockScript.TakeDamage(1);
                Debug.Log("Ball hit block: " + collision.gameObject.name);
            }

            // Ne pas marquer la balle comme "collidée" ici pour qu'elle continue à rebondir
            return;
        }

        // Gestion du retour de la balle (par exemple, si elle touche le bas de l'écran ou une zone de retour)
        if (!hasCollided)
        {
            if (collision.gameObject.CompareTag("ReturnZone") || 
                collision.gameObject.name.Contains("Ground") ||
                collision.gameObject.layer == LayerMask.NameToLayer("Ground") ||
                collision.gameObject.transform.position.y < -4.0f)  
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
            else
            {
                // Calculer le rebond
                Vector2 normal = collision.contacts[0].normal;
                Vector2 reflection = Vector2.Reflect(lastVelocity.normalized, normal);
                
                // Appliquer la nouvelle vélocité avec la même magnitude
                float speed = lastVelocity.magnitude;
                rb.linearVelocity = reflection * speed;

                // Ajouter un petit effet de rotation pour plus de réalisme
                float rotationAmount = Vector2.SignedAngle(lastVelocity, reflection);
                rb.angularVelocity = rotationAmount * 0.5f;
            }
        }
    }

    public void MarkAsCollided()
    {
        hasCollided = true;
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
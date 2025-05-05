using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    private GameManager gameManager;
    private bool hasCollided = false;
    private Rigidbody2D rb;
    public float minVelocityThreshold = 5f; // Seuil minimal de vitesse pour éviter les balles trop lentes

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
            
            // Appliquer une force d'impulsion pour un mouvement plus naturel au lieu d'une vélocité constante
            rb.AddForce(launchDirection * manager.ballSpeed, ForceMode2D.Impulse);
            Debug.Log("Ball.Initialize() - Force appliquée: " + (launchDirection * manager.ballSpeed));
        }
    }

    void FixedUpdate()
    {
        // Maintenir une certaine vitesse minimale pour éviter les mouvements trop lents
        if (rb != null && rb.linearVelocity.magnitude < minVelocityThreshold)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * minVelocityThreshold;
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
                blockScript.TakeDamage();
                Debug.Log("Ball hit block: " + collision.gameObject.name);
            }
        }

        // Ajouter une légère variation à la vitesse après collision pour éviter les trajectoires prévisibles
        if (rb != null)
        {
            // Légère variation aléatoire de la trajectoire après collision pour plus de dynamisme
            Vector2 currentVelocity = rb.linearVelocity;
            float randomVariation = Random.Range(-0.1f, 0.1f);
            rb.linearVelocity = new Vector2(
                currentVelocity.x + randomVariation, 
                currentVelocity.y + randomVariation
            ).normalized * currentVelocity.magnitude;
        }

        // Gestion du retour de la balle
        if (!hasCollided)
        {
            if (collision.gameObject.CompareTag("Block") || 
                collision.gameObject.name.Contains("ReturnZone") || 
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
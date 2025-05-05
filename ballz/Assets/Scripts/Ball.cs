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
    private Vector2 lastPosition;
    private float minBounceAngle = 20f; // Angle minimum de rebond en degrés

    // Détection de blocage
    private float stuckTimer = 0f;
    private float stuckThreshold = 2f; // secondes
    private float minMoveDistance = 0.1f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f; // Suppression de la gravité
            rb.linearDamping = 0f;
            rb.angularDamping = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation; // Empêcher la rotation automatique
        }
        
        // Ajouter un tag à la balle pour la détection
        gameObject.tag = "Ball";
    }

    public void Initialize(Vector2 launchDirection, GameManager manager)
    {
        gameManager = manager;
        if (rb != null)
        {
            currentSpeed = gameManager.ballSpeed;
            rb.linearVelocity = launchDirection * currentSpeed;
        }
        lastPosition = transform.position;
        stuckTimer = 0f;
    }

    void FixedUpdate()
    {
        if (rb != null)
        {
            // Détection de blocage
            float moveDistance = Vector2.Distance((Vector2)transform.position, lastPosition);
            if (moveDistance < minMoveDistance)
            {
                stuckTimer += Time.fixedDeltaTime;
                if (stuckTimer > stuckThreshold)
                {
                    // Relance la balle dans une direction aléatoire vers le bas
                    Vector2 randomDir = new Vector2(Random.Range(-0.7f, 0.7f), -1f).normalized;
                    rb.linearVelocity = randomDir * currentSpeed;
                    stuckTimer = 0f;
                }
            }
            else
            {
                stuckTimer = 0f;
            }
            lastPosition = transform.position;
            lastVelocity = rb.linearVelocity;

            // Maintenir une vitesse constante
            if (rb.linearVelocity.magnitude != currentSpeed)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * currentSpeed;
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
                HandleBounce(collision);
            }
        }
    }

    void HandleBounce(Collision2D collision)
    {
        ContactPoint2D contact = collision.contacts[0];
        Vector2 normal = contact.normal;
        Vector2 incomingVelocity = lastVelocity;

        // Calculer la nouvelle direction de rebond
        Vector2 reflection = Vector2.Reflect(incomingVelocity.normalized, normal);

        // Correction d'angle pour éviter les trajectoires trop horizontales ou verticales
        float minAngle = 15f; // angle minimum par rapport à l'axe horizontal
        float angleWithHorizontal = Mathf.Abs(Vector2.Angle(reflection, Vector2.right));
        if (angleWithHorizontal < minAngle || angleWithHorizontal > 180f - minAngle)
        {
            float sign = Mathf.Sign(reflection.y);
            float angle = minAngle * sign;
            reflection = Quaternion.Euler(0, 0, angle) * Vector2.right;
        }

        // Appliquer la nouvelle vélocité avec une vitesse constante
        rb.linearVelocity = reflection.normalized * currentSpeed;

        // Ajouter une légère variation aléatoire à l'angle de rebond
        float randomVariation = Random.Range(-gameManager.bounceAngleVariation, gameManager.bounceAngleVariation);
        rb.linearVelocity = Quaternion.Euler(0, 0, randomVariation) * rb.linearVelocity;
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
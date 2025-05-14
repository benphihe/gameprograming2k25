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
    private Vector2 prevPosition;
    private float stuckTime = 0f;
    private float stuckThreshold = 0.05f;
    private float stuckTimeLimit = 0.5f;
    private int consecutiveBounces = 0;
    private float consecutiveBounceTimeThreshold = 0.1f;
    private float lastBounceTime = 0f;

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
            rb.sharedMaterial = CreateBouncyMaterial(); // Ajouter un matériau physique pour améliorer les rebonds
        }
        
        // Ajouter un tag à la balle pour la détection
        gameObject.tag = "Ball";
    }

    // Créer un matériau physique pour améliorer les rebonds
    private PhysicsMaterial2D CreateBouncyMaterial()
    {
        PhysicsMaterial2D material = new PhysicsMaterial2D("BouncyBallMaterial");
        material.friction = 0f;
        material.bounciness = 1f;
        return material;
    }

    public void Initialize(Vector2 launchDirection, GameManager manager)
    {
        if (manager == null)
        {
            Debug.LogError("GameManager is null in Ball.Initialize!");
            return;
        }

        gameManager = manager;
        if (rb != null)
        {
            // Appliquer les multiplicateurs de progression
            currentSpeed = gameManager.ballSpeed * gameManager.ballSpeedMultiplier;
            rb.linearVelocity = launchDirection * currentSpeed;
            
            // Appliquer le multiplicateur de taille
            transform.localScale = Vector3.one * gameManager.ballSize * gameManager.ballSizeMultiplier;
        }
        lastPosition = transform.position;
        prevPosition = lastPosition;
    }

    void FixedUpdate()
    {
        if (rb != null)
        {
            // Détection de blocage améliorée
            prevPosition = lastPosition;
            float moveDistance = Vector2.Distance((Vector2)transform.position, lastPosition);
            lastPosition = transform.position;
            lastVelocity = rb.linearVelocity;

            // Vérifier si la balle est coincée
            DetectAndFixStuck(moveDistance);

            // Maintenir une vitesse constante
            if (rb.linearVelocity.magnitude != currentSpeed)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * currentSpeed;
            }

            // Éviter les trajectoires trop horizontales ou verticales
            EnsureNonExtremePaths();
        }
    }

    void DetectAndFixStuck(float moveDistance)
    {
        // Si la balle bouge très peu
        if (moveDistance < stuckThreshold && rb.linearVelocity.magnitude < currentSpeed * 0.5f)
        {
            stuckTime += Time.fixedDeltaTime;
            
            // Si la balle est coincée pendant trop longtemps
            if (stuckTime > stuckTimeLimit)
            {
                // Donner une nouvelle direction à la balle légèrement aléatoire
                float randomAngle = Random.Range(30f, 150f);
                Vector2 newDirection = Quaternion.Euler(0, 0, randomAngle) * Vector2.right;
                rb.linearVelocity = newDirection * currentSpeed;
                
                // Déplacer légèrement la balle pour la sortir de la collision
                transform.position += new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(0.05f, 0.15f), 0);
                
                // Réinitialiser le compteur de temps coincé
                stuckTime = 0f;
                Debug.Log("Ball unstuck with new direction");
            }
        }
        else
        {
            // Réinitialiser le compteur si la balle se déplace correctement
            stuckTime = 0f;
        }
    }

    void EnsureNonExtremePaths()
    {
        Vector2 velocity = rb.linearVelocity;
        
        // Vérifier si la trajectoire est trop horizontale ou verticale
        float absX = Mathf.Abs(velocity.x);
        float absY = Mathf.Abs(velocity.y);
        
        // Si la trajectoire est trop horizontale ou verticale
        if (absX < 0.1f * currentSpeed || absY < 0.1f * currentSpeed)
        {
            // Ajouter une légère déviation
            float angleAdjustment = Random.Range(5f, 15f);
            velocity = Quaternion.Euler(0, 0, velocity.x < 0.1f * currentSpeed ? angleAdjustment : -angleAdjustment) * velocity;
            rb.linearVelocity = velocity.normalized * currentSpeed;
            Debug.Log("Corrected extreme trajectory");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Block"))
        {
            Block block = collision.gameObject.GetComponent<Block>();
            if (block != null)
            {
                int damage = gameManager.CalculateBallDamage();
                block.TakeDamage(damage);
            }
        }
        else if (collision.gameObject.CompareTag("ReturnZone") || 
                collision.gameObject.name.Contains("Ground") ||
                collision.gameObject.layer == LayerMask.NameToLayer("Ground") ||
                collision.gameObject.transform.position.y < -4.0f)
        {
            hasCollided = true;
            if (gameManager != null)
            {
                gameManager.BallReturned(gameObject);
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        // Supprimer cette méthode car elle cause des dégâts en double
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
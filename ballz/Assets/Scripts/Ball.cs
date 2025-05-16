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
            rb.gravityScale = 0f; 
            rb.linearDamping = 0f;
            rb.angularDamping = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.sharedMaterial = CreateBouncyMaterial();
        }
        
        gameObject.tag = "Ball";
    }

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
            float speedMultiplier = 1f;
            float sizeMultiplier = 1f;

            if (ProgressionManager.Instance != null)
            {
                speedMultiplier = ProgressionManager.Instance.ballSpeedMultiplier;
                sizeMultiplier = ProgressionManager.Instance.ballSizeMultiplier;
            }

            currentSpeed = gameManager.ballSpeed * speedMultiplier;
            rb.linearVelocity = launchDirection * currentSpeed;
            
            transform.localScale = Vector3.one * gameManager.ballSize * sizeMultiplier;
        }
        lastPosition = transform.position;
        prevPosition = lastPosition;
    }

    void FixedUpdate()
    {
        if (rb != null)
        {
            prevPosition = lastPosition;
            float moveDistance = Vector2.Distance((Vector2)transform.position, lastPosition);
            lastPosition = transform.position;
            lastVelocity = rb.linearVelocity;

            DetectAndFixStuck(moveDistance);

            if (rb.linearVelocity.magnitude != currentSpeed)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * currentSpeed;
            }

            EnsureNonExtremePaths();
        }
    }

    void DetectAndFixStuck(float moveDistance)
    {
        if (moveDistance < stuckThreshold && rb.linearVelocity.magnitude < currentSpeed * 0.5f)
        {
            stuckTime += Time.fixedDeltaTime;
            
            if (stuckTime > stuckTimeLimit)
            {
                float randomAngle = Random.Range(30f, 150f);
                Vector2 newDirection = Quaternion.Euler(0, 0, randomAngle) * Vector2.right;
                rb.linearVelocity = newDirection * currentSpeed;
                
                transform.position += new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(0.05f, 0.15f), 0);
                
                stuckTime = 0f;
                Debug.Log("Ball unstuck with new direction");
            }
        }
        else
        {
            stuckTime = 0f;
        }
    }

    void EnsureNonExtremePaths()
    {
        Vector2 velocity = rb.linearVelocity;
        
        float absX = Mathf.Abs(velocity.x);
        float absY = Mathf.Abs(velocity.y);
        
        if (absX < 0.1f * currentSpeed || absY < 0.1f * currentSpeed)
        {
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
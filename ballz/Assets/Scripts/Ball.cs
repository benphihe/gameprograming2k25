using UnityEngine;

public class Ball : MonoBehaviour
{
    private Vector2 velocity;
    private GameManager gameManager;
    private Rigidbody2D rb;
    private CircleCollider2D circleCollider;
    private float screenWidth;
    private float screenHeight;
    private float ballRadius;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        circleCollider = GetComponent<CircleCollider2D>();
        ballRadius = circleCollider.radius;
        
        // Calculate screen bounds
        screenWidth = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0)).x;
        screenHeight = Camera.main.ScreenToWorldPoint(new Vector3(0, Screen.height, 0)).y;
    }
    
    public void Initialize(Vector2 initialVelocity, GameManager manager)
    {
        velocity = initialVelocity;
        gameManager = manager;
        rb.linearVelocity = velocity;
    }
    
    void Update()
    {
        // Check if ball is out of bounds
        if (transform.position.y < -6f)
        {
            gameManager.BallReturned(gameObject);
        }
        
        // Simple bounce off screen edges
        if ((transform.position.x < -screenWidth + ballRadius && rb.linearVelocity.x < 0) ||
            (transform.position.x > screenWidth - ballRadius && rb.linearVelocity.x > 0))
        {
            rb.linearVelocity = new Vector2(-rb.linearVelocity.x, rb.linearVelocity.y);
        }
        
        if ((transform.position.y > screenHeight - ballRadius && rb.linearVelocity.y > 0))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -rb.linearVelocity.y);
        }
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Block"))
        {
            Block block = collision.gameObject.GetComponent<Block>();
            if (block != null)
            {
                block.TakeDamage();
            }
        }
    }
}
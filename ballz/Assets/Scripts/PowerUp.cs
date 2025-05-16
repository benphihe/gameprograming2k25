using UnityEngine;

public class PowerUp : MonoBehaviour
{
    public enum PowerUpType
    {
        MultiBall,
        BallSize,
        SlowMotion,
        ExtraBalls
    }

    public PowerUpType type;
    public float duration = 10f;
    public float fallSpeed = 2f;
    public float rotationSpeed = 100f;
    public GameObject effectPrefab;

    private GameManager gameManager;

    void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();
    }

    void Update()
    {
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);
        transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);

        if (transform.position.y < -6f)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ball"))
        {
            ApplyPowerUp();
            if (effectPrefab != null)
            {
                Instantiate(effectPrefab, transform.position, Quaternion.identity);
            }
            Destroy(gameObject);
        }
    }

    void ApplyPowerUp()
    {
        switch (type)
        {
            case PowerUpType.MultiBall:
                GameObject[] activeBalls = GameObject.FindGameObjectsWithTag("Ball");
                foreach (GameObject ball in activeBalls)
                {
                    Vector3 position = ball.transform.position;
                    Vector2 velocity = ball.GetComponent<Rigidbody2D>().linearVelocity;
                    
                    CreateNewBall(position, Quaternion.Euler(0, 0, 15) * velocity);
                    CreateNewBall(position, Quaternion.Euler(0, 0, -15) * velocity);
                }
                break;

            case PowerUpType.BallSize:
                foreach (GameObject ball in GameObject.FindGameObjectsWithTag("Ball"))
                {
                    ball.transform.localScale *= 1.5f;
                }
                break;

            case PowerUpType.SlowMotion:
                Time.timeScale = 0.5f;
                Invoke("ResetTimeScale", duration);
                break;

            case PowerUpType.ExtraBalls:
                if (gameManager != null)
                {
                    var field = typeof(GameManager).GetField("ballCount", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (field != null)
                    {
                        int currentCount = (int)field.GetValue(gameManager);
                        field.SetValue(gameManager, currentCount + 1);
                    }
                }
                break;
        }
    }

    void CreateNewBall(Vector3 position, Vector2 velocity)
    {
        GameObject newBall = Instantiate(gameManager.ballPrefab, position, Quaternion.identity);
        Rigidbody2D rb = newBall.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = velocity;
        }
    }

    void ResetTimeScale()
    {
        Time.timeScale = 1f;
    }
} 
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
        // Faire tomber le power-up
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);
        transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);

        // Détruire si hors de l'écran
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
                // Dupliquer toutes les balles actives
                GameObject[] activeBalls = GameObject.FindGameObjectsWithTag("Ball");
                foreach (GameObject ball in activeBalls)
                {
                    Vector3 position = ball.transform.position;
                    Vector2 velocity = ball.GetComponent<Rigidbody2D>().linearVelocity;
                    
                    // Créer deux nouvelles balles avec des directions légèrement différentes
                    CreateNewBall(position, Quaternion.Euler(0, 0, 15) * velocity);
                    CreateNewBall(position, Quaternion.Euler(0, 0, -15) * velocity);
                }
                break;

            case PowerUpType.BallSize:
                // Augmenter la taille de toutes les balles
                foreach (GameObject ball in GameObject.FindGameObjectsWithTag("Ball"))
                {
                    ball.transform.localScale *= 1.5f;
                }
                break;

            case PowerUpType.SlowMotion:
                // Ralentir le temps
                Time.timeScale = 0.5f;
                Invoke("ResetTimeScale", duration);
                break;

            case PowerUpType.ExtraBalls:
                // Ajouter une balle supplémentaire
                if (gameManager != null)
                {
                    // Utiliser la réflexion pour accéder à la variable privée ballCount
                    var field = typeof(GameManager).GetField("ballCount", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (field != null)
                    {
                        int currentCount = (int)field.GetValue(gameManager);
                        field.SetValue(gameManager, currentCount + 1);  // Ajoute une seule balle
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
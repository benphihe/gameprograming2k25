using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Elements")]
    public GameObject blockPrefab;
    public GameObject ballPrefab;
    public Transform blockContainer;
    public LineRenderer trajectoryLine;
    
    [Header("Power-ups")]
    public GameObject[] powerUpPrefabs;
    public float powerUpDropChance = 0.2f;
    public float powerUpDuration = 10f;
    
    [Header("UI Elements")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI ballCountText;
    public TextMeshProUGUI comboText;
    
    [Header("Game Settings")]
    public int gridWidth = 7;
    public int gridHeight = 8;
    public float blockSize = 1f;
    public float ballSpeed = 15f;
    public int initialBallCount = 3;
    public float ballSize = 1f;
    
    [Header("Ball Physics Settings")]
    public float minLaunchAngle = 20f;
    public float maxLaunchAngle = 160f;
    public float minBallSpeed = 8f;
    public float maxBallSpeed = 20f;
    public float bounceEnergyLoss = 0.1f;
    public float maxBallSpeedMultiplier = 1.5f;
    public float bounceAngleVariation = 5f; // Variation d'angle lors des rebonds
    
    private Vector3 launchPosition;
    private Vector2 launchDirection;
    private bool isDragging = false;
    private int score = 0;
    private int ballCount;
    private bool canLaunch = true;
    private List<GameObject> activeBalls = new List<GameObject>();
    private Vector3 dragStartPosition;
    private int currentCombo = 0;
    private float comboTimeRemaining = 0f;
    private float comboTimeWindow = 2f;
    private Color[] blockColors = new Color[] {
        new Color(0.95f, 0.3f, 0.6f), // Pink
        new Color(0.3f, 0.7f, 0.9f),  // Blue
        new Color(1f, 0.85f, 0.2f)    // Yellow
    };

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Debug.Log("GameManager.Start() called.");
        ballCount = initialBallCount;
        UpdateBallCountText();
        launchPosition = new Vector3(0, -4.5f, 0);
        InitializeGrid();
        
        // Configuration de la ligne de trajectoire
        if (trajectoryLine != null)
        {
            trajectoryLine.positionCount = 20; // Plus de points pour une meilleure visualisation
            trajectoryLine.startWidth = 0.1f;  // Ligne plus fine au début
            trajectoryLine.endWidth = 0.02f;   // Ligne qui s'affine à la fin
        }
    }
    
    void Update()
    {
        HandleInput();

        if (activeBalls.Count == 0 && !canLaunch && ballCount > 0)
        {
            canLaunch = true;
        }

        // Gestion du combo
        if (comboTimeRemaining > 0)
        {
            comboTimeRemaining -= Time.deltaTime;
            if (comboTimeRemaining <= 0)
            {
                currentCombo = 0;
                UpdateComboText();
            }
        }
    }
    
    void HandleInput()
    {
        if (!canLaunch || ballCount <= 0) return;
        
        if (Input.GetMouseButtonDown(0))
        {
            dragStartPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            dragStartPosition.z = 0;
            isDragging = true;
            trajectoryLine.gameObject.SetActive(true);
            Debug.Log("GameManager.HandleInput() - Mouse Down.");
        }
        
        if (isDragging)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            
            // Calculer la direction en fonction de la position du toucher et de la position de lancement
            Vector2 direction = (mousePos - launchPosition);
            
            // Limiter l'angle de lancement pour un mouvement plus prévisible
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            
            // Restreindre l'angle entre minLaunchAngle et maxLaunchAngle
            if (angle < 0) angle += 360f;
            
            if (angle < minLaunchAngle || angle > maxLaunchAngle)
            {
                // Fixer l'angle aux limites
                float clampedAngle;
                if (Mathf.Abs(angle - minLaunchAngle) < Mathf.Abs(angle - maxLaunchAngle))
                    clampedAngle = minLaunchAngle;
                else
                    clampedAngle = maxLaunchAngle;
                
                float radAngle = clampedAngle * Mathf.Deg2Rad;
                direction = new Vector2(Mathf.Cos(radAngle), Mathf.Sin(radAngle));
            }
            
            launchDirection = direction.normalized;
            
            // Visualiser la trajectoire avec une courbe balistique
            DrawTrajectoryLine(launchPosition, launchDirection * ballSpeed);
            
            Debug.Log("GameManager.HandleInput() - Dragging. Launch direction: " + launchDirection);
        }
        
        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            trajectoryLine.gameObject.SetActive(false);
            LaunchBall();
            Debug.Log("GameManager.HandleInput() - Mouse Up, LaunchBall() called.");
        }
    }
    
    // Méthode pour dessiner une trajectoire balistique plus réaliste
    void DrawTrajectoryLine(Vector3 startPos, Vector2 velocity)
    {
        if (trajectoryLine == null) return;
        
        Vector3[] points = new Vector3[trajectoryLine.positionCount];
        
        // Simuler la physique pour obtenir une trajectoire réaliste
        float timeStep = 0.1f;
        float gravity = Physics2D.gravity.y * 0.5f; // Utiliser la même gravité que la balle
        
        for (int i = 0; i < points.Length; i++)
        {
            float time = i * timeStep;
            points[i] = startPos + new Vector3(
                velocity.x * time,
                velocity.y * time + 0.5f * gravity * time * time,
                0
            );
        }
        
        trajectoryLine.SetPositions(points);
    }
        
    void LaunchBall()
    {
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning("GameManager was inactive. Activating it now.");
            gameObject.SetActive(true);
        }

        if (ballCount > 0)
        {
            StartCoroutine(LaunchBallsSequentially());
            canLaunch = false;
            Debug.Log("GameManager.LaunchBall() called. Starting coroutine.");
        }
    }
    
    IEnumerator LaunchBallsSequentially()
    {
        if (ballCount > 0)
        {
            GameObject ball = Instantiate(ballPrefab, launchPosition, Quaternion.identity);
            
            // Ajuster la taille de la balle si nécessaire
            ball.transform.localScale = new Vector3(ballSize, ballSize, ballSize);
            
            Ball ballScript = ball.GetComponent<Ball>();
            ballScript.Initialize(launchDirection, this);
            activeBalls.Add(ball);
            ballCount--;
            UpdateBallCountText();
            Debug.Log("GameManager.LaunchBallsSequentially() - Ball launched. Ball count: " + ballCount + ", Active balls: " + activeBalls.Count);

            yield return new WaitUntil(() => ballScript.HasCollided());

            BallReturned(ball);
            Debug.Log("GameManager.LaunchBallsSequentially() - Ball returned in coroutine.");
        }
    }
    
    void InitializeGrid()
    {
        foreach (Transform child in blockContainer)
        {
            Destroy(child.gameObject);
        }
        
        for (int row = 0; row < 3; row++)
        {
            SpawnRowAtPosition(row);
        }
    }
    
    void SpawnRowAtPosition(int row)
    {
        if (blockContainer == null) 
        {
            Debug.LogError("Block container is not assigned!");
            return;
        }
        
        if (blockPrefab == null)
        {
            Debug.LogError("Block prefab is not assigned!");
            return;
        }
        
        float spacing = 0.2f;
        
        for (int col = 0; col < gridWidth; col++)
        {
            if (Random.value < 0.7f)
            {
                Vector2 position = new Vector2(
                    (col - gridWidth / 2) * (blockSize + spacing) + blockSize / 2,
                    (row - gridHeight / 2) * (blockSize + spacing) + 3f
                );
                
                GameObject block = Instantiate(blockPrefab, position, Quaternion.identity);
                if (block == null)
                {
                    Debug.LogError("Failed to instantiate block!");
                    continue;
                }
                
                block.transform.SetParent(blockContainer, true);
                
                Block blockScript = block.GetComponent<Block>();
                if (blockScript == null)
                {
                    Debug.LogError("Block script not found on instantiated block!");
                    continue;
                }
                
                // Générer une valeur entre 1 et 3 pour les points de vie
                int value = Random.Range(1, 4);
                blockScript.Initialize(value, Color.white); // La couleur sera gérée par le Block
            }
        }
    }
    
    public void BallReturned(GameObject ball)
    {
        if (ball == null)
        {
            Debug.LogWarning("GameManager.BallReturned() - Ball is null or already destroyed.");
            return;
        }

        activeBalls.Remove(ball);
        Destroy(ball);

        if (ballCount > 0)
        {
            canLaunch = true;
            Debug.Log("GameManager.BallReturned() - Can launch again.");
        }
        else
        {
            Debug.Log("GameManager.BallReturned() - Out of balls, game over!");
        }
    }
    
    public void BlockDestroyed(int value)
    {
        score += (int)(value * (1 + currentCombo * 0.5f));
        UpdateScoreText();
        
        // Gestion du combo
        currentCombo++;
        comboTimeRemaining = comboTimeWindow;
        UpdateComboText();

        // Chance de faire apparaître un power-up
        if (Random.value < powerUpDropChance)
        {
            SpawnPowerUp();
        }
    }
    
    void UpdateBallCountText()
    {
        ballCountText.text = ballCount.ToString();
    }

    void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }
    }

    void SpawnPowerUp()
    {
        if (powerUpPrefabs.Length == 0) return;

        Vector3 spawnPosition = new Vector3(
            Random.Range(-3f, 3f),
            Random.Range(0f, 4f),
            0f
        );

        int randomIndex = Random.Range(0, powerUpPrefabs.Length);
        GameObject powerUp = Instantiate(powerUpPrefabs[randomIndex], spawnPosition, Quaternion.identity);
    }

    void UpdateComboText()
    {
        if (comboText != null)
        {
            if (currentCombo > 1)
            {
                comboText.text = $"Combo x{currentCombo}";
                comboText.gameObject.SetActive(true);
            }
            else
            {
                comboText.gameObject.SetActive(false);
            }
        }
    }
}
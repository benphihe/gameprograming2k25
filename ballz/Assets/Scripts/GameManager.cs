using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

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
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI progressionPointsText;
    
    [Header("Game Settings")]
    public int gridWidth = 7;
    public int gridHeight = 8;
    public float blockSize = 1f;
    public float ballSpeed = 15f;
    public int initialBallCount = 5;
    public float ballSize = 1f;
    public int baseBallDamage = 1;
    
    [Header("Ball Physics Settings")]
    public float minLaunchAngle = 20f;
    public float maxLaunchAngle = 160f;
    public float minBallSpeed = 8f;
    public float maxBallSpeed = 20f;
    public float bounceEnergyLoss = 0.1f;
    public float maxBallSpeedMultiplier = 1.5f;
    public float bounceAngleVariation = 5f;
    
    [Header("Difficulty Settings")]
    public float blockHealthIncreaseRate = 0.2f;
    public float blockDensityIncreaseRate = 0.05f;
    public int bonusBallsPerLevel = 3;

    [Header("Progression System")]
    public int progressionPoints = 0;
    public int score = 0;
    public int blocksDestroyed = 0; 
    public int blocksForPoint = 10;
    public float ballDamageMultiplier = 1f;
    public float ballSizeMultiplier = 1f;
    public float ballSpeedMultiplier = 1f;
    public int extraBallsPerRun = 0;
    public float powerUpDropRateMultiplier = 1f;
    
    private Vector3 launchPosition;
    private Vector2 launchDirection;
    private bool isDragging = false;
    private int ballCount;
    private bool canLaunch = true;
    private List<GameObject> activeBalls = new List<GameObject>();
    private Vector3 dragStartPosition;
    private int currentCombo = 0;
    private float comboTimeRemaining = 0f;
    private float comboTimeWindow = 2f;
    private Color[] blockColors = new Color[] {
        new Color(0.95f, 0.3f, 0.6f),
        new Color(0.3f, 0.7f, 0.9f), 
        new Color(1f, 0.85f, 0.2f)  
    };
    
    private int currentLevel = 1;
    private float baseBlockSpawnChance = 0.7f;
    private int totalBlockCount = 0;
    private bool isLevelCleared = false;
    private float levelClearCheckInterval = 0.5f;
    private float levelClearCheckTimer = 0f;

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
        Debug.Log("GameManager.Start() appelé");
        if (ProgressionManager.Instance != null)
        {
            LoadProgression();
        }
        initialBallCount = 5;
        ballCount = initialBallCount + ProgressionManager.Instance.extraBallsPerRun;
        UpdateBallCountText();
        UpdateLevelText();
        UpdateProgressionPointsText();
        launchPosition = new Vector3(0, -4.5f, 0);
        InitializeGrid();
        if (trajectoryLine != null)
        {
            trajectoryLine.positionCount = 20;
            trajectoryLine.startWidth = 0.1f;
            trajectoryLine.endWidth = 0.02f;
        }
    }

    void OnApplicationQuit()
    {
        if (ProgressionManager.Instance != null)
        {
            ProgressionManager.Instance.ResetProgression();
        }
    }
    
    void Update()
    {
        if (SceneManager.GetActiveScene().name != "ProgressionMenu")
        {
            HandleInput();
        }

        if (activeBalls.Count == 0 && !canLaunch && ballCount > 0)
        {
            canLaunch = true;
        }

        if (comboTimeRemaining > 0)
        {
            comboTimeRemaining -= Time.deltaTime;
            if (comboTimeRemaining <= 0)
            {
                currentCombo = 0;
                UpdateComboText();
            }
        }
        
        if (!isLevelCleared)
        {
            levelClearCheckTimer += Time.deltaTime;
            if (levelClearCheckTimer >= levelClearCheckInterval)
            {
                CheckForLevelClear();
                levelClearCheckTimer = 0f;
            }
        }
    }
    
    void HandleInput()
    {
        if (SceneManager.GetActiveScene().name == "ProgressionMenu") return;
        if (!canLaunch || ballCount <= 0) return;
        if (Input.GetMouseButtonDown(0))
        {
            dragStartPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            dragStartPosition.z = 0;
            isDragging = true;
            if (trajectoryLine != null)
            {
                trajectoryLine.gameObject.SetActive(true);
            }
            Debug.Log("GameManager.HandleInput() - Mouse Down.");
        }
        if (isDragging)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            Vector2 direction = (mousePos - launchPosition);
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            if (angle < 0) angle += 360f;
            if (angle < minLaunchAngle || angle > maxLaunchAngle)
            {
                float clampedAngle;
                if (Mathf.Abs(angle - minLaunchAngle) < Mathf.Abs(angle - maxLaunchAngle))
                    clampedAngle = minLaunchAngle;
                else
                    clampedAngle = maxLaunchAngle;
                float radAngle = clampedAngle * Mathf.Deg2Rad;
                direction = new Vector2(Mathf.Cos(radAngle), Mathf.Sin(radAngle));
            }
            launchDirection = direction.normalized;
            if (trajectoryLine != null)
            {
                DrawTrajectoryLine(launchPosition, launchDirection * ballSpeed);
            }
            Debug.Log("GameManager.HandleInput() - Dragging. Launch direction: " + launchDirection);
        }
        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            if (trajectoryLine != null)
            {
                trajectoryLine.gameObject.SetActive(false);
            }
            LaunchBall();
            Debug.Log("GameManager.HandleInput() - Mouse Up, LaunchBall() called.");
        }
    }
    
    void DrawTrajectoryLine(Vector3 startPos, Vector2 velocity)
    {
        if (trajectoryLine == null) return;
        Vector3[] points = new Vector3[trajectoryLine.positionCount];
        float timeStep = 0.1f;
        float gravity = Physics2D.gravity.y * 0.5f;
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
            Debug.LogWarning("GameManager était inactif. Activation maintenant.");
            gameObject.SetActive(true);
        }
        if (ballCount > 0)
        {
            StartCoroutine(LaunchBallsSequentially());
            canLaunch = false;
            UpdateProgressionPointsText();
            Debug.Log("GameManager.LaunchBall() appelé. Démarrage de la coroutine.");
        }
    }
    
    IEnumerator LaunchBallsSequentially()
    {
        if (ballCount > 0)
        {
            GameObject ball = Instantiate(ballPrefab, launchPosition, Quaternion.identity);
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
        ClearBlocks();
        int rowsToSpawn = Mathf.Clamp(3 + (currentLevel - 1) / 2, 3, 6);
        totalBlockCount = 0;
        for (int row = 0; row < rowsToSpawn; row++)
        {
            SpawnRowAtPosition(row);
        }
        Debug.Log($"Level {currentLevel} initialized with {totalBlockCount} blocks.");
        isLevelCleared = false;
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
        float spawnChance = baseBlockSpawnChance + blockDensityIncreaseRate * (currentLevel - 1);
        spawnChance = Mathf.Clamp(spawnChance, 0.5f, 0.9f);
        for (int col = 0; col < gridWidth; col++)
        {
            if (Random.value < spawnChance)
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
                int maxHealth = CalculateMaxBlockHealth();
                int value = Random.Range(1, maxHealth + 1);
                blockScript.Initialize(value, Color.white);
                totalBlockCount++;
            }
        }
    }
    
    int CalculateMaxBlockHealth()
    {
        if (currentLevel <= 3)
            return 3;
        else if (currentLevel <= 6)
            return 4;
        else
            return 5;
    }
    
    void ClearBlocks()
    {
        foreach (Transform child in blockContainer)
        {
            Destroy(child.gameObject);
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
        if (activeBalls.Count == 0 && ballCount <= 0)
        {
            if (totalBlockCount > 0)
            {
                Debug.Log("Game Over - No more balls and blocks remaining!");
                GameOver();
            }
            else
            {
                CheckForLevelClear();
            }
        }
        if (ballCount > 0)
        {
            canLaunch = true;
            Debug.Log("GameManager.BallReturned() - Can launch again.");
        }
        else
        {
            Debug.Log("GameManager.BallReturned() - Out of balls, waiting for all balls to return.");
        }
    }
    
    public void BlockDestroyed(int value)
    {
        int scoreGain = (int)(value * (1 + currentCombo * 0.5f));
        score += scoreGain;
        blocksDestroyed++;
        if (blocksDestroyed >= blocksForPoint)
        {
            progressionPoints++;
            blocksDestroyed = 0;
            UpdateProgressionPointsText();
            SaveProgression();
            Debug.Log($"Point de progression gagné! Total: {progressionPoints}");
        }
        UpdateScoreText();
        totalBlockCount--;
        currentCombo++;
        comboTimeRemaining = comboTimeWindow;
        UpdateComboText();
        SpawnPowerUp();
    }
    
    void CheckForLevelClear()
    {
        if (totalBlockCount <= 0 && !isLevelCleared)
        {
            Debug.Log("All blocks destroyed! Starting next level.");
            isLevelCleared = true;
            StartCoroutine(StartNextLevel());
        }
    }
    
    IEnumerator StartNextLevel()
    {
        yield return new WaitForSeconds(1.5f);
        currentLevel++;
        UpdateLevelText();
        ballCount = initialBallCount + ProgressionManager.Instance.extraBallsPerRun;
        UpdateBallCountText();
        currentCombo = 0;
        UpdateComboText();
        InitializeGrid();
        canLaunch = true;
        Debug.Log($"Niveau {currentLevel} démarré. Nombre de balles: {ballCount}");
    }
    
    void UpdateBallCountText()
    {
        if (ballCountText != null)
        {
            ballCountText.text = "Balls : " +  ballCount.ToString();
        }
    }

    void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score : {score}";
        }
    }
    
    void UpdateLevelText()
    {
        if (levelText != null)
        {
            levelText.text = "Niveau " + currentLevel;
        }
    }

    void SpawnPowerUp()
    {
        if (powerUpPrefabs == null || powerUpPrefabs.Length == 0) return;
        float dropChance = powerUpDropChance;
        if (ProgressionManager.Instance != null)
        {
            dropChance *= ProgressionManager.Instance.powerUpDropRateMultiplier;
        }
        if (Random.value < dropChance)
        {
            int randomIndex = Random.Range(0, powerUpPrefabs.Length);
            Vector3 spawnPosition = new Vector3(Random.Range(-3f, 3f), Random.Range(-3f, 3f), 0);
            Instantiate(powerUpPrefabs[randomIndex], spawnPosition, Quaternion.identity);
        }
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

    void UpdateProgressionPointsText()
    {
        if (progressionPointsText != null)
        {
            progressionPointsText.text = $"Points de progression : {progressionPoints}";
        }
    }

    public void GameOver()
    {
        Debug.Log("Game Over - Saving progression and loading progression menu");
        SaveProgression();
        StopAllCoroutines();
        foreach (GameObject ball in activeBalls.ToArray())
        {
            if (ball != null)
            {
                Destroy(ball);
            }
        }
        activeBalls.Clear();
        SceneManager.LoadScene("ProgressionMenu");
    }

    public void SaveProgression()
    {
        PlayerPrefs.SetInt("ProgressionPoints", progressionPoints);
        PlayerPrefs.SetInt("Score", score);
        PlayerPrefs.SetInt("BlocksDestroyed", blocksDestroyed);
        PlayerPrefs.SetFloat("BallDamageMultiplier", ballDamageMultiplier);
        PlayerPrefs.SetFloat("BallSizeMultiplier", ballSizeMultiplier);
        PlayerPrefs.SetFloat("BallSpeedMultiplier", ballSpeedMultiplier);
        PlayerPrefs.SetInt("ExtraBallsPerRun", extraBallsPerRun);
        PlayerPrefs.SetFloat("PowerUpDropRateMultiplier", powerUpDropRateMultiplier);
        PlayerPrefs.Save();
    }

    public void LoadProgression()
    {
        if (ProgressionManager.Instance != null)
        {
            progressionPoints = ProgressionManager.Instance.progressionPoints;
            ballDamageMultiplier = ProgressionManager.Instance.ballDamageMultiplier;
            ballSizeMultiplier = ProgressionManager.Instance.ballSizeMultiplier;
            ballSpeedMultiplier = ProgressionManager.Instance.ballSpeedMultiplier;
            extraBallsPerRun = ProgressionManager.Instance.extraBallsPerRun;
            powerUpDropRateMultiplier = ProgressionManager.Instance.powerUpDropRateMultiplier;
        }
    }

    public int CalculateBallDamage()
    {
        if (ProgressionManager.Instance != null)
        {
            int damage = Mathf.RoundToInt(ProgressionManager.Instance.ballDamageMultiplier);
            Debug.Log($"Calcul des dégâts: multiplicateur={ProgressionManager.Instance.ballDamageMultiplier}, dégâts={damage}");
            return Mathf.Max(1, damage);
        }
        Debug.LogWarning("ProgressionManager.Instance est null dans CalculateBallDamage");
        return baseBallDamage;
    }
}

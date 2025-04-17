using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Game Elements")]
    public GameObject blockPrefab;
    public GameObject ballPrefab;
    public Transform blockContainer;
    public LineRenderer trajectoryLine;
    
    [Header("UI Elements")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI ballCountText;
    
    [Header("Game Settings")]
    public int gridWidth = 7;
    public int gridHeight = 8;
    public float blockSize = 1f;
    public float ballSpeed = 15f;
    public int initialBallCount = 1;
    
    private Vector3 launchPosition;
    private Vector2 launchDirection;
    private bool isDragging = false;
    private int score = 0;
    private int ballCount;
    private int movingBallCount = 0;
    private bool canLaunch = true;
    private List<GameObject> activeBalls = new List<GameObject>();
    private Color[] blockColors = new Color[] {
        new Color(0.95f, 0.3f, 0.6f), // Pink
        new Color(0.3f, 0.7f, 0.9f),  // Blue
        new Color(1f, 0.85f, 0.2f)    // Yellow
    };
    
    void Start()
    {
        ballCount = initialBallCount;
        UpdateBallCountText();
        launchPosition = new Vector3(0, -4.5f, 0);
        InitializeGrid();
    }
    
    void Update()
    {
        HandleInput();
        
        if (movingBallCount == 0 && !canLaunch)
        {
            canLaunch = true;
            MoveBlocksDown();
            SpawnNewRow();
            UpdateBallCountText();
        }
    }
    
    void HandleInput()
    {
        if (!canLaunch) return;
        
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            trajectoryLine.gameObject.SetActive(true);
        }
        
        if (isDragging)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            
            // Calculate direction from launch position to mouse, but only upward
            launchDirection = (mousePos - launchPosition).normalized;
            if (launchDirection.y < 0.1f) launchDirection.y = 0.1f;
            launchDirection = launchDirection.normalized;
            
            // Update trajectory line
            trajectoryLine.SetPosition(0, launchPosition);
            trajectoryLine.SetPosition(1, launchPosition + new Vector3(launchDirection.x, launchDirection.y, 0) * 5f);
        }
        
        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            trajectoryLine.gameObject.SetActive(false);
            LaunchBalls();
        }
    }
    
    void LaunchBalls()
    {
        StartCoroutine(LaunchBallsSequentially());
        canLaunch = false;
    }
    
    IEnumerator LaunchBallsSequentially()
    {
        for (int i = 0; i < ballCount; i++)
        {
            GameObject ball = Instantiate(ballPrefab, launchPosition, Quaternion.identity);
            Ball ballScript = ball.GetComponent<Ball>();
            ballScript.Initialize(launchDirection * ballSpeed, this);
            activeBalls.Add(ball);
            movingBallCount++;
            
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    void InitializeGrid()
    {
        // Clear existing blocks
        foreach (Transform child in blockContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Create initial rows of blocks
        for (int row = 0; row < 3; row++)
        {
            SpawnRowAtPosition(row);
        }
    }
    
    void SpawnRowAtPosition(int row)
    {
        for (int col = 0; col < gridWidth; col++)
        {
            // Random chance to spawn a block
            if (Random.value < 0.7f)
            {
                Vector2 position = new Vector2(
                    (col - gridWidth / 2) * blockSize + blockSize / 2,
                    (row - gridHeight / 2) * blockSize + 3f
                );
                
                GameObject block = Instantiate(blockPrefab, position, Quaternion.identity, blockContainer);
                Block blockScript = block.GetComponent<Block>();
                
                int value = Random.Range(1, 4) + score / 10;
                Color color = blockColors[Random.Range(0, blockColors.Length)];
                
                blockScript.Initialize(value, color);
            }
        }
    }
    
    void SpawnNewRow()
    {
        SpawnRowAtPosition(0);
        
        // Check for game over
        foreach (Transform block in blockContainer)
        {
            if (block.position.y <= launchPosition.y)
            {
                Debug.Log("Game Over!");
                // Implement game over logic here
            }
        }
    }
    
    void MoveBlocksDown()
    {
        foreach (Transform block in blockContainer)
        {
            block.position += new Vector3(0, -blockSize, 0);
        }
    }
    
    public void BallReturned(GameObject ball)
    {
        movingBallCount--;
        activeBalls.Remove(ball);
        Destroy(ball);
        
        if (movingBallCount <= 0)
        {
            ballCount++; // Increase ball count after each round
        }
    }
    
    public void BlockDestroyed(int value)
    {
        score += value;
        scoreText.text = score.ToString();
    }
    
    void UpdateBallCountText()
    {
        ballCountText.text = ballCount.ToString();
    }
}
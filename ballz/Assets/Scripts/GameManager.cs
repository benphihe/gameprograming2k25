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
    public int initialBallCount = 3;
    public float ballSize = 1f;
    
    private Vector3 launchPosition;
    private Vector2 launchDirection;
    private bool isDragging = false;
    private int score = 0;
    private int ballCount;
    private bool canLaunch = true;
    private List<GameObject> activeBalls = new List<GameObject>();
    private Color[] blockColors = new Color[] {
        new Color(0.95f, 0.3f, 0.6f), // Pink
        new Color(0.3f, 0.7f, 0.9f),  // Blue
        new Color(1f, 0.85f, 0.2f)    // Yellow
    };
    void Start()
    {
        Debug.Log("GameManager.Start() called."); // [DEBUG]
        ballCount = initialBallCount;
        UpdateBallCountText();
        launchPosition = new Vector3(0, -4.5f, 0);
        InitializeGrid();
    }
    
    void Update()
    {
        HandleInput();

        if (activeBalls.Count == 0 && !canLaunch && ballCount > 0)
        {
            canLaunch = true;
            Debug.Log("GameManager.Update() - Can launch again."); // [DEBUG]
        }
    }
    
    void HandleInput()
    {
        if (!canLaunch || ballCount <= 0) return;
        
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            trajectoryLine.gameObject.SetActive(true);
            Debug.Log("GameManager.HandleInput() - Mouse Down."); // [DEBUG]
        }
        
        if (isDragging)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            
            launchDirection = (mousePos - launchPosition).normalized;
            if (launchDirection.y < 0.1f) launchDirection.y = 0.1f;
            launchDirection = launchDirection.normalized;
            
            trajectoryLine.SetPosition(0, launchPosition);
            trajectoryLine.SetPosition(1, launchPosition + new Vector3(launchDirection.x, launchDirection.y, 0) * 5f);
            Debug.Log("GameManager.HandleInput() - Dragging. Launch direction: " + launchDirection); // [DEBUG]
        }
        
        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            trajectoryLine.gameObject.SetActive(false);
            LaunchBall();
            Debug.Log("GameManager.HandleInput() - Mouse Up, LaunchBall() called."); // [DEBUG]
        }
    }
        
    void LaunchBall()
    {
        if (ballCount > 0)
        {
            StartCoroutine(LaunchBallsSequentially());
            canLaunch = false;
            Debug.Log("GameManager.LaunchBall() called. Starting coroutine."); // [DEBUG]
        }
    }
    
    IEnumerator LaunchBallsSequentially()
    {
        if (ballCount > 0)
        {
            GameObject ball = Instantiate(ballPrefab, launchPosition, Quaternion.identity);
            Ball ballScript = ball.GetComponent<Ball>();
            ballScript.Initialize(launchDirection * ballSpeed, this);
            activeBalls.Add(ball);
            ballCount--;
            UpdateBallCountText();
            Debug.Log("GameManager.LaunchBallsSequentially() - Ball launched. Ball count: " + ballCount + ", Active balls: " + activeBalls.Count); // [DEBUG]

            yield return new WaitUntil(() => ballScript.HasCollided());

            BallReturned(ball);
            Debug.Log("GameManager.LaunchBallsSequentially() - Ball returned in coroutine."); // [DEBUG]
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
        if (blockContainer == null) 
        {
            Debug.LogError("Block container is not assigned!");
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
                block.transform.SetParent(blockContainer, true);
                
                Block blockScript = block.GetComponent<Block>();
                
                int value = Random.Range(1, 4) + score / 10;
                Color color = blockColors[Random.Range(0, blockColors.Length)];
                
                blockScript.Initialize(value, color);
            }
        }
    }
    
    void MoveBlocksDown()
    {
        float spacing = 0.2f;
        
        foreach (Transform block in blockContainer)
        {
            block.position += new Vector3(0, -(blockSize + spacing), 0);
        }
    }
    public void BallReturned(GameObject ball)
    {
        Debug.Log("GameManager.BallReturned() called. Ball: " + ball.name + ", Active balls before remove: " + activeBalls.Count); // [DEBUG]
        activeBalls.Remove(ball);
        Destroy(ball);
        Debug.Log("GameManager.BallReturned() - Ball removed and destroyed. Active balls after remove: " + activeBalls.Count); // [DEBUG]
        
        if (ballCount > 0)
        {
            canLaunch = true;
            Debug.Log("GameManager.BallReturned() - Can launch again."); // [DEBUG]
        }
        else
        {
            Debug.Log("GameManager.BallReturned() - Out of balls, game over!");
        }
    }
    
    public void BlockDestroyed(int value)
    {
        Debug.Log("GameManager.BlockDestroyed() called. Value: " + value); // [DEBUG]
        score += value;
        scoreText.text = score.ToString();
    }
    
    void UpdateBallCountText()
    {
        ballCountText.text = ballCount.ToString();
    }

    void OnDestroy()
    {
        Debug.Log("GameManager.OnDestroy() called."); // [DEBUG]
    }
}

using UnityEngine;
using TMPro;

public class Block : MonoBehaviour
{
    public int hitPoints;
    public TextMeshPro valueText;
    public SpriteRenderer spriteRenderer;
    
    private GameManager gameManager;
    private int initialValue; // Pour conserver la valeur initiale pour le score
    
    void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        Debug.Log("Block.Start() called. GameManager: " + gameManager);
    }
    
    public void Initialize(int value, Color color)
    {
        hitPoints = value;
        initialValue = value; // Sauvegarde la valeur initiale
        if (valueText != null)
        {
            valueText.text = value.ToString();
        }
        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }
        else
        {
            Debug.LogWarning("SpriteRenderer not found on Block");
        }
        Debug.Log("Block.Initialize() called. hitPoints: " + hitPoints + ", color: " + color);
    }
    
    public void TakeDamage()
    {
        hitPoints--;
        if (valueText != null)
        {
            valueText.text = hitPoints.ToString();
        }
        Debug.Log("Block.TakeDamage() called. hitPoints: " + hitPoints);
        
        if (hitPoints <= 0)
        {
            if (gameManager != null)
                gameManager.BlockDestroyed(initialValue); // Utilise la valeur initiale pour le score
            
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        Debug.Log("Block.OnDestroy() called. Block destroyed: " + gameObject.name);
    }
}
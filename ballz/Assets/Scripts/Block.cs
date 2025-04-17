using UnityEngine;
using TMPro;

public class Block : MonoBehaviour
{
    public int hitPoints;
    public TextMeshPro valueText;
    public SpriteRenderer spriteRenderer;
    
    private GameManager gameManager;
    
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }
    
    public void Initialize(int value, Color color)
    {
        hitPoints = value;
        valueText.text = value.ToString();
        spriteRenderer.color = color;
    }
    
    public void TakeDamage()
    {
        hitPoints--;
        valueText.text = hitPoints.ToString();
        
        if (hitPoints <= 0)
        {
            gameManager.BlockDestroyed(hitPoints);
            Destroy(gameObject);
        }
    }
    
}
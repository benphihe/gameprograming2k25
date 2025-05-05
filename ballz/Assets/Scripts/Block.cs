using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Block : MonoBehaviour
{
    public int health = 1;
    public TextMeshPro valueText;
    public SpriteRenderer blockRenderer;
    private GameManager gameManager;
    
    void Start()
    {
        Debug.Log("Block.Start() called. GameManager: " + FindObjectOfType<GameManager>());
        gameManager = FindObjectOfType<GameManager>();
        
        // Assurez-vous que les composants nécessaires sont présents
        if (valueText == null)
        {
            Debug.LogWarning("TextMeshPro manquant sur le block!");
        }
        
        if (blockRenderer == null)
        {
            blockRenderer = GetComponent<SpriteRenderer>();
            if (blockRenderer == null)
            {
                Debug.LogWarning("SpriteRenderer manquant sur le block!");
            }
        }
    }
    
    public void Initialize(int value, Color color)
    {
        health = value;
        
        // Mettre à jour le texte si le TextMeshPro est présent
        if (valueText != null)
        {
            valueText.text = value.ToString();
        }
        
        // Mettre à jour la couleur si le SpriteRenderer est présent
        if (blockRenderer != null)
        {
            blockRenderer.color = color;
        }
    }
    
    public void TakeDamage()
    {
        health--;
        
        if (health <= 0)
        {
            // Informer le GameManager que le bloc est détruit
            if (gameManager != null)
            {
                gameManager.BlockDestroyed(1);
            }
            
            Destroy(gameObject);
        }
        else
        {
            // Mettre à jour le texte
            if (valueText != null)
            {
                valueText.text = health.ToString();
            }
        }
    }
}
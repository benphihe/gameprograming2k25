using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Block : MonoBehaviour
{
    private int health;
    private int maxHealth;
    private SpriteRenderer spriteRenderer;
    private TextMeshPro healthText;
    private Color[] healthColors = new Color[] {
        new Color(0.95f, 0.3f, 0.6f),  // Rose pour 1 PV
        new Color(0.3f, 0.7f, 0.9f),   // Bleu pour 2 PV
        new Color(1f, 0.85f, 0.2f)     // Jaune pour 3 PV
    };

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer not found on Block!");
            return;
        }

        // Créer le texte pour afficher les points de vie
        GameObject textObj = new GameObject("HealthText");
        textObj.transform.SetParent(transform);
        textObj.transform.localPosition = Vector3.zero;
        healthText = textObj.AddComponent<TextMeshPro>();
        healthText.alignment = TextAlignmentOptions.Center;
        healthText.fontSize = 3;
        healthText.color = Color.white;
        healthText.sortingOrder = 1;

        // Vérifier que le GameManager existe
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager instance not found in Block.Awake!");
        }
    }

    public void Initialize(int value, Color color)
    {
        maxHealth = Mathf.Clamp(value, 1, 3);
        health = maxHealth;
        UpdateVisuals();
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.Log($"Block took {damage} damage. Health remaining: {health}");
        UpdateVisuals();
        
        if (health <= 0)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.BlockDestroyed(maxHealth);
            }
            Destroy(gameObject);
        }
    }

    private void UpdateVisuals()
    {
        // Mettre à jour la couleur en fonction des points de vie
        if (health > 0 && health <= healthColors.Length)
        {
            spriteRenderer.color = healthColors[health - 1];
        }
        
        // Mettre à jour le texte des points de vie
        if (healthText != null)
        {
            healthText.text = health.ToString();
        }
    }

    private void UpdateColor()
    {
        // Mettre à jour la couleur en fonction des points de vie
        spriteRenderer.color = healthColors[health - 1];
        
        // Mettre à jour le texte des points de vie
        if (healthText != null)
        {
            healthText.text = health.ToString();
        }
    }
}
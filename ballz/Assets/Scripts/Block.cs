using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Block : MonoBehaviour
{
    private int health;
    private TextMeshPro textMesh;

    void Start()
    {
        // Récupérer le composant TextMeshPro sur l'enfant
        textMesh = GetComponentInChildren<TextMeshPro>();
        if (textMesh == null)
        {
            Debug.LogWarning("TextMeshPro manquant sur le block!");
        }
    }

    public void Initialize(int value, Color color)
    {
        health = value;

        // Mettre à jour le texte et la couleur
        if (textMesh != null)
        {
            textMesh.text = health.ToString();
            textMesh.color = color;
        }

        // Appliquer la couleur au sprite du bloc
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;

        // Mettre à jour le texte
        if (textMesh != null)
        {
            textMesh.text = health.ToString();
        }

        // Détruire le bloc si les points de vie tombent à 0 ou moins
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
}
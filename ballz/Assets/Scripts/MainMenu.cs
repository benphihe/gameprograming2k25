using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartNewGame()
    {
        SceneManager.LoadScene("GameScene"); // Remplacez "GameScene" par le nom de votre scène de jeu
    }

    public void OpenLevelSelection()
    {
        SceneManager.LoadScene("LevelSelection"); // Remplacez par le nom de votre scène de sélection de niveau
    }

    public void OpenCredits()
    {
        SceneManager.LoadScene("Credits"); // Remplacez par le nom de votre scène de crédits
    }

    public void QuitGame()
    {
        #if UNITY_WEBGL
            Debug.Log("Quitter le jeu n'est pas disponible sur WebGL.");
        #else
            Application.Quit();
        #endif
    }
}
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartNewGame()
    {
        SceneManager.LoadScene("MainScene");
    }

    public void OpenLevelSelection()
    {
        SceneManager.LoadScene("LevelSelection");
    }

    public void OpenCredits()
    {
        SceneManager.LoadScene("Credits");
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
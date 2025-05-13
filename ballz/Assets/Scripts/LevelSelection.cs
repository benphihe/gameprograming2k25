using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelection : MonoBehaviour
{
    public void LoadLevel(string levelName)
    {
        SceneManager.LoadScene(levelName); // Chargez la scène correspondant au niveau
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu"); // Retour au menu principal
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProgressionManager : MonoBehaviour
{
    public static ProgressionManager Instance { get; private set; }

    [Header("Progression System")]
    public int progressionPoints = 0;
    public float ballDamageMultiplier = 1f;
    public float ballSizeMultiplier = 1f;
    public float ballSpeedMultiplier = 1f;
    public int extraBallsPerRun = 0;
    public float powerUpDropRateMultiplier = 1f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        ResetProgression();
    }

    public void SaveProgression()
    {
        Debug.Log($"Sauvegarde de la progression - Points: {progressionPoints}");
        PlayerPrefs.SetInt("ProgressionPoints", progressionPoints);
        PlayerPrefs.SetFloat("BallDamageMultiplier", ballDamageMultiplier);
        PlayerPrefs.SetFloat("BallSizeMultiplier", ballSizeMultiplier);
        PlayerPrefs.SetFloat("BallSpeedMultiplier", ballSpeedMultiplier);
        PlayerPrefs.SetInt("ExtraBallsPerRun", extraBallsPerRun);
        PlayerPrefs.SetFloat("PowerUpDropRateMultiplier", powerUpDropRateMultiplier);
        PlayerPrefs.Save();
    }

    public void LoadProgression()
    {
        Debug.Log("Chargement de la progression");
        progressionPoints = PlayerPrefs.GetInt("ProgressionPoints", 0);
        ballDamageMultiplier = PlayerPrefs.GetFloat("BallDamageMultiplier", 1f);
        ballSizeMultiplier = PlayerPrefs.GetFloat("BallSizeMultiplier", 1f);
        ballSpeedMultiplier = PlayerPrefs.GetFloat("BallSpeedMultiplier", 1f);
        extraBallsPerRun = PlayerPrefs.GetInt("ExtraBallsPerRun", 0);
        powerUpDropRateMultiplier = PlayerPrefs.GetFloat("PowerUpDropRateMultiplier", 1f);
        Debug.Log($"Progression chargée - Points: {progressionPoints}");
    }

    public void ResetProgression()
    {
        Debug.Log("Réinitialisation de la progression");
        progressionPoints = 0;
        ballDamageMultiplier = 1f;
        ballSizeMultiplier = 1f;
        ballSpeedMultiplier = 1f;
        extraBallsPerRun = 0;
        powerUpDropRateMultiplier = 1f;
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("Progression réinitialisée - Points: 0");
    }
} 
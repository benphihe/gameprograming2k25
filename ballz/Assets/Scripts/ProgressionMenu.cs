using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProgressionMenu : MonoBehaviour
{
    [Header("UI Elements")]
    [Tooltip("Texte affichant les points de progression")]
    public TextMeshProUGUI progressionPointsText;
    
    [Tooltip("Boutons pour chaque amélioration")]
    public Button[] upgradeButtons;
    
    [Tooltip("Textes affichant le niveau de chaque amélioration")]
    public TextMeshProUGUI[] upgradeLevelTexts;

    private ProgressionManager progressionManager;
    private const int UPGRADE_COST = 1; // Coût fixe de 1 point pour chaque amélioration

    void Awake()
    {
        Debug.Log("ProgressionMenu.Awake() appelé");
        
        // Vérifier que tous les tableaux sont initialisés
        if (upgradeButtons == null)
        {
            Debug.LogError("Le tableau upgradeButtons est null!");
            upgradeButtons = new Button[5];
        }
        else if (upgradeButtons.Length != 5)
        {
            Debug.LogError($"Le tableau upgradeButtons a une taille incorrecte: {upgradeButtons.Length} au lieu de 5");
            upgradeButtons = new Button[5];
        }

        if (upgradeLevelTexts == null)
        {
            Debug.LogError("Le tableau upgradeLevelTexts est null!");
            upgradeLevelTexts = new TextMeshProUGUI[5];
        }

        // Vérifier chaque bouton
        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            if (upgradeButtons[i] == null)
            {
                Debug.LogError($"Le bouton {i} n'est pas assigné dans l'inspecteur!");
            }
            else
            {
                Debug.Log($"Bouton {i} trouvé: {upgradeButtons[i].name}");
                // Vérifier que le bouton a un composant Button
                Button buttonComponent = upgradeButtons[i].GetComponent<Button>();
                if (buttonComponent == null)
                {
                    Debug.LogError($"Le bouton {i} n'a pas de composant Button!");
                }
            }
        }

        // Obtenir le ProgressionManager
        progressionManager = ProgressionManager.Instance;
        if (progressionManager == null)
        {
            Debug.LogError("ProgressionManager instance not found! Make sure it exists in the scene.");
        }
        else
        {
            Debug.Log("ProgressionManager trouvé!");
        }
    }

    void Start()
    {
        Debug.Log("ProgressionMenu.Start() appelé");
        if (progressionManager != null)
        {
            progressionManager.LoadProgression();
            UpdateUI();
        }
        else
        {
            Debug.LogError("ProgressionManager est null dans Start!");
        }
    }

    void UpdateUI()
    {
        Debug.Log("ProgressionMenu.UpdateUI() appelé");
        if (progressionManager == null)
        {
            Debug.LogError("ProgressionManager est null dans UpdateUI!");
            return;
        }

        // Mettre à jour le texte des points de progression
        if (progressionPointsText != null)
        {
            progressionPointsText.text = $"Points disponibles : {progressionManager.progressionPoints}";
            Debug.Log($"Points de progression mis à jour: {progressionManager.progressionPoints}");
        }
        else
        {
            Debug.LogError("progressionPointsText n'est pas assigné!");
        }

        // Mettre à jour les boutons d'amélioration
        UpdateUpgradeButtons();
    }

    void UpdateUpgradeButtons()
    {
        Debug.Log("ProgressionMenu.UpdateUpgradeButtons() appelé");
        if (progressionManager == null)
        {
            Debug.LogError("ProgressionManager est null dans UpdateUpgradeButtons!");
            return;
        }

        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            if (upgradeButtons[i] == null)
            {
                Debug.LogError($"Le bouton {i} est null dans UpdateUpgradeButtons!");
                continue;
            }

            Button button = upgradeButtons[i].GetComponent<Button>();
            if (button == null)
            {
                Debug.LogError($"Le bouton {i} n'a pas de composant Button!");
                continue;
            }

            bool canAfford = progressionManager.progressionPoints >= UPGRADE_COST;
            button.interactable = canAfford;
            Debug.Log($"Bouton {i} ({upgradeButtons[i].name}) interactable: {canAfford}");
        }
    }

    public void UpgradeBallDamage()
    {
        Debug.Log("UpgradeBallDamage appelé");
        if (progressionManager == null)
        {
            Debug.LogError("ProgressionManager est null dans UpgradeBallDamage!");
            return;
        }

        if (progressionManager.progressionPoints >= UPGRADE_COST)
        {
            Debug.Log("Amélioration achetée: Dégâts de balle");
            progressionManager.ballDamageMultiplier += 0.1f;
            progressionManager.progressionPoints -= UPGRADE_COST;
            progressionManager.SaveProgression();
            UpdateUI();
        }
        else
        {
            Debug.Log("Pas assez de points pour l'amélioration");
        }
    }

    public void UpgradeBallSize()
    {
        Debug.Log("UpgradeBallSize appelé");
        if (progressionManager == null)
        {
            Debug.LogError("ProgressionManager est null dans UpgradeBallSize!");
            return;
        }

        if (progressionManager.progressionPoints >= UPGRADE_COST)
        {
            Debug.Log("Amélioration achetée: Taille de balle");
            progressionManager.ballSizeMultiplier -= 0.1f;
            progressionManager.progressionPoints -= UPGRADE_COST;
            progressionManager.SaveProgression();
            UpdateUI();
        }
        else
        {
            Debug.Log("Pas assez de points pour l'amélioration");
        }
    }

    public void UpgradeBallSpeed()
    {
        Debug.Log("UpgradeBallSpeed appelé");
        if (progressionManager == null)
        {
            Debug.LogError("ProgressionManager est null dans UpgradeBallSpeed!");
            return;
        }

        if (progressionManager.progressionPoints >= UPGRADE_COST)
        {
            Debug.Log("Amélioration achetée: Vitesse de balle");
            progressionManager.ballSpeedMultiplier += 0.1f;
            progressionManager.progressionPoints -= UPGRADE_COST;
            progressionManager.SaveProgression();
            UpdateUI();
        }
        else
        {
            Debug.Log("Pas assez de points pour l'amélioration");
        }
    }

    public void UpgradeExtraBalls()
    {
        Debug.Log("UpgradeExtraBalls appelé");
        if (progressionManager == null)
        {
            Debug.LogError("ProgressionManager est null dans UpgradeExtraBalls!");
            return;
        }

        if (progressionManager.progressionPoints >= UPGRADE_COST)
        {
            Debug.Log("Amélioration achetée: Balles supplémentaires");
            progressionManager.extraBallsPerRun++;
            progressionManager.progressionPoints -= UPGRADE_COST;
            progressionManager.SaveProgression();
            UpdateUI();
        }
        else
        {
            Debug.Log("Pas assez de points pour l'amélioration");
        }
    }

    public void UpgradePowerUpRate()
    {
        Debug.Log("UpgradePowerUpRate appelé");
        if (progressionManager == null)
        {
            Debug.LogError("ProgressionManager est null dans UpgradePowerUpRate!");
            return;
        }

        if (progressionManager.progressionPoints >= UPGRADE_COST)
        {
            Debug.Log("Amélioration achetée: Taux de power-ups");
            progressionManager.powerUpDropRateMultiplier += 0.1f;
            progressionManager.progressionPoints -= UPGRADE_COST;
            progressionManager.SaveProgression();
            UpdateUI();
        }
        else
        {
            Debug.Log("Pas assez de points pour l'amélioration");
        }
    }

    public void StartNewRun()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
    }

    public void ReturnToMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
} 
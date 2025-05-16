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
    private const int DAMAGE_UPGRADE_COST = 3; // Coût de 3 points pour l'amélioration des dégâts
    private const int STANDARD_UPGRADE_COST = 1; // Coût de 1 point pour les autres améliorations

    // Niveaux actuels des améliorations
    private int ballDamageLevel = 0;
    private int ballSizeLevel = 0;
    private int ballSpeedLevel = 0;
    private int extraBallsLevel = 0;
    private int powerUpRateLevel = 0;

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

        // Obtenir ou créer le ProgressionManager
        if (ProgressionManager.Instance == null)
        {
            Debug.Log("Création d'une nouvelle instance de ProgressionManager");
            GameObject progressionManagerObj = new GameObject("ProgressionManager");
            progressionManager = progressionManagerObj.AddComponent<ProgressionManager>();
        }
        else
        {
            progressionManager = ProgressionManager.Instance;
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
            progressionPointsText.text = $"Points : {progressionManager.progressionPoints}";
            Debug.Log($"Points de progression mis à jour: {progressionManager.progressionPoints}");
        }
        else
        {
            Debug.LogError("progressionPointsText n'est pas assigné!");
        }

        // Mettre à jour les textes des niveaux
        if (upgradeLevelTexts != null)
        {
            if (upgradeLevelTexts.Length > 0 && upgradeLevelTexts[0] != null) 
                upgradeLevelTexts[0].text = $"Niveau {ballDamageLevel}";
            if (upgradeLevelTexts.Length > 1 && upgradeLevelTexts[1] != null) 
                upgradeLevelTexts[1].text = $"Niveau {ballSizeLevel}";
            if (upgradeLevelTexts.Length > 2 && upgradeLevelTexts[2] != null) 
                upgradeLevelTexts[2].text = $"Niveau {ballSpeedLevel}";
            if (upgradeLevelTexts.Length > 3 && upgradeLevelTexts[3] != null) 
                upgradeLevelTexts[3].text = $"Niveau {extraBallsLevel}";
            if (upgradeLevelTexts.Length > 4 && upgradeLevelTexts[4] != null) 
                upgradeLevelTexts[4].text = $"Niveau {powerUpRateLevel}";
        }
        else
        {
            Debug.LogError("upgradeLevelTexts n'est pas assigné!");
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

        if (upgradeButtons == null)
        {
            Debug.LogError("upgradeButtons n'est pas assigné!");
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

            // Vérifier si on a assez de points pour l'amélioration
            int cost = (i == 0) ? DAMAGE_UPGRADE_COST : STANDARD_UPGRADE_COST; // Le premier bouton (index 0) est pour les dégâts
            bool canAfford = progressionManager.progressionPoints >= cost;
            Debug.Log($"Bouton {i}: Points disponibles = {progressionManager.progressionPoints}, Coût = {cost}, Peut acheter = {canAfford}");
            
            button.interactable = canAfford;
            
            // Changer la couleur du bouton en fonction de l'état
            ColorBlock colors = button.colors;
            colors.normalColor = canAfford ? Color.white : Color.gray;
            colors.disabledColor = Color.gray;
            button.colors = colors;
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

        if (progressionManager.progressionPoints >= DAMAGE_UPGRADE_COST)
        {
            Debug.Log($"Amélioration achetée: Dégâts de balle (Coût: {DAMAGE_UPGRADE_COST}, Points restants: {progressionManager.progressionPoints - DAMAGE_UPGRADE_COST})");
            progressionManager.ballDamageMultiplier += 1f;
            progressionManager.progressionPoints -= DAMAGE_UPGRADE_COST;
            ballDamageLevel++;
            progressionManager.SaveProgression();
            UpdateUI();
        }
        else
        {
            Debug.Log($"Pas assez de points pour l'amélioration (Points: {progressionManager.progressionPoints}, Coût: {DAMAGE_UPGRADE_COST})");
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

        if (progressionManager.progressionPoints >= STANDARD_UPGRADE_COST)
        {
            Debug.Log($"Amélioration achetée: Taille de balle (Coût: {STANDARD_UPGRADE_COST}, Points restants: {progressionManager.progressionPoints - STANDARD_UPGRADE_COST})");
            progressionManager.ballSizeMultiplier -= 0.1f;
            progressionManager.progressionPoints -= STANDARD_UPGRADE_COST;
            ballSizeLevel++;
            progressionManager.SaveProgression();
            UpdateUI();
        }
        else
        {
            Debug.Log($"Pas assez de points pour l'amélioration (Points: {progressionManager.progressionPoints}, Coût: {STANDARD_UPGRADE_COST})");
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

        if (progressionManager.progressionPoints >= STANDARD_UPGRADE_COST)
        {
            Debug.Log($"Amélioration achetée: Vitesse de balle (Coût: {STANDARD_UPGRADE_COST}, Points restants: {progressionManager.progressionPoints - STANDARD_UPGRADE_COST})");
            progressionManager.ballSpeedMultiplier += 0.1f;
            progressionManager.progressionPoints -= STANDARD_UPGRADE_COST;
            ballSpeedLevel++;
            progressionManager.SaveProgression();
            UpdateUI();
        }
        else
        {
            Debug.Log($"Pas assez de points pour l'amélioration (Points: {progressionManager.progressionPoints}, Coût: {STANDARD_UPGRADE_COST})");
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

        if (progressionManager.progressionPoints >= STANDARD_UPGRADE_COST)
        {
            Debug.Log($"Amélioration achetée: Balles supplémentaires (Coût: {STANDARD_UPGRADE_COST}, Points restants: {progressionManager.progressionPoints - STANDARD_UPGRADE_COST})");
            progressionManager.extraBallsPerRun++;
            progressionManager.progressionPoints -= STANDARD_UPGRADE_COST;
            extraBallsLevel++;
            progressionManager.SaveProgression();
            UpdateUI();
        }
        else
        {
            Debug.Log($"Pas assez de points pour l'amélioration (Points: {progressionManager.progressionPoints}, Coût: {STANDARD_UPGRADE_COST})");
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

        if (progressionManager.progressionPoints >= STANDARD_UPGRADE_COST)
        {
            Debug.Log($"Amélioration achetée: Taux de power-ups (Coût: {STANDARD_UPGRADE_COST}, Points restants: {progressionManager.progressionPoints - STANDARD_UPGRADE_COST})");
            progressionManager.powerUpDropRateMultiplier += 0.1f;
            progressionManager.progressionPoints -= STANDARD_UPGRADE_COST;
            powerUpRateLevel++;
            progressionManager.SaveProgression();
            UpdateUI();
        }
        else
        {
            Debug.Log($"Pas assez de points pour l'amélioration (Points: {progressionManager.progressionPoints}, Coût: {STANDARD_UPGRADE_COST})");
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
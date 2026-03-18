using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStats : MonoBehaviour
{
    CharacterScriptableObject characterData;

    float currentHealth;
    float currentRecovery;
    float currentMoveSpeed;
    float currentMight;
    float currentProjectileSpeed;
    float currentMagnet;

    #region Current Stats Properties
    public float CurrentHealth
    {
        get { return currentHealth; }
        set
        {
            if (currentHealth != value)
            {
                currentHealth = value;
                if (GameManager.instance != null && GameManager.instance.currentHealthDisplay != null)
                {
                    GameManager.instance.currentHealthDisplay.text = "Health: " + Mathf.RoundToInt(currentHealth).ToString();
                }
            }
        }
    }

    public float CurrentRecovery
    {
        get { return currentRecovery; }
        set
        {
            if (currentRecovery != value)
            {
                currentRecovery = value;
                if (GameManager.instance != null && GameManager.instance.currentRecoveryDisplay != null)
                {
                    GameManager.instance.currentRecoveryDisplay.text = "Recovery: " + currentRecovery.ToString("F1");
                }
            }
        }
    }

    public float CurrentMoveSpeed
    {
        get { return currentMoveSpeed; }
        set
        {
            if (currentMoveSpeed != value)
            {
                currentMoveSpeed = value;
                if (GameManager.instance != null && GameManager.instance.currentMoveSpeedDisplay != null)
                {
                    GameManager.instance.currentMoveSpeedDisplay.text = "Move Speed: " + currentMoveSpeed.ToString("F1");
                }
            }
        }
    }

    public float CurrentMight
    {
        get { return currentMight; }
        set
        {
            if (currentMight != value)
            {
                currentMight = value;
                if (GameManager.instance != null && GameManager.instance.currentMightDisplay != null)
                {
                    GameManager.instance.currentMightDisplay.text = "Might: " + currentMight.ToString("F1");
                }
            }
        }
    }

    public float CurrentProjectileSpeed
    {
        get { return currentProjectileSpeed; }
        set
        {
            if (currentProjectileSpeed != value)
            {
                currentProjectileSpeed = value;
                if (GameManager.instance != null && GameManager.instance.currentProjectileSpeedDisplay != null)
                {
                    GameManager.instance.currentProjectileSpeedDisplay.text = "Projectile Speed: " + currentProjectileSpeed.ToString("F1");
                }
            }
        }
    }

    public float CurrentMagnet
    {
        get { return currentMagnet; }
        set
        {
            if (currentMagnet != value)
            {
                currentMagnet = value;
                if (GameManager.instance != null && GameManager.instance.currentMagnetDisplay != null)
                {
                    GameManager.instance.currentMagnetDisplay.text = "Magnet: " + currentMagnet.ToString("F1");
                }
            }
        }
    }
    #endregion

    [Header("Experience/Level")]
    public int experience = 0;
    public int level = 1;
    public int experienceCap;

    [System.Serializable]
    public class LevelRange
    {
        public int startLevel;
        public int endLevel;
        public int experienceCapIncrease;
    }

    [Header("I-Frames")]
    public float iFrameDuration;
    float iFrameTimer;
    bool isInvincible;

    public List<LevelRange> levelRanges;

    InventoryManager inventory;
    public int weaponIndex;
    public int passiveItemIndex;

    [Header("UI")]
    public Image healthBar;
    public Image expBar;
    public TextMeshProUGUI levelText;


    void Awake()
    {
        characterData = CharacterSelector.GetData();

        if(characterData == null)
        {
            Debug.LogError("No character data found! Make sure to select a character in the Character Selector scene.");
            return;
        }
        if (CharacterSelector.instance != null)
        {
            CharacterSelector.instance.DestroySingleton();
        }

        inventory = GetComponent<InventoryManager>();

        CurrentHealth = characterData.MaxHealth;
        CurrentRecovery = characterData.Recovery;
        CurrentMoveSpeed = characterData.MoveSpeed;
        CurrentMight = characterData.Might;
        CurrentProjectileSpeed = characterData.ProjectileSpeed;
        CurrentMagnet = characterData.Magnet;

        SpawnWeapon(characterData.StartingWeapon);
    }

    void Start()
    {
        experienceCap = levelRanges[0].experienceCapIncrease;

        GameManager.instance.currentHealthDisplay.text = "Health: " + Mathf.RoundToInt(CurrentHealth).ToString();
        GameManager.instance.currentRecoveryDisplay.text = "Recovery: " + CurrentRecovery.ToString("F1");
        GameManager.instance.currentMoveSpeedDisplay.text = "Move Speed: " + CurrentMoveSpeed.ToString("F1");
        GameManager.instance.currentMightDisplay.text = "Might: " + CurrentMight.ToString("F1");
        GameManager.instance.currentProjectileSpeedDisplay.text = "Projectile Speed: " + CurrentProjectileSpeed.ToString("F1");
        GameManager.instance.currentMagnetDisplay.text = "Magnet: " + CurrentMagnet.ToString("F1");

        GameManager.instance.AssignChosenCharacterUI(characterData);

        UpdateHealthBar();
        UpdateExpBar();
        UpdateLevelText();
    }

    void Update()
    {
        if (iFrameTimer > 0)
        {
            iFrameTimer -= Time.deltaTime;
            if (iFrameTimer <= 0)
            {
                isInvincible = false;
            }
        }
        Recover();
    }

    public void IncreaseExperience(int amount)
    {
        experience += amount;
        LevelUpChecker();

        UpdateExpBar();
    }

    void LevelUpChecker()
    {
        if (experience >= experienceCap)
        {
            experience -= experienceCap;
            level++;

            int experienceCapIncrease = 0;

            if (levelRanges != null)
            {
                foreach (LevelRange range in levelRanges)
                {
                    if (level >= range.startLevel && level <= range.endLevel)
                    {
                        experienceCapIncrease = range.experienceCapIncrease;
                        break;
                    }
                }
            }
            
            experienceCap += experienceCapIncrease;

            UpdateLevelText();

            if (GameManager.instance != null)
            {
                GameManager.instance.StartLevelUp();
            }
        }
    }

    void UpdateExpBar()
    {
        if(expBar != null)
        {
            expBar.fillAmount = (float)experience / experienceCap;
        }
    }

    void UpdateLevelText()
    {
        if (levelText != null)
        {
            levelText.text = "LV " + level.ToString();
        }
    }

    public void TakeDamage(float damage)
    {
        if (!isInvincible)
        {
            CurrentHealth -= damage;

            iFrameTimer = iFrameDuration;
            isInvincible = true;

            if (CurrentHealth <= 0)
            {
                Die();
            }
        }
        UpdateHealthBar();
    }

    void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.fillAmount = CurrentHealth / characterData.MaxHealth;
        }
    }

    public void Die()
    {
        if (!GameManager.instance.isGameOver)
        {
            GameManager.instance.AssignLevelReachedUI(level);
            GameManager.instance.AssignChosenWeaponsAndPassiveItemsUI(inventory.weaponUISlots, inventory.passiveItemUISlots);
            GameManager.instance.GameOver();
        }
    }

    public void Heal(float amount)
    {
        if (characterData == null) return;
        if (CurrentHealth < characterData.MaxHealth)
        {
            CurrentHealth += amount;

            if (CurrentHealth > characterData.MaxHealth)
            {
                CurrentHealth = characterData.MaxHealth;
            }
        }
    }

    void Recover()
    {
        if (CurrentHealth < characterData.MaxHealth)
        {
            CurrentHealth += CurrentRecovery * Time.deltaTime;

            if (CurrentHealth > characterData.MaxHealth)
            {
                CurrentHealth = characterData.MaxHealth;
            }
        }
    }

    public void SpawnWeapon(GameObject weaponPrefab)
    {
        if (weaponIndex >= inventory.weaponSlots.Count)
        {
            Debug.LogError("No more weapon slots available!");
            return;
        }

        GameObject spawnedWeapon = Instantiate(weaponPrefab, transform.position, Quaternion.identity);
        spawnedWeapon.transform.SetParent(transform);
        inventory.AddWeapon(weaponIndex, spawnedWeapon.GetComponent<WeaponController>());

        weaponIndex++;
    }

    public void SpawnPassiveItem(GameObject passiveItemPrefab)
    {
        if (passiveItemIndex >= inventory.passiveItemSlots.Count)
        {
            Debug.LogError("No more passive item slots available!");
            return;
        }

        GameObject spawnedPassiveItem = Instantiate(passiveItemPrefab, transform.position, Quaternion.identity);
        spawnedPassiveItem.transform.SetParent(transform);
        inventory.AddPassiveItem(passiveItemIndex, spawnedPassiveItem.GetComponent<PassiveItem>());

        passiveItemIndex++;
    }
}

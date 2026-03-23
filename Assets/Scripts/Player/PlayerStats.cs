using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStats : MonoBehaviour
{
    CharacterData characterData;
    public CharacterData.Stats baseStats;
    [SerializeField] CharacterData.Stats actualStats;

    float health;

    #region Current Stats Properties
    public float CurrentHealth
    {
        get { return health; }
        set
        {
            if (health != value)
            {
                health = value;
                if (GameManager.instance != null && GameManager.instance.currentHealthDisplay != null)
                {
                    GameManager.instance.currentHealthDisplay.text = string.Format(
                        "Health: {0} / {1}",
                        health, actualStats.maxHealth
                    );
                }
            }
        }
    }

    public float MaxHealth
    {
        get { return actualStats.maxHealth; }
        set
        {
            if(actualStats.maxHealth != value)
            {
                actualStats.maxHealth = value;
                if (GameManager.instance != null && GameManager.instance.currentHealthDisplay != null)
                {
                    GameManager.instance.currentHealthDisplay.text = string.Format(
                        "Health: {0} / {1}",
                        health, actualStats.maxHealth
                    );
                }
            }
        }
    }

    public float CurrentRecovery
    {
        get { return Recovery; }
        set { Recovery = value; }
    }
    public float Recovery
    {
        get { return actualStats.recovery; }
        set
        {
            if(actualStats.recovery != value)
            {
                actualStats.recovery = value;
                if (GameManager.instance != null && GameManager.instance.currentRecoveryDisplay != null)
                {
                    GameManager.instance.currentRecoveryDisplay.text = "Recovery: " + actualStats.recovery.ToString("F1");
                }
            }
        }
    }

    public float CurrentMoveSpeed
    {
        get { return MoveSpeed; }
        set { MoveSpeed = value; }
    }
    public float MoveSpeed
    {
        get { return actualStats.moveSpeed; }
        set
        {
            if (actualStats.moveSpeed != value)
            {
                actualStats.moveSpeed = value;
                if (GameManager.instance != null && GameManager.instance.currentMoveSpeedDisplay != null)
                {
                    GameManager.instance.currentMoveSpeedDisplay.text = "Move Speed: " + actualStats.moveSpeed.ToString("F1");
                }
            }
        }
    }

    public float CurrentMight
    {
        get { return Might; }
        set { Might = value; }
    }
    public float Might
    {
        get { return actualStats.might; }
        set
        {
            if (actualStats.might != value)
            {
                actualStats.might = value;
                if (GameManager.instance != null && GameManager.instance.currentMightDisplay != null)
                {
                    GameManager.instance.currentMightDisplay.text = "Might: " + actualStats.might.ToString("F1");
                }
            }
        }
    }

    public float CurrentProjectileSpeed
    {
        get { return ProjectileSpeed; }
        set { ProjectileSpeed = value; }
    }
    public float ProjectileSpeed
    {
        get { return actualStats.speed; }
        set
        {
            if (actualStats.speed != value)
            {
                actualStats.speed = value;
                if (GameManager.instance != null && GameManager.instance.currentProjectileSpeedDisplay != null)
                {
                    GameManager.instance.currentProjectileSpeedDisplay.text = "Projectile Speed: " + actualStats.speed.ToString("F1");
                }
            }
        }
    }

    public float CurrentMagnet
    {
        get { return Magnet; }
        set { Magnet = value; }
    }
    public float Magnet
    {
        get { return actualStats.magnet; }
        set
        {
            if (actualStats.magnet != value)
            {
                actualStats.magnet = value;
                if (GameManager.instance != null && GameManager.instance.currentMagnetDisplay != null)
                {
                    GameManager.instance.currentMagnetDisplay.text = "Magnet: " + actualStats.magnet.ToString("F1");
                }
            }
        }
    }
    #endregion

    public ParticleSystem damageEffect;

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

    PlayerInventory inventory;
    public int weaponIndex;
    public int passiveItemIndex;

    [Header("UI")]
    public Image healthBar;
    public Image expBar;
    public TextMeshProUGUI levelText;


    void Awake()
    {
        characterData = CharacterSelector.GetData();

        if (characterData == null)
        {
            Debug.LogError("No character data found! Make sure to select a character in the Character Selector scene.");
            return;
        }
        if (CharacterSelector.instance != null)
        {
            CharacterSelector.instance.DestroySingleton();
        }

        inventory = GetComponent<PlayerInventory>();

        baseStats = actualStats = characterData.stats;
        health = actualStats.maxHealth;
    }

    void Start()
    {
        inventory.Add(characterData.StartingWeapon);
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

    public void RecalculateStats()
    {
        actualStats = baseStats;
        foreach (PlayerInventory.Slot slot in inventory.passiveSlots)
        {
            Passive passive = slot.item as Passive;
            if (passive)
            {
                actualStats += passive.GetBoosts();
            }
        }
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

            if (damageEffect) Destroy(Instantiate(damageEffect, transform.position, Quaternion.identity), 5f);

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
            healthBar.fillAmount = CurrentHealth / actualStats.maxHealth;
        }
    }

    public void Die()
    {
        if (!GameManager.instance.isGameOver)
        {
            GameManager.instance.AssignLevelReachedUI(level);

            GameManager.instance.GameOver();
        }
    }

    public void Heal(float amount)
    {
        if (characterData == null) return;
        if (CurrentHealth < actualStats.maxHealth)
        {
            CurrentHealth += amount;

            if (CurrentHealth > actualStats.maxHealth)
            {
                CurrentHealth = actualStats.maxHealth;
            }
        }
        UpdateHealthBar();
    }

    void Recover()
    {
        if (CurrentHealth < actualStats.maxHealth)
        {
            CurrentHealth += CurrentRecovery * Time.deltaTime;

            if (CurrentHealth > actualStats.maxHealth)
            {
                CurrentHealth = actualStats.maxHealth;
            }
        }
        UpdateHealthBar();
    }

    [System.Obsolete("Old function that is kept to maintain compatibility with the InventoryManager. Will be removed after testing.")]
    public void SpawnWeapon(GameObject weaponPrefab)
    {
        if (weaponIndex >= inventory.weaponSlots.Count)
        {
            Debug.LogError("No more weapon slots available!");
            return;
        }

        GameObject spawnedWeapon = Instantiate(weaponPrefab, transform.position, Quaternion.identity);
        spawnedWeapon.transform.SetParent(transform);
        //inventory.AddWeapon(weaponIndex, spawnedWeapon.GetComponent<WeaponController>());

        weaponIndex++;
    }

    [System.Obsolete("Old function that is kept to maintain compatibility with the InventoryManager. Will be removed after testing.")]
    public void SpawnPassiveItem(GameObject passiveItemPrefab)
    {
        if (passiveItemIndex >= inventory.passiveSlots.Count - 1)
        {
            Debug.LogError("No more passive item slots available!");
            return;
        }

        GameObject spawnedPassiveItem = Instantiate(passiveItemPrefab, transform.position, Quaternion.identity);
        spawnedPassiveItem.transform.SetParent(transform);
        //inventory.AddPassiveItem(passiveItemIndex, spawnedPassiveItem.GetComponent<PassiveItem>());

        passiveItemIndex++;
    }
}

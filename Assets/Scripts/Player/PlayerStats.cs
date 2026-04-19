using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Misc;

public class PlayerStats : MonoBehaviour
{
    CharacterData characterData;
    public CharacterData.Stats baseStats;
    [SerializeField] CharacterData.Stats actualStats;

    public CharacterData.Stats Stats
    {
        get { return actualStats; }
        set
        {
            actualStats = value;
        }
    }

    float health;
    public float CurrentHealth
    {
        get { return health; }
        set
        {
            if (health != value)
            {
                health = value;
                UpdateHealthBar();
            }
        }
    }

    [Header("Visuals")]
    public ParticleSystem damageEffect;
    public ParticleSystem blockedEffect;

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

    PlayerCollector collector;
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
        collector = GetComponentInChildren<PlayerCollector>();

        baseStats = actualStats = characterData.stats;
        health = actualStats.maxHealth;
        collector.SetRadius(actualStats.magnet);
    }

    void Start()
    {
        inventory.Add(characterData.StartingWeapon);
        experienceCap = levelRanges[0].experienceCapIncrease;

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
        collector.SetRadius(actualStats.magnet);
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
        if (expBar != null)
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
            damage -= actualStats.armor;

            if (damage > 0)
            {
                CurrentHealth -= damage;

                if (damageEffect) Destroy(Instantiate(damageEffect, transform.position, Quaternion.identity), 5f);

                if (CurrentHealth <= 0)
                {
                    Die();
                }
            }
            else
            {
                if (blockedEffect) Destroy(Instantiate(blockedEffect, transform.position, Quaternion.identity), 5f);
            }
            
            iFrameTimer = iFrameDuration;
            isInvincible = true;
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
            GameManager.instance.AssignChosenWeaponsAndPassiveItemsUI(inventory.weaponSlots, inventory.passiveSlots);
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
            CurrentHealth += Stats.recovery * Time.deltaTime;

            if (CurrentHealth > actualStats.maxHealth)
            {
                CurrentHealth = actualStats.maxHealth;
            }
        }
        UpdateHealthBar();
    }
}

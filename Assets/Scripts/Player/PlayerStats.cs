using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : EntityStats
{
    const string LevelPrefix = "LV ";
    const float ReviveHealthFactor = 0.5f;

    CharacterData characterData;
    public CharacterData.Stats baseStats;
    CharacterData.Stats actualStats;
    int revivesUsed;

    public CharacterData.Stats Stats
    {
        get { return actualStats; }
        set
        {
            actualStats = value;
        }
    }

    public CharacterData.Stats Actual
    {
        get { return actualStats; }
    }

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

    [Header("UI")]
    public Image healthBar;
    public Image expBar;
    public TextMeshProUGUI levelText;


    void Awake()
    {
        characterData = UICharacterSelector.GetData();
        if (!characterData)
        {
            Debug.LogError("No character data found. Select a character before loading the game scene.");
            enabled = false;
            return;
        }

        inventory = GetComponent<PlayerInventory>();
        collector = GetComponentInChildren<PlayerCollector>();
        if (!inventory)
        {
            Debug.LogError("PlayerStats requires a PlayerInventory on the same GameObject.");
            enabled = false;
            return;
        }

        baseStats = actualStats = characterData.stats;
        health = actualStats.maxHealth;
        if (collector)
            collector.SetRadius(actualStats.magnet);
    }

    protected override void Start()
    {
        base.Start();

        if (UILevelSelector.globalBuff && !UILevelSelector.globalBuffAffectsPlayer)
        {
            ApplyBuff(UILevelSelector.globalBuff);
        }

        inventory.Add(characterData.StartingWeapon);
        experienceCap = GetExperienceCapIncreaseForCurrentLevel();

        if (GameManager.instance)
            GameManager.instance.AssignChosenCharacterUI(characterData);

        UpdateHealthBar();
        UpdateExpBar();
        UpdateLevelText();
    }

    protected override void Update()
    {
        base.Update();
        UpdateInvincibilityTimer();
        Recover();
    }

    void UpdateInvincibilityTimer()
    {
        if (iFrameTimer <= 0)
            return;

        iFrameTimer -= Time.deltaTime;
        if (iFrameTimer <= 0)
            isInvincible = false;
    }

    public override void RecalculateStats()
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

        CharacterData.Stats multiplier = new CharacterData.Stats
        {
            maxHealth = 1f,
            recovery = 1f,
            armor = 1f,
            moveSpeed = 1f,
            might = 1f,
            area = 1f,
            speed = 1f,
            duration = 1f,
            amount = 1,
            cooldown = 1f,
            luck = 1f,
            growth = 1f,
            greed = 1f,
            curse = 1f,
            magnet = 1f,
            revival = 1
        };

        foreach (Buff buff in activeBuffs)
        {
            BuffData.Stats buffStats = buff.GetData();
            switch (buffStats.modifierType)
            {
                case BuffData.ModifierType.additive:
                    actualStats += buffStats.playerModifier;
                    break;
                case BuffData.ModifierType.multiplicative:
                    multiplier *= buffStats.playerModifier;
                    break;
            }
        }

        actualStats *= multiplier;

        actualStats.revival = Mathf.Max(0, actualStats.revival - revivesUsed);
        if (collector)
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
            level++;
            experience -= experienceCap;

            experienceCap += GetExperienceCapIncreaseForCurrentLevel();

            UpdateLevelText();

            if (GameManager.instance != null)
            {
                GameManager.instance.StartLevelUp();
            }

            if (experience >= experienceCap)
            {
                LevelUpChecker();
            }
        }
    }

    void UpdateExpBar()
    {
        if (expBar != null)
        {
            expBar.fillAmount = experienceCap > 0 ? (float)experience / experienceCap : 0f;
        }
    }

    void UpdateLevelText()
    {
        if (levelText != null)
        {
            levelText.text = LevelPrefix + level;
        }
    }

    int GetExperienceCapIncreaseForCurrentLevel()
    {
        if (levelRanges == null)
            return 0;

        foreach (LevelRange range in levelRanges)
        {
            if (level >= range.startLevel && level <= range.endLevel)
                return range.experienceCapIncrease;
        }

        return 0;
    }

    public override void TakeDamage(float damage)
    {
        if (!isInvincible)
        {
            damage -= actualStats.armor;

            if (damage > 0)
            {
                CurrentHealth -= damage;

                if (damageEffect)
                    Destroy(Instantiate(damageEffect, transform.position, Quaternion.identity), 5f);

                if (CurrentHealth <= 0)
                {
                    Kill();
                }
            }
            else
            {
                if (blockedEffect)
                    Destroy(Instantiate(blockedEffect, transform.position, Quaternion.identity), 5f);
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

    public override void Kill()
    {
        if (TryRevive())
        {
            return;
        }

        if (!GameManager.instance.isGameOver)
        {
            GameManager.instance.GameOver(level);
        }
    }

    bool TryRevive()
    {
        if (actualStats.revival <= 0)
        {
            return false;
        }

        actualStats.revival--;
        revivesUsed++;
        CurrentHealth = actualStats.maxHealth * ReviveHealthFactor;
        iFrameTimer = iFrameDuration;
        isInvincible = true;

        return true;
    }

    public override void RestoreHealth(float amount)
    {
        if (characterData == null)
            return;
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

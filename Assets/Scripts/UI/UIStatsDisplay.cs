using System.Reflection;
using System.Text;
using TMPro;
using UnityEngine;

public class UIStatsDisplay : MonoBehaviour
{
    public PlayerStats player;
    public CharacterData character;
    public bool displayCurrentHealth = false;
    public bool updateInEditor = false;
    TextMeshProUGUI statNames, statValues;

    void OnEnable()
    {
        UpdateStatFields();
    }

    void OnDrawGizmosSelected()
    {
        if (updateInEditor) UpdateStatFields();
    }

    public CharacterData.Stats GetDisplayedStats()
    {
        if (player) return player.Stats;
        if (character) return character.stats;
        return new CharacterData.Stats();
    }

    public void UpdateStatFields()
    {
        if (!player && !character) return;

        if (!statNames) statNames = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        if (!statValues) statValues = transform.GetChild(1).GetComponent<TextMeshProUGUI>();

        StringBuilder names = new StringBuilder();
        StringBuilder values = new StringBuilder();

        if (displayCurrentHealth)
        {
            names.AppendLine("Health");
            values.AppendLine(player ? player.CurrentHealth.ToString() : "-");
        }

        CharacterData.Stats displayedStats = GetDisplayedStats();
        FieldInfo[] fields = typeof(CharacterData.Stats).GetFields(BindingFlags.Public | BindingFlags.Instance);
        foreach (FieldInfo field in fields)
        {
            names.AppendLine(field.Name);
            object value = field.GetValue(displayedStats);
            float floatValue = value is int ? (int)value : (float)value;

            PropertyAttribute attribute = (PropertyAttribute)PropertyAttribute.GetCustomAttribute(field, typeof(PropertyAttribute));
            if (attribute != null && field.FieldType == typeof(float))
            {
                float neutralValue = field.Name == nameof(CharacterData.Stats.curse) ? 0f : 1f;
                float percentage = Mathf.Round((floatValue - neutralValue) * 100);

                if (Mathf.Approximately(percentage, 0))
                {
                    values.Append("-").Append('\n');
                }
                else
                {
                    if (percentage > 0) values.Append("+");
                    values.Append(percentage).Append("%").Append('\n');
                }
            }
            else
            {
                values.Append(floatValue).Append('\n');
            }

        }

        statNames.text = PrettifyNames(names);
        statValues.text = values.ToString();
    }

    public static string PrettifyNames(StringBuilder input)
    {
        if (input.Length == 0) return string.Empty;

        StringBuilder output = new StringBuilder();
        char last = '\0';
        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];

            if (last == '\0' || char.IsWhiteSpace(last))
            {
                c = char.ToUpper(c);
            }
            else if (char.IsUpper(c))
            {
                output.Append(' ');
            }
            output.Append(c);

            last = c;
        }
        return output.ToString();
    }

    void Reset()
    {
        player = FindAnyObjectByType<PlayerStats>();
    }
}

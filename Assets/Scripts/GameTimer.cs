using System;
using System.Diagnostics.CodeAnalysis;
using TMPro;
using UnityEngine;

[ExcludeFromCodeCoverage]
public class GameTimer : MonoBehaviour
{
    float elapsedTime;
    float timeLimit;
    float clockSpeed = 1f;
    bool timeLimitReached;
    TextMeshProUGUI stopwatchDisplay;

    public event Action TimeLimitReached;

    public float ElapsedTime => elapsedTime;
    public string FormattedTime => FormatTime(elapsedTime);

    public void Initialize(float limit, TextMeshProUGUI display, float speed = 1f)
    {
        timeLimit = limit;
        clockSpeed = speed;
        stopwatchDisplay = display;
        UpdateDisplay();
    }

    public void Tick(float deltaTime)
    {
        elapsedTime += deltaTime * clockSpeed;
        UpdateDisplay();

        if (!timeLimitReached && timeLimit > 0f && elapsedTime >= timeLimit)
        {
            timeLimitReached = true;
            TimeLimitReached?.Invoke();
        }
    }

    void UpdateDisplay()
    {
        if (stopwatchDisplay)
            stopwatchDisplay.text = FormattedTime;
    }

    public static string FormatTime(float seconds)
    {
        int totalSeconds = Mathf.Max(0, Mathf.FloorToInt(seconds));
        return string.Format("{0:00}:{1:00}", totalSeconds / 60, totalSeconds % 60);
    }
}

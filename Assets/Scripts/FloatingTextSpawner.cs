using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;

[ExcludeFromCoverage]
public class FloatingTextSpawner : MonoBehaviour
{
    Canvas damageTextCanvas;
    float textFontSize;
    TMP_FontAsset damageTextFont;
    Camera referenceCamera;

    public void Initialize(Canvas canvas, float fontSize, TMP_FontAsset font, Camera camera)
    {
        damageTextCanvas = canvas;
        textFontSize = fontSize;
        damageTextFont = font;
        referenceCamera = camera;
    }

    public void Show(string text, Transform target, float duration = 1f, float speed = 1f)
    {
        if (!damageTextCanvas)
            return;
        if (!referenceCamera)
            referenceCamera = Camera.main;
        if (!referenceCamera)
            return;

        StartCoroutine(GenerateFloatingTextCoroutine(text, target, duration, speed));
    }

    IEnumerator GenerateFloatingTextCoroutine(string text, Transform target, float duration, float speed)
    {
        GameObject floatingTextObj = new GameObject("Damage Floating Text");
        RectTransform rectTransform = floatingTextObj.AddComponent<RectTransform>();
        TextMeshProUGUI textComponent = floatingTextObj.AddComponent<TextMeshProUGUI>();

        textComponent.text = text;
        textComponent.horizontalAlignment = HorizontalAlignmentOptions.Center;
        textComponent.verticalAlignment = VerticalAlignmentOptions.Middle;
        textComponent.fontSize = textFontSize;

        if (damageTextFont != null)
            textComponent.font = damageTextFont;

        Vector3 worldPosition = target ? target.position : Vector3.zero;
        rectTransform.position = referenceCamera.WorldToScreenPoint(worldPosition);

        Destroy(floatingTextObj, duration);

        floatingTextObj.transform.SetParent(damageTextCanvas.transform, false);
        floatingTextObj.transform.SetAsFirstSibling();

        WaitForEndOfFrame wait = new WaitForEndOfFrame();
        float elapsedTime = 0f;
        float yOffset = 0f;
        Vector3 lastKnownPosition = worldPosition;

        while (elapsedTime < duration)
        {
            if (floatingTextObj == null || rectTransform == null || textComponent == null)
                yield break;

            if (target)
                lastKnownPosition = target.position;

            Color color = textComponent.color;
            textComponent.color = new Color(color.r, color.g, color.b, 1f - elapsedTime / duration);

            yOffset += speed * Time.deltaTime;
            rectTransform.position = referenceCamera.WorldToScreenPoint(
                lastKnownPosition + new Vector3(0f, yOffset, 0f)
            );

            yield return wait;
            elapsedTime += Time.deltaTime;
        }

        if (floatingTextObj)
            Destroy(floatingTextObj);
    }
}

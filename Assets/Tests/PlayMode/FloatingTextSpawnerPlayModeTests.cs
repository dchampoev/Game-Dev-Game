using System.Collections;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class FloatingTextSpawnerPlayModeTests
{
    [UnityTearDown]
    public IEnumerator TearDown()
    {
        foreach (GameObject obj in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            Object.Destroy(obj);
        }

        yield return null;
    }

    Canvas CreateCanvas()
    {
        GameObject canvasObject = new GameObject("Canvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    Camera CreateCamera()
    {
        GameObject cameraObject = new GameObject("Main Camera");
        Camera camera = cameraObject.AddComponent<Camera>();
        cameraObject.tag = "MainCamera";
        camera.orthographic = true;
        return camera;
    }

    [UnityTest]
    public IEnumerator Show_WhenCanvasAndCameraExist_ShouldCreateFloatingText()
    {
        Canvas canvas = CreateCanvas();
        Camera camera = CreateCamera();
        FloatingTextSpawner spawner = new GameObject("Spawner").AddComponent<FloatingTextSpawner>();
        GameObject target = new GameObject("Target");
        target.transform.position = Vector3.zero;

        spawner.Initialize(canvas, 24f, null, camera);
        spawner.Show("48", target.transform, 1f, 0f);

        yield return null;

        TextMeshProUGUI text = canvas.GetComponentInChildren<TextMeshProUGUI>();

        Assert.NotNull(text);
        Assert.AreEqual("48", text.text);
        Assert.AreEqual(24f, text.fontSize);
    }

    [UnityTest]
    public IEnumerator Initialize_ShouldRenderDamageTextCanvasBehindModalUi()
    {
        Canvas canvas = CreateCanvas();
        Camera camera = CreateCamera();
        FloatingTextSpawner spawner = new GameObject("Spawner").AddComponent<FloatingTextSpawner>();

        spawner.Initialize(canvas, 24f, null, camera);

        yield return null;

        Assert.Less(canvas.sortingOrder, 0);
    }

    [UnityTest]
    public IEnumerator Show_WhenDurationEnds_ShouldDestroyFloatingText()
    {
        Canvas canvas = CreateCanvas();
        Camera camera = CreateCamera();
        FloatingTextSpawner spawner = new GameObject("Spawner").AddComponent<FloatingTextSpawner>();

        spawner.Initialize(canvas, 24f, null, camera);
        spawner.Show("12", null, 0.01f, 0f);

        yield return new WaitForSeconds(0.05f);

        Assert.IsNull(canvas.GetComponentInChildren<TextMeshProUGUI>());
    }
}

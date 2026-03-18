using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;
using UnityEngine.TestTools;

[ExcludeFromCoverage]
public class ChunkMapGenerator : MonoBehaviour
{

    public Camera worldCamera;
    public float updateInterval = 0.5f;

    [Header("Chunk Settings")]
    public PropRandomizer[] chunkPrefabs;
    public Vector2 chunkWorldSize = new Vector2(20f, 20f);
    public LayerMask chunkCollisionMask = 1;
    public bool destroyCulledChunks = false;

    Vector3 lastCameraPosition;
    Rect lastCameraRect;
    float cullDistanceSqr;

    void Start()
    {
        if (!worldCamera)
            Debug.LogError("ChunkMapGenerator cannot work without a reference camera.");

        if (chunkPrefabs.Length < 1)
            Debug.LogError("There are no Terrain Chunks assigned, so the map cannot be dynamically generated.");

        StartCoroutine(UpdateChunksLoop());
        SpawnMissingChunksInView(Vector2.zero, true);
    }

    void Reset()
    {
        worldCamera = Camera.main;
    }

    IEnumerator UpdateChunksLoop()
    {
        for (; ; )
        {
            yield return new WaitForSeconds(updateInterval);

            Vector3 moveDelta = worldCamera.transform.position - lastCameraPosition;
            bool hasCamWidthChanged = !Mathf.Approximately(worldCamera.pixelWidth - lastCameraRect.width, 0),
                 hasCamHeightChanged = !Mathf.Approximately(worldCamera.pixelHeight - lastCameraRect.height, 0);

            if (hasCamWidthChanged || hasCamHeightChanged || moveDelta.magnitude > 0.1f)
            {
                CullDistantChunks();
                SpawnMissingChunksInView(moveDelta, true);
            }

            lastCameraPosition = worldCamera.transform.position;
            lastCameraRect = worldCamera.pixelRect;
        }
    }

    public Rect GetCameraWorldRect()
    {
        if (!worldCamera)
        {
            Debug.LogError("Reference camera not found. Using Main Camera instead.");
            worldCamera = Camera.main;
        }

        Vector2 minPoint = worldCamera.ViewportToWorldPoint(worldCamera.rect.min),
                maxPoint = worldCamera.ViewportToWorldPoint(worldCamera.rect.max);
        Vector2 size = new Vector2(maxPoint.x - minPoint.x, maxPoint.y - minPoint.y);
        cullDistanceSqr = Mathf.Max(size.sqrMagnitude, chunkWorldSize.sqrMagnitude) * 3;

        return new Rect(minPoint, size);
    }

    public Vector2[] GetCheckedPoints()
    {
        Rect viewArea = GetCameraWorldRect();
        Vector2Int tileCount = new Vector2Int(
            (int)Mathf.Ceil(viewArea.width / chunkWorldSize.x) + 1,
            (int)Mathf.Ceil(viewArea.height / chunkWorldSize.y) + 1
        );

        HashSet<Vector2> result = new HashSet<Vector2>();
        for (int y = -1; y < tileCount.y; y++)
        {
            for (int x = -1; x < tileCount.x; x++)
            {
                result.Add(new Vector2(
                    viewArea.min.x + chunkWorldSize.x * x,
                    viewArea.min.y + chunkWorldSize.y * y
                ));
            }
        }

        return result.ToArray();
    }

    void SpawnMissingChunksInView(Vector2 moveDelta, bool checkWithoutDelta = false)
    {

        HashSet<Vector2> spawnedPositions = new HashSet<Vector2>();
        Vector2 currentPosition = worldCamera.transform.position;

        foreach (Vector3 vp in GetCheckedPoints())
        {
            if (!checkWithoutDelta)
            {
                if (moveDelta.x > 0 && vp.x < 0.5f) continue;
                else if (moveDelta.x < 0 && vp.x > 0.5f) continue;

                if (moveDelta.y > 0 && vp.y < 0.5f) continue;
                else if (moveDelta.y < 0 && vp.y > 0.5f) continue;
            }

            Vector3 checkedPosition = SnapToChunkGrid(vp);

            if (!spawnedPositions.Contains(checkedPosition) && !Physics2D.OverlapPoint(checkedPosition, chunkCollisionMask))
                SpawnChunk(checkedPosition);

            spawnedPositions.Add(checkedPosition);
        }
    }

    Vector3 SnapToChunkGrid(Vector3 position)
    {
        return new Vector3(
            Mathf.Round(position.x / chunkWorldSize.x) * chunkWorldSize.x,
            Mathf.Round(position.y / chunkWorldSize.y) * chunkWorldSize.y,
            transform.position.z
        );
    }

    PropRandomizer SpawnChunk(Vector3 spawnPosition, int variant = -1)
    {
        if (chunkPrefabs.Length < 1) return null;
        int rand = variant < 0 ? Random.Range(0, chunkPrefabs.Length) : variant;
        PropRandomizer chunk = Instantiate(chunkPrefabs[rand], transform);
        chunk.transform.position = spawnPosition;
        return chunk;
    }

    void CullDistantChunks()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform chunk = transform.GetChild(i);
            Vector2 dist = worldCamera.transform.position - chunk.position;
            bool cull = dist.sqrMagnitude > cullDistanceSqr;
            chunk.gameObject.SetActive(!cull);
            if (destroyCulledChunks && cull) Destroy(chunk.gameObject);
        }
    }
}
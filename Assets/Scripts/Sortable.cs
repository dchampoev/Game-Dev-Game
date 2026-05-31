using UnityEngine;

/// <summary>
/// This is a class that can be subclassed by any other class to make the sprites
/// of the class automatically sort themselves by the y-axis.
/// </summary>

[RequireComponent(typeof(SpriteRenderer))]
public abstract class Sortable : MonoBehaviour
{
    SpriteRenderer[] sortedRenderers;
    int[] baseSortingOrders;
    public bool sortingActive = true;
    public float minimumDistance = 0.2f;
    int lastSortOrder = int.MinValue;

    protected virtual void Start()
    {
        CacheRenderers();
    }

    protected virtual void LateUpdate()
    {
        if (!sortingActive)
        {
            return;
        }

        if (sortedRenderers == null || sortedRenderers.Length == 0)
        {
            CacheRenderers();
        }

        if (sortedRenderers.Length == 0 || minimumDistance <= 0)
        {
            return;
        }

        int newSortOrder = (int)(-transform.position.y / minimumDistance);
        if (lastSortOrder == newSortOrder)
        {
            return;
        }

        for (int i = 0; i < sortedRenderers.Length; i++)
        {
            if (sortedRenderers[i] != null)
            {
                sortedRenderers[i].sortingOrder = baseSortingOrders[i] + newSortOrder;
            }
        }

        lastSortOrder = newSortOrder;
    }

    void CacheRenderers()
    {
        sortedRenderers = GetComponentsInChildren<SpriteRenderer>();
        baseSortingOrders = new int[sortedRenderers.Length];

        for (int i = 0; i < sortedRenderers.Length; i++)
        {
            baseSortingOrders[i] = sortedRenderers[i].sortingOrder;
        }
    }
}

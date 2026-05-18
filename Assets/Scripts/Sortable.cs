using UnityEngine;

/// <summary>
/// This is a class that can be subclassed by any other class to make the sprites
/// of the class automatically sort themselves by the y-axis.
/// </summary>

[RequireComponent(typeof(SpriteRenderer))]
public abstract class Sortable : MonoBehaviour
{
    SpriteRenderer sorted;
    public bool sortingActive = true;
    public float minimumDistance = 0.2f;
    int lastSortOrder = 0;

    protected virtual void Start()
    {
        sorted = GetComponent<SpriteRenderer>();
    }

    protected virtual void LateUpdate()
    {
        int newSortOrder = (int)(-transform.position.y / minimumDistance);
        if(lastSortOrder != newSortOrder) sorted.sortingOrder = newSortOrder;
    }
}

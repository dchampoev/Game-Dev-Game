using UnityEngine;

public class TestCollectable : MonoBehaviour, ICollectable
{
    public bool wasCollected = false;

    public void Collect()
    {
        wasCollected = true;
    }
}
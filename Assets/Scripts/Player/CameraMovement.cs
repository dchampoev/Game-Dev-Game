using UnityEngine;
using UnityEngine.TestTools;

[ExcludeFromCoverage]
public class CameraMovement : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;

    void Update()
    {
        transform.position = target.position + offset;
    }
}

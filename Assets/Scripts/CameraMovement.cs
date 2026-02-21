using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform target; // The target the camera will follow
    public Vector3 offset; // The offset from the target's position

    // Update is called once per frame
    void Update()
    {
        transform.position = target.position + offset;
    }
}

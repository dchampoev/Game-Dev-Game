using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInput))]
public sealed class PlayerMovement : MonoBehaviour
{
    private const float DirectionEpsilon = 0.01f;
    [SerializeField] private float moveSpeed = 5f;

    Rigidbody2D rb;
    [HideInInspector]
    public Vector2 lastMoveDirection = Vector2.down; // Default facing down
    [HideInInspector]
    public Vector2 moveInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();

        if (moveInput.sqrMagnitude > DirectionEpsilon * DirectionEpsilon)
        { 
            lastMoveDirection = moveInput.normalized;   
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = moveInput.normalized * moveSpeed;
    }
}
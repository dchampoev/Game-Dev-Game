using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInput))]
public sealed class PlayerMovement : MonoBehaviour
{
    private const float DirectionEpsilon = 0.01f;
    [SerializeField] private float moveSpeed = 5f;

    Rigidbody2D rigidBody;
    [HideInInspector]
    public Vector2 lastMoveDirection = Vector2.down;
    [HideInInspector]
    public Vector2 movementInput;

    void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
    }

    public void OnMove(InputValue value)
    {
        movementInput = value.Get<Vector2>();

        if (movementInput.sqrMagnitude > DirectionEpsilon * DirectionEpsilon)
        { 
            lastMoveDirection = movementInput.normalized;   
        }
    }

    void FixedUpdate()
    {
        rigidBody.linearVelocity = movementInput.normalized * moveSpeed;
    }
}
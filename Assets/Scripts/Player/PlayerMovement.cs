using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;

[ExcludeFromCoverage]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInput))]
public sealed class PlayerMovement : Sortable
{
    public const float DEFAULT_MOVE_SPEED = 5f;
    private const float DirectionEpsilon = 0.01f;

    [HideInInspector]
    public Vector2 lastMoveDirection = Vector2.down;
    [HideInInspector]
    public Vector2 movementInput;

    Rigidbody2D rigidBody;
    PlayerStats player;

    void Awake()
    {
        player = GetComponent<PlayerStats>();
        rigidBody = GetComponent<Rigidbody2D>();
    }

    public void OnMove(InputValue value)
    {
        if (GameManager.instance && GameManager.instance.currentState != GameManager.GameState.Gameplay)
        {
            return;
        }

        if (value == null)
            return;

        movementInput = value.Get<Vector2>();

        if (movementInput.sqrMagnitude > DirectionEpsilon * DirectionEpsilon)
        {
            lastMoveDirection = movementInput.normalized;
        }
    }

    public void StopMovement()
    {
        movementInput = Vector2.zero;
        lastMoveDirection = Vector2.zero;

        if (rigidBody != null)
            rigidBody.linearVelocity = Vector2.zero;
    }

    void FixedUpdate()
    {
        if (!rigidBody || !player)
            return;

        rigidBody.linearVelocity = movementInput.normalized * DEFAULT_MOVE_SPEED * player.Stats.moveSpeed;
    }
}

using UnityEngine;
using UnityEngine.TestTools;

[ExcludeFromCoverage]
public class PlayerAnimator : MonoBehaviour
{
    Animator animator;
    PlayerMovement playerMovement;
    SpriteRenderer spriteRenderer;

    void Start()
    {
        animator = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        Vector2 input = playerMovement.movementInput;
        float speed = input.sqrMagnitude;

        animator.SetFloat("Speed", speed);

        animator.SetFloat("MoveX", input.x);
        animator.SetFloat("MoveY", input.y);

        Vector2 look = speed > 0.001f ? input : playerMovement.lastMoveDirection;
        animator.SetFloat("LookX", look.x);
        animator.SetFloat("LookY", look.y);
    }
}

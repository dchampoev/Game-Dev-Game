using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    //References
    Animator animator;
    PlayerMovement pm;
    SpriteRenderer sr;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        pm = GetComponent<PlayerMovement>();
        sr = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 input = pm.moveInput;
        float speed = input.sqrMagnitude;

        animator.SetFloat("Speed", speed);

        //Move direction(for walking animation)
        animator.SetFloat("MoveX", input.x);
        animator.SetFloat("MoveY", input.y);

        //Look direction(for idle + aiming)
        Vector2 look = speed > 0.001f ? input : pm.lastMoveDirection;
        animator.SetFloat("LookX", look.x);
        animator.SetFloat("LookY", look.y);
    }
}

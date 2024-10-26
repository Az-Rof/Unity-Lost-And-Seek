using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private bool canDash = true;
    private bool isDashing;
    [SerializeField] float dashingPower = 4f;
    [SerializeField] float dashingTime = 0.2f;
    [SerializeField] float dashingCooldown = 1f;
    Rigidbody2D rigidbody2D;
    Animator animator;
    [SerializeField] float speed = 100f;

    [SerializeField] float jumpforce = 8f;
    [SerializeField] bool onground;
    [SerializeField] bool isjump;
    [SerializeField] bool isattack;

    [SerializeField] private TrailRenderer tr;

    public LayerMask groundLayer;


    //Wall Jump
    [SerializeField] private float wallJumpForce = 10f;
    [SerializeField] private float wallSlideSpeed = 0.3f;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float wallCheckDistance = 0.5f;
    private bool isWallSliding = false;
    private bool isWallJumping = false;
    private float wallJumpDirection = 0f;
    private float wallJumpTime = 0.2f;
    private float wallJumpTimer;


    void Awake()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        tr = GetComponent<TrailRenderer>();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        isGrounded();
        movement();

    }

    void Update()
    {
        jump();
        attack();
        Dashing();
        WallSlide();
        WallJump();
    }

    private bool IsTouchingWall()
    {
        RaycastHit2D hitRight = Physics2D.Raycast(transform.position, Vector2.right, wallCheckDistance, wallLayer);
        RaycastHit2D hitLeft = Physics2D.Raycast(transform.position, Vector2.left, wallCheckDistance, wallLayer);

        // Draw debug rays
        Debug.DrawRay(transform.position, Vector2.right * wallCheckDistance, Color.red);
        Debug.DrawRay(transform.position, Vector2.left * wallCheckDistance, Color.red);

        return hitRight.collider != null || hitLeft.collider != null;
    }


    void movement()
    {
        if (!isDashing && !isWallJumping)
        {
            float h = Input.GetAxis("Horizontal");
            if (h != 0)
            {
                if (h > 0)
                {
                    transform.localScale = new Vector3(1f, 1f, 1f);
                }
                else if (h < 0)
                {
                    transform.localScale = new Vector3(-1f, 1f, 1f);
                }
                //rigidbody2D.velocity = new Vector2(h, rigidbody2D.velocity.y) * speed * Time.deltaTime;

                Vector2 newVelocity = rigidbody2D.velocity;
                newVelocity.x = h * speed * Time.deltaTime;
                rigidbody2D.velocity = newVelocity;
                animator.SetFloat("hMove", MathF.Abs(h));
                animator.SetFloat("hSpeed", MathF.Abs(speed) * MathF.Abs(h));
            }
            animator.SetFloat("yMove", MathF.Abs(rigidbody2D.velocity.y));
        }

    }

    void jump()
    {
        if (!isjump && onground && Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(PrepareJump());
        }
    }

    IEnumerator PrepareJump()
    {
        animator.SetBool("onGround", false);
        yield return new WaitForSeconds(0.25f);
        rigidbody2D.AddForce(new Vector2(0, jumpforce), ForceMode2D.Impulse);
    }

    void isGrounded()
    {
        Vector2 position = transform.position;
        Vector2 direction = Vector2.down;
        float distance = 0.75f;
        Debug.DrawRay(position, direction, Color.green);
        RaycastHit2D hit = Physics2D.Raycast(position, direction, distance, groundLayer);
        if (hit.collider != null)
        {
            onground = true;
            isjump = false;
            animator.SetBool("onGround", true);
        }
        else
        {
            isjump = true;
            onground = false;
            animator.SetBool("onGround", false);
        }
    }
    void attack()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            animator.SetTrigger("isAttack");

        }
    }

    void Dashing()
    {
        if (canDash && Input.GetKeyDown(KeyCode.LeftShift))
        {
            animator.SetTrigger("isDash");
            StartCoroutine(Dash());
        }
    }



    IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        float originalGravity = rigidbody2D.gravityScale;
        rigidbody2D.gravityScale = 0f;
        rigidbody2D.velocity = new Vector2(transform.localScale.x * dashingPower, 0f);
        tr.emitting = true;
        yield return new WaitForSeconds(dashingTime);
        tr.emitting = false;
        rigidbody2D.gravityScale = originalGravity;
        isDashing = false; // End the dash
        rigidbody2D.velocity = Vector2.zero;
        //yield return new WaitForSeconds(dashingCooldown - dashingTime);
        canDash = true;
    }


    void WallSlide()
    {
        if (IsTouchingWall() && !onground && rigidbody2D.velocity.y < 0)
        {
            isWallSliding = true;
            rigidbody2D.velocity = new Vector2(rigidbody2D.velocity.x, Mathf.Clamp(rigidbody2D.velocity.y, -wallSlideSpeed, float.MaxValue));
        }
        else
        {
            isWallSliding = false;
        }
    }

    void WallJump()
{
    if (isWallSliding)
    {
        // Determine the direction of the wall
        float wallDirection = 0f;
        if (Physics2D.Raycast(transform.position, Vector2.right, wallCheckDistance, wallLayer))
        {
            wallDirection = -1f; // Wall is on the right
        }
        else if (Physics2D.Raycast(transform.position, Vector2.left, wallCheckDistance, wallLayer))
        {
            wallDirection = 1f; // Wall is on the left
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Update player scale to face the wall
            if (wallDirection != 0)
            {
                transform.localScale = new Vector3(wallDirection, 1f, 1f);
            }

            isWallJumping = true;
            wallJumpDirection = wallDirection; // Use the wall direction for jumping
            wallJumpTimer = wallJumpTime;

            CancelInvoke(nameof(StopWallJumping));
            rigidbody2D.velocity = new Vector2(wallJumpDirection * wallJumpForce, jumpforce);
            Invoke(nameof(StopWallJumping), wallJumpTime);
        }
    }
}

    void StopWallJumping()
    {
        isWallJumping = false;
    }
}
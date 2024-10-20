using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private bool canDash = true;
    private bool isDashing;
    [SerializeField] float dashingPower =4f;
    [SerializeField] float dashingTime = 0.2f;
    [SerializeField] float dashingCooldown = 1f;
    Rigidbody2D rigidbody2D;
    Animator animator;
    [SerializeField] float speed;

    [SerializeField] float jumpforce;
    [SerializeField] bool onground;
    [SerializeField] bool isjump;
    [SerializeField] bool isattack;

    [SerializeField] private TrailRenderer tr;

    public LayerMask groundLayer;

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
        jump();
        attack();
        movement();
        Dashing();
        if (isDashing)
        {
            return;
        }
    }

    void Update()
    {

    }

    void movement()
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
            rigidbody2D.velocity = new Vector2(h, rigidbody2D.velocity.y) * speed * Time.deltaTime;
            animator.SetFloat("hMove", MathF.Abs(h));
            animator.SetFloat("hSpeed", MathF.Abs(speed) * MathF.Abs(h));
        }
        animator.SetFloat("yMove", MathF.Abs(rigidbody2D.velocity.y));
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
        rigidbody2D.AddForce(new Vector2(rigidbody2D.velocity.x, jumpforce), ForceMode2D.Impulse);
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
        canDash = true;
    }
}
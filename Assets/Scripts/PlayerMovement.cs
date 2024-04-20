using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    //Basic Movement Script
    public Rigidbody2D rb;
    public float moveSpeed;
    private SpriteRenderer sprite;

    public float jumpForce;
    private float moveInput;

    private bool isGrounded;
    public Transform feetPos;
    public float checkRadius;
    public LayerMask whatIsGround;

    //Variable Jumping
    public float jumpStartTime;
    private float jumpTime;
    private bool isJumping;

    //Multiple Jumps
    public int maxJumps;
    private int availableJumps;

    //Coyote Time
    [SerializeField]
    private float coyoteTime = 0.2f;
    private float coyoteTimeCounter;

    //Walljumping
    private bool isWallSliding;
    private bool isWallJumping;
    private float wallSlidingSpeed = 2f;
    private float wallJumpingDirection;
    private float wallJumpingTime = 0.2f;
    private float wallJumpingCounter;
    private float WallJumpingDuration = 0.4f;
    private Vector2 wallJumpingPower = new Vector2(8f, 16f);
    [SerializeField]
    private Transform wallCheck;
    [SerializeField]
    private LayerMask wallLayer;

    //Dash
    private bool canDash = true;
    private bool isDashing;
    private float dashingPower = 24f;
    private float dashingTime = 0.2f;
    private float dashingCooldown = 1f;
    [SerializeField]
    private TrailRenderer tr;

    //Collactable
    public CollectableManager cm;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if(isDashing)
        {
            return;
        }


        moveInput = Input.GetAxisRaw("Horizontal");
        FaceMoveDirection();
        Jump();
        WallSlide();
        WallJump();

        if(Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            StartCoroutine(Dash());
        }
    }

    void FixedUpdate()
    {
        if(isDashing)
        {
            return;
        }
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
    }

    //Flip Player
    void FaceMoveDirection()
    {
        if(moveInput > 0)
        {
            gameObject.transform.localScale = new Vector3(1, 1, 1);
        }
        else if (moveInput < 0)
        {
            gameObject.transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    void Jump()
    {
        isGrounded = Physics2D.OverlapCircle(feetPos.position, checkRadius, whatIsGround);

        //Jumping
        if(coyoteTimeCounter > 0f && isJumping == false)
        {
            isJumping = true;
            jumpTime = jumpStartTime;

            availableJumps = maxJumps;
        }

        //Multiple jumps
        if(Input.GetButtonDown("Jump") && availableJumps > 0)
        {
            isJumping = true;
            availableJumps--;
            rb.velocity = Vector2.up * jumpForce;
        }

        //Check if player is in a jump
        if(Input.GetButton("Jump") && isJumping == true)
        {
            if(jumpTime > 0)
            {
                rb.velocity = Vector2.up * jumpForce;
                jumpTime -= Time.deltaTime;
            }
            else
            {
                isJumping = false;
            }
        }

        //If jump button is released
        if (Input.GetButtonUp("Jump"))
        {
            coyoteTimeCounter = 0f;
            isJumping = false;
        }

        //Coyote Time
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
    }

    //Wall jumping

    private void WallJump()
    {
        if (isWallSliding)
        {
            isWallSliding = false;
            wallJumpingDirection = -transform.localScale.x;
            wallJumpingCounter = wallJumpingTime;

            CancelInvoke(nameof(StopWallJumping));
        }
        else
        {
            wallJumpingCounter -= Time.deltaTime;
        }

        if(Input.GetButtonDown("Jump") && wallJumpingCounter > 0f)
        {
            isWallJumping = true;
            rb.velocity = new Vector2(wallJumpingDirection * wallJumpingPower.x, wallJumpingPower.y);
            wallJumpingCounter = 0f;
        }

        Invoke(nameof(StopWallJumping), WallJumpingDuration);
    }

    private void StopWallJumping()
    {
        isWallJumping = false;
    }

    private bool isWalled()
    {
        return Physics2D.OverlapCircle(wallCheck.position, 0.2f, wallLayer);
    }

    private void WallSlide()
    {
        if(isWalled() && !isGrounded)
        {
            isWallSliding = true;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlidingSpeed, float.MaxValue));
        }
        else
        {
            isWallSliding = false;
        }
    }

    //Dash
    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.velocity = new Vector2(transform.localScale.x * dashingPower, 0f);
        tr.emitting = true;
        yield return new WaitForSeconds(dashingTime);
        tr.emitting = false;
        rb.gravityScale = originalGravity;
        isDashing = false;
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }


    void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(feetPos.position, checkRadius);
    }

    //Colect toys
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Collectable"))
        {
            Destroy(other.gameObject);
            cm.collectableCount++; 
        }
    }
}

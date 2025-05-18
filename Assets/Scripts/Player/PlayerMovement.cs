using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HUST
{
    public class PlayerMovement : MonoBehaviour
    {
        public static PlayerMovement Instance;

        [Header("Horizontal Movement Settings")]
        [SerializeField] private float walkSpeed;

        [Header("Vertical Movement Settings")]
        [SerializeField] private float jumpForce;
        [SerializeField] private float groundCheckY = 0.2f;
        [SerializeField] private float groundCheckX = 0.5f;
        [SerializeField] private float jumpBufferFrames;
        [SerializeField] private float coyoteTime;
        private float jumpBufferCounter = 0;
        private float coyoteTimeCounter = 0;
        private float xAxis;
        private float airJumpCounter = 0;
        [SerializeField] private int maxAirJumps;

        [Header("Dash Settings")]
        [SerializeField] private float dashSpeed;
        [SerializeField] private float dashTime;
        [SerializeField] private float dashCoolDown;
        [SerializeField] private GameObject dashEffect;
        private float gravity;

        [Header("Ground Check Settings")]
        [SerializeField] private Transform groundCheckPoint;
        [SerializeField] private LayerMask WhatIsGround;
        private Rigidbody2D rb;
        private Animator anim;
        public PlayerStateList pState;

        private bool canDash = true;
        private bool dashed;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            anim = GetComponent<Animator>();
            pState = GetComponent<PlayerStateList>();
            gravity = rb.gravityScale;
        }

        // Update is called once per frame
        void Update()
        {
            GetInputs();
            UpdateJumpVariables();
            
            if (pState.isDashing) return;
            Flip();
            Move();
            Jump();
            StartDash();
        }

        private void GetInputs()
        {
            xAxis = Input.GetAxisRaw("Horizontal");
        }

        private void Flip()
        {
            if (xAxis < 0)
            {
                transform.localScale = new Vector2(-1.5f, transform.localScale.y);
            }
            else if (xAxis > 0)
            {
                transform.localScale = new Vector2(1.5f, transform.localScale.y);
            }
        }

        private void Move()
        {
            rb.velocity = new Vector2(walkSpeed * xAxis, rb.velocity.y);
            anim.SetBool("isWalking", rb.velocity.x != 0 && Grounded());
        }

        private void StartDash()
        {
            if (Input.GetButtonDown("Dash") && canDash && !dashed)
            {
                StartCoroutine(Dash());
                dashed = true;
            }
            if (Grounded())
            {
                dashed = false;
            }
        }

        private IEnumerator Dash()
        {
            canDash = false;
            pState.isDashing = true;
            anim.SetTrigger("isDashing");
            rb.gravityScale = 0;
            rb.velocity = new Vector2(transform.localScale.x * dashSpeed, 0);
            if (Grounded()) Instantiate(dashEffect, transform);

            yield return new WaitForSeconds(dashTime);
            rb.gravityScale = gravity;
            pState.isDashing = false;
            yield return new WaitForSeconds(dashCoolDown);
            canDash = true;
        }

        public bool Grounded()
        {
            if (Physics2D.Raycast(groundCheckPoint.position, Vector2.down, groundCheckY, WhatIsGround) ||
                Physics2D.Raycast(groundCheckPoint.position + new Vector3(groundCheckX, 0, 0), Vector2.down, groundCheckY, WhatIsGround) ||
                Physics2D.Raycast(groundCheckPoint.position + new Vector3(-groundCheckX, 0, 0), Vector2.down, groundCheckY, WhatIsGround))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void Jump()
        {
            if (Input.GetButtonUp("Jump") && rb.velocity.y > 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, 0);

                pState.isJumping = false;
            }

            if (!pState.isJumping)
            {
                if (jumpBufferCounter > 0 && coyoteTimeCounter > 0)
                {
                    rb.velocity = new Vector3(rb.velocity.x, jumpForce);

                    pState.isJumping = true;
                }
                else if (!Grounded() && airJumpCounter < maxAirJumps && Input.GetButtonDown("Jump"))
                {
                    pState.isJumping = true;

                    airJumpCounter++;
                    rb.velocity = new Vector3(rb.velocity.x, jumpForce);
                }
            }

            anim.SetBool("isJumping", !Grounded());
        }
        
        private void UpdateJumpVariables()
        {
            if (Grounded())
            {
                pState.isJumping = false;
                coyoteTimeCounter = coyoteTime;
                airJumpCounter = 0;
            }
            else
            {
                coyoteTimeCounter -= Time.deltaTime;
            }

            if (Input.GetButtonDown("Jump"))
            {
                jumpBufferCounter = jumpBufferFrames;
            }
            else
            {
                jumpBufferCounter = jumpBufferCounter - Time.deltaTime * 10;
            }
        }
    }
}

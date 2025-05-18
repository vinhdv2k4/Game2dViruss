using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace HUST
{
    public class Enzyme1 : Enemy
    {
        [Header("Enzyme Settings:")]
        [SerializeField] private float chargeSpeedMultiplier;
        [SerializeField] private float chargeDuration;
        [SerializeField] private float jumpForce;
        [SerializeField] private float ledgeCheckX;
        [SerializeField] private float ledgeCheckY;
        [SerializeField] private LayerMask whatIsGround;
        private EnemyStates currentStates;


        private bool isFacingPlayer = false;
        float timer;

        protected override void Start()
        {
            base.Start();
            currentStates = GetComponent<EnemyStates>();
            currentStates.ChangeState(EnemyState.enzyme1_idle);
        }

        protected override void Update()
        {
            base.Update();
            if (!PlayerMovement.Instance.pState.alive)
            {
                currentStates.ChangeState(EnemyState.enzyme1_idle);
            }
        }
        public override void AttackPlayer()
        {
            Debug.Log("Enzyme1 Attack");
        }

        public override void Turn()
        {
            transform.localScale = new Vector2(transform.localScale.x * -1, transform.localScale.y);

        }

        protected override void UpdateEnemyStates()
        {
            Vector3 ledgeCheckStart = transform.localScale.x > 0 ? new Vector3(ledgeCheckX, 0) : new Vector3(-ledgeCheckX, 0);
            Vector2 wallCheckDir = transform.localScale.x > 0 ? transform.right : -transform.right;
            switch (currentStates.GetCurrentEnemyState)
            {
                case EnemyState.enzyme1_idle:
                    if (!Physics2D.Raycast(transform.position + ledgeCheckStart, Vector2.down, ledgeCheckY, whatIsGround)
                     || Physics2D.Raycast(transform.position, wallCheckDir, ledgeCheckX, whatIsGround))
                    {
                        transform.localScale = new Vector2(transform.localScale.x * -1, transform.localScale.y);
                    }
                    RaycastHit2D hit = Physics2D.Raycast(transform.position + ledgeCheckStart, wallCheckDir, ledgeCheckX * 7);
                    if (hit.collider != null && hit.collider.gameObject.CompareTag("Player"))
                    {
                        isFacingPlayer = true;
                        currentStates.ChangeState(EnemyState.enzyme1_Charge);
                    }
                    if (transform.localScale.x > 0)
                    {
                        enemyRb.velocity = new Vector2(speed, enemyRb.velocity.y);
                    }
                    else
                    {
                        enemyRb.velocity = new Vector2(-speed, enemyRb.velocity.y);
                    }
                    break;

                case EnemyState.enzyme2_detect:
                    if (!PlayerMovement.Instance.pState.Invincible)
                    {
                        enemyRb.velocity = new Vector2(0, jumpForce);

                        currentStates.ChangeState(EnemyState.enzyme1_Charge);
                    }
                    else
                    {
                        enemyRb.velocity = new Vector2(0, 0);
                        currentStates.ChangeState(EnemyState.enzyme1_idle);
                    }
                    break;

                case EnemyState.enzyme1_Charge:
                    timer += Time.deltaTime;
                    if (timer < chargeDuration && isFacingPlayer)
                    {
                        if (Physics2D.Raycast(transform.position, Vector2.down, ledgeCheckY, whatIsGround))
                        {
                            if (transform.localScale.x > 0)
                            {
                                enemyRb.velocity = new Vector2(speed * chargeSpeedMultiplier, enemyRb.velocity.y);
                            }
                            else
                            {
                                enemyRb.velocity = new Vector2(-speed * chargeSpeedMultiplier, enemyRb.velocity.y);
                            }
                        }
                        else
                        {
                            enemyRb.velocity = new Vector2(0, enemyRb.velocity.y);
                        }
                    }
                    else
                    {
                        isFacingPlayer = false;
                        timer = 0;
                        currentStates.ChangeState(EnemyState.enzyme1_idle);
                    }
                    break;
                case EnemyState.enzyme1_death:
                    break;
 

            }

        }
        public override void ChangeCurrentAnimation()
        {
            base.ChangeCurrentAnimation();
            enemyAnimator.SetBool("isIdle", currentStates.GetCurrentEnemyState == EnemyState.enzyme1_idle);
            enemyAnimator.SetBool("isCharge", currentStates.GetCurrentEnemyState == EnemyState.enzyme1_Charge);
            if(currentStates.GetCurrentEnemyState == EnemyState.enzyme1_death)
            {
                Vector2 deathvelocity;
                deathvelocity.x = 0;
                deathvelocity.y = 0;
                enemyRb.velocity = deathvelocity;
                enemyAnimator.SetTrigger("isDeath");
            }
            
        }
        public override void ResetStats()
        {
            base.ResetStats();
            timer = 0;
            isFacingPlayer = false;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace HUST
{
    public class Enzyme1 : Enemy
    {
        [Header("Patrol Settings")]
        [SerializeField] private float flipWaitTime;
        [SerializeField] private float ledgeCheckX;
        [SerializeField] private float ledgeCheckY;
        [SerializeField] private LayerMask WhatIsGround;

        [Header("Player Detection")]
        [SerializeField] private float detectionRange = 5f;
        [SerializeField] private float loseTargetRange = 7f;
        [SerializeField] private float chaseSpeed = 3f;
        [SerializeField] private float attackRange = 1.5f;
        [SerializeField] private float flipThreshold = 0.1f;
        [SerializeField] private LayerMask playerLayerMask = 1 << 6; 

        private Transform playerTransform;
        private bool hasFlippedTowardsPlayer = false; // Cờ để track việc flip
        float timer;

        protected override void Start()
        {
            base.Start();
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }

        protected override void UpdateEnemyState()
        {
            base.UpdateEnemyState();
            switch (currentState)
            {
                case EnemyState.Idle:
                    HandleIdleState();
                    break;

                case EnemyState.Chasing:
                    HandleChasingState();
                    break;

                case EnemyState.Flip:
                    HandleFlipState();
                    break;

                case EnemyState.Attacking:
                    rb.velocity = new Vector2(0, rb.velocity.y);
                    break;

                case EnemyState.Recoiling:
                    break;
            }
        }

        private void HandleIdleState()
        {
            Debug.Log($"HandleIdleState: Current State = {currentState}");
            
            // Kiểm tra player có trong tầm nhìn không
            if (CanDetectPlayer())
            {
                ChangeState(EnemyState.Chasing);
                hasFlippedTowardsPlayer = false; // Reset flag
                return;
            }

            // Kiểm tra có nên flip về phía player không (không cần player nearby)
            if (!hasFlippedTowardsPlayer && ShouldFlipTowardsPlayer())
            {
                Debug.Log("Flipping towards player!");
                TurnTowardsPlayer();
                hasFlippedTowardsPlayer = true;
                
                // Sau khi flip, kiểm tra lại detection
                if (CanDetectPlayer())
                {
                    ChangeState(EnemyState.Chasing);
                    hasFlippedTowardsPlayer = false;
                    return;
                }
            }

            // Reset flag nếu player không còn trong tầm có thể flip
            if (!IsPlayerInFlipRange())
            {
                hasFlippedTowardsPlayer = false;
            }

            // Logic patrol thông thường
            DoPatrolMovement();
        }

        private void DoPatrolMovement()
        {
            Vector3 ledgeCheckStartPoint = transform.localScale.x > 0 ? new Vector3(ledgeCheckX, 0) : new Vector3(-ledgeCheckX, 0);
            Vector2 wallCheckDir = transform.localScale.x > 0 ? transform.right : -transform.right;

            bool hitWall = Physics2D.Raycast(transform.position, wallCheckDir, ledgeCheckX, WhatIsGround);
            bool noGround = !Physics2D.Raycast(transform.position + ledgeCheckStartPoint, Vector2.down, ledgeCheckY, WhatIsGround);

            Debug.DrawRay(transform.position, wallCheckDir * ledgeCheckX, Color.red); // Vẽ ray để test trong Scene
            Debug.DrawRay(transform.position + ledgeCheckStartPoint, Vector2.down * ledgeCheckY, Color.blue);

            Debug.Log($"[DoPatrolMovement] hitWall={hitWall}, noGround={noGround}");

            if (hitWall || noGround)
            {
                ChangeState(EnemyState.Flip);
                hasFlippedTowardsPlayer = false;
            }
            else
            {
                float moveDirection = transform.localScale.x > 0 ? 1 : -1;
                Vector2 newVelocity = new Vector2(moveDirection * speed, rb.velocity.y);
                rb.velocity = newVelocity;

                Debug.Log($"[DoPatrolMovement] Moving: {newVelocity}");
            }
        }


        private void HandleChasingState()
        {
            ChasePlayer();

            if (!IsPlayerInLoseRange())
            {
                ChangeState(EnemyState.Idle);
                hasFlippedTowardsPlayer = false;
            }

            if (CanAttackPlayer())
            {
                ChangeState(EnemyState.Attacking);
            }

            // Kiểm tra vật cản khi chase
            Vector2 chaseWallCheckDir = transform.localScale.x > 0 ? transform.right : -transform.right;
            if (Physics2D.Raycast(transform.position, chaseWallCheckDir, ledgeCheckX, WhatIsGround))
            {
                rb.velocity = new Vector2(0, rb.velocity.y);
            }
        }

        private void HandleFlipState()
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            timer += Time.deltaTime;
            if (timer > flipWaitTime)
            {
                timer = 0;
                transform.localScale = new Vector2(transform.localScale.x * -1, transform.localScale.y);
                ChangeState(EnemyState.Idle);
            }
        }

        // Kiểm tra có nên flip về phía player không
        private bool ShouldFlipTowardsPlayer()
        {
            if (playerTransform == null) return false;

            // Tăng tầm flip để enemy có thể flip từ xa hơn
            float flipDistance = detectionRange * 1.5f; // hoặc loseTargetRange
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            
            if (distanceToPlayer > flipDistance) return false;

            float directionToPlayer = playerTransform.position.x - transform.position.x;
            float currentFacing = transform.localScale.x;

            // Nếu player ở bên phải nhưng enemy đang hướng trái
            if (directionToPlayer > flipThreshold && currentFacing < 0)
                return true;

            // Nếu player ở bên trái nhưng enemy đang hướng phải
            if (directionToPlayer < -flipThreshold && currentFacing > 0)
                return true;

            return false;
        }

        // Kiểm tra player có trong tầm flip không
        private bool IsPlayerInFlipRange()
        {
            if (playerTransform == null) return false;
            
            float flipDistance = detectionRange * 1.5f;
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            return distanceToPlayer <= flipDistance;
        }

        private bool CanDetectPlayer()
        {
            if (playerTransform == null) return false;

            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer <= detectionRange)
            {
                Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
                RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, detectionRange, ~playerLayerMask);

                return hit.collider == null || hit.collider.CompareTag("Player");
            }
            return false;
        }

        private void ChasePlayer()
        {
            if (playerTransform == null) return;

            Vector2 direction = (playerTransform.position - transform.position).normalized;
            FlipTowardsPlayer(direction.x);
            rb.velocity = new Vector2(direction.x * chaseSpeed, rb.velocity.y);
        }

        private void FlipTowardsPlayer(float directionX)
        {
            if (directionX > 0.1f && transform.localScale.x < 0)
            {
                transform.localScale = new Vector2(Mathf.Abs(transform.localScale.x), transform.localScale.y);
            }
            else if (directionX < -0.1f && transform.localScale.x > 0)
            {
                transform.localScale = new Vector2(-Mathf.Abs(transform.localScale.x), transform.localScale.y);
            }
        }

        private bool CanAttackPlayer()
        {
            if (playerTransform == null) return false;
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            return distanceToPlayer <= attackRange;
        }

        private bool IsPlayerNearby()
        {
            if (playerTransform == null) return false;
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            return distanceToPlayer <= loseTargetRange;
        }

        private bool IsPlayerInLoseRange()
        {
            if (playerTransform == null) return false;
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            return distanceToPlayer <= loseTargetRange;
        }

        private void TurnTowardsPlayer()
        {
            if (playerTransform == null) return;
            float directionX = playerTransform.position.x - transform.position.x;
            FlipTowardsPlayer(directionX);
        }
    }
}
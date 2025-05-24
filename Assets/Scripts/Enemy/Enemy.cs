using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HUST
{
    public class Enemy : MonoBehaviour
    {
        [SerializeField] protected float health;
        [SerializeField] protected float recoilLength;
        [SerializeField] protected float recoilFactor;
        [SerializeField] protected bool isRecoiling = false;

        protected float recoilTimer;
        protected Rigidbody2D rb;
        [SerializeField] protected PlayerMovement player;
        [SerializeField] protected float speed;
        [SerializeField] protected float damage;

        protected virtual void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            player = PlayerMovement.Instance;
        }

        // Update is called once per frame
        protected virtual void Update()
        {
            if (health <= 0)
            {
                Destroy(gameObject);
            }
            if (isRecoiling)
            {
                if (recoilTimer < recoilLength)
                {
                    recoilTimer += Time.deltaTime;
                }
                else
                {
                    isRecoiling = false;
                }
            }
        }

        public virtual void EnemyHit(float _damageDone, Vector2 _hitDirection, float _hitForce)
        {
            health -= _damageDone;

            rb.velocity = Vector2.zero;
            rb.AddForce(-_hitForce * recoilFactor * _hitDirection, ForceMode2D.Impulse);

            recoilTimer = 0;
            isRecoiling = true;
        }

        protected void OnCollisionStay2D(Collision2D _other)
        {
            if (_other.gameObject.CompareTag("Player") && !PlayerMovement.Instance.pState.invincible)
            {
                Attack();
                PlayerMovement.Instance.HitStopTime(0, 20, 0);
            }
        }

        protected virtual void Attack()
        {
            PlayerMovement.Instance.TakeDamage(damage);
        }
    }
}


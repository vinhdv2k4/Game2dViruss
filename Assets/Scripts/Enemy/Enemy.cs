using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace HUST
{
    public abstract class Enemy : MonoBehaviour
    {
        [Header("Enemy Setting")]
        [SerializeField] public float health;
        [SerializeField] public float maxHealth;
        [SerializeField] public float speed;
        [SerializeField] public float damage;
        [SerializeField] public float destroy;
        [SerializeField] protected float playerEnemyDistance;

        [SerializeField] public Rigidbody2D enemyRb;
        [SerializeField] public int enzyme;

        [Header("Tranform")]
        protected Transform playerTranform;
        protected Transform enemyTranform;

        [Header("Animator")]
        public Animator enemyAnimator;

        public delegate void OnHealthChanged();
        public OnHealthChanged onHealthChangedCallback;

        protected virtual void Start()
        {
            playerTranform = PlayerMovement.Instance.transform;
            enemyRb = GetComponent<Rigidbody2D>();
            enemyAnimator = GetComponent<Animator>();
            enemyTranform = transform;
        }
       protected virtual void Update()
        {
            playerEnemyDistance = Vector2.Distance(playerTranform.position, enemyTranform.position);
            UpdateEnemyStates();
            if (health <= 0)
            {
                Destroy(gameObject, destroy);
            }
        }
        public virtual void ChangeCurrentAnimation() { }

        public abstract void Turn();
        public abstract void AttackPlayer();

        protected virtual void UpdateEnemyStates() { }

        public float Distance()
        {
            return playerEnemyDistance;
        }
        public virtual void ResetStats()
        {
            health = maxHealth;
        }


        public virtual void EnemyGetHit(float damaged, Vector2 hitDirection,float hitforce)
        {
            health -= damaged;
            enemyRb.velocity = hitDirection*hitforce;
        }
        protected virtual void OnCollisionStay2D(Collision2D collision)
        {
            string layerName = LayerMask.LayerToName(collision.collider.gameObject.layer);

            if (layerName == "Player" &&  health > 0)
            {
                AttackPlayer();
            }
           
        }

        public float Health
        {
            get { return health; }
            set
            {
                if (health != value)
                {
                    health = Mathf.Clamp(value, 0, maxHealth);

                    if (onHealthChangedCallback != null)
                    {
                        onHealthChangedCallback.Invoke();
                    }
                }
            }
        }

    }
}

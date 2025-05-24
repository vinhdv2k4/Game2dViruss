using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEngine.UI;

namespace HUST
{
    public class PlayerMovement : MonoBehaviour
    {
        public static PlayerMovement Instance;

        [Header("Horizontal Movement Settings")]
        [SerializeField] private float walkSpeed;
        [Space(5)]

        [Header("Vertical Movement Settings")]
        [SerializeField] private float jumpForce;
        [SerializeField] private float groundCheckY = 0.2f;
        [SerializeField] private float groundCheckX = 0.5f;
        [SerializeField] private float jumpBufferFrames;
        [SerializeField] private float coyoteTime;
        private float jumpBufferCounter = 0;
        private float coyoteTimeCounter = 0;
        private float xAxis, yAxis;
        private float airJumpCounter = 0;
        [SerializeField] private int maxAirJumps;
        [Space(5)]

        [Header("Dash Settings")]
        [SerializeField] private float dashSpeed;
        [SerializeField] private float dashTime;
        [SerializeField] private float dashCoolDown;
        [SerializeField] private GameObject dashEffect;
        private float gravity;
        [Space(5)]

        [Header("Ground Check Settings")]
        [SerializeField] private Transform groundCheckPoint;
        [SerializeField] private LayerMask WhatIsGround;
        [HideInInspector] public PlayerStateList pState;
        private Rigidbody2D rb;
        private Animator anim;
        private SpriteRenderer sr;

        private bool canDash = true;
        private bool dashed;
        [Space(5)]

        [Header("Attack Settings")]
        private bool attack = false;
        [SerializeField] private float timeBetweenAttack;
        private float timeSinceAttack;
        [SerializeField] Transform SideAttackTransform, UpAttackTransform, DownAttackTransform;
        [SerializeField] Vector2 SideAttackArea, UpAttackArea, DownAttackArea;
        [SerializeField] LayerMask attackableLayer;
        [SerializeField] private float damage;
        [SerializeField] GameObject slashEffect;
        private bool restoreTime;
        private float restoreTimeSpeed;
        [Space(5)]

        [Header("Recoil")]
        [SerializeField] private int recoilXSteps = 5;
        [SerializeField] private int recoilYSteps = 5;
        [SerializeField] private float recoilXSpeed = 100;
        [SerializeField] private float recoilYSpeed = 100;
        private int stepsXRecoiled, stepsYRecoiled;
        [Space(5)]

        [Header("Health Settings")]
        public int health;
        public int maxHealth;
        [SerializeField] private float hitFlashSpeed;
        public delegate void OnHealthChangedDelegate();
        [HideInInspector] public OnHealthChangedDelegate onHealthChangedCallBack;
        private float healTimer;
        [SerializeField] private float timeToHeal;
        [Space(5)]

        [Header("Mana Settings")]
        [SerializeField] private UnityEngine.UI.Image manaStorage;
        [SerializeField] private float mana;
        [SerializeField] private float manaDrainSpeed;
        [SerializeField] private float manaGain;
        [Space(5)]

        [Header("Spell Settings")]
        [SerializeField] private float manaSpellCost = 0.3f;
        [SerializeField] private float timeBetweenCast = 0.5f;
        [SerializeField] private float spellDamage;
        [SerializeField] private float downSpellForce;

        private float timeSinceCast;
        [SerializeField] GameObject sideSpellFireball;
        [SerializeField] GameObject upSpellExplosion;
        [SerializeField] GameObject downSpellFireball;

        private bool canFlash = true;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
            DontDestroyOnLoad(gameObject);
        }

        // Start is called before the first frame update
        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            anim = GetComponent<Animator>();
            pState = GetComponent<PlayerStateList>();
            sr = GetComponent<SpriteRenderer>();
            gravity = rb.gravityScale;

            Mana = mana;
            manaStorage.fillAmount = Mana;

            Health = maxHealth;
        }

        // Update is called once per frame
        void Update()
        {
            if (pState.cutscene) return;
            GetInputs();
            UpdateJumpVariables();

            if (pState.isDashing) return;
            RestoreTimeScale();
            FlashWhileInvincible();
            Move();
            Heal();
            CastSpell();

            if (pState.isHealing) return;
            Flip();
            Jump();
            StartDash();
            Attack();

        }

        private void OnTriggerEnter2D(Collider2D _other)
        {
            if (_other.GetComponent<Enemy>() != null && pState.isCasting)
            {
                _other.GetComponent<Enemy>().EnemyHit(spellDamage, (_other.transform.position - transform.position).normalized, -recoilYSpeed);
            }
        }

        private void FixedUpdate()
        {
            if (pState.cutscene) return;
            if (pState.isDashing) return;
            Recoil();
        }

        private void GetInputs()
        {
            xAxis = Input.GetAxisRaw("Horizontal");
            yAxis = Input.GetAxisRaw("Vertical");
            attack = Input.GetKeyDown(KeyCode.J);
        }

        private void Flip()
        {
            if (xAxis < 0)
            {
                transform.localScale = new Vector2(-1.5f, transform.localScale.y);
                pState.lookingRight = false;
            }
            else if (xAxis > 0)
            {
                transform.localScale = new Vector2(1.5f, transform.localScale.y);
                pState.lookingRight = true;
            }
        }

        private void Move()
        {
            if (pState.isHealing) rb.velocity = new Vector2(0, 0);
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
            int _dir = pState.lookingRight ? 1 : -1;
            rb.velocity = new Vector2(_dir * dashSpeed, 0);
            if (Grounded()) Instantiate(dashEffect, transform);

            yield return new WaitForSeconds(dashTime);
            rb.gravityScale = gravity;
            pState.isDashing = false;
            yield return new WaitForSeconds(dashCoolDown);
            canDash = true;
        }

        public IEnumerator WalkInToNewScene(Vector2 _exitDir, float _delay)
        {
            // pState.invincible = true;
            // If exit direction is upwards
            if (_exitDir.y > 0)
            {
                rb.velocity = jumpForce * _exitDir;
            }

            // If exit direction requires horizontal movement
            if (_exitDir.x != 0)
            {
                xAxis = _exitDir.x > 0 ? 1 : -1;
                Move();
            }
            Flip();
            yield return new WaitForSeconds(_delay);
            // pState.invincible = false;
            pState.cutscene = false;
        }

        private void Attack()
        {
            timeSinceAttack += Time.deltaTime;
            if (attack && timeSinceAttack >= timeBetweenAttack)
            {
                timeSinceAttack = 0;
                anim.SetTrigger("isAttacking");

                if (yAxis <= 0 && Grounded())
                {
                    Hit(SideAttackTransform, SideAttackArea, ref pState.recoilingX, recoilXSpeed);
                    Instantiate(slashEffect, SideAttackTransform);
                }
                else if (yAxis > 0)
                {
                    Hit(UpAttackTransform, UpAttackArea, ref pState.recoilingY, recoilYSpeed);
                    SlashEffectAtAngle(slashEffect, 180, UpAttackTransform);
                }
                else if (yAxis < 0 && !Grounded())
                {
                    Hit(DownAttackTransform, DownAttackArea, ref pState.recoilingY, recoilYSpeed);
                    SlashEffectAtAngle(slashEffect, 0, DownAttackTransform);
                }
            }
        }

        private void Hit(Transform _attackTransform, Vector2 _attackArea, ref bool _recoilDir, float _recoilStrength)
        {
            Collider2D[] objectsToHit = Physics2D.OverlapBoxAll(_attackTransform.position, _attackArea, 0, attackableLayer);
            List<Enemy> hitEnemies = new List<Enemy>();

            if (objectsToHit.Length > 0)
            {
                _recoilDir = true;
            }
            for (int i = 0; i < objectsToHit.Length; i++)
            {
                Enemy enemy = objectsToHit[i].GetComponent<Enemy>();
                if (enemy && !hitEnemies.Contains(enemy))
                {
                    enemy.EnemyHit(damage, (transform.position - objectsToHit[i].transform.position).normalized, _recoilStrength);
                    hitEnemies.Add(enemy);

                    if (objectsToHit[i].CompareTag("Enemy"))
                    {
                        Mana += manaGain;
                    }
                }
            }
        }

        private void SlashEffectAtAngle(GameObject _slashEffect, int _effectAngle, Transform _attackTransform)
        {
            _slashEffect = Instantiate(_slashEffect, _attackTransform);
            _slashEffect.transform.eulerAngles = new Vector3(0, 0, _effectAngle);
            _slashEffect.transform.localScale = new Vector2(transform.localScale.x, transform.localScale.y);
        }

        private void Recoil()
        {
            if (pState.recoilingX)
            {
                if (pState.lookingRight)
                {
                    rb.velocity = new Vector2(-recoilXSpeed, 0);
                }
                else
                {
                    rb.velocity = new Vector2(recoilXSpeed, 0);
                }
            }

            if (pState.recoilingY)
            {
                rb.gravityScale = 0;
                if (yAxis < 0)
                {
                    rb.velocity = new Vector2(rb.velocity.x, recoilYSpeed);
                }
                else
                {
                    rb.velocity = new Vector2(rb.velocity.x, -recoilYSpeed);
                }
                airJumpCounter = 0;
            }
            else
            {
                rb.gravityScale = gravity;
            }
            // Stop recoil
            if (pState.recoilingX && stepsXRecoiled < recoilXSteps)
            {
                stepsXRecoiled++;
            }
            else
            {
                StopRecoilX();
            }
            if (pState.recoilingY && stepsYRecoiled < recoilYSteps)
            {
                stepsYRecoiled++;
            }
            else
            {
                StopRecoilY();
            }
            if (Grounded())
            {
                StopRecoilY();
            }

        }

        private void StopRecoilX()
        {
            stepsXRecoiled = 0;
            pState.recoilingX = false;
        }

        private void StopRecoilY()
        {
            stepsYRecoiled = 0;
            pState.recoilingY = false;
        }

        public void TakeDamage(float _damage)
        {
            Health -= Mathf.RoundToInt(_damage);
            StartCoroutine(StopTakingDamage());
        }

        private IEnumerator StopTakingDamage()
        {
            pState.invincible = true;
            anim.SetTrigger("TakeDamage");
            yield return new WaitForSeconds(1f);
            pState.invincible = false;
        }

        private IEnumerator Flash()
        {
            sr.enabled = !sr.enabled;
            canFlash = false;
            yield return new WaitForSeconds(0.1f);
            canFlash = true;
        }

        private void FlashWhileInvincible()
        {
            if (pState.invincible && !pState.cutscene)
            {
                if (Time.timeScale > 0.2 && canFlash)
                {
                    StartCoroutine(Flash());
                }
                else
                {
                    sr.enabled = true;
                }
            }
        }

        private void RestoreTimeScale()
        {
            if (restoreTime)
            {
                if (Time.timeScale < 1)
                {
                    Time.timeScale += Time.unscaledDeltaTime * restoreTimeSpeed;
                }
                else
                {
                    Time.timeScale = 1;
                    restoreTime = false;
                }
            }
        }

        public void HitStopTime(float _newTimeScale, int _restoreSpeed, float _delay)
        {
            restoreTimeSpeed = _restoreSpeed;
            Time.timeScale = _newTimeScale;
            if (_delay > 0)
            {
                StopCoroutine(StartTimeAgain(_delay));
                StartCoroutine(StartTimeAgain(_delay));
            }
            else
            {
                restoreTime = true;
            }
        }

        private IEnumerator StartTimeAgain(float _delay)
        {
            yield return new WaitForSecondsRealtime(_delay);
            restoreTime = true;
        }

        public int Health
        {
            get { return health; }
            set
            {
                if (health != value)
                {
                    health = Mathf.Clamp(value, 0, maxHealth);

                    if (onHealthChangedCallBack != null)
                    {
                        onHealthChangedCallBack.Invoke();
                    }
                }
            }
        }

        private void Heal()
        {
            if (Input.GetKey(KeyCode.K) && Health < maxHealth && Mana > 0 && Grounded() && !pState.isDashing)
            {
                pState.isHealing = true;
                anim.SetBool("isHealing", true);

                // Healing
                healTimer += Time.deltaTime;
                if (healTimer >= timeToHeal)
                {
                    Health++;
                    healTimer = 0;
                }
                // Drain mana
                Mana -= Time.deltaTime * manaDrainSpeed;
            }
            else
            {
                pState.isHealing = false;
                anim.SetBool("isHealing", false);
                healTimer = 0;
            }
        }

        private float Mana
        {
            get { return mana; }
            set
            {
                if (mana != value)
                {
                    mana = Mathf.Clamp(value, 0, 1);
                    manaStorage.fillAmount = Mana;
                }
            }
        }

        private void CastSpell()
        {
            if (Input.GetKeyDown(KeyCode.L) && timeSinceCast >= timeBetweenCast && Mana >= manaSpellCost)
            {
                pState.isCasting = true;
                timeSinceCast = 0;
                StartCoroutine(CastCoroutine());
            }
            else
            {
                timeSinceCast += Time.deltaTime;
            }

            if (Grounded())
            {
                downSpellFireball.SetActive(false);
            }

            if (downSpellFireball.activeInHierarchy)
            {
                rb.velocity += downSpellForce * Vector2.down;
            }
        }

        private IEnumerator CastCoroutine()
        {
            anim.SetBool("isCasting", true);
            yield return new WaitForSeconds(0.15f);

            // Side Cast
            if (yAxis == 0 || (yAxis < 0 && Grounded()))
            {
                GameObject _fireBall = Instantiate(sideSpellFireball, SideAttackTransform.position, Quaternion.identity);
                if (pState.lookingRight)
                {
                    _fireBall.transform.eulerAngles = Vector3.zero;
                }
                else
                {
                    _fireBall.transform.eulerAngles = new Vector2(_fireBall.transform.eulerAngles.x, 180);
                }
                pState.recoilingX = true;
            }
            // Up Cast
            else if (yAxis > 0)
            {
                Instantiate(upSpellExplosion, transform);
                rb.velocity = Vector2.zero;
            }
            // Down Cast
            else if (yAxis < 0 && !Grounded())
            {
                downSpellFireball.SetActive(true);
            }

            Mana -= manaSpellCost;
            yield return new WaitForSeconds(0.3f);
            anim.SetBool("isCasting", false);
            pState.isCasting = false;
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
            if (jumpBufferCounter > 0 && coyoteTimeCounter > 0 && !pState.isJumping)
            {
                rb.velocity = new Vector3(rb.velocity.x, jumpForce);

                pState.isJumping = true;
            }
            if (!Grounded() && airJumpCounter < maxAirJumps && Input.GetButtonDown("Jump"))
            {
                pState.isJumping = true;

                airJumpCounter++;
                rb.velocity = new Vector3(rb.velocity.x, jumpForce);
            }

            if (Input.GetButtonUp("Jump") && rb.velocity.y > 3)
            {
                rb.velocity = new Vector2(rb.velocity.x, 0);

                pState.isJumping = false;
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

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(SideAttackTransform.position, SideAttackArea);
            Gizmos.DrawWireCube(UpAttackTransform.position, UpAttackArea);
            Gizmos.DrawWireCube(DownAttackTransform.position, DownAttackArea);
        }
    }
}

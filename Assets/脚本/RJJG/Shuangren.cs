


////
//////
////////
/////2.0
using UnityEngine;
using System.Collections;

public enum PlayerNumber
{
    Player1,
    Player2
}

public class Shuangren : MonoBehaviour
{
    [Header("玩家设置")]
    [SerializeField] private PlayerNumber playerNumber;
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;
    private bool isDead = false;
    private bool isHurt;

    [Header("受击颜色效果")]
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Color hurtColor = new Color(1f, 0.2f, 0.2f);
    [SerializeField] private float hurtColorDuration = 0.15f;
    [SerializeField] private int hurtFlashCount = 2;
    private Color originalColor;

    [Header("拼刀设置")]
    [SerializeField]
    [Tooltip("拼刀判定窗口（延长至0.2秒，更容易触发）")]
    private float parryWindow = 0.2f;

    [SerializeField]
    [Tooltip("拼刀后退力度（稍减小，避免后退过远）")]
    private float parryForce = 8f;

    [SerializeField] private Vector2 parryUpForce = new Vector2(0, 3f);

    [SerializeField]
    [Tooltip("是否放宽拼刀范围（仅检测单向范围）")]
    private bool relaxParryRange = true;

    [SerializeField]
    [Tooltip("拼刀窗口额外延长时间")]
    private float parryWindowExtra = 0.05f;

    private bool isParrying;
    private bool isInParryWindow;
    private float parryWindowEndTime;

    [Header("受击后退设置")]
    [SerializeField] private float hurtForce = 8f;
    [SerializeField] private float hurtDuration = 0.2f;
    [SerializeField] private Vector2 hurtUpForce = new Vector2(0, 2f);

    [Header("攻击检测")]
    [SerializeField] private Transform attackCheck;
    [SerializeField] private float attackRange = 0.8f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private Color attackRangeGizmoColor = Color.blue;

    [Header("攻击碰撞体（可选）")]
    [SerializeField] private Collider2D attackHitbox;

    [Header("移动参数")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;

    [Header("重力设置")]
    [SerializeField] private float gravityScale = 2f;
    [SerializeField] private float fallMultiplier = 2.5f;

    [Header("地面检测")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float checkRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("攻击设置")]
    [SerializeField] private float attackDuration = 0.3f;

    [Header("跳跃设置")]
    [SerializeField] private int maxJumpCount = 2;

    [Header("组件")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator anim;

    // 状态变量
    private float horizontalInput;
    private bool isGrounded;
    private bool isAttacking;
    private PlayerState currentState;
    private PlayerState preAttackState;
    private int currentJumpCount;

    // 公开属性
    public PlayerNumber PlayerNumber => playerNumber;
    public bool IsInParryWindow => isInParryWindow;
    public bool IsAttacking => isAttacking;

    private void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (anim == null) anim = GetComponent<Animator>();
        if (sr == null) sr = GetComponent<SpriteRenderer>();

        if (attackHitbox != null) attackHitbox.enabled = false;

        if (sr != null) originalColor = sr.color;

        rb.gravityScale = gravityScale;
        currentState = PlayerState.Idle;
        currentJumpCount = 0;
        currentHealth = maxHealth;
        isHurt = false;
        isParrying = false;
        isInParryWindow = false;
    }

    private void Update()
    {
        if (isDead || isHurt || isParrying) return;

        // 检查拼刀窗口是否过期
        if (isInParryWindow && Time.time > parryWindowEndTime)
        {
            isInParryWindow = false;
        }

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
        if (isGrounded)
        {
            currentJumpCount = 0;
            anim.SetBool("IsJumping", false);
        }

        // 攻击输入
        if (Input.GetKeyDown(GetAttackKey()) && !isAttacking)
        {
            Shuangren otherPlayer = FindOtherPlayer();
            // 第一步：检测对方是否在攻击范围内（放宽版）
            if (otherPlayer != null && IsMutualInAttackRange(otherPlayer))
            {
                // 第二步：检测对方是否在拼刀窗口期内
                if (otherPlayer.IsInParryWindow)
                {
                    TriggerMutualParry(otherPlayer);
                    return;
                }
            }

            // 普通攻击：开启延长的拼刀窗口
            StartAttackWithParryWindow();
        }

        // 跳跃输入
        if (Input.GetKeyDown(GetJumpKey()) && currentJumpCount < maxJumpCount && !isAttacking)
        {
            Jump();
        }

        // 重力调整
        if (!isGrounded && rb.velocity.y < 0)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }

        if (!isAttacking)
        {
            currentState = !isGrounded ? PlayerState.Jump :
                           (Mathf.Abs(horizontalInput) > 0.1f ? PlayerState.Run : PlayerState.Idle);
        }

        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        if (isDead || isAttacking || isHurt || isParrying) return;
        DetectMoveInput();
        Move();
    }

    #region 拼刀逻辑（终极修复变量重复 + 放宽难度）
    // 检测拼刀范围（变量提升到方法顶部，彻底解决重复定义）
    private bool IsMutualInAttackRange(Shuangren otherPlayer)
    {
        // 1. 提前声明所有需要的变量，避免重复定义
        float selfToOtherDistance = 0f;
        float otherToSelfDistance = 0f;

        // 2. 基础校验
        if (attackCheck == null || otherPlayer == null || otherPlayer.attackCheck == null)
            return false;

        // 3. 计算自身到对方的距离（只算一次，提升性能）
        selfToOtherDistance = Vector2.Distance(attackCheck.position, otherPlayer.transform.position);

        // 4. 放宽模式：仅检测自身→对方的范围（更容易触发）
        if (relaxParryRange)
        {
            return selfToOtherDistance <= attackRange;
        }

        // 5. 严格模式：双向范围检测
        otherToSelfDistance = Vector2.Distance(otherPlayer.attackCheck.position, transform.position);
        return selfToOtherDistance <= attackRange && otherToSelfDistance <= otherPlayer.attackRange;
    }

    // 启动攻击并开启延长的拼刀窗口
    private void StartAttackWithParryWindow()
    {
        isAttacking = true;
        currentState = PlayerState.Attack;
        preAttackState = currentState;
        rb.velocity = new Vector2(0, rb.velocity.y);

        // 延长拼刀窗口：基础0.2秒 + 额外0.05秒 = 0.25秒
        isInParryWindow = true;
        parryWindowEndTime = Time.time + parryWindow + parryWindowExtra;

        if (attackHitbox != null) attackHitbox.enabled = true;

        // 攻击检测
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackCheck.position, attackRange, enemyLayer);
        foreach (var enemy in hitEnemies)
        {
            Shuangren enemyPlayer = enemy.GetComponent<Shuangren>();
            if (enemyPlayer != null && enemyPlayer.playerNumber != this.playerNumber && !enemyPlayer.isParrying)
            {
                enemyPlayer.TakeDamage(transform);
            }
        }

        Invoke(nameof(EndAttack), attackDuration);
    }

    // 触发双方拼刀
    private void TriggerMutualParry(Shuangren otherPlayer)
    {
        // 自身拼刀状态
        isParrying = true;
        isAttacking = false;
        isInParryWindow = false;
        rb.velocity = Vector2.zero;

        // 对方拼刀状态
        otherPlayer.isParrying = true;
        otherPlayer.isAttacking = false;
        otherPlayer.isInParryWindow = false;
        otherPlayer.rb.velocity = Vector2.zero;

        // 自身后退（减小力度，更友好）
        Vector2 selfParryDir = (transform.position - otherPlayer.transform.position).normalized;
        rb.AddForce(new Vector2(selfParryDir.x * parryForce, parryUpForce.y), ForceMode2D.Impulse);

        // 对方后退
        Vector2 otherParryDir = (otherPlayer.transform.position - transform.position).normalized;
        otherPlayer.rb.AddForce(new Vector2(otherParryDir.x * parryForce, otherPlayer.parryUpForce.y), ForceMode2D.Impulse);

        // 拼刀颜色反馈（更明显）
        StartCoroutine(PlayParryColorEffect());
        otherPlayer.StartCoroutine(otherPlayer.PlayParryColorEffect());

        // 拼刀硬直缩短（更快恢复）
        Invoke(nameof(EndParry), hurtDuration * 0.8f);
        otherPlayer.Invoke(nameof(otherPlayer.EndParry), hurtDuration * 0.8f);
    }

    private void EndParry()
    {
        isParrying = false;
    }

    private Shuangren FindOtherPlayer()
    {
        Shuangren[] allPlayers = FindObjectsOfType<Shuangren>();
        foreach (Shuangren player in allPlayers)
        {
            if (player != this && !player.isDead)
            {
                return player;
            }
        }
        return null;
    }
    #endregion

    #region 受击颜色效果
    private IEnumerator PlayHurtColorEffect()
    {
        if (sr == null) yield break;
        StopCoroutine(PlayHurtColorEffect());

        for (int i = 0; i < hurtFlashCount; i++)
        {
            sr.color = hurtColor;
            yield return new WaitForSeconds(hurtColorDuration);
            sr.color = originalColor;
            if (i < hurtFlashCount - 1)
            {
                yield return new WaitForSeconds(hurtColorDuration);
            }
        }
    }

    // 增强拼刀颜色反馈（更明显）
    private IEnumerator PlayParryColorEffect()
    {
        if (sr == null) yield break;
        Color parryColor = new Color(0.2f, 0.8f, 1f);
        // 闪烁两次，更易察觉
        for (int i = 0; i < 2; i++)
        {
            sr.color = parryColor;
            yield return new WaitForSeconds(0.08f);
            sr.color = originalColor;
            yield return new WaitForSeconds(0.08f);
        }
    }
    #endregion

    #region 基础逻辑
    private void DetectMoveInput()
    {
        horizontalInput = 0;
        if (playerNumber == PlayerNumber.Player1)
        {
            if (Input.GetKey(KeyCode.A)) horizontalInput = -1;
            if (Input.GetKey(KeyCode.D)) horizontalInput = 1;
        }
        else
        {
            if (Input.GetKey(KeyCode.J)) horizontalInput = -1;
            if (Input.GetKey(KeyCode.L)) horizontalInput = 1;
        }
    }

    private KeyCode GetJumpKey()
    {
        return playerNumber == PlayerNumber.Player1 ? KeyCode.W : KeyCode.I;
    }

    private KeyCode GetAttackKey()
    {
        return playerNumber == PlayerNumber.Player1 ? KeyCode.E : KeyCode.O;
    }

    private void Move()
    {
        rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);
        if (horizontalInput != 0)
        {
            transform.localScale = new Vector3(horizontalInput > 0 ? 1 : -1, 1, 1);
        }
    }

    private void Jump()
    {
        rb.velocity = new Vector2(horizontalInput * moveSpeed, 0);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        currentJumpCount++;
        anim.SetBool("IsJumping", true);
    }

    private void EndAttack()
    {
        isAttacking = false;
        isInParryWindow = false;
        currentState = preAttackState;
        if (attackHitbox != null) attackHitbox.enabled = false;
    }

    public void TakeDamage(Transform attacker)
    {
        if (isDead || isHurt || isParrying) return;

        currentHealth--;
        StartCoroutine(PlayHurtColorEffect());
        StartHurt(attacker);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void TakeDamage()
    {
        TakeDamage(transform);
    }

    private void StartHurt(Transform attacker)
    {
        isHurt = true;
        rb.velocity = Vector2.zero;

        Vector2 hurtDirection = (transform.position - attacker.position).normalized;
        rb.AddForce(new Vector2(hurtDirection.x * hurtForce, hurtUpForce.y), ForceMode2D.Impulse);

        Invoke(nameof(EndHurt), hurtDuration);
    }

    private void EndHurt()
    {
        isHurt = false;
    }

    private void Die()
    {
        isDead = true;
        gameObject.SetActive(false);
        if (GameManager.Instance != null)
            GameManager.Instance.OnPlayerDeath(this);
    }

    public void Respawn(Vector3 spawnPos)
    {
        gameObject.SetActive(true);
        transform.position = spawnPos;
        currentHealth = maxHealth;
        isDead = false;
        isHurt = false;
        isParrying = false;
        isInParryWindow = false;
        currentState = PlayerState.Idle;
        anim.Rebind();

        if (sr != null) sr.color = originalColor;
    }

    private void UpdateAnimation()
    {
        anim.SetBool("IsRunning", currentState == PlayerState.Run);
        anim.SetBool("IsAttacking", currentState == PlayerState.Attack);
    }
    #endregion

    #region Gizmos可视化
    private void OnDrawGizmos()
    {
        // 地面检测
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
        }

        // 攻击范围（扩大后更明显）
        if (attackCheck != null)
        {
            // 拼刀有效范围（黄色）
            if (UnityEditor.EditorApplication.isPlaying == false)
            {
                Shuangren otherPlayer = FindOtherPlayer();
                if (otherPlayer != null && IsMutualInAttackRange(otherPlayer))
                {
                    Gizmos.color = new Color(1f, 1f, 0f, 0.15f);
                    Gizmos.DrawSphere(attackCheck.position, attackRange);
                }
            }

            // 基础攻击范围
            Gizmos.color = new Color(attackRangeGizmoColor.r, attackRangeGizmoColor.g, attackRangeGizmoColor.b, 0.25f);
            Gizmos.DrawSphere(attackCheck.position, attackRange);

            Gizmos.color = attackRangeGizmoColor;
            Gizmos.DrawWireSphere(attackCheck.position, attackRange);
            Gizmos.DrawSphere(attackCheck.position, 0.05f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackCheck != null)
        {
            Gizmos.color = new Color(attackRangeGizmoColor.r, attackRangeGizmoColor.g, attackRangeGizmoColor.b, 0.4f);
            Gizmos.DrawSphere(attackCheck.position, attackRange);
        }
    }
    #endregion

    // 动画事件
    public void EnableHitbox()
    {
        if (attackHitbox != null) attackHitbox.enabled = true;
    }

    public void DisableHitbox()
    {
        if (attackHitbox != null) attackHitbox.enabled = false;
    }
}

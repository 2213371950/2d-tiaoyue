//using UnityEngine;

///// <summary>
///// 玩家状态枚举
///// </summary>
//public enum PlayerState
//{
//    Idle,    // 闲置状态
//    Run,     // 奔跑状态
//    Attack,  // 攻击状态
//    Jump     // 跳跃状态（空中状态）
//}

///// <summary>
///// 玩家控制器核心脚本
///// 负责处理玩家的移动、跳跃、攻击等核心逻辑
///// </summary>
//public class PlayerController : MonoBehaviour
//{
//    [Header("移动参数")]
//    [SerializeField] private float moveSpeed = 5f;       // 移动速度
//    [SerializeField] private float jumpForce = 10f;       // 跳跃力度（建议7f以上，避免跳跃过矮）

//    [Header("重力相关参数")]
//    [SerializeField] private float gravityScale = 2f;    // 重力缩放（可根据手感调整）
//    [SerializeField] private float fallMultiplier = 2.5f; // 下落系数（让下落更快，手感更实）

//    [Header("地面检测")]
//    [SerializeField] private Transform groundCheck;      // 地面检测点
//    [SerializeField] private float checkRadius = 0.2f;   // 检测半径
//    [SerializeField] private LayerMask groundLayer;      // 地面图层

//    [Header("攻击参数")]
//    [SerializeField] private float attackDuration = 0.3f;// 攻击持续时间
//    [SerializeField] private KeyCode attackKey = KeyCode.J; // 攻击按键

//    [Header("跳跃设置")]
//    [SerializeField] private int maxJumpCount = 2;       // 最大跳跃次数（2为二段跳）

//    [Header("组件引用")]
//    [SerializeField] private Rigidbody2D rb;             // 刚体组件
//    [SerializeField] private Animator anim;              // 动画组件

//    private float horizontalInput;       // 水平输入轴
//    private bool isGrounded;             // 是否在地面
//    private bool isAttacking;            // 是否正在攻击
//    private PlayerState currentState;    // 当前状态
//    private PlayerState preAttackState;  // 攻击前的状态
//    private int currentJumpCount;        // 当前跳跃次数

//    private void Start()
//    {
//        // 自动获取组件（防止手动赋值遗漏）
//        if (rb == null)
//            rb = GetComponent<Rigidbody2D>();
//        if (anim == null)
//            anim = GetComponent<Animator>();

//        // 初始化重力缩放（影响整体下落速度）
//        rb.gravityScale = gravityScale;

//        // 初始化状态和跳跃计数
//        currentState = PlayerState.Idle;
//        currentJumpCount = 0;
//    }

//    private void Update()
//    {
//        // 检测是否在地面（圆形检测）
//        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);

//        // 落地时重置跳跃次数和跳跃动画
//        if (isGrounded)
//        {
//            currentJumpCount = 0;
//            anim.SetBool("IsJumping", false);
//        }

//        // 攻击逻辑（攻击中无法再次攻击）
//        if (Input.GetKeyDown(attackKey) && !isAttacking)
//        {
//            StartAttack();
//        }

//        // 跳跃逻辑（攻击中无法跳跃）
//        if (Input.GetKeyDown(KeyCode.Space) && currentJumpCount < maxJumpCount && !isAttacking)
//        {
//            Jump();
//        }

//        // 下落加速（优化跳跃手感）
//        if (!isGrounded && rb.velocity.y < 0)
//        {
//            // 下落时应用更快的重力，让下落速度更自然
//            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
//        }

//        // 更新状态（攻击状态优先级最高）
//        if (!isAttacking)
//        {
//            currentState = !isGrounded ? PlayerState.Jump :
//                           (Mathf.Abs(horizontalInput) > 0.1f ? PlayerState.Run : PlayerState.Idle);
//        }

//        // 更新动画状态
//        UpdateAnimation();
//    }

//    private void FixedUpdate()
//    {
//        // 攻击中禁止移动
//        if (!isAttacking)
//        {
//            Move();
//        }
//    }

//    /// <summary>
//    /// 处理玩家水平移动逻辑
//    /// </summary>
//    private void Move()
//    {
//        // 获取水平输入（-1 左 | 0 无 | 1 右）
//        horizontalInput = Input.GetAxisRaw("Horizontal");
//        // 保持垂直速度不变，只更新水平速度
//        rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);

//        // 翻转玩家朝向
//        if (horizontalInput != 0)
//        {
//            transform.localScale = new Vector3(horizontalInput > 0 ? 1 : -1, 1, 1);
//        }
//    }

//    /// <summary>
//    /// 处理玩家跳跃逻辑
//    /// </summary>
//    private void Jump()
//    {
//        rb.velocity = new Vector2(rb.velocity.x, 0); // 重置垂直速度（防止二段跳高度异常）
//        rb.velocity += new Vector2(0, jumpForce);    // 应用跳跃力
//        currentJumpCount++; // 增加跳跃计数
//        anim.SetBool("IsJumping", true); // 触发跳跃动画
//    }

//    /// <summary>
//    /// 开始攻击逻辑
//    /// 攻击时停止水平移动，保留垂直速度（防止空中攻击掉落）
//    /// </summary>
//    private void StartAttack()
//    {
//        isAttacking = true;
//        preAttackState = currentState; // 记录攻击前状态
//        currentState = PlayerState.Attack;
//        // 仅重置水平速度，保留垂直速度（避免空中攻击时直接掉落）
//        rb.velocity = new Vector2(0, rb.velocity.y);
//        // 延迟结束攻击
//        Invoke(nameof(EndAttack), attackDuration);
//    }

//    /// <summary>
//    /// 结束攻击逻辑
//    /// 恢复攻击前的状态和移动能力
//    /// </summary>
//    private void EndAttack()
//    {
//        isAttacking = false;
//        currentState = preAttackState; // 恢复攻击前的状态
//    }

//    /// <summary>
//    /// 根据当前状态更新动画参数
//    /// </summary>
//    private void UpdateAnimation()
//    {
//        switch (currentState)
//        {
//            case PlayerState.Idle:
//                anim.SetBool("IsRunning", false);
//                anim.SetBool("IsAttacking", false);
//                break;
//            case PlayerState.Run:
//                anim.SetBool("IsRunning", true);
//                anim.SetBool("IsAttacking", false);
//                break;
//            case PlayerState.Attack:
//                anim.SetBool("IsRunning", false);
//                anim.SetBool("IsAttacking", true);
//                break;
//            case PlayerState.Jump:
//                anim.SetBool("IsRunning", false);
//                anim.SetBool("IsAttacking", false);
//                break;
//        }
//    }

//    /// <summary>
//    /// 绘制地面检测区域的Gizmos（编辑器可视化）
//    /// </summary>
//    private void OnDrawGizmos()
//    {
//        if (groundCheck != null)
//        {
//            Gizmos.color = Color.red;
//            Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
//        }
//    }
//}



using UnityEngine;
using System.Collections;

/// <summary>
/// 玩家状态枚举
/// </summary>
public enum PlayerState
{
    Idle,    // 闲置状态
    Run,     // 奔跑状态
    Attack,  // 攻击状态
    Jump,    // 跳跃状态（空中状态）
    Hit      // 受击状态
}

/// <summary>
/// 玩家控制器核心脚本
/// 负责处理玩家的移动、跳跃、攻击等核心逻辑
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("移动参数")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;

    [Header("重力相关参数")]
    [SerializeField] private float gravityScale = 2f;
    [SerializeField] private float fallMultiplier = 2.5f;

    [Header("地面检测")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float checkRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("攻击参数")]
    [SerializeField] private float attackDuration = 0.3f;
    [SerializeField] private KeyCode attackKey = KeyCode.J;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private int attackDamage = 1;

    [Header("受击参数")]
    [SerializeField] private float hitDuration = 0.3f;
    [SerializeField] private float hitKnockbackForce = 5f;

    [Header("跳跃设置")]
    [SerializeField] private int maxJumpCount = 2;

    [Header("音频设置")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip jumpSound;

    [Header("组件引用")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator anim;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private float horizontalInput;
    private bool isGrounded;
    private bool isAttacking;
    private bool isHit;
    private PlayerState currentState;
    private PlayerState preAttackState;
    private int currentJumpCount;
    private Color originalColor;

    private void Start()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
        if (anim == null)
            anim = GetComponent<Animator>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        rb.gravityScale = gravityScale;
        currentState = PlayerState.Idle;
        currentJumpCount = 0;

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    private void Update()
    {
        if (isHit)
            return;

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);

        if (isGrounded)
        {
            currentJumpCount = 0;
            anim.SetBool("IsJumping", false);
        }

        if (Input.GetKeyDown(attackKey) && !isAttacking)
        {
            StartAttack();
        }

        if (Input.GetKeyDown(KeyCode.Space) && currentJumpCount < maxJumpCount && !isAttacking && !isHit)
        {
            Jump();
        }

        if (!isGrounded && rb.velocity.y < 0)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }

        if (!isAttacking && !isHit)
        {
            currentState = !isGrounded ? PlayerState.Jump :
                           (Mathf.Abs(horizontalInput) > 0.1f ? PlayerState.Run : PlayerState.Idle);
        }

        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        if (!isAttacking && !isHit)
        {
            Move();
        }
    }

    private void Move()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);

        if (horizontalInput != 0)
        {
            transform.localScale = new Vector3(horizontalInput > 0 ? 1 : -1, 1, 1);
        }
    }

    private void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, 0);
        rb.velocity += new Vector2(0, jumpForce);
        currentJumpCount++;
        anim.SetBool("IsJumping", true);
        PlaySound(jumpSound);
    }

    private void StartAttack()
    {
        isAttacking = true;
        preAttackState = currentState;
        currentState = PlayerState.Attack;

        rb.velocity = new Vector2(0, rb.velocity.y);
        PlaySound(attackSound);

        Invoke(nameof(PerformAttack), 0.1f);
        Invoke(nameof(EndAttack), attackDuration);
    }

    private void PerformAttack()
    {
        if (attackPoint == null)
        {
            Debug.LogWarning("未设置攻击判定点(attackPoint)！");
            return;
        }

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                enemyAI.TakeDamage(attackDamage);
                Debug.Log($"击中敌人：{enemy.name}，伤害：{attackDamage}");
            }
        }

        if (hitEnemies.Length > 0)
        {
            Debug.Log($"攻击命中 {hitEnemies.Length} 个敌人");
        }
    }

    private void EndAttack()
    {
        isAttacking = false;
        currentState = preAttackState;
    }

    /// <summary>
    /// 玩家受到伤害
    /// </summary>
    public void TakeDamage(int damage)
    {
        if (isHit) return;

        Debug.Log($"玩家受到 {damage} 点伤害");

        isHit = true;
        currentState = PlayerState.Hit;

        PlaySound(hitSound);
        StartCoroutine(HitFlash());

        Vector2 knockbackDir = new Vector2(-transform.localScale.x, 0.5f).normalized;
        rb.velocity = Vector2.zero;
        rb.AddForce(knockbackDir * hitKnockbackForce, ForceMode2D.Impulse);

        Invoke(nameof(EndHit), hitDuration);
    }

    private void EndHit()
    {
        isHit = false;
        currentState = PlayerState.Idle;

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
    }

    private IEnumerator HitFlash()
    {
        if (spriteRenderer == null) yield break;

        float flashDuration = 0.1f;
        int flashCount = 3;

        for (int i = 0; i < flashCount; i++)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(flashDuration);
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void UpdateAnimation()
    {
        switch (currentState)
        {
            case PlayerState.Idle:
                anim.SetBool("IsRunning", false);
                anim.SetBool("IsAttacking", false);
                anim.SetBool("IsHit", false);
                break;
            case PlayerState.Run:
                anim.SetBool("IsRunning", true);
                anim.SetBool("IsAttacking", false);
                anim.SetBool("IsHit", false);
                break;
            case PlayerState.Attack:
                anim.SetBool("IsRunning", false);
                anim.SetBool("IsAttacking", true);
                anim.SetBool("IsHit", false);
                break;
            case PlayerState.Jump:
                anim.SetBool("IsRunning", false);
                anim.SetBool("IsAttacking", false);
                anim.SetBool("IsHit", false);
                break;
            case PlayerState.Hit:
                anim.SetBool("IsRunning", false);
                anim.SetBool("IsAttacking", false);
                anim.SetBool("IsHit", true);
                break;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
        }

        if (attackPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}
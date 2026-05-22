////using UnityEngine;

////public enum BoarState
////{
////    Idle,    // 待机状态
////    Hurt     // 受击状态
////}

////public class BoarAI : MonoBehaviour
////{
////    [Header("状态参数")]
////    [SerializeField] private float hurtDuration = 0.5f; // 受击硬直时间

////    [Header("组件引用")]
////    [SerializeField] private Animator anim;
////    [SerializeField] private Transform hurtCheck;      // 受击检测点
////    [SerializeField] private float hurtCheckRadius = 0.5f; // 受击检测范围
////    [SerializeField] private LayerMask attackLayer;  // 攻击层

////    private BoarState currentState;
////    private bool isHurt;             // 是否处于受击硬直中

////    private void Start()
////    {
////        currentState = BoarState.Idle;

////        // 获取动画组件
////        if (anim == null)
////            anim = GetComponent<Animator>();
////    }

////    private void Update()
////    {
////        switch (currentState)
////        {
////            case BoarState.Idle:
////                // 待机状态下检测是否受击
////                CheckPlayerAttack();
////                break;

////            case BoarState.Hurt:
////                // 受击状态不执行其他操作
////                break;
////        }

////        UpdateAnimation();
////    }

////    // 切换到受击状态
////    private void SwitchToHurt()
////    {
////        if (currentState == BoarState.Hurt) return;

////        currentState = BoarState.Hurt;
////        isHurt = true;
////        anim.SetTrigger("Hurt");
////        Invoke(nameof(EndHurt), hurtDuration);
////    }

////    // 结束受击状态，返回待机
////    private void EndHurt()
////    {
////        isHurt = false;
////        currentState = BoarState.Idle;
////    }

////    // 更新动画
////    private void UpdateAnimation()
////    {
////        anim.SetBool("IsIdle", currentState == BoarState.Idle);
////    }

////    // 检测玩家攻击
////    private void CheckPlayerAttack()
////    {
////        if (isHurt) return; // 已经处于受击状态，避免重复检测

////        // 检测攻击层碰撞体是否接触到受击检测点
////        Collider2D[] hitAttacks = Physics2D.OverlapCircleAll(
////            hurtCheck.position,
////            hurtCheckRadius,
////            attackLayer
////        );

////        if (hitAttacks.Length > 0)
////        {
////            SwitchToHurt();
////        }
////    }

////    private void OnDrawGizmosSelected()
////    {
////        // 绘制受击检测范围
////        if (hurtCheck != null)
////        {
////            Gizmos.color = Color.red;
////            Gizmos.DrawWireSphere(hurtCheck.position, hurtCheckRadius);
////        }
////    }
////}

//using UnityEngine;

//public enum BoarState
//{
//    Idle,    // 待机状态
//    Patrol,  // 巡逻状态
//    Hurt,    // 受击状态
//    Dead     // 死亡状态（无动画，仅逻辑）
//}

//public class BoarAI : MonoBehaviour
//{
//    [Header("状态参数")]
//    [SerializeField] private float hurtDuration = 0.5f; // 受击硬直时间
//    [SerializeField] private int maxHp = 3;            // 最大生命值

//    [Header("巡逻参数")]
//    [SerializeField] private float patrolSpeed = 2f;   // 巡逻移动速度
//    [SerializeField] private float patrolRange = 5f;   // 巡逻范围（左右距离）
//    [SerializeField] private float idleTime = 2f;      // 巡逻点待机时间

//    [Header("组件引用")]
//    [SerializeField] private Animator anim;
//    [SerializeField] private Transform hurtCheck;      // 受击检测点
//    [SerializeField] private float hurtCheckRadius = 0.5f; // 受击检测范围
//    [SerializeField] private LayerMask attackLayer;    // 攻击层

//    private BoarState currentState;
//    private bool isHurt;             // 是否处于受击硬直中
//    private int currentHp;          // 当前生命值

//    // 巡逻相关
//    private Vector2 startPos;       // 初始位置（巡逻中心点）
//    private float idleTimer;        // 待机计时器
//    private int moveDir = 1;        // 移动方向 1=右 -1=左

//    private void Start()
//    {
//        currentState = BoarState.Patrol;
//        currentHp = maxHp;
//        startPos = transform.position; // 记录出生点为巡逻中心
//        idleTimer = idleTime;

//        // 获取动画组件
//        if (anim == null)
//            anim = GetComponent<Animator>();

//        // 初始化朝向
//        FlipToMoveDir();
//    }

//    private void Update()
//    {
//        // 死亡状态：直接禁用所有逻辑和动画，防止出错
//        if (currentState == BoarState.Dead)
//        {
//            return;
//        }

//        // 检测受击（所有状态都可以被攻击）
//        CheckPlayerAttack();

//        switch (currentState)
//        {
//            case BoarState.Idle:
//                IdleState();
//                break;

//            case BoarState.Patrol:
//                PatrolState();
//                break;

//            case BoarState.Hurt:
//                // 受击状态不执行移动逻辑
//                break;
//        }

//        UpdateAnimation();
//    }

//    #region 核心状态逻辑
//    /// <summary>
//    /// 待机状态：计时结束后继续巡逻
//    /// </summary>
//    private void IdleState()
//    {
//        idleTimer -= Time.deltaTime;
//        if (idleTimer <= 0)
//        {
//            SwitchToPatrol();
//        }
//    }

//    /// <summary>
//    /// 巡逻状态：在范围内移动，到达边界后掉头+待机
//    /// </summary>
//    private void PatrolState()
//    {
//        // 移动方向与朝向保持一致
//        transform.Translate(Vector2.right * moveDir * patrolSpeed * Time.deltaTime);

//        // 检测是否超出巡逻范围
//        float distanceFromStart = Mathf.Abs(transform.position.x - startPos.x);
//        if (distanceFromStart >= patrolRange)
//        {
//            // 先反转方向
//            moveDir *= -1;
//            // 再根据新方向翻转角色
//            FlipToMoveDir();
//            // 切换到待机
//            SwitchToIdle();
//        }
//    }
//    #endregion

//    #region 状态切换
//    /// <summary>
//    /// 切换到待机
//    /// </summary>
//    private void SwitchToIdle()
//    {
//        currentState = BoarState.Idle;
//        idleTimer = idleTime;
//    }

//    /// <summary>
//    /// 切换到巡逻
//    /// </summary>
//    private void SwitchToPatrol()
//    {
//        currentState = BoarState.Patrol;
//        FlipToMoveDir(); // 恢复巡逻时，保证面朝移动方向
//    }

//    /// <summary>
//    /// 切换到受击状态
//    /// </summary>
//    private void SwitchToHurt()
//    {
//        if (currentState == BoarState.Dead || currentState == BoarState.Hurt) return;

//        currentState = BoarState.Hurt;
//        isHurt = true;
//        anim.SetTrigger("Hurt");
//        Invoke(nameof(EndHurt), hurtDuration);
//    }

//    /// <summary>
//    /// 结束受击，根据当前情况返回巡逻或待机
//    /// </summary>
//    private void EndHurt()
//    {
//        isHurt = false;
//        // 硬直结束后，继续巡逻
//        SwitchToPatrol();
//    }

//    /// <summary>
//    /// 切换到死亡状态（无动画，直接销毁）
//    /// </summary>
//    private void SwitchToDead()
//    {
//        currentState = BoarState.Dead;
//        // 禁用碰撞体，防止继续被攻击
//        Collider2D col = GetComponent<Collider2D>();
//        if (col != null)
//            col.enabled = false;
//        // 直接销毁物体（你也可以改成隐藏）
//        Destroy(gameObject);
//    }
//    #endregion

//    #region 工具方法
//    /// <summary>
//    /// 根据当前moveDir翻转角色，面朝移动方向
//    /// </summary>
//    private void FlipToMoveDir()
//    {
//        Vector3 scale = transform.localScale;
//        scale.x = Mathf.Abs(scale.x) * moveDir;
//        transform.localScale = scale;
//    }

//    /// <summary>
//    /// 更新动画参数，完全适配你现有的Animator
//    /// </summary>
//    private void UpdateAnimation()
//    {
//        // 巡逻/走路状态
//        bool isWalking = currentState == BoarState.Patrol;
//        // 待机状态
//        bool isIdle = currentState == BoarState.Idle;

//        anim.SetBool("IsWalking", isWalking);
//        anim.SetBool("IsIdle", isIdle);
//    }

//    /// <summary>
//    /// 检测玩家攻击
//    /// </summary>
//    private void CheckPlayerAttack()
//    {
//        if (isHurt || currentState == BoarState.Dead) return;

//        Collider2D[] hitAttacks = Physics2D.OverlapCircleAll(
//            hurtCheck.position,
//            hurtCheckRadius,
//            attackLayer
//        );

//        if (hitAttacks.Length > 0)
//        {
//            TakeDamage();
//        }
//    }

//    /// <summary>
//    /// 受到伤害
//    /// </summary>
//    private void TakeDamage()
//    {
//        currentHp--;
//        if (currentHp <= 0)
//        {
//            SwitchToDead();
//        }
//        else
//        {
//            SwitchToHurt();
//        }
//    }
//    #endregion

//    // 绘制检测范围和巡逻范围
//    private void OnDrawGizmosSelected()
//    {
//        // 受击检测范围
//        if (hurtCheck != null)
//        {
//            Gizmos.color = Color.red;
//            Gizmos.DrawWireSphere(hurtCheck.position, hurtCheckRadius);
//        }

//        // 巡逻范围
//        Gizmos.color = Color.blue;
//        Vector3 center = Application.isPlaying ? startPos : transform.position;
//        Gizmos.DrawLine(new Vector3(center.x - patrolRange, center.y, center.z),
//                        new Vector3(center.x + patrolRange, center.y, center.z));
//    }
//}


using UnityEngine;

public enum BoarState
{
    Idle,    // 待机状态
    Patrol,  // 巡逻状态
    Hurt,    // 受击状态
    Dead     // 死亡状态（无动画，仅逻辑）
}

public class BoarAI : MonoBehaviour
{
    [Header("状态参数")]
    [SerializeField] private float hurtDuration = 0.5f; // 受击硬直时间
    [SerializeField] private int maxHp = 3;            // 最大生命值

    [Header("巡逻参数")]
    [SerializeField] private float patrolSpeed = 2f;   // 巡逻移动速度
    [SerializeField] private float patrolRange = 5f;   // 巡逻范围（左右距离）
    [SerializeField] private float idleTime = 2f;      // 巡逻点待机时间

    [Header("组件引用")]
    [SerializeField] private Animator anim;
    [SerializeField] private Transform hurtCheck;      // 受击检测点
    [SerializeField] private float hurtCheckRadius = 0.5f; // 受击检测范围
    [SerializeField] private LayerMask attackLayer;    // 攻击层

    private BoarState currentState;
    private bool isHurt;             // 是否处于受击硬直中
    private int currentHp;          // 当前生命值

    // 巡逻相关
    private Vector2 startPos;       // 初始位置（巡逻中心点）
    private float idleTimer;        // 待机计时器
    private int moveDir = -1;       // 初始移动方向：-1=左，1=右
    private bool isFacingLeft = true; // 野猪默认面朝左

    private void Start()
    {
        currentState = BoarState.Patrol;
        currentHp = maxHp;
        startPos = transform.position; // 记录出生点为巡逻中心
        idleTimer = idleTime;

        // 自动获取动画组件（如果没手动赋值）
        if (anim == null)
            anim = GetComponent<Animator>();

        // 初始化朝向（不强制翻转，只记录当前方向）
        if (transform.localScale.x < 0)
            isFacingLeft = true;
        else
            isFacingLeft = false;

        // 初始状态：面朝左，向左移动，不需要翻转
    }

    private void Update()
    {
        // 死亡状态：直接禁用所有逻辑和动画，防止出错
        if (currentState == BoarState.Dead)
        {
            return;
        }

        // 检测受击（所有状态都可以被攻击）
        CheckPlayerAttack();

        switch (currentState)
        {
            case BoarState.Idle:
                IdleState();
                break;

            case BoarState.Patrol:
                PatrolState();
                break;

            case BoarState.Hurt:
                // 受击状态不执行移动逻辑
                break;
        }

        UpdateAnimation();
    }

    #region 核心状态逻辑
    /// <summary>
    /// 待机状态：计时结束后继续巡逻
    /// </summary>
    private void IdleState()
    {
        idleTimer -= Time.deltaTime;
        if (idleTimer <= 0)
        {
            SwitchToPatrol();
        }
    }

    /// <summary>
    /// 巡逻状态：在范围内移动，到达边界后掉头+待机
    /// </summary>
    private void PatrolState()
    {
        // 移动方向与朝向保持一致
        transform.Translate(Vector2.right * moveDir * patrolSpeed * Time.deltaTime);

        // 检测是否超出巡逻范围
        float distanceFromStart = Mathf.Abs(transform.position.x - startPos.x);
        if (distanceFromStart >= patrolRange)
        {
            // 先反转方向
            moveDir *= -1;
            // 再根据新方向翻转角色
            FlipToMoveDir();
            // 切换到待机
            SwitchToIdle();
        }
    }
    #endregion

    #region 状态切换
    /// <summary>
    /// 切换到待机
    /// </summary>
    private void SwitchToIdle()
    {
        currentState = BoarState.Idle;
        idleTimer = idleTime;
    }

    /// <summary>
    /// 切换到巡逻
    /// </summary>
    private void SwitchToPatrol()
    {
        currentState = BoarState.Patrol;
        FlipToMoveDir(); // 恢复巡逻时，保证面朝移动方向
    }

    /// <summary>
    /// 切换到受击状态
    /// </summary>
    private void SwitchToHurt()
    {
        if (currentState == BoarState.Dead || currentState == BoarState.Hurt) return;

        currentState = BoarState.Hurt;
        isHurt = true;
        anim.SetTrigger("Hurt");
        Invoke(nameof(EndHurt), hurtDuration);
    }

    /// <summary>
    /// 结束受击，根据当前情况返回巡逻或待机
    /// </summary>
    private void EndHurt()
    {
        isHurt = false;
        // 硬直结束后，继续巡逻
        SwitchToPatrol();
    }

    /// <summary>
    /// 切换到死亡状态（无动画，直接销毁）
    /// </summary>
    private void SwitchToDead()
    {
        currentState = BoarState.Dead;
        // 禁用碰撞体，防止继续被攻击
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;
        // 直接销毁物体（你也可以改成隐藏）
        Destroy(gameObject);
    }
    #endregion

    #region 工具方法
    /// <summary>
    /// 根据当前moveDir翻转角色，面朝移动方向（适配原始面朝左的美术）
    /// </summary>
    private void FlipToMoveDir()
    {
        // 逻辑：移动方向向左（-1）→ 面朝左，不翻转；移动方向向右（1）→ 面朝右，翻转
        if (moveDir == 1 && isFacingLeft)
        {
            // 向右移动，翻转角色
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
            isFacingLeft = false;
        }
        else if (moveDir == -1 && !isFacingLeft)
        {
            // 向左移动，翻转角色恢复原始朝向
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
            isFacingLeft = true;
        }
    }

    /// <summary>
    /// 更新动画参数，完全适配你现有的Animator
    /// </summary>
    private void UpdateAnimation()
    {
        // 巡逻/走路状态
        bool isWalking = currentState == BoarState.Patrol;
        // 待机状态
        bool isIdle = currentState == BoarState.Idle;

        anim.SetBool("IsWalking", isWalking);
        anim.SetBool("IsIdle", isIdle);
    }

    /// <summary>
    /// 检测玩家攻击
    /// </summary>
    private void CheckPlayerAttack()
    {
        if (isHurt || currentState == BoarState.Dead) return;

        Collider2D[] hitAttacks = Physics2D.OverlapCircleAll(
            hurtCheck.position,
            hurtCheckRadius,
            attackLayer
        );

        if (hitAttacks.Length > 0)
        {
            TakeDamage();
        }
    }

    /// <summary>
    /// 受到伤害
    /// </summary>
    private void TakeDamage()
    {
        currentHp--;
        if (currentHp <= 0)
        {
            SwitchToDead();
        }
        else
        {
            SwitchToHurt();
        }
    }
    #endregion

    // 绘制检测范围和巡逻范围
    private void OnDrawGizmosSelected()
    {
        // 受击检测范围
        if (hurtCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(hurtCheck.position, hurtCheckRadius);
        }

        // 巡逻范围
        Gizmos.color = Color.blue;
        Vector3 center = Application.isPlaying ? startPos : transform.position;
        Gizmos.DrawLine(new Vector3(center.x - patrolRange, center.y, center.z),
                        new Vector3(center.x + patrolRange, center.y, center.z));
    }
}

////using UnityEngine;

////public class EnemyAI : MonoBehaviour
////{
////    // ����״̬ö��
////    private enum EnemyState { Idle, Walk, Chase, Attack }

////    [Header("Ѳ������")]
////    public float patrolRange = 5f;      // Ѳ�߷�Χ����startXΪ���ģ����Ҹ����죩
////    public float patrolInterval = 2f;   // Ѳ�߼��ʱ��
////    public float moveSpeed = 2f;        // Ѳ���ƶ��ٶ�

////    [Header("ս������")]
////    public float attackRange = 1f;      // ��������
////    public float chaseSpeed = 4f;       // ׷���ٶ�
////    public float attackCooldown = 1.5f; // ������ȴʱ��
////    public LayerMask playerLayer;       // ���ͼ��

////    [Header("�����ϰ���������")]
////    public float groundCheckDistance = 0.5f;
////    public LayerMask groundLayer;
////    public Vector2 groundCheckOffset = new(0.3f, 0.2f);

////    [Header("��������")]
////    public bool debugMode = true;
////    public bool drawGroundCheckGizmos = true;
////    private float lastDebugLogTime;
////    public float debugLogInterval = 0.5f;

////    [Header("�������")]
////    public Animator animator;
////    public Transform skeletonRoot;
////    public Transform attackPoint;
////    public CircleCollider2D sightCollider;

////    // ˽�б���
////    private EnemyState currentState;
////    private float idleTimer;
////    private float currentAttackCooldown;
////    private float startX;               // Ѳ�����X����
////    private float leftPatrolBound;      // Ѳ����߽磨startX - patrolRange��
////    private float rightPatrolBound;     // Ѳ���ұ߽磨startX + patrolRange��
////    private bool patrolMovingRight = true; // Ѳ��ר�ó���
////    private bool chaseMovingRight = true;  // ׷��ר�ó���
////    private Transform targetPlayer;
////    private Rigidbody2D rb;
////    private Collider2D enemyCollider;

////    void Start()
////    {
////        // �����ʼ��
////        rb = GetComponent<Rigidbody2D>();
////        enemyCollider = GetComponent<Collider2D>();

////        if (sightCollider == null)
////        {
////            sightCollider = gameObject.AddComponent<CircleCollider2D>();
////            sightCollider.isTrigger = true;
////            sightCollider.radius = 5f;
////            LogWarning("�Զ�����������ײ�壬�����ֶ������뾶");
////        }
////        else
////        {
////            sightCollider.isTrigger = true;
////        }

////        if (animator == null)
////            animator = GetComponent<Animator>();

////        if (skeletonRoot == null)
////            skeletonRoot = transform;

////        if (groundLayer.value == 0)
////        {
////            LogWarning("δ����Groundͼ�㣡�ϰ����⹦�ܽ�ʧЧ������Inspector��ָ��Groundͼ��");
////        }

////        // ��ʼ��Ѳ�߽߱�
////        startX = transform.position.x;
////        leftPatrolBound = startX - patrolRange;
////        rightPatrolBound = startX + patrolRange;
////        // ��ʼ�������Ծ����ʼ����Ϊ׼��
////        patrolMovingRight = skeletonRoot.localScale.x > 0;
////        chaseMovingRight = patrolMovingRight;

////        currentState = EnemyState.Idle;
////        currentAttackCooldown = 0;
////        UpdateAnimation();

////        LogDebug($"��ʼ����� - Ѳ�߷�Χ��[{leftPatrolBound:F2}, {rightPatrolBound:F2}]");
////    }

////    void Update()
////    {
////        // ������ȴ����
////        if (currentAttackCooldown > 0)
////            currentAttackCooldown -= Time.deltaTime;

////        // ������־
////        if (debugMode && Time.time - lastDebugLogTime > debugLogInterval)
////        {
////            bool isInPatrolRange = IsInPatrolRange();
////            LogDebug($"״̬: {currentState}, Ŀ��: {GetPlayerName()}, λ��: {transform.position.x:F2}, ��Ѳ�߷�Χ��: {isInPatrolRange}, Ѳ�߳���: {(patrolMovingRight ? "��" : "��")}");
////            lastDebugLogTime = Time.time;
////        }

////        // ����׷����
////        PriorityChaseCheck();

////        // ִ�е�ǰ״̬�߼�
////        switch (currentState)
////        {
////            case EnemyState.Idle:
////                HandleIdleState();
////                break;
////            case EnemyState.Walk:
////                HandleWalkState();
////                break;
////            case EnemyState.Chase:
////                HandleChaseState();
////                break;
////            case EnemyState.Attack:
////                HandleAttackState();
////                break;
////        }
////    }

////    #region �����Ż�������׷���⣨ȫ����Ч��
////    private void PriorityChaseCheck()
////    {
////        if (targetPlayer != null && currentState != EnemyState.Attack)
////        {
////            if (currentState != EnemyState.Chase)
////            {
////                LogDebug($"[���ȼ��] ��⵽��ң���{currentState}�л���Chase");
////                currentState = EnemyState.Chase;
////                // �л�׷��ʱ��ͬ����ǰ�Ӿ�����Ϊ׷����
////                chaseMovingRight = skeletonRoot.localScale.x > 0;
////                UpdateAnimation();
////                rb.velocity = Vector2.zero;
////            }
////        }
////    }
////    #endregion

////    #region ״̬�����������߼�������Χ���ֶ�ʧ��Ҵ�����
////    private void HandleIdleState()
////    {
////        rb.velocity = Vector2.zero;
////        idleTimer += Time.deltaTime;

////        if (idleTimer >= patrolInterval)
////        {
////            currentState = EnemyState.Walk;
////            idleTimer = 0;
////            UpdateAnimation();
////            LogDebug("[Idle��Walk] ����ʱ���������ʼѲ��");
////        }
////    }

////    private void HandleWalkState()
////    {
////        // �ϰ�����
////        if (IsObstacleAhead(patrolMovingRight))
////        {
////            LogDebug("[Ѳ��״̬] ǰ����⵽Ground�ϰ���Զ���ͷ");
////            patrolMovingRight = !patrolMovingRight;
////            FlipSkeleton(patrolMovingRight);
////        }

////        // Ѳ���ƶ����ص�ǰѲ�߳�������ƶ���
////        float targetX = patrolMovingRight ? rightPatrolBound : leftPatrolBound;
////        Vector2 direction = new Vector2(targetX - transform.position.x, 0).normalized;
////        rb.velocity = new Vector2(direction.x * moveSpeed, rb.velocity.y);

////        // ����Ѳ�߽߱磬�л���Idle����ת����
////        if (Mathf.Abs(transform.position.x - targetX) < 0.15f)
////        {
////            LogDebug($"[Ѳ��״̬] ����Ѳ�߽߱� {targetX:F2}���л�������״̬");
////            currentState = EnemyState.Idle;
////            patrolMovingRight = !patrolMovingRight;
////            FlipSkeleton(patrolMovingRight);
////            UpdateAnimation();
////            rb.velocity = Vector2.zero;
////        }
////    }

////    private void HandleChaseState()
////    {
////        // �����߼�����Ҷ�ʧʱ�����ݵ�ǰλ�ô���
////        if (targetPlayer == null)
////        {
////            bool isInRange = IsInPatrolRange();
////            if (isInRange)
////            {
////                // ���1����Ѳ�߷�Χ�ڶ�ʧ��ҡ������ص�ǰѲ�߳���Ѳ��
////                LogDebug("��Ҷ�ʧ����Ѳ�߷�Χ�ڣ��� ������ǰ����Ѳ��");
////                // ͬ��Ѳ�߳���Ϊ��ʧ���ʱ���Ӿ����򣨱��ⷽ��ͻ�䣩
////                patrolMovingRight = skeletonRoot.localScale.x > 0;
////                currentState = EnemyState.Walk; // ֱ���л���Ѳ�ߣ���Idle
////                UpdateAnimation();
////                rb.velocity = Vector2.zero;
////            }
////            else
////            {
////                // ���2����Ѳ�߷�Χ�ⶪʧ��ҡ����л���Idle���ٷ�ת����
////                LogDebug("��Ҷ�ʧ����Ѳ�߷�Χ�⣩�� ��Idle���ٷ�ת����");
////                // У������Ѳ�߷�Χ�ĳ���
////                CorrectReturnDirection();
////                currentState = EnemyState.Idle; // �Ƚ���Idle״̬
////                FlipSkeleton(patrolMovingRight); // ������ת�����ط���
////                UpdateAnimation();
////                rb.velocity = Vector2.zero;
////                idleTimer = 0; // ����Idle��ʱ����ȷ��Idleʱ������
////            }
////            return;
////        }

////        // �ϰ����⣨ʹ��׷����
////        if (IsObstacleAhead(chaseMovingRight))
////        {
////            LogDebug("[׷��״̬] ǰ����⵽Ground�ϰ���Զ���ͷ");
////            chaseMovingRight = !chaseMovingRight;
////            FlipSkeleton(chaseMovingRight);
////        }

////        // ������Χ���
////        float distanceToPlayer = Vector2.Distance(transform.position, targetPlayer.position);
////        if (distanceToPlayer <= attackRange && currentAttackCooldown <= 0)
////        {
////            currentState = EnemyState.Attack;
////            UpdateAnimation();
////            return;
////        }

////        // ׷���ƶ��볯�����
////        ChaseMoveTowardsTarget(targetPlayer.position, chaseSpeed);
////        UpdateChaseFacingDirection(targetPlayer.position.x);
////    }

////    private void HandleAttackState()
////    {
////        rb.velocity = Vector2.zero;

////        if (targetPlayer == null || Vector2.Distance(transform.position, targetPlayer.position) > attackRange)
////        {
////            currentState = EnemyState.Chase;
////            UpdateAnimation();
////            return;
////        }
////    }
////    #endregion

////    #region ���Ĺ��߷���
////    /// <summary>
////    /// �жϹ��ﵱǰ�Ƿ���Ѳ�߷�Χ�ڣ������߽磩
////    /// </summary>
////    private bool IsInPatrolRange()
////    {
////        return transform.position.x >= leftPatrolBound - 0.1f && transform.position.x <= rightPatrolBound + 0.1f;
////    }

////    /// <summary>
////    /// ��Χ�ⶪʧ���ʱ��У������Ѳ�߷�Χ�ĳ���
////    /// </summary>
////    private void CorrectReturnDirection()
////    {
////        float currentX = transform.position.x;
////        // ���1������߽������������ң������ұ߽磩
////        if (currentX < leftPatrolBound)
////        {
////            patrolMovingRight = true;
////            LogDebug($"[У������] ����߽��⣬������Ϊ�ң�����Ѳ�߷�Χ��");
////        }
////        // ���2�����ұ߽��Ҳ���������󣨷�����߽磩
////        else if (currentX > rightPatrolBound)
////        {
////            patrolMovingRight = false;
////            LogDebug($"[У������] ���ұ߽��⣬������Ϊ�󣨷���Ѳ�߷�Χ��");
////        }
////    }

////    /// <summary>
////    /// ͳһ��ת���鷽��
////    /// </summary>
////    private void FlipSkeleton(bool targetRight)
////    {
////        Vector3 newScale = skeletonRoot.localScale;
////        newScale.x = targetRight ? Mathf.Abs(newScale.x) : -Mathf.Abs(newScale.x);
////        skeletonRoot.localScale = newScale;
////    }

////    /// <summary>
////    /// �ϰ����⣨֧�ִ��볯��
////    /// </summary>
////    private bool IsObstacleAhead(bool isFacingRight)
////    {
////        Vector2 rayOrigin = (Vector2)transform.position +
////                           (isFacingRight ? groundCheckOffset : -groundCheckOffset);
////        Vector2 rayDirection = isFacingRight ? Vector2.right : Vector2.left;

////        RaycastHit2D hit = Physics2D.Raycast(
////            rayOrigin,
////            rayDirection,
////            groundCheckDistance,
////            groundLayer
////        );

////        if (hit.collider != null && debugMode)
////        {
////            LogDebug($"��⵽Ground�ϰ���: {hit.collider.gameObject.name}������: {hit.distance:F2}");
////        }

////        return hit.collider != null;
////    }
////    #endregion

////    #region ��������
////    /// <summary>
////    /// ׷��ר���ƶ�����
////    /// </summary>
////    private void ChaseMoveTowardsTarget(Vector2 targetPos, float speed)
////    {
////        float directionX = targetPos.x - transform.position.x;
////        // �������������⴩ǽ
////        if ((directionX > 0 && !chaseMovingRight) || (directionX < 0 && chaseMovingRight))
////        {
////            directionX = chaseMovingRight ? 1 : -1;
////        }

////        Vector2 direction = new Vector2(directionX, 0).normalized;
////        rb.velocity = new Vector2(direction.x * speed, rb.velocity.y);
////    }

////    /// <summary>
////    /// ׷��ר�ó������
////    /// </summary>
////    private void UpdateChaseFacingDirection(float targetX)
////    {
////        bool shouldFaceRight = targetX > transform.position.x;
////        if (shouldFaceRight != chaseMovingRight)
////        {
////            chaseMovingRight = shouldFaceRight;
////            FlipSkeleton(chaseMovingRight);
////            LogDebug($"[׷��] �������Ϊ: {(chaseMovingRight ? "��" : "��")}");
////        }
////    }

////    private void UpdateAnimation()
////    {
////        if (animator == null) return;

////        animator.SetBool("IsWalking", currentState == EnemyState.Walk);
////        animator.SetBool("IsChasing", currentState == EnemyState.Chase);
////        animator.SetBool("IsAttacking", currentState == EnemyState.Attack);

////        if (currentState == EnemyState.Attack)
////        {
////            animator.SetTrigger("Attack");
////            currentAttackCooldown = attackCooldown;
////        }
////    }

////    public void OnAttackFinish()
////    {
////        if (currentState == EnemyState.Attack)
////        {
////            currentState = EnemyState.Chase;
////            UpdateAnimation();
////        }
////    }

////    private string GetPlayerName()
////    {
////        return targetPlayer != null ? targetPlayer.name : "��";
////    }
////    #endregion

////    #region ��ײ��������
////    private void OnTriggerEnter2D(Collider2D other)
////    {
////        if (((1 << other.gameObject.layer) & playerLayer) != 0)
////        {
////            targetPlayer = other.transform;
////            LogDebug($"��� {targetPlayer.name} ��������");
////        }
////    }

////    private void OnTriggerExit2D(Collider2D other)
////    {
////        if (((1 << other.gameObject.layer) & playerLayer) != 0 && other.transform == targetPlayer)
////        {
////            LogDebug($"��� {targetPlayer.name} �뿪����");
////            targetPlayer = null;
////        }
////    }

////    private void LogDebug(string message)
////    {
////        if (debugMode)
////            Debug.Log($"[{gameObject.name}] {message}", this);
////    }

////    private void LogWarning(string message)
////    {
////        Debug.LogWarning($"[{gameObject.name}] {message}", this);
////    }

////    private void OnDrawGizmosSelected()
////    {
////        // Ѳ�߷�Χ����ɫ�߿�
////        Gizmos.color = Color.red;
////        Gizmos.DrawLine(
////            new Vector3(leftPatrolBound, transform.position.y - 0.5f, 0),
////            new Vector3(leftPatrolBound, transform.position.y + 0.5f, 0)
////        );
////        Gizmos.DrawLine(
////            new Vector3(rightPatrolBound, transform.position.y - 0.5f, 0),
////            new Vector3(rightPatrolBound, transform.position.y + 0.5f, 0)
////        );
////        Gizmos.DrawLine(
////            new Vector3(leftPatrolBound, transform.position.y, 0),
////            new Vector3(rightPatrolBound, transform.position.y, 0)
////        );
////        // Ѳ������ǣ���ɫСԲȦ��
////        Gizmos.DrawWireSphere(new Vector3(startX, transform.position.y, 0), 0.3f);

////        // ������Χ����ɫԲȦ��
////        Gizmos.color = Color.yellow;
////        Vector3 attackPos = attackPoint != null ? attackPoint.position : transform.position;
////        Gizmos.DrawWireSphere(attackPos, attackRange);

////        // ���߷�Χ����ɫԲȦ��
////        Gizmos.color = Color.blue;
////        if (sightCollider != null)
////            Gizmos.DrawWireSphere(transform.position, sightCollider.radius);

////        // �����ϰ��������ߣ����ݵ�ǰ״̬��ʾ����
////        if (drawGroundCheckGizmos && Application.isPlaying)
////        {
////            bool currentFacing = currentState == EnemyState.Chase ? chaseMovingRight : patrolMovingRight;
////            Vector2 rayOrigin = (Vector2)transform.position +
////                               (currentFacing ? groundCheckOffset : -groundCheckOffset);
////            Vector2 rayDirection = currentFacing ? Vector2.right : Vector2.left;
////            Vector2 rayEnd = rayOrigin + rayDirection * groundCheckDistance;

////            Gizmos.color = IsObstacleAhead(currentFacing) ? Color.red : Color.green;
////            Gizmos.DrawLine(rayOrigin, rayEnd);
////            Gizmos.DrawWireSphere(rayOrigin, 0.05f);
////        }
////    }
////    #endregion
////}



//using UnityEngine;

///// <summary>
///// 敌人AI核心逻辑类
///// 实现敌人的巡逻、追逐、攻击状态机，包含地面检测、碰撞触发、动画同步等功能
///// </summary>
//public class EnemyAI : MonoBehaviour
//{
//    /// <summary>
//    /// 敌人状态枚举
//    /// </summary>
//    private enum EnemyState
//    {
//        Idle,    // 闲置状态
//        Walk,    // 巡逻行走状态
//        Chase,   // 追逐玩家状态
//        Attack   // 攻击玩家状态
//    }

//    [Header("巡逻配置")]
//    [Tooltip("巡逻范围（以初始X坐标为中心，左右延伸的距离）")]
//    public float patrolRange = 5f;
//    [Tooltip("巡逻闲置间隔时间（Idle状态持续时长）")]
//    public float patrolInterval = 2f;
//    [Tooltip("巡逻移动速度")]
//    public float moveSpeed = 2f;

//    [Header("战斗配置")]
//    [Tooltip("攻击触发距离")]
//    public float attackRange = 1f;
//    [Tooltip("追逐玩家速度")]
//    public float chaseSpeed = 4f;
//    [Tooltip("攻击冷却时间（两次攻击的间隔）")]
//    public float attackCooldown = 1.5f;
//    [Tooltip("玩家所在的图层（用于检测玩家触发）")]
//    public LayerMask playerLayer;

//    [Header("地面检测配置")]
//    [Tooltip("地面检测射线长度")]
//    public float groundCheckDistance = 0.5f;
//    [Tooltip("地面图层（用于检测障碍物）")]
//    public LayerMask groundLayer;
//    [Tooltip("地面检测射线偏移量（相对于敌人锚点）")]
//    public Vector2 groundCheckOffset = new(0.3f, 0.2f);

//    [Header("调试配置")]
//    [Tooltip("是否开启调试日志")]
//    public bool debugMode = true;
//    [Tooltip("是否绘制地面检测Gizmos")]
//    public bool drawGroundCheckGizmos = true;
//    [Tooltip("调试日志输出间隔（避免刷屏）")]
//    public float debugLogInterval = 0.5f;
//    private float lastDebugLogTime; // 上一次输出调试日志的时间

//    [Header("组件引用")]
//    [Tooltip("动画控制器（同步状态动画）")]
//    public Animator animator;
//    [Tooltip("敌人骨骼根节点（用于翻转朝向）")]
//    public Transform skeletonRoot;
//    [Tooltip("攻击判定点（用于绘制攻击范围）")]
//    public Transform attackPoint;
//    [Tooltip("视野碰撞体（用于检测玩家进入/离开）")]
//    public CircleCollider2D sightCollider;

//    // 私有状态变量
//    private EnemyState currentState; // 当前敌人状态
//    private float idleTimer;         // 闲置状态计时器
//    private float currentAttackCooldown; // 当前攻击冷却剩余时间
//    private float startX;            // 巡逻初始X坐标（中心点）
//    private float leftPatrolBound;   // 巡逻左边界（startX - patrolRange）
//    private float rightPatrolBound;  // 巡逻右边界（startX + patrolRange）
//    private bool patrolMovingRight;  // 巡逻时是否向右移动
//    private bool chaseMovingRight;   // 追逐时是否向右移动
//    private Transform targetPlayer;  // 目标玩家（视野内的玩家）
//    private Rigidbody2D rb;          // 刚体组件（控制移动）
//    private Collider2D enemyCollider;// 敌人碰撞体

//    /// <summary>
//    /// 初始化组件和巡逻参数
//    /// </summary>
//    void Start()
//    {
//        // 获取核心组件
//        rb = GetComponent<Rigidbody2D>();
//        enemyCollider = GetComponent<Collider2D>();

//        // 自动创建视野碰撞体（如果未配置）
//        if (sightCollider == null)
//        {
//            sightCollider = gameObject.AddComponent<CircleCollider2D>();
//            sightCollider.isTrigger = true;
//            sightCollider.radius = 5f;
//            LogWarning("自动创建视野碰撞体，默认半径5f，请手动调整");
//        }
//        else
//        {
//            sightCollider.isTrigger = true;
//        }

//        // 自动获取动画组件（如果未配置）
//        if (animator == null)
//            animator = GetComponent<Animator>();

//        // 骨骼根节点默认赋值为自身（如果未配置）
//        if (skeletonRoot == null)
//            skeletonRoot = transform;

//        // 检测地面图层配置
//        if (groundLayer.value == 0)
//        {
//            LogWarning("未配置Ground图层！地面检测功能将失效，请在Inspector中指定Ground图层");
//        }

//        // 初始化巡逻边界
//        startX = transform.position.x;
//        leftPatrolBound = startX - patrolRange;
//        rightPatrolBound = startX + patrolRange;
//        // 初始化移动朝向（根据骨骼缩放判定）
//        patrolMovingRight = skeletonRoot.localScale.x > 0;
//        chaseMovingRight = patrolMovingRight;

//        // 初始状态为闲置
//        currentState = EnemyState.Idle;
//        currentAttackCooldown = 0;
//        UpdateAnimation();

//        LogDebug($"初始化完成 - 巡逻范围：[{leftPatrolBound:F2}, {rightPatrolBound:F2}]");
//    }

//    /// <summary>
//    /// 每帧更新状态机逻辑
//    /// </summary>
//    void Update()
//    {
//        // 更新攻击冷却时间
//        if (currentAttackCooldown > 0)
//            currentAttackCooldown -= Time.deltaTime;

//        // 输出调试日志（按间隔）
//        if (debugMode && Time.time - lastDebugLogTime > debugLogInterval)
//        {
//            bool isInPatrolRange = IsInPatrolRange();
//            LogDebug($"状态: {currentState}, 目标: {GetPlayerName()}, 位置: {transform.position.x:F2}, 在巡逻范围: {isInPatrolRange}, 巡逻朝向: {(patrolMovingRight ? "右" : "左")}");
//            lastDebugLogTime = Time.time;
//        }

//        // 优先检测是否需要追逐玩家（全局优先级）
//        PriorityChaseCheck();

//        // 执行当前状态逻辑
//        switch (currentState)
//        {
//            case EnemyState.Idle:
//                HandleIdleState();
//                break;
//            case EnemyState.Walk:
//                HandleWalkState();
//                break;
//            case EnemyState.Chase:
//                HandleChaseState();
//                break;
//            case EnemyState.Attack:
//                HandleAttackState();
//                break;
//        }
//    }

//    #region 核心逻辑 - 追逐检测
//    /// <summary>
//    /// 高优先级追逐检测（玩家进入视野时强制切换到追逐状态）
//    /// </summary>
//    private void PriorityChaseCheck()
//    {
//        if (targetPlayer != null && currentState != EnemyState.Attack)
//        {
//            if (currentState != EnemyState.Chase)
//            {
//                LogDebug($"[高优先级] 发现玩家，从{currentState}切换到Chase");
//                currentState = EnemyState.Chase;
//                // 切换追逐时同步当前朝向为追逐朝向
//                chaseMovingRight = skeletonRoot.localScale.x > 0;
//                UpdateAnimation();
//                rb.velocity = Vector2.zero;
//            }
//        }
//    }
//    #endregion

//    #region 状态处理 - 核心状态机逻辑
//    /// <summary>
//    /// 处理闲置状态逻辑
//    /// </summary>
//    private void HandleIdleState()
//    {
//        // 闲置时停止移动
//        rb.velocity = Vector2.zero;
//        idleTimer += Time.deltaTime;

//        // 闲置时间达到阈值后切换到巡逻行走
//        if (idleTimer >= patrolInterval)
//        {
//            currentState = EnemyState.Walk;
//            idleTimer = 0;
//            UpdateAnimation();
//            LogDebug("[Idle→Walk] 闲置时间结束，开始巡逻");
//        }
//    }

//    /// <summary>
//    /// 处理巡逻行走状态逻辑
//    /// </summary>
//    private void HandleWalkState()
//    {
//        // 检测前方是否有障碍物（地面）
//        if (IsObstacleAhead(patrolMovingRight))
//        {
//            LogDebug("[巡逻状态] 前方发现Ground障碍物，自动掉头");
//            patrolMovingRight = !patrolMovingRight;
//            FlipSkeleton(patrolMovingRight);
//        }

//        // 计算巡逻目标点并移动
//        float targetX = patrolMovingRight ? rightPatrolBound : leftPatrolBound;
//        Vector2 direction = new Vector2(targetX - transform.position.x, 0).normalized;
//        rb.velocity = new Vector2(direction.x * moveSpeed, rb.velocity.y);

//        // 到达巡逻边界后切换回闲置状态，并反转朝向
//        if (Mathf.Abs(transform.position.x - targetX) < 0.15f)
//        {
//            LogDebug($"[巡逻状态] 到达巡逻边界 {targetX:F2}，切换到闲置状态");
//            currentState = EnemyState.Idle;
//            patrolMovingRight = !patrolMovingRight;
//            FlipSkeleton(patrolMovingRight);
//            UpdateAnimation();
//            rb.velocity = Vector2.zero;
//        }
//    }

//    /// <summary>
//    /// 处理追逐玩家状态逻辑
//    /// </summary>
//    private void HandleChaseState()
//    {
//        // 玩家丢失时的逻辑
//        if (targetPlayer == null)
//        {
//            bool isInRange = IsInPatrolRange();
//            if (isInRange)
//            {
//                // 情况1：在巡逻范围内丢失玩家 → 直接切换到巡逻行走
//                LogDebug("玩家丢失（在巡逻范围内），恢复当前方向巡逻");
//                patrolMovingRight = skeletonRoot.localScale.x > 0;
//                currentState = EnemyState.Walk;
//                UpdateAnimation();
//                rb.velocity = Vector2.zero;
//            }
//            else
//            {
//                // 情况2：超出巡逻范围丢失玩家 → 先闲置，再返回巡逻范围
//                LogDebug("玩家丢失（超出巡逻范围），Idle后返回巡逻范围");
//                CorrectReturnDirection();
//                currentState = EnemyState.Idle;
//                FlipSkeleton(patrolMovingRight);
//                UpdateAnimation();
//                rb.velocity = Vector2.zero;
//                idleTimer = 0; // 重置闲置计时器
//            }
//            return;
//        }

//        // 追逐时检测前方障碍物
//        if (IsObstacleAhead(chaseMovingRight))
//        {
//            LogDebug("[追逐状态] 前方发现Ground障碍物，自动掉头");
//            chaseMovingRight = !chaseMovingRight;
//            FlipSkeleton(chaseMovingRight);
//        }

//        // 检测是否进入攻击范围
//        float distanceToPlayer = Vector2.Distance(transform.position, targetPlayer.position);
//        if (distanceToPlayer <= attackRange && currentAttackCooldown <= 0)
//        {
//            currentState = EnemyState.Attack;
//            UpdateAnimation();
//            return;
//        }

//        // 向玩家移动
//        ChaseMoveTowardsTarget(targetPlayer.position, chaseSpeed);
//        // 更新追逐朝向
//        UpdateChaseFacingDirection(targetPlayer.position.x);
//    }

//    /// <summary>
//    /// 处理攻击状态逻辑
//    /// </summary>
//    private void HandleAttackState()
//    {
//        // 攻击时停止移动
//        rb.velocity = Vector2.zero;

//        // 玩家离开攻击范围/丢失时，切回追逐状态
//        if (targetPlayer == null || Vector2.Distance(transform.position, targetPlayer.position) > attackRange)
//        {
//            currentState = EnemyState.Chase;
//            UpdateAnimation();
//            return;
//        }
//    }
//    #endregion

//    #region 辅助方法 - 检测/判定
//    /// <summary>
//    /// 判断当前位置是否在巡逻范围内
//    /// </summary>
//    /// <returns>是否在巡逻范围</returns>
//    private bool IsInPatrolRange()
//    {
//        return transform.position.x >= leftPatrolBound - 0.1f && transform.position.x <= rightPatrolBound + 0.1f;
//    }

//    /// <summary>
//    /// 校正返回巡逻范围的方向（超出范围时）
//    /// </summary>
//    private void CorrectReturnDirection()
//    {
//        float currentX = transform.position.x;
//        // 超出左边界 → 向右返回
//        if (currentX < leftPatrolBound)
//        {
//            patrolMovingRight = true;
//            LogDebug($"[校正方向] 超出左边界，朝向改为右（返回巡逻范围）");
//        }
//        // 超出右边界 → 向左返回
//        else if (currentX > rightPatrolBound)
//        {
//            patrolMovingRight = false;
//            LogDebug($"[校正方向] 超出右边界，朝向改为左（返回巡逻范围）");
//        }
//    }

//    /// <summary>
//    /// 统一翻转骨骼朝向
//    /// </summary>
//    /// <param name="targetRight">是否朝右</param>
//    private void FlipSkeleton(bool targetRight)
//    {
//        Vector3 newScale = skeletonRoot.localScale;
//        newScale.x = targetRight ? Mathf.Abs(newScale.x) : -Mathf.Abs(newScale.x);
//        skeletonRoot.localScale = newScale;
//    }

//    /// <summary>
//    /// 检测前方是否有障碍物（地面）
//    /// </summary>
//    /// <param name="isFacingRight">当前朝向是否向右</param>
//    /// <returns>是否有障碍物</returns>
//    private bool IsObstacleAhead(bool isFacingRight)
//    {
//        // 计算射线起点（根据朝向偏移）
//        Vector2 rayOrigin = (Vector2)transform.position +
//                           (isFacingRight ? groundCheckOffset : -groundCheckOffset);
//        // 计算射线方向
//        Vector2 rayDirection = isFacingRight ? Vector2.right : Vector2.left;

//        // 发射2D射线检测地面
//        RaycastHit2D hit = Physics2D.Raycast(
//            rayOrigin,
//            rayDirection,
//            groundCheckDistance,
//            groundLayer
//        );

//        // 调试日志
//        if (hit.collider != null && debugMode)
//        {
//            LogDebug($"检测到Ground障碍物: {hit.collider.gameObject.name}，距离: {hit.distance:F2}");
//        }

//        return hit.collider != null;
//    }
//    #endregion

//    #region 辅助方法 - 移动/动画
//    /// <summary>
//    /// 追逐时向目标移动（避开障碍物）
//    /// </summary>
//    /// <param name="targetPos">目标位置</param>
//    /// <param name="speed">移动速度</param>
//    private void ChaseMoveTowardsTarget(Vector2 targetPos, float speed)
//    {
//        float directionX = targetPos.x - transform.position.x;
//        // 前方有障碍物时，按当前朝向移动（避免穿墙）
//        if ((directionX > 0 && !chaseMovingRight) || (directionX < 0 && chaseMovingRight))
//        {
//            directionX = chaseMovingRight ? 1 : -1;
//        }

//        Vector2 direction = new Vector2(directionX, 0).normalized;
//        rb.velocity = new Vector2(direction.x * speed, rb.velocity.y);
//    }

//    /// <summary>
//    /// 更新追逐时的朝向（面向玩家）
//    /// </summary>
//    /// <param name="targetX">目标X坐标</param>
//    private void UpdateChaseFacingDirection(float targetX)
//    {
//        bool shouldFaceRight = targetX > transform.position.x;
//        if (shouldFaceRight != chaseMovingRight)
//        {
//            chaseMovingRight = shouldFaceRight;
//            FlipSkeleton(chaseMovingRight);
//            LogDebug($"[追逐] 朝向更新为: {(chaseMovingRight ? "右" : "左")}");
//        }
//    }

//    /// <summary>
//    /// 更新动画状态（同步状态机与动画控制器）
//    /// </summary>
//    private void UpdateAnimation()
//    {
//        if (animator == null) return;

//        animator.SetBool("IsWalking", currentState == EnemyState.Walk);
//        animator.SetBool("IsChasing", currentState == EnemyState.Chase);
//        animator.SetBool("IsAttacking", currentState == EnemyState.Attack);

//        // 攻击状态触发攻击动画并重置冷却
//        if (currentState == EnemyState.Attack)
//        {
//            animator.SetTrigger("Attack");
//            currentAttackCooldown = attackCooldown;
//        }
//    }

//    /// <summary>
//    /// 攻击动画结束回调（需在动画控制器中配置事件）
//    /// </summary>
//    public void OnAttackFinish()
//    {
//        if (currentState == EnemyState.Attack)
//        {
//            currentState = EnemyState.Chase;
//            UpdateAnimation();
//        }
//    }

//    /// <summary>
//    /// 获取玩家名称（调试用）
//    /// </summary>
//    /// <returns>玩家名称（无则返回"无"）</returns>
//    private string GetPlayerName()
//    {
//        return targetPlayer != null ? targetPlayer.name : "无";
//    }
//    #endregion

//    #region 触发器/调试/可视化
//    /// <summary>
//    /// 玩家进入视野触发器
//    /// </summary>
//    /// <param name="other">碰撞体</param>
//    private void OnTriggerEnter2D(Collider2D other)
//    {
//        if (((1 << other.gameObject.layer) & playerLayer) != 0)
//        {
//            targetPlayer = other.transform;
//            LogDebug($"检测到 {targetPlayer.name} 进入视野");
//        }
//    }

//    /// <summary>
//    /// 玩家离开视野触发器
//    /// </summary>
//    /// <param name="other">碰撞体</param>
//    private void OnTriggerExit2D(Collider2D other)
//    {
//        if (((1 << other.gameObject.layer) & playerLayer) != 0 && other.transform == targetPlayer)
//        {
//            LogDebug($"检测到 {targetPlayer.name} 离开视野");
//            targetPlayer = null;
//        }
//    }

//    /// <summary>
//    /// 调试日志输出（带敌人名称前缀）
//    /// </summary>
//    /// <param name="message">日志内容</param>
//    private void LogDebug(string message)
//    {
//        if (debugMode)
//            Debug.Log($"[{gameObject.name}] {message}", this);
//    }

//    /// <summary>
//    /// 警告日志输出（强制输出，带敌人名称前缀）
//    /// </summary>
//    /// <param name="message">警告内容</param>
//    private void LogWarning(string message)
//    {
//        Debug.LogWarning($"[{gameObject.name}] {message}", this);
//    }

//    /// <summary>
//    /// 绘制Gizmos（场景视图可视化调试）
//    /// </summary>
//    private void OnDrawGizmosSelected()
//    {
//        // 绘制巡逻范围（红色线条）
//        Gizmos.color = Color.red;
//        Gizmos.DrawLine(
//            new Vector3(leftPatrolBound, transform.position.y - 0.5f, 0),
//            new Vector3(leftPatrolBound, transform.position.y + 0.5f, 0)
//        );
//        Gizmos.DrawLine(
//            new Vector3(rightPatrolBound, transform.position.y - 0.5f, 0),
//            new Vector3(rightPatrolBound, transform.position.y + 0.5f, 0)
//        );
//        Gizmos.DrawLine(
//            new Vector3(leftPatrolBound, transform.position.y, 0),
//            new Vector3(rightPatrolBound, transform.position.y, 0)
//        );
//        // 巡逻中心点（红色小圆）
//        Gizmos.DrawWireSphere(new Vector3(startX, transform.position.y, 0), 0.3f);

//        // 攻击范围（黄色空心圆）
//        Gizmos.color = Color.yellow;
//        Vector3 attackPos = attackPoint != null ? attackPoint.position : transform.position;
//        Gizmos.DrawWireSphere(attackPos, attackRange);

//        // 视野范围（蓝色空心圆）
//        Gizmos.color = Color.blue;
//        if (sightCollider != null)
//            Gizmos.DrawWireSphere(transform.position, sightCollider.radius);

//        // 地面检测射线（运行时绘制）
//        if (drawGroundCheckGizmos && Application.isPlaying)
//        {
//            bool currentFacing = currentState == EnemyState.Chase ? chaseMovingRight : patrolMovingRight;
//            Vector2 rayOrigin = (Vector2)transform.position +
//                               (currentFacing ? groundCheckOffset : -groundCheckOffset);
//            Vector2 rayDirection = currentFacing ? Vector2.right : Vector2.left;
//            Vector2 rayEnd = rayOrigin + rayDirection * groundCheckDistance;

//            // 有障碍物为红色，无则为绿色
//            Gizmos.color = IsObstacleAhead(currentFacing) ? Color.red : Color.green;
//            Gizmos.DrawLine(rayOrigin, rayEnd);
//            Gizmos.DrawWireSphere(rayOrigin, 0.05f);
//        }
//    }
//    #endregion
//}

using UnityEngine;

/// <summary>
/// 敌人AI核心逻辑类
/// 实现敌人的巡逻、追逐、攻击状态机，包含地面检测、碰撞触发、动画同步等功能
/// </summary>
public class EnemyAI : MonoBehaviour
{
    /// <summary>
    /// 敌人状态枚举
    /// </summary>
    private enum EnemyState
    {
        Idle,    // 闲置状态
        Walk,    // 巡逻行走状态
        Chase,   // 追逐玩家状态
        Attack   // 攻击玩家状态
    }

    [Header("巡逻配置")]
    [Tooltip("巡逻范围（以初始X坐标为中心，左右延伸的距离）")]
    public float patrolRange = 5f;
    [Tooltip("巡逻闲置间隔时间（Idle状态持续时长）")]
    public float patrolInterval = 2f;
    [Tooltip("巡逻移动速度")]
    public float moveSpeed = 2f;

    [Header("战斗配置")]
    [Tooltip("攻击触发距离")]
    public float attackRange = 1f;
    [Tooltip("追逐玩家速度")]
    public float chaseSpeed = 4f;
    [Tooltip("攻击冷却时间（两次攻击的间隔）")]
    public float attackCooldown = 1.5f;
    [Tooltip("攻击伤害值")]
    public int attackDamage = 1;
    [Tooltip("玩家所在的图层（用于检测玩家触发）")]
    public LayerMask playerLayer;

    [Header("受击检测配置")]
    [Tooltip("受击检测碰撞体（用于接收玩家攻击）")]
    public Collider2D hitCollider;
    [Tooltip("受击检测点偏移（如果没有指定碰撞体，使用圆形检测）")]
    public Vector2 hitPointOffset = Vector2.zero;
    [Tooltip("受击检测半径")]
    public float hitCheckRadius = 0.5f;

    [Header("地面检测配置")]
    [Tooltip("地面检测射线长度")]
    public float groundCheckDistance = 0.5f;
    [Tooltip("地面图层（用于检测障碍物）")]
    public LayerMask groundLayer;
    [Tooltip("地面检测射线偏移量（相对于敌人锚点）")]
    public Vector2 groundCheckOffset = new(0.3f, 0.2f);

    [Header("受击特效配置")]
    [Tooltip("受击闪烁颜色")]
    public Color hitColor = Color.white;
    [Tooltip("受击闪烁持续时间")]
    public float hitFlashDuration = 0.1f;
    [Tooltip("受击闪烁次数")]
    public int hitFlashCount = 2;
    [Tooltip("受击击退力度")]
    public float hitKnockbackForce = 5f;
    [Tooltip("精灵渲染器（用于受击特效）")]
    public SpriteRenderer[] spriteRenderers;

    [Header("调试配置")]
    [Tooltip("是否开启调试日志")]
    public bool debugMode = true;
    [Tooltip("是否绘制地面检测Gizmos")]
    public bool drawGroundCheckGizmos = true;
    [Tooltip("调试日志输出间隔（避免刷屏）")]
    public float debugLogInterval = 0.5f;
    private float lastDebugLogTime;

    [Header("组件引用")]
    [Tooltip("动画控制器（同步状态动画）")]
    public Animator animator;
    [Tooltip("敌人骨骼根节点（用于翻转朝向）")]
    public Transform skeletonRoot;
    [Tooltip("攻击判定点（用于绘制攻击范围）")]
    public Transform attackPoint;
    [Tooltip("视野碰撞体（用于检测玩家进入/离开）")]
    public CircleCollider2D sightCollider;

    // 私有状态变量
    private EnemyState currentState;
    private float idleTimer;
    private float currentAttackCooldown;
    private float startX;
    private float leftPatrolBound;
    private float rightPatrolBound;
    private bool patrolMovingRight;
    private bool chaseMovingRight;
    private Transform targetPlayer;
    private Rigidbody2D rb;
    private Collider2D enemyCollider;

    // 受击特效相关
    private float hitFlashTimer;
    private int currentFlashCount;
    private Color[] originalColors;

    void Start()
    {
        // 获取核心组件
        rb = GetComponent<Rigidbody2D>();
        enemyCollider = GetComponent<Collider2D>();

        // 设置受击检测碰撞体
        SetupHitCollider();

        // 自动创建视野碰撞体（如果未配置）
        if (sightCollider == null)
        {
            sightCollider = gameObject.AddComponent<CircleCollider2D>();
            sightCollider.isTrigger = true;
            sightCollider.radius = 5f;
            LogWarning("自动创建视野碰撞体，默认半径5f，请手动调整");
        }
        else
        {
            sightCollider.isTrigger = true;
        }

        // 自动获取动画组件（如果未配置）
        if (animator == null)
            animator = GetComponent<Animator>();

        // 骨骼根节点默认赋值为自身（如果未配置）
        if (skeletonRoot == null)
            skeletonRoot = transform;

        // 检测地面图层配置
        if (groundLayer.value == 0)
        {
            LogWarning("未配置Ground图层！地面检测功能将失效，请在Inspector中指定Ground图层");
        }

        // 初始化受击特效
        if (spriteRenderers == null || spriteRenderers.Length == 0)
        {
            // 自动获取所有 SpriteRenderer
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
            if (spriteRenderers.Length == 0)
            {
                LogWarning("未找到 SpriteRenderer 组件，受击特效将失效");
            }
        }

        // 保存原始颜色
        if (spriteRenderers.Length > 0)
        {
            originalColors = new Color[spriteRenderers.Length];
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                if (spriteRenderers[i] != null)
                    originalColors[i] = spriteRenderers[i].color;
            }
        }

        // 初始化巡逻边界
        startX = transform.position.x;
        leftPatrolBound = startX - patrolRange;
        rightPatrolBound = startX + patrolRange;
        // 初始化移动朝向（根据骨骼缩放判定）
        patrolMovingRight = skeletonRoot.localScale.x > 0;
        chaseMovingRight = patrolMovingRight;

        // 初始状态为闲置
        currentState = EnemyState.Idle;
        currentAttackCooldown = 0;
        UpdateAnimation();

        LogDebug($"初始化完成 - 巡逻范围：[{leftPatrolBound:F2}, {rightPatrolBound:F2}]");
    }

    /// <summary>
    /// 设置受击检测碰撞体
    /// </summary>
    private void SetupHitCollider()
    {
        // 如果没有手动指定受击碰撞体，尝试自动获取
        if (hitCollider == null)
        {
            // 尝试查找名为 "HitBox" 的子物体碰撞体
            Transform hitBoxTransform = transform.Find("HitBox");
            if (hitBoxTransform != null)
            {
                hitCollider = hitBoxTransform.GetComponent<Collider2D>();
                if (hitCollider != null)
                {
                    LogDebug("自动找到 HitBox 碰撞体作为受击检测");
                }
            }

            // 如果还是没找到，使用默认的碰撞体
            if (hitCollider == null)
            {
                hitCollider = GetComponent<Collider2D>();
                if (hitCollider != null)
                {
                    LogDebug("使用默认 Collider2D 作为受击检测");
                }
                else
                {
                    LogWarning("未找到受击检测碰撞体，将使用圆形检测代替");
                }
            }
        }

        // 确保受击碰撞体不是触发器（用于物理碰撞）
        if (hitCollider != null && hitCollider.isTrigger)
        {
            LogWarning("受击碰撞体不应是触发器，已自动关闭 IsTrigger");
            hitCollider.isTrigger = false;
        }
    }

    void Update()
    {
        // 更新攻击冷却时间
        if (currentAttackCooldown > 0)
            currentAttackCooldown -= Time.deltaTime;

        // 更新受击闪烁效果
        if (hitFlashTimer > 0)
        {
            hitFlashTimer -= Time.deltaTime;
            if (hitFlashTimer <= 0 && currentFlashCount < hitFlashCount)
            {
                // 完成一次闪烁，开始下一次
                currentFlashCount++;
                if (currentFlashCount < hitFlashCount)
                {
                    SetSpritesColor(hitColor);
                    hitFlashTimer = hitFlashDuration;
                }
                else
                {
                    // 所有闪烁完成，恢复原始颜色
                    RestoreOriginalColors();
                }
            }
            else if (hitFlashTimer <= 0 && currentFlashCount >= hitFlashCount)
            {
                RestoreOriginalColors();
            }
        }

        // 输出调试日志（按间隔）
        if (debugMode && Time.time - lastDebugLogTime > debugLogInterval)
        {
            bool isInPatrolRange = IsInPatrolRange();
            LogDebug($"状态: {currentState}, 目标: {GetPlayerName()}, 位置: {transform.position.x:F2}, 在巡逻范围: {isInPatrolRange}, 巡逻朝向: {(patrolMovingRight ? "右" : "左")}");
            lastDebugLogTime = Time.time;
        }

        // 优先检测是否需要追逐玩家（全局优先级）
        PriorityChaseCheck();

        // 执行当前状态逻辑
        switch (currentState)
        {
            case EnemyState.Idle:
                HandleIdleState();
                break;
            case EnemyState.Walk:
                HandleWalkState();
                break;
            case EnemyState.Chase:
                HandleChaseState();
                break;
            case EnemyState.Attack:
                HandleAttackState();
                break;
        }
    }

    #region 核心逻辑 - 追逐检测
    /// <summary>
    /// 高优先级追逐检测（玩家进入视野时强制切换到追逐状态）
    /// </summary>
    private void PriorityChaseCheck()
    {
        if (targetPlayer != null && currentState != EnemyState.Attack)
        {
            if (currentState != EnemyState.Chase)
            {
                LogDebug($"[高优先级] 发现玩家，从{currentState}切换到Chase");
                currentState = EnemyState.Chase;
                // 切换追逐时同步当前朝向为追逐朝向
                chaseMovingRight = skeletonRoot.localScale.x > 0;
                UpdateAnimation();
                rb.velocity = Vector2.zero;
            }
        }
    }
    #endregion

    #region 状态处理 - 核心状态机逻辑
    private void HandleIdleState()
    {
        rb.velocity = Vector2.zero;
        idleTimer += Time.deltaTime;

        if (idleTimer >= patrolInterval)
        {
            currentState = EnemyState.Walk;
            idleTimer = 0;
            UpdateAnimation();
            LogDebug("[Idle→Walk] 闲置时间结束，开始巡逻");
        }
    }

    private void HandleWalkState()
    {
        if (IsObstacleAhead(patrolMovingRight))
        {
            LogDebug("[巡逻状态] 前方发现Ground障碍物，自动掉头");
            patrolMovingRight = !patrolMovingRight;
            FlipSkeleton(patrolMovingRight);
        }

        float targetX = patrolMovingRight ? rightPatrolBound : leftPatrolBound;
        Vector2 direction = new Vector2(targetX - transform.position.x, 0).normalized;
        rb.velocity = new Vector2(direction.x * moveSpeed, rb.velocity.y);

        if (Mathf.Abs(transform.position.x - targetX) < 0.15f)
        {
            LogDebug($"[巡逻状态] 到达巡逻边界 {targetX:F2}，切换到闲置状态");
            currentState = EnemyState.Idle;
            patrolMovingRight = !patrolMovingRight;
            FlipSkeleton(patrolMovingRight);
            UpdateAnimation();
            rb.velocity = Vector2.zero;
        }
    }

    private void HandleChaseState()
    {
        if (targetPlayer == null)
        {
            bool isInRange = IsInPatrolRange();
            if (isInRange)
            {
                LogDebug("玩家丢失（在巡逻范围内），恢复当前方向巡逻");
                patrolMovingRight = skeletonRoot.localScale.x > 0;
                currentState = EnemyState.Walk;
                UpdateAnimation();
                rb.velocity = Vector2.zero;
            }
            else
            {
                LogDebug("玩家丢失（超出巡逻范围），Idle后返回巡逻范围");
                CorrectReturnDirection();
                currentState = EnemyState.Idle;
                FlipSkeleton(patrolMovingRight);
                UpdateAnimation();
                rb.velocity = Vector2.zero;
                idleTimer = 0;
            }
            return;
        }

        if (IsObstacleAhead(chaseMovingRight))
        {
            LogDebug("[追逐状态] 前方发现Ground障碍物，自动掉头");
            chaseMovingRight = !chaseMovingRight;
            FlipSkeleton(chaseMovingRight);
        }

        float distanceToPlayer = Vector2.Distance(transform.position, targetPlayer.position);
        if (distanceToPlayer <= attackRange && currentAttackCooldown <= 0)
        {
            currentState = EnemyState.Attack;
            UpdateAnimation();
            return;
        }

        ChaseMoveTowardsTarget(targetPlayer.position, chaseSpeed);
        UpdateChaseFacingDirection(targetPlayer.position.x);
    }

    private void HandleAttackState()
    {
        rb.velocity = Vector2.zero;

        if (targetPlayer == null || Vector2.Distance(transform.position, targetPlayer.position) > attackRange)
        {
            currentState = EnemyState.Chase;
            UpdateAnimation();
            return;
        }
    }
    #endregion

    #region 辅助方法 - 检测/判定
    private bool IsInPatrolRange()
    {
        return transform.position.x >= leftPatrolBound - 0.1f && transform.position.x <= rightPatrolBound + 0.1f;
    }

    private void CorrectReturnDirection()
    {
        float currentX = transform.position.x;
        if (currentX < leftPatrolBound)
        {
            patrolMovingRight = true;
            LogDebug($"[校正方向] 超出左边界，朝向改为右（返回巡逻范围）");
        }
        else if (currentX > rightPatrolBound)
        {
            patrolMovingRight = false;
            LogDebug($"[校正方向] 超出右边界，朝向改为左（返回巡逻范围）");
        }
    }

    private void FlipSkeleton(bool targetRight)
    {
        Vector3 newScale = skeletonRoot.localScale;
        newScale.x = targetRight ? Mathf.Abs(newScale.x) : -Mathf.Abs(newScale.x);
        skeletonRoot.localScale = newScale;
    }

    private bool IsObstacleAhead(bool isFacingRight)
    {
        Vector2 rayOrigin = (Vector2)transform.position +
                           (isFacingRight ? groundCheckOffset : -groundCheckOffset);
        Vector2 rayDirection = isFacingRight ? Vector2.right : Vector2.left;

        RaycastHit2D hit = Physics2D.Raycast(
            rayOrigin,
            rayDirection,
            groundCheckDistance,
            groundLayer
        );

        if (hit.collider != null && debugMode)
        {
            LogDebug($"检测到Ground障碍物: {hit.collider.gameObject.name}，距离: {hit.distance:F2}");
        }

        return hit.collider != null;
    }

    /// <summary>
    /// 检查是否被击中（供玩家攻击调用）
    /// 这个方法会通过碰撞检测来判断是否受到攻击
    /// </summary>
    public bool CheckHit(Vector2 attackPointPosition, float attackRange)
    {
        // 方法1：使用专门的受击碰撞体检测
        if (hitCollider != null)
        {
            // 检测攻击点是否在受击碰撞体内
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(attackPointPosition, attackRange);
            foreach (var hit in hitColliders)
            {
                if (hit == hitCollider)
                {
                    return true;
                }
            }
        }

        // 方法2：使用圆形检测（备用方案）
        Vector2 hitPoint = (Vector2)transform.position + hitPointOffset;
        float distance = Vector2.Distance(attackPointPosition, hitPoint);
        return distance <= hitCheckRadius;
    }

    /// <summary>
    /// 受到伤害时的回调方法（供玩家攻击调用）
    /// </summary>
    public void TakeDamage(int damage = 1)
    {
        LogDebug($"受到伤害！伤害值：{damage}");

        // 触发受击闪烁特效
        StartHitFlash();

        // 应用击退效果
        ApplyKnockback();

        // 可选：后续可扩展血量系统
        // health -= damage;
        // if (health <= 0) Die();
    }

    /// <summary>
    /// 开始受击闪烁特效
    /// </summary>
    private void StartHitFlash()
    {
        if (spriteRenderers == null || spriteRenderers.Length == 0) return;

        hitFlashTimer = hitFlashDuration;
        currentFlashCount = 0;
        SetSpritesColor(hitColor);
    }

    /// <summary>
    /// 设置所有精灵渲染器的颜色
    /// </summary>
    private void SetSpritesColor(Color color)
    {
        foreach (var sr in spriteRenderers)
        {
            if (sr != null)
                sr.color = color;
        }
    }

    /// <summary>
    /// 恢复所有精灵渲染器的原始颜色
    /// </summary>
    private void RestoreOriginalColors()
    {
        if (originalColors == null || spriteRenderers == null) return;

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null && i < originalColors.Length)
                spriteRenderers[i].color = originalColors[i];
        }
    }

    /// <summary>
    /// 应用击退效果
    /// </summary>
    private void ApplyKnockback()
    {
        if (targetPlayer != null)
        {
            Vector2 knockbackDir = (transform.position - targetPlayer.position).normalized;
            knockbackDir.y = 0.5f;
            rb.velocity = new Vector2(knockbackDir.x * hitKnockbackForce, knockbackDir.y * 3f);
        }
        else
        {
            float direction = patrolMovingRight ? -1f : 1f;
            rb.velocity = new Vector2(direction * hitKnockbackForce, 3f);
        }
    }
    #endregion

    #region 辅助方法 - 移动/动画
    private void ChaseMoveTowardsTarget(Vector2 targetPos, float speed)
    {
        float directionX = targetPos.x - transform.position.x;
        if ((directionX > 0 && !chaseMovingRight) || (directionX < 0 && chaseMovingRight))
        {
            directionX = chaseMovingRight ? 1 : -1;
        }

        Vector2 direction = new Vector2(directionX, 0).normalized;
        rb.velocity = new Vector2(direction.x * speed, rb.velocity.y);
    }

    private void UpdateChaseFacingDirection(float targetX)
    {
        bool shouldFaceRight = targetX > transform.position.x;
        if (shouldFaceRight != chaseMovingRight)
        {
            chaseMovingRight = shouldFaceRight;
            FlipSkeleton(chaseMovingRight);
            LogDebug($"[追逐] 朝向更新为: {(chaseMovingRight ? "右" : "左")}");
        }
    }

    private void UpdateAnimation()
    {
        if (animator == null) return;

        // 安全设置动画参数
        SafeSetBool("IsWalking", currentState == EnemyState.Walk);
        SafeSetBool("IsChasing", currentState == EnemyState.Chase);
        SafeSetBool("IsAttacking", currentState == EnemyState.Attack);

        if (currentState == EnemyState.Attack)
        {
            if (HasAnimatorParameter("Attack"))
                animator.SetTrigger("Attack");
            currentAttackCooldown = attackCooldown;

            // 攻击时对玩家造成伤害
            if (targetPlayer != null && Vector2.Distance(transform.position, targetPlayer.position) <= attackRange)
            {
                PlayerController player = targetPlayer.GetComponent<PlayerController>();
                if (player != null)
                {
                    player.TakeDamage(attackDamage);
                    LogDebug($"对玩家造成 {attackDamage} 点伤害");
                }
            }
        }
    }

    /// <summary>
    /// 安全设置布尔动画参数
    /// </summary>
    private void SafeSetBool(string paramName, bool value)
    {
        if (animator != null && HasAnimatorParameter(paramName))
        {
            animator.SetBool(paramName, value);
        }
    }

    /// <summary>
    /// 检查动画控制器是否存在指定参数
    /// </summary>
    private bool HasAnimatorParameter(string paramName)
    {
        if (animator == null) return false;

        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName)
                return true;
        }
        return false;
    }

    public void OnAttackFinish()
    {
        if (currentState == EnemyState.Attack)
        {
            currentState = EnemyState.Chase;
            UpdateAnimation();
        }
    }

    private string GetPlayerName()
    {
        return targetPlayer != null ? targetPlayer.name : "无";
    }
    #endregion

    #region 触发器/调试/可视化
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            targetPlayer = other.transform;
            LogDebug($"检测到 {targetPlayer.name} 进入视野");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & playerLayer) != 0 && other.transform == targetPlayer)
        {
            LogDebug($"检测到 {targetPlayer.name} 离开视野");
            targetPlayer = null;
        }
    }

    private void LogDebug(string message)
    {
        if (debugMode)
            Debug.Log($"[{gameObject.name}] {message}", this);
    }

    private void LogWarning(string message)
    {
        Debug.LogWarning($"[{gameObject.name}] {message}", this);
    }

    private void OnDrawGizmosSelected()
    {
        // 巡逻范围
        Gizmos.color = Color.red;
        Gizmos.DrawLine(
            new Vector3(leftPatrolBound, transform.position.y - 0.5f, 0),
            new Vector3(leftPatrolBound, transform.position.y + 0.5f, 0)
        );
        Gizmos.DrawLine(
            new Vector3(rightPatrolBound, transform.position.y - 0.5f, 0),
            new Vector3(rightPatrolBound, transform.position.y + 0.5f, 0)
        );
        Gizmos.DrawLine(
            new Vector3(leftPatrolBound, transform.position.y, 0),
            new Vector3(rightPatrolBound, transform.position.y, 0)
        );
        Gizmos.DrawWireSphere(new Vector3(startX, transform.position.y, 0), 0.3f);

        // 攻击范围
        Gizmos.color = Color.yellow;
        Vector3 attackPos = attackPoint != null ? attackPoint.position : transform.position;
        Gizmos.DrawWireSphere(attackPos, attackRange);

        // 视野范围
        Gizmos.color = Color.blue;
        if (sightCollider != null)
            Gizmos.DrawWireSphere(transform.position, sightCollider.radius);

        // 受击检测范围
        Gizmos.color = Color.magenta;
        Vector3 hitCheckPos = (Vector3)transform.position + (Vector3)hitPointOffset;
        Gizmos.DrawWireSphere(hitCheckPos, hitCheckRadius);

        // 如果有专门的受击碰撞体，绘制其边界
        if (hitCollider != null)
        {
            Gizmos.color = Color.cyan;
            if (hitCollider is BoxCollider2D box)
            {
                Gizmos.DrawWireCube(box.bounds.center, box.bounds.size);
            }
            else if (hitCollider is CircleCollider2D circle)
            {
                Gizmos.DrawWireSphere(circle.bounds.center, circle.radius);
            }
        }

        // 地面检测射线
        if (drawGroundCheckGizmos && Application.isPlaying)
        {
            bool currentFacing = currentState == EnemyState.Chase ? chaseMovingRight : patrolMovingRight;
            Vector2 rayOrigin = (Vector2)transform.position +
                               (currentFacing ? groundCheckOffset : -groundCheckOffset);
            Vector2 rayDirection = currentFacing ? Vector2.right : Vector2.left;
            Vector2 rayEnd = rayOrigin + rayDirection * groundCheckDistance;

            Gizmos.color = IsObstacleAhead(currentFacing) ? Color.red : Color.green;
            Gizmos.DrawLine(rayOrigin, rayEnd);
            Gizmos.DrawWireSphere(rayOrigin, 0.05f);
        }
    }
    #endregion
}
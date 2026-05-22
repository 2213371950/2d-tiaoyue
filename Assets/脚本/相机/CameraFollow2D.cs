using UnityEngine;

/// <summary>
/// 2D摄像机跟随玩家脚本（支持平滑跟随、边界限制、偏移调整）
/// </summary>
public class CameraFollow2D : MonoBehaviour
{
    [Header("核心设置")]
    [Tooltip("需要跟随的玩家目标")]
    public Transform target; // 玩家Transform组件

    [Tooltip("跟随偏移量（调整摄像机与玩家的相对位置）")]
    public Vector3 followOffset = new Vector3(0, 2, -10); // 2D默认Z轴-10（确保能看到2D对象）

    [Tooltip("跟随平滑系数（值越小越平滑，建议0.1-0.3）")]
    [Range(0.01f, 1f)]
    public float smoothSpeed = 0.125f;

    [Header("边界限制（可选）")]
    [Tooltip("是否启用边界限制（防止摄像机超出地图范围）")]
    public bool useBoundary = true;

    [Tooltip("地图左边界X坐标")]
    public float leftBoundary = -10f;

    [Tooltip("地图右边界X坐标")]
    public float rightBoundary = 10f;

    [Tooltip("地图下边界Y坐标")]
    public float bottomBoundary = -5f;

    [Tooltip("地图上边界Y坐标")]
    public float topBoundary = 5f;

    [Header("额外设置")]
    [Tooltip("是否锁定摄像机旋转（2D游戏建议开启）")]
    public bool lockRotation = true;

    [Tooltip("是否在目标为空时自动查找玩家（标签为Player）")]
    public bool autoFindPlayer = true;

    private Camera mainCamera;
    private float cameraHalfWidth;  // 摄像机半宽（用于边界精确计算）
    private float cameraHalfHeight; // 摄像机半高（用于边界精确计算）

    private void Awake()
    {
        // 获取主摄像机组件
        mainCamera = GetComponent<Camera>();
        if (mainCamera == null)
        {
            Debug.LogError("摄像机组件未找到！请确保脚本挂载在Camera对象上");
            enabled = false;
            return;
        }

        // 自动查找玩家（如果目标为空且启用了自动查找）
        if (target == null && autoFindPlayer)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
                Debug.Log($"自动找到玩家：{player.name}");
            }
            else
            {
                Debug.LogWarning("未找到标签为Player的玩家对象！请手动指定目标");
            }
        }

        // 计算摄像机半宽/半高（基于正交相机尺寸）
        CalculateCameraSize();
    }

    private void Start()
    {
        // 初始位置直接定位到目标位置（避免初始偏移）
        if (target != null)
        {
            Vector3 targetPos = GetClampedTargetPosition();
            transform.position = targetPos;
        }
    }

    private void LateUpdate()
    {
        // 如果没有目标，直接返回
        if (target == null) return;

        // 锁定旋转（保持2D相机正向）
        if (lockRotation)
        {
            transform.rotation = Quaternion.identity;
        }

        // 获取目标位置（包含偏移和边界限制）
        Vector3 targetPosition = GetClampedTargetPosition();

        // 平滑移动摄像机
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, targetPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }

    /// <summary>
    /// 计算摄像机半宽/半高（适配不同分辨率和相机尺寸）
    /// </summary>
    private void CalculateCameraSize()
    {
        if (mainCamera.orthographic)
        {
            // 正交相机：半高 = 相机尺寸，半宽 = 相机尺寸 * 屏幕宽高比
            cameraHalfHeight = mainCamera.orthographicSize;
            cameraHalfWidth = cameraHalfHeight * Screen.width / Screen.height;
        }
        else
        {
            // 透视相机（可选支持）：根据相机距离计算可视范围
            float distance = Mathf.Abs(followOffset.z);
            cameraHalfHeight = Mathf.Tan(mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad) * distance;
            cameraHalfWidth = cameraHalfHeight * Screen.width / Screen.height;
        }
    }

    /// <summary>
    /// 获取带边界限制的目标位置
    /// </summary>
    private Vector3 GetClampedTargetPosition()
    {
        // 目标基础位置 = 玩家位置 + 跟随偏移
        Vector3 targetPos = target.position + followOffset;

        // 如果启用边界限制，计算限制后的位置
        if (useBoundary)
        {
            // X轴限制：左右边界
            float clampedX = Mathf.Clamp(
                targetPos.x,
                leftBoundary + cameraHalfWidth,  // 相机左边缘不超过左边界
                rightBoundary - cameraHalfWidth   // 相机右边缘不超过右边界
            );

            // Y轴限制：上下边界
            float clampedY = Mathf.Clamp(
                targetPos.y,
                bottomBoundary + cameraHalfHeight, // 相机下边缘不超过下边界
                topBoundary - cameraHalfHeight     // 相机上边缘不超过上边界
            );

            // Z轴保持不变（2D相机Z轴固定）
            targetPos = new Vector3(clampedX, clampedY, targetPos.z);
        }

        return targetPos;
    }

    /// <summary>
    /// 手动设置跟随目标（支持代码动态切换目标）
    /// </summary>
    /// <param name="newTarget">新的跟随目标</param>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        // 切换目标后立即更新位置
        if (target != null)
        {
            transform.position = GetClampedTargetPosition();
        }
    }

    /// <summary>
    /// Gizmos绘制边界范围（Scene视图可视化）
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (!useBoundary) return;

        // 绘制边界矩形（Scene视图中显示，方便调整）
        Gizmos.color = Color.cyan;
        Vector3 center = new Vector3(
            (leftBoundary + rightBoundary) / 2f,
            (bottomBoundary + topBoundary) / 2f,
            0f
        );
        Vector3 size = new Vector3(
            rightBoundary - leftBoundary,
            topBoundary - bottomBoundary,
            0f
        );
        Gizmos.DrawWireCube(center, size);
    }
}
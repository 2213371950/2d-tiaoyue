using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ParallaxBackground : MonoBehaviour
{
    [Header("视差核心配置")]
    [Tooltip("视差系数（0=跟随相机不动，<1=比相机慢（远景），>1=比相机快（近景））")]
    public float parallaxFactor = 0.5f;
    [Tooltip("是否忽略Y轴移动（横版游戏建议开启）")]
    public bool ignoreYAxis = true;
    [Tooltip("是否自动平铺背景（防止相机移动后出现空白）")]
    public bool autoTile = true;

    [Header("调试参数（无需手动修改）")]
    private Camera mainCamera;          // 主相机引用
    private Vector3 lastCameraPos;      // 上一帧相机位置
    private SpriteRenderer bgRenderer;  // 背景渲染器
    private float bgWidth;              // 背景图片宽度（世界空间）

    // 初始化
    private void Start()
    {
        // 获取核心组件
        mainCamera = Camera.main;
        bgRenderer = GetComponent<SpriteRenderer>();
        if (mainCamera == null || bgRenderer == null) return;

        // 计算背景宽度（世界空间）
        if (autoTile && bgRenderer.sprite != null)
        {
            bgWidth = bgRenderer.sprite.bounds.size.x * transform.localScale.x;
        }

        // 记录初始相机位置
        lastCameraPos = mainCamera.transform.position;
    }

    // 每帧更新（LateUpdate确保相机先移动，背景后更新）
    private void LateUpdate()
    {
        if (mainCamera == null) return;

        // 计算相机移动增量
        Vector3 cameraDelta = mainCamera.transform.position - lastCameraPos;

        // 忽略Y轴移动（横版游戏）
        if (ignoreYAxis)
        {
            cameraDelta.y = 0;
        }

        // 计算背景移动量（视差核心公式）
        Vector3 bgMoveDelta = cameraDelta * parallaxFactor;

        // 移动背景（仅偏移X轴，保持Y轴固定）
        transform.position += new Vector3(bgMoveDelta.x, 0, bgMoveDelta.z);

        // 自动平铺背景（循环位移，避免空白）
        if (autoTile)
        {
            AutoTileBackground();
        }

        // 更新上一帧相机位置
        lastCameraPos = mainCamera.transform.position;
    }

    // 自动平铺背景逻辑
    private void AutoTileBackground()
    {
        // 计算相机与背景的X轴偏移
        float cameraX = mainCamera.transform.position.x;
        float bgX = transform.position.x;
        float offsetX = cameraX - bgX;

        // 当偏移超过背景宽度的一半时，重置位置（循环平铺）
        if (Mathf.Abs(offsetX) > bgWidth / 2)
        {
            transform.position = new Vector3(
                cameraX - (offsetX % bgWidth),
                transform.position.y,
                transform.position.z
            );
        }
    }

    // 编辑器预览（可选，方便调试）
    private void OnDrawGizmos()
    {
        if (autoTile && bgRenderer != null && bgRenderer.sprite != null)
        {
            Gizmos.color = Color.red;
            float width = bgRenderer.sprite.bounds.size.x * transform.localScale.x;
            Gizmos.DrawLine(transform.position - Vector3.right * width / 2, transform.position + Vector3.right * width / 2);
        }
    }
}
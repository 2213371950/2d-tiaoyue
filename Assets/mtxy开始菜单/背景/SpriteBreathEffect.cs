using UnityEngine;

/// <summary>
/// 2D Sprite 呼吸灯效果：周期性缩放大小 + 透明度变化
/// </summary>
[RequireComponent(typeof(SpriteRenderer))] // 强制要求挂载 SpriteRenderer 组件
public class SpriteBreathEffect : MonoBehaviour
{
    [Header("呼吸效果参数")]
    [Tooltip("缩放变化幅度（基础大小 ± 该值）")]
    [Range(0.1f, 2f)] public float scaleRange = 0.3f;

    [Tooltip("透明度变化范围（0-1）")]
    [Range(0.1f, 0.9f)] public float alphaMin = 0.3f; // 最小透明度
    [Range(0.5f, 1f)] public float alphaMax = 1f;   // 最大透明度

    [Tooltip("呼吸周期（秒/次）")]
    [Range(0.5f, 5f)] public float breathDuration = 2f;

    [Tooltip("是否启用缩放效果")] public bool enableScale = true;
    [Tooltip("是否启用透明度效果")] public bool enableAlpha = true;

    private SpriteRenderer spriteRenderer; // Sprite 渲染器组件
    private Vector3 originalScale;         // 初始缩放大小
    private Color originalColor;           // 初始颜色（记录原始透明度）

    void Start()
    {
        // 获取 SpriteRenderer 组件（因添加了 RequireComponent，无需判空）
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 记录初始状态（避免后续重置时丢失原始设置）
        originalScale = transform.localScale;
        originalColor = spriteRenderer.color;
    }

    void Update()
    {
        // 计算呼吸进度（0-1-0 循环，使用正弦函数实现平滑过渡）
        float breathProgress = Mathf.Sin(Time.time * (2 * Mathf.PI / breathDuration)) * 0.5f + 0.5f;
        // Sin 函数范围是 [-1,1]，乘以 0.5 加 0.5 后转为 [0,1] 范围

        // 1. 控制缩放
        if (enableScale)
        {
            float currentScale = Mathf.Lerp(
                originalScale.x - scaleRange,  // 最小缩放
                originalScale.x + scaleRange,  // 最大缩放
                breathProgress                 // 插值进度
            );
            transform.localScale = new Vector3(currentScale, currentScale, originalScale.z);
        }

        // 2. 控制透明度
        if (enableAlpha)
        {
            float currentAlpha = Mathf.Lerp(alphaMin, alphaMax, breathProgress);
            Color newColor = originalColor;
            newColor.a = currentAlpha; // 只修改 alpha 通道，保持原始 RGB 颜色
            spriteRenderer.color = newColor;
        }
    }

    /// <summary>
    /// 重置为初始状态（可选调用）
    /// </summary>
    public void ResetToOriginalState()
    {
        transform.localScale = originalScale;
        spriteRenderer.color = originalColor;
    }
}
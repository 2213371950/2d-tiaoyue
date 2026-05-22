using UnityEngine;

/// <summary>
/// 2D退出游戏触发器 - 包含点击反馈动画和音效
/// 需要给物体添加Collider2D并勾选Is Trigger
/// </summary>
public class ExitGameWithFeedback : MonoBehaviour
{
    [Header("点击设置")]
    [Tooltip("是否需要长按（false=点击一次）")]
    [SerializeField] private bool needHoldClick = false;

    [Tooltip("长按所需时间（秒）")]
    [SerializeField] private float holdTime = 0.5f;

    [Header("反馈设置")]
    [Tooltip("点击时播放的音效")]
    [SerializeField] private AudioClip clickSound;

    [Tooltip("音效音量")]
    [SerializeField][Range(0f, 1f)] private float soundVolume = 0.7f;

    [Tooltip("动画持续时间（秒）")]
    [SerializeField] private float bounceDuration = 0.2f;

    [Tooltip("缩小时的最小比例")]
    [SerializeField] private float minScale = 0.85f;

    [Tooltip("弹起时的最大比例")]
    [SerializeField] private float maxScale = 1.1f;

    private float _holdTimer;
    private bool _isMouseOver;
    private bool _isAnimating; // 防止重复触发动画
    private Vector3 _originalScale; // 保存原始缩放大小
    private AudioSource _audioSource; // 音效播放器

    private void Awake()
    {
        // 保存初始缩放
        _originalScale = transform.localScale;

        // 自动添加AudioSource组件（如果没有）
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }
        _audioSource.playOnAwake = false; // 禁止唤醒时自动播放
        _audioSource.volume = soundVolume;
    }

    private void OnMouseEnter()
    {
        _isMouseOver = true;
        _holdTimer = 0;
    }

    private void OnMouseExit()
    {
        _isMouseOver = false;
        _holdTimer = 0;
    }

    private void Update()
    {
        if (needHoldClick)
        {
            HandleHoldClick();
        }
    }

    private void OnMouseDown()
    {
        if (!needHoldClick && !_isAnimating)
        {
            PlayClickFeedback(); // 播放点击反馈（音效+动画）
            Invoke(nameof(ExitGame), bounceDuration * 0.8f); // 动画播放一段时间后退出
        }
    }

    /// <summary>
    /// 处理长按逻辑
    /// </summary>
    private void HandleHoldClick()
    {
        if (_isMouseOver && Input.GetMouseButton(0))
        {
            _holdTimer += Time.deltaTime;

            // 长按时间达到阈值且不在动画中
            if (_holdTimer >= holdTime && !_isAnimating)
            {
                PlayClickFeedback();
                Invoke(nameof(ExitGame), bounceDuration * 0.8f);
                _holdTimer = 0;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            _holdTimer = 0;
        }
    }

    /// <summary>
    /// 播放点击反馈（音效+缩放动画）
    /// </summary>
    private void PlayClickFeedback()
    {
        _isAnimating = true;

        // 播放音效（如果有设置）
        if (clickSound != null && _audioSource != null)
        {
            _audioSource.PlayOneShot(clickSound, soundVolume);
        }

        // 启动缩放动画：缩小 -> 放大 -> 恢复原大小
        StartCoroutine(BounceAnimation());
    }

    /// <summary>
    /// 执行缩放弹跳动画
    /// </summary>
    private System.Collections.IEnumerator BounceAnimation()
    {
        // 1. 缩小到最小尺寸
        float time = 0;
        while (time < bounceDuration / 3)
        {
            transform.localScale = Vector3.Lerp(_originalScale, _originalScale * minScale, time / (bounceDuration / 3));
            time += Time.deltaTime;
            yield return null;
        }

        // 2. 放大到最大尺寸
        time = 0;
        while (time < bounceDuration / 3)
        {
            transform.localScale = Vector3.Lerp(_originalScale * minScale, _originalScale * maxScale, time / (bounceDuration / 3));
            time += Time.deltaTime;
            yield return null;
        }

        // 3. 恢复原始尺寸
        time = 0;
        while (time < bounceDuration / 3)
        {
            transform.localScale = Vector3.Lerp(_originalScale * maxScale, _originalScale, time / (bounceDuration / 3));
            time += Time.deltaTime;
            yield return null;
        }

        // 确保精确恢复原始缩放，防止误差累积
        transform.localScale = _originalScale;
        _isAnimating = false;
    }

    /// <summary>
    /// 执行退出游戏操作
    /// </summary>
    private void ExitGame()
    {
        Debug.Log("退出游戏");

        // 编辑器中退出播放模式，实际游戏中退出应用
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // 在Scene视图中绘制触发器范围（选中时）
    private void OnDrawGizmosSelected()
    {
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, collider.bounds.size);
        }
    }
}
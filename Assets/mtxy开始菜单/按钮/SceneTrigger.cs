using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 2D场景跳转脚本（含点击音效+弹跳缩放动画）
/// 要求：物体需挂载Collider2D并勾选Is Trigger
/// </summary>
public class SceneLoadWithFeedback : MonoBehaviour
{
    [Header("场景跳转设置")]
    [Tooltip("目标场景索引（Build Settings中的顺序，从0开始）")]
    [SerializeField] private int targetSceneIndex = 1;

    [Tooltip("是否需要按住鼠标（false=点击一次）")]
    [SerializeField] private bool needHoldClick = false;

    [Tooltip("按住触发时间（秒）")]
    [SerializeField] private float holdTime = 0.5f;

    [Header("点击反馈设置")]
    [Tooltip("点击时播放的音效")]
    [SerializeField] private AudioClip clickSound;

    [Tooltip("音效音量")]
    [SerializeField][Range(0f, 1f)] private float soundVolume = 0.7f;

    [Tooltip("弹跳动画时长（秒）")]
    [SerializeField] private float bounceDuration = 0.2f;

    [Tooltip("最小缩放比例（点击时的缩小程度）")]
    [SerializeField] private float minScale = 0.85f;

    [Tooltip("最大缩放比例（回弹后的放大程度）")]
    [SerializeField] private float maxScale = 1.1f;

    private float _holdTimer;
    private bool _isMouseOver;
    private bool _isAnimating; // 防止动画重复触发
    private Vector3 _originalScale; // 物体原始缩放大小
    private AudioSource _audioSource; // 音效播放组件

    private void Awake()
    {
        // 保存物体原始缩放
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
            Invoke(nameof(LoadTargetScene), bounceDuration * 0.8f); // 动画播放到一半时跳转
        }
    }

    /// <summary>
    /// 处理按住点击逻辑
    /// </summary>
    private void HandleHoldClick()
    {
        if (_isMouseOver && Input.GetMouseButton(0))
        {
            _holdTimer += Time.deltaTime;

            // 按住时间达到阈值且未在动画中
            if (_holdTimer >= holdTime && !_isAnimating)
            {
                PlayClickFeedback();
                Invoke(nameof(LoadTargetScene), bounceDuration * 0.8f);
                _holdTimer = 0;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            _holdTimer = 0;
        }
    }

    /// <summary>
    /// 播放点击反馈（音效+弹跳动画）
    /// </summary>
    private void PlayClickFeedback()
    {
        _isAnimating = true;

        // 播放音效（如果有赋值）
        if (clickSound != null && _audioSource != null)
        {
            _audioSource.PlayOneShot(clickSound, soundVolume);
        }

        // 弹跳动画：缩小 -> 放大 -> 恢复原始大小
        StartCoroutine(BounceAnimation());
    }

    /// <summary>
    /// 弹跳缩放动画协程
    /// </summary>
    private System.Collections.IEnumerator BounceAnimation()
    {
        // 1. 缩小到最小比例
        float time = 0;
        while (time < bounceDuration / 3)
        {
            transform.localScale = Vector3.Lerp(_originalScale, _originalScale * minScale, time / (bounceDuration / 3));
            time += Time.deltaTime;
            yield return null;
        }

        // 2. 放大到最大比例
        time = 0;
        while (time < bounceDuration / 3)
        {
            transform.localScale = Vector3.Lerp(_originalScale * minScale, _originalScale * maxScale, time / (bounceDuration / 3));
            time += Time.deltaTime;
            yield return null;
        }

        // 3. 恢复原始大小
        time = 0;
        while (time < bounceDuration / 3)
        {
            transform.localScale = Vector3.Lerp(_originalScale * maxScale, _originalScale, time / (bounceDuration / 3));
            time += Time.deltaTime;
            yield return null;
        }

        // 确保最终恢复原始缩放（防止动画误差）
        transform.localScale = _originalScale;
        _isAnimating = false;
    }

    /// <summary>
    /// 执行场景跳转
    /// </summary>
    private void LoadTargetScene()
    {
        if (targetSceneIndex < 0 || targetSceneIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogError($"场景索引 {targetSceneIndex} 无效！请在Build Settings中添加场景");
            return;
        }

        SceneManager.LoadSceneAsync(targetSceneIndex);
        Debug.Log($"跳转至场景：{SceneManager.GetSceneByBuildIndex(targetSceneIndex).name}（索引：{targetSceneIndex}）");
    }

    // 可视化触发器范围（可选）
    private void OnDrawGizmosSelected()
    {
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, collider.bounds.size);
        }
    }
}
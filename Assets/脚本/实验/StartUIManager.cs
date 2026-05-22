using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 开始界面UI管理器
/// 处理开始游戏、设置、退出按钮的逻辑
/// </summary>
public class StartUIManager : MonoBehaviour
{
    [Header("场景配置")]
    // 手动配置游戏场景名称（便于调试）
    [Tooltip("游戏场景的名称（需与Build Settings中的场景名一致）")]
    public string gameSceneName = "GameScene";

    // 设置场景的索引（对应Build Settings中的场景序号）
    [Tooltip("设置场景的索引（Build Settings中的Scene列表序号，从0开始）")]
    public int settingSceneIndex = 1;

    private void Start()
    {
        // 移除原有的设置面板初始化逻辑，因为不再使用面板
    }

    /// <summary>
    /// 开始游戏按钮点击事件
    /// </summary>
    public void OnStartGameButtonClick()
    {
        // 校验场景名是否为空
        if (string.IsNullOrEmpty(gameSceneName))
        {
            Debug.LogError("游戏场景名称未配置！请在Inspector中设置gameSceneName字段");
            return;
        }

        // 校验场景是否在Build Settings中
        if (IsSceneInBuildSettings(gameSceneName))
        {
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            Debug.LogError($"场景{gameSceneName}未添加到Build Settings！");
        }
    }

    /// <summary>
    /// 设置按钮点击事件（修改为加载设置场景）
    /// </summary>
    public void OnSettingButtonClick()
    {
        // 校验场景索引是否有效
        if (IsSceneIndexValid(settingSceneIndex))
        {
            // 通过索引加载设置场景
            SceneManager.LoadScene(settingSceneIndex);
        }
        else
        {
            Debug.LogError($"设置场景索引{settingSceneIndex}无效！请检查Build Settings中的场景列表");
        }
    }

    /// <summary>
    /// 退出游戏按钮点击事件
    /// </summary>
    public void OnQuitButtonClick()
    {
        // 编辑器中输出日志，打包后退出应用
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>
    /// 校验场景是否在Build Settings中
    /// </summary>
    /// <param name="sceneName">场景名称</param>
    /// <returns>是否存在</returns>
    private bool IsSceneInBuildSettings(string sceneName)
    {
        int sceneCount = SceneManager.sceneCountInBuildSettings;
        // 用for循环遍历每个场景的BuildIndex（从0到sceneCount-1）
        for (int i = 0; i < sceneCount; i++)
        {
            // 通过BuildIndex获取场景路径
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            // 从路径中提取场景名称（去掉扩展名）
            string sceneNameInBuild = System.IO.Path.GetFileNameWithoutExtension(path);
            // 对比场景名
            if (sceneNameInBuild == sceneName)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 校验场景索引是否有效
    /// </summary>
    /// <param name="index">场景索引</param>
    /// <returns>是否有效</returns>
    private bool IsSceneIndexValid(int index)
    {
        int sceneCount = SceneManager.sceneCountInBuildSettings;
        // 索引需大于等于0且小于场景总数
        return index >= 0 && index < sceneCount;
    }
}
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 游戏界面UI管理器
/// 处理游戏内暂停菜单的显示、隐藏及相关逻辑
/// </summary>
public class GameUIManager : MonoBehaviour
{
    [Header("UI组件")]
    // 暂停菜单面板（默认隐藏）
    public GameObject pauseMenuPanel;
    // 游戏设置按钮
    public Button settingButton;

    [Header("场景配置")]
    // 手动配置主菜单场景名称（便于调试）
    [Tooltip("主菜单场景的名称（需与Build Settings中的场景名一致）")]
    public string startSceneName = "StartScene";

    private void Start()
    {
        // 初始化时隐藏暂停菜单
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        // 确保时间缩放为1（游戏正常运行）
        Time.timeScale = 1;
    }

    /// <summary>
    /// 游戏内设置按钮点击事件（调出暂停菜单）
    /// </summary>
    public void OnGameSettingButtonClick()
    {
        if (pauseMenuPanel != null)
        {
            // 显示暂停菜单
            pauseMenuPanel.SetActive(true);
            // 暂停游戏（Time.timeScale设为0，物体运动、协程等暂停）
            Time.timeScale = 0;
            // 可选：隐藏设置按钮，避免重复点击
            settingButton.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("未分配暂停菜单面板，请在Inspector中指定！");
        }
    }

    /// <summary>
    /// 继续游戏按钮点击事件
    /// </summary>
    public void OnContinueButtonClick()
    {
        if (pauseMenuPanel != null)
        {
            // 隐藏暂停菜单
            pauseMenuPanel.SetActive(false);
            // 恢复游戏运行
            Time.timeScale = 1;
            // 显示设置按钮
            settingButton.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 返回主菜单按钮点击事件
    /// </summary>
    public void OnBackToStartButtonClick()
    {
        // 恢复时间缩放，避免主菜单场景暂停
        Time.timeScale = 1;

        // 校验场景名是否为空
        if (string.IsNullOrEmpty(startSceneName))
        {
            Debug.LogError("主菜单场景名称未配置！请在Inspector中设置startSceneName字段");
            return;
        }

        // 校验场景是否在Build Settings中
        if (IsSceneInBuildSettings(startSceneName))
        {
            SceneManager.LoadScene(startSceneName);
        }
        else
        {
            Debug.LogError($"场景{startSceneName}未添加到Build Settings！");
        }
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
}
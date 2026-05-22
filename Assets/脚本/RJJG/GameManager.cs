using UnityEngine;
using TMPro; // 替换为TMPro命名空间（核心修改）

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("重生位置")]
    [SerializeField] private Transform player1Spawn;
    [SerializeField] private Transform player2Spawn;

    [Header("玩家引用")]
    [SerializeField] private Shuangren player1;
    [SerializeField] private Shuangren player2;

    [Header("计分板UI（TMPro）")]
    [SerializeField] private TMP_Text player1ScoreText; // 替换为TMP_Text
    [SerializeField] private TMP_Text player2ScoreText; // 替换为TMP_Text
    [SerializeField] private TMP_Text winnerText;       // 替换为TMP_Text
    [SerializeField] private int winScore = 3;          // 获胜分数（默认3分）

    // 计分变量
    private int player1Score;
    private int player2Score;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // 初始化UI
        if (winnerText != null)
            winnerText.gameObject.SetActive(false);
        UpdateScoreUI();
    }

    // 玩家死亡时调用（新增计分逻辑）
    public void OnPlayerDeath(Shuangren deadPlayer)
    {
        // 判定击杀方并加分
        if (deadPlayer.PlayerNumber == PlayerNumber.Player1)
        {
            player2Score++; // 玩家1死亡，玩家2加分
            CheckWinCondition(PlayerNumber.Player2);
        }
        else
        {
            player1Score++; // 玩家2死亡，玩家1加分
            CheckWinCondition(PlayerNumber.Player1);
        }
    }

    // 检查胜利条件
    private void CheckWinCondition(PlayerNumber killer)
    {
        UpdateScoreUI(); // 先更新分数显示

        // 未达到胜利分数：仅重置位置
        if (player1Score < winScore && player2Score < winScore)
        {
            Invoke(nameof(ResetAllPlayers), 0.5f);
        }
        // 达到胜利分数：显示胜利提示，延迟后重置所有
        else
        {
            string winnerName = killer == PlayerNumber.Player1 ? "玩家1" : "玩家2";
            if (winnerText != null)
            {
                winnerText.text = $"{winnerName}获胜！";
                winnerText.gameObject.SetActive(true);
            }

            // 延迟2秒后重置分数和位置
            Invoke(nameof(ResetGame), 2f);
        }
    }

    // 重置玩家位置（仅位置，不重置分数）
    private void ResetAllPlayers()
    {
        if (player1 != null)
            player1.Respawn(player1Spawn.position);
        if (player2 != null)
            player2.Respawn(player2Spawn.position);
    }

    // 重置整个游戏（分数+位置+UI）
    private void ResetGame()
    {
        // 重置分数
        player1Score = 0;
        player2Score = 0;

        // 重置位置
        ResetAllPlayers();

        // 重置UI
        if (winnerText != null)
            winnerText.gameObject.SetActive(false);
        UpdateScoreUI();
    }

    // 更新计分板UI（适配TMP）
    private void UpdateScoreUI()
    {
        if (player1ScoreText != null)
            player1ScoreText.text = $"玩家1：{player1Score}分";

        if (player2ScoreText != null)
            player2ScoreText.text = $"玩家2：{player2Score}分";
    }
}
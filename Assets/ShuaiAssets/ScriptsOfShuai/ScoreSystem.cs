using UnityEngine;
using TMPro;

public class ScoreSystem : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [Header("Score Settings")]
    [SerializeField] private string scorePrefix = "Score: ";

    // 本地分数变量，不再使用NetworkVariable
    private int currentScore = 0;

    // 单例模式，方便其他脚本访问
    public static ScoreSystem Instance { get; private set; }

    private void Awake()
    {
        // 设置单例
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // 确保有引用到TextMeshPro组件
        if (scoreText == null)
        {
            Debug.LogError("ScoreSystem: No TextMeshProUGUI reference set!");
            return;
        }

        // 初始化显示
        UpdateScoreDisplay(currentScore);
    }

    // 供外部调用的加分方法
    public void AddScore(int amount)
    {
        // 直接在本地修改分数
        currentScore += amount;
        // 更新显示
        UpdateScoreDisplay(currentScore);
    }

    // 供外部调用的设置分数方法
    public void SetScore(int newScore)
    {
        // 直接在本地设置分数
        currentScore = newScore;
        // 更新显示
        UpdateScoreDisplay(currentScore);
    }

    // 获取当前分数
    public int GetCurrentScore()
    {
        return currentScore;
    }

    // 更新UI显示
    private void UpdateScoreDisplay(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"{scorePrefix}{score}";
        }
    }
}
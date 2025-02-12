using UnityEngine;
using Unity.Netcode;
using TMPro;

public class ScoreSystem : NetworkBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Score Settings")]
    [SerializeField] private string scorePrefix = "Score: ";

    // 使用 NetworkVariable 来同步分数
    private NetworkVariable<int> currentScore = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

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

        // 订阅分数变化事件
        currentScore.OnValueChanged += OnScoreChanged;

        // 初始化显示
        UpdateScoreDisplay(currentScore.Value);
    }

    // 供外部调用的加分方法
    public void AddScore(int amount)
    {
        if (!IsServer)
        {
            // 如果不是服务器，请求服务器添加分数
            AddScoreServerRpc(amount);
            return;
        }

        // 在服务器上直接修改分数
        currentScore.Value += amount;
    }

    // 供外部调用的设置分数方法
    public void SetScore(int newScore)
    {
        if (!IsServer)
        {
            // 如果不是服务器，请求服务器设置分数
            SetScoreServerRpc(newScore);
            return;
        }

        // 在服务器上直接设置分数
        currentScore.Value = newScore;
    }

    // 获取当前分数
    public int GetCurrentScore()
    {
        return currentScore.Value;
    }

    // 当分数变化时更新显示
    private void OnScoreChanged(int previousValue, int newValue)
    {
        UpdateScoreDisplay(newValue);
    }

    // 更新UI显示
    private void UpdateScoreDisplay(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"{scorePrefix}{score}";
        }
    }

    #region ServerRPCs

    [ServerRpc(RequireOwnership = false)]
    private void AddScoreServerRpc(int amount)
    {
        currentScore.Value += amount;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetScoreServerRpc(int newScore)
    {
        currentScore.Value = newScore;
    }

    #endregion
}
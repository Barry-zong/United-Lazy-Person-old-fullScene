using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class ScoreSystem : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Score Settings")]
    [SerializeField] private string scorePrefix = "Score: ";

    [Header("Audio Settings")]
    [SerializeField] private AudioSource scoreAudioSource; // 音频源组件
    

    // 分数变量，后续会使用NetworkVariable
    private int currentScore = 0;
    private bool canAddScore = true; // 控制是否允许加分

    // 单例模式，使其他脚本可以访问
    public static ScoreSystem Instance { get; private set; }

    private void Awake()
    {
        // 设置单例
        if (Instance == null)
        {
            Instance = this;
           
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResetScore();
    }

    private void Start()
    {
        // 重置分数
        ResetScore();

        // 确保设置了TextMeshPro组件
        if (scoreText == null)
        {
            Debug.LogError("ScoreSystem: No TextMeshProUGUI reference set!");
            return;
        }

        // 确保设置了AudioSource组件
        if (scoreAudioSource == null)
        {
            Debug.LogWarning("ScoreSystem: No AudioSource reference set! Score sounds will not play.");
        }

        // 初始化显示
        UpdateScoreDisplay(currentScore);
    }

    // 供外部调用的加分方法
    public void AddScore(int amount)
    {
        if (!canAddScore) return; // 如果不允许加分，直接返回

        // 记录旧分数
        int oldScore = currentScore;

        // 直接在本机修改分数
        currentScore += amount;

        // 只有在分数增加时才播放音效
        if (currentScore > oldScore)
        {
            PlayScoreSound();
        }

        // 更新显示
        UpdateScoreDisplay(currentScore);
    }

    // 供外部调用的设置分数方法
    public void SetScore(int newScore)
    {
        if (!canAddScore) return; // 如果不允许加分，直接返回

        // 记录旧分数
        int oldScore = currentScore;

        // 直接在本机设置分数
        currentScore = newScore;

        // 只有在分数增加时才播放音效
        if (currentScore > oldScore)
        {
            PlayScoreSound();
        }

        // 更新显示
        UpdateScoreDisplay(currentScore);
    }

    // 获取当前分数
    public int GetCurrentScore()
    {
        return currentScore;
    }

    // 设置是否允许加分
    public void SetCanAddScore(bool canAdd)
    {
        canAddScore = canAdd;
    }

    // 更新UI显示
    private void UpdateScoreDisplay(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"{scorePrefix}{score}";
        }
    }

    // 播放得分音效
    private void PlayScoreSound()
    {
        if (scoreAudioSource != null && scoreAudioSource.clip != null)
        {
            scoreAudioSource.Play();
        }
    }

    // 重置分数方法
    public void ResetScore()
    {
        currentScore = 0;
        canAddScore = true;
    }
}
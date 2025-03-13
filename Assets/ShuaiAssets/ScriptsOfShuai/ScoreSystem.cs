using UnityEngine;
using TMPro;

public class ScoreSystem : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [Header("Score Settings")]
    [SerializeField] private string scorePrefix = "Score: ";

    // ���ط�������������ʹ��NetworkVariable
    private int currentScore = 0;

    // ����ģʽ�����������ű�����
    public static ScoreSystem Instance { get; private set; }

    private void Awake()
    {
        // ���õ���
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
        // ȷ�������õ�TextMeshPro���
        if (scoreText == null)
        {
            Debug.LogError("ScoreSystem: No TextMeshProUGUI reference set!");
            return;
        }

        // ��ʼ����ʾ
        UpdateScoreDisplay(currentScore);
    }

    // ���ⲿ���õļӷַ���
    public void AddScore(int amount)
    {
        // ֱ���ڱ����޸ķ���
        currentScore += amount;
        // ������ʾ
        UpdateScoreDisplay(currentScore);
    }

    // ���ⲿ���õ����÷�������
    public void SetScore(int newScore)
    {
        // ֱ���ڱ������÷���
        currentScore = newScore;
        // ������ʾ
        UpdateScoreDisplay(currentScore);
    }

    // ��ȡ��ǰ����
    public int GetCurrentScore()
    {
        return currentScore;
    }

    // ����UI��ʾ
    private void UpdateScoreDisplay(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"{scorePrefix}{score}";
        }
    }
}
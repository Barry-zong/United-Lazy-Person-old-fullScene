using UnityEngine;
using TMPro;
public class ScoreSystem : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Score Settings")]
    [SerializeField] private string scorePrefix = "Score: ";

    [Header("Audio Settings")]
    [SerializeField] private AudioSource scoreAudioSource; // ������ƵԴ����

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

        // ȷ�������õ�AudioSource���
        if (scoreAudioSource == null)
        {
            Debug.LogWarning("ScoreSystem: No AudioSource reference set! Score sounds will not play.");
        }

        // ��ʼ����ʾ
        UpdateScoreDisplay(currentScore);
    }

    // ���ⲿ���õļӷַ���
    public void AddScore(int amount)
    {
        // ��¼�ɷ���
        int oldScore = currentScore;

        // ֱ���ڱ����޸ķ���
        currentScore += amount;

        // ֻ���ڷ�����������ʱ�Ų�����Ч
        if (currentScore > oldScore)
        {
            PlayScoreSound();
        }

        // ������ʾ
        UpdateScoreDisplay(currentScore);
    }

    // ���ⲿ���õ����÷�������
    public void SetScore(int newScore)
    {
        // ��¼�ɷ���
        int oldScore = currentScore;

        // ֱ���ڱ������÷���
        currentScore = newScore;

        // ֻ���ڷ�����������ʱ�Ų�����Ч
        if (currentScore > oldScore)
        {
            PlayScoreSound();
        }

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

    // ���ŵ÷���Ч
    private void PlayScoreSound()
    {
        if (scoreAudioSource != null && scoreAudioSource.clip != null)
        {
            scoreAudioSource.Play();
        }
    }
}
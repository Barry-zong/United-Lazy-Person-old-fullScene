using UnityEngine;
using TMPro;

public class Scoretotext : MonoBehaviour
{
    private TextMeshProUGUI scoreText;
    private ScoreSystem scoreSystem;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 获取TextMeshPro组件
        scoreText = GetComponent<TextMeshProUGUI>();
        if (scoreText == null)
        {
            Debug.LogError("Scoretotext: No TextMeshProUGUI component found!");
            return;
        }

        // 获取ScoreSystem实例
        scoreSystem = ScoreSystem.Instance;
        if (scoreSystem == null)
        {
            Debug.LogError("Scoretotext: ScoreSystem instance not found!");
            return;
        }

        // 初始化显示分数
        UpdateScoreDisplay();
    }

    // Update is called once per frame
    void Update()
    {
        // 每帧更新分数显示
        UpdateScoreDisplay();
    }

    private void UpdateScoreDisplay()
    {
        if (scoreText != null && scoreSystem != null)
        {
            scoreText.text = scoreSystem.GetCurrentScore().ToString();
        }
    }
}

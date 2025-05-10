using UnityEngine;
using TMPro;

public class TimeUpdateForWatch : MonoBehaviour
{
    private TextMeshProUGUI timeText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 获取TextMeshPro组件
        timeText = GetComponentInChildren<TextMeshProUGUI>();
        if (timeText == null)
        {
            Debug.LogError("未找到TextMeshProUGUI组件！");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (timeText != null && CountdownTimer.Instance != null)
        {
            // 获取剩余时间并格式化为分:秒
            float timeRemaining = CountdownTimer.Instance.GetRemainingTime();
            int minutes = Mathf.FloorToInt(timeRemaining / 60f);
            int seconds = Mathf.FloorToInt(timeRemaining % 60f);
            
            // 更新文本显示
            timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }
}

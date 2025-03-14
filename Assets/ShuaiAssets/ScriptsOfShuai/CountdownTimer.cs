using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CountdownTimer : MonoBehaviour
{
    [Header("计时器设置")]
    [Tooltip("倒计时总时间（秒）")]
    public float totalTime = 60f;

    [Tooltip("是否在倒计时结束时暂停游戏")]
    public bool pauseGameWhenFinished = false;

    [Header("UI引用")]
    [Tooltip("用于显示倒计时的TextMeshPro组件")]
    public TMP_Text countdownText;

    [Header("音频设置")]
    [Tooltip("倒计时结束时需要关闭的音乐源")]
    public AudioSource musicToStop;

    private float timeRemaining;
    private bool isRunning = false;

    void Start()
    {
        // 初始化倒计时
        timeRemaining = totalTime;
        UpdateTimerDisplay();
        StartTimer();
    }

    void Update()
    {
        if (isRunning)
        {
            if (timeRemaining > 0)
            {
                // 每帧减少时间
                timeRemaining -= Time.deltaTime;

                // 确保时间不会变为负数
                if (timeRemaining < 0)
                {
                    timeRemaining = 0;
                    TimerFinished();
                }

                // 更新UI显示
                UpdateTimerDisplay();
            }
        }
    }

    // 开始计时器
    public void StartTimer()
    {
        isRunning = true;
    }

    // 暂停计时器
    public void PauseTimer()
    {
        isRunning = false;
    }

    // 重置计时器
    public void ResetTimer()
    {
        timeRemaining = totalTime;
        UpdateTimerDisplay();
    }

    // 更新UI显示
    private void UpdateTimerDisplay()
    {
        if (countdownText != null)
        {
            // 将剩余时间转换为分:秒格式
            int minutes = Mathf.FloorToInt(timeRemaining / 60f);
            int seconds = Mathf.FloorToInt(timeRemaining % 60f);

            // 更新文本显示
            countdownText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    // 计时器结束时调用
    private void TimerFinished()
    {
        isRunning = false;

        // 如果启用了暂停选项，则暂停游戏
        if (pauseGameWhenFinished)
        {
            Time.timeScale = 0f;
            Debug.Log("倒计时结束，游戏已暂停");
        }

        // 关闭指定的音乐
        if (musicToStop != null)
        {
            musicToStop.Stop();
            Debug.Log("音乐已停止播放");
        }

        // 这里可以添加其他倒计时结束时需要执行的逻辑
        Debug.Log("倒计时结束");
    }
}
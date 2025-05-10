using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CountdownTimer : MonoBehaviour
{
    private static CountdownTimer _instance;
    public static CountdownTimer Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<CountdownTimer>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("CountdownTimer");
                    _instance = go.AddComponent<CountdownTimer>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    [Header("计时器设置")]
    [Tooltip("设置计时总时间（秒）")]
    public float totalTime = 60f;

    [Tooltip("是否在倒计时结束时暂停游戏")]
    public bool pauseGameWhenFinished = false;

    [Header("UI设置")]
    [Tooltip("用于显示倒计时的TextMeshPro组件")]
    public TMP_Text countdownText;

    [Header("游戏对象设置")]
    [Tooltip("倒计时结束时需要禁用的传送点")]
    public GameObject teleport;
    public GameObject teleport2;

    private float timeRemaining;
    private bool isRunning = false;
    private bool isTimerFinished = false;

    // 公共属性，用于获取计时器是否结束
    public bool IsTimerFinished => isTimerFinished;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        
        // 初始化计时器状态
        timeRemaining = totalTime;
        isRunning = false;
        isTimerFinished = false;
        UpdateTimerDisplay();
    }

    void Start()
    {
        // 初始化倒计时
        timeRemaining = totalTime;
        UpdateTimerDisplay();
        // 只有在游戏状态为Playing时才开始计时
        if (GameStateCenter.Instance != null && GameStateCenter.Instance.IsState(GameState.Playing))
        {
            StartTimer();
        }
    }

    void Update()
    {
        // 只有在游戏状态为Playing时才更新计时器
        if (isRunning && GameStateCenter.Instance != null && GameStateCenter.Instance.IsState(GameState.Playing))
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
        isTimerFinished = true;

        // 设置游戏状态为GameOver
        if (GameStateCenter.Instance != null)
        {
            GameStateCenter.Instance.SetGameState(GameState.GameOver);
        }

        // 禁用传送点
        if (teleport != null)
        {
            teleport.SetActive(false);
            Debug.Log("传送点1已禁用");
        }

        if (teleport2 != null)
        {
            teleport2.SetActive(false);
            Debug.Log("传送点2已禁用");
        }

        // 在这里添加倒计时结束时需要执行的逻辑
        Debug.Log("倒计时结束");
    }

    public void SetRemainingTime(float time)
    {
        timeRemaining = time;
        UpdateTimerDisplay();
    }

    // 获取剩余时间
    public float GetRemainingTime()
    {
        return timeRemaining;
    }
}
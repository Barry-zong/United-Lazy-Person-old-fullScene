using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Diagnostics;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class EndingEvent : MonoBehaviour
{
    public GameObject endingUI;
    public GameObject endingUI2;
    public float restartDelay = 60f; // 游戏结束后多久重启场景
    private bool hasEnded = false;
    private float endTime;
    
    // 添加触发检测相关变量
    private int triggerCount = 0;
    private float lastTriggerTime;
    [SerializeField ]
    private float doubleTriggerThreshold = 2f; // 两次触发的最大时间间隔
    [SerializeField ]
    private int requiredTriggerCount = 6; // 需要触发的次数

    // 添加手部交互相关变量
    private float handTriggerCooldown = 0.5f; // 手部交互冷却时间
    private float lastHandTriggerTime; // 上次手部交互时间
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        endingUI.SetActive(false);
        endingUI2.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (CountdownTimer.Instance.IsTimerFinished && !hasEnded)
        {
            hasEnded = true;
            endTime = Time.time;
            endingUI.SetActive(true);
            endingUI2.SetActive(true);
            // 禁用加分系统
            if (ScoreSystem.Instance != null)
            {
                ScoreSystem.Instance.SetCanAddScore(false);
            }
        }

        if (hasEnded && Time.time - endTime >= restartDelay)
        {
            RestartScene();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Finish"))
        {
            float currentTime = Time.time;
            
            if (currentTime - lastTriggerTime <= doubleTriggerThreshold)
            {
                triggerCount++;
                if (triggerCount >= requiredTriggerCount)
                {
                    // 设置游戏状态为已加载
                    if (GameStateCenter.Instance != null)
                    {
                        GameStateCenter.Instance.SetGameState(GameState.Loaded);
                    }
                    RestartScene();
                }
            }
            else
            {
                triggerCount = 1;
            }
            
            lastTriggerTime = currentTime;
        }
        else if (other.CompareTag("RightHand"))
        {
            float currentTime = Time.time;
            if (currentTime - lastHandTriggerTime >= handTriggerCooldown)
            {
                WristUI.Instance.ToggleUI();
                lastHandTriggerTime = currentTime;
            }
        }
    }

    private void RestartScene()
    {
        if (Application.isEditor)
        {
            // 在Unity编辑器中运行时，关闭游戏
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }
        else
        {
            // 在VR头显设备中运行时，强制退出游戏
            Application.Quit();
            #if UNITY_STANDALONE_WIN
            System.Diagnostics.Process.GetCurrentProcess().Kill();
            #endif
        }
    }
}

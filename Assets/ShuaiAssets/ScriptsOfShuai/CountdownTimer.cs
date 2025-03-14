using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CountdownTimer : MonoBehaviour
{
    [Header("��ʱ������")]
    [Tooltip("����ʱ��ʱ�䣨�룩")]
    public float totalTime = 60f;

    [Tooltip("�Ƿ��ڵ���ʱ����ʱ��ͣ��Ϸ")]
    public bool pauseGameWhenFinished = false;

    [Header("UI����")]
    [Tooltip("������ʾ����ʱ��TextMeshPro���")]
    public TMP_Text countdownText;

    [Header("��Ƶ����")]
    [Tooltip("����ʱ����ʱ��Ҫ�رյ�����Դ")]
    public AudioSource musicToStop;

    private float timeRemaining;
    private bool isRunning = false;

    void Start()
    {
        // ��ʼ������ʱ
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
                // ÿ֡����ʱ��
                timeRemaining -= Time.deltaTime;

                // ȷ��ʱ�䲻���Ϊ����
                if (timeRemaining < 0)
                {
                    timeRemaining = 0;
                    TimerFinished();
                }

                // ����UI��ʾ
                UpdateTimerDisplay();
            }
        }
    }

    // ��ʼ��ʱ��
    public void StartTimer()
    {
        isRunning = true;
    }

    // ��ͣ��ʱ��
    public void PauseTimer()
    {
        isRunning = false;
    }

    // ���ü�ʱ��
    public void ResetTimer()
    {
        timeRemaining = totalTime;
        UpdateTimerDisplay();
    }

    // ����UI��ʾ
    private void UpdateTimerDisplay()
    {
        if (countdownText != null)
        {
            // ��ʣ��ʱ��ת��Ϊ��:���ʽ
            int minutes = Mathf.FloorToInt(timeRemaining / 60f);
            int seconds = Mathf.FloorToInt(timeRemaining % 60f);

            // �����ı���ʾ
            countdownText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    // ��ʱ������ʱ����
    private void TimerFinished()
    {
        isRunning = false;

        // �����������ͣѡ�����ͣ��Ϸ
        if (pauseGameWhenFinished)
        {
            Time.timeScale = 0f;
            Debug.Log("����ʱ��������Ϸ����ͣ");
        }

        // �ر�ָ��������
        if (musicToStop != null)
        {
            musicToStop.Stop();
            Debug.Log("������ֹͣ����");
        }

        // ������������������ʱ����ʱ��Ҫִ�е��߼�
        Debug.Log("����ʱ����");
    }
}
using UnityEngine;
using System.Collections;

public class waterAudioControl : MonoBehaviour
{
    private AudioSource audioSource;
    private float targetVolume;
    private float fadeDuration = 3f; // 3秒渐变时间
    private Coroutine fadeCoroutine;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            targetVolume = audioSource.volume;
            audioSource.volume = 0f; // 初始音量为0
        }

        // 注册游戏状态改变事件
        GameStateCenter.Instance.OnGameStateChanged += HandleGameStateChanged;
    }

    void OnDestroy()
    {
        // 取消注册事件
        if (GameStateCenter.Instance != null)
        {
            GameStateCenter.Instance.OnGameStateChanged -= HandleGameStateChanged;
        }
    }

    void HandleGameStateChanged(GameState newState)
    {
        if (audioSource == null) return;

        switch (newState)
        {
            case GameState.Playing:
                if (GameStateCenter.Instance.CurrentState == GameState.Loaded)
                {
                    StartFadeVolume(targetVolume);
                }
                break;
            case GameState.GameOver:
                if (audioSource.volume > 0)
                {
                    StartFadeVolume(0f);
                }
                break;
        }
    }

    private void StartFadeVolume(float target)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(FadeVolume(target));
    }

    IEnumerator FadeVolume(float target)
    {
        float startVolume = audioSource.volume;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeDuration;
            audioSource.volume = Mathf.Lerp(startVolume, target, t);
            yield return null;
        }

        audioSource.volume = target;
        fadeCoroutine = null;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

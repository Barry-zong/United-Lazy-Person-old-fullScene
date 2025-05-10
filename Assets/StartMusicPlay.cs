using UnityEngine;
using System.Collections;

public class StartMusicPlay : MonoBehaviour
{
    private AudioSource audioSource;
    private float targetVolume;
    private float fadeSpeed = 1f; // 1秒内完成渐变
    private bool isFading = false;

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
            case GameState.Loaded:
                StartCoroutine(FadeVolume(targetVolume));
                break;
            case GameState.GameOver:
                StartCoroutine(FadeVolume(0f));
                break;
        }
    }

    IEnumerator FadeVolume(float target)
    {
        isFading = true;
        float startVolume = audioSource.volume;
        float elapsedTime = 0f;

        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * fadeSpeed;
            audioSource.volume = Mathf.Lerp(startVolume, target, elapsedTime);
            yield return null;
        }

        audioSource.volume = target;
        isFading = false;
    }

    // Update is called once per frame
    void Update()
    {
        // 可以在这里添加其他更新逻辑
    }
}

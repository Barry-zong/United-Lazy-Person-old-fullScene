using UnityEngine;
using UnityEngine.SceneManagement;

public class AutoCheckStateTutorio : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 注册游戏状态改变事件
        GameStateCenter.Instance.OnGameStateChanged += HandleGameStateChanged;
        
        // 检查当前游戏状态
        if (GameStateCenter.Instance.CurrentState == GameState.Playing)
        {
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
        }
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
        if (newState == GameState.Playing)
        {
            gameObject.SetActive(false);
        }
        else if (newState == GameState.Loaded)
        {
            // 当场景重新加载时，确保对象是激活的
            gameObject.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    Loading,        // 游戏加载中
    Loaded,         // 游戏加载完成
    TutorialStart,  // 教程开始
    Playing,        // 游戏进行中
    GameOver        // 游戏结束
}

public class GameStateCenter : MonoBehaviour
{
    private static GameStateCenter _instance;
    public static GameStateCenter Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameStateCenter>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("GameStateCenter");
                    _instance = go.AddComponent<GameStateCenter>();
                }
            }
            return _instance;
        }
    }

    private GameState _currentState = GameState.Loading;
    public GameState CurrentState
    {
        get { return _currentState; }
        private set
        {
            if (_currentState != value)
            {
                _currentState = value;
                OnGameStateChanged?.Invoke(_currentState);
            }
        }
    }

    // 状态改变事件
    public delegate void GameStateChangedHandler(GameState newState);
    public event GameStateChangedHandler OnGameStateChanged;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // 状态切换方法
    public void SetGameState(GameState newState)
    {
        Debug.Log($"游戏状态从 {CurrentState} 变更为 {newState}");
        CurrentState = newState;
    }

    // 状态检查方法
    public bool IsState(GameState state)
    {
        return CurrentState == state;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        // 注册场景加载完成事件
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        // 取消注册场景加载完成事件
        SceneManager.sceneLoaded -= OnSceneLoaded;
        
        // 清理单例实例
        if (_instance == this)
        {
            _instance = null;
        }
    }

    // 添加静态清理方法
    public static void Cleanup()
    {
        if (_instance != null)
        {
            Destroy(_instance.gameObject);
            _instance = null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"场景加载完成: {scene.name}, 模式: {mode}");
        // 当场景加载完成时，将状态设置为Loaded
        SetGameState(GameState.Loaded);
    }

    // 添加场景重载时的状态重置
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        // 当场景开始加载时，设置状态为Loading
        SetGameState(GameState.Loading);
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

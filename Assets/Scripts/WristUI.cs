using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class WristUI : MonoBehaviour
{
    public static WristUI Instance { get; private set; }

    [SerializeField] private GameObject uiPanel; // UI面板
    [SerializeField] private float smoothSpeed = 10f; // 平滑过渡速度
    [SerializeField] private float autoHideTime = 20f; // 自动隐藏时间（秒）

    private bool isShowing = false;
    private Vector3 targetScale = Vector3.zero;
    private float lastToggleTime = 0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 检查必要组件
        if (uiPanel == null)
            Debug.LogError("未设置UI面板！");

        // 初始化UI为隐藏状态
        if (uiPanel != null)
        {
            targetScale = Vector3.zero;
            uiPanel.transform.localScale = Vector3.zero;
        }
    }

    private void Update()
    {
        if (uiPanel == null) return;

        // 平滑过渡UI的缩放
        uiPanel.transform.localScale = Vector3.Lerp(
            uiPanel.transform.localScale,
            targetScale,
            Time.deltaTime * smoothSpeed
        );

        // 检查是否需要自动隐藏
        if (isShowing && Time.time - lastToggleTime >= autoHideTime)
        {
            HideUI();
        }
    }

    public void ToggleUI()
    {
        // 如果在20秒内再次触发，则关闭UI
        if (isShowing && Time.time - lastToggleTime < autoHideTime)
        {
            HideUI();
        }
        else
        {
            ShowUI();
        }
        lastToggleTime = Time.time;
    }

    private void ShowUI()
    {
        isShowing = true;
        targetScale = Vector3.one;
    }

    private void HideUI()
    {
        isShowing = false;
        targetScale = Vector3.zero;
    }
} 
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class WristUI : MonoBehaviour
{
    [SerializeField] private Transform palmObject; // 手掌物体
    [SerializeField] private GameObject uiPanel; // UI面板
    [SerializeField] private float showAngle = 30f; // 隐藏UI的角度阈值
    [SerializeField] private float hideAngle = 80f; // 显示UI的角度阈值（增大到80度）
    [SerializeField] private float smoothSpeed = 10f; // 平滑过渡速度

    private bool isShowing = false;
    private Vector3 targetScale = Vector3.zero;

    private void Start()
    {
        // 检查必要组件
        if (palmObject == null)
            Debug.LogError("未设置手掌物体！");
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
        if (palmObject == null || uiPanel == null) return;

        // 获取手掌物体的旋转角度
        Vector3 rotation = palmObject.eulerAngles;
        
        // 根据Z轴旋转角度决定是否显示UI
        float normalizedZ = rotation.z > 180f ? rotation.z - 360f : rotation.z;
        
        // 检查左手所有手指的弯曲度
        bool anyFingerBent = false;
        for (int i = 0; i < 5; i++)
        {
            float bend = HandFingerDetect.Instance.GetFingerBend(false, i);
            if (bend > 0)
            {
                anyFingerBent = true;
                break;
            }
        }
        
        // 修改判断逻辑：正手（手掌朝上）时开启UI，反手（手掌朝下）时关闭UI
        // 同时检查是否有手指弯曲
        if (!isShowing && (normalizedZ < -hideAngle || normalizedZ > hideAngle) && !anyFingerBent)
        {
            ShowUI();
        }
        else if (isShowing && (normalizedZ > -showAngle && normalizedZ < showAngle || anyFingerBent))
        {
            HideUI();
        }

        // 平滑过渡UI的缩放
        uiPanel.transform.localScale = Vector3.Lerp(
            uiPanel.transform.localScale,
            targetScale,
            Time.deltaTime * smoothSpeed
        );
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
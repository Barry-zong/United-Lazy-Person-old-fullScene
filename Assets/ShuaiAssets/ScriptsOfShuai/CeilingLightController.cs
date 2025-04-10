using UnityEngine;

public class CeilingLightController : MonoBehaviour
{
    // 天花板灯游戏对象
    public GameObject ceilingLight;

    // 记录初始旋转值
    private Quaternion initialRotation;

    // 冷却时间相关变量
    private float cooldownTime = 1f;
    private float cooldownTimer = 0f;

    // 记录上一次检测时的旋转值
    private float lastCheckedRotation;

    // 追踪是否是第一次切换状态
    private bool isFirstSwitch = true;

    void Start()
    {
        // 确保已经指定了天花板灯
        if (ceilingLight == null)
        {
            Debug.LogError("请在Inspector中指定天花板灯！");
            enabled = false;
            return;
        }

        // 记录初始旋转值
        initialRotation = transform.rotation;
        lastCheckedRotation = transform.rotation.eulerAngles.z;
    }

    void Update()
    {
        // 更新冷却时间
        cooldownTimer -= Time.deltaTime;

        // 如果在冷却中，直接返回
        if (cooldownTimer > 0)
        {
            return;
        }

        // 获取当前Z轴旋转值
        float currentRotationZ = transform.rotation.eulerAngles.z;

        // 计算与上一次检测时的旋转差值
        float rotationDelta = Mathf.Abs(currentRotationZ - lastCheckedRotation);

        // 如果旋转差值大于50度
        if (rotationDelta > 50f)
        {
            // 切换灯光状态
            ceilingLight.SetActive(!ceilingLight.activeSelf);

            // 检查是否是第一次切换
            if (isFirstSwitch)
            {
                Debug.Log("light off add point");
               
                    ScoreSystem.Instance.AddScore(1);
                   
                isFirstSwitch = false;
            }
            else
            {
                Debug.Log($"检测到旋转变化{rotationDelta}度，切换灯光状态为: {ceilingLight.activeSelf}");
            }

            // 重置冷却时间
            cooldownTimer = cooldownTime;

            // 更新上一次检测的旋转值
            lastCheckedRotation = currentRotationZ;
        }
    }
}
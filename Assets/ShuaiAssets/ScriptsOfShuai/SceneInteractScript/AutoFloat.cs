using UnityEngine;

public class AutoFloat : MonoBehaviour
{
    [Header("漂浮参数")]
    [SerializeField] private float floatHeight = 0.5f; // 漂浮高度
    [SerializeField] private float floatSpeed = 1f;    // 漂浮速度
    [SerializeField] private float smoothTime = 0.3f;  // 平滑过渡时间

    private Vector3 startPosition;
    private float currentVelocity;
    private float targetY;
    private float currentY;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPosition = transform.position;
        currentY = startPosition.y;
        targetY = startPosition.y;
    }

    // Update is called once per frame
    void Update()
    {
        // 计算目标Y位置
        targetY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        
        // 使用平滑阻尼实现渐进渐出效果
        currentY = Mathf.SmoothDamp(currentY, targetY, ref currentVelocity, smoothTime);
        
        // 更新位置
        transform.position = new Vector3(startPosition.x, currentY, startPosition.z);
    }
}

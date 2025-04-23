using UnityEngine;

public class AutoFloat : MonoBehaviour
{
    [Header("漂浮参数")]
    [SerializeField] private float floatHeight = 0.5f; // 漂浮高度
    [SerializeField] private float floatSpeed = 1f;    // 漂浮速度
    [SerializeField] private float smoothTime = 0.3f;  // 平滑过渡时间
    [SerializeField] private bool useLocalSpace = true; // 是否使用局部坐标系
    [SerializeField] private float positionThreshold = 0.001f; // 位置变化阈值

    private Vector3 startPosition;
    private Vector3 lastPosition;
    private float currentVelocity;
    private float targetY;
    private float currentY;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPosition = useLocalSpace ? transform.localPosition : transform.position;
        lastPosition = startPosition;
        currentY = startPosition.y;
        targetY = startPosition.y;
        currentVelocity = 0f; // 初始化速度
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 currentPosition = useLocalSpace ? transform.localPosition : transform.position;
        
        // 使用阈值判断位置是否发生变化
        if (Vector3.Distance(currentPosition, lastPosition) > positionThreshold)
        {
            // 计算位置偏移
            Vector3 positionDelta = currentPosition - lastPosition;
            // 更新起始位置
            startPosition += positionDelta;
            // 更新当前Y值
            currentY += positionDelta.y;
            // 更新上一帧位置
            lastPosition = currentPosition;
        }

        // 计算目标Y位置
        targetY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        
        // 使用平滑阻尼实现渐进渐出效果
        currentY = Mathf.SmoothDamp(currentY, targetY, ref currentVelocity, smoothTime);
        
        // 更新位置
        if (useLocalSpace)
        {
            transform.localPosition = new Vector3(currentPosition.x, currentY, currentPosition.z);
        }
        else
        {
            transform.position = new Vector3(currentPosition.x, currentY, currentPosition.z);
        }
    }
}

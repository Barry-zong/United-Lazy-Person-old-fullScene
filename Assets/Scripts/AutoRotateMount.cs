using UnityEngine;

public class AutoRotateMount : MonoBehaviour
{
    [Header("旋转设置")]
    public float targetRotation = -90f; // 目标旋转角度
    public float rotationSpeed = 1f;    // 旋转速度

    private Vector3 initialRotation;    // 初始旋转值
    private bool isRotating = false;    // 是否正在旋转
    private float currentRotationTime = 0f; // 当前旋转时间

    private void Start()
    {
        // 记录初始旋转值
        initialRotation = transform.localEulerAngles;
    }

    private void Update()
    {
        if (isRotating)
        {
            // 计算当前旋转时间
            currentRotationTime += Time.deltaTime * rotationSpeed;
            
            // 使用Lerp进行平滑旋转
            float newX = Mathf.Lerp(initialRotation.x, targetRotation, currentRotationTime);
            
            // 应用新的旋转值
            transform.localEulerAngles = new Vector3(
                newX,
                transform.localEulerAngles.y,
                transform.localEulerAngles.z
            );

            // 检查是否达到目标旋转
            if (currentRotationTime >= 1f)
            {
                isRotating = false;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 检查进入的物体是否带有"Hand"标签
        if (other.CompareTag("Hand"))
        {
            // 开始旋转
            isRotating = true;
            currentRotationTime = 0f;
        }
    }
} 
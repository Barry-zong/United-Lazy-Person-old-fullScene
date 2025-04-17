using UnityEngine;

public class BinCoverAutoOpen : MonoBehaviour
{
    [Header("旋转设置")]
    [SerializeField]
    private GameObject binCover;
    public float rotationAngle = 90f;   // 旋转角度（相对）
    public float rotationSpeed = 1f;    // 旋转速度
    [Header("测试设置")]
    public bool testMode = false;       // 测试模式开关

    private Vector3 initialRotation;    // 初始旋转值
    private bool isRotating = false;    // 是否正在旋转
    private float currentRotationTime = 0f; // 当前旋转时间
    private bool isOpen = false;        // 盖子当前状态
    private float targetRotation;       // 目标旋转角度
    private float currentVelocity = 0f; // 当前速度

    private void Start()
    {
        // 记录初始旋转值
        initialRotation = binCover.transform.localEulerAngles;
        targetRotation = initialRotation.x + rotationAngle;
      //  Debug.Log($"初始角度: {initialRotation.x}, 目标角度: {targetRotation}");
    }

    private void Update()
    {
        // 测试模式下的控制
        if (testMode)
        {
            isRotating = true;
            currentVelocity = 0f;
        }

        if (isRotating)
        {
            // 获取当前角度
            float currentX = binCover.transform.localEulerAngles.x;
            
            // 处理欧拉角在0-360度之间的转换
            if (currentX > 180f) currentX -= 360f;
            
            // 计算目标角度
            float targetX = isOpen ? initialRotation.x : targetRotation;
            if (targetX > 180f) targetX -= 360f;
            
            // 使用SmoothDamp进行更平滑的旋转
            float newX = Mathf.SmoothDamp(currentX, targetX, ref currentVelocity, rotationSpeed);
            
            // 应用新的旋转值
            binCover.transform.localEulerAngles = new Vector3(
                newX,
                binCover.transform.localEulerAngles.y,
                binCover.transform.localEulerAngles.z
            );

            // 检查是否接近目标角度
            float angleDiff = Mathf.Abs(newX - targetX);
          //  Debug.Log($"当前角度: {newX}, 目标角度: {targetX}, 角度差: {angleDiff}");
            
            if (angleDiff < 0.5f)
            {
                isRotating = false;
                isOpen = !isOpen;
               // Debug.Log($"旋转完成，新状态: {(isOpen ? "打开" : "关闭")}");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {

        // 检查退出的物体是否带有"Hand"标签
        if (other.CompareTag("Hand") && !testMode)
        {
          //  Debug.Log("检测到Hand标签物体退出");
            // 开始旋转回初始位置
            isRotating = true;
            isOpen = false;  // 设置为关闭状态
            currentVelocity = 0f;
        }
        
    }

    private void OnTriggerExit(Collider other)
    {
        // 检查进入的物体是否带有"Hand"标签
        if (other.CompareTag("Hand") && !testMode)
        {
           // Debug.Log("检测到Hand标签物体进入");
            // 开始旋转
            isRotating = true;
            isOpen = true;  // 设置为打开状态
            currentVelocity = 0f;
        }
    }
}

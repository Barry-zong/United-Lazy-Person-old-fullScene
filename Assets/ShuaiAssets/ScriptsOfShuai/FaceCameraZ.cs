using UnityEngine;

public class FaceCameraZ : MonoBehaviour
{
    [Tooltip("需要面向的目标相机")]
    public Transform targetCamera;

    [Tooltip("是否在Y轴方向保持垂直")]
    public bool lockYRotation = false;

    [Tooltip("是否反转朝向使物体尾部朝向相机")]
    public bool invertFacing = true;

    void Start()
    {
        // 如果没有指定目标相机，默认使用主相机
        if (targetCamera == null)
        {
            targetCamera = Camera.main.transform;
        }
    }

    void Update()
    {
        if (targetCamera != null)
        {
            Vector3 directionToCamera = targetCamera.position - transform.position;

            // 如果需要锁定Y轴旋转，只在水平面内旋转
            if (lockYRotation)
            {
                directionToCamera.y = 0;
            }

            // 确保方向向量不为零
            if (directionToCamera != Vector3.zero)
            {
                // 反转方向使物体尾部（而不是前端）朝向相机
                if (invertFacing)
                {
                    directionToCamera = -directionToCamera;
                }

                // 计算物体旋转使Z轴基准朝向目标
                transform.rotation = Quaternion.LookRotation(directionToCamera);
            }
        }
    }
}
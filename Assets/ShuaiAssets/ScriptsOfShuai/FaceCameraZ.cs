using UnityEngine;

public class FaceCameraZ : MonoBehaviour
{
    [Tooltip("需要朝向的目标摄像机")]
    public Transform targetCamera;

    [Tooltip("是否在Y轴上锁定旋转（保持垂直）")]
    public bool lockYRotation = false;

    [Tooltip("是否反转朝向（蓝轴尾端朝向摄像机）")]
    public bool invertFacing = true;

    void Start()
    {
        // 如果没有指定摄像机，默认使用主摄像机
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

            // 如果需要锁定Y轴旋转（只在水平面上旋转）
            if (lockYRotation)
            {
                directionToCamera.y = 0;
            }

            // 确保方向向量不为零
            if (directionToCamera != Vector3.zero)
            {
                // 反转方向，使蓝轴尾端（而非前端）朝向摄像机
                if (invertFacing)
                {
                    directionToCamera = -directionToCamera;
                }

                // 设置物体旋转，使Z轴对准摄像机方向
                transform.rotation = Quaternion.LookRotation(directionToCamera);
            }
        }
    }
}
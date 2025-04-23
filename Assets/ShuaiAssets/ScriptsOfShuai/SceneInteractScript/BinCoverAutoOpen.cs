using UnityEngine;
using System.Collections.Generic;

public class BinCoverAutoOpen : MonoBehaviour
{
    [Header("旋转设置")]
    [SerializeField]
    private GameObject binCover;
    public float rotationAngle = 90f;   // 旋转角度（相对）
    public float rotationSpeed = 1f;    // 旋转速度
    
    public enum RotationAxis
    {
        X,
        Y,
        Z
    }
    [SerializeField]
    public RotationAxis rotationAxis = RotationAxis.Y;  // 默认改为Y轴旋转
    
    [Header("测试设置")]
    public bool testMode = false;       // 测试模式开关

    private Quaternion initialRotation;    // 改用Quaternion存储初始旋转
    private bool isRotating = false;
    private bool isOpen = false;
    private HashSet<GameObject> handsInTrigger = new HashSet<GameObject>(); // 跟踪区域内的手
    private bool isGameEnded = false; // 游戏是否结束

    private void Start()
    {
        if (binCover == null)
        {
            Debug.LogError("BinCover reference is missing!");
            return;
        }
        initialRotation = binCover.transform.localRotation;
        isOpen = false; // 确保初始状态为关闭
    }

    private void Update()
    {
        if (binCover == null) return;

        // 检查游戏是否结束
        if (CountdownTimer.Instance != null && CountdownTimer.Instance.IsTimerFinished)
        {
            isGameEnded = true;
            // 如果游戏结束，强制关闭垃圾桶盖
            if (isOpen)
            {
                isRotating = true;
                isOpen = false;
            }
        }

        if (testMode)
        {
            isRotating = true;
            isOpen = !isOpen; // 在测试模式下切换状态
        }

        if (isRotating)
        {
            Vector3 rotationAxis = Vector3.up; // 默认Y轴

            // 根据选择的轴向设置旋转轴
            switch (this.rotationAxis)
            {
                case RotationAxis.X:
                    rotationAxis = Vector3.right;
                    break;
                case RotationAxis.Y:
                    rotationAxis = Vector3.up;
                    break;
                case RotationAxis.Z:
                    rotationAxis = Vector3.forward;
                    break;
            }

            // 计算目标旋转
            Quaternion targetRotation = isOpen ? 
                initialRotation * Quaternion.AngleAxis(rotationAngle, rotationAxis) : 
                initialRotation;

            // 使用更稳定的旋转插值
            float smoothTime = 0.1f; // 平滑时间
            binCover.transform.localRotation = Quaternion.Slerp(
                binCover.transform.localRotation,
                targetRotation,
                Time.deltaTime * (1.0f / smoothTime) * rotationSpeed
            );

            // 检查是否接近目标旋转
            if (Quaternion.Angle(binCover.transform.localRotation, targetRotation) < 0.5f)
            {
                isRotating = false;
                binCover.transform.localRotation = targetRotation; // 确保精确到达目标旋转
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == null || isGameEnded) return; // 如果游戏结束，不响应触发
        
        if (other.CompareTag("Hand") && !testMode)
        {
            handsInTrigger.Add(other.gameObject);
            isRotating = true;
            isOpen = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == null || isGameEnded) return; // 如果游戏结束，不响应触发
        
        if (other.CompareTag("Hand") && !testMode)
        {
            handsInTrigger.Remove(other.gameObject);
            
            // 只有当所有手都离开时才关闭
            if (handsInTrigger.Count == 0)
            {
                isRotating = true;
                isOpen = false;
            }
        }
    }
}

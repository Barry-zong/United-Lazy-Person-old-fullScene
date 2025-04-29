using UnityEngine;

public class AutoRotate : MonoBehaviour
{
    [Header("旋转控制")]
    public float rotateSpeed = 30f; // 旋转速度
    public bool rotateAroundX = false; // 是否绕X轴旋转
    public bool rotateAroundY = false; // 是否绕Y轴旋转
    public bool rotateAroundZ = false; // 是否绕Z轴旋转

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // 计算旋转角度
        float rotationAmount = rotateSpeed * Time.deltaTime;
        
        // 根据勾选的轴进行旋转
        if (rotateAroundX)
        {
            transform.Rotate(Vector3.right * rotationAmount);
        }
        if (rotateAroundY)
        {
            transform.Rotate(Vector3.up * rotationAmount);
        }
        if (rotateAroundZ)
        {
            transform.Rotate(Vector3.forward * rotationAmount);
        }
    }
}

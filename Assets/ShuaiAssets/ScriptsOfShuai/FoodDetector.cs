using UnityEngine;
using System.Collections.Generic;

public class FoodDetector : MonoBehaviour
{
    // 使用HashSet记录已经检测过的物体，确保每个物体只被检测一次
    private HashSet<GameObject> detectedFood = new HashSet<GameObject>();

    private void OnTriggerEnter(Collider other)
    {
        // 检查碰撞物体是否带有"Food"标签，且之前没有被检测过
        if (other.CompareTag("Food") && !detectedFood.Contains(other.gameObject))
        {
            // 将物体添加到已检测集合中
            detectedFood.Add(other.gameObject);
            ScoreSystem.Instance.AddScore(1);
            // 输出标志
            Debug.Log("trash point added");
        }
    }
}
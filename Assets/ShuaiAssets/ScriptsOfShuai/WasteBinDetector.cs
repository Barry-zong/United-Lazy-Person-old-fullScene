using UnityEngine;
using System.Collections.Generic;

public class WasteBinDetector : MonoBehaviour
{
    [Header("垃圾桶类型设置")]
    public bool isFoodWaste = false;
    public bool isRecyclables = false;
    public bool isLandfill = false;
    public bool isCompost = false;

    // 使用HashSet记录已经检测过的物体，确保每个物体只被检测一次
    private HashSet<GameObject> detectedWaste = new HashSet<GameObject>();

    // 定义合法的垃圾标签
    private readonly string[] validWasteTags = { "Food", "Compost", "Recyclables", "LandFill" };

    private bool IsValidWaste(GameObject obj)
    {
        foreach (string tag in validWasteTags)
        {
            if (obj.CompareTag(tag))
                return true;
        }
        return false;
    }

    private void Awake()
    {
        // 检查必要的组件
        Collider collider = GetComponent<Collider>();
        if (collider == null)
        {
          //  Debug.LogError($"垃圾桶 {gameObject.name} 缺少Collider组件！");
        }
        else
        {
          //  Debug.Log($"垃圾桶 {gameObject.name} Collider设置: IsTrigger={collider.isTrigger}");
        }

        // 检查垃圾桶类型设置
        int selectedCount = 0;
        if (isFoodWaste) selectedCount++;
        if (isRecyclables) selectedCount++;
        if (isLandfill) selectedCount++;
        if (isCompost) selectedCount++;

        if (selectedCount == 0)
        {
           // Debug.LogError($"垃圾桶 {gameObject.name} 没有选择任何垃圾类型！");
        }
        else if (selectedCount > 1)
        {
          //  Debug.LogError($"垃圾桶 {gameObject.name} 选择了多个垃圾类型！");
        }
        else
        {
            string type = isFoodWaste ? "食物" : 
                         isRecyclables ? "可回收" : 
                         isLandfill ? "其他" : "堆肥";
           // Debug.Log($"垃圾桶 {gameObject.name} 设置为: {type}垃圾桶");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 首先检查是否是有效的垃圾物体
        if (!IsValidWaste(other.gameObject))
            return;

        // 如果物体已经被检测过，直接返回
        if (detectedWaste.Contains(other.gameObject))
            return;

        // 缩小物体尺寸
        if (other.gameObject.CompareTag("Food") || 
            other.gameObject.CompareTag("Recyclables") || 
            other.gameObject.CompareTag("LandFill") || 
            other.gameObject.CompareTag("Compost"))
        {
            other.gameObject.transform.localScale *= 0.5f;
        }

        bool isCorrectBin = false;

        // 根据垃圾桶类型检查对应的垃圾标签
        if (isFoodWaste && other.gameObject.CompareTag("Food"))
            isCorrectBin = true;
        else if (isRecyclables && other.gameObject.CompareTag("Recyclables"))
            isCorrectBin = true;
        else if (isLandfill && other.gameObject.CompareTag("LandFill"))
            isCorrectBin = true;
        else if (isCompost && (other.gameObject.CompareTag("Compost") || other.gameObject.CompareTag("Food")))
            isCorrectBin = true;

        // 将物体添加到已检测集合中
        detectedWaste.Add(other.gameObject);

        if (isCorrectBin)
        {
            if (ScoreSystem.Instance != null)
            {
                ScoreSystem.Instance.AddScore(1);
               // Debug.Log("分类正确！得分 +1");
            }

            // 检查物体是否有UISystemOfOBJ组件并触发成功事件
            UISystemOfOBJ uiSystem = other.gameObject.GetComponentInChildren<UISystemOfOBJ>();
            if (uiSystem != null)
            {
                uiSystem.TriggerWin();
            }
        }
        else
        {
           // Debug.Log("分类错误，请放入正确的垃圾桶");

            // 检查物体是否有UISystemOfOBJ组件并触发失败事件
            UISystemOfOBJ uiSystem = other.gameObject.GetComponentInChildren<UISystemOfOBJ>();
            if (uiSystem != null)
            {
                uiSystem.TriggerFail();
            }
        }
    }

    // 用于重置检测状态的方法
    public void ResetDetection()
    {
        detectedWaste.Clear();
    }

    // 在Inspector中验证只能选择一种类型
    private void OnValidate()
    {
        int selectedCount = 0;
        if (isFoodWaste) selectedCount++;
        if (isRecyclables) selectedCount++;
        if (isLandfill) selectedCount++;
        if (isCompost) selectedCount++;

        if (selectedCount > 1)
        {
          //  Debug.LogWarning("每个检测器只能选择一种垃圾类型！");
            isFoodWaste = false;
            isRecyclables = false;
            isLandfill = false;
            isCompost = false;
        }
    }
} 
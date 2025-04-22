using UnityEngine;
using System.Collections.Generic;

public class SideTreeGen : MonoBehaviour
{
    public GameObject treeForest;
    private List<Transform> childTransforms = new List<Transform>();
    private List<Vector3> originalScales = new List<Vector3>();
    private float growthSpeed = 2f;
    private bool isGrowing = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (treeForest != null)
        {
            // 获取所有子物体并记录它们的原始大小
            foreach (Transform child in treeForest.transform)
            {
                childTransforms.Add(child);
                originalScales.Add(child.localScale);
                child.localScale = Vector3.zero;
            }
            treeForest.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (treeForest == null) return;

        // 检查分数是否大于8
        if (ScoreSystem.Instance.GetCurrentScore() > 8)
        {
            if (!treeForest.activeSelf)
            {
                treeForest.SetActive(true);
                isGrowing = true;
            }

            if (isGrowing)
            {
                bool allReachedTarget = true;
                
                // 遍历所有子物体，渐变增加它们的大小
                for (int i = 0; i < childTransforms.Count; i++)
                {
                    Transform child = childTransforms[i];
                    Vector3 targetScale = originalScales[i];
                    
                    child.localScale = Vector3.Lerp(
                        child.localScale,
                        targetScale,
                        Time.deltaTime * growthSpeed
                    );

                    // 检查是否接近目标大小
                    if (Vector3.Distance(child.localScale, targetScale) > 0.01f)
                    {
                        allReachedTarget = false;
                    }
                }

                // 当所有子物体都达到目标大小时停止生长
                if (allReachedTarget)
                {
                    for (int i = 0; i < childTransforms.Count; i++)
                    {
                        childTransforms[i].localScale = originalScales[i];
                    }
                    isGrowing = false;
                }
            }
        }
    }
}

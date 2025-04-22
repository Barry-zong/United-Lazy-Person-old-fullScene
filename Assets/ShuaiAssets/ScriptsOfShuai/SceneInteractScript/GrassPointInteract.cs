using UnityEngine;

public class GrassPointInteract : MonoBehaviour
{
    public GameObject grassObject;
    public float growthSpeed = 0.5f; // 生长速度
    public float targetScale = 0f; // 目标高度
    private Vector3 originalScale; // 原始缩放
    private ScoreSystem scoreSystem; // 分数系统引用

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (grassObject != null)
        {
            originalScale = grassObject.transform.localScale;
            // 初始时将草的Y轴缩放设为0并禁用对象
            grassObject.transform.localScale = new Vector3(originalScale.x, 0, originalScale.z);
            grassObject.SetActive(false);
        }
        
        // 获取分数系统实例
        scoreSystem = FindFirstObjectByType<ScoreSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        if (grassObject == null || scoreSystem == null) return;

        // 根据分数设置目标高度
        switch (scoreSystem.GetCurrentScore())
        {
            case 0:
                targetScale = 0f;
                break;  
            case 1:
                targetScale = 0.4f;
                break;
            case 2:
                targetScale = 0.8f;
                break;
            case 3:
            default: // 3分及以上都保持最大高度
                targetScale = 1f;
                break;
        }

        // 如果目标高度大于0且对象未激活，则激活对象
        if (targetScale > 0 && !grassObject.activeSelf)
        {
            grassObject.SetActive(true);
        }
        // 如果目标高度为0且对象已激活，则禁用对象
        else if (targetScale == 0 && grassObject.activeSelf)
        {
            grassObject.SetActive(false);
        }

        // 平滑过渡到目标高度
        Vector3 currentScale = grassObject.transform.localScale;
        float newYScale = Mathf.Lerp(currentScale.y, targetScale, Time.deltaTime * growthSpeed);
        grassObject.transform.localScale = new Vector3(originalScale.x, newYScale, originalScale.z);
    }
}

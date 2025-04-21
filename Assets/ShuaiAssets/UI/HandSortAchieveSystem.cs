using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class HandSortAchieveSystem : MonoBehaviour
{
    public static HandSortAchieveSystem Instance { get; private set; }
    public float defaultAlpha = 0.5f; // 默认透明度
    public float transitionDuration = 0.5f; // 渐变过渡时间

    private List<Image> achievementImages = new List<Image>();
    private List<Material> achievementMaterials = new List<Material>();
    [SerializeField] private int n = 0;
    private HashSet<int> activatedAchievements = new HashSet<int>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 获取所有子物体
        Transform[] children = GetComponentsInChildren<Transform>(true);
        
        foreach (Transform child in children)
        {
            // 检查子物体名称是否为纯数字
            if (int.TryParse(child.name, out int number))
            {
                Image image = child.GetComponent<Image>();
                if (image != null)
                {
                    achievementImages.Add(image);
                    n++;

                    // 实例化材质
                    Material material = new Material(Shader.Find("Unlit/ShaderForUI"));
                    image.material = material;
                    achievementMaterials.Add(material);
                    
                    // 设置初始饱和度
                    material.SetFloat("_Saturation", 0f);
                }
            }
        }

        // 初始化所有成就图标为半透明
        foreach (Image image in achievementImages)
        {
            SetImageAlpha(image, defaultAlpha);
        }
    }

    /// <summary>
    /// 激活指定序号的成就
    /// </summary>
    /// <param name="achievementNumber">成就序号（1-n）</param>
    public void ActivateAchieve(int achievementNumber)
    {
        if (achievementNumber < 1 || achievementNumber > n)
        {
            Debug.LogWarning($"成就序号 {achievementNumber} 超出范围 (1-{n})");
            return;
        }

        // 检查是否已经激活过
        if (activatedAchievements.Contains(achievementNumber))
        {
            Debug.Log($"成就 {achievementNumber} 已经激活过，不再重复激活");
            return;
        }

        // 记录已激活的成就
        activatedAchievements.Add(achievementNumber);

        Image targetImage = achievementImages[achievementNumber - 1];
        Material targetMaterial = achievementMaterials[achievementNumber - 1];
        StartCoroutine(TransitionToColorful(targetImage, targetMaterial));
    }

    private IEnumerator TransitionToColorful(Image image, Material material)
    {
        float elapsedTime = 0f;
        Color startColor = image.color;
        Color targetColor = new Color(1f, 1f, 1f, 1f);

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / transitionDuration);
            
            // 使用平滑的插值
            t = Mathf.SmoothStep(0f, 1f, t);
            
            // 插值Alpha和饱和度
            float a = Mathf.Lerp(startColor.a, targetColor.a, t);
            material.SetFloat("_Saturation", t);
            
            image.color = new Color(1f, 1f, 1f, a);
            yield return null;
        }

        // 确保最终颜色和饱和度准确
        image.color = targetColor;
        material.SetFloat("_Saturation", 1f);
    }

    private void SetImageAlpha(Image image, float alpha)
    {
        if (image == null) return;
        
        Color color = image.color;
        image.color = new Color(1f, 1f, 1f, Mathf.Clamp01(alpha));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

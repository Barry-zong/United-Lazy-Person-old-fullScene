using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class HandSortAchieveSystem : MonoBehaviour
{
    public static HandSortAchieveSystem Instance { get; private set; }
    public float defaultAlpha = 0.1f; // 默认透明度
    public float transitionDuration = 0.5f; // 渐变过渡时间

    private List<List<Image>> achievementImages = new List<List<Image>>();
    private List<List<Color>> originalColors = new List<List<Color>>(); // 存储原始颜色
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
                // 获取该数字物体下所有的Image组件（包括子物体）
                Image[] images = child.GetComponentsInChildren<Image>(true);
                if (images.Length > 0)
                {
                    List<Image> imageList = new List<Image>();
                    List<Color> colorList = new List<Color>();
                    
                    foreach (Image image in images)
                    {
                        imageList.Add(image);
                        // 记录原始颜色
                        colorList.Add(image.color);
                        // 设置图片为灰色
                        SetImageGray(image);
                    }
                    
                    achievementImages.Add(imageList);
                    originalColors.Add(colorList);
                    n++;
                }
            }
        }
    }

    private void SetImageGray(Image image)
    {
        if (image == null) return;
        // 设置图片为灰色（降低饱和度）
        image.color = new Color(0.15f, 0.15f, 0.15f, defaultAlpha);
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

        // 获取该成就的所有Image和原始颜色
        List<Image> targetImages = achievementImages[achievementNumber - 1];
        List<Color> targetColors = originalColors[achievementNumber - 1];

        // 为每个Image启动过渡效果
        for (int i = 0; i < targetImages.Count; i++)
        {
            StartCoroutine(TransitionToColorful(targetImages[i], targetColors[i]));
        }
    }

    private IEnumerator TransitionToColorful(Image image, Color targetColor)
    {
        float elapsedTime = 0f;
        Color startColor = image.color;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / transitionDuration);
            
            // 使用平滑的插值
            t = Mathf.SmoothStep(0f, 1f, t);
            
            // 从灰色渐变到原始颜色
            image.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }

        // 确保最终颜色准确
        image.color = targetColor;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

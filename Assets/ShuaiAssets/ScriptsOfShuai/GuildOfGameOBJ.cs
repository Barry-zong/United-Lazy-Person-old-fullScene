using UnityEngine;

public class GuildOfGameOBJ : MonoBehaviour
{
    public GameObject PlayObj;
    public GameObject col;
    public float gapTime = 1.0f;
    public float minIntensity = -10f;
    public float maxIntensity = 2f;
    public float breathSpeed = 1.0f;
    public Color emissionColor = Color.white;
    public GameObject followActiveObj;

    private Material[] materials;
    private bool isBreathing = false;
    private float currentIntensity = 0f;
    private float timer = 0f;
    private bool isIncreasing = true;
    private bool isInitialized = false;
    private GuildOfStatueCheck statueCheck;

    void Awake()
    {
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 检查必要参数
        if (PlayObj == null || col == null)
        {
            Debug.LogError("GuildOfGameOBJ: PlayObj or col is not assigned!");
            enabled = false;
            return;
        }

        // 获取 GuildOfStatueCheck 组件
        statueCheck = col.GetComponent<GuildOfStatueCheck>();
        if (statueCheck == null)
        {
            Debug.LogError("GuildOfGameOBJ: col does not have GuildOfStatueCheck component!");
            enabled = false;
            return;
        }

        Renderer renderer = PlayObj.GetComponent<Renderer>();
        if (renderer == null)
        {
            Debug.LogError("GuildOfGameOBJ: PlayObj has no Renderer component!");
            enabled = false;
            return;
        }

        // 获取所有材质
        materials = new Material[renderer.materials.Length];
        for (int i = 0; i < renderer.materials.Length; i++)
        {
            // 实例化每个材质
            materials[i] = new Material(renderer.materials[i]);
            // 启用自发光
            materials[i].EnableKeyword("_EMISSION");
            materials[i].globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;

            // 移除Emission Map
            if (materials[i].HasProperty("_EmissionMap"))
            {
                materials[i].SetTexture("_EmissionMap", null);
            }

            // 设置默认自发光强度为-10
            float initialIntensity = Mathf.Pow(2, minIntensity);
            Color initialColor = emissionColor * initialIntensity;
            materials[i].SetColor("_EmissionColor", initialColor);
        }

        // 应用所有新材质
        renderer.materials = materials;
        isInitialized = true;
    }

    // Update is called once per frame
    void Update()
    {
        // 检查followActiveObj是否被销毁
        if (followActiveObj == null)
        {
            gameObject.SetActive(false);
            return;
        }

        if (!isInitialized) return;

        // 根据 statueCheck.HandIn 状态控制呼吸效果
        if (statueCheck.HandIn)
        {
            if (!isBreathing)
            {
                isBreathing = true;
                timer = 0f;
                isIncreasing = true;
            }

            // 呼吸效果
            if (isIncreasing)
            {
                currentIntensity = Mathf.Lerp(minIntensity, maxIntensity, timer / breathSpeed);
                if (timer >= breathSpeed)
                {
                    isIncreasing = false;
                    timer = 0f;
                }
            }
            else
            {
                currentIntensity = Mathf.Lerp(maxIntensity, minIntensity, timer / breathSpeed);
                if (timer >= breathSpeed)
                {
                    isIncreasing = true;
                    timer = 0f;
                }
            }

            // 应用自发光强度到所有材质
            if (materials != null)
            {
                float intensity = Mathf.Pow(2, currentIntensity);
                Color newColor = emissionColor * intensity;
                foreach (var material in materials)
                {
                    material.SetColor("_EmissionColor", newColor);
                }
            }

            timer += Time.deltaTime;
        }
        else if (isBreathing)
        {
            isBreathing = false;
            if (materials != null)
            {
                // 恢复默认发光强度为-10
                float defaultIntensity = Mathf.Pow(2, minIntensity);
                Color defaultColor = emissionColor * defaultIntensity;
                foreach (var material in materials)
                {
                    material.SetColor("_EmissionColor", defaultColor);
                }
            }
        }
    }

    private void OnDisable()
    {
        if (!isInitialized) return;
        
        if (materials != null)
        {
            // 恢复默认发光强度为-10
            float defaultIntensity = Mathf.Pow(2, minIntensity);
            Color defaultColor = emissionColor * defaultIntensity;
            foreach (var material in materials)
            {
                material.SetColor("_EmissionColor", defaultColor);
            }
        }
    }

    private void OnDestroy()
    {
        if (!isInitialized) return;
        
        if (materials != null)
        {
            // 恢复默认发光强度为-10
            float defaultIntensity = Mathf.Pow(2, minIntensity);
            Color defaultColor = emissionColor * defaultIntensity;
            foreach (var material in materials)
            {
                material.SetColor("_EmissionColor", defaultColor);
            }
        }
    }
}

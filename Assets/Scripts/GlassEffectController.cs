using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class GlassEffectController : MonoBehaviour
{
    [SerializeField] private float blurAmount = 0.5f;
    [SerializeField] private float transparency = 0.5f;
    
    private Material glassMaterial;
    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
        if (image != null)
        {
            // 确保使用正确的Shader
            var shader = Shader.Find("Custom/GlassEffect");
            if (shader != null)
            {
                glassMaterial = new Material(shader);
                image.material = glassMaterial;
                
                // 确保Image组件设置正确
                image.raycastTarget = true;
                image.maskable = true;
                
                UpdateMaterialProperties();
            }
            else
            {
                Debug.LogError("无法找到Custom/GlassEffect Shader！请确保Shader文件已正确导入。");
            }
        }
    }

    private void OnEnable()
    {
        UpdateMaterialProperties();
    }

    private void UpdateMaterialProperties()
    {
        if (glassMaterial != null)
        {
            glassMaterial.SetFloat("_BlurAmount", blurAmount);
            glassMaterial.SetFloat("_Transparency", transparency);
        }
    }

    public void SetBlurAmount(float amount)
    {
        blurAmount = Mathf.Clamp01(amount);
        UpdateMaterialProperties();
    }

    public void SetTransparency(float value)
    {
        transparency = Mathf.Clamp01(value);
        UpdateMaterialProperties();
    }

    private void OnDestroy()
    {
        if (glassMaterial != null)
        {
            if (Application.isPlaying)
            {
                Destroy(glassMaterial);
            }
            else
            {
                DestroyImmediate(glassMaterial);
            }
        }
    }
} 
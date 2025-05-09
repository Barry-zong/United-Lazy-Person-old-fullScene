using UnityEngine;

public class MaterialColorCycle : MonoBehaviour
{
    public float cycleSpeed = 1f; // 色彩循环速度
    public string colorProperty = "_Color"; // 材质颜色属性名

    private Material mat;
    private float hue = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            // 实例化材质，避免影响同材质的其他物体
            mat = renderer.material;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (mat != null)
        {
            hue += Time.deltaTime * cycleSpeed;
            hue = hue % 1f; // 保证hue在0-1之间循环
            Color color = Color.HSVToRGB(hue, 1f, 1f);
            mat.SetColor(colorProperty, color);
        }
    }
}

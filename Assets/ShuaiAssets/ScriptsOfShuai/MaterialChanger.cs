using UnityEngine;

public class MaterialChanger : MonoBehaviour
{
    public Material targetMaterial;
    private bool Firstadded = true;
    public Color triggerColor = Color.red;

    void Start()
    {
        // 确保材质已设置
        if (targetMaterial == null)
        {
            Debug.LogWarning("没有设置目标材质!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (targetMaterial != null)
        {
            // 修改材质的自发光颜色
            targetMaterial.SetColor("_EmissionColor", triggerColor);
            // 确保自发光是启用的
            targetMaterial.EnableKeyword("_EMISSION");

            if (Firstadded)
            {
                ScoreSystem.Instance.AddScore(1);
                Firstadded = false;
            }
        }
    }
}
using UnityEngine;

public class MaterialChanger : MonoBehaviour
{
    public Material targetMaterial;
    private bool Firstadded = true;
    public Color triggerColor = Color.red;
    public UISystemOfOBJ uisysys;
    public Color triggerColorS = Color.red;

    void Start()
    {
        // 确保材质存在
        if (targetMaterial == null)
        {
            Debug.LogWarning("没有设置目标材质!");
        }
        targetMaterial.SetColor("_EmissionColor", triggerColorS);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (targetMaterial != null && other.CompareTag("Hand"))
        {
            // 修改材质的发光颜色
            targetMaterial.SetColor("_EmissionColor", triggerColor);
            // 确保发光效果被启用
            targetMaterial.EnableKeyword("_EMISSION");

            if (Firstadded)
            {
                ScoreSystem.Instance.AddScore(1);
                uisysys.TriggerWin();
                Firstadded = false;
            }
        }
    }
}
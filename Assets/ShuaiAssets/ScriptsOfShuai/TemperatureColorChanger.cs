using UnityEngine;

namespace CustomInteraction
{
    public class TemperatureColorChanger : MonoBehaviour
    {
        [Header("Color Settings")]
        public Material targetMaterial;      // 目标材质
        public Color coldColor = Color.blue; // 低温颜色 (60°F)
        public Color hotColor = Color.red;   // 高温颜色 (90°F)

        private void Start()
        {
            if (targetMaterial == null)
            {
                // 如果没有指定材质，尝试从当前对象获取
                var renderer = GetComponent<Renderer>();
                if (renderer != null)
                {
                    targetMaterial = renderer.material;
                }
                else
                {
                    Debug.LogError("No target material assigned and no renderer found!");
                    return;
                }
            }

            // 获取温度控制器并订阅事件
            var tempController = GetComponent<TemperatureControllerTransformer>();
            if (tempController != null)
            {
                tempController.OnTemperatureChanged.AddListener(UpdateColor);
            }
            else
            {
                Debug.LogError("TemperatureControllerTransformer not found on the same object!");
            }
        }

        private void UpdateColor(float temperature)
        {
            // 将温度(90°F - 60°F)映射到颜色插值(0-1)
            float t = (temperature - 60f) / (90f - 60f);
            targetMaterial.color = Color.Lerp(coldColor, hotColor, t);
        }

        private void OnDestroy()
        {
            // 清理事件订阅
            var tempController = GetComponent<TemperatureControllerTransformer>();
            if (tempController != null)
            {
                tempController.OnTemperatureChanged.RemoveListener(UpdateColor);
            }
        }
    }
}
using UnityEngine;
namespace CustomInteraction
{
    public class TemperatureColorChanger : MonoBehaviour
    {
        [Header("Color Settings")]
        public Material targetMaterial;
        public UISystemOfOBJ uisysys;
        public Color coldColor = Color.blue;
        public Color hotColor = Color.red;
        private bool Firstadded = true;

        [Header("Temperature Settings")]
        [SerializeField] private float minTempThreshold = 69f;
        [SerializeField] private float maxTempThreshold = 71f;

        private void Start()
        {
           // uisysys = GetComponent<UISystemOfOBJ>();
            if (targetMaterial == null)
            {
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
            // 检查温度是否在 69-71 范围内
            if (temperature >= minTempThreshold && temperature <= maxTempThreshold)
            {
                if (Firstadded)
                {
                   
                    ScoreSystem.Instance.AddScore(1);
                     uisysys.TriggerWin();
                    Firstadded = false;
                   // Debug.Log($"Score added! Temperature: {temperature}");
                }
            }

            // 颜色插值计算
            float t = (temperature - 60f) / (90f - 60f);
            targetMaterial.color = Color.Lerp(coldColor, hotColor, t);
        }

        private void OnDestroy()
        {
            var tempController = GetComponent<TemperatureControllerTransformer>();
            if (tempController != null)
            {
                tempController.OnTemperatureChanged.RemoveListener(UpdateColor);
            }
        }
    }
}
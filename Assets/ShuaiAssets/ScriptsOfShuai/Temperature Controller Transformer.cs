using UnityEngine;
using UnityEngine.Events;
using TMPro;

namespace CustomInteraction
{
    public class TemperatureControllerTransformer : MonoBehaviour
    {
        [Header("Display Settings")]
        [SerializeField]
        private TextMeshProUGUI temperatureText;    // 温度显示文本
       

        [Header("Temperature Settings")]
        [SerializeField]
        private float _maxTemperature = 90f;        // 最高温度（华氏度）
        [SerializeField]
        private float _minTemperature = 60f;        // 最低温度（华氏度）

        // 温度改变时的事件
        public UnityEvent<float> OnTemperatureChanged;

        private float _initialZRotation;            // 初始Z轴旋转值
        private float _currentTemperature;          // 当前温度

        private void Start()
        {
            // 初始化事件
            if (OnTemperatureChanged == null)
            {
                OnTemperatureChanged = new UnityEvent<float>();
            }

            // 记录初始旋转值
            _initialZRotation = transform.localEulerAngles.z;

            // 确保有温度显示文本组件
            if (temperatureText == null)
            {
                Debug.LogError("Temperature Text component is not assigned!");
            }

            // 初始化温度显示
            UpdateTemperatureFromRotation();
        }

        private void Update()
        {
            UpdateTemperatureFromRotation();
        }

        // 根据旋转更新温度
        private void UpdateTemperatureFromRotation()
        {
            // 获取当前Z轴旋转
            float currentZRotation = transform.localEulerAngles.z;

            // 计算相对于初始位置的旋转差值（确保在0-90度范围内）
            float rotationDelta = Mathf.DeltaAngle(currentZRotation, _initialZRotation);
            float normalizedRotation = Mathf.Clamp(rotationDelta, 0f, 90f);

            // 将0-90度的旋转映射到90-60华氏度（注意是反向映射）
            float newTemperature = Mathf.Lerp(_maxTemperature, _minTemperature, normalizedRotation / 90f);
           

            // 如果温度发生变化，更新显示并触发事件
            if (Mathf.Abs(newTemperature - _currentTemperature) > 0.01f)
            {
                _currentTemperature = newTemperature;
                UpdateTemperatureDisplay();
                OnTemperatureChanged.Invoke(_currentTemperature);
            }
        }

        // 更新温度显示
        private void UpdateTemperatureDisplay()
        {
            if (temperatureText != null)
            {
                temperatureText.text = $"{_currentTemperature:F1}°F";
            }
        }

        // 获取当前温度
        public float GetCurrentTemperature()
        {
            return _currentTemperature;
        }

        // 获取摄氏度值
        public float GetTemperatureCelsius()
        {
            return (_currentTemperature - 32f) * 5f / 9f;
        }
    }
}
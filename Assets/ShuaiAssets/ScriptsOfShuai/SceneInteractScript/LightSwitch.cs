using UnityEngine;

public class LightSwitch : MonoBehaviour
{
    public GameObject lightOn;
    public GameObject lightOff;
    private bool isOn = true;
    private float cooldownTime = 1f;
    private float lastToggleTime = 0f;
    private bool isFirstTimeOff = true;
    public UISystemOfOBJ uisysys;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateLightState();
    }

    // Update is called once per frame
    void Update()
    {
        // 更新灯光状态
        UpdateLightState();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hand"))
        {
            ToggleLight();
        }
    }

    private void ToggleLight()
    {
        // 检查是否在冷却时间内
        if (Time.time - lastToggleTime >= cooldownTime)
        {
            bool previousState = isOn;
            isOn = !isOn;
            lastToggleTime = Time.time;

            // 检查是否是第一次从开灯状态切换到关灯状态
            if (previousState && !isOn && isFirstTimeOff)
            {
                isFirstTimeOff = false;
                ScoreSystem.Instance.AddScore(1);
                uisysys.TriggerWin();
            }
        }
    }

    private void UpdateLightState()
    {
        if (lightOn != null && lightOff != null)
        {
            lightOn.SetActive(isOn);
            lightOff.SetActive(!isOn);
        }
    }
}

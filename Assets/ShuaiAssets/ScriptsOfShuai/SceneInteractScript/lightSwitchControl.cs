using UnityEngine;

public class lightSwitchControl : MonoBehaviour
{
    public GameObject SwitchTrigger;
    private float originalStartValue;

    [SerializeField]
    private bool isSwitched = false;
     [SerializeField]
    private float switchSpeed = 5f;
     [SerializeField]
    private float cooldownTime = 1f;
     [SerializeField]
    private float switchAngle = 60f;
    
    private float lastSwitchTime = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (SwitchTrigger != null)
        {
            originalStartValue = SwitchTrigger.transform.localRotation.eulerAngles.z;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (SwitchTrigger != null && isSwitched)
        {
            float targetRotation = originalStartValue + switchAngle;
            Quaternion targetQuaternion = Quaternion.Euler(0, 0, targetRotation);
            SwitchTrigger.transform.localRotation = Quaternion.Lerp(
                SwitchTrigger.transform.localRotation,
                targetQuaternion,
                Time.deltaTime * switchSpeed
            );
        }
        else if (SwitchTrigger != null && !isSwitched)
        {
            Quaternion targetQuaternion = Quaternion.Euler(0, 0, originalStartValue);
            SwitchTrigger.transform.localRotation = Quaternion.Lerp(
                SwitchTrigger.transform.localRotation,
                targetQuaternion,
                Time.deltaTime * switchSpeed
            );
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hand") && Time.time - lastSwitchTime >= cooldownTime)
        {
            isSwitched = !isSwitched;
            lastSwitchTime = Time.time;
        }
    }
}

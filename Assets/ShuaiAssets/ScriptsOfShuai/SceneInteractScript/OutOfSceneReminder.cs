using UnityEngine;

public class OutOfSceneReminder : MonoBehaviour
{
    public GameObject reminder;
    public GameObject playerBlack;
    private float cooldownTime = 0.5f;
    private float lastCheckTime;
    private bool isInCooldown;
    private bool isHeadInTrigger = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (reminder == null || playerBlack == null)
        {
            Debug.LogError("Reminder or PlayerBlack reference is missing!");
            return;
        }
        lastCheckTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (isInCooldown)
        {
            if (Time.time - lastCheckTime >= cooldownTime)
            {
                isInCooldown = false;
                UpdateReminderState();
            }
            return;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Head"))
        {
            isHeadInTrigger = true;
            UpdateReminderState();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Head"))
        {
            isHeadInTrigger = false;
            UpdateReminderState();
        }
    }

    private void UpdateReminderState()
    {
        if (!isHeadInTrigger)
        {
            reminder.SetActive(true);
            playerBlack.SetActive(true);
        }
        else
        {
            reminder.SetActive(false);
            playerBlack.SetActive(false);
        }

        lastCheckTime = Time.time;
        isInCooldown = true;
    }
}

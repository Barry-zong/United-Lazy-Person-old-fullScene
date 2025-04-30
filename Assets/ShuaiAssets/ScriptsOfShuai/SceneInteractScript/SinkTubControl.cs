using UnityEngine;

public class SinkTubControl : MonoBehaviour
{
    public GameObject tubWaterFace;
    public GameObject tubWaterPole;
    public AudioSource tubWaterAudio;
    public UISystemOfOBJ uisysys;
    
    private float originalFaceY;
    private float originalPoleY;
    private float originalVolume;
    private bool isFirstTrigger = true;
    private bool isWaterDown = false;
    private float cooldownTime = 4f;
    private float lastTriggerTime = 0f;
    private float faceTransitionTime = 3f;
    private float poleTransitionTime = 1f;
    private float audioTransitionTime = 1f;
    private float faceTransitionTimer = 0f;
    private float poleTransitionTimer = 0f;
    private float audioTransitionTimer = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 记录初始位置和音量
        if (tubWaterFace != null)
            originalFaceY = tubWaterFace.transform.position.y;
        if (tubWaterPole != null)
            originalPoleY = tubWaterPole.transform.position.y;
        if (tubWaterAudio != null)
            originalVolume = tubWaterAudio.volume;
    }

    // Update is called once per frame
    void Update()
    {
        // 处理水面下降（关水）
        if (isWaterDown)
        {
            faceTransitionTimer += Time.deltaTime;
            poleTransitionTimer += Time.deltaTime;
            audioTransitionTimer += Time.deltaTime;

            // 水面下降
            if (tubWaterFace != null)
            {
                float progress = Mathf.Clamp01(faceTransitionTimer / faceTransitionTime);
                Vector3 newPos = tubWaterFace.transform.position;
                newPos.y = Mathf.Lerp(originalFaceY, 0, progress);
                tubWaterFace.transform.position = newPos;
            }

            // 水柱下降
            if (tubWaterPole != null)
            {
                float progress = Mathf.Clamp01(poleTransitionTimer / poleTransitionTime);
                Vector3 newPos = tubWaterPole.transform.position;
                newPos.y = Mathf.Lerp(originalPoleY, 0, progress);
                tubWaterPole.transform.position = newPos;
            }

            // 音量降低
            if (tubWaterAudio != null)
            {
                float progress = Mathf.Clamp01(audioTransitionTimer / audioTransitionTime);
                tubWaterAudio.volume = Mathf.Lerp(originalVolume, 0, progress);
            }
        }
        // 处理水面上升（放水）
        else
        {
            faceTransitionTimer += Time.deltaTime;
            poleTransitionTimer += Time.deltaTime;
            audioTransitionTimer += Time.deltaTime;

            // 水面上升
            if (tubWaterFace != null)
            {
                float progress = Mathf.Clamp01(faceTransitionTimer / faceTransitionTime);
                Vector3 newPos = tubWaterFace.transform.position;
                newPos.y = Mathf.Lerp(0, originalFaceY, progress);
                tubWaterFace.transform.position = newPos;
            }

            // 水柱上升
            if (tubWaterPole != null)
            {
                float progress = Mathf.Clamp01(poleTransitionTimer / poleTransitionTime);
                Vector3 newPos = tubWaterPole.transform.position;
                newPos.y = Mathf.Lerp(0, originalPoleY, progress);
                tubWaterPole.transform.position = newPos;
            }

            // 音量恢复
            if (tubWaterAudio != null)
            {
                float progress = Mathf.Clamp01(audioTransitionTimer / audioTransitionTime);
                tubWaterAudio.volume = Mathf.Lerp(0, originalVolume, progress);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hand") && Time.time - lastTriggerTime >= cooldownTime)
        {
            lastTriggerTime = Time.time;
            isWaterDown = !isWaterDown; // 切换状态
            faceTransitionTimer = 0f;
            poleTransitionTimer = 0f;
            audioTransitionTimer = 0f;

            if (isWaterDown && isFirstTrigger) // 只有在关水状态且是第一次触发时才加分
            {
                ScoreSystem.Instance.AddScore(1);
                uisysys.TriggerWin();
                isFirstTrigger = false;
            }
        }
    }
}

using UnityEngine;

public class DrinkWater : MonoBehaviour
{
    private AudioSource audioSource;
    private MeshRenderer meshRenderer;
    private bool hasEntered = false;
    private float timer = 0f;
    private bool isPlaying = false;
   public UISystemOfOBJ uisysys;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        meshRenderer = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isPlaying)
        {
            timer += Time.deltaTime;
            if (timer >= 3f)
            {
                meshRenderer.enabled = false;
                isPlaying = false;
                 ScoreSystem.Instance.AddScore(1);
                 uisysys.TriggerWin();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Head") && !hasEntered)
        {
            hasEntered = true;
            isPlaying = true;
            timer = 0f;
            if (audioSource != null)
            {
                audioSource.Play();
            }
        }
    }
}

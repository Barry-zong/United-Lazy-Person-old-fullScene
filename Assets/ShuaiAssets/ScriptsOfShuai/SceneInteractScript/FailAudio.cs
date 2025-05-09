using UnityEngine;

public class FailAudio : MonoBehaviour
{
    public AudioSource failAudioSource;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (failAudioSource == null)
        {
            failAudioSource = GetComponent<AudioSource>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayFailAudio()
    {
        if (failAudioSource != null)
        {
            failAudioSource.Play();
        }
    }

    private void OnEnable()
    {
        PlayFailAudio();
    }
}

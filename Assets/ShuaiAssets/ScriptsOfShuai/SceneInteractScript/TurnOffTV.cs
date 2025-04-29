using UnityEngine;
using UnityEngine.Video;

public class TurnOffTV : MonoBehaviour
{
    public GameObject videoPlayer;
    public UISystemOfOBJ uisysys;
    private bool hasBeenTurnedOff = false;

    void Update()
    {
        if (CountdownTimer.Instance != null && CountdownTimer.Instance.IsTimerFinished)
        {
            videoPlayer.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hand"))
        {
            videoPlayer.SetActive(false);
            if (!hasBeenTurnedOff)
            {
                ScoreSystem.Instance.AddScore(1);
                uisysys.TriggerWin();
                hasBeenTurnedOff = true;
            }
        }
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class EndingEvent : MonoBehaviour
{
    public GameObject endingUI;
    public GameObject endingUI2;
    public float restartDelay = 60f; // 游戏结束后多久重启场景
    private bool hasEnded = false;
    private float endTime;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        endingUI.SetActive(false);
        endingUI2.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (CountdownTimer.Instance.IsTimerFinished && !hasEnded)
        {
            hasEnded = true;
            endTime = Time.time;
            endingUI.SetActive(true);
            endingUI2.SetActive(true);
            // 禁用加分系统
            if (ScoreSystem.Instance != null)
            {
                ScoreSystem.Instance.SetCanAddScore(false);
            }
        }

        if (hasEnded && Time.time - endTime >= restartDelay)
        {
            RestartScene();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (CountdownTimer.Instance.IsTimerFinished && other.CompareTag("Finish"))
        {
            RestartScene();
        }
    }

    private void RestartScene()
    {
        // 完全重启游戏，加载第一个场景
        SceneManager.LoadScene(0);
    }
}

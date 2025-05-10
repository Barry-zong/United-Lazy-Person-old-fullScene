using UnityEngine;

public class candleFireAdd : MonoBehaviour
{
    public GameObject candleFire;
    public UISystemOfOBJ uisysys;
    private bool isFirstTrigger = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hand") && isFirstTrigger)
        {
            ScoreSystem.Instance.AddScore(1);
            uisysys.TriggerWin();
            candleFire.SetActive(false);
            isFirstTrigger = false;
        }
    }
}

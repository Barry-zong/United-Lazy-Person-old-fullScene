using UnityEngine;

public class TestScoreAdded : MonoBehaviour
{
    private bool canAddScore = true;

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
        if (other.CompareTag("Hand") && canAddScore)
        {
            ScoreSystem.Instance.AddScore(1);
            canAddScore = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Hand"))
        {
            canAddScore = true;
        }
    }
}

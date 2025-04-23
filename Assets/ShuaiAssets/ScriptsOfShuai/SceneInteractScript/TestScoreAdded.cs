using UnityEngine;

public class TestScoreAdded : MonoBehaviour
{
    public enum TriggerAction
    {
        AddScore,
        EndGame
    }

    [SerializeField] private TriggerAction actionType = TriggerAction.AddScore;
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
            if (actionType == TriggerAction.AddScore)
            {
                ScoreSystem.Instance.AddScore(1);
            }
            else if (actionType == TriggerAction.EndGame)
            {
                CountdownTimer.Instance.SetRemainingTime(3f);
            }
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

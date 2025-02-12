using UnityEngine;

public class PointAdded : MonoBehaviour
{
    private bool Firstadded = true;
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
        if(Firstadded)
        {
            ScoreSystem.Instance.AddScore(1);
            Firstadded= false;
        }
       
    }
}

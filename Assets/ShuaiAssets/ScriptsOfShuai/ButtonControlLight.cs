using UnityEngine;

public class ButtonControlLight : MonoBehaviour
{
    private Collider DetectBox;
    public GameObject Light;
    private bool Firstadded = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DetectBox = GetComponent<Collider>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        Light.SetActive(false);
        if (Firstadded)
        {
          //  Debug.Log("scoreadded");
                
           ScoreSystem.Instance.AddScore(1);
            Firstadded = false;
        }
    }
}

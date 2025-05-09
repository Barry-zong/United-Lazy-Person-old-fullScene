using UnityEngine;

public class PointAdded : MonoBehaviour
{
    public UISystemOfOBJ uisysys;
    public bool good = true;
    public bool isHand = false;
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
        if(!Firstadded) return;  // 如果不是第一次触发，直接返回

        if(other.CompareTag("Food"))
        {
            if(good)
            {
                ScoreSystem.Instance.AddScore(1);
                uisysys.TriggerWin();
            }
            else
            {
                // 触发失败状态
                Debug.Log("触发失败状态");  
                uisysys.TriggerFail();
            }
            Firstadded = false;
        }

        if(isHand && other.CompareTag("Hand"))
        {
            ScoreSystem.Instance.AddScore(1);
            uisysys.TriggerWin();
            Firstadded = false;
        }
    }
}

using UnityEngine;

public class ExitTurnOffTutorio : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Head"))
        {
            // 设置游戏状态为Playing
            GameStateCenter.Instance.SetGameState(GameState.Playing);
        }
    }
}

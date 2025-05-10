using UnityEngine;

public class buttonStartGame : MonoBehaviour
{
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
        if (other.CompareTag("Hand"))
        {
            // 设置游戏状态为Playing
            GameStateCenter.Instance.SetGameState(GameState.Playing);
        }
    }
}

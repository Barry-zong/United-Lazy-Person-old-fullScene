using UnityEngine;

public class controllerStartGame : MonoBehaviour
{
    [Header("OVR Buttons")]
    [SerializeField] private OVRInput.RawButton startButton = OVRInput.RawButton.Y;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (OVRInput.GetDown(startButton))
        {
            Debug.Log("Start Button Pressed - Starting Game");
            GameStateCenter.Instance.SetGameState(GameState.Playing);
        }  
    }
}

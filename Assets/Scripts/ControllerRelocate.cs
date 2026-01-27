using UnityEngine;

public class ControllerRelocate : MonoBehaviour
{
    [Header("OVR Buttons")]
    [SerializeField] private OVRInput.RawButton relocateButton = OVRInput.RawButton.Start;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (OVRInput.GetDown(relocateButton))
        {
            Debug.Log("Relocate Button Pressed - Relocating Player");
            RelocatePlayer();
        }
    }

    void RelocatePlayer()
    {
        // Implement player relocation logic here
    }
}

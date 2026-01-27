using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class ControllerSceneManager : MonoBehaviour
{
    public static ControllerSceneManager Instance { get; private set; }

    [Header("Scene names must match exactly what's in Build Settings")]
    [SerializeField] private string sceneX = "ShuaiAssets/LazyPplKidsDaySmallScene_1";
    [SerializeField] private string sceneY = "ShuaiAssets/LazyPplKidsDaySmallScene_2";

    [Header("OVR Buttons")]
    [SerializeField] private OVRInput.RawButton scene1Button = OVRInput.RawButton.X;
    [SerializeField] private OVRInput.RawButton scene2Button = OVRInput.RawButton.Y;

    [Header("Optional")]
    [SerializeField] private LoadSceneMode loadMode = LoadSceneMode.Single;

    private void Awake()
    {
        // Singleton guard
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (OVRInput.GetDown(scene1Button))
            Load(sceneX);

        if (OVRInput.GetDown(scene2Button))
            Load(sceneY);
    }

    private void Load(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("Scene name is empty.");
            return;
        }

        SceneManager.LoadScene(sceneName, loadMode);
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

public class ControllerSceneManager : MonoBehaviour
{
    public static ControllerSceneManager Instance { get; private set; }

    [Header("Scene names must match exactly what's in Build Settings")]
    [SerializeField] private string scene1 = "ShuaiAssets/LazyPplKidsDaySmallScene_1";
    [SerializeField] private string scene2 = "ShuaiAssets/LazyPplKidsDaySmallScene_2";

    [Header("OVR Buttons")]
    [SerializeField] private OVRInput.RawButton sceneButton = OVRInput.RawButton.X;

    [Header("Optional")]
    [SerializeField] private LoadSceneMode loadMode = LoadSceneMode.Single;

    private int currentSceneIndex = 0;

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
        if (OVRInput.GetDown(sceneButton))
        {
            currentSceneIndex = 1 - currentSceneIndex; // Toggle between 0 and 1
            if (currentSceneIndex == 0)
                Load(scene1);
            else
                Load(scene2);
        }
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

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class ControllerSceneLoader : MonoBehaviour
{
    public static ControllerSceneLoader Instance { get; private set; }

    [Header("Scene names must match exactly what's in Build Settings")]
    [SerializeField] private string sceneX = "ShuaiAssets/LazyPplKidsDaySmallScene_1";
    [SerializeField] private string sceneY = "ShuaiAssets/LazyPplKidsDaySmallScene_2";

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
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.xKey.wasPressedThisFrame)
            Load(sceneX);

        if (Keyboard.current.yKey.wasPressedThisFrame)
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

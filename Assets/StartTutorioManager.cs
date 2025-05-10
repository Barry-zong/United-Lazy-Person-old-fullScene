using UnityEngine;
using System.Collections;

public class StartTutorioManager : MonoBehaviour
{
    public GameObject tutorialUI1;
    public GameObject tutorialUI2;
    public GameObject tutorialUI3;
    public GameObject tutorialUI4;
    public GameObject tutorialUI5;
    
    private bool hasTriggered = false;
    private int currentTutorialIndex = 0;
    private float cooldownTime = 2f;
    private float transitionTime = 1.5f;
    private bool isInTransition = false;
    private float lastTriggerTime = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 注册游戏状态改变事件
        GameStateCenter.Instance.OnGameStateChanged += HandleGameStateChanged;
        InitializeUIState();
    }

    void OnDestroy()
    {
        // 取消注册事件
        if (GameStateCenter.Instance != null)
        {
            GameStateCenter.Instance.OnGameStateChanged -= HandleGameStateChanged;
        }
    }

    void HandleGameStateChanged(GameState newState)
    {
        if (newState == GameState.Loaded)
        {
            currentTutorialIndex = 0;
            ResetAllUI();
        }
    }

    private void InitializeUIState()
    {
        // 初始化所有UI的scale为0
        tutorialUI2.transform.localScale = Vector3.zero;
        tutorialUI3.transform.localScale = Vector3.zero;
        tutorialUI4.transform.localScale = Vector3.zero;
        tutorialUI5.transform.localScale = Vector3.zero;
        
        // 设置tutorialUI1的scale为1
        tutorialUI1.transform.localScale = Vector3.one;
        
        // 设置初始UI状态
        tutorialUI1.SetActive(true);
        tutorialUI2.SetActive(false);
        tutorialUI3.SetActive(false);
        tutorialUI4.SetActive(false);
        tutorialUI5.SetActive(false);
    }

    private void SetAllUIScale(Vector3 scale)
    {
        tutorialUI1.transform.localScale = scale;
        tutorialUI2.transform.localScale = scale;
        tutorialUI3.transform.localScale = scale;
        tutorialUI4.transform.localScale = scale;
        tutorialUI5.transform.localScale = scale;
    }

    private void ResetAllUI()
    {
        SetAllUIScale(Vector3.zero);
        tutorialUI1.SetActive(true);
        tutorialUI2.SetActive(false);
        tutorialUI3.SetActive(false);
        tutorialUI4.SetActive(false);
        tutorialUI5.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hand") && !isInTransition && Time.time - lastTriggerTime >= cooldownTime)
        {
            lastTriggerTime = Time.time;
            GameStateCenter.Instance.SetGameState(GameState.TutorialStart);
            StartCoroutine(TransitionToNextTutorial());
        }
    }

    private IEnumerator TransitionToNextTutorial()
    {
        isInTransition = true;
        GameObject currentUI = GetCurrentUI();
        GameObject nextUI = GetNextUI();

        if (currentUI != null)
        {
            // 关闭当前UI
            float elapsedTime = 0f;
            while (elapsedTime < transitionTime)
            {
                elapsedTime += Time.deltaTime;
                float scale = Mathf.Lerp(1f, 0f, elapsedTime / transitionTime);
                currentUI.transform.localScale = new Vector3(scale, scale, scale);
                yield return null;
            }
            currentUI.SetActive(false);
        }

        if (nextUI != null)
        {
            // 开启下一个UI
            nextUI.SetActive(true);
            float elapsedTime = 0f;
            while (elapsedTime < transitionTime)
            {
                elapsedTime += Time.deltaTime;
                float scale = Mathf.Lerp(0f, 1f, elapsedTime / transitionTime);
                nextUI.transform.localScale = new Vector3(scale, scale, scale);
                yield return null;
            }
        }
        else
        {
            // 如果已经是最后一个UI，设置游戏状态为Playing
            GameStateCenter.Instance.SetGameState(GameState.Playing);
        }

        isInTransition = false;
    }

    private GameObject GetCurrentUI()
    {
        switch (currentTutorialIndex)
        {
            case 0: return tutorialUI1;
            case 1: return tutorialUI2;
            case 2: return tutorialUI3;
            case 3: return tutorialUI4;
            case 4: return tutorialUI5;
            default: return null;
        }
    }

    private GameObject GetNextUI()
    {
        currentTutorialIndex++;
        switch (currentTutorialIndex)
        {
            case 1: return tutorialUI2;
            case 2: return tutorialUI3;
            case 3: return tutorialUI4;
            case 4: return tutorialUI5;
            default: return null;
        }
    }
}

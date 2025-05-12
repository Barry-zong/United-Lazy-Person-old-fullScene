using UnityEngine;
using System.Collections;

public class StartTutorioManager : MonoBehaviour
{
    public GameObject RemindText;
    public GameObject tutorialUI1;
    public GameObject tutorialUI2;
    public GameObject tutorialUI3;
    public GameObject tutorialUI4;
    public GameObject tutorialUI5;
    
    [Header("教程步骤之间的自动触发时间（秒）")]
    [Tooltip("第一个到第二个UI的等待时间")]
    [SerializeField] private float firstToSecondTime = 5f;
    [Tooltip("第二个到第三个UI的等待时间")]
    [SerializeField] private float secondToThirdTime = 30f;
    [Tooltip("第三个到第四个UI的等待时间")]
    [SerializeField] private float thirdToFourthTime = 20f;
    
    private bool hasTriggered = false;
    private bool isFirstTrigger = true;
    private int currentTutorialIndex = 0;
    private float cooldownTime = 4f;
    private float transitionTime = 1.5f;
    private bool isInTransition = false;
    private float lastTriggerTime = 0f;
    private Vector3 initialRemindTextScale;
    private Coroutine remindTextCoroutine;
    private Coroutine autoTriggerCoroutine;
    private GameObject currentActiveUI;  // 当前活动的UI对象

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 注册游戏状态改变事件
        GameStateCenter.Instance.OnGameStateChanged += HandleGameStateChanged;
        InitializeUIState();
        
        // 记录RemindText的初始scale
        if (RemindText != null)
        {
            initialRemindTextScale = RemindText.transform.localScale;
        }
    }

    void OnDestroy()
    {
        // 取消注册事件
        if (GameStateCenter.Instance != null)
        {
            GameStateCenter.Instance.OnGameStateChanged -= HandleGameStateChanged;
        }
        
        // 停止所有协程
        if (autoTriggerCoroutine != null)
        {
            StopCoroutine(autoTriggerCoroutine);
        }
        
        // 清理所有UI对象
        if (tutorialUI1 != null) Destroy(tutorialUI1);
        if (tutorialUI2 != null) Destroy(tutorialUI2);
        if (tutorialUI3 != null) Destroy(tutorialUI3);
        if (tutorialUI4 != null) Destroy(tutorialUI4);
        if (tutorialUI5 != null) Destroy(tutorialUI5);
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
        // 确保所有UI对象都是激活的，但scale为0
        tutorialUI1.SetActive(true);
        tutorialUI2.SetActive(true);
        tutorialUI3.SetActive(true);
        tutorialUI4.SetActive(true);
        tutorialUI5.SetActive(true);
        
        // 设置所有UI的scale为0
        tutorialUI1.transform.localScale = Vector3.zero;
        tutorialUI2.transform.localScale = Vector3.zero;
        tutorialUI3.transform.localScale = Vector3.zero;
        tutorialUI4.transform.localScale = Vector3.zero;
        tutorialUI5.transform.localScale = Vector3.zero;
        
        // 设置初始UI状态
        currentActiveUI = tutorialUI1;
        tutorialUI1.transform.localScale = Vector3.one;
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
        // 销毁所有非第一个UI
        if (tutorialUI2 != null) Destroy(tutorialUI2);
        if (tutorialUI3 != null) Destroy(tutorialUI3);
        if (tutorialUI4 != null) Destroy(tutorialUI4);
        if (tutorialUI5 != null) Destroy(tutorialUI5);
        
        // 重置第一个UI
        tutorialUI1.SetActive(true);
        tutorialUI1.transform.localScale = Vector3.one;
        currentActiveUI = tutorialUI1;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hand") && !isInTransition && (isFirstTrigger || Time.time - lastTriggerTime >= cooldownTime))
        {
            lastTriggerTime = Time.time;
            isFirstTrigger = false;  // 设置标志为false，表示已经不是第一次触发了
            GameStateCenter.Instance.SetGameState(GameState.TutorialStart);
            
            // 停止之前的自动触发协程（如果存在）
            if (autoTriggerCoroutine != null)
            {
                StopCoroutine(autoTriggerCoroutine);
            }
            
            // 分别启动两个独立的协程
            StartCoroutine(TransitionToNextTutorial());
            HandleRemindTextAnimation();
            
            // 启动新的自动触发协程
            autoTriggerCoroutine = StartCoroutine(AutoTriggerNextTutorial());
        }
    }

    private void HandleRemindTextAnimation()
    {
        // 停止之前的动画协程（如果有）
        if (remindTextCoroutine != null)
        {
            StopCoroutine(remindTextCoroutine);
        }
        
        // 开始新的动画协程
        remindTextCoroutine = StartCoroutine(AnimateRemindText());
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
            
            // 销毁当前UI
            if (currentUI != tutorialUI1)  // 保持第一个UI始终存在
            {
                Destroy(currentUI);
            }
        }

        if (nextUI != null)
        {
            // 确保下一个UI是激活的
            nextUI.SetActive(true);
            float elapsedTime = 0f;
            while (elapsedTime < transitionTime)
            {
                elapsedTime += Time.deltaTime;
                float scale = Mathf.Lerp(0f, 1f, elapsedTime / transitionTime);
                nextUI.transform.localScale = new Vector3(scale, scale, scale);
                yield return null;
            }
            currentActiveUI = nextUI;
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

    private IEnumerator AnimateRemindText()
    {
        if (RemindText == null) yield break;

        // 从当前scale过渡到0
        float elapsedTime = 0f;
        Vector3 startScale = RemindText.transform.localScale;
        
        while (elapsedTime < 2f)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / 2f;
            RemindText.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            yield return null;
        }

        // 等待冷却时间
        float waitTime = cooldownTime - 2f;
        while (waitTime > 0)
        {
            waitTime -= Time.deltaTime;
            yield return null;
        }

        // 从0过渡回初始scale
        elapsedTime = 0f;
        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / 1f;
            RemindText.transform.localScale = Vector3.Lerp(Vector3.zero, initialRemindTextScale, t);
            yield return null;
        }
    }

    private IEnumerator AutoTriggerNextTutorial()
    {
        // 获取当前步骤的自动触发时间
        float currentAutoTriggerTime = GetCurrentAutoTriggerTime();
        
        // 等待指定的自动触发时间
        yield return new WaitForSeconds(currentAutoTriggerTime);
        
        // 如果当前不在过渡状态，则自动触发下一次
        if (!isInTransition)
        {
            GameStateCenter.Instance.SetGameState(GameState.TutorialStart);
            StartCoroutine(TransitionToNextTutorial());
            HandleRemindTextAnimation();
            
            // 重新启动自动触发协程
            autoTriggerCoroutine = StartCoroutine(AutoTriggerNextTutorial());
        }
    }

    private float GetCurrentAutoTriggerTime()
    {
        switch (currentTutorialIndex)
        {
            case 0: return firstToSecondTime;
            case 1: return secondToThirdTime;
            case 2: return thirdToFourthTime;
            default: return 15f; // 默认返回15秒
        }
    }
}

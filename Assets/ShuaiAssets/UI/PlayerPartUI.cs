using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class PlayerPartUI : MonoBehaviour
{
    public GameObject _WinUI;
    public GameObject _FailUI;
    public float _autoCloseTime = 4f;  // UI自动关闭时间
    public float _animationDuration = 0.3f; // 动画持续时间
    private static PlayerPartUI _instance;
    public static PlayerPartUI Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<PlayerPartUI>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("PlayerPartUI");
                    _instance = obj.AddComponent<PlayerPartUI>();
                }
            }
            return _instance;
        }
    }

    [SerializeField] private TextMeshProUGUI _text1;
    [SerializeField] private TextMeshProUGUI _text2;
    [SerializeField] private TextMeshProUGUI _text3;
    [SerializeField] private TextMeshProUGUI _text4;
    [SerializeField] private TextMeshProUGUI _text5;

    private Dictionary<GameObject, Vector3> originalScales = new Dictionary<GameObject, Vector3>();

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (_WinUI != null)
        {
            _WinUI.SetActive(false);
            originalScales[_WinUI] = _WinUI.transform.localScale;
            _WinUI.transform.localScale = Vector3.zero;
        }
        if (_FailUI != null)
        {
            _FailUI.SetActive(false);
            originalScales[_FailUI] = _FailUI.transform.localScale;
            _FailUI.transform.localScale = Vector3.zero;
        }
    }

    public void ShowWinUI(string text1, string text2, string text3)
    {
        if (_WinUI != null)
        {
            _WinUI.SetActive(true);
            StartCoroutine(ScaleAnimation(_WinUI, true));
        }
        if (_FailUI != null) _FailUI.SetActive(false);

        if (_text1 != null) _text1.text = text1;
        if (_text2 != null) _text2.text = text2;
        if (_text3 != null) _text3.text = text3;

        StartCoroutine(AutoCloseUI(true));
    }

    public void ShowFailUI(string text4, string text5)
    {
        if (_WinUI != null) _WinUI.SetActive(false);
        if (_FailUI != null)
        {
            _FailUI.SetActive(true);
            StartCoroutine(ScaleAnimation(_FailUI, true));
        }

        if (_text4 != null) _text4.text = text4;
        if (_text5 != null) _text5.text = text5;

        StartCoroutine(AutoCloseUI(false));
    }

    private IEnumerator ScaleAnimation(GameObject uiObject, bool show)
    {
        float elapsedTime = 0f;
        Vector3 startScale = show ? Vector3.zero : originalScales[uiObject];
        Vector3 endScale = show ? originalScales[uiObject] : Vector3.zero;

        while (elapsedTime < _animationDuration)
        {
            uiObject.transform.localScale = Vector3.Lerp(startScale, endScale, elapsedTime / _animationDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        uiObject.transform.localScale = endScale;
        if (!show)
        {
            uiObject.SetActive(false);
        }
    }

    private IEnumerator AutoCloseUI(bool isWinUI)
    {
        yield return new WaitForSeconds(_autoCloseTime);
        
        if (isWinUI)
        {
            if (_WinUI != null)
            {
                StartCoroutine(ScaleAnimation(_WinUI, false));
            }
        }
        else
        {
            if (_FailUI != null)
            {
                StartCoroutine(ScaleAnimation(_FailUI, false));
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

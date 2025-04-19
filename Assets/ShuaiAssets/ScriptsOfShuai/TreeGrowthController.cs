using UnityEngine;
using System.Collections;

public class TreeGrowthController : MonoBehaviour
{
    [Header("测试设置")]
    [Tooltip("在Inspector中设置分数用于测试")]
    public int testScore = 0;

    [Header("生长设置")]
    [Tooltip("每次得分后允许生长的时长（秒）")]
    public float growthDuration = 1f;
    [Tooltip("第一次得分时的生长时长（秒）")]
    public float firstGrowthDuration = 3f;

    private Generator _treeGenerator;
    private float _lastScore;
    private Coroutine _growthCoroutine;
    private bool _isFirstScore = true;

    void Start()
    {
        // 获取组件引用
        _treeGenerator = GetComponent<Generator>();
        
        if (_treeGenerator == null)
        {
            Debug.LogError("TreeGrowthController: Generator component not found!");
            return;
        }
        
        if (ScoreSystem.Instance == null)
        {
            Debug.LogError("TreeGrowthController: ScoreSystem not found!");
            return;
        }

        // 初始化分数
        _lastScore = ScoreSystem.Instance.GetCurrentScore();
        
        // 初始状态为暂停
        _treeGenerator._pauseGrowth = true;
    }

    void Update()
    {
        // 检查分数变化
        float currentScore = ScoreSystem.Instance.GetCurrentScore();
        if (currentScore > _lastScore)
        {
            // 分数增加，触发生长
            if (_growthCoroutine != null)
            {
                StopCoroutine(_growthCoroutine);
            }
            _growthCoroutine = StartCoroutine(GrowthSequence(_isFirstScore));
            _lastScore = currentScore;
            _isFirstScore = false;
        }

        // 测试用：当testScore大于0时设置分数
        if (testScore > 0)
        {
            ScoreSystem.Instance.SetScore(testScore);
            testScore = 0; // 重置为0，避免重复设置
        }
    }

    private IEnumerator GrowthSequence(bool isFirstScore)
    {
        // 允许生长指定时间
        _treeGenerator._pauseGrowth = false;
        yield return new WaitForSeconds(isFirstScore ? firstGrowthDuration : growthDuration);
        
        // 暂停生长
        _treeGenerator._pauseGrowth = true;
        _growthCoroutine = null;
    }
} 
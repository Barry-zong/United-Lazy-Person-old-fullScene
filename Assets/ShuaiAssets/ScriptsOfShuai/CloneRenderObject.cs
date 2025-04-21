using UnityEngine;

public class CloneRenderObject : MonoBehaviour
{
    [Header("克隆设置")]
    public GameObject treePrefab; // 要克隆的Tree预制体
    [SerializeField] private Vector3 scale = Vector3.one; // 缩放值
    [SerializeField] private Vector3 rotation = Vector3.zero; // 旋转值
    [SerializeField] private float updateInterval = 0.1f; // 更新间隔

    [Header("生长控制")]
    [SerializeField] private float currentScore = 0f; // 当前分数
    [SerializeField] private float maxScore = 100f; // 最大分数
    [SerializeField] private float growthSpeed = 1f; // 生长速度
    [SerializeField] private bool autoGrow = false; // 是否自动生长

    private GameObject clonedTreeObject;
    private Generator originalGenerator;
    private Generator clonedGenerator;
    private float timer = 0f;
    private float lastScore = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CloneTree();
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
            UpdateClonedTree();
            timer = 0f;
        }

        // 如果分数发生变化，更新生长
        if (currentScore != lastScore)
        {
            UpdateGrowth();
            lastScore = currentScore;
        }

        // 自动生长模式
        if (autoGrow && clonedGenerator != null)
        {
            currentScore = Mathf.Min(currentScore + Time.deltaTime * growthSpeed, maxScore);
        }
    }

    private void CloneTree()
    {
        if (treePrefab != null)
        {
            // 如果已存在克隆体，先销毁
            if (clonedTreeObject != null)
            {
                Destroy(clonedTreeObject);
            }

            // 获取原始Generator组件
            originalGenerator = treePrefab.GetComponent<Generator>();
            if (originalGenerator == null)
            {
                Debug.LogError("原始物体没有Generator组件！");
                return;
            }

            // 创建新的游戏对象
            clonedTreeObject = new GameObject("ClonedTree");
            clonedTreeObject.transform.SetParent(transform);
            clonedTreeObject.transform.localPosition = Vector3.zero;
            clonedTreeObject.transform.localRotation = Quaternion.Euler(rotation);
            clonedTreeObject.transform.localScale = scale;

            // 添加Generator组件
            clonedGenerator = clonedTreeObject.AddComponent<Generator>();

            // 复制Generator的所有参数
            clonedGenerator._nbAttractors = originalGenerator._nbAttractors;
            clonedGenerator._radius = originalGenerator._radius;
            clonedGenerator._startPosition = originalGenerator._startPosition;
            clonedGenerator._branchLength = originalGenerator._branchLength;
            clonedGenerator._timeBetweenIterations = originalGenerator._timeBetweenIterations;
            clonedGenerator._attractionRange = originalGenerator._attractionRange;
            clonedGenerator._killRange = originalGenerator._killRange;
            clonedGenerator._randomGrowth = originalGenerator._randomGrowth;
            clonedGenerator._useRandomSeed = originalGenerator._useRandomSeed;
            clonedGenerator._randomSeed = originalGenerator._randomSeed;
            clonedGenerator._pauseGrowth = true; // 初始时暂停生长
            clonedGenerator._leafPrefabs = originalGenerator._leafPrefabs;
            clonedGenerator._leafSpawnProbability = originalGenerator._leafSpawnProbability;
            clonedGenerator._leafSize = originalGenerator._leafSize;
            clonedGenerator._leafRotationRandomness = originalGenerator._leafRotationRandomness;
            clonedGenerator._minDistanceFromRootForLeaves = originalGenerator._minDistanceFromRootForLeaves;
            clonedGenerator._radialSubdivisions = originalGenerator._radialSubdivisions;
            clonedGenerator._extremitiesSize = originalGenerator._extremitiesSize;
            clonedGenerator._invertGrowth = originalGenerator._invertGrowth;

            // 添加MeshFilter组件
            clonedTreeObject.AddComponent<MeshFilter>();

            // 初始化分数
            currentScore = 0f;
            lastScore = 0f;
        }
    }

    private void UpdateClonedTree()
    {
        if (clonedTreeObject != null)
        {
            // 更新克隆体的变换
            clonedTreeObject.transform.localScale = scale;
            clonedTreeObject.transform.localRotation = Quaternion.Euler(rotation);
        }
    }

    private void UpdateGrowth()
    {
        if (clonedGenerator != null)
        {
            // 根据分数比例控制生长
            float growthProgress = currentScore / maxScore;
            
            // 调整生长参数
            clonedGenerator._branchLength = originalGenerator._branchLength * growthProgress;
            clonedGenerator._radius = originalGenerator._radius * growthProgress;
            clonedGenerator._leafSize = originalGenerator._leafSize * growthProgress;
            
            // 如果分数达到一定阈值，允许生长
            if (growthProgress > 0.1f)
            {
                clonedGenerator._pauseGrowth = false;
            }
            else
            {
                clonedGenerator._pauseGrowth = true;
            }
        }
    }

    // 外部调用设置分数
    public void SetScore(float score)
    {
        currentScore = Mathf.Clamp(score, 0f, maxScore);
    }

    // 外部调用增加分数
    public void AddScore(float amount)
    {
        currentScore = Mathf.Clamp(currentScore + amount, 0f, maxScore);
    }

    // 外部调用减少分数
    public void SubtractScore(float amount)
    {
        currentScore = Mathf.Clamp(currentScore - amount, 0f, maxScore);
    }
}

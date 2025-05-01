using UnityEngine;
using TMPro;

public class PointConvert : MonoBehaviour
{
    public enum ConvertType
    {
        月度电,  // 1分等于1/2的文本数量
        碳排放,  // 1分等于2*分数的文本数量
        种树     // 1分等于3*分数的文本数量
    }

    [SerializeField] private ConvertType currentType = ConvertType.月度电;
    [SerializeField] private TextMeshProUGUI targetText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (targetText == null)
        {
            targetText = GetComponent<TextMeshProUGUI>();
            if (targetText == null)
            {
                Debug.LogError("PointConvert: No TextMeshProUGUI component found!");
                return;
            }
        }
        UpdateText();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateText();
    }

    private void UpdateText()
    {
        if (ScoreSystem.Instance == null || targetText == null) return;

        int currentScore = ScoreSystem.Instance.GetCurrentScore();
        int convertedValue = 0;

        switch (currentType)
        {
            case ConvertType.月度电:
                convertedValue = (int)(0.8f * currentScore);
                break;
            case ConvertType.碳排放:
                convertedValue = 45 * currentScore ;
                break;
            case ConvertType.种树:
                convertedValue = (int)(1.8f * currentScore);
                break;
        }

        targetText.text = convertedValue.ToString();
    }
}

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class CanvasBehavior : MonoBehaviour
{
    private Object obj;

    [SerializeField] private Image background;
    [SerializeField] private List<Color> colorList;
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private GameObject completePanel;
    [SerializeField] private TextMeshProUGUI objectName;
    [SerializeField] private TextMeshProUGUI hint;
    [SerializeField] private TextMeshProUGUI explanation;

    void Start()
    {
        infoPanel.SetActive(false);
        completePanel.SetActive(false);
    }

    public void SetCanvas(Object scriptableObject)
    {
        obj = scriptableObject;

        background.color = colorList[obj.level - 1];
        objectName.text = obj.Name;
        hint.text = obj.hint;
        explanation.text = obj.explantion;
    }

    public void ShowInfoPanel()
    {
        infoPanel.SetActive(true);
        completePanel.SetActive(false);
    }

    public void ShowCompletePanel()
    {
        infoPanel.SetActive(false);
        completePanel.SetActive(true);
    }
}

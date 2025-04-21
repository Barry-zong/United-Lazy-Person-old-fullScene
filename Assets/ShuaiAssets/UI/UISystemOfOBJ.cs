using UnityEngine;
using System.Collections;

/// <summary>
/// UI系统控制类，用于管理提示文本的显示
/// </summary>
public class UISystemOfOBJ : MonoBehaviour
{
    #region 文本内容字段
    [TextArea(2, 50)]
    [SerializeField] private string lookingBack;  // 提示回头
    [TextArea(2, 50)]
    [SerializeField] private string congratulationContent;  // 恭喜回头提示词
    [TextArea(2, 50)]
    [SerializeField] private string sourceDetailsContent;   // 来源详情
    [TextArea(2, 50)]
    [SerializeField] private string failureReminderContent; // 失败提醒
    [TextArea(2, 50)]
    [SerializeField] private string reasonHintContent;      // 原因提示
    [SerializeField] private int ObjSort = 0;      // 分类

    [SerializeField] private GameObject needCloseOBJ;       // 需要关闭的游戏对象
    #endregion

    #region 属性访问器
    public string LookingBack => lookingBack;
    public string CongratulationContent => congratulationContent;
    public string SourceDetailsContent => sourceDetailsContent;
    public string FailureReminderContent => failureReminderContent;
    public string ReasonHintContent => reasonHintContent;
    #endregion

    #region 公共方法
    /// <summary>
    /// 设置成功状态下的文本内容并显示成功UI
    /// </summary>
    public void SetSuccessCondition(string congratulation, string sourceDetails)
    {
        congratulationContent = congratulation;
        sourceDetailsContent = sourceDetails;
        ClearFailureTexts();
        ShowWinUI();
    }

    /// <summary>
    /// 设置失败状态下的文本内容并显示失败UI
    /// </summary>
    public void SetFailureCondition(string failureReminder, string reasonHint)
    {
        failureReminderContent = failureReminder;
        reasonHintContent = reasonHint;
        ClearSuccessTexts();
        ShowFailUI();
    }

    /// <summary>
    /// 显示成功UI
    /// </summary>
    public void ShowWinUI()
    {
        if (PlayerPartUI.Instance != null)
        {
            PlayerPartUI.Instance.ShowWinUI(lookingBack, congratulationContent, sourceDetailsContent);
            if (HandSortAchieveSystem.Instance != null && ObjSort != 0)
            {
                HandSortAchieveSystem.Instance.ActivateAchieve(ObjSort);
            }
            if (needCloseOBJ != null) StartCoroutine(ScaleDownAndDisable(needCloseOBJ));
        }
    }

    /// <summary>
    /// 显示失败UI
    /// </summary>
    public void ShowFailUI()
    {
        if (PlayerPartUI.Instance != null)
        {
            PlayerPartUI.Instance.ShowFailUI(failureReminderContent, reasonHintContent);
        }
    }

    /// <summary>
    /// 重置所有文本内容为空
    /// </summary>
    public void ResetAllTexts()
    {
        lookingBack = string.Empty;
        congratulationContent = string.Empty;
        sourceDetailsContent = string.Empty;
        failureReminderContent = string.Empty;
        reasonHintContent = string.Empty;
    }

    /// <summary>
    /// 外部触发成功状态
    /// </summary>
    public void TriggerWin()
    {
        ShowWinUI();
    }

    /// <summary>
    /// 外部触发失败状态
    /// </summary>
    public void TriggerFail()
    {
        ShowFailUI();
    }
    #endregion

    #region 私有辅助方法
    private void ClearSuccessTexts()
    {
        congratulationContent = string.Empty;
        sourceDetailsContent = string.Empty;
    }

    private void ClearFailureTexts()
    {
        failureReminderContent = string.Empty;
        reasonHintContent = string.Empty;
    }

    private IEnumerator ScaleDownAndDisable(GameObject obj)
    {
        float duration = 0.5f;
        float elapsedTime = 0f;
        Vector3 originalScale = obj.transform.localScale;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            obj.transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);
            yield return null;
        }

        obj.SetActive(false);
        obj.transform.localScale = originalScale; // 重置缩放，以便下次使用
    }
    #endregion
}

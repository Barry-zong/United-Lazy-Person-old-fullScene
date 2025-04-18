using UnityEngine;

[CreateAssetMenu(fileName = "ObjectScriptable", menuName = "Scriptable Objects/ObjectScriptable")]
public class Object : ScriptableObject
{
    public bool grabable;    // �Ƿ����ȡ
    public string Name;     // ����
    public int level;       // ����

    [TextArea(1, 2)]
    public string hint;     // һ������ʾ

    [TextArea(3, 10)]
    public string explantion;       // ���Ϲ�������һ���ֵ�����

}

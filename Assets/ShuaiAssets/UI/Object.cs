using UnityEngine;

[CreateAssetMenu(fileName = "ObjectScriptable", menuName = "Scriptable Objects/ObjectScriptable")]
public class Object : ScriptableObject
{
    public bool grabable;    // �Ƿ����ȡ
    public string Name;     // ����
    public int level;       // ����
    public string hint;     // һ������ʾ
    public string explantion;       // ���Ϲ�������һ���ֵ�����

}

using UnityEngine;

[CreateAssetMenu(fileName = "ObjectScriptable", menuName = "Scriptable Objects/ObjectScriptable")]
public class Object : ScriptableObject
{
    public bool grabable;    // 是否可拿取
    public string Name;     // 名称
    public int level;       // 属性
    public string hint;     // 一部分提示
    public string explantion;       // 联合国具体哪一部分的内容

}

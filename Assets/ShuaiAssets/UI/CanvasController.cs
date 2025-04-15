using UnityEngine;

public class CanvasController : MonoBehaviour
{
    public CanvasBehavior canvas;
    public Object obj;

    void Start()
    {
        canvas.SetCanvas(obj);
    }
}

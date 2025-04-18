using UnityEngine;

public class Throw : MonoBehaviour
{
    public CanvasBehavior canvas;
    public Object obj;

    void Start()
    {
        canvas.SetCanvas(obj);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hand"))
        {
            canvas.gameObject.SetActive(true);
            canvas.ShowInfoPanel();
        }

        if (other.CompareTag("bin"))
        {
            canvas.ShowCompletePanel();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Hand"))
        {
            canvas.gameObject.SetActive(false);
        }


    }
}

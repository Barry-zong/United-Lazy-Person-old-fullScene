using UnityEngine;
public class UiObjHandInteract : MonoBehaviour
{
    public GameObject UIpartObj;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UIpartObj.SetActive(false);
    }
    // Update is called once per frame
    void Update()
    {

    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hand"))
        {
            UIpartObj.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Hand"))
        {
            UIpartObj.SetActive(false);
        }
    }
}
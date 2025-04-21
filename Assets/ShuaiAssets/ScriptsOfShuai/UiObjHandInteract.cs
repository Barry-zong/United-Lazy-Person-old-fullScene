using UnityEngine;
using System.Collections;

public class UiObjHandInteract : MonoBehaviour
{
    public GameObject UIpartObj;
    public float animationDuration = 0.3f;
    private Vector3 originalScale;
    private Coroutine scaleCoroutine;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        originalScale = UIpartObj.transform.localScale;
        UIpartObj.SetActive(false);
        UIpartObj.transform.localScale = Vector3.zero;
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
            if (scaleCoroutine != null)
            {
                StopCoroutine(scaleCoroutine);
            }
            scaleCoroutine = StartCoroutine(ScaleObject(Vector3.zero, originalScale));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Hand"))
        {
            if (scaleCoroutine != null)
            {
                StopCoroutine(scaleCoroutine);
            }
            scaleCoroutine = StartCoroutine(ScaleObject(originalScale, Vector3.zero, true));
        }
    }

    private IEnumerator ScaleObject(Vector3 startScale, Vector3 endScale, bool disableAfterAnimation = false)
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < animationDuration)
        {
            UIpartObj.transform.localScale = Vector3.Lerp(startScale, endScale, elapsedTime / animationDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        UIpartObj.transform.localScale = endScale;
        
        if (disableAfterAnimation)
        {
            UIpartObj.SetActive(false);
        }
    }
}
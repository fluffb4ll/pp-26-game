using UnityEngine;
using UnityEngine.UI;     
using System.Collections;

public class NotificationUI : MonoBehaviour
{
    public UnityEngine.UI.Text messageText;   
    public float displayDuration = 2f;

    void Start()
    {
        if (messageText == null)
            messageText = GetComponent<UnityEngine.UI.Text>();
        gameObject.SetActive(false);
    }

    public void ShowMessage(string message)
    {
        StopAllCoroutines();
        messageText.text = message;
        gameObject.SetActive(true);
        StartCoroutine(HideAfterTime());
    }

    IEnumerator HideAfterTime()
    {
        yield return new WaitForSeconds(displayDuration);
        gameObject.SetActive(false);
    }
}
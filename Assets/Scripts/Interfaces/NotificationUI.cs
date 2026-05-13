using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using YG;

public class NotificationUI : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Text messageText;
    [SerializeField] private float displayDuration = 2f;

    private void Start()
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

    private IEnumerator HideAfterTime()
    {
        yield return new WaitForSeconds(displayDuration);
        gameObject.SetActive(false);
    }
}
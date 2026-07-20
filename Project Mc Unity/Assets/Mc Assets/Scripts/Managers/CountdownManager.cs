using System;
using System.Collections;
using UnityEngine;
using TMPro;

public class CountdownManager : MonoBehaviour
{
    public static event Action OnCountdownFinished;

    [SerializeField] private TextMeshProUGUI countdownText;

    public void StartCountdown()
    {
        countdownText.gameObject.SetActive(true);
        StartCoroutine(CountdownRoutine());
    }

    private IEnumerator CountdownRoutine()
    {
        int count = 3;
        while (count > 0)
        {
            countdownText.text = count.ToString();
            yield return new WaitForSeconds(1f);
            count--;
        }

        countdownText.text = "BAŞLA!";
        OnCountdownFinished?.Invoke();

        yield return new WaitForSeconds(1f);
        countdownText.gameObject.SetActive(false);
    }
}
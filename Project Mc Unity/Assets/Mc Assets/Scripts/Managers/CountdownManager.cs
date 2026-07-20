using System;
using System.Collections;
using UnityEngine;
using TMPro;

public class CountdownManager : MonoBehaviour
{
    // Geri sayım bittiğinde GameManager'ı ve diğer sistemleri uyaracak event 
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
        OnCountdownFinished?.Invoke(); // Event fırlatıldı!

        yield return new WaitForSeconds(1f);
        countdownText.gameObject.SetActive(false); // Ekranda yer kaplamaması için kapatıyoruz
    }
}
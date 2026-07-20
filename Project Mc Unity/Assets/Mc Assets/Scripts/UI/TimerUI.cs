using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TimerUI : MonoBehaviour
{
    [Tooltip("Sahnedeki RaceTimer (Beyin) objesini buraya sürükle")]
    [SerializeField] private RaceTimer raceTimer;
    
    private TextMeshProUGUI timeText;

    private void Awake()
    {
        timeText = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        // Sadece yarış devam ediyorsa UI güncellenir.
        if (raceTimer != null && raceTimer.IsRunning)
        {
            DisplayTime(raceTimer.CurrentTime);
        }
    }

    private void DisplayTime(float timeInSeconds)
    {
        // Sektör standardı optimum zaman ayrıştırması
        int minutes = Mathf.FloorToInt(timeInSeconds / 60F);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60F);
        int milliseconds = Mathf.FloorToInt((timeInSeconds * 1000F) % 1000F);

        // String.Format, arka arkaya " " + " " yapmaktan (String allocation) çok daha performanslıdır.
        timeText.text = string.Format("{0:00}:{1:00}.{2:000}", minutes, seconds, milliseconds);
    }
}
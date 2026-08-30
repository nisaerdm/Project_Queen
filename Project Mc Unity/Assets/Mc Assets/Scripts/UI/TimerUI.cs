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
        if (raceTimer != null && raceTimer.IsRunning && timeText != null)
        {
            DisplayTime(raceTimer.CurrentTime);
        }
    }

    private void DisplayTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60F);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60F);
        int milliseconds = Mathf.FloorToInt((timeInSeconds * 1000F) % 1000F);

        // OPTİMİZASYON: string.Format bile mobilde saniyede 60 kez çağrılırsa çöp(GC) yaratır.
        // TMP'nin SetText'i, string oluşturmadan sayıyı doğrudan ekrana gömer. 0 çöp üretir!
        timeText.SetText("{0:00}:{1:00}.{2:000}", minutes, seconds, milliseconds);
    }
}
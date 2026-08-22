using UnityEngine;
using TMPro;

public class LapUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI lapText;

    // CheckpointManager referansına artık gerek kalmadı, PlayerPrefs kullanacağız
    // [SerializeField] private CheckpointManager checkpointManager; 

    private void OnEnable()
    {
        CheckpointManager.OnLapCompleted += HandleLapCompleted;
    }

    private void OnDisable()
    {
        CheckpointManager.OnLapCompleted -= HandleLapCompleted;
    }

    private void Start()
    {
        // ÇÖZÜM: Scriptlerin çalışma sırası (Race Condition) çakışmasını engellemek için
        // Toplam turu doğrudan hafızadan kendimiz okuyoruz
        int totalLaps = PlayerPrefs.GetInt("Race_Laps", 1);
        lapText.text = $"Tur 1/{totalLaps}";
    }

    private void HandleLapCompleted(Transform carTransform, int completedLaps)
    {
        if (carTransform.GetComponent<F1PlayerInput>() != null)
        {
            UpdateUI(completedLaps);
        }
    }

    private void UpdateUI(int completedLaps)
    {
        int currentDisplayLap = completedLaps + 1;
        int totalLaps = PlayerPrefs.GetInt("Race_Laps", 1);

        currentDisplayLap = Mathf.Clamp(currentDisplayLap, 1, totalLaps);
        lapText.text = $"Tur {currentDisplayLap}/{totalLaps}";
    }
}
using UnityEngine;
using TMPro;

public class LapUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI lapText;

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
        if (lapText != null)
        {
            int totalLaps = PlayerPrefs.GetInt("Race_Laps", 1);
            lapText.text = $"1/{totalLaps}";
        }
    }

    private void HandleLapCompleted(Transform carTransform, int completedLaps)
    {
        if (carTransform.CompareTag("Player"))
        {
            UpdateUI(completedLaps);
        }
    }

    private void UpdateUI(int completedLaps)
    {
        if (lapText == null) return;

        int totalLaps = PlayerPrefs.GetInt("Race_Laps", 1);
        int currentDisplayLap = completedLaps + 1;

        currentDisplayLap = Mathf.Clamp(currentDisplayLap, 1, totalLaps);
        lapText.text = $"Tur {currentDisplayLap}/{totalLaps}";
    }
}
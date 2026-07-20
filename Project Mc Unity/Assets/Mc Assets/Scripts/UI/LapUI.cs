using UnityEngine;
using TMPro;

public class LapUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI lapText;
    [SerializeField] private CheckpointManager checkpointManager;

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
        // Yarış başladığında ekranda "Tur 1/X" yazması için manuel tetikliyoruz
        UpdateUI(0);
    }

    private void HandleLapCompleted(Transform carTransform, int completedLaps)
    {
        // KONTROL: Turu geçen araç oyuncu mu?
        // Oyuncunun aracında F1PlayerInput olduğunu bildiğimiz için buradan filtreliyoruz.
        if (carTransform.GetComponent<F1PlayerInput>() != null)
        {
            UpdateUI(completedLaps);
        }
    }

    private void UpdateUI(int completedLaps)
    {
        // completedLaps 0 ise 1. turdayız demektir.
        int currentDisplayLap = completedLaps + 1;

        // Eğer yarış bittiyse (örneğin 3/3 tamsa), UI'ın "Tur 4/3" yazmasını engelliyoruz
        currentDisplayLap = Mathf.Clamp(currentDisplayLap, 1, checkpointManager.TotalLaps);

        lapText.text = $"Tur {currentDisplayLap}/{checkpointManager.TotalLaps}";
    }
}
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    [Header("UI Panelleri")]
    [Tooltip("Yarış bittiğinde açılacak olan ana panel (Arkaplan, yazılar vs. içerir)")]
    [SerializeField] private GameObject gameOverPanel;

    [Header("Yazı Referansları")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI rankText;

    [Header("Süre Referansları (Lego Modülü)")]
    [SerializeField] private TextMeshProUGUI currentTimeText;
    [SerializeField] private TextMeshProUGUI bestTimeText;

    [Tooltip("Yarış süresini tutan ana sayaç scriptimiz")]
    [SerializeField] private RaceTimer raceTimer;

    private RacePlacementManager placementManager;

    private void OnEnable()
    {
        CheckpointManager.OnRaceFinished += HandleRaceFinished;
    }

    private void OnDisable()
    {
        CheckpointManager.OnRaceFinished -= HandleRaceFinished;
    }

    private void Start()
    {
        placementManager = Object.FindFirstObjectByType<RacePlacementManager>();

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    private void HandleRaceFinished(Transform carTransform, bool isPlayer)
    {
        if (isPlayer)
        {
            ShowGameOverScreen(carTransform);
        }
    }

    private void ShowGameOverScreen(Transform playerTransform)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (titleText != null) titleText.text = "YARIŞ TAMAMLANDI!";
        }

        PlacementUI placementUI = Object.FindFirstObjectByType<PlacementUI>();
        if (placementUI != null && rankText != null)
        {
            TextMeshProUGUI livePlacementText = placementUI.GetComponentInChildren<TextMeshProUGUI>();
            if (livePlacementText != null)
            {
                rankText.text = $"Sıralamanız: {livePlacementText.text}";
            }
        }

        if (raceTimer != null)
        {
            float finalTime = raceTimer.GetCurrentTime();
            string currentLevelName = SceneManager.GetActiveScene().name;
            string saveKey = "BestTime_" + currentLevelName;

            float bestTime = PlayerPrefs.GetFloat(saveKey, Mathf.Infinity);

            if (finalTime < bestTime)
            {
                bestTime = finalTime;
                PlayerPrefs.SetFloat(saveKey, bestTime);
                PlayerPrefs.Save();
            }

            if (currentTimeText != null) currentTimeText.text = $"Süreniz: {FormatTime(finalTime)}";
            if (bestTimeText != null) bestTimeText.text = $"En İyi Süre: {FormatTime(bestTime)}";
        }
    }

    private string FormatTime(float time)
    {
        int minutes = (int)(time / 60f);
        int seconds = (int)(time % 60f);
        int fraction = (int)((time * 100f) % 100f);
        return string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, fraction);
    }
}
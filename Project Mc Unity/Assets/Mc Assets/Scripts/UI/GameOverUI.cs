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
    [Tooltip("Yarışın güncel süresini ekrana basacağımız UI yazısı")]
    [SerializeField] private TextMeshProUGUI currentTimeText;
    [Tooltip("En iyi süreyi ekrana basacağımız UI yazısı")]
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
        // Bitiş çizgisine gelen araç YAPAY ZEKA ise paneli açma, sadece OYUNCU bitirdiğinde aç!
        if (isPlayer)
        {
            ShowGameOverScreen(carTransform);
        }
    }

    private void ShowGameOverScreen(Transform playerTransform)
    {
        // 1. Paneli Görünür Yap
        gameOverPanel.SetActive(true);
        titleText.text = "YARIŞ TAMAMLANDI!";

        // 2. Sıralamayı Çek
        PlacementUI placementUI = Object.FindFirstObjectByType<PlacementUI>();
        if (placementUI != null)
        {
            TextMeshProUGUI livePlacementText = placementUI.GetComponentInChildren<TextMeshProUGUI>();
            if (livePlacementText != null)
            {
                rankText.text = $"Sıralamanız: {livePlacementText.text}";
            }
        }

        // 3. Süreyi Hesapla ve Kaydet
        if (raceTimer != null)
        {
            // RaceTimer içinden o anki güncel süreyi float cinsinden alıyoruz
            float finalTime = raceTimer.GetCurrentTime();

            // Rekoru sadece bulunduğumuz haritaya özel kaydetmek için sahne adını çekiyoruz
            string currentLevelName = SceneManager.GetActiveScene().name;
            string saveKey = "BestTime_" + currentLevelName;

            // Önceden kaydedilmiş rekoru oku (Yoksa çok büyük bir sayı döner ki ilk rekor kolay kırılsın)
            float bestTime = PlayerPrefs.GetFloat(saveKey, Mathf.Infinity);

            // Yeni süre rekordan daha kısaysa, yeni rekoru cihaza kaydet
            if (finalTime < bestTime)
            {
                bestTime = finalTime;
                PlayerPrefs.SetFloat(saveKey, bestTime);
                PlayerPrefs.Save();
            }

            // Ekrana formatlanmış şekilde bas
            if (currentTimeText != null) currentTimeText.text = $"Süreniz: {FormatTime(finalTime)}";
            if (bestTimeText != null) bestTimeText.text = $"En İyi Süre: {FormatTime(bestTime)}";
        }
        else
        {
            Debug.LogWarning("[GameOverUI] HATA: RaceTimer referansı atanmamış!");
        }
    }

    /// <summary>
    /// Float saniye cinsinden gelen süreyi "Dakika:Saniye:Salise" formatına dönüştürür
    /// </summary>
    private string FormatTime(float time)
    {
        int minutes = (int)(time / 60f);
        int seconds = (int)(time % 60f);
        int fraction = (int)((time * 100f) % 100f);
        return string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, fraction);
    }
}
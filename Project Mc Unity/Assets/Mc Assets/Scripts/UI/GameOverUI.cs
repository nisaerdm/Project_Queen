using UnityEngine;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    [Header("UI Panelleri")]
    [Tooltip("Yarış bittiğinde açılacak olan ana panel (Arkaplan, yazılar vs. içerir)")]
    [SerializeField] private GameObject gameOverPanel;

    [Header("Yazı Referansları")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI rankText;

    private RacePlacementManager placementManager;

    private void OnEnable()
    {
        // CheckpointManager'dan gelen Bitiş sinyaline abone ol
        CheckpointManager.OnRaceFinished += HandleRaceFinished;
    }

    private void OnDisable()
    {
        CheckpointManager.OnRaceFinished -= HandleRaceFinished;
    }

    private void Start()
    {
        placementManager = Object.FindFirstObjectByType<RacePlacementManager>();

        // Oyun başında paneli kapalı tut
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

        // 2. Ekrana "Yarış Bitti" yaz
        titleText.text = "YARIŞ TAMAMLANDI!";

        // 3. Oyuncunun final sıralamasını Placement Text'ten (veya UI'dan) bağımsız olarak hesapla ve bas
        // (Bunu bulmak için PlacementUI içindeki TextMeshPro objesini okumak yerine veriyi baştan alıyoruz ki Lego yapısı bozulmasın)
        PlacementUI placementUI = Object.FindFirstObjectByType<PlacementUI>();
        if (placementUI != null)
        {
            // PlacementUI'daki o anki Text neyse (Örn: "1 / 4") aynısını Bitiş ekranına kopyala
            TextMeshProUGUI livePlacementText = placementUI.GetComponentInChildren<TextMeshProUGUI>();
            if (livePlacementText != null)
            {
                rankText.text = $"Sıralamanız: {livePlacementText.text}";
            }
        }
    }
}
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlacementUI : MonoBehaviour
{
    [Header("UI Referansları")]
    [SerializeField] private TextMeshProUGUI placementText;

    private Transform playerCarRoot;

    private void OnEnable()
    {
        RacePlacementManager.OnPlacementUpdated += UpdatePlacementUI;
    }

    private void OnDisable()
    {
        RacePlacementManager.OnPlacementUpdated -= UpdatePlacementUI;
    }

    private void UpdatePlacementUI(List<Transform> currentStandings)
    {
        // 1. Oyuncu aracı henüz bulunamadıysa, listeyi tarayıp bul
        if (playerCarRoot == null)
        {
            // OPTİMİZASYON: Tüm sahneyi aramak yerine sadece bize gelen kısa listeyi (yarışan araçları) tarıyoruz
            foreach (Transform car in currentStandings)
            {
                if (car != null && car.CompareTag("Player"))
                {
                    playerCarRoot = car;
                    break;
                }
            }

            // Hala bulamadıysa (araç spawn olmadıysa) çık
            if (playerCarRoot == null) return;
        }

        // 2. Sıralamayı Hesapla
        int playerRank = currentStandings.IndexOf(playerCarRoot) + 1;

        if (playerRank > 0 && placementText != null)
        {
            placementText.text = $"{playerRank} / {currentStandings.Count}";
        }
    }
}
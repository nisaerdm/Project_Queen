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
        // 1. Oyuncu aracı henüz bulunamadıysa bulmayı dene
        if (playerCarRoot == null)
        {
            F1PlayerInput playerInput = UnityEngine.Object.FindFirstObjectByType<F1PlayerInput>(FindObjectsInactive.Include);

            if (playerInput != null)
            {
                // ÇÖZÜM: Listenin içindekiyle eşleşmesi için transform.root'u alıyoruz
                playerCarRoot = playerInput.transform.root;
            }
            else
            {
                return;
            }
        }

        // 2. Sıralamayı Hesapla
        int playerRank = currentStandings.IndexOf(playerCarRoot) + 1;

        if (playerRank > 0)
        {
            placementText.text = $"{playerRank} / {currentStandings.Count}";
        }
    }
}
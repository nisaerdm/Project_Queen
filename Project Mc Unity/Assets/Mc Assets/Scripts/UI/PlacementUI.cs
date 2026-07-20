using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlacementUI : MonoBehaviour
{
    [Header("UI Referansları")]
    [SerializeField] private TextMeshProUGUI placementText;

    private Transform playerCar;

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
        // 1. Oyuncu aracı henüz bulunamadıysa (veya sonradan spawn olduysa) bulmayı dene
        if (playerCar == null)
        {
            // Araç veya bileşen geçici olarak inaktif (kapalı) olsa bile bulur
            F1PlayerInput playerInput = UnityEngine.Object.FindFirstObjectByType<F1PlayerInput>(FindObjectsInactive.Include);

            if (playerInput != null)
            {
                playerCar = playerInput.transform;
            }
            else
            {
                return; // Hala yoksa sessizce bekle, bir sonraki sefere tekrar dener.
            }
        }

        // 2. Sıralamayı Hesapla
        int playerRank = currentStandings.IndexOf(playerCar) + 1;

        if (playerRank > 0)
        {
            placementText.text = $"{playerRank} / {currentStandings.Count}";
        }
    }
}
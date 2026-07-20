using UnityEngine;
using UnityEngine.InputSystem;

public class StartingGridInputLocker : MonoBehaviour
{
    [Header("Kilitlenecek Sistemler")]
    [Tooltip("Aracın üzerindeki Player Input bileşeni")]
    [SerializeField] private PlayerInput playerInput;

    [Tooltip("Arcade Vehicle Physics assetinin ana hareket scriptini buraya sürükle")]
    [SerializeField] private MonoBehaviour carMovementScript;

    private void Awake()
    {
        // Araç sahneye doğduğu (Spawn) an sistemleri kilitleriz
        if (playerInput != null)
            playerInput.DeactivateInput();

        // Aracın kendi fizik/hareket kodunu da kapatıyoruz ki asset kendi kendine gaz vermesin
        if (carMovementScript != null)
            carMovementScript.enabled = false;
    }

    private void OnEnable()
    {
        // Geri sayım bittiğinde tetiklenecek event'e abone ol
        CountdownManager.OnCountdownFinished += UnlockVehicle;
    }

    private void OnDisable()
    {
        // Hafıza sızıntısını önlemek için aboneliği iptal et
        CountdownManager.OnCountdownFinished -= UnlockVehicle;
    }

    private void UnlockVehicle()
    {
        // 3-2-1 BAŞLA! dendiğinde zincirleri kırıyoruz
        if (playerInput != null)
            playerInput.ActivateInput();

        if (carMovementScript != null)
            carMovementScript.enabled = true;
    }
}
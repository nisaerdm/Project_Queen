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
        if (playerInput != null)
            playerInput.DeactivateInput();

        if (carMovementScript != null)
            carMovementScript.enabled = false;
    }

    private void OnEnable()
    {
        CountdownManager.OnCountdownFinished += UnlockVehicle;
    }

    private void OnDisable()
    {
        CountdownManager.OnCountdownFinished -= UnlockVehicle;
    }

    private void UnlockVehicle()
    {
        if (playerInput != null)
            playerInput.ActivateInput();

        if (carMovementScript != null)
            carMovementScript.enabled = true;
    }
}
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class RespawnController : MonoBehaviour
{
    [Header("Hayalet Modu (Ghost) Ayarları")]
    [Tooltip("Aracın hayalet modunda kalacağı süre (saniye)")]
    [SerializeField] private float ghostDuration = 3f;
    [Tooltip("Yanıp sönme hızı")]
    [SerializeField] private float blinkRate = 0.15f;

    private Rigidbody rb;
    private CheckpointManager checkpointManager;
    private Renderer[] allRenderers;
    private int originalLayer;
    private bool isRespawning = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        allRenderers = GetComponentsInChildren<Renderer>();
        checkpointManager = Object.FindFirstObjectByType<CheckpointManager>();
        originalLayer = gameObject.layer;
    }

    public void OnRespawn(InputAction.CallbackContext context)
    {
        if (context.performed && !isRespawning)
        {
            StartCoroutine(RespawnSequence());
        }
    }

    private IEnumerator RespawnSequence()
    {
        if (checkpointManager == null || checkpointManager.GetLastPassedCheckpoint(transform) == null)
        {
            Debug.LogError("[RespawnController] Checkpoint veya Manager bulunamadı!");
            yield break;
        }

        isRespawning = true;
        Transform targetPoint = checkpointManager.GetLastPassedCheckpoint(transform);

        // YENİ: Yere saplanmayı veya map altına düşmeyi engellemek için hafif havadan bırakıyoruz
        Vector3 safePosition = targetPoint.position + (Vector3.up * 1.5f);

        rb.isKinematic = true;

        // 1. Ana objeyi ışınla
        transform.position = safePosition;
        transform.rotation = targetPoint.rotation;

        // 2. BUG FIX (ArcadeVP Asıl Fizik Küresi): 
        // Aracın kendi kasası ışınlansa bile görünmez fizik küresi düştüğü yerde kalıyordu!
        ArcadeVP.ArcadeVehicleController carController = GetComponent<ArcadeVP.ArcadeVehicleController>();
        if (carController != null && carController.carBody != null)
        {
            carController.carBody.isKinematic = true; // Kürenin de fiziğini dondur

            // Küreyi de kasayla aynı yere ışınla
            carController.carBody.transform.position = safePosition;
            carController.carBody.transform.rotation = targetPoint.rotation;

            carController.carBody.isKinematic = false;
            carController.carBody.linearVelocity = Vector3.zero;
            carController.carBody.angularVelocity = Vector3.zero;
        }

        rb.isKinematic = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        yield return StartCoroutine(GhostEffectRoutine());

        isRespawning = false;
    }

    public void ForceRespawn()
    {
        if (!isRespawning)
        {
            StartCoroutine(RespawnSequence());
        }
    }

    private IEnumerator GhostEffectRoutine()
    {
        int ghostLayer = LayerMask.NameToLayer("Ghost");
        if (ghostLayer != -1) gameObject.layer = ghostLayer;

        float timer = 0f;
        bool isVisible = true;

        while (timer < ghostDuration)
        {
            isVisible = !isVisible;
            foreach (Renderer r in allRenderers) r.enabled = isVisible;
            yield return new WaitForSeconds(blinkRate);
            timer += blinkRate;
        }

        foreach (Renderer r in allRenderers) r.enabled = true;
        gameObject.layer = originalLayer;
    }
}
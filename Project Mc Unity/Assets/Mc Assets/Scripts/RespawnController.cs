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
            // Debug.LogError("[RespawnController] Checkpoint veya Manager bulunamadı!");
            yield break;
        }

        isRespawning = true;
        Transform targetPoint = checkpointManager.GetLastPassedCheckpoint(transform);
        Vector3 safePosition = targetPoint.position + (Vector3.up * 1.5f);

        ArcadeVP.ArcadeVehicleController carController = GetComponent<ArcadeVP.ArcadeVehicleController>();

        rb.isKinematic = true;
        if (carController != null && carController.carBody != null)
        {
            carController.carBody.isKinematic = true;
        }

        yield return new WaitForFixedUpdate();

        transform.position = safePosition;
        transform.rotation = targetPoint.rotation;

        if (carController != null && carController.carBody != null)
        {
            carController.carBody.transform.position = safePosition;
            carController.carBody.transform.rotation = targetPoint.rotation;
        }

        yield return new WaitForFixedUpdate();

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = false;

        if (carController != null && carController.carBody != null)
        {
            carController.carBody.linearVelocity = Vector3.zero;
            carController.carBody.angularVelocity = Vector3.zero;
            carController.carBody.isKinematic = false;
        }

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

            // OPTİMİZASYON KORUMASI: Araba parçalandıysa veya silindiyse hata verme
            foreach (Renderer r in allRenderers)
            {
                if (r != null) r.enabled = isVisible;
            }

            yield return new WaitForSeconds(blinkRate);
            timer += blinkRate;
        }

        foreach (Renderer r in allRenderers)
        {
            if (r != null) r.enabled = true;
        }

        gameObject.layer = originalLayer;
    }
}
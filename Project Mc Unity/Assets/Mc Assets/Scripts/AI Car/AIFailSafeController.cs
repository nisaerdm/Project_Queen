using System.Collections;
using UnityEngine;

[RequireComponent(typeof(RespawnController))]
public class AIFailSafeController : MonoBehaviour
{
    [Header("Fail-Safe Ayarları")]
    [Tooltip("Aracın bir sonraki checkpoint'e ulaşması için verilen maksimum süre (saniye).")]
    [SerializeField] private float maxTimeWithoutCheckpoint = 12f;

    private RespawnController respawnController;
    private MonoBehaviour aiBrain;
    private CheckpointManager checkpointManager;

    private int lastCheckpointIndex = -1;
    private float timeWithoutProgress = 0f;
    private Coroutine failSafeRoutine;

    private void Awake()
    {
        respawnController = GetComponent<RespawnController>();
        aiBrain = GetComponent("F1AIBrain") as MonoBehaviour;
        checkpointManager = Object.FindFirstObjectByType<CheckpointManager>();
    }

    private void OnEnable()
    {
        CountdownManager.OnCountdownFinished += StartMonitoring;
    }

    private void OnDisable()
    {
        CountdownManager.OnCountdownFinished -= StartMonitoring;
        StopMonitoring();
    }

    public void StartMonitoring()
    {
        if (checkpointManager == null) return; // Optimizasyon: Yoksa hiç başlama

        lastCheckpointIndex = checkpointManager.GetCarNextCheckpointIndex(transform);
        timeWithoutProgress = 0f;

        if (failSafeRoutine != null) StopCoroutine(failSafeRoutine);
        failSafeRoutine = StartCoroutine(StuckCheckRoutine());
    }

    public void StopMonitoring()
    {
        if (failSafeRoutine != null)
        {
            StopCoroutine(failSafeRoutine);
            failSafeRoutine = null;
        }
    }

    private IEnumerator StuckCheckRoutine()
    {
        // Sonsuz döngüden önce bekleme objesini yaratıp çöpten kurtuluyoruz (GC Alloc)
        WaitForSeconds waitOneSec = new WaitForSeconds(1f);

        while (true)
        {
            yield return waitOneSec;

            if (aiBrain != null && !aiBrain.enabled) continue;

            int currentTargetIndex = checkpointManager.GetCarNextCheckpointIndex(transform);

            if (currentTargetIndex != lastCheckpointIndex)
            {
                lastCheckpointIndex = currentTargetIndex;
                timeWithoutProgress = 0f;
            }
            else
            {
                timeWithoutProgress += 1f;

                if (timeWithoutProgress >= maxTimeWithoutCheckpoint)
                {
                    StartCoroutine(ExecuteFailSafeSequence());
                }
            }
        }
    }

    private IEnumerator ExecuteFailSafeSequence()
    {
        // Debug.Log'u mobil performans için yoruma alıyoruz. Çok işlemci yer!
        // Debug.Log($"[AIFailSafe] {gameObject.name} ışınlanıyor...");

        if (aiBrain != null) aiBrain.enabled = false;

        respawnController.ForceRespawn();

        yield return new WaitForSeconds(0.5f);

        if (aiBrain != null) aiBrain.enabled = true;

        timeWithoutProgress = 0f;
        if (checkpointManager != null)
        {
            lastCheckpointIndex = checkpointManager.GetCarNextCheckpointIndex(transform);
        }
    }
}
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(RespawnController))]
public class AIFailSafeController : MonoBehaviour
{
    [Header("Fail-Safe Ayarları")]
    [SerializeField] private float checkInterval = 2f;
    [SerializeField] private float minimumTravelDistance = 1.5f;
    [SerializeField] private int maxStuckCount = 3;

    private RespawnController respawnController;
    private Vector3 lastRecordedPosition;
    private int currentStuckCount = 0;
    private Coroutine failSafeRoutine;

    private void Awake()
    {
        respawnController = GetComponent<RespawnController>();
    }

    private void OnEnable()
    {
        // Fail-Safe SADECE geri sayım bittikten sonra tetiklenir
        CountdownManager.OnCountdownFinished += StartMonitoring;
    }

    private void OnDisable()
    {
        CountdownManager.OnCountdownFinished -= StartMonitoring;
        StopMonitoring();
    }

    public void StartMonitoring()
    {
        lastRecordedPosition = transform.position;
        currentStuckCount = 0;

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
        while (true)
        {
            yield return new WaitForSeconds(checkInterval);

            float distanceTravelled = Vector3.Distance(transform.position, lastRecordedPosition);

            if (distanceTravelled < minimumTravelDistance)
            {
                currentStuckCount++;
                if (currentStuckCount >= maxStuckCount) ExecuteFailSafe();
            }
            else
            {
                currentStuckCount = 0;
            }

            lastRecordedPosition = transform.position;
        }
    }

    private void ExecuteFailSafe()
    {
        Debug.Log($"[AIFailSafe] {gameObject.name} takılı kaldı! AI ışınlanıyor...");
        respawnController.ForceRespawn();
        currentStuckCount = 0;
        lastRecordedPosition = transform.position;
    }
}
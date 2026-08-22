using System.Collections;
using UnityEngine;

[RequireComponent(typeof(RespawnController))]
public class AIFailSafeController : MonoBehaviour
{
    [Header("Fail-Safe Ayarları")]
    [Tooltip("Aracın bir sonraki checkpoint'e ulaşması için verilen maksimum süre (saniye). Bu süre aşılırsa araç takılmış/düşmüş sayılır.")]
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
        if (checkpointManager != null)
        {
            lastCheckpointIndex = checkpointManager.GetCarNextCheckpointIndex(transform);
        }
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
        while (true)
        {
            // Performansı korumak için her frame yerine saniyede bir kontrol ediyoruz
            yield return new WaitForSeconds(1f);

            // Eğer araç zaten ışınlanıyorsa (beyni kapalıysa) süreyi sayma, pas geç
            if (aiBrain != null && !aiBrain.enabled) continue;

            if (checkpointManager == null) continue;

            // Aracın gitmesi gereken sıradaki Checkpoint'i soruyoruz
            int currentTargetIndex = checkpointManager.GetCarNextCheckpointIndex(transform);

            // Eğer hedef checkpoint değiştiyse (yani başarıyla bir sonrakine geçtiyse) süreyi sıfırla
            if (currentTargetIndex != lastCheckpointIndex)
            {
                lastCheckpointIndex = currentTargetIndex;
                timeWithoutProgress = 0f;
            }
            else
            {
                // Hedef değişmediyse kronomereyi 1 saniye artır
                timeWithoutProgress += 1f;

                // Eğer limit aşıldıysa, ameliyat sürecini başlat
                if (timeWithoutProgress >= maxTimeWithoutCheckpoint)
                {
                    StartCoroutine(ExecuteFailSafeSequence());
                }
            }
        }
    }

    private IEnumerator ExecuteFailSafeSequence()
    {
        Debug.Log($"[AIFailSafe] {gameObject.name} {maxTimeWithoutCheckpoint} saniyedir checkpoint geçemedi (Düşmüş olabilir). Işınlanıyor...");

        // 1. AI beynini geçici olarak uyut
        if (aiBrain != null) aiBrain.enabled = false;

        // 2. Işınlamayı tetikle
        respawnController.ForceRespawn();

        // 3. Işınlanma işleminin ve fiziklerin (isKinematic) tamamlanması için bekle
        yield return new WaitForSeconds(0.5f);

        // 4. Araç güvenli bir şekilde piste oturduğunda AI beynini tekrar uyandır
        if (aiBrain != null) aiBrain.enabled = true;

        // 5. Işınlandıktan sonra süreleri ve hedefleri sıfırla ki hemen tekrar ışınlanmasın
        timeWithoutProgress = 0f;
        if (checkpointManager != null)
        {
            lastCheckpointIndex = checkpointManager.GetCarNextCheckpointIndex(transform);
        }
    }
}
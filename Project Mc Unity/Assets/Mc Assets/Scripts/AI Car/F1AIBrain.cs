using UnityEngine;
using ArcadeVP;

[RequireComponent(typeof(ArcadeVehicleController))]
[RequireComponent(typeof(Rigidbody))]
public class F1AIBrain : MonoBehaviour
{
    [Header("AI Hedefleme")]
    public WaypointProgressTracker progressTracker;
    public AnimationCurve turnCurve;

    [Header("Öngörülü Fren (Look-Ahead Braking)")]
    public float baseLookAhead = 15f;
    public float minCornerSpeed = 35f;
    public float maxSpeedRef = 100f;

    [Header("AI İnsanlaştırma ve Fizik")]
    [Tooltip("Yapay zeka gaza maksimum yüzde kaç basabilsin? (Örn: 0.85 = %85. İnsan oyuncuyu dengelemek için)")]
    [Range(0.5f, 1f)] public float maxThrottleLimit = 0.85f;

    [Tooltip("Lerp yerine gerçekçi fiziksel fren gücü (Tekerlek sürtünmesi hissi)")]
    public float naturalBrakeForce = 2500f;

    private ArcadeVehicleController carController;
    private Rigidbody rb;
    private bool isRaceStarted = false;

    private float currentTurnAI;
    private float currentSpeedAI;
    private float currentBrakeAI;

    private void Awake()
    {
        carController = GetComponent<ArcadeVehicleController>();
        rb = GetComponent<Rigidbody>();
        if (progressTracker == null) progressTracker = GetComponent<WaypointProgressTracker>();
    }

    private void OnEnable()
    {
        CountdownManager.OnCountdownFinished += UnlockAIBrain;
        CheckpointManager.OnRaceFinished += StopAI;
    }

    private void OnDisable()
    {
        CountdownManager.OnCountdownFinished -= UnlockAIBrain;
        CheckpointManager.OnRaceFinished -= StopAI;
    }

    private void UnlockAIBrain() => isRaceStarted = true;

    private void StopAI(Transform finisherCar, bool isPlayer)
    {
        if (finisherCar == transform.root)
        {
            isRaceStarted = false;
        }
    }

    private void Update()
    {
        if (!isRaceStarted || progressTracker == null || progressTracker.circuit == null || carController == null)
        {
            if (carController != null) carController.ProvideInputs(0, 0, 1f);
            currentBrakeAI = 1f;
            return;
        }

        float currentSpeed = carController.carVelocity.z;
        float dynamicLookAhead = baseLookAhead + (currentSpeed * 0.4f);

        WaypointCircuit.RoutePoint currentRoute = progressTracker.circuit.GetRoutePoint(progressTracker.progressDistance);
        WaypointCircuit.RoutePoint upcomingRoute = progressTracker.circuit.GetRoutePoint(progressTracker.progressDistance + dynamicLookAhead);

        float upcomingCornerAngle = Vector3.Angle(currentRoute.direction, upcomingRoute.direction);
        float targetSpeed = Mathf.Lerp(maxSpeedRef, minCornerSpeed, upcomingCornerAngle / 75f);

        // --- GAZ VE FREN KARARI ---
        if (currentSpeed > targetSpeed + 5f)
        {
            currentBrakeAI = Mathf.Clamp01((currentSpeed - targetSpeed) / 15f);
            currentSpeedAI = 0f;
        }
        else if (currentSpeed < targetSpeed - 5f)
        {
            currentBrakeAI = 0f;
            currentSpeedAI = maxThrottleLimit;
        }
        else
        {
            currentBrakeAI = 0f;
            currentSpeedAI = maxThrottleLimit * 0.4f;
        }

        Vector3 dirToTarget = (progressTracker.target.position - transform.position).normalized;

        if (Vector3.Dot(transform.forward, dirToTarget) < -0.5f && currentSpeed < 5f)
        {
            currentBrakeAI = 1f;
            currentSpeedAI = -maxThrottleLimit;
        }

        float angleToDir = Vector3.SignedAngle(transform.forward, dirToTarget, Vector3.up);
        float steeringNormalized = Mathf.Clamp(angleToDir / 20f, -1f, 1f);

        currentTurnAI = (turnCurve != null && turnCurve.length > 0) ?
                        turnCurve.Evaluate(Mathf.Abs(steeringNormalized)) * Mathf.Sign(steeringNormalized) :
                        steeringNormalized;

        carController.ProvideInputs(currentTurnAI, currentSpeedAI, currentBrakeAI);
    }

    private void FixedUpdate()
    {
        if (carController == null || !carController.grounded()) return;

        if (currentBrakeAI > 0.1f && carController.carVelocity.z > 1f)
        {
            Vector3 brakingForce = -transform.forward * (naturalBrakeForce * currentBrakeAI);
            rb.AddForce(brakingForce, ForceMode.Acceleration);
        }
    }
}
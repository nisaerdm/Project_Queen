using System.Collections;
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
    [Range(0.5f, 1f)] public float maxThrottleLimit = 0.85f;
    public float throttleSmoothing = 2.5f;
    public float brakeSmoothing = 3.5f;
    public float naturalBrakeForce = 2500f;

    [Header("Çarpışma Önleyici Sensör (Anti-Bulldozer)")]
    public float frontSensorLength = 6f;

    private ArcadeVehicleController carController;
    private Rigidbody rb;
    private bool isRaceStarted = false;

    private float currentTurnAI;
    private float currentSpeedAI;
    private float currentBrakeAI;

    // Optimizasyon: Lazer kontrol sonucu
    private bool isBlockedAhead = false;

    private void Awake()
    {
        carController = GetComponent<ArcadeVehicleController>();
        rb = GetComponent<Rigidbody>();
        if (progressTracker == null) progressTracker = GetComponent<WaypointProgressTracker>();
    }

    private void OnEnable()
    {
        CountdownManager.OnCountdownFinished += UnlockAIBrain;
        CheckpointManager.OnRaceFinished += HandleRaceFinished;

        // Optimizasyon: Lazer taramasını Update dışına alıp yavaşlatıyoruz
        StartCoroutine(AntiBulldozerSensor());
    }

    private void OnDisable()
    {
        CountdownManager.OnCountdownFinished -= UnlockAIBrain;
        CheckpointManager.OnRaceFinished -= HandleRaceFinished;
        StopAllCoroutines();
    }

    private void UnlockAIBrain() => isRaceStarted = true;

    private void HandleRaceFinished(Transform finisherCar, bool isPlayer)
    {
        if (finisherCar == transform.root)
        {
            maxThrottleLimit *= 0.5f;
        }
    }

    // YENİLİK: Saniyede 60 kere lazer atmak yerine, saniyede 5 kere atar. Performansı kurtarır.
    private IEnumerator AntiBulldozerSensor()
    {
        WaitForSeconds waitFast = new WaitForSeconds(0.2f);
        while (true)
        {
            if (isRaceStarted)
            {
                isBlockedAhead = false;
                if (Physics.Raycast(transform.position + (Vector3.up * 0.5f), transform.forward, out RaycastHit hit, frontSensorLength))
                {
                    if (hit.collider.CompareTag("Player") || hit.collider.CompareTag("Car"))
                    {
                        isBlockedAhead = true;
                    }
                }
            }
            yield return waitFast;
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

        float desiredGas = 0f;
        float desiredBrake = 0f;

        // --- GAZ/FREN KARARI (Artık Update içinde Raycast yok!) ---
        if (isBlockedAhead)
        {
            desiredGas = 0f;
            desiredBrake = 0.4f;
        }
        else if (currentSpeed > targetSpeed)
        {
            desiredGas = 0f;
            desiredBrake = Mathf.Clamp01((currentSpeed - targetSpeed) / 20f);
        }
        else
        {
            desiredBrake = 0f;
            desiredGas = maxThrottleLimit;

            if (targetSpeed - currentSpeed < 5f) desiredGas *= 0.5f;
        }

        Vector3 dirToTarget = (progressTracker.target.position - transform.position).normalized;

        if (Vector3.Dot(transform.forward, dirToTarget) < -0.5f && currentSpeed < 5f)
        {
            desiredBrake = 1f;
            desiredGas = -maxThrottleLimit;
        }

        currentSpeedAI = Mathf.MoveTowards(currentSpeedAI, desiredGas, Time.deltaTime * throttleSmoothing);
        currentBrakeAI = Mathf.MoveTowards(currentBrakeAI, desiredBrake, Time.deltaTime * brakeSmoothing);

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
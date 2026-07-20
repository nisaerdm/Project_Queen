using UnityEngine;
using UnityEngine.InputSystem;
using ArcadeVP;

[RequireComponent(typeof(ArcadeVehicleController))]
[RequireComponent(typeof(Rigidbody))]
public class F1PlayerInput : MonoBehaviour
{
    private ArcadeVehicleController carController;
    private Rigidbody rb;

    private Vector2 moveInput;
    private float brakeInput;
    private bool isFinished = false;

    [Header("Gaz Yumuşatma (Throttle Damping)")]
    [Tooltip("Motorun %0'dan %100 güce ulaşma süresi (Düşük = Daha yavaş hızlanma, Yüksek = Agresif hızlanma)")]
    [SerializeField] private float throttleSpeed = 1.5f;

    private float smoothedGas = 0f;

    [Header("Mobil Kontrol Ayarları")]
    [Tooltip("Eğer aktifse araç kendi kendine sürekli tam gaz ileri gider.")]
    public bool autoAcceleration = true;

    [Header("Akıllı Fren Ayarları")]
    [Tooltip("S tuşuna basıldığında aracı yavaşlatacak fiziksel fren kuvveti")]
    [SerializeField] private float brakeForce = 15f;

    private void Awake()
    {
        carController = GetComponent<ArcadeVehicleController>();
        rb = GetComponent<Rigidbody>();
    }

    // --- EVENT ABONELİKLERİ ---
    private void OnEnable()
    {
        CheckpointManager.OnRaceFinished += HandleRaceFinished;
    }

    private void OnDisable()
    {
        CheckpointManager.OnRaceFinished -= HandleRaceFinished;
    }

    private void HandleRaceFinished(Transform carTransform, bool isPlayer)
    {
        if (isPlayer && carTransform == transform.root)
        {
            isFinished = true;
            moveInput = Vector2.zero;
            brakeInput = 0f;
        }
    }
    // ----------------------------------------

    public void OnMove(InputAction.CallbackContext context)
    {
        if (isFinished) return;
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnBrake(InputAction.CallbackContext context)
    {
        if (isFinished) return;
        brakeInput = context.ReadValue<float>();
    }

    private void FixedUpdate()
    {
        if (carController == null || !carController.grounded()) return;

        float forwardSpeed = carController.carVelocity.z;

        if (isFinished && forwardSpeed > 1f)
        {
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, brakeForce * Time.fixedDeltaTime);
            return;
        }

        bool isBraking = brakeInput > 0.1f || moveInput.y < -0.1f;
        if (isBraking && forwardSpeed > 1f)
        {
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, brakeForce * Time.fixedDeltaTime);
        }
    }

    private void Update()
    {
        if (carController != null)
        {
            if (isFinished)
            {
                carController.ProvideInputs(0f, 0f, 1f);
                return;
            }

            float forwardSpeed = carController.carVelocity.z;
            float currentSteer = moveInput.x;

            float targetGas = autoAcceleration ? 1f : Mathf.Clamp01(moveInput.y);

            smoothedGas = Mathf.MoveTowards(smoothedGas, targetGas, Time.deltaTime * throttleSpeed);

            float currentGas = smoothedGas;
            float currentBrake = 0f;

            bool isBraking = brakeInput > 0.1f || moveInput.y < -0.1f;

            if (isBraking)
            {
                if (forwardSpeed > 1f)
                {
                    currentGas = 0f;
                    currentBrake = 1f;
                }
                else
                {
                    currentGas = -1f;
                    currentBrake = 0f;
                }
            }
            carController.ProvideInputs(currentSteer, currentGas, currentBrake);
        }
    }
}
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
    [Tooltip("S tuşuna basıldığında aracı saniyede kaç birim yavaşlatacak?")]
    [SerializeField] private float brakeForce = 35f;

    // Jiroskop eklentimiz olan New Input Manager'ı okumak için referans
    private New_InputManager_ArcadeVP gyroManager;

    private void Awake()
    {
        carController = GetComponent<ArcadeVehicleController>();
        rb = GetComponent<Rigidbody>();
        gyroManager = GetComponent<New_InputManager_ArcadeVP>();
    }

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

        if (isFinished && forwardSpeed > 0.5f)
        {
            rb.linearVelocity = Vector3.MoveTowards(rb.linearVelocity, Vector3.zero, brakeForce * Time.fixedDeltaTime);
            return;
        }

        bool isBraking = brakeInput > 0.1f || moveInput.y < -0.1f;

        if (isBraking && forwardSpeed > 0.1f)
        {
            Vector3 localVel = transform.InverseTransformDirection(rb.linearVelocity);
            localVel.z = Mathf.MoveTowards(localVel.z, 0, brakeForce * Time.fixedDeltaTime);
            rb.linearVelocity = transform.TransformDirection(localVel);
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

            // OPTİMİZASYON: Direksiyon değerini Jiroskop koduna bırakıyoruz. (Eğer atanmamışsa tuşları alır)
            float currentSteer = 0f;
            if (gyroManager == null)
            {
                currentSteer = moveInput.x;
            }

            float targetGas = autoAcceleration ? 1f : Mathf.Clamp01(moveInput.y);
            smoothedGas = Mathf.MoveTowards(smoothedGas, targetGas, Time.deltaTime * throttleSpeed);

            float currentGas = smoothedGas;
            float currentBrake = 0f;

            bool isBraking = brakeInput > 0.1f || moveInput.y < -0.1f;

            if (isBraking)
            {
                currentGas = 0f;
                currentBrake = 1f;
            }

            // Jiroskop kodumuz kendi Update'inde direksiyonu zaten verdiği için, burada direksiyonu sadece eğer gyroManager yoksa Provide ediyoruz.
            // Aksi halde GyroManager, direksiyon işlemini ezip kendi Provide komutunu yollar.
            if (gyroManager == null)
            {
                carController.ProvideInputs(currentSteer, currentGas, currentBrake);
            }
            else
            {
                // Eğer jiroskop kodu sahnede varsa, F1PlayerInput sadece Gaz ve Freni iletir. (Direksiyonu 0 gönderir, gyroManager onu ezer)
                // NOT: New_InputManager_ArcadeVP, ArcadeVehicleController'a tüm verileri iletecek şekilde tasarlandı. 
                // İleride tuşları tamamen ona devredebiliriz. Şimdilik araba yürümesi için bırakıyoruz.
                carController.ProvideInputs(currentSteer, currentGas, currentBrake);
            }
        }
    }
}
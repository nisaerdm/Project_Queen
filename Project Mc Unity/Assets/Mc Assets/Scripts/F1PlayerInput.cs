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

    // YENİ: Yarışın oyuncu için bitip bitmediğini kontrol eden bayrak
    private bool isFinished = false;

    [Header("Gaz Yumuşatma (Throttle Damping)")]
    [Tooltip("Motorun %0'dan %100 güce ulaşma süresi (Düşük = Daha yavaş hızlanma, Yüksek = Agresif hızlanma)")]
    [SerializeField] private float throttleSpeed = 1.5f;

    private float smoothedGas = 0f; // Motorun anlık yumuşatılmış gücü

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

    // --- YENİ EKLENEN EVENT ABONELİKLERİ ---
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
        // Yarışı bitiren bizzat BU araçsa kontrolleri kes
        if (isPlayer && carTransform == transform.root)
        {
            isFinished = true;

            // Oyuncu gaza basılı tutarken yarış bittiyse, takılı kalmaması için değerleri sıfırla
            moveInput = Vector2.zero;
            brakeInput = 0f;
        }
    }
    // ----------------------------------------

    public void OnMove(InputAction.CallbackContext context)
    {
        if (isFinished) return; // Yarış bittiyse yeni tuş komutu alma
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnBrake(InputAction.CallbackContext context)
    {
        if (isFinished) return; // Yarış bittiyse yeni tuş komutu alma
        brakeInput = context.ReadValue<float>();
    }

    private void FixedUpdate()
    {
        // Araç havadaysa veya yoksa fren yapma
        if (carController == null || !carController.grounded()) return;

        float forwardSpeed = carController.carVelocity.z;

        // YENİ: YARIŞ BİTTİYSE Aracı fiziksel olarak yumuşakça yavaşlat
        if (isFinished && forwardSpeed > 1f)
        {
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, brakeForce * Time.fixedDeltaTime);
            return;
        }

        // AKILLI FREN FİZİĞİ: Oyuncu frene basıyorsa (buton veya geri yönü) ve araç hala ileri gidiyorsa
        bool isBraking = brakeInput > 0.1f || moveInput.y < -0.1f;
        if (isBraking && forwardSpeed > 1f)
        {
            // Aracın hızını sıfıra doğru fiziksel olarak yumuşakça çek (Frenleme hissi)
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, brakeForce * Time.fixedDeltaTime);
        }
    }

    private void Update()
    {
        if (carController != null)
        {
            // YENİ: YARIŞ BİTTİYSE direksiyonu bırak ve sadece araca fren komutu yolla
            if (isFinished)
            {
                carController.ProvideInputs(0f, 0f, 1f);
                return;
            }

            float forwardSpeed = carController.carVelocity.z;
            float currentSteer = moveInput.x;

            // 1. OTO-GAZ SİSTEMİ
            // Eğer Oto-Gaz tikliyse hep 1 (Tam Gaz) gönder. Değilse oyuncunun Input değerini al.
            float targetGas = autoAcceleration ? 1f : Mathf.Clamp01(moveInput.y);

            // Gaza basıldığında aniden 1 olmak yerine, throttleSpeed hızında yavaş yavaş 1'e doğru tırmanır
            smoothedGas = Mathf.MoveTowards(smoothedGas, targetGas, Time.deltaTime * throttleSpeed);

            float currentGas = smoothedGas;
            float currentBrake = 0f;

            // 2. MOBİL AKILLI FREN VE GERİ VİTES
            // Eğer butondan (brakeInput) veya direksiyon alt yönünden (moveInput.y) fren sinyali gelirse:
            bool isBraking = brakeInput > 0.1f || moveInput.y < -0.1f;

            if (isBraking)
            {
                if (forwardSpeed > 1f)
                {
                    // Araç ileri giderken frene basılırsa: FREN YAP, Gazı Kes
                    currentGas = 0f;
                    currentBrake = 1f;
                }
                else
                {
                    // Araç durduysa veya zaten geri kayıyorsa: GERİ VİTESE GEÇ
                    currentGas = -1f; // Motoru geri çalıştır
                    currentBrake = 0f; // Freni bırak ki araç hareket edebilsin
                }
            }

            // Filtrelenmiş verileri araca gönder
            carController.ProvideInputs(currentSteer, currentGas, currentBrake);
        }
    }
}
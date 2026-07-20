using UnityEngine;
using ArcadeVP;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(ArcadeVehicleController))]
public class F1PhysicsEnhancer : MonoBehaviour
{
    [Header("F1 Yere Basma Kuvveti (Downforce)")]
    [Tooltip("Araç hızlandıkça asfalta ne kadar ekstra bastırılacak?")]
    [SerializeField] private float dynamicDownforceMultiplier = 3.0f;
    [Tooltip("Downforce'un devreye gireceği minimum hız.")]
    [SerializeField] private float minSpeedForDownforce = 5f;

    [Header("F1 Ağırlık Merkezi (Center of Mass)")]
    [Tooltip("Aracın takla atmaması için ağırlık merkezini şasenin ne kadar altına çekeceğiz?")]
    [SerializeField] private Vector3 centerOfMassOffset = new Vector3(0, -0.6f, 0);

    [Header("Geri Vites Limiti")]
    [Tooltip("Geri geri giderken ulaşılabilecek maksimum hız")]
    [SerializeField] private float maxReverseSpeed = 15f;

    private Rigidbody rb;
    private ArcadeVehicleController carController;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        carController = GetComponent<ArcadeVehicleController>();

        // 1. Ağırlık Merkezini (Center of Mass) aşağı çekiyoruz.
        // Bu, aracın virajlarda bir F1 aracı gibi tok dönmesini ve takla atmamasını sağlar.
        rb.centerOfMass += centerOfMassOffset;
    }

    private void FixedUpdate()
    {
        // Eğer araç havadaysa veya yerle teması kesildiyse downforce uygulamıyoruz.
        if (!carController.grounded()) return;

        ApplyDynamicDownforce();
        LimitReverseSpeed();
    }

    private void ApplyDynamicDownforce()
    {
        // Aracın ileri yönlü hızını alıyoruz
        float forwardSpeed = carController.carVelocity.z;

        // F1 araçlarında downforce hızla beraber artar.
        if (forwardSpeed > minSpeedForDownforce)
        {
            float downforceAmount = forwardSpeed * dynamicDownforceMultiplier;
            // Aracı yerel (local) alt ekseninde (-transform.up) aşağı doğru itiyoruz
            rb.AddForce(-transform.up * downforceAmount, ForceMode.Force);
        }
    }

    private void LimitReverseSpeed()
    {
        // carVelocity.z negatifse araç geri geri gidiyor demektir.
        float currentZVelocity = carController.carVelocity.z;

        if (currentZVelocity < -maxReverseSpeed)
        {
            // Unity 6 standartlarına uygun olarak linearVelocity kullanıyoruz
            Vector3 localVel = transform.InverseTransformDirection(rb.linearVelocity);

            // Eğer geri geri gitme hızı limiti aştıysa, hızı kelepçeliyoruz (Clamp)
            if (localVel.z < -maxReverseSpeed)
            {
                localVel.z = -maxReverseSpeed;
                rb.linearVelocity = transform.TransformDirection(localVel);
            }
        }
    }

    // Geliştirici kolaylığı: Seçiliyken Editor'de ağırlık merkezini kırmızı bir top olarak gösterir
    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying && rb != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.TransformPoint(rb.centerOfMass), 0.15f);
        }
    }
}
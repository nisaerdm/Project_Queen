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
        rb.centerOfMass += centerOfMassOffset;
    }

    private void FixedUpdate()
    {
        if (!carController.grounded()) return;

        ApplyDynamicDownforce();
        LimitReverseSpeed();
    }

    private void ApplyDynamicDownforce()
    {
        float forwardSpeed = carController.carVelocity.z;

        if (forwardSpeed > minSpeedForDownforce)
        {
            float downforceAmount = forwardSpeed * dynamicDownforceMultiplier;
            rb.AddForce(-transform.up * downforceAmount, ForceMode.Force);
        }
    }

    private void LimitReverseSpeed()
    {
        float currentZVelocity = carController.carVelocity.z;

        if (currentZVelocity < -maxReverseSpeed)
        {
            Vector3 localVel = transform.InverseTransformDirection(rb.linearVelocity);

            if (localVel.z < -maxReverseSpeed)
            {
                localVel.z = -maxReverseSpeed;
                rb.linearVelocity = transform.TransformDirection(localVel);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying && rb != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.TransformPoint(rb.centerOfMass), 0.15f);
        }
    }
}
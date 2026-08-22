using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class F1CollisionFeedback : MonoBehaviour
{
    [Header("Çarpışma Ayarları")]
    [Tooltip("Çarpışma anında araçların birbirini ne kadar şiddetle iteceği")]
    [SerializeField] private float collisionRepelForce = 15f;
    
    [Tooltip("Sadece bu etiketlere sahip objelerle çarpışıldığında tepki ver (Örn: Player, Car)")]
    [SerializeField] private string[] targetTags = { "Player", "Car" };

    private Rigidbody rb;
    private float lastCollisionTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Peş peşe çok fazla çarpışma algılanıp aracı uzaya fırlatmasını engellemek için Cooldown (0.1 sn)
        if (Time.time < lastCollisionTime + 0.1f) return;

        // Çarptığımız obje geçerli bir araç mı?
        bool isTargetValid = false;
        foreach (string t in targetTags)
        {
            if (collision.gameObject.CompareTag(t))
            {
                isTargetValid = true;
                break;
            }
        }

        if (isTargetValid)
        {
            // 1. Çarpışma noktasını ve yönünü bul
            Vector3 collisionNormal = collision.contacts[0].normal;
            
            // 2. Y eksenini (Yukarı/Aşağı) sıfırla ki araçlar çarpışınca havaya uçup takla atmasın
            collisionNormal.y = 0;
            collisionNormal.Normalize();

            // 3. Mevcut ivmeyi anlık olarak zayıflat (Arcade Controller'ın inatlaşmasını kırmak için)
            rb.linearVelocity *= 0.5f;

            // 4. Aracı çarpışma noktasının tersine doğru it (Sekme hissi)
            rb.AddForce(-collisionNormal * collisionRepelForce, ForceMode.VelocityChange);

            lastCollisionTime = Time.time;
        }
    }
}
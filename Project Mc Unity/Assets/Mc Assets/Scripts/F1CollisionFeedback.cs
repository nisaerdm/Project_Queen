using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class F1CollisionFeedback : MonoBehaviour
{
    [Header("Çarpışma Ayarları")]
    [Tooltip("Çarpışma anında araçların birbirini ne kadar şiddetle iteceği")]
    [SerializeField] private float collisionRepelForce = 15f;

    private Rigidbody rb;
    private float lastCollisionTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // OPTİMİZASYON: Sürtünmelerde FPS drop yememek için Cooldown çeyrek saniyeye çekildi.
        if (Time.time < lastCollisionTime + 0.25f) return;

        // OPTİMİZASYON: Array içinde string dolaşmak yerine direkt en hızlı metodu (CompareTag) kullanıyoruz.
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Car"))
        {
            // 1. Çarpışma noktasını ve yönünü bul
            Vector3 collisionNormal = collision.contacts[0].normal;

            // 2. Y eksenini (Yukarı/Aşağı) sıfırla ki araçlar havaya uçmasın
            collisionNormal.y = 0;
            collisionNormal.Normalize();

            // 3. Mevcut ivmeyi anlık olarak zayıflat
            rb.linearVelocity *= 0.5f;

            // 4. Aracı çarpışma noktasının tersine doğru it (Sekme hissi)
            rb.AddForce(-collisionNormal * collisionRepelForce, ForceMode.VelocityChange);

            lastCollisionTime = Time.time;
        }
    }
}
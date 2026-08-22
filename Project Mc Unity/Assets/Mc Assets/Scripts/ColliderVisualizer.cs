using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ColliderVisualizer : MonoBehaviour
{
#if UNITY_EDITOR
    [Header("Görselleştirme Ayarları")]
    [Tooltip("Gizmo'nun rengini ve saydamlığını belirleyin.")]
    public Color gizmoColor = new Color(0f, 1f, 0f, 0.5f); // Varsayılan: Yarı saydam yeşil

    [Tooltip("Sadece dış hatlar (tel örgü) mı çizilsin? Kapatılırsa içi dolu çizilir.")]
    public bool wireframeOnly = true;

    private void OnDrawGizmos()
    {
        // Objedeki collider'ı al
        Collider col = GetComponent<Collider>();
        if (col == null) return;

        // Rengi ve transform (pozisyon, rotasyon, scale) matrisini ayarla
        Gizmos.color = gizmoColor;
        Gizmos.matrix = transform.localToWorldMatrix;

        // Collider türüne göre çizim yap
        if (col is BoxCollider box)
        {
            if (wireframeOnly) Gizmos.DrawWireCube(box.center, box.size);
            else Gizmos.DrawCube(box.center, box.size);
        }
        else if (col is SphereCollider sphere)
        {
            if (wireframeOnly) Gizmos.DrawWireSphere(sphere.center, sphere.radius);
            else Gizmos.DrawSphere(sphere.center, sphere.radius);
        }
        else if (col is MeshCollider meshCollider && meshCollider.sharedMesh != null)
        {
            if (wireframeOnly) Gizmos.DrawWireMesh(meshCollider.sharedMesh);
            else Gizmos.DrawMesh(meshCollider.sharedMesh);
        }
    }
#endif
}
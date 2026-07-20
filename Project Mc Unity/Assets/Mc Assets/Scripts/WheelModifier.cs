using UnityEngine;

public class WheelModifier : MonoBehaviour
{
    [Header("Tekerlek Referansları")]
    [Tooltip("Aracın 4 tekerleğindeki MeshFilter bileşenleri")]
    [SerializeField] private MeshFilter[] wheelMeshFilters;

    public void ApplyWheel(Mesh newWheelMesh, Material newWheelMaterial)
    {
        if (wheelMeshFilters == null || wheelMeshFilters.Length == 0) return;

        foreach (MeshFilter filter in wheelMeshFilters)
        {
            if (filter != null)
            {
                filter.sharedMesh = newWheelMesh;
                MeshRenderer renderer = filter.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.sharedMaterial = newWheelMaterial;
                }
            }
        }
    }

    // --- SUNUM VE TEST KISMI ---
    [Header("Test Jantı")]
    [Tooltip("Test için kullanılacak 3D tekerlek modeli")]
    public Mesh testWheelMesh;

    [Tooltip("Test tekerleğinin materyali")]
    public Material testWheelMaterial;

    public void TestTekerlegiDegistir()
    {
        if (testWheelMesh != null && testWheelMaterial != null)
        {
            ApplyWheel(testWheelMesh, testWheelMaterial);
            Debug.Log("[Jant Değişti] Test tekerleği başarıyla araca takıldı!");
        }
        else
        {
            Debug.LogWarning("Uyarı: Test için Mesh veya Material boş bırakılmış!");
        }
    }
}
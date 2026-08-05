using UnityEngine;

public class CarColorModifier : MonoBehaviour
{
    [Header("Boya Ayarları")]
    [Tooltip("Aracın boyanmasını istediğin BÜTÜN parçalarını (Renderer) buraya ekle.")]
    [SerializeField] private Renderer[] bodyRenderers;

    [Tooltip("Kullanılabilir renk materyalleri (0: Kırmızı, 1: Mavi vs.)")]
    [SerializeField] private Material[] colorMaterials;

    [Header("Veri Kaydı")]
    [Tooltip("Hangi oyuncunun rengini okuyacağız? (Split-screen için P1_Color, P2_Color)")]
    [SerializeField] private string playerPrefsKey = "P1_Color";

    private void Start()
    {
        int savedColorIndex = PlayerPrefs.GetInt(playerPrefsKey, 0);
        ApplyColor(savedColorIndex);
    }

    private void OnEnable()
    {
        // Renk değiştirme olayını dinlemeye başla
        LobbyEventManager.OnColorSelected += HandleColorSelected;
    }

    private void OnDisable()
    {
        // Dinlemeyi bırak
        LobbyEventManager.OnColorSelected -= HandleColorSelected;
    }

    private void HandleColorSelected(int colorIndex)
    {
        // 1. Yeni rengi cihaza kaydet
        PlayerPrefs.SetInt(playerPrefsKey, colorIndex);
        PlayerPrefs.Save();

        // 2. Aracı anında boya
        ApplyColor(colorIndex);
    }

    public void ApplyColor(int colorIndex)
    {
        // Dizi boşsa veya materyal yoksa iptal et
        if (colorMaterials.Length == 0 || bodyRenderers == null || bodyRenderers.Length == 0) return;

        colorIndex = Mathf.Clamp(colorIndex, 0, colorMaterials.Length - 1);

        // Bütün parçaları döngüye sokup tek tek boyuyoruz
        foreach (Renderer partRenderer in bodyRenderers)
        {
            if (partRenderer != null)
            {
                partRenderer.material = colorMaterials[colorIndex];
            }
        }
    }
}
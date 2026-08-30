using UnityEngine;

public class CarColorModifier : MonoBehaviour
{
    [Header("Boya Ayarları")]
    [Tooltip("Aracın boyanmasını istediğin BÜTÜN parçalarını (Renderer) SIRASIYLA buraya ekle.")]
    [SerializeField] private Renderer[] bodyRenderers;

    [Tooltip("Project penceresinde oluşturduğun Car Skin Data (Renk Paketleri) dosyalarını buraya sürükle.")]
    [SerializeField] private CarSkinData[] carSkins;

    [Header("Veri Kaydı")]
    [Tooltip("Hangi oyuncunun rengini okuyacağız?")]
    [SerializeField] private string playerPrefsKey = "P1_Color";

    private void Start()
    {
        int savedColorIndex = PlayerPrefs.GetInt(playerPrefsKey, 0);
        ApplyColor(savedColorIndex);
    }

    private void OnEnable()
    {
        LobbyEventManager.OnColorSelected += HandleColorSelected;
    }

    private void OnDisable()
    {
        LobbyEventManager.OnColorSelected -= HandleColorSelected;
    }

    private void HandleColorSelected(int colorIndex)
    {
        PlayerPrefs.SetInt(playerPrefsKey, colorIndex);
        PlayerPrefs.Save();
        ApplyColor(colorIndex);
    }

    public void ApplyColor(int colorIndex)
    {
        if (carSkins == null || carSkins.Length == 0 || bodyRenderers == null || bodyRenderers.Length == 0) return;

        colorIndex = Mathf.Clamp(colorIndex, 0, carSkins.Length - 1);
        CarSkinData selectedSkin = carSkins[colorIndex];

        if (selectedSkin == null) return;

        if (!selectedSkin.isCustomDesign)
        {
            foreach (Renderer partRenderer in bodyRenderers)
            {
                if (partRenderer != null && selectedSkin.normalMaterial != null)
                {
                    partRenderer.material = selectedSkin.normalMaterial;
                }
            }
        }
        else
        {
            for (int i = 0; i < bodyRenderers.Length; i++)
            {
                if (i < selectedSkin.customPartMaterials.Length && bodyRenderers[i] != null && selectedSkin.customPartMaterials[i] != null)
                {
                    bodyRenderers[i].material = selectedSkin.customPartMaterials[i];
                }
            }
        }
    }
}
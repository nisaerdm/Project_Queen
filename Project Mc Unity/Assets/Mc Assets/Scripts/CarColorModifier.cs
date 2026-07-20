using UnityEngine;

public class CarColorModifier : MonoBehaviour
{
    [Header("Boya Ayarları")]
    [Tooltip("Aracın gövdesini temsil eden ve colormap materyalini kullanan MeshRenderer")]
    [SerializeField] private MeshRenderer bodyRenderer;

    // Performans için Property ID'sini önbelleğe alıyoruz
    private int colorPropertyID = Shader.PropertyToID("_BaseColor"); // URP/HDRP kullanıyorsan _BaseColor, Standard ise _Color yap.

    /// <summary>
    /// Lobi UI sistemindeki renk butonlarından çağrılacak asıl metod.
    /// </summary>
    public void ApplyColor(Color newColor)
    {
        if (bodyRenderer == null) return;

        // 1. Yeni bir özellik bloğu oluştur
        MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();

        // 2. Renderer'ın mevcut özelliklerini bloğa kopyala (başka ayarlar bozulmasın diye)
        bodyRenderer.GetPropertyBlock(propertyBlock);

        // 3. Bloğun renk değerini değiştir
        propertyBlock.SetColor(colorPropertyID, newColor);

        // 4. Bloğu tekrar renderer'a enjekte et (Materyal KOPYALANMAZ!)
        bodyRenderer.SetPropertyBlock(propertyBlock);
    }

    // --- SUNUM VE TEST KISMI ---
    [Header("Test Ayarları")]
    [Tooltip("Ekibe gösterirken buradan istediğin rengi seçip test butonuna basabilirsin")]
    public Color testColor = Color.green;

    public void TestRenginiUygula()
    {
        ApplyColor(testColor);
        Debug.Log($"[Araç Boyandı] Yeni Renk: {testColor}");
    }
}
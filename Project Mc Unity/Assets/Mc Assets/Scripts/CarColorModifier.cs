using UnityEngine;

public class CarColorModifier : MonoBehaviour
{
    [Header("Boya Ayarları")]
    [Tooltip("Aracın gövdesini temsil eden ve colormap materyalini kullanan MeshRenderer")]
    [SerializeField] private MeshRenderer bodyRenderer;

    private int colorPropertyID = Shader.PropertyToID("_BaseColor");

    public void ApplyColor(Color newColor)
    {
        if (bodyRenderer == null) return;

        MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();

        bodyRenderer.GetPropertyBlock(propertyBlock);

        propertyBlock.SetColor(colorPropertyID, newColor);

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
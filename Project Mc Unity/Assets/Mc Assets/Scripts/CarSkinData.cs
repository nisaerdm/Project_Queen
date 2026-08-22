using UnityEngine;

[CreateAssetMenu(fileName = "NewCarSkin", menuName = "DarkHaven/Car Skin Data")]
public class CarSkinData : ScriptableObject
{
    [Tooltip("Bu renk, arabanın her parçası için ayrı bir çizim içeriyor mu?")]
    public bool isCustomDesign = false;

    [Tooltip("Tik KAPALIYSA tüm araca bu renk uygulanır.")]
    public Material normalMaterial;

    [Tooltip("Tik AÇIKSA her parçaya sırasıyla bu listedeki materyaller atanır.")]
    public Material[] customPartMaterials;
}
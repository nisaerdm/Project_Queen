using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSessionData", menuName = "1.5 Adana/Player Session Data")]
public class PlayerSessionDataSO : ScriptableObject
{
    [Tooltip("Oyuncunun garajda seçtiği aracın kaporta materyali")]
    public Material selectedCarMaterial;
}
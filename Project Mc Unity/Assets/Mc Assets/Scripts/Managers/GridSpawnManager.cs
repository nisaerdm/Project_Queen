using System;
using System.Collections.Generic;
using UnityEngine;

public class GridSpawnManager : MonoBehaviour
{
    // Araçlar spawnlandığında Kamera veya Split-Screen yöneticisinin yakalaması için event
    public static event Action<List<GameObject>> OnVehiclesSpawned;

    [Header("Grid Konfigürasyonu")]
    [Tooltip("Pole pozisyonundan başlayarak geriye doğru sıralanacak spawn noktaları.")]
    [SerializeField] private List<Transform> spawnPoints;

    [Header("Araç Verileri")]
    [Tooltip("Yarışa katılacak araçların prefabları. (Lobby'den gelen verilerle dinamik olarak da doldurulabilir)")]
    [SerializeField] private List<GameObject> vehiclePrefabs;

    /// <summary>
    /// Bu metot, oyunun başlangıç sekansını yöneten bir GameManager veya StateMachine tarafından çağrılmalıdır.
    /// </summary>
    public void InitializeGrid()
    {
        if (spawnPoints.Count == 0)
        {
            Debug.LogError("[GridSpawnManager] HATA: Spawn noktaları atanmamış!");
            return;
        }

        if (vehiclePrefabs.Count == 0)
        {
            Debug.LogWarning("[GridSpawnManager] UYARI: Spawnlanacak araç prefab'ı yok!");
            return;
        }

        List<GameObject> spawnedVehicles = new List<GameObject>();

        // Araç sayısı ile grid sayısından hangisi küçükse o kadar spawn işlemi yap (IndexOut sınırlandırması)
        int spawnCount = Mathf.Min(vehiclePrefabs.Count, spawnPoints.Count);

        for (int i = 0; i < spawnCount; i++)
        {
            GameObject vehicle = Instantiate(vehiclePrefabs[i], spawnPoints[i].position, spawnPoints[i].rotation);
            vehicle.name = $"Player_{i + 1}";

            // Sadece Player 1 (i == 0) aracı kontrol edebilsin, diğerlerinin input'unu kapat.
            if (i > 0)
            {
                var inputManager = vehicle.GetComponent<ArcadeVP.New_InputManager_ArcadeVP>();
                if (inputManager != null)
                {
                    inputManager.enabled = false;
                }
            }

            spawnedVehicles.Add(vehicle);
        }

        Debug.Log($"[GridSpawnManager] {spawnCount} adet araç başarıyla grid'e yerleştirildi.");

        // İşi bitince diğer sistemlere (örn: Input Manager, Cinemachine) araçların listesini pasla
        OnVehiclesSpawned?.Invoke(spawnedVehicles);
    }

    /// <summary>
    /// GDD Madde 4: Respawn Controller için kullanılacak yardımcı metot.
    /// Belirli bir aracı, en son geçtiği geçerli checkpoint'e ışınlar.
    /// </summary>
    public void RespawnVehicleAtPosition(GameObject vehicle, Transform targetCheckpoint)
    {
        // Arcade Car Physics gibi rigidbody tabanlı araçlarda teleport işlemi öncesi hız sıfırlanmalıdır.
        Rigidbody rb = vehicle.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        vehicle.transform.position = targetCheckpoint.position;
        vehicle.transform.rotation = targetCheckpoint.rotation;
    }
}
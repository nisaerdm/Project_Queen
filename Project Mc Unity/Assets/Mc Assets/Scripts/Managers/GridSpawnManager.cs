using System;
using System.Collections.Generic;
using UnityEngine;

public class GridSpawnManager : MonoBehaviour
{
    public static event Action<List<GameObject>> OnVehiclesSpawned;

    [Header("Grid Konfigürasyonu")]
    [Tooltip("Pole pozisyonundan başlayarak geriye doğru sıralanacak spawn noktaları.")]
    [SerializeField] private List<Transform> spawnPoints;

    [Header("Araç Verileri")]
    [Tooltip("Yarışa katılacak araçların prefabları. (Lobby'den gelen verilerle dinamik olarak da doldurulabilir)")]
    [SerializeField] private List<GameObject> vehiclePrefabs;

    public void InitializeGrid()
    {
        if (spawnPoints.Count == 0)
        {
            return;
        }

        if (vehiclePrefabs.Count == 0)
        {
            return;
        }

        List<GameObject> spawnedVehicles = new List<GameObject>();

        int targetCarCount = PlayerPrefs.GetInt("Race_Cars", 2);
        int spawnCount = Mathf.Min(targetCarCount, vehiclePrefabs.Count, spawnPoints.Count);

        List<Transform> activeSpawnPoints = spawnPoints.GetRange(0, spawnCount);

        for (int i = 0; i < activeSpawnPoints.Count; i++)
        {
            int randomIndex = UnityEngine.Random.Range(i, activeSpawnPoints.Count);
            Transform temp = activeSpawnPoints[i];
            activeSpawnPoints[i] = activeSpawnPoints[randomIndex];
            activeSpawnPoints[randomIndex] = temp;
        }

        for (int i = 0; i < spawnCount; i++)
        {
            GameObject vehicle = Instantiate(vehiclePrefabs[i], activeSpawnPoints[i].position, activeSpawnPoints[i].rotation);
            vehicle.name = $"Player_{i + 1}";

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

        OnVehiclesSpawned?.Invoke(spawnedVehicles);
    }

    public void RespawnVehicleAtPosition(GameObject vehicle, Transform targetCheckpoint)
    {
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
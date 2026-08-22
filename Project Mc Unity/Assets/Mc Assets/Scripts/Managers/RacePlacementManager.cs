using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RacePlacementManager : MonoBehaviour
{
    public static event Action<List<Transform>> OnPlacementUpdated;

    [Header("Optimizasyon Ayarları")]
    [Tooltip("Sıralama hesaplama sıklığı. Update yerine Coroutine ile çalışır. 0.25f gayet akıcıdır.")]
    [SerializeField] private float calculationRate = 0.25f;

    private CheckpointManager checkpointManager;
    private List<Transform> activeCars = new List<Transform>();
    private Coroutine placementRoutine;

    private void Awake()
    {
        checkpointManager = UnityEngine.Object.FindFirstObjectByType<CheckpointManager>();
    }

    private void OnEnable()
    {
        GridSpawnManager.OnVehiclesSpawned += InitializeCars;
    }

    private void OnDisable()
    {
        GridSpawnManager.OnVehiclesSpawned -= InitializeCars;
    }

    private void InitializeCars(List<GameObject> spawnedVehicles)
    {
        activeCars.Clear();
        foreach (var vehicle in spawnedVehicles)
        {
            Transform rootCar = vehicle.transform.root;
            activeCars.Add(rootCar);
            checkpointManager.RegisterCar(rootCar);
        }

        if (placementRoutine != null) StopCoroutine(placementRoutine);
        placementRoutine = StartCoroutine(CalculatePlacementRoutine());
    }

    private IEnumerator CalculatePlacementRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(calculationRate);
        while (true)
        {
            if (activeCars.Count > 0)
            {
                SortCars();
                OnPlacementUpdated?.Invoke(activeCars);
            }
            yield return wait;
        }
    }

    private void SortCars()
    {
        // ÇÖZÜM: Son viraj hatasını önlemek için Checkpoint 0'ı (Bitiş çizgisi) en yüksek değer (9999) olarak algılatıyoruz.
        activeCars = activeCars.OrderByDescending(car => checkpointManager.GetCarLap(car))
                               .ThenByDescending(car =>
                               {
                                   int nextCp = checkpointManager.GetCarNextCheckpointIndex(car);
                                   return nextCp == 0 ? 9999 : nextCp;
                               })
                               .ThenBy(car => Vector3.Distance(car.position, checkpointManager.GetNextCheckpoint(car).position))
                               .ToList();
    }
}
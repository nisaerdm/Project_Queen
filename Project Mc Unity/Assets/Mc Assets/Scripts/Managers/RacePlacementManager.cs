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

    private void Start()
    {
        checkpointManager = UnityEngine.Object.FindFirstObjectByType<CheckpointManager>();
        GameObject[] cars = GameObject.FindGameObjectsWithTag("Car");
        foreach (var car in cars)
        {
            activeCars.Add(car.transform);
            checkpointManager.RegisterCar(car.transform);
        }

        if (placementRoutine == null)
        {
            placementRoutine = StartCoroutine(CalculatePlacementRoutine());
        }
    }

    private IEnumerator CalculatePlacementRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(calculationRate);
        while (true)
        {
            SortCars();
            OnPlacementUpdated?.Invoke(activeCars);
            yield return wait;
        }
    }

    private void SortCars()
    {
        List<GameObject> allVehiclesInScene = new List<GameObject>();
        allVehiclesInScene.AddRange(GameObject.FindGameObjectsWithTag("Player"));
        allVehiclesInScene.AddRange(GameObject.FindGameObjectsWithTag("Car"));

        if (activeCars.Count != allVehiclesInScene.Count)
        {
            activeCars.Clear();
            foreach (var vehicle in allVehiclesInScene)
            {
                Transform rootCar = vehicle.transform.root;
                activeCars.Add(rootCar);
                checkpointManager.RegisterCar(rootCar);
            }
        }

        if (activeCars.Count == 0) return;
        activeCars = activeCars.OrderByDescending(car => checkpointManager.GetCarLap(car))
                               .ThenByDescending(car => checkpointManager.GetCarNextCheckpointIndex(car))
                               .ThenBy(car => Vector3.Distance(car.position, checkpointManager.GetNextCheckpoint(car).position))
                               .ToList();
    }
}
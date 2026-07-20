using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RacePlacementManager : MonoBehaviour
{
    // UI scriptlerin bu evente abone olarak güncel sıralamayı çekecek
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
        
        // Sahnede "Car" tag'ine sahip tüm araçları bul ve sisteme kaydet
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
        // 1. Hem Player'ı hem de yapay zeka araçlarını (Car) listeye topla
        List<GameObject> allVehiclesInScene = new List<GameObject>();
        allVehiclesInScene.AddRange(GameObject.FindGameObjectsWithTag("Player"));
        allVehiclesInScene.AddRange(GameObject.FindGameObjectsWithTag("Car"));

        // 2. Sahnedeki araç sayısı, bizim takip listemizden farklıysa listeyi yenile
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

        // Eğer henüz sahnede hiç araba yoksa sıralama yapma
        if (activeCars.Count == 0) return;

        // 3. Sıralama Mantığı
        activeCars = activeCars.OrderByDescending(car => checkpointManager.GetCarLap(car))
                               .ThenByDescending(car => checkpointManager.GetCarNextCheckpointIndex(car))
                               .ThenBy(car => Vector3.Distance(car.position, checkpointManager.GetNextCheckpoint(car).position))
                               .ToList();
    }
}
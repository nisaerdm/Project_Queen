using System;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static event Action<Transform, int> OnLapCompleted;
    public static event Action<Transform> OnWrongWay;
    public static event Action<Transform, bool> OnRaceFinished;

    [Header("Pist Ayarları")]
    [Tooltip("Haritadaki tüm checkpointleri SIRASIYLA buraya sürükle")]
    [SerializeField] private List<CheckpointSingle> checkpointList;

    [Tooltip("Toplam kaç tur atılacak?")]
    [SerializeField] private int totalLaps = 15;

    public int TotalLaps => totalLaps;

    private class CarProgress
    {
        public int currentLap = 0;
        public int nextCheckpointIndex = 0;
    }

    private Dictionary<Transform, CarProgress> carTrackers = new Dictionary<Transform, CarProgress>();

    private void Start()
    {
        foreach (CheckpointSingle cp in checkpointList)
        {
            cp.Initialize(this);
        }
    }

    public void RegisterCar(Transform carTransform)
    {
        Transform rootCar = carTransform.root;

        if (!carTrackers.ContainsKey(rootCar))
        {
            carTrackers.Add(rootCar, new CarProgress());
        }
    }

    public void PlayerThroughCheckpoint(CheckpointSingle checkpoint, Transform carTransform)
    {
        Transform rootCar = carTransform.root;

        if (!carTrackers.ContainsKey(rootCar)) RegisterCar(rootCar);

        CarProgress progress = carTrackers[rootCar];
        int hitCheckpointIndex = checkpointList.IndexOf(checkpoint);

        if (hitCheckpointIndex == progress.nextCheckpointIndex)
        {
            if (hitCheckpointIndex == 0)
            {
                progress.currentLap++;

                if (progress.currentLap > 1)
                {
                    int completedLap = progress.currentLap - 1;

                    // BİLDİRİM GERİ GELDİ (Tur)
                    Debug.Log($" [{rootCar.name}] Tur {completedLap} Tamamlandı!");

                    OnLapCompleted?.Invoke(rootCar, completedLap);

                    if (completedLap >= totalLaps)
                    {
                        bool isPlayer = rootCar.GetComponent<F1PlayerInput>() != null;
                        OnRaceFinished?.Invoke(rootCar, isPlayer);
                    }
                }
            }

            // BİLDİRİM GERİ GELDİ (Doğru Geçiş)
            Debug.Log($" [{rootCar.name}] Doğru Geçiş! Sıradaki Checkpoint: {((progress.nextCheckpointIndex + 1) % checkpointList.Count)}");

            progress.nextCheckpointIndex = (progress.nextCheckpointIndex + 1) % checkpointList.Count;
        }
        else
        {
            // BİLDİRİM GERİ GELDİ (Yanlış Yön)
            Debug.Log($" [{rootCar.name}] Yanlış Yön veya Atlanan Checkpoint! (Beklenen: {progress.nextCheckpointIndex}, Girilen: {hitCheckpointIndex})");
            OnWrongWay?.Invoke(rootCar);
        }
    }

    public Transform GetNextCheckpoint(Transform carTransform)
    {
        Transform rootCar = carTransform.root;
        if (!carTrackers.ContainsKey(rootCar)) return checkpointList[0].transform;
        return checkpointList[carTrackers[rootCar].nextCheckpointIndex].transform;
    }

    public int GetCarLap(Transform carTransform)
    {
        Transform rootCar = carTransform.root;
        return carTrackers.ContainsKey(rootCar) ? carTrackers[rootCar].currentLap : 0;
    }

    public int GetCarNextCheckpointIndex(Transform carTransform)
    {
        Transform rootCar = carTransform.root;
        return carTrackers.ContainsKey(rootCar) ? carTrackers[rootCar].nextCheckpointIndex : 0;
    }

    public Transform GetLastPassedCheckpoint(Transform carTransform)
    {
        Transform rootCar = carTransform.root;
        if (!carTrackers.ContainsKey(rootCar)) return checkpointList[0].transform;

        int lastIndex = carTrackers[rootCar].nextCheckpointIndex - 1;
        if (lastIndex < 0) lastIndex = 0;

        return checkpointList[lastIndex].transform;
    }
}
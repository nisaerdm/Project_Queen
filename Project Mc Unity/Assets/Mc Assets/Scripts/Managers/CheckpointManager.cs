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

    [Tooltip("Toplam kaç tur atılacak? (Lobi'den otomatik okunur)")]
    [SerializeField] private int totalLaps = 1;

    public int TotalLaps => totalLaps;

    private class CarProgress
    {
        public int currentLap = 0;
        public int nextCheckpointIndex = 0;
    }

    private Dictionary<Transform, CarProgress> carTrackers = new Dictionary<Transform, CarProgress>();

    private void Start()
    {
        totalLaps = PlayerPrefs.GetInt("Race_Laps", 1);

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
                    OnLapCompleted?.Invoke(rootCar, completedLap);

                    if (completedLap >= totalLaps)
                    {
                        // OPTİMİZASYON: GetComponent yerine daha hızlı olan CompareTag kullanıldı
                        bool isPlayer = rootCar.CompareTag("Player");
                        OnRaceFinished?.Invoke(rootCar, isPlayer);
                    }
                }
            }

            // OPTİMİZASYON: Mobil performansı artırmak için Debug logları gizlendi
            // Debug.Log($" [{rootCar.name}] Doğru Geçiş! Sıradaki Checkpoint: {((progress.nextCheckpointIndex + 1) % checkpointList.Count)}");

            progress.nextCheckpointIndex = (progress.nextCheckpointIndex + 1) % checkpointList.Count;
        }
        else
        {
            // Debug.Log($" [{rootCar.name}] Yanlış Yön veya Atlanan Checkpoint!");
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

        if (lastIndex < 0)
        {
            lastIndex = carTrackers[rootCar].currentLap > 0 ? checkpointList.Count - 1 : 0;
        }

        return checkpointList[lastIndex].transform;
    }
}
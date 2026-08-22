using System;
using UnityEngine;

public class RaceTimer : MonoBehaviour
{
    public static event Action<float> OnTimerStopped;

    public bool IsRunning { get; private set; }

    // Süreyi canlı olarak hesaplayan veya bitmiş süreyi veren property
    public float CurrentTime => IsRunning ? Time.time - startTime : finalTime;

    private float startTime;
    private float finalTime;

    public void StartTimer()
    {
        startTime = Time.time;
        IsRunning = true;
        Debug.Log("[RaceTimer] Kronometre Başladı!");
    }

    public void StopTimer()
    {
        if (!IsRunning) return;

        finalTime = Time.time - startTime;
        IsRunning = false;

        Debug.Log($"[RaceTimer] Yarış Bitti! Süre: {finalTime}");
        OnTimerStopped?.Invoke(finalTime);
    }

    // YENİ EKLENEN KISIM: GameOverUI scriptinin süreyi sorunsuz okuyabilmesi için gereken köprü
    public float GetCurrentTime()
    {
        return CurrentTime;
    }
}
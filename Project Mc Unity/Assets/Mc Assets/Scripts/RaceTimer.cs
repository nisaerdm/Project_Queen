using System;
using UnityEngine;

public class RaceTimer : MonoBehaviour
{
    // Yarış bitince UI ve diğer sistemleri uyarmak için Event
    public static event Action<float> OnTimerStopped;

    public bool IsRunning { get; private set; }
    
    // Time.time oyunun mutlak süresidir. FPS düşse bile bu matematik asla şaşmaz.
    public float CurrentTime => IsRunning ? Time.time - startTime : finalTime;

    private float startTime;
    private float finalTime;

    /// <summary>
    /// GameManager tarafından yarış başladığında tetiklenir.
    /// </summary>
    public void StartTimer()
    {
        startTime = Time.time;
        IsRunning = true;
        Debug.Log("[RaceTimer] Kronometre Başladı!");
    }

    /// <summary>
    /// CheckpointManager (Bitiş Çizgisi) tarafından tetiklenir.
    /// </summary>
    public void StopTimer()
    {
        if (!IsRunning) return;

        finalTime = Time.time - startTime;
        IsRunning = false;
        
        Debug.Log($"[RaceTimer] Yarış Bitti! Süre: {finalTime}");
        OnTimerStopped?.Invoke(finalTime);
    }
}
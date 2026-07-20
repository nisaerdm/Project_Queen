using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    [Header("Lego Sistem Modülleri")]
    [SerializeField] private GridSpawnManager gridManager;
    [SerializeField] private CinematicIntroManager introManager;
    [SerializeField] private CountdownManager countdownManager;
    [SerializeField] private RaceTimer raceTimer;

    [Header("Oyuncu Kontrolleri")]
    [SerializeField] private PlayerInput[] playerInputs;

    private void OnEnable()
    {
        CinematicIntroManager.OnIntroFinished += HandleIntroFinished;
        CountdownManager.OnCountdownFinished += HandleCountdownFinished;
        CheckpointManager.OnRaceFinished += HandleRaceFinished;
    }

    private void OnDisable()
    {
        CinematicIntroManager.OnIntroFinished -= HandleIntroFinished;
        CountdownManager.OnCountdownFinished -= HandleCountdownFinished;
        CheckpointManager.OnRaceFinished -= HandleRaceFinished;
    }

    private void Start()
    {
        LockPlayerInputs(true);
        gridManager.InitializeGrid();
        introManager.StartIntro();
    }

    private void HandleIntroFinished()
    {
        countdownManager.StartCountdown();
    }

    private void HandleCountdownFinished()
    {
        LockPlayerInputs(false);
        raceTimer.StartTimer();
    }

    // YENİ: Oyun Bitiş State'i
    private void HandleRaceFinished(Transform car, bool isPlayer)
    {
        if (isPlayer)
        {
            Debug.Log("🏁 OYUNCU YARIŞI BİTİRDİ! 🏁");

            LockPlayerInputs(true);
            raceTimer.StopTimer();

            // UI Eventlerini tetikle (Bitiş ekranını aç vb.)
            // İlerleyen aşamalarda buraya: UIManager.ShowGameOverScreen() eklenecek.
        }
        else
        {
            Debug.Log($"🤖 Bot {car.name} yarışı tamamladı!");
        }
    }

    private void LockPlayerInputs(bool isLocked)
    {
        foreach (var input in playerInputs)
        {
            if (input == null) continue;

            if (isLocked) input.DeactivateInput();
            else input.ActivateInput();
        }
    }
}
using UnityEngine;
using Unity.Cinemachine;

public class LobbyCameraManager : MonoBehaviour
{
    [Header("Cinemachine Cameras")]
    [SerializeField] private CinemachineCamera vcamMainMenu;
    [SerializeField] private CinemachineCamera vcamPlay;
    [SerializeField] private CinemachineCamera vcamGarage;
    [SerializeField] private CinemachineCamera vcamSettings;
    [SerializeField] private CinemachineCamera vcamCredits;

    private void OnEnable()
    {
        LobbyEventManager.OnMenuStateChanged += HandleCameraChange;
    }

    private void OnDisable()
    {
        LobbyEventManager.OnMenuStateChanged -= HandleCameraChange;
    }

    private void HandleCameraChange(LobbyEventManager.LobbyState state)
    {
        vcamMainMenu.Priority = 10;
        vcamPlay.Priority = 10;
        vcamGarage.Priority = 10;
        vcamSettings.Priority = 10;
        vcamCredits.Priority = 10;

        switch (state)
        {
            case LobbyEventManager.LobbyState.MainMenu:
                vcamMainMenu.Priority = 12;
                break;
            case LobbyEventManager.LobbyState.Play:
                vcamPlay.Priority = 12;
                break;
            case LobbyEventManager.LobbyState.Garage:
                vcamGarage.Priority = 12;
                break;
            case LobbyEventManager.LobbyState.Settings:
                vcamSettings.Priority = 12;
                break;
            case LobbyEventManager.LobbyState.Credits:
                vcamCredits.Priority = 12;
                break;
        }
    }
}
using UnityEngine;

public class LobbyUIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject playPanel;
    [SerializeField] private GameObject garagePanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject creditsPanel;

    private void OnEnable()
    {
        // Event'e abone oluyoruz
        LobbyEventManager.OnMenuStateChanged += HandleMenuStateChanged;
    }

    private void OnDisable()
    {
        // Script kapanırsa hafıza sızıntısı olmasın diye aboneliği iptal ediyoruz
        LobbyEventManager.OnMenuStateChanged -= HandleMenuStateChanged;
    }

    private void HandleMenuStateChanged(LobbyEventManager.LobbyState state)
    {
        // Önce hepsini kapatıyoruz
        mainMenuPanel.SetActive(false);
        playPanel.SetActive(false);
        garagePanel.SetActive(false);
        settingsPanel.SetActive(false);
        creditsPanel.SetActive(false);

        // İlgili paneli açıyoruz
        switch (state)
        {
            case LobbyEventManager.LobbyState.MainMenu:
                mainMenuPanel.SetActive(true);
                break;
            case LobbyEventManager.LobbyState.Play:
                playPanel.SetActive(true);
                break;
            case LobbyEventManager.LobbyState.Garage:
                garagePanel.SetActive(true);
                break;
            case LobbyEventManager.LobbyState.Settings:
                settingsPanel.SetActive(true);
                break;
            case LobbyEventManager.LobbyState.Credits:
                creditsPanel.SetActive(true);
                break;
        }
    }

    // Unity Editor'deki Butonların OnClick olaylarına atanacak metodlar
    public void OnClick_MainMenu() => LobbyEventManager.OnMenuStateChanged?.Invoke(LobbyEventManager.LobbyState.MainMenu);
    public void OnClick_Play() => LobbyEventManager.OnMenuStateChanged?.Invoke(LobbyEventManager.LobbyState.Play);
    public void OnClick_Garage() => LobbyEventManager.OnMenuStateChanged?.Invoke(LobbyEventManager.LobbyState.Garage);
    public void OnClick_Settings() => LobbyEventManager.OnMenuStateChanged?.Invoke(LobbyEventManager.LobbyState.Settings);
    public void OnClick_Credits() => LobbyEventManager.OnMenuStateChanged?.Invoke(LobbyEventManager.LobbyState.Credits);
}
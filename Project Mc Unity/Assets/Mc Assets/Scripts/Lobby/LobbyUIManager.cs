using UnityEngine;
using UnityEngine.SceneManagement; // Sahne işlemleri için gereken kütüphane

public class LobbyUIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject playPanel;
    [SerializeField] private GameObject garagePanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject creditsPanel;

    [Header("Scene Names")]
    [Tooltip("Geçiş yapılacak yarış sahnesinin tam adı")]
    [SerializeField] private string gameSceneName = "GameScene";
    [Tooltip("Lobi sahnesinin tam adı")]
    [SerializeField] private string lobbySceneName = "MainMenu";

    private void OnEnable()
    {
        LobbyEventManager.OnMenuStateChanged += HandleMenuStateChanged;
    }

    private void OnDisable()
    {
        LobbyEventManager.OnMenuStateChanged -= HandleMenuStateChanged;
    }

    private void HandleMenuStateChanged(LobbyEventManager.LobbyState state)
    {
        // Yarış sahnesinde bu paneller olmayacağı için null kontrolü yapıyoruz
        if (mainMenuPanel) mainMenuPanel.SetActive(false);
        if (playPanel) playPanel.SetActive(false);
        if (garagePanel) garagePanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);
        if (creditsPanel) creditsPanel.SetActive(false);

        switch (state)
        {
            case LobbyEventManager.LobbyState.MainMenu:
                if (mainMenuPanel) mainMenuPanel.SetActive(true);
                break;
            case LobbyEventManager.LobbyState.Play:
                if (playPanel) playPanel.SetActive(true);
                break;
            case LobbyEventManager.LobbyState.Garage:
                if (garagePanel) garagePanel.SetActive(true);
                break;
            case LobbyEventManager.LobbyState.Settings:
                if (settingsPanel) settingsPanel.SetActive(true);
                break;
            case LobbyEventManager.LobbyState.Credits:
                if (creditsPanel) creditsPanel.SetActive(true);
                break;
        }
    }

    // Menü Navigasyon Butonları
    public void OnClick_MainMenu() => LobbyEventManager.OnMenuStateChanged?.Invoke(LobbyEventManager.LobbyState.MainMenu);
    public void OnClick_Play() => LobbyEventManager.OnMenuStateChanged?.Invoke(LobbyEventManager.LobbyState.Play);
    public void OnClick_Garage() => LobbyEventManager.OnMenuStateChanged?.Invoke(LobbyEventManager.LobbyState.Garage);
    public void OnClick_Settings() => LobbyEventManager.OnMenuStateChanged?.Invoke(LobbyEventManager.LobbyState.Settings);
    public void OnClick_Credits() => LobbyEventManager.OnMenuStateChanged?.Invoke(LobbyEventManager.LobbyState.Credits);
    public void OnClick_SelectColor(int colorIndex) => LobbyEventManager.OnColorSelected?.Invoke(colorIndex);

    // --- SAHNE VE OYUN KONTROL BUTONLARI ---

    // 1. Oyunu Başlat (Lobi'deki "Start Match" tarzı butona atanacak)
    public void OnClick_LoadGameScene()
    {
        Time.timeScale = 1f; // Zamanın aktığından emin ol
        SceneManager.LoadScene(gameSceneName);
    }

    // 2. Oyunu Durdur (Yarış sahnesindeki Pause butonuna atanacak)
    public void OnClick_PauseGame()
    {
        Time.timeScale = 0f; // Zamanı dondur (Update, fizik vb. durur)
    }

    // 3. Oyuna Devam Et (Yarış sahnesindeki Resume butonuna atanacak)
    public void OnClick_ResumeGame()
    {
        Time.timeScale = 1f; // Zamanı normale çevir
    }

    // 4. Sahneyi Yeniden Başlat (Yarış sahnesindeki Restart butonuna atanacak)
    public void OnClick_RestartScene()
    {
        Time.timeScale = 1f; // Sahne donuksa çöz
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // Mevcut sahneyi baştan yükle
    }

    // 5. Ana Menüye Dön (Yarış sahnesinden çıkış butonuna atanacak)
    public void OnClick_ReturnToLobby()
    {
        Time.timeScale = 1f; // Sahne donuksa çöz
        SceneManager.LoadScene(lobbySceneName);
    }
}
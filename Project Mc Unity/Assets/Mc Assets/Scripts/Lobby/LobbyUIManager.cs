using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // YENİLİK: New Input System Kütüphanesi eklendi

public class LobbyUIManager : MonoBehaviour
{
    [Header("Lobby Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject playPanel;
    [SerializeField] private GameObject garagePanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject creditsPanel;

    [Header("Game Panels (Lego Modülü)")]
    [Tooltip("ESC'ye basıldığında açılacak olan Duraklatma (Pause) Menüsü")]
    [SerializeField] private GameObject pausePanel;

    [Header("Scene Names")]
    [Tooltip("Geçiş yapılacak yarış sahnesinin tam adı")]
    [SerializeField] private string gameSceneName = "GameScene";
    [Tooltip("Lobi sahnesinin tam adı")]
    [SerializeField] private string lobbySceneName = "MainMenu";

    private InputAction pauseAction;
    private bool isPaused = false;

    private void Awake()
    {
        // Lego Kuralı: Bu scripti başka projeye aktarsan bile ESC tuşu otomatik tanımlanır.
        // Dışarıdan Input Action Asset bağlamana gerek kalmaz, kendi içinde bağımsız yaşar.
        pauseAction = new InputAction("Pause", binding: "<Keyboard>/escape");
    }

    private void OnEnable()
    {
        LobbyEventManager.OnMenuStateChanged += HandleMenuStateChanged;

        // Input dinleyicisini aktifleştir ve ESC'ye basıldığında TogglePause metodunu tetikle
        pauseAction.Enable();
        pauseAction.performed += ctx => TogglePause();
    }

    private void OnDisable()
    {
        LobbyEventManager.OnMenuStateChanged -= HandleMenuStateChanged;

        // Dinleyiciyi kapat (Hafıza sızıntısını önlemek için optimum yaklaşım)
        pauseAction.performed -= ctx => TogglePause();
        pauseAction.Disable();
    }

    private void Start()
    {
        // Oyun başladığında pause panelinin kapalı olduğundan emin ol
        if (pausePanel) pausePanel.SetActive(false);
    }

    private void HandleMenuStateChanged(LobbyEventManager.LobbyState state)
    {
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

    // 1. Oyunu Başlat
    public void OnClick_LoadGameScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName);
    }

    // 2. Duraklatma (Pause) İşlemlerini Yöneten Ana Metod
    public void TogglePause()
    {
        // Sadece yarış sahnesindeysek (veya panel atanmışsa) çalışsın
        if (pausePanel == null) return;

        isPaused = !isPaused;

        if (isPaused)
        {
            pausePanel.SetActive(true);
            Time.timeScale = 0f; // Zamanı dondur (Fizikler ve Update'ler durur)
        }
        else
        {
            pausePanel.SetActive(false);
            Time.timeScale = 1f; // Zamanı normale çevir
        }
    }

    // 3. UI Butonundan Oyuna Devam Etmek İçin (Resume Butonuna Atanacak)
    public void OnClick_ResumeGame()
    {
        if (isPaused)
        {
            TogglePause();
        }
    }

    // 4. Sahneyi Yeniden Başlat
    public void OnClick_RestartScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // 5. Ana Menüye Dön
    public void OnClick_ReturnToLobby()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(lobbySceneName);
    }
}
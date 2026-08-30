using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

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
        pauseAction = new InputAction("Pause", binding: "<Keyboard>/escape");
    }

    private void OnEnable()
    {
        LobbyEventManager.OnMenuStateChanged += HandleMenuStateChanged;
        pauseAction.Enable();
        pauseAction.performed += ctx => TogglePause();
    }

    private void OnDisable()
    {
        LobbyEventManager.OnMenuStateChanged -= HandleMenuStateChanged;
        pauseAction.performed -= ctx => TogglePause();
        pauseAction.Disable();
    }

    private void Start()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    private void HandleMenuStateChanged(LobbyEventManager.LobbyState state)
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (playPanel != null) playPanel.SetActive(false);
        if (garagePanel != null) garagePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);

        switch (state)
        {
            case LobbyEventManager.LobbyState.MainMenu:
                if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
                break;
            case LobbyEventManager.LobbyState.Play:
                if (playPanel != null) playPanel.SetActive(true);
                break;
            case LobbyEventManager.LobbyState.Garage:
                if (garagePanel != null) garagePanel.SetActive(true);
                break;
            case LobbyEventManager.LobbyState.Settings:
                if (settingsPanel != null) settingsPanel.SetActive(true);
                break;
            case LobbyEventManager.LobbyState.Credits:
                if (creditsPanel != null) creditsPanel.SetActive(true);
                break;
        }
    }

    public void OnClick_MainMenu() => LobbyEventManager.OnMenuStateChanged?.Invoke(LobbyEventManager.LobbyState.MainMenu);
    public void OnClick_Play() => LobbyEventManager.OnMenuStateChanged?.Invoke(LobbyEventManager.LobbyState.Play);
    public void OnClick_Garage() => LobbyEventManager.OnMenuStateChanged?.Invoke(LobbyEventManager.LobbyState.Garage);
    public void OnClick_Settings() => LobbyEventManager.OnMenuStateChanged?.Invoke(LobbyEventManager.LobbyState.Settings);
    public void OnClick_Credits() => LobbyEventManager.OnMenuStateChanged?.Invoke(LobbyEventManager.LobbyState.Credits);
    public void OnClick_SelectColor(int colorIndex) => LobbyEventManager.OnColorSelected?.Invoke(colorIndex);

    public void OnClick_LoadGameScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName);
    }

    public void TogglePause()
    {
        // Optimizasyon: Eğer pause menüsü (yarışta değilsek) yoksa boşuna hesaplama yapma
        if (pausePanel == null) return;

        isPaused = !isPaused;
        pausePanel.SetActive(isPaused);

        // Zamanı dondur veya çöz
        Time.timeScale = isPaused ? 0f : 1f;
    }

    public void OnClick_ResumeGame()
    {
        if (isPaused) TogglePause();
    }

    public void OnClick_RestartScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnClick_ReturnToLobby()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(lobbySceneName);
    }
}
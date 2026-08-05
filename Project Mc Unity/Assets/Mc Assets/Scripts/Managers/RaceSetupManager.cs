using UnityEngine;
using TMPro; // TextMeshPro kullanıyorsan bu şart

public class RaceSetupManager : MonoBehaviour
{
    [Header("UI Referansları")]
    [SerializeField] private TextMeshProUGUI lapsText;
    [SerializeField] private TextMeshProUGUI carsText;

    private int currentLaps;
    private int currentCars;

    private void OnEnable()
    {
        // Panel her açıldığında kayıtlı verileri oku (Eğer yoksa Tur:1, Araç:2 olarak başla)
        currentLaps = PlayerPrefs.GetInt("Race_Laps", 1);
        currentCars = PlayerPrefs.GetInt("Race_Cars", 2);
        UpdateUI();
    }

    // Tur artırma/azaltma butonlarına bağlanacak (Örn: +1 veya -1 gönderecek)
    public void ChangeLaps(int amount)
    {
        currentLaps += amount;
        currentLaps = Mathf.Clamp(currentLaps, 1, 5); // En az 1, en fazla 5 tur
        PlayerPrefs.SetInt("Race_Laps", currentLaps);
        PlayerPrefs.Save();
        UpdateUI();
    }

    // Araç artırma/azaltma butonlarına bağlanacak (Örn: +1 veya -1 gönderecek)
    public void ChangeCars(int amount)
    {
        currentCars += amount;
        currentCars = Mathf.Clamp(currentCars, 2, 8); // En az 2, en fazla 8 araç
        PlayerPrefs.SetInt("Race_Cars", currentCars);
        PlayerPrefs.Save();
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (lapsText != null) lapsText.text = currentLaps.ToString();
        if (carsText != null) carsText.text = currentCars.ToString();
    }
}
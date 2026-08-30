using UnityEngine;
using TMPro;

public class RaceSetupManager : MonoBehaviour
{
    [Header("UI Referansları")]
    [SerializeField] private TextMeshProUGUI lapsText;
    [SerializeField] private TextMeshProUGUI carsText;

    private int currentLaps;
    private int currentCars;

    private void OnEnable()
    {
        currentLaps = PlayerPrefs.GetInt("Race_Laps", 1);
        currentCars = PlayerPrefs.GetInt("Race_Cars", 2);
        UpdateUI();
    }

    public void ChangeLaps(int amount)
    {
        currentLaps += amount;
        currentLaps = Mathf.Clamp(currentLaps, 1, 5);
        PlayerPrefs.SetInt("Race_Laps", currentLaps);
        PlayerPrefs.Save();
        UpdateUI();
    }

    public void ChangeCars(int amount)
    {
        currentCars += amount;
        currentCars = Mathf.Clamp(currentCars, 2, 8);
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
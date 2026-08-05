using UnityEngine;
using TMPro;
using System.Collections;

public class FPSCounter : MonoBehaviour
{
    [Header("UI Referansı")]
    [SerializeField] private TextMeshProUGUI fpsText;

    [Header("Ayarlar")]
    [SerializeField] private float updateInterval = 0.5f; // Saniyede 2 kez günceller

    private static FPSCounter instance;

    private void Awake()
    {
        // Sahneye tekrar dönüldüğünde kopyaların oluşmasını engelliyoruz
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Performansı korumak için Update yerine Coroutine başlatıyoruz
        if (fpsText != null)
        {
            StartCoroutine(CalculateFPS());
        }
    }

    private IEnumerator CalculateFPS()
    {
        // Bekleme süresini önbelleğe alarak ekstra bellek tüketimini önlüyoruz
        WaitForSeconds wait = new WaitForSeconds(updateInterval);

        while (true)
        {
            // O anki karenin çizim süresi üzerinden FPS hesabı (Oyun dursa bile doğru sayar)
            float currentFps = 1f / Time.unscaledDeltaTime;

            // "FPS: " + currentFps gibi string birleştirme işlemleri çöp (Garbage) yaratır. 
            // Bunun yerine TMP'nin formatlı SetText özelliğini kullanıyoruz.
            fpsText.SetText("FPS: {0}", Mathf.RoundToInt(currentFps));

            yield return wait;
        }
    }
}
using System;
using System.Collections;
using UnityEngine;
using TMPro;

public class CountdownManager : MonoBehaviour
{
    // Tur süresi ve kontrollerin kilidini açan Event (Hiç dokunulmadı)
    public static event Action OnCountdownFinished;

    [Header("UI Ayarları")]
    [SerializeField] private TextMeshProUGUI countdownText;

    [Header("Başlangıç Işıkları (Lego Modülü)")]
    [Tooltip("Pistteki başlangıç ışıklarının (Renderer bileşenine sahip objelerin) listesi.")]
    [SerializeField] private Renderer[] startLights;

    [Tooltip("Geri sayım (3-2-1) sırasında yanacak Kırmızı Materyal")]
    [SerializeField] private Material redLightMaterial;

    [Tooltip("Başla! anında yanacak Yeşil Materyal")]
    [SerializeField] private Material greenLightMaterial;

    public void StartCountdown()
    {
        countdownText.gameObject.SetActive(true);

        // Yarış başlamadan hemen önce sahnede kaç ışık varsa hepsini kırmızıya çevir
        SetAllLightsMaterial(redLightMaterial);

        StartCoroutine(CountdownRoutine());
    }

    private IEnumerator CountdownRoutine()
    {
        int count = 3;
        while (count > 0)
        {
            countdownText.text = count.ToString();
            yield return new WaitForSeconds(1f);
            count--;
        }

        countdownText.text = "START!";

        // Geri sayım bittiği an bütün ışıkları aynı anda yeşile (Go!) çevir
        SetAllLightsMaterial(greenLightMaterial);

        // Araçların kilidini açan yayın (Dokunulmadı)
        OnCountdownFinished?.Invoke();

        yield return new WaitForSeconds(1f);
        countdownText.gameObject.SetActive(false);
    }

    /// <summary>
    /// Işıkların sadece 2. materyalini (İç Işık Rengi) değiştiren güvenli metod
    /// </summary>
    private void SetAllLightsMaterial(Material newMaterial)
    {
        // Eğer ışık eklenmemişse veya materyal atanmamışsa hata vermemesi için kalkan
        if (startLights == null || startLights.Length == 0 || newMaterial == null) return;

        foreach (Renderer lightRenderer in startLights)
        {
            if (lightRenderer != null)
            {
                // Unity'de objenin çoklu materyallerinden sadece birini değiştirmek için dizinin kopyası alınır
                Material[] mats = lightRenderer.materials;

                // 0: Dış Demir, 1: İç Işık. Bu yüzden uzunluk 1'den büyük mü diye kontrol ediyoruz
                if (mats.Length > 1)
                {
                    mats[1] = newMaterial;

                    // Değiştirilmiş diziyi tekrar objeye atıyoruz
                    lightRenderer.materials = mats;
                }
            }
        }
    }
}
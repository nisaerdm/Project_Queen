using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CinematicIntroManager : MonoBehaviour
{
    public static event Action OnIntroFinished;

    [Header("Sinematik Ayarlar")]
    [Tooltip("Sırasıyla aktif edilecek Cinemachine Sanal Kamera objeleri")]
    [SerializeField] private GameObject[] cinematicCameras;

    [Tooltip("Kameranın ekranda kalacağı TOPLAM süre (Saniye)")]
    [SerializeField] private float shotDuration = 3f;

    [Tooltip("Ekranın kararma ve açılma hızı (Saniye)")]
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("UI Ayarları")]
    [Tooltip("Ekranı kaplayan siyah UI Image")]
    [SerializeField] private Image fadeImage;

    public void StartIntro()
    {
        if (cinematicCameras == null || cinematicCameras.Length == 0)
        {
            OnIntroFinished?.Invoke();
            return;
        }
        StartCoroutine(IntroRoutine());
    }

    private IEnumerator IntroRoutine()
    {
        // 1. BAŞLANGIÇ: Ekran tamamen saydam (Alpha 0). Oyun ilk açıldığında anlık olarak araç görünür.
        fadeImage.color = new Color(0, 0, 0, 0f);
        fadeImage.gameObject.SetActive(true);

        // İntro kameralarına geçmeden önce ekranı yumuşakça siyah yapıyoruz
        fadeImage.DOFade(1f, fadeDuration);
        yield return new WaitForSeconds(fadeDuration);

        // Tüm kameraları kapalı duruma getiriyoruz
        foreach (var cam in cinematicCameras)
        {
            if (cam != null) cam.SetActive(false);
        }

        // Zamanlama Matematiği: Bekleme süresinden kararma süresini çıkarıyoruz.
        // Böylece kararma işlemi tam olarak shotDuration'ın son anlarında başlar.
        float waitTime = Mathf.Max(0, shotDuration - fadeDuration);

        // 2. DÖNGÜ: Kameraları sırayla gez
        for (int i = 0; i < cinematicCameras.Length; i++)
        {
            if (cinematicCameras[i] == null) continue;

            // Kameraya ışınlan (Ekran şu an simsiyah olduğu için geçiş görünmez)
            cinematicCameras[i].SetActive(true);
            if (i > 0 && cinematicCameras[i - 1] != null)
            {
                cinematicCameras[i - 1].SetActive(false);
            }

            // Ekranı Aydınlat (Siyah -> Saydam)
            fadeImage.DOFade(0f, fadeDuration);

            // Kameranın aktif kalacağı süre kadar bekle (Kararma süresi çıkarılmış haliyle)
            yield return new WaitForSeconds(waitTime);

            // Ekranı Karart (Saydam -> Siyah) - Tam "Shot Duration" bitmeye yaklaşırken tetiklenir!
            fadeImage.DOFade(1f, fadeDuration);

            // Kararmanın tamamen bitmesini bekle, böylece diğer kameraya geçerken ekran tamamen siyah olur
            yield return new WaitForSeconds(fadeDuration);
        }

        // 3. BİTİŞ: Son intro kamerasını kapat, oyuncu kamerasına (araca) dön
        if (cinematicCameras.Length > 0 && cinematicCameras[cinematicCameras.Length - 1] != null)
        {
            cinematicCameras[cinematicCameras.Length - 1].SetActive(false);
        }

        // Arabanın arkasındayken ekranı son kez aydınlat
        fadeImage.DOFade(0f, fadeDuration);
        yield return new WaitForSeconds(fadeDuration);

        // UI Image tıklamaları engellemesin diye tamamen kapat
        fadeImage.gameObject.SetActive(false);

        // Geri sayımın (3-2-1) başlaması için GameManager'a sinyal gönder!
        OnIntroFinished?.Invoke();
    }
}
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
// Sadece Yeni Input Sistemini kullanıyoruz
using UnityEngine.InputSystem;

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

    private Coroutine introCoroutine;
    private bool isIntroPlaying = false;

    public void StartIntro()
    {
        if (cinematicCameras == null || cinematicCameras.Length == 0)
        {
            OnIntroFinished?.Invoke();
            return;
        }

        isIntroPlaying = true;
        introCoroutine = StartCoroutine(IntroRoutine());
    }

    private void Update()
    {
        // YENİLİK: Sadece Yeni Input Sistemi Kullanılıyor
        if (isIntroPlaying)
        {
            // 1. Ekrana Dokunma Kontrolü (Mobil İçin)
            bool isScreenTouched = Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame;

            // 2. Herhangi Bir Tuşa Basılma Kontrolü (PC/Editör İçin)
            bool isAnyKeyPressed = Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame;

            // Eğer ekrana dokunulduysa VEYA bir tuşa basıldıysa introyu atla
            if (isScreenTouched || isAnyKeyPressed)
            {
                SkipIntro();
            }
        }
    }

    private void SkipIntro()
    {
        // 1. Zaten atlanmışsa tekrar atlama
        if (!isIntroPlaying) return;
        isIntroPlaying = false;

        // 2. Devam eden döngüyü (Coroutine) durdur
        if (introCoroutine != null)
        {
            StopCoroutine(introCoroutine);
        }

        // 3. Devam eden ekran kararması/açılması animasyonlarını (DOTween) anında öldür
        if (fadeImage != null)
        {
            fadeImage.DOKill();
            fadeImage.color = new Color(0, 0, 0, 0f);
            fadeImage.gameObject.SetActive(false);
        }

        // 4. Tüm sinematik kameraları kapat ki oyun ana kameraya (yarış kamerasına) dönsün
        foreach (var cam in cinematicCameras)
        {
            if (cam != null) cam.SetActive(false);
        }

        // 5. Intro'nun bittiğini sisteme (GameManager'a) bildir, yarış başlasın!
        OnIntroFinished?.Invoke();
    }

    private IEnumerator IntroRoutine()
    {
        fadeImage.color = new Color(0, 0, 0, 0f);
        fadeImage.gameObject.SetActive(true);

        fadeImage.DOFade(1f, fadeDuration);
        yield return new WaitForSeconds(fadeDuration);

        foreach (var cam in cinematicCameras)
        {
            if (cam != null) cam.SetActive(false);
        }

        float waitTime = Mathf.Max(0, shotDuration - fadeDuration);

        for (int i = 0; i < cinematicCameras.Length; i++)
        {
            if (cinematicCameras[i] == null) continue;

            cinematicCameras[i].SetActive(true);
            if (i > 0 && cinematicCameras[i - 1] != null)
            {
                cinematicCameras[i - 1].SetActive(false);
            }

            fadeImage.DOFade(0f, fadeDuration);
            yield return new WaitForSeconds(waitTime);
            fadeImage.DOFade(1f, fadeDuration);
            yield return new WaitForSeconds(fadeDuration);
        }

        if (cinematicCameras.Length > 0 && cinematicCameras[cinematicCameras.Length - 1] != null)
        {
            cinematicCameras[cinematicCameras.Length - 1].SetActive(false);
        }

        fadeImage.DOFade(0f, fadeDuration);
        yield return new WaitForSeconds(fadeDuration);
        fadeImage.gameObject.SetActive(false);

        isIntroPlaying = false;
        OnIntroFinished?.Invoke();
    }
}
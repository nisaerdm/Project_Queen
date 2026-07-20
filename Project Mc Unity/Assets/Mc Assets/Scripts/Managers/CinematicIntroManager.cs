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
        OnIntroFinished?.Invoke();
    }
}
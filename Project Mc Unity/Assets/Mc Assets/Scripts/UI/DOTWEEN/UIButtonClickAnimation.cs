using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class UIButtonClickAnimation : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Animasyon Ayarları")]
    [SerializeField] private float kuculmeOrani = 0.9f;
    [SerializeField] private float animasyonSuresi = 0.15f;

    private Vector3 orijinalBoyut;
    private Tween boyutTween;

    private void Awake()
    {
        orijinalBoyut = transform.localScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        boyutTween?.Kill(); // Eğer önceki animasyon bitmediyse iptal et
        boyutTween = transform.DOScale(orijinalBoyut * kuculmeOrani, animasyonSuresi)
            .SetEase(Ease.OutQuad)
            .SetLink(gameObject);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        boyutTween?.Kill();
        boyutTween = transform.DOScale(orijinalBoyut, animasyonSuresi)
            .SetEase(Ease.OutBack) // Tatlı bir sekme efekti verir
            .SetLink(gameObject);
    }
}
using UnityEngine;
using TMPro;
using System.Collections;

public class FPSCounter : MonoBehaviour
{
    [Header("UI Referansı")]
    [SerializeField] private TextMeshProUGUI fpsText;

    [Header("Ayarlar")]
    [SerializeField] private float updateInterval = 0.5f;

    private static FPSCounter instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            transform.SetParent(null); // Uyarı engelleme yaması
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
        if (fpsText != null) StartCoroutine(CalculateFPS());
    }

    private IEnumerator CalculateFPS()
    {
        WaitForSeconds wait = new WaitForSeconds(updateInterval);

        while (true)
        {
            float currentFps = 1f / Time.unscaledDeltaTime;
            fpsText.SetText("FPS: {0}", Mathf.RoundToInt(currentFps));
            yield return wait;
        }
    }
}
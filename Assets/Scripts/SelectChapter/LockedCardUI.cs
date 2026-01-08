using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LockedCardUI : MonoBehaviour
{
    [Header("Map Index (0=Map1, 1=Map2, 2=Map3)")]
    public int mapIndex = 0;

    [Header("References")]
    public GameObject lockIcon;
    public Image cardImage;
    public HoverCard hoverCard;
    public Button button;
    public RectTransform cardTransform;

    [Header("Locked Visual")]
    [Range(0f, 1f)] public float lockedAlpha = 0.35f;
    public Color lockedTint = Color.gray;

    [Header("Pulse Lock Icon")]
    public float pulseScale = 1.08f;
    public float pulseSpeed = 1.6f;

    [Header("Breathing Card (locked)")]
    public float cardBreathScale = 1.02f;
    public float cardBreathSpeed = 1.2f;

    CanvasGroup lockCanvas;
    Vector3 cardOriginScale;
    Coroutine pulseCo;

    void Awake()
    {
        if (lockIcon != null)
        {
            lockCanvas = lockIcon.GetComponent<CanvasGroup>();
            if (lockCanvas == null)
                lockCanvas = lockIcon.AddComponent<CanvasGroup>();
        }

        if (cardTransform != null)
            cardOriginScale = cardTransform.localScale;
    }

    void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        bool unlocked = GameManager.Instance == null
            ? (mapIndex == 0)
            : GameManager.Instance.IsMapUnlocked(mapIndex);

        // ===== ICON KHÓA =====
        if (lockIcon != null)
            lockIcon.SetActive(!unlocked);

        // ===== MÀU CARD =====
        if (cardImage != null)
        {
            if (!unlocked)
                cardImage.color = new Color(
                    lockedTint.r,
                    lockedTint.g,
                    lockedTint.b,
                    lockedAlpha
                );
            else
                cardImage.color = Color.white;
        }

        // ===== HOVER =====
        if (hoverCard != null)
        {
            hoverCard.enabled = unlocked;
            if (!unlocked) hoverCard.ForceReset();
        }

        // ===== BUTTON =====
        if (button != null)
            button.interactable = true; // vẫn cho click để rung

        // ===== ANIMATION =====
        if (!unlocked)
        {
            StartPulse();
        }
        else
        {
            StopPulse();
        }
    }

    void StartPulse()
    {
        if (pulseCo != null) StopCoroutine(pulseCo);
        pulseCo = StartCoroutine(PulseRoutine());
    }

    void StopPulse()
    {
        if (pulseCo != null) StopCoroutine(pulseCo);
        pulseCo = null;

        if (lockIcon != null)
            lockIcon.transform.localScale = Vector3.one;

        if (cardTransform != null)
            cardTransform.localScale = cardOriginScale;

        if (lockCanvas != null)
            lockCanvas.alpha = 1f;
    }

    IEnumerator PulseRoutine()
    {
        float t = 0f;

        while (true)
        {
            t += Time.unscaledDeltaTime;

            // ===== ICON PULSE =====
            if (lockIcon != null)
            {
                float s = 1f + Mathf.Sin(t * pulseSpeed) * 0.08f;
                lockIcon.transform.localScale = Vector3.one * s;

                if (lockCanvas != null)
                    lockCanvas.alpha = 0.75f + Mathf.Sin(t * pulseSpeed) * 0.25f;
            }

            // ===== CARD BREATH =====
            if (cardTransform != null)
            {
                float c = 1f + Mathf.Sin(t * cardBreathSpeed) * 0.02f;
                cardTransform.localScale = cardOriginScale * c;
            }

            yield return null;
        }
    }
}

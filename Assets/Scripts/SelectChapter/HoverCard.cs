using UnityEngine;
using UnityEngine.EventSystems;

public class HoverCard : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler
{
    public GameObject glow;
    public float hoverScale = 1.08f;

    Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
        if (glow) glow.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = originalScale * hoverScale;
        if (glow) glow.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ForceReset();
    }

    public void ForceReset()
    {
        transform.localScale = originalScale;
        if (glow) glow.SetActive(false);
    }
}

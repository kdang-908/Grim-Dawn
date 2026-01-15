using UnityEngine;

public class MenuParallax : MonoBehaviour
{
    [SerializeField] RectTransform target;   // BG RectTransform
    [SerializeField] float amount = 25f;     // độ lệch px (15–40 hợp lý)
    [SerializeField] float smooth = 6f;

    Vector2 startPos;

    void Awake()
    {
        if (target == null) target = GetComponent<RectTransform>();
        startPos = target.anchoredPosition;
    }

    void Update()
    {
        float nx = (Input.mousePosition.x / Screen.width) * 2f - 1f;  // -1..1
        float ny = (Input.mousePosition.y / Screen.height) * 2f - 1f;

        Vector2 offset = new Vector2(nx, ny) * amount;
        Vector2 desired = startPos + offset;

        target.anchoredPosition = Vector2.Lerp(target.anchoredPosition, desired, Time.unscaledDeltaTime * smooth);
    }
}

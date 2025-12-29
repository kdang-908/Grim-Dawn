using UnityEngine;
using UnityEngine.UI;

public class UIFireflies : MonoBehaviour
{
    [Header("Spawn")]
    [SerializeField] RectTransform area; // vùng spawn (BG RectTransform)
    [SerializeField] Image fireflyPrefab;
    [SerializeField] int count = 40;

    [Header("Motion")]
    [SerializeField] float followSpeed = 18f;   // tốc độ bám theo target (Lerp)
    [SerializeField] float wander = 20f;        // biên độ lượn
    [SerializeField] float driftSpeed = 0.35f;  // tốc độ đổi hướng (Perlin)

    [Header("Pulse (Blink)")]
    [SerializeField] float pulseSpeed = 1.2f; // tăng để nhấp nháy nhanh hơn
    [SerializeField, Range(0f, 1f)] float minAlpha = 0.15f;
    [SerializeField, Range(0f, 1f)] float maxAlpha = 0.65f;

    RectTransform[] flies;
    Image[] flyImgs;
    Vector2[] basePos;
    float[] seed;

    void Start()
    {
        if (area == null) area = (RectTransform)transform;

        flies = new RectTransform[count];
        flyImgs = new Image[count];
        basePos = new Vector2[count];
        seed = new float[count];

        for (int i = 0; i < count; i++)
        {
            var img = Instantiate(fireflyPrefab, area);
            img.raycastTarget = false;

            var rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);

            Vector2 p = RandomPointIn(area);
            rt.anchoredPosition = p;

            basePos[i] = p;
            seed[i] = Random.value * 1000f;

            float s = Random.Range(6f, 14f);
            rt.sizeDelta = new Vector2(s, s);

            flies[i] = rt;
            flyImgs[i] = img;
        }

        if (fireflyPrefab != null) fireflyPrefab.gameObject.SetActive(false);
    }

    void Update()
    {
        float t = Time.unscaledTime;

        for (int i = 0; i < count; i++)
        {
            float a = seed[i];

            // Lượn (target thay đổi theo thời gian)
            Vector2 off = new Vector2(
                Mathf.PerlinNoise(a, t * driftSpeed) * 2f - 1f,
                Mathf.PerlinNoise(a + 33.3f, t * driftSpeed) * 2f - 1f
            ) * wander;

            // Bám target (mượt, không bão hoà)
            float k = Mathf.Clamp01(Time.unscaledDeltaTime * followSpeed);
            flies[i].anchoredPosition = Vector2.Lerp(
                flies[i].anchoredPosition,
                basePos[i] + off,
                k
            );

            // Nhấp nháy (alpha)
            float n = Mathf.PerlinNoise(a + 99.9f, t * pulseSpeed);
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, n);

            var c = flyImgs[i].color;
            c.a = alpha;
            flyImgs[i].color = c;
        }
    }

    Vector2 RandomPointIn(RectTransform rt)
    {
        Vector2 size = rt.rect.size;
        return new Vector2(
            Random.Range(-size.x * 0.5f, size.x * 0.5f),
            Random.Range(-size.y * 0.5f, size.y * 0.5f)
        );
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHPHUD : MonoBehaviour
{
    public CharacterStats stats;

    public Image fill;
    public TMP_Text levelText;
    public TMP_Text hpText;

    [Header("Smooth HP")]
    public float smoothSpeed = 8f;        // càng lớn càng bám nhanh
    float shownFill = 1f;

    [Header("Low HP Color")]
    [Range(0f, 1f)] public float lowHpThreshold = 0.3f;
    public Color highColor = Color.green;
    public Color lowColor = Color.red;

    void Start()
    {
        if (stats != null) shownFill = stats.HPPercent;
    }

    void Update()
    {
        if (stats == null || fill == null) return;

        float targetFill = stats.HPPercent;

        // 1) HP tuột mượt
        shownFill = Mathf.MoveTowards(shownFill, targetFill, Time.deltaTime * smoothSpeed);
        fill.fillAmount = shownFill;

        // 2) Text
        if (levelText != null) levelText.text = $"Lv.{stats.level}";
        if (hpText != null) hpText.text = $"{stats.currentHP} / {stats.maxHP}";

        // 3) Đổi màu khi thấp (mượt luôn)
        float t = Mathf.InverseLerp(lowHpThreshold, 1f, targetFill); // thấp -> 0, cao -> 1
        fill.color = Color.Lerp(lowColor, highColor, t);
    }
}

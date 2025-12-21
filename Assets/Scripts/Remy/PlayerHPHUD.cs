using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHPHUD : MonoBehaviour
{
    [Header("Kết nối Stats")]
    public CharacterStats stats;

    [Header("UI Components")]
    public Image fill;
    public TMP_Text levelText;
    public TMP_Text hpText;

    [Header("Smooth HP")]
    public float smoothSpeed = 8f;        // Tốc độ thanh máu chạy
    float shownFill = 1f;

    [Header("Low HP Color")]
    [Range(0f, 1f)] public float lowHpThreshold = 0.3f; // Dưới 30% máu sẽ đỏ lòm
    public Color highColor = Color.green;
    public Color lowColor = Color.red;

    void Start()
    {
        // Khởi đầu gán thanh máu đầy luôn cho đỡ bị giật
        if (stats != null && stats.TotalMaxHP > 0)
        {
            shownFill = (float)stats.currentHP / stats.TotalMaxHP;
            if (fill != null) fill.fillAmount = shownFill;
        }
    }

    void Update()
    {
        if (stats == null || fill == null) return;

        
        // Lấy TotalMaxHP (Máu tổng) thay vì maxHP cũ
        float maxHP = stats.TotalMaxHP;
        float currentHP = stats.currentHP;

        // Tính phần trăm máu (Tránh lỗi chia cho 0)
        float targetFill = (maxHP > 0) ? currentHP / maxHP : 0;

        // 1) Hiệu ứng HP tuột mượt mà (Lerp)
        shownFill = Mathf.MoveTowards(shownFill, targetFill, Time.deltaTime * smoothSpeed);
        fill.fillAmount = shownFill;

        // 2) Cập nhật chữ số
        if (levelText != null) levelText.text = $"Lv.{stats.level}";

        // Hiển thị: Máu hiện tại / Máu tổng
        if (hpText != null) hpText.text = $"{currentHP} / {maxHP}";

        // 3) Đổi màu thanh máu (Xanh -> Đỏ khi máu thấp)
        // targetFill thấp -> màu đỏ, targetFill cao -> màu xanh
        float t = Mathf.InverseLerp(lowHpThreshold, 1f, targetFill);
        fill.color = Color.Lerp(lowColor, highColor, t);
    }
}
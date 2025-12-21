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
    public float smoothSpeed = 8f;
    float shownFill = 1f;

    [Header("Low HP Color")]
    [Range(0f, 1f)] public float lowHpThreshold = 0.3f;
    public Color highColor = Color.green;
    public Color lowColor = Color.red;

    void Start()
    {
        TryFindPlayer();
    }

    void Update()
    {
        // SỬA: Tìm lại player nếu mất reference
        if (stats == null)
        {
            TryFindPlayer();
            return;
        }

        if (fill == null) return;

        float targetFill = stats.HPPercent;

        // 1) HP tuột mượt
        shownFill = Mathf.MoveTowards(shownFill, targetFill, Time.deltaTime * smoothSpeed);
        fill.fillAmount = shownFill;

        // 2) Text
        if (levelText != null) levelText.text = $"Lv.{stats.level}";
        if (hpText != null) hpText.text = $"{stats.currentHP} / {stats.maxHP}";

        // 3) Đổi màu khi thấp
        float t = Mathf.InverseLerp(lowHpThreshold, 1f, targetFill);
        fill.color = Color.Lerp(lowColor, highColor, t);
    }

    // THÊM method riêng để tìm player
    void TryFindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            stats = player.GetComponent<CharacterStats>();

            if (stats != null)
            {
                shownFill = stats.HPPercent;
                Debug.Log($"[PlayerHPHUD] Tìm thấy player: {player.name}, HP: {stats.currentHP}/{stats.maxHP}");
            }
            else
            {
                Debug.LogWarning("[PlayerHPHUD] Player không có CharacterStats!");
            }
        }
        else
        {
            Debug.LogWarning("[PlayerHPHUD] Không tìm thấy player tag 'Player'!");
        }
    }
}

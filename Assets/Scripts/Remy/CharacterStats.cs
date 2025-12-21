using UnityEngine;
using TMPro;

public class CharacterStats : MonoBehaviour
{
    [Header("Identity")]
    public string characterName = "Niche";
    public int level = 12;

    [Header("CHỈ SỐ GỐC (Base Stats)")]
    // Đây là sức mạnh trần trụi của nhân vật, không bao giờ bị thay đổi
    public int baseATK = 120;
    public int baseDEF = 75;
    public int baseMaxHP = 1000;

    [Header("CHỈ SỐ TỪ ĐỒ (Equipment Stats)")]
    // Chỉ số cộng thêm từ đồ đạc, tự động tính toán
    public int equipATK = 0;
    public int equipDEF = 0;
    public int equipHP = 0;

    [Header("Combat Stats (Read Only)")]
    public int currentHP = 1000;
    public int energy = 60;

    [Header("UI Hiển thị")]
    public TextMeshProUGUI atkText;
    public TextMeshProUGUI defText;
    public TextMeshProUGUI hpText;

    // Tính tổng tự động: Gốc + Đồ
    public int TotalATK => baseATK + equipATK;
    public int TotalDEF => baseDEF + equipDEF;
    public int TotalMaxHP => baseMaxHP + equipHP;

    void Start()
    {
        currentHP = TotalMaxHP; // Vào game đầy máu
        UpdateUI();
    }

    public void AddBonus(int atk, int def, int hp)
    {
        equipATK += atk;
        equipDEF += def;
        equipHP += hp;

        // Tăng max HP thì hồi thêm máu cho nhân vật
        currentHP += hp;

        UpdateUI();
    }

    public void RemoveBonus(int atk, int def, int hp)
    {
        equipATK -= atk;
        equipDEF -= def;
        equipHP -= hp;

        // Đảm bảo không bị âm (đề phòng lỗi)
        if (equipATK < 0) equipATK = 0;
        if (equipDEF < 0) equipDEF = 0;

        // Nếu máu hiện tại lỡ cao hơn Max mới thì gọt bớt
        if (currentHP > TotalMaxHP) currentHP = TotalMaxHP;

        UpdateUI();
    }

    void UpdateUI()
    {
        // Hiển thị số TỔNG ra màn hình
        if (atkText != null) atkText.text = "ATK: " + TotalATK;
        if (defText != null) defText.text = "DEF: " + TotalDEF;
        if (hpText != null) hpText.text = "HP: " + TotalMaxHP;

        Debug.Log($"Stats: Base({baseATK}) + Equip({equipATK}) = {TotalATK}");
    }

    // Các hàm cũ giữ nguyên
    public void TakeDamage(int dmg)
    {
        currentHP -= dmg;
        if (currentHP < 0) currentHP = 0;
    }
    public void Heal(int amount)
    {
        currentHP += amount;
        if (currentHP > TotalMaxHP) currentHP = TotalMaxHP;
    }
}
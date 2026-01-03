using UnityEngine;
using UnityEngine.Events;

public class CharacterStats : MonoBehaviour
{
    [Header("Identity")]
    public string characterName = "Niche";
    public int level = 12;

    [Header("Combat Stats")]
    public int maxHP = 1000;

    public int atk = 120;
    public int def = 75;
    public int energy = 60;

    [Header("Chỉ số thực tế (Sau khi cộng đồ)")]
    public int maxHP_Total;
    public int atk_Total;
    public int def_Total;
    public int energy_Total;
    private int _currentHP;
    public int currentHP
    {
        get => _currentHP;
        set => _currentHP = Mathf.Clamp(value, 0, maxHP_Total);
    }

    [Header("Death")]
    public UnityEvent onDeath;
    bool isDead = false;

    PlayerDeathSound deathSound;

    public float HPPercent => maxHP_Total <= 0 ? 0 : (float)currentHP / maxHP_Total;

    private void Start()
    {
        UpdateFinalStats();
        currentHP = maxHP_Total;
    }

    public void UpdateFinalStats()
    {
        atk_Total = atk;
        def_Total = def;
        maxHP_Total = maxHP;
        energy_Total = energy;

        
        EquipmentManager em = FindFirstObjectByType<EquipmentManager>();

        if (em != null)
        {
            if (em.currentWeapon != null) AddBonus(em.currentWeapon);
            if (em.currentHelmet != null) AddBonus(em.currentHelmet);
            if (em.currentChest != null) AddBonus(em.currentChest);
        }

        if (currentHP > maxHP_Total) currentHP = maxHP_Total;
        currentHP = maxHP_Total;
        // Cập nhật lại giao diện 
        FindFirstObjectByType<CharacterStatsUI>()?.Refresh();
    }

    void AddBonus(WeaponData data)
    {
        atk_Total += data.bonusATK;
        def_Total += data.bonusDEF;
        maxHP_Total += data.bonusMaxHP;
        energy_Total += data.bonusEnergy;
    }

    public void TakeDamage(int dmg)
    {
        currentHP -= dmg;
        if (currentHP < 0)
        {
            currentHP = 0;
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        Debug.Log("[CharacterStats] Player ch?t");
        deathSound?.PlayDeathSound();
        onDeath?.Invoke();
    }

    public void Heal(int amount)
    {
        currentHP += amount;
        if (currentHP > maxHP_Total) currentHP = maxHP_Total;
    }
}

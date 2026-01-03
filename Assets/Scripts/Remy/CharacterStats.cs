using UnityEngine;
using UnityEngine.Events;

public class CharacterStats : MonoBehaviour
{
    [Header("Identity")]
    public string characterName = "Niche";
    public int level = 12;

    [Header("Base Combat Stats")]
    public int maxHP = 1000;
    public int atk = 120;
    public int def = 75;
    public int energy = 60;

    [Header("Final Stats (After Equipment)")]
    public int maxHP_Total;
    public int atk_Total;
    public int def_Total;
    public int energy_Total;

    [SerializeField] private int _currentHP;

    public int currentHP
    {
        get => _currentHP;
        set => _currentHP = Mathf.Clamp(value, 0, Mathf.Max(0, maxHP_Total));
    }

    [Header("Death")]
    public UnityEvent onDeath;
    [SerializeField] private bool isDead = false;

    private PlayerDeathSound deathSound;

    public float HPPercent => maxHP_Total <= 0 ? 0 : (float)currentHP / maxHP_Total;

    private void Awake()
    {
        // Lấy script âm thanh chết nếu có trên cùng GameObject
        deathSound = GetComponent<PlayerDeathSound>();
    }

    private void Start()
    {
        UpdateFinalStats();

        // Nếu mới vào game, set full máu
        currentHP = maxHP_Total;
        isDead = false;

        // Refresh UI nếu có
        FindFirstObjectByType<CharacterStatsUI>()?.Refresh();
    }

    public void UpdateFinalStats()
    {
        atk_Total = atk;
        def_Total = def;
        maxHP_Total = maxHP;
        energy_Total = energy;

        // Tìm equipment manager trong scene
        EquipmentManager em = FindFirstObjectByType<EquipmentManager>();

        if (em != null)
        {
            if (em.currentWeapon != null) AddBonus(em.currentWeapon);
            if (em.currentHelmet != null) AddBonus(em.currentHelmet);
            if (em.currentChest != null) AddBonus(em.currentChest);
        }
        currentHP = maxHP_Total;

        // Cập nhật UI
        FindFirstObjectByType<CharacterStatsUI>()?.Refresh();
        
    }

    private void AddBonus(WeaponData data)
    {
        atk_Total += data.bonusATK;
        def_Total += data.bonusDEF;
        maxHP_Total += data.bonusMaxHP;
        energy_Total += data.bonusEnergy;
    }

    public void TakeDamage(int dmg)
    {
        if (isDead) return;
        if (dmg <= 0) return;

        currentHP -= dmg;

        
        if (currentHP <= 0)
        {
            currentHP = 0;
            Die();
        }

        FindFirstObjectByType<CharacterStatsUI>()?.Refresh();
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("[CharacterStats] Player chết");

        // ✅ phát âm chết
        if (deathSound != null) deathSound.PlayDeathSound();

        // ✅ gọi event (để hiện DeathScreen)
        onDeath?.Invoke();
    }

    public void Heal(int amount)
    {
        if (isDead) return;
        if (amount <= 0) return;

        currentHP += amount;
        FindFirstObjectByType<CharacterStatsUI>()?.Refresh();
    }

    // (Optional) dùng khi revive
    public void ReviveFull()
    {
        isDead = false;
        currentHP = maxHP_Total;
        FindFirstObjectByType<CharacterStatsUI>()?.Refresh();
    }
}

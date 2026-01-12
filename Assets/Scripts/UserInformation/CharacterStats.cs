using UnityEngine;
using UnityEngine.Events;

public class CharacterStats : MonoBehaviour
{
    [Header("Identity")]
    public string characterName = "N";
    public int level = 1;

    [Header("Base Combat Stats (LEVEL UP CHỈ TĂNG Ở ĐÂY)")]
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
        deathSound = GetComponent<PlayerDeathSound>();
    }

    private void Start()
    {
        // Lần đầu vào scene: tính stat + full máu
        UpdateFinalStats(keepCurrentHP: false, keepHPPercent: false);
        currentHP = maxHP_Total;
        isDead = false;

        FindFirstObjectByType<CharacterStatsUI>()?.Refresh();
    }

    public void UpdateFinalStats(bool keepCurrentHP = true, bool keepHPPercent = true)
    {
        int oldMax = Mathf.Max(1, maxHP_Total);
        int oldHP = Mathf.Clamp(currentHP, 0, oldMax);
        float oldPercent = (oldMax <= 0) ? 1f : (float)oldHP / oldMax;

        // base
        atk_Total = atk;
        def_Total = def;
        maxHP_Total = maxHP;
        energy_Total = energy;

        // equipment
        EquipmentManager em = FindFirstObjectByType<EquipmentManager>(FindObjectsInactive.Include);
        if (em != null)
        {
            if (em.currentWeapon != null) AddBonus(em.currentWeapon, em.weaponUpgradeLevel);
            if (em.currentHelmet != null) AddBonus(em.currentHelmet, em.helmetUpgradeLevel);
            if (em.currentChest != null) AddBonus(em.currentChest, em.chestUpgradeLevel);
        }

        // giữ máu
        if (!keepCurrentHP && !keepHPPercent)
        {
            currentHP = maxHP_Total; // chỉ dùng khi spawn/respawn
        }
        else if (keepHPPercent)
        {
            int newHP = Mathf.RoundToInt(oldPercent * maxHP_Total);
            currentHP = Mathf.Clamp(newHP, 1, maxHP_Total);
        }
        else
        {
            currentHP = Mathf.Clamp(oldHP, 1, maxHP_Total);
        }

        FindFirstObjectByType<CharacterStatsUI>()?.Refresh();
    }


    private void AddBonus(WeaponData data, int level)
    {
        if (data == null) return;

        int lv = Mathf.Max(level, 1);

        atk_Total += data.GetATK(lv);
        def_Total += data.GetDEF(lv);
        maxHP_Total += data.GetMaxHP(lv);
        energy_Total += data.GetEnergy(lv);
    }

    public void TakeDamage(int dmg)
    {
        if (isDead) return;
        if (dmg <= 0) return;

        int before = currentHP;
        currentHP -= dmg;

        Debug.Log($"[{characterName}] TakeDamage {dmg} | HP: {before} -> {currentHP}");

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

        if (deathSound != null) deathSound.PlayDeathSound();
        onDeath?.Invoke();
    }

    public void Heal(int amount)
    {
        if (isDead) return;
        if (amount <= 0) return;

        currentHP += amount;
        FindFirstObjectByType<CharacterStatsUI>()?.Refresh();
    }

    public void ReviveFull()
    {
        isDead = false;
        currentHP = maxHP_Total;
        FindFirstObjectByType<CharacterStatsUI>()?.Refresh();
    }
}

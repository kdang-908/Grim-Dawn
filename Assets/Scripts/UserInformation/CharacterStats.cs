using UnityEngine;
using UnityEngine.Events;

public class CharacterStats : MonoBehaviour
{
    [Header("Identity")]
    public string characterName = "N";
    public int level = 12;

    [Header("Base Combat Stats")]
    public int maxHP = 10000;
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
        UpdateFinalStats();

        currentHP = maxHP_Total;
        isDead = false;

        FindFirstObjectByType<CharacterStatsUI>()?.Refresh();
    }

    public void UpdateFinalStats()
    {
        float hpRatio = (maxHP_Total > 0) ? (float)currentHP / maxHP_Total : 1f;
        // base
        atk_Total = atk;
        def_Total = def;
        maxHP_Total = maxHP;
        energy_Total = energy;

        // equipment
        EquipmentManager em = FindFirstObjectByType<EquipmentManager>(FindObjectsInactive.Include);

        if (em != null)
        {
            if (em.currentWeapon != null)
                AddBonus(em.currentWeapon, em.weaponUpgradeLevel);

            if (em.currentHelmet != null)
                AddBonus(em.currentHelmet, em.helmetUpgradeLevel);

            if (em.currentChest != null)
                AddBonus(em.currentChest, em.chestUpgradeLevel);
        }

        // game hiện tại đang luôn full máu khi cập nhật stat
        //currentHP = maxHP_Total;
        currentHP = Mathf.RoundToInt(maxHP_Total * hpRatio);
        currentHP = Mathf.Clamp(currentHP, 0, maxHP_Total);

        // 5. Cập nhật UI
        FindFirstObjectByType<CharacterStatsUI>()?.Refresh();

        FindFirstObjectByType<CharacterStatsUI>()?.Refresh();
        Debug.Log($"Updated Stats: ATK {atk_Total} | DEF {def_Total} | HP {maxHP_Total}");
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

        // DEBUG: xem nó đang trừ bao nhiêu
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

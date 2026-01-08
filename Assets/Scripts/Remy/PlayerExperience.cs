using UnityEngine;

public class PlayerExperience : MonoBehaviour
{
    [Header("Refs")]
    public CharacterStats stats;

    [Header("EXP Runtime")]
    public int currentExp = 0;
    public int expToNextLevel = 50;   // level 1 cần 50 exp

    // ===============================
    //  LEVEL UP: ÂM THANH + HIỆU ỨNG
    // ===============================
    [Header("Level Up FX")]
    public AudioSource audioSource;
    public AudioClip levelUpClip;
    [Range(0f, 1f)] public float levelUpVolume = 0.85f;

    [Tooltip("Prefab hiệu ứng level up (aura, vòng sáng, v.v.)")]
    public GameObject levelUpVfxPrefab;
    public float levelUpVfxLifeTime = 2f;
    public Vector3 levelUpVfxOffset = new Vector3(0, 1.6f, 0); // lệch lên ngang người

    private void Awake()
    {
        if (stats == null)
            stats = GetComponent<CharacterStats>();

        if (stats == null)
            Debug.LogError("[PlayerExperience] Không tìm thấy CharacterStats trên Player!");

        // nếu chưa gán AudioSource thì thử tự tìm trên player
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// Cộng EXP khi giết quái
    /// </summary>
    public void AddExp(int amount)
    {
        if (amount <= 0) return;

        currentExp += amount;
        Debug.Log($"[EXP] +{amount} exp. Tổng: {currentExp}/{expToNextLevel}");

        while (currentExp >= expToNextLevel)
        {
            currentExp -= expToNextLevel;
            LevelUp();
        }
    }

    void LevelUp()
    {
        stats.level++;

        Debug.Log($"[LEVEL UP] Level mới: {stats.level}");

        // +20% chỉ số
        stats.maxHP = Mathf.RoundToInt(stats.maxHP * 1.2f);
        stats.atk = Mathf.RoundToInt(stats.atk * 1.2f);
        stats.def = Mathf.RoundToInt(stats.def * 1.2f);
        stats.energy = Mathf.RoundToInt(stats.energy * 1.2f);

        // hồi máu = full theo chỉ số mới
        stats.UpdateFinalStats();
        

        Debug.Log(
            $"[STATS UP] HP={stats.maxHP}, ATK={stats.atk}, DEF={stats.def}, ENERGY={stats.energy}"
        );

        // tăng EXP cần cho level tiếp theo (+20%)
        expToNextLevel = Mathf.RoundToInt(expToNextLevel * 1.2f);

        Debug.Log($"[EXP] Level {stats.level} → cần {expToNextLevel} exp cho cấp tiếp theo");

        // 🔥 GỌI HIỆU ỨNG & ÂM THANH LEVEL UP
        PlayLevelUpEffects();
    }

    void PlayLevelUpEffects()
    {
        // 🎧 Âm thanh
        if (audioSource != null && levelUpClip != null)
        {
            audioSource.PlayOneShot(levelUpClip, levelUpVolume);
        }
        else
        {
            Debug.LogWarning("[PlayerExperience] Chưa gán AudioSource hoặc LevelUpClip!");
        }

        // ✨ VFX
        if (levelUpVfxPrefab != null)
        {
            Vector3 pos = transform.position + levelUpVfxOffset;

            GameObject vfx = Instantiate(levelUpVfxPrefab, pos, Quaternion.identity);
            vfx.transform.SetParent(transform); // bám theo player

            if (levelUpVfxLifeTime > 0f)
                Destroy(vfx, levelUpVfxLifeTime);
        }
        else
        {
            Debug.LogWarning("[PlayerExperience] Chưa gán LevelUpVfxPrefab!");
        }
    }
}

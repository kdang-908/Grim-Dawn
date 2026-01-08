using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PotionManager : MonoBehaviour
{
    [Header("Target (runtime)")]
    [SerializeField] private CharacterStats activeCharacter;

    public static PotionManager Instance;

    [Header("UI")]
    [SerializeField] private Image potionIcon;
    [SerializeField] private TMP_Text potionCountText;
    [SerializeField] private TMP_Text cooldownText;

    [Header("Potion Settings")]
    [SerializeField] private int potions = 5;
    [SerializeField] private int healAmount = 250;
    [SerializeField] private KeyCode healKey = KeyCode.H;

    [Header("Cooldown")]
    [SerializeField] private float healCooldown = 5f;
    private float cooldownTimer = 0f;
    private bool isOnCooldown = false;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip healClip;
    [Range(0f, 1f)][SerializeField] private float healVolume = 0.7f;

    [Header("Visual")]
    [Range(0f, 1f)][SerializeField] private float emptyAlpha = 0.35f;

    [Header("Heal VFX")]
    [SerializeField] private GameObject healVfxPrefab;
    [SerializeField] private float healVfxLifeTime = 2f;
    [SerializeField] private Vector3 healVfxOffset = new Vector3(0, 1.2f, 0);

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ✅ GIỮ LẠI QUA SCENE
        }
        else
        {
            Destroy(gameObject);
            return; // ✅ QUAN TRỌNG: tránh chạy tiếp
        }

        UpdateUI();
    }

    void Update()
    {
        // auto bind lại player khi qua scene mới
        if (activeCharacter == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                activeCharacter = player.GetComponent<CharacterStats>() ?? player.GetComponentInChildren<CharacterStats>(true);
        }

        UpdateCooldown();

        if (Input.GetKeyDown(healKey))
            TryHeal();
    }

    public void RegisterCharacter(CharacterStats stats)
    {
        activeCharacter = stats;
    }

    public int GetPotionCount() => potions;

    // ✅ Cho GameManager set lại potion sau khi load scene
    public void SetPotionCount(int value)
    {
        potions = Mathf.Max(0, value);
        UpdateUI();
    }

    public void TryHeal()
    {
        if (activeCharacter == null) return;
        if (isOnCooldown) return;
        if (potions <= 0) return;

        // bạn đang dùng maxHP hay maxHP_Total tùy CharacterStats của bạn
        int maxHp = (activeCharacter.maxHP_Total > 0) ? activeCharacter.maxHP_Total : activeCharacter.maxHP;
        if (activeCharacter.currentHP >= maxHp) return;

        potions--;
        activeCharacter.Heal(healAmount);

        if (audioSource != null && healClip != null)
            audioSource.PlayOneShot(healClip, healVolume);

        SpawnHealVfx();

        StartCooldown();
        UpdateUI();
    }

    void SpawnHealVfx()
    {
        if (healVfxPrefab == null || activeCharacter == null) return;

        Vector3 pos = activeCharacter.transform.position + healVfxOffset;
        GameObject vfx = Instantiate(healVfxPrefab, pos, Quaternion.identity);

        vfx.transform.SetParent(activeCharacter.transform);

        if (healVfxLifeTime > 0f)
            Destroy(vfx, healVfxLifeTime);
    }

    void StartCooldown()
    {
        isOnCooldown = true;
        cooldownTimer = healCooldown;

        if (cooldownText != null)
            cooldownText.gameObject.SetActive(true);
    }

    void UpdateCooldown()
    {
        if (!isOnCooldown) return;

        cooldownTimer -= Time.deltaTime;

        if (cooldownText != null)
            cooldownText.text = cooldownTimer.ToString("0.0");

        if (cooldownTimer <= 0f)
        {
            isOnCooldown = false;
            cooldownTimer = 0f;

            if (cooldownText != null)
                cooldownText.gameObject.SetActive(false);
        }
    }

    public void AddPotions(int amount)
    {
        if (amount <= 0) return;
        potions += amount;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (potionCountText != null)
            potionCountText.text = $"x{potions}";

        if (potionIcon != null)
        {
            var c = potionIcon.color;
            c.a = (potions > 0) ? 1f : emptyAlpha;
            potionIcon.color = c;
        }
    }
}

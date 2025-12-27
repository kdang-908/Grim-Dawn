using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PotionManager : MonoBehaviour
{
    [Header("Target (runtime)")]
    [SerializeField] private CharacterStats activeCharacter;

    [Header("UI")]
    [SerializeField] private Image potionIcon;
    [SerializeField] private TMP_Text potionCountText;
    [SerializeField] private TMP_Text cooldownText;   // ? text hi?n th? cooldown

    [Header("Potion Settings")]
    [SerializeField] private int potions = 5;
    [SerializeField] private int healAmount = 250;
    [SerializeField] private KeyCode healKey = KeyCode.H;

    [Header("Cooldown")]
    [SerializeField] private float healCooldown = 5f; // ? delay dùng l?i
    private float cooldownTimer = 0f;
    private bool isOnCooldown = false;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip healClip;
    [Range(0f, 1f)][SerializeField] private float healVolume = 0.7f;

    [Header("Visual")]
    [Range(0f, 1f)][SerializeField] private float emptyAlpha = 0.35f;

    void Awake()
    {
        UpdateUI();
    }

    void Update()
    {
        // Tìm player n?u ch?a có
        if (activeCharacter == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                activeCharacter = player.GetComponent<CharacterStats>();
        }

        // ? Update cooldown
        UpdateCooldown();

        if (Input.GetKeyDown(healKey))
            TryHeal();
    }

    public void RegisterCharacter(CharacterStats stats)
    {
        activeCharacter = stats;
    }

    public void TryHeal()
    {
        if (activeCharacter == null) return;
        if (isOnCooldown) return;
        if (potions <= 0) return;
        if (activeCharacter.currentHP >= activeCharacter.maxHP) return;

        potions--;
        activeCharacter.Heal(healAmount);

        if (audioSource != null && healClip != null)
            audioSource.PlayOneShot(healClip, healVolume);

        StartCooldown();
        UpdateUI();
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

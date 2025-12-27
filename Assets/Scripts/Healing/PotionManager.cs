using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PotionManager : MonoBehaviour
{
    [Header("Target (runtime)")]
    [SerializeField] private CharacterStats activeCharacter;   // nhân v?t ?ang ch?i

    [Header("UI")]
    [SerializeField] private Image potionIcon;
    [SerializeField] private TMP_Text potionCountText;

    [Header("Potion Settings")]
    [SerializeField] private int potions = 5;
    [SerializeField] private int healAmount = 250;
    [SerializeField] private KeyCode healKey = KeyCode.H;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;          // 2D audio
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
        if (Input.GetKeyDown(healKey))
            TryHeal();
    }

    /// <summary>
    /// G?i sau khi spawn nhân v?t ???c ch?n.
    /// </summary>
    public void RegisterCharacter(CharacterStats stats)
    {
        activeCharacter = stats;
    }

    public void TryHeal()
    {
        if (activeCharacter == null) return;

        if (potions <= 0) return;
        if (activeCharacter.currentHP >= activeCharacter.maxHP) return;

        potions--;
        activeCharacter.Heal(healAmount);

        if (audioSource != null && healClip != null)
            audioSource.PlayOneShot(healClip, healVolume);

        UpdateUI();
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

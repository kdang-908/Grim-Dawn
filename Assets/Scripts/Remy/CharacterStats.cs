using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    [Header("Identity")]
    public string characterName = "Niche";
    public int level = 12;

    [Header("Combat Stats")]
    public int maxHP = 1000;
    public int currentHP = 1000;

    public int atk = 120;
    public int def = 75;
    public int energy = 60;

    public float HPPercent => maxHP <= 0 ? 0 : (float)currentHP / maxHP;

    public void TakeDamage(int dmg)
    {
        currentHP -= dmg;
        if (currentHP < 0) currentHP = 0;
    }

    public void Heal(int amount)
    {
        currentHP += amount;
        if (currentHP > maxHP) currentHP = maxHP;
    }
}

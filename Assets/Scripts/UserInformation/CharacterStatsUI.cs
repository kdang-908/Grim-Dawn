using UnityEngine;
using TMPro;

public class CharacterStatsUI : MonoBehaviour
{
    [Header("Texts")]
    public TMP_Text txtName;
    public TMP_Text txtLevel;
    public TMP_Text txtHP;
    public TMP_Text txtATK;
    public TMP_Text txtDEF;
    public TMP_Text txtEnergy;

    [Header("Data")]
    public HeroStats hero; // kéo object có HeroStats vào

    void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (hero == null) return;

        txtName.text = hero.heroName;
        txtLevel.text = "Level: " + hero.level;
        txtHP.text = "HP: " + hero.hp;          // ✅ CHỈ 1 SỐ
        txtATK.text = "ATK: " + hero.atk;
        txtDEF.text = "DEF: " + hero.def;
        txtEnergy.text = "Energy: " + hero.energy;
    }
}

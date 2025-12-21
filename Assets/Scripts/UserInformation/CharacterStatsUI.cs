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
    public CharacterStats stats; // kéo Player (hoặc hero object) có CharacterStats vào

    void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (stats == null) return;

        txtName.text = stats.characterName;
        txtLevel.text = "Level: " + stats.level;

        // Hiện kiểu panel (1 số) hoặc kiểu combat (current/max) đều được:
        txtHP.text = "HP: " + stats.TotalMaxHP;
        txtATK.text = "ATK: " + stats.TotalATK;
        txtDEF.text = "DEF: " + stats.TotalDEF;
        txtEnergy.text = "Energy: " + stats.energy;
    }
}

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

    [Header("Auto bind")]
    public string playerTag = "Player";
    public bool autoFindPlayer = true;

    [Header("Data (optional)")]
    public CharacterStats stats;

    void OnEnable()
    {
        if (autoFindPlayer && stats == null) TryBindPlayer();
        Refresh();
    }

    void Update()
    {
        if (autoFindPlayer && stats == null)
        {
            TryBindPlayer();
            Refresh();
            return;
        }
        Refresh();
    }

    public void TryBindPlayer()
    {
        var player = GameObject.FindGameObjectWithTag(playerTag);
        if (player == null) return;

        stats = player.GetComponent<CharacterStats>();
        if (stats == null) stats = player.GetComponentInChildren<CharacterStats>();
    }

    public void Refresh()
    {
        if (stats == null) return;

        if (txtName) txtName.text = $"Name: {stats.characterName}";
        if (txtLevel) txtLevel.text = $"Level: {stats.level}";

        // Hiển thị HP hiện tại 
        if (txtHP) txtHP.text = $"HP: {stats.maxHP_Total}";

        // HIỂN THỊ CHỈ SỐ ĐÃ CỘNG ĐỒ (_Total)
        if (txtATK) txtATK.text = $"ATK: {stats.atk_Total}";
        if (txtDEF) txtDEF.text = $"DEF: {stats.def_Total}";
        if (txtEnergy) txtEnergy.text = $"Energy: {stats.energy_Total}";
    }
}

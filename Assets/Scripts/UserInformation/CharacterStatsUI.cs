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

        // nếu muốn nhẹ hơn: chỉ Refresh khi mở inventory, hoặc khi HP đổi
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

        // ✅ NAME có label + đúng hoa thường
        if (txtName) txtName.text = $"Name: {stats.characterName}";

        // ✅ Level có label
        if (txtLevel) txtLevel.text = $"Level: {stats.level}";

        // ✅ Trong túi đồ chỉ hiện currentHP (1000)
        if (txtHP) txtHP.text = $"HP: {stats.currentHP}";

        // ✅ Các chỉ số có label
        if (txtATK) txtATK.text = $"ATK: {stats.atk}";
        if (txtDEF) txtDEF.text = $"DEF: {stats.def}";
        if (txtEnergy) txtEnergy.text = $"Energy: {stats.energy}";
    }
}

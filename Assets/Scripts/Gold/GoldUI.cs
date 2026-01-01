using UnityEngine;
using TMPro;

public class GoldUI : MonoBehaviour
{
    public TMP_Text goldText;

    void Start()
    {
        if (goldText == null)
            goldText = GetComponentInChildren<TMP_Text>();

        Refresh();
    }

    void Update()
    {
        Refresh();
    }

    void Refresh()
    {
        var gm = GameManager.Instance;
        if (gm == null || goldText == null) return;

        goldText.text = FormatGold(gm.gold);
    }

    string FormatGold(int value)
    {
        // 1 000 000 -> 1M
        if (value >= 1_000_000)
            return (value / 1_000_000) + "M";

        // 1 000 -> 100K, 15 000 -> 15K
        if (value >= 1_000)
            return (value / 1_000) + "K";

        return value.ToString();
    }
}

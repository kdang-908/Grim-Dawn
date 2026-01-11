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

        // 1111111 -> 1,111,111
        goldText.text = gm.gold.ToString("N0");
    }
}

using UnityEngine;

public class ApplySelectedCharacterData : MonoBehaviour
{
    public CharacterStats stats; // kéo CharacterStats (trên PlayerRuntime) vào
    public bool applyOnStart = true;

    void Start()
    {
        if (applyOnStart) Apply();
    }

    public void Apply()
    {
        if (stats == null) stats = GetComponent<CharacterStats>();
        if (stats == null) stats = GetComponentInChildren<CharacterStats>();
        if (stats == null)
        {
            Debug.LogWarning("[ApplySelectedCharacterData] stats not found");
            return;
        }

        // lấy tên đã nhập
        string savedName = PlayerPrefs.GetString("PLAYER_NAME", "").Trim();
        if (!string.IsNullOrEmpty(savedName))
            stats.characterName = savedName;

        // nếu bạn muốn lấy index nhân vật nữa:
        int idx = PlayerPrefs.GetInt("SELECTED_CHAR", 0);
        // idx dùng cho spawn male/female (nếu bạn cần)

        Debug.Log($"[ApplySelectedCharacterData] Applied name='{stats.characterName}', idx={idx}");
    }
}

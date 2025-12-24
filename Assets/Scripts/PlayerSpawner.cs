using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public string playerTag = "Player";

    void Start()
    {
        // Nếu đã có Player => khỏi spawn
        var exist = GameObject.FindGameObjectWithTag(playerTag);
        if (exist != null)
        {
            Debug.Log("[PlayerSpawner] Player already exists, skip spawn.");
            return;
        }

        // Lấy prefab từ GameManager (ưu tiên)
        GameObject prefab = null;
        if (GameManager.Instance != null)
            prefab = GameManager.Instance.GetSelectedPrefab();

        // fallback PlayerPrefs (nếu cần)
        if (prefab == null && GameManager.Instance != null && GameManager.Instance.gameplayPrefabs != null)
        {
            int gender = PlayerPrefs.GetInt("SelectedGender", 0);
            if (gender >= 0 && gender < GameManager.Instance.gameplayPrefabs.Length)
                prefab = GameManager.Instance.gameplayPrefabs[gender];
        }

        if (prefab == null)
        {
            Debug.LogError("[PlayerSpawner] Selected prefab NULL. Check GameManager.gameplayPrefabs");
            return;
        }

        var p = Instantiate(prefab, transform.position, transform.rotation);
        p.tag = playerTag;
        p.name = "PlayerRuntime";
    }
}

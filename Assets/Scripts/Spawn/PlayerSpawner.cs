using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("Điểm hồi sinh của Player. Nếu để trống sẽ dùng chính Transform của object này.")]
    public Transform playerSpawn;

    public string playerTag = "Player";

    [Header("Spawn FX")]
    public PlayerSpawnFX spawnFX;   // <= thêm dòng này

    void Awake()
    {
        if (playerSpawn == null)
            playerSpawn = transform;

        // auto lấy PlayerSpawnFX nếu chưa gán
        if (spawnFX == null)
            spawnFX = GetComponent<PlayerSpawnFX>();
    }

    void Start()
    {
        if (playerSpawn == null)
        {
            Debug.LogError("[PlayerSpawner] playerSpawn NULL!");
            return;
        }

        // Nếu đã có Player => chỉ đưa nó về đúng chỗ spawn
        var exist = GameObject.FindGameObjectWithTag(playerTag);
        if (exist != null)
        {
            Debug.Log("[PlayerSpawner] Found existing player → move to spawn");
            exist.transform.SetPositionAndRotation(playerSpawn.position, playerSpawn.rotation);
            BindPotionManager(exist);

            // 🔥 gọi hiệu ứng spawn
            if (spawnFX != null) spawnFX.PlaySpawnFX();
            return;
        }

        // --- Nếu chưa có Player => spawn mới ---

        GameObject prefab = null;
        if (GameManager.Instance != null)
            prefab = GameManager.Instance.GetSelectedPrefab();

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

        var p = Instantiate(prefab, playerSpawn.position, playerSpawn.rotation);
        p.tag = playerTag;
        p.name = "PlayerRuntime";

        BindPotionManager(p);

        // 🔥 gọi hiệu ứng spawn lần đầu
        if (spawnFX != null) spawnFX.PlaySpawnFX();
    }

    private void BindPotionManager(GameObject playerObj)
    {
        var potion = FindObjectOfType<PotionManager>();
        if (potion == null)
        {
            Debug.LogWarning("[PlayerSpawner] PotionManager not found in scene.");
            return;
        }

        var stats = playerObj.GetComponent<CharacterStats>();
        if (stats == null)
        {
            Debug.LogWarning("[PlayerSpawner] CharacterStats not found on spawned player.");
            return;
        }

        potion.RegisterCharacter(stats);
    }
}

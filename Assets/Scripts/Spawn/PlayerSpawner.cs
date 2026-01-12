using System.Collections;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("Điểm hồi sinh của Player. Nếu để trống sẽ dùng chính Transform của object này.")]
    public Transform playerSpawn;

    [Tooltip("Tag dùng để nhận diện Player trong scene.")]
    public string playerTag = "Player";

    [Header("Minimap")]
    [Tooltip("Kéo object có script MinimapBinder vào đây (khuyến nghị). Nếu để trống sẽ auto-find.")]
    public MinimapBinder minimapBinder;

    [Tooltip("Số lần thử bind minimap (phòng trường hợp minimap load trễ).")]
    public int bindTryCount = 60; // ~1s nếu 60fps

    [Tooltip("Khoảng cách giữa mỗi lần thử (giây).")]
    public float bindTryInterval = 0.05f;

    [Header("Spawn FX")]
    public PlayerSpawnFX spawnFX;

    void Awake()
    {
        if (playerSpawn == null) playerSpawn = transform;
        if (spawnFX == null) spawnFX = GetComponent<PlayerSpawnFX>();

        if (minimapBinder == null)
            minimapBinder = FindObjectOfType<MinimapBinder>(true);
    }

    void Start()
    {
        if (playerSpawn == null)
        {
            Debug.LogError("[PlayerSpawner] playerSpawn NULL!");
            return;
        }

        // 1) Tìm tất cả object có tag Player
        var players = GameObject.FindGameObjectsWithTag(playerTag);

        // 2) Nếu đã có player (hoặc lỡ có nhiều)
        if (players != null && players.Length > 0)
        {
            GameObject exist = PickMainPlayer(players);

            if (exist != null)
            {
                // ✅ ép identity cho chắc
                exist.name = "PlayerRuntime";
                exist.tag = playerTag;
            }

            DestroyDuplicates(players, exist);

            if (exist == null)
            {
                Debug.LogWarning("[PlayerSpawner] Found players but main player is NULL after cleanup.");
                return;
            }

            exist.transform.SetPositionAndRotation(playerSpawn.position, playerSpawn.rotation);

            BindPotionManager(exist);

            StartCoroutine(BindMinimapWhenReady(exist.transform));

            if (spawnFX != null) spawnFX.PlaySpawnFX();
            return;
        }

        // 3) Chưa có player => spawn mới
        GameObject prefab = GetSelectedPrefab();
        if (prefab == null)
        {
            Debug.LogError("[PlayerSpawner] Selected prefab NULL. Check GameManager/gameplayPrefabs.");
            return;
        }

        var p = Instantiate(prefab, playerSpawn.position, playerSpawn.rotation);
        p.name = "PlayerRuntime";
        p.tag = playerTag;

        BindPotionManager(p);

        StartCoroutine(BindMinimapWhenReady(p.transform));

        if (spawnFX != null) spawnFX.PlaySpawnFX();
    }

    // ✅ Bind minimap: retry đến khi sẵn sàng + tự recover nếu player bị Destroy
    IEnumerator BindMinimapWhenReady(Transform player)
    {
        yield return null;

        for (int i = 0; i < bindTryCount; i++)
        {
            // ✅ nếu player bị destroy -> tìm lại
            if (player == null)
            {
                var go = GameObject.FindGameObjectWithTag(playerTag);
                player = (go != null) ? go.transform : null;
            }

            if (player == null)
            {
                yield return new WaitForSeconds(bindTryInterval);
                continue;
            }

            if (minimapBinder == null)
                minimapBinder = FindObjectOfType<MinimapBinder>(true);

            if (minimapBinder != null)
            {
                minimapBinder.BindPlayer(player);

                bool okCam = (minimapBinder.minimapCamera != null && minimapBinder.minimapCamera.target == player);
                bool okArrow = (minimapBinder.minimapArrow != null && minimapBinder.minimapArrow.target == player);

                if (okCam && okArrow)
                {
                    Debug.Log("[PlayerSpawner] Minimap bind OK -> " + player.name);
                    yield break;
                }
            }

            yield return new WaitForSeconds(bindTryInterval);
        }

        Debug.LogWarning("[PlayerSpawner] Minimap bind FAILED (timeout). Check MinimapBinder exists & enabled in scene.");
    }

    // ===== Helpers =====

    private GameObject PickMainPlayer(GameObject[] players)
    {
        // ưu tiên đúng PlayerRuntime
        foreach (var go in players)
            if (go != null && go.name == "PlayerRuntime")
                return go;

        foreach (var go in players)
            if (go != null && go.name.Contains("PlayerRuntime"))
                return go;

        // ưu tiên có CharacterStats (player thật)
        foreach (var go in players)
            if (go != null && go.GetComponentInChildren<CharacterStats>() != null)
                return go;

        return (players != null && players.Length > 0) ? players[0] : null;
    }

    private void DestroyDuplicates(GameObject[] players, GameObject keep)
    {
        if (players == null) return;

        foreach (var go in players)
        {
            if (go != null && go != keep)
            {
                Debug.LogWarning("[PlayerSpawner] Duplicate player -> Destroy: " + go.name);
                Destroy(go);
            }
        }
    }

    private GameObject GetSelectedPrefab()
    {
        GameObject prefab = null;

        if (GameManager.Instance != null)
            prefab = GameManager.Instance.GetSelectedPrefab();

        if (prefab == null && GameManager.Instance != null && GameManager.Instance.gameplayPrefabs != null)
        {
            int gender = PlayerPrefs.GetInt("SelectedGender", 0);
            if (gender >= 0 && gender < GameManager.Instance.gameplayPrefabs.Length)
                prefab = GameManager.Instance.gameplayPrefabs[gender];
        }

        return prefab;
    }

    private void BindPotionManager(GameObject playerObj)
    {
        var potion = FindObjectOfType<PotionManager>(true);
        if (potion == null)
        {
            Debug.LogWarning("[PlayerSpawner] PotionManager not found in scene.");
            return;
        }

        var stats = playerObj.GetComponent<CharacterStats>();
        if (stats == null)
        {
            Debug.LogWarning("[PlayerSpawner] CharacterStats not found on player.");
            return;
        }

        potion.RegisterCharacter(stats);
    }
}

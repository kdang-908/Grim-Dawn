using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public Transform spawnPoint;

    void Start()
    {
        Debug.Log("[PlayerSpawner] Start");

        var gm = GameManager.Instance;
        if (gm == null)
        {
            Debug.LogError("[PlayerSpawner] GameManager.Instance == null");
            return;
        }

        var prefab = gm.GetSelectedPrefab();
        Debug.Log($"[PlayerSpawner] selectedCharacter={gm.selectedCharacter}, prefab={(prefab ? prefab.name : "null")}");

        if (prefab == null)
        {
            Debug.LogError("[PlayerSpawner] No selected prefab.");
            return;
        }

        // XÓA TẤT CẢ PLAYER CŨ (nếu có)
        GameObject[] oldPlayers = GameObject.FindGameObjectsWithTag("Player");
        foreach (var old in oldPlayers)
        {
            Debug.Log($"[PlayerSpawner] Xóa player cũ: {old.name}");
            Destroy(old);
        }

        // SPAWN PLAYER MỚI
        var p = spawnPoint != null ? spawnPoint : transform;
        GameObject player = Instantiate(prefab, p.position, p.rotation);
        player.name = "Player"; // đổi tên cho dễ nhận

        // GÁN TAG "Player"
        if (player.tag != "Player")
        {
            player.tag = "Player";
            Debug.Log($"[PlayerSpawner] Đã gán tag 'Player' cho {player.name}");
        }

        Debug.Log($"[PlayerSpawner] Player spawned: {player.name} tại {player.transform.position}");

        // Lưu reference
        PlayerRef.Instance = player.transform;

        // BẮT SKELETON TÌM LẠI PLAYER
        NotifyEnemies();
    }

    // Gọi tất cả skeleton tìm lại player
    void NotifyEnemies()
    {
        SkeletonAI[] skeletons = FindObjectsOfType<SkeletonAI>();
        foreach (var sk in skeletons)
        {
            sk.RefreshPlayer();
        }
        Debug.Log($"[PlayerSpawner] Đã thông báo {skeletons.Length} skeleton tìm lại player");
    }
}

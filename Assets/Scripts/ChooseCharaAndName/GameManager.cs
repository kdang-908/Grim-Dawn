using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Player data")]
    public int selectedCharacter;          // 0 = Remy, 1 = A03
    public string playerName = "Niche";    // tên mặc định, sẽ bị override khi nhập

    [Header("Gameplay prefabs (0=Remy, 1=A03)")]
    public GameObject[] gameplayPrefabs;

    [Header("Currency")]
    public int gold = 0;                   // tổng vàng hiện có

    [Header("Spawn")]
    public string playerTag = "Player";
    public string spawnPointName = "PlayerSpawn"; // đặt 1 empty trong Map tên PlayerSpawn

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Gọi ở màn hình chọn nhân vật
    public void SetPlayerData(int index, string name)
    {
        selectedCharacter = index;
        playerName = name;
        Debug.Log($"[GM] SetPlayerData selectedCharacter={selectedCharacter}, name={playerName}");
    }

    public GameObject GetSelectedPrefab()
    {
        if (gameplayPrefabs == null || gameplayPrefabs.Length == 0) return null;
        if (selectedCharacter < 0 || selectedCharacter >= gameplayPrefabs.Length) return null;
        return gameplayPrefabs[selectedCharacter];
    }

    // Gọi khi bấm nút Play ở CharacterSelection
    public void StartGameplay()
    {
        if (!SceneManager.GetSceneByName("Map").isLoaded)
        {
            SceneManager.LoadSceneAsync("Map", LoadSceneMode.Additive)
                .completed += (op) => OnMapLoaded();
        }
        else
        {
            OnMapLoaded();
        }
    }

    void OnMapLoaded()
    {
        Scene mapScene = SceneManager.GetSceneByName("Map");
        SceneManager.SetActiveScene(mapScene);

        Debug.Log($"[GM] Map loaded. selectedCharacter={selectedCharacter}, prefab={GetSelectedPrefab()?.name}");

        // Spawn player (đợi 1 frame để object trong Map sẵn sàng)
        StartCoroutine(SpawnPlayerNextFrame());

        // Unload scene chọn
        Scene selectionScene = SceneManager.GetSceneByName("CharacterSelection");
        if (selectionScene.isLoaded)
            SceneManager.UnloadSceneAsync(selectionScene);
    }

    IEnumerator SpawnPlayerNextFrame()
    {
        // đợi 1 frame để tất cả object trong Map spawn xong
        yield return null;

        // nếu đã có player rồi thì khỏi spawn nữa
        var exist = GameObject.FindGameObjectWithTag(playerTag);
        if (exist != null)
        {
            Debug.Log("[GM] Player already exists, skip spawn.");
            yield break;
        }

        var prefab = GetSelectedPrefab();
        if (prefab == null)
        {
            Debug.LogError("[GM] Selected prefab NULL. Check gameplayPrefabs in Inspector.");
            yield break;
        }

        Vector3 pos = Vector3.zero;
        Quaternion rot = Quaternion.identity;

        var sp = GameObject.Find(spawnPointName);
        if (sp != null)
        {
            pos = sp.transform.position;
            rot = sp.transform.rotation;
        }
        else
        {
            Debug.LogWarning($"[GM] SpawnPoint '{spawnPointName}' not found. Spawning at (0,0,0).");
        }

        // Spawn player
        var p = Instantiate(prefab, pos, rot);

        // Gán tên vào CharacterStats
        var stats = p.GetComponent<CharacterStats>();
        if (stats == null) stats = p.GetComponentInChildren<CharacterStats>();
        if (stats != null && !string.IsNullOrEmpty(playerName))
        {
            stats.characterName = playerName;
        }

        // Gắn weapon equipper cho EquipmentManager (nếu có)
        var em = FindFirstObjectByType<EquipmentManager>();
        if (em != null)
        {
            em.playerWeaponEquipper = p.GetComponentInChildren<WeaponEquipper>(true);
        }

        p.tag = playerTag;
        p.name = "PlayerRuntime";

        // Gán camera follow player nếu có component FollowPlayerCamera
        var cam = Camera.main;
        if (cam != null)
        {
            var follow = cam.GetComponent<FollowPlayerCamera>();
            if (follow != null)
                follow.target = p.transform;
        }

        Debug.Log($"[GM] Spawned player: {p.name} ({prefab.name}) | PlayerName='{playerName}'");
    }

    // =========================
    //  CURRENCY: GOLD
    // =========================

    // Cộng vàng
    public void AddGold(int amount)
    {
        if (amount <= 0) return;

        gold += amount;
        Debug.Log($"[GM] +{amount} gold. Total = {gold}");
    }

    // Trừ vàng (khi mua đồ)
    public bool SpendGold(int amount)
    {
        if (amount <= 0) return true;

        if (gold < amount)
        {
            Debug.Log($"[GM] Not enough gold. Have {gold}, need {amount}");
            return false;
        }

        gold -= amount;
        Debug.Log($"[GM] Spend {amount} gold. Left = {gold}");
        return true;
    }
}

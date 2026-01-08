using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Player data")]
    public int selectedCharacter;
    public string playerName = "Niche";

    [Header("Gameplay prefabs (0=Remy, 1=A03)")]
    public GameObject[] gameplayPrefabs;

    [Header("Currency")]
    public int gold = 0;

    [Header("Spawn")]
    public string playerTag = "Player";
    public string spawnPointName = "PlayerSpawn";

    // =========================
    // UNLOCK MAP PROGRESS (RAM ONLY - NOT SAVED)
    // =========================
    [Header("Chapter Progress (Runtime only)")]
    [Tooltip("0=Map1, 1=Map2, 2=Map3 (map cao nhất đã mở)")]
    public int maxUnlockedMap = 0;

    [Tooltip("Nếu true thì mỗi lần bấm Play sẽ reset về chỉ mở Map1")]
    public bool resetUnlockOnStart = true;

    [System.Serializable]
    public class PlayerSaveData
    {
        public int level;
        public int maxHP;
        public int currentHP;
        public int atk;
        public int def;
    }

    [Header("Saved Stats (debug)")]
    public PlayerSaveData playerData = new PlayerSaveData();
    public bool hasSavedData = false;

    // ✅ SAVE/LOAD POTION (runtime only - currently not persisted)
    [Header("Saved Potions")]
    public int savedPotions = 0;
    public bool hasSavedPotions = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // ✅ Runtime only: mỗi lần Play reset unlock nếu bạn muốn
            if (resetUnlockOnStart)
                maxUnlockedMap = 0; // chỉ mở Map1
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnEnable()
    {
        if (Instance == this)
            SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(ApplyAfterSceneLoaded());
    }

    IEnumerator ApplyAfterSceneLoaded()
    {
        yield return new WaitForEndOfFrame();
        yield return null;

        var player = GameObject.FindGameObjectWithTag(playerTag);
        if (player == null)
        {
            Debug.LogWarning("[GM] ApplyAfterSceneLoaded: Player not found");
            yield break;
        }

        var stats = player.GetComponent<CharacterStats>() ?? player.GetComponentInChildren<CharacterStats>(true);
        if (stats == null)
        {
            Debug.LogWarning("[GM] ApplyAfterSceneLoaded: CharacterStats not found");
            yield break;
        }

        // ✅ NAME
        if (!string.IsNullOrEmpty(playerName))
            stats.characterName = playerName;

        // ✅ STATS
        if (hasSavedData && playerData != null && playerData.maxHP > 0)
            LoadPlayer(stats);

        // ✅ POTION
        LoadPotions();

        // ✅ refresh UI nếu có
        FindFirstObjectByType<CharacterStatsUI>()?.Refresh();

        Debug.Log($"[GM] Applied | Scene={SceneManager.GetActiveScene().name} | HP={stats.currentHP}/{stats.maxHP_Total} | Potions={(PotionManager.Instance ? PotionManager.Instance.GetPotionCount() : -1)} | UnlockedMax={maxUnlockedMap}");
    }

    // =========================
    // SETUP
    // =========================
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

        StartCoroutine(SpawnPlayerNextFrame());

        Scene selectionScene = SceneManager.GetSceneByName("CharacterSelection");
        if (selectionScene.isLoaded)
            SceneManager.UnloadSceneAsync(selectionScene);
    }

    IEnumerator SpawnPlayerNextFrame()
    {
        yield return null;

        var exist = GameObject.FindGameObjectWithTag(playerTag);
        if (exist != null)
        {
            var statsExist = exist.GetComponent<CharacterStats>() ?? exist.GetComponentInChildren<CharacterStats>(true);
            if (statsExist != null)
            {
                if (!string.IsNullOrEmpty(playerName))
                    statsExist.characterName = playerName;

                if (hasSavedData && playerData != null && playerData.maxHP > 0)
                    LoadPlayer(statsExist);
            }

            LoadPotions();
            FindFirstObjectByType<CharacterStatsUI>()?.Refresh();
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

        var p = Instantiate(prefab, pos, rot);
        p.tag = playerTag;
        p.name = "PlayerRuntime";

        var stats = p.GetComponent<CharacterStats>() ?? p.GetComponentInChildren<CharacterStats>(true);
        if (stats != null)
        {
            if (!string.IsNullOrEmpty(playerName))
                stats.characterName = playerName;

            if (hasSavedData && playerData != null && playerData.maxHP > 0)
                LoadPlayer(stats);
        }

        LoadPotions();
        FindFirstObjectByType<CharacterStatsUI>()?.Refresh();
    }

    // =========================
    // GOLD
    // =========================
    public void AddGold(int amount)
    {
        if (amount <= 0) return;
        gold += amount;
    }

    public bool SpendGold(int amount)
    {
        if (amount <= 0) return true;
        if (gold < amount) return false;
        gold -= amount;
        return true;
    }

    // =========================
    // SAVE/LOAD STATS (runtime only)
    // =========================
    public void SavePlayer(CharacterStats stats)
    {
        if (stats == null)
        {
            Debug.LogWarning("[GM] SavePlayer: stats == null");
            return;
        }

        playerData.level = stats.level;
        playerData.maxHP = stats.maxHP_Total;
        playerData.currentHP = stats.currentHP;
        playerData.atk = stats.atk_Total;
        playerData.def = stats.def_Total;

        hasSavedData = true;

        Debug.Log($"[GM] SavePlayer: LV {playerData.level}, HP {playerData.currentHP}/{playerData.maxHP}");
    }

    public void LoadPlayer(CharacterStats stats)
    {
        if (stats == null) return;
        if (!hasSavedData || playerData == null || playerData.maxHP <= 0) return;

        stats.level = playerData.level;
        stats.maxHP_Total = playerData.maxHP;
        stats.currentHP = Mathf.Clamp(playerData.currentHP, 1, playerData.maxHP);
        stats.atk_Total = playerData.atk;
        stats.def_Total = playerData.def;

        Debug.Log($"[GM] LoadPlayer: LV {playerData.level}, HP {stats.currentHP}/{playerData.maxHP}");
    }

    // =========================
    // SAVE/LOAD POTIONS (runtime only)
    // =========================
    public void SavePotions()
    {
        if (PotionManager.Instance == null)
        {
            Debug.LogWarning("[GM] SavePotions: PotionManager.Instance NULL");
            return;
        }

        savedPotions = PotionManager.Instance.GetPotionCount();
        hasSavedPotions = true;

        Debug.Log($"[GM] SavePotions = {savedPotions}");
    }

    public void LoadPotions()
    {
        if (!hasSavedPotions) return;
        if (PotionManager.Instance == null)
        {
            Debug.LogWarning("[GM] LoadPotions: PotionManager.Instance NULL");
            return;
        }

        PotionManager.Instance.SetPotionCount(savedPotions);
        Debug.Log($"[GM] LoadPotions = {savedPotions}");
    }

    // =========================
    // UNLOCK MAP API (runtime only)
    // =========================
    public bool IsMapUnlocked(int mapIndex)
    {
        return mapIndex <= maxUnlockedMap;
    }

    public void UnlockMap(int mapIndex)
    {
        if (mapIndex <= maxUnlockedMap) return;

        maxUnlockedMap = mapIndex;
        Debug.Log($"[GM] UnlockMap (runtime) -> maxUnlockedMap = {maxUnlockedMap}");
    }

    public void ResetUnlock()
    {
        maxUnlockedMap = 0;
        Debug.Log("[GM] ResetUnlock (runtime) -> only Map1 unlocked");
    }
}

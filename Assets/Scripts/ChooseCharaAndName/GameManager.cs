using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int selectedCharacter; // 0 = Remy, 1 = A03
    public string playerName;

    [Header("Gameplay prefabs (0=Remy, 1=A03)")]
    public GameObject[] gameplayPrefabs;

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
        else Destroy(gameObject);
    }

    public void SetPlayerData(int index, string name)
    {
        selectedCharacter = index;
        playerName = name;
        Debug.Log($"[GM] SetPlayerData selectedCharacter={selectedCharacter}, name={playerName}");
    }

    public GameObject GetSelectedPrefab()
    {
        if (gameplayPrefabs == null) return null;
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
        yield return null;

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
        else Debug.LogWarning($"[GM] SpawnPoint '{spawnPointName}' not found. Spawning at (0,0,0).");

        var p = Instantiate(prefab, pos, rot);
        var em = FindFirstObjectByType<EquipmentManager>();
        if (em != null)
        {
            em.playerWeaponEquipper = p.GetComponentInChildren<WeaponEquipper>(true);
        }
        p.tag = playerTag;
        p.name = "PlayerRuntime";

        // ✅ gán camera follow player sau khi spawn
        var cam = Camera.main;
        if (cam != null)
        {
            var follow = cam.GetComponent<FollowPlayerCamera>();
            if (follow != null) follow.target = p.transform; // hoặc p.transform.Find("PlayerRoot")
        }

        Debug.Log($"[GM] Spawned player: {p.name} ({prefab.name}) | PlayerName='{playerName}'");
    }

}

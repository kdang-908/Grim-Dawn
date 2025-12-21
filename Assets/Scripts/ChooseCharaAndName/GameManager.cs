using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;


    public int selectedCharacter; // 0 = Male, 1 = Female
    public string playerName;
    [Header("Gameplay prefabs (0=Remy, 1=A03)")]
    public GameObject[] gameplayPrefabs;    

    void LateUpdate()
    {
        AudioListener[] listeners = FindObjectsOfType<AudioListener>();
        if (listeners.Length > 1)
        {
            Debug.Log("⚠ AudioListeners:");
            foreach (var l in listeners)
            {
                Debug.Log(l.gameObject.name + " | Enabled=" + l.enabled);
            }
        }
    }


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

    // ======================================
    // LƯU DỮ LIỆU NHÂN VẬT
    // ======================================
    public void SetPlayerData(int index, string name)
    {
        selectedCharacter = index;
        playerName = name;
        Debug.Log($"[GM] SetPlayerData selectedCharacter={selectedCharacter}, name={playerName}");
    }

    // ======================================
    // LOAD MAP + CHUYỂN GAMEPLAY + HIỆN NHÂN VẬT
    // CHỈ ĐƯỢC GỌI KHI NHẤN PLAY
    //
    public GameObject GetSelectedPrefab()
    {
        if (gameplayPrefabs == null) return null;
        if (selectedCharacter < 0 || selectedCharacter >= gameplayPrefabs.Length) return null;
        return gameplayPrefabs[selectedCharacter];
    }
    public void StartGameplay()
    {
        // Nếu Map chưa load → load additive
        if (!SceneManager.GetSceneByName("Map").isLoaded)
        {
            SceneManager.LoadSceneAsync("Map", LoadSceneMode.Additive)
                .completed += (op) =>
                {
                    OnMapLoaded();
                };
        }
        else
        {
            // Trường hợp Map đã load sẵn
            OnMapLoaded();
        }
    }

    // ======================================
    // MAP LOAD XONG
    // ======================================
    void OnMapLoaded()
    {
        Scene mapScene = SceneManager.GetSceneByName("Map");

        // Set Map làm scene active
        SceneManager.SetActiveScene(mapScene);
        //log để biết đã chọn ai
        Debug.Log($"[GM] Map loaded. selectedCharacter={selectedCharacter}, prefab={GetSelectedPrefab()?.name}");

        // Unload scene chọn nhân vật
        Scene selectionScene = SceneManager.GetSceneByName("CharacterSelection");
        if (selectionScene.isLoaded)
        {
            SceneManager.UnloadSceneAsync(selectionScene);
        }
    }

}

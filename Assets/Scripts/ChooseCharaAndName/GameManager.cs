using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int selectedCharacter;
    public string playerName;

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
    }

    // Gọi từ CharacterSelection khi bắt đầu, để load Map đằng sau
    public void LoadMapAdditive()
    {
        // Nếu Map chưa load thì mới load
        if (!SceneManager.GetSceneByName("Map").isLoaded)
        {
            SceneManager.LoadSceneAsync("Map", LoadSceneMode.Additive);
        }
    }

    // Gọi khi người chơi xác nhận chọn nhân vật
    public void GoToGameplay()
    {
        // Giả sử Map đã load additive rồi, bây giờ chỉ cần unload scene chọn nhân vật
        var currentScene = SceneManager.GetActiveScene();
        if (currentScene.name != "Map")
        {
            SceneManager.UnloadSceneAsync(currentScene);
            // Map sẽ trở thành scene active nếu chỉ còn nó
        }
    }
}

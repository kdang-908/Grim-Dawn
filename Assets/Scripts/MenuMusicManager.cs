using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuMusicManager : MonoBehaviour
{
    public static MenuMusicManager Instance;

    [SerializeField] private string[] allowedScenes = { "MainMenu", "CharacterSelection" };
    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        if (audioSource != null && !audioSource.isPlaying)
            audioSource.Play();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool allowed = false;
        for (int i = 0; i < allowedScenes.Length; i++)
        {
            if (scene.name == allowedScenes[i])
            {
                allowed = true;
                break;
            }
        }

        if (!allowed)
        {
            // Rời khỏi menu flow -> tắt nhạc (và huỷ luôn manager)
            Destroy(gameObject);
        }
        else
        {
            // Đảm bảo vẫn phát nếu quay lại
            if (audioSource != null && !audioSource.isPlaying)
                audioSource.Play();
        }
    }
}

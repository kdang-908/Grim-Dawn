using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] string characterSelectionScene = "CharacterSelection";

    public void StartGame()
    {
        Time.timeScale = 1f;              // tránh bị đứng do timescale = 0 từ scene trước
        SceneManager.LoadScene(characterSelectionScene);
    }

    public void QuitGame()
    {
        Application.Quit();              
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // dừng Play trong Editor
#endif
    }
}

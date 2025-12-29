using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadGameEndUIScene : MonoBehaviour
{
    void Start()
    {
        if (!SceneManager.GetSceneByName("GameEndUI").isLoaded)
        {
            SceneManager.LoadSceneAsync("GameEndUI", LoadSceneMode.Additive);
        }
    }
}

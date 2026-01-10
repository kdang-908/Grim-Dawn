using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameEndUIController : MonoBehaviour
{
    public static GameEndUIController Instance;

    [Header("UI")]
    public GameObject deathScreen;
    public GameObject victoryScreen;

    [Header("Timing")]
    [Tooltip("Delay trước khi hiện UI Death/Victory")]
    public float showDelay = 1.5f;

    [Tooltip("Giữ lại nếu sau này muốn auto load, hiện tại KHÔNG dùng")]
    public float autoLoadDelay = 2f;

    [Header("Scene Names")]
    [Tooltip("Tên scene khi Retry (nếu muốn ép về 1 scene cụ thể)")]
    public string retrySceneName = "Map";

    [Tooltip("Map kế tiếp thứ 1 (sau Map hiện tại)")]
    public string nextSceneName = "SceneMap2";

    [Tooltip("Map kế tiếp thứ 2 (sau SceneMap2)")]
    public string nextSceneName2 = "SceneMap3";

    [Header("End Game")]
    public GameObject endScreen;

    [Header("End Game Audio")]
    [SerializeField] private AudioSource endAudioSource;
    [SerializeField] private AudioClip endMusic;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        gameObject.SetActive(true);

        if (deathScreen != null) deathScreen.SetActive(false);
        if (victoryScreen != null) victoryScreen.SetActive(false);
    }

    private void Start()
    {
        StartCoroutine(WaitForPlayerAndHook());
    }

    IEnumerator WaitForPlayerAndHook()
    {
        GameObject player = null;
        while (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            yield return null;
        }

        CharacterStats stats = player.GetComponent<CharacterStats>()
                               ?? player.GetComponentInChildren<CharacterStats>(true);

        if (stats == null)
        {
            Debug.LogError("[GameEndUI] Player không có CharacterStats!");
            yield break;
        }

        // Hook death event
        stats.onDeath.AddListener(OnPlayerDeath);
        Debug.Log("[GameEndUI] Đã hook OnDeath");
    }

    // ===================== DEATH =====================
    public void OnPlayerDeath()
    {
        StartCoroutine(ShowDeathScreenDelay());
    }

    IEnumerator ShowDeathScreenDelay()
    {
        yield return new WaitForSecondsRealtime(showDelay);

        gameObject.SetActive(true);
        if (victoryScreen != null) victoryScreen.SetActive(false);
        if (deathScreen != null) deathScreen.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f;
    }

    // ===================== VICTORY =====================
    public void ShowVictory()
    {
        StartCoroutine(ShowVictoryDelay());
    }

    IEnumerator ShowVictoryDelay()
    {
        yield return new WaitForSecondsRealtime(showDelay);

        gameObject.SetActive(true);
        if (deathScreen != null) deathScreen.SetActive(false);
        if (victoryScreen != null) victoryScreen.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f;
    }

    // ===================== End Game =====================
    public void ShowEndGame()
    {
        Debug.Log("[GameEndUI] GAME COMPLETED - SHOW THE END");

        gameObject.SetActive(true);

        if (victoryScreen != null) victoryScreen.SetActive(false);
        if (deathScreen != null) deathScreen.SetActive(false);
        if (endScreen != null) endScreen.SetActive(true);

        // Unlock chuột
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Dừng game
        Time.timeScale = 0f;

        // Play end music
        if (endAudioSource != null && endMusic != null)
        {
            endAudioSource.clip = endMusic;
            endAudioSource.loop = false;
            endAudioSource.Play();
        }
    }

    /// <summary>
    /// Gán hàm này cho nút "Next" / "Continue" ở màn hình Victory
    /// - Nếu đang ở Map => qua SceneMap2 (và unlock Map2)
    /// - Nếu đang ở SceneMap2 => qua SceneMap3 (và unlock Map3)
    /// </summary>
    public void NextMap()
    {
        Debug.Log("[GameEndUI] Next Map (Button)");

        string current = SceneManager.GetActiveScene().name;

        var gm = GameManager.Instance;
        if (gm != null)
        {
            // ✅ Save stats
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                var stats = player.GetComponent<CharacterStats>()
                           ?? player.GetComponentInChildren<CharacterStats>(true);

                if (stats != null)
                {
                    gm.SavePlayer(stats); // LV/HP/ATK/DEF
                }
                else
                {
                    Debug.LogWarning("[GameEndUI] Không tìm thấy CharacterStats để Save");
                }
            }
            else
            {
                Debug.LogWarning("[GameEndUI] Không tìm thấy Player (tag Player) để Save");
            }

            // ✅ Save potions
            gm.SavePotions();

            // ✅ UNLOCK theo scene hiện tại
            // Quy ước của bạn: 0=Map1, 1=Map2, 2=Map3
            if (current == retrySceneName || current == "Map")
            {
                // thắng Map1 -> mở Map2
                gm.UnlockMap(1);
            }
            if (current == nextSceneName) // SceneMap2
            {
                // thắng Map2 -> mở Map3
                gm.UnlockMap(2);
            }
            else if (current == nextSceneName2) // SceneMap3
            {
                // ✅ ĐÃ THẮNG MAP CUỐI
                ShowEndGame();
                return;
            }
        }

        // Resume time + lock cursor trước khi chuyển scene
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Chọn scene đích
        string targetScene = nextSceneName;
        if (current == nextSceneName)
            targetScene = nextSceneName2;

        Debug.Log($"[GameEndUI] LoadScene => {targetScene}");
        SceneManager.LoadScene(targetScene);
    }

    // ===================== CONTINUE (ẩn UI, không đổi scene) =====================
    public void ContinueGame()
    {
        Debug.Log("[GameEndUI] Continue Game");

        if (victoryScreen != null) victoryScreen.SetActive(false);
        if (deathScreen != null) deathScreen.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Time.timeScale = 1f;
    }

    // ===================== RETRY =====================
    public void Retry()
    {
        Debug.Log("[GameEndUI] Retry");

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        string currentScene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentScene);
    }
}

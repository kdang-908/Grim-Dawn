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
    public string retrySceneName = "Map";       // hiện tại mình sẽ lấy scene hiện tại luôn
    [Tooltip("Tên scene map kế tiếp khi Victory")]
    public string nextSceneName = "SceneMap2";

    private void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

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

        // chờ player sinh ra (nếu spawn bằng script)
        while (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            yield return null;
        }

        CharacterStats stats = player.GetComponent<CharacterStats>();
        if (stats == null)
        {
            Debug.LogError("[GameEndUI] Player không có CharacterStats!");
            yield break;
        }

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
        // dùng Realtime để không bị Time.timeScale ảnh hưởng
        yield return new WaitForSecondsRealtime(showDelay);

        gameObject.SetActive(true);
        if (victoryScreen != null) victoryScreen.SetActive(false);
        if (deathScreen != null) deathScreen.SetActive(true);

        // hiển thị chuột để bấm Retry
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // pause game
        Time.timeScale = 0f;
    }

    // ===================== VICTORY =====================
    public void ShowVictory()
    {
        StartCoroutine(ShowVictoryDelay());
    }

    IEnumerator ShowVictoryDelay()
    {
        // chờ 1 chút trước khi hiện UI Victory
        yield return new WaitForSecondsRealtime(showDelay);

        gameObject.SetActive(true);
        if (deathScreen != null) deathScreen.SetActive(false);
        if (victoryScreen != null) victoryScreen.SetActive(true);

        // hiện chuột để người chơi thấy màn hình Victory
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // dừng game lại
        Time.timeScale = 0f;

        // ⛔ TỪ ĐÂY TRỞ ĐI KHÔNG AUTO LOAD NỮA
        // Người chơi sẽ bấm nút (Next / Continue) để qua map
    }

    /// <summary>
    /// Gán hàm này cho nút "Next" / "Continue" ở màn hình Victory
    /// </summary>
    public void NextMap()
    {
        Debug.Log("[GameEndUI] Next Map (Button)");

        // 🔹 SAVE PLAYER TRƯỚC KHI QUA MAP MỚI
        var gm = GameManager.Instance;
        if (gm != null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                var stats = player.GetComponent<CharacterStats>();
                if (stats != null)
                {
                    gm.SavePlayer(stats);
                }
                else
                {
                    Debug.LogWarning("[GameEndUI] Không tìm thấy CharacterStats trên Player để Save");
                }
            }
            // SAVE POTION TRƯỚC KHI QUA SCENE MỚI
            if (PotionManager.Instance != null)
            {
                PotionManager.SavedPotions = PotionManager.Instance.GetPotionCount();
                Debug.Log($"[GameEndUI] Saved potions = {PotionManager.SavedPotions}");
            }

            else
            {
                Debug.LogWarning("[GameEndUI] Không tìm thấy Player (tag Player) để Save trước khi qua map mới");
            }
        }

        // bỏ pause, khóa chuột lại như đang chơi
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // chuyển sang map kế tiếp
        SceneManager.LoadScene(nextSceneName);
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

        // Reload đúng scene hiện tại
        string currentScene = SceneManager.GetActiveScene().name;
        // nếu muốn luôn về 1 scene cố định thì dùng:
        // SceneManager.LoadScene(retrySceneName);
        SceneManager.LoadScene(currentScene);
    }
}

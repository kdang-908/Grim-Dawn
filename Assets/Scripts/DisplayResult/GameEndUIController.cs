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
    public float showDelay = 1.5f;

    void Awake()
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

    void Start()
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

    // ================= DEATH =================
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

        Time.timeScale = 0f;
    }

    // ================= VICTORY =================
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

    // ================= CONTINUE =================
    public void ContinueGame()
    {
        Debug.Log("[GameEndUI] Continue Game");

        if (victoryScreen != null) victoryScreen.SetActive(false);
        if (deathScreen != null) deathScreen.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Time.timeScale = 1f;
    }

    // ================= RETRY =================
    public void Retry()
    {
        Debug.Log("[GameEndUI] Retry");

        // bỏ pause game
        Time.timeScale = 1f;

        // khóa chuột lại như bình thường khi chơi
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // load lại scene Map (tên phải đúng y như file trong folder Scenes)
        SceneManager.LoadScene("Map");
    }
}

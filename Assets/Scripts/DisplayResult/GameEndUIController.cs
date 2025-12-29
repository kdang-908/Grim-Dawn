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

        deathScreen?.SetActive(false);
        victoryScreen?.SetActive(false);
    }

    void Start()
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

        CharacterStats stats = player.GetComponent<CharacterStats>();
        if (stats == null)
        {
            Debug.LogError("[GameEndUI] Player không có CharacterStats!");
            yield break;
        }

        stats.onDeath.AddListener(OnPlayerDeath);
        Debug.Log("[GameEndUI] ?ã hook OnDeath");
    }

    // ================= DEATH =================
    public void OnPlayerDeath()
    {
        StartCoroutine(ShowDeathScreenDelay());
    }

    IEnumerator ShowDeathScreenDelay()
    {
        yield return new WaitForSecondsRealtime(showDelay);

        gameObject.SetActive(true);
        victoryScreen?.SetActive(false);
        deathScreen?.SetActive(true);

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
        deathScreen?.SetActive(false);
        victoryScreen?.SetActive(true);

        Time.timeScale = 0f;
    }

    // ================= CONTINUE =================
    public void ContinueGame()
    {
        Debug.Log("[GameEndUI] Continue Game");

        victoryScreen?.SetActive(false);
        deathScreen?.SetActive(false);

        Time.timeScale = 1f;
    }

    // ================= RETRY =================
    public void Retry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Map");
    }
}

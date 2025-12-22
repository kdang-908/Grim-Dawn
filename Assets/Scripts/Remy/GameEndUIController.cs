using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameEndUIController : MonoBehaviour
{
    public GameObject deathScreen;
    public float showDelay = 1.5f;

    void Awake()
    {
        if (deathScreen != null)
            deathScreen.SetActive(false);
    }

    void Start()
    {
        StartCoroutine(WaitForPlayerAndHook());
    }

    IEnumerator WaitForPlayerAndHook()
    {
        GameObject player = null;
        CharacterStats stats = null;

        while (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            yield return null; // ??i frame ti?p theo
        }

        stats = player.GetComponent<CharacterStats>();
        if (stats == null)
        {
            Debug.LogError("[GameEndUI] Player không có CharacterStats!");
            yield break;
        }

        stats.onDeath.AddListener(OnPlayerDeath);
        Debug.Log("[GameEndUI] ?ã hook OnDeath thành công (waited)");
    }


    public void OnPlayerDeath()
    {
        StartCoroutine(ShowDeathScreenDelay());
    }

    IEnumerator ShowDeathScreenDelay()
    {
        yield return new WaitForSecondsRealtime(showDelay);

        if (deathScreen != null)
            deathScreen.SetActive(true);

        Time.timeScale = 0f;
    }

    public void Retry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Map");
    }
}

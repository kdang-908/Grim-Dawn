using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;


public class EnemyLeftChecker : MonoBehaviour
{
    public float checkInterval = 1f;

    bool victoryTriggered = false;
    EnemySpawner spawner;

    void Start()
    {
        spawner = FindFirstObjectByType<EnemySpawner>();

        if (spawner == null)
        {
            //Debug.LogError("[EnemyLeftChecker] Không tìm thấy EnemySpawner!");
            return;
        }

        StartCoroutine(CheckEnemyRoutine());
    }

    IEnumerator CheckEnemyRoutine()
    {
        while (!victoryTriggered)
        {
            yield return new WaitForSeconds(checkInterval);

            // Chưa spawn xong thì bỏ qua
            if (!spawner.finishedSpawning)
                continue;

            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

            if (enemies.Length == 0)
            {
                victoryTriggered = true;
                //Debug.Log("== NO ENEMY LEFT + SPAWN FINISHED => VICTORY ==");

                TriggerVictoryUI();
            }
        }
    }

    void TriggerVictoryUI()
    {
        if (GameEndUIController.Instance == null)
        {
            //Debug.LogError("[EnemyLeftChecker] GameEndUIController.Instance = NULL");
            return;
        }

        string currentScene = SceneManager.GetActiveScene().name;

        //  MAP CUỐI → THE END (KHÔNG HIỆN VICTORY)
        if (currentScene == "SceneMap3")
        {
            //Debug.Log("[EnemyLeftChecker] Final Map cleared → SHOW THE END");
            GameEndUIController.Instance.ShowEndGame();
        }
        else
        {
            // MAP THƯỜNG → VICTORY
            //Debug.Log("[EnemyLeftChecker] Map cleared → SHOW VICTORY");
            GameEndUIController.Instance.ShowVictory();
        }
    }

}

using UnityEngine;
using System.Collections;

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
            Debug.LogError("[EnemyLeftChecker] Không tìm thấy EnemySpawner!");
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
                Debug.Log("== NO ENEMY LEFT + SPAWN FINISHED => VICTORY ==");

                TriggerVictoryUI();
            }
        }
    }

    void TriggerVictoryUI()
    {
        if (GameEndUIController.Instance != null)
        {
            GameEndUIController.Instance.ShowVictory();
        }
        else
        {
            Debug.LogError("[EnemyLeftChecker] GameEndUIController.Instance = NULL");
        }
    }
}

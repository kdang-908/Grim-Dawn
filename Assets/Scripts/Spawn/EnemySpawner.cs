using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefab")]
    public GameObject enemyPrefab;

    [Header("Spawn Settings")]
    public float spawnDelay = 1f;
    public int spawnCount = 5;
    public float spawnRadius = 3f;

    [Header("Player Detect")]
    public string playerTag = "Player";

    // chỉ bật 1 lần khi player bước vào trigger
    private bool activated = false;

    // đã spawn bao nhiêu con
    private int spawnedEnemies = 0;

    // cho EnemyLeftChecker kiểm tra đã spawn xong chưa
    [HideInInspector]
    public bool finishedSpawning = false;

    private void OnTriggerEnter(Collider other)
    {
        if (activated) return;

        if (other.CompareTag(playerTag))
        {
            activated = true;
            finishedSpawning = false;    // mới kích hoạt, chuẩn bị spawn
            //Debug.Log("Spawner ACTIVATED — Player đã kích hoạt vùng spawn");

            StartCoroutine(SpawnRoutine());
        }
    }

    IEnumerator SpawnRoutine()
    {
        // spawn từng con cho đến khi đủ spawnCount
        while (spawnedEnemies < spawnCount)
        {
            SpawnEnemy();
            spawnedEnemies++;

            yield return new WaitForSeconds(spawnDelay);
        }

        finishedSpawning = true; // 🔥 báo cho EnemyLeftChecker là đã spawn xong
        //Debug.Log("Spawner hoàn thành, đã spawn đủ quái");
    }

    void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            //Debug.LogWarning("[EnemySpawner] Chưa gán enemyPrefab!");
            return;
        }

        Vector3 pos = transform.position;

        Vector2 circle = Random.insideUnitCircle * spawnRadius;
        pos += new Vector3(circle.x, 0, circle.y);

        Instantiate(enemyPrefab, pos, Quaternion.identity);

        //Debug.Log($"Spawn Skeleton #{spawnedEnemies + 1} tại {pos}");
    }
}

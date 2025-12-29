using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    // Prefab quái sẽ được spawn ra
    public GameObject enemyPrefab;

    // Thời gian chờ giữa mỗi lần spawn (30 giây)
    public float spawnDelay = 30f;

    // Số lượng quái sẽ spawn tổng cộng (5 con)
    public int spawnCount = 5;

    // Bán kính random spawn quanh spawner
    // Tránh quái bị spawn chồng lên nhau
    public float spawnRadius = 3f;

    [HideInInspector]
    public bool finishedSpawning = false;

    private void Start()
    {
        // Bắt đầu coroutine spawn từng con theo thời gian
        StartCoroutine(SpawnEnemies());
    }

    IEnumerator SpawnEnemies()
    {
        // Lặp spawn "spawnCount" lần
        for (int i = 0; i < spawnCount; i++)
        {
            // Spawn 1 con quái
            SpawnOneEnemy();

            // Chờ spawnDelay giây rồi spawn tiếp
            yield return new WaitForSeconds(spawnDelay);
        }

        finishedSpawning = true;
        Debug.Log("EnemySpawner: Spawn xong toàn bộ");
    }

    void SpawnOneEnemy()
    {
        // Vị trí spawn cơ bản (vị trí spawner)
        Vector3 randomPos = transform.position;

        // Random offset trong hình tròn (2D) để tránh bị chồng lên nhau
        Vector2 circle = Random.insideUnitCircle * spawnRadius;

        // Thêm offset vào vị trí spawn
        randomPos += new Vector3(circle.x, 0, circle.y);

        // Tạo quái tại vị trí random
        Instantiate(enemyPrefab, randomPos, Quaternion.identity);
    }
}

using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public Transform spawnPoint;

    void Start()
    {
        Debug.Log("[Spawner] Start");

        var gm = GameManager.Instance;
        if (gm == null)
        {
            Debug.LogError("[Spawner] GameManager.Instance == null");
            return;
        }

        var prefab = gm.GetSelectedPrefab();
        Debug.Log("[Spawner] selectedCharacter=" + gm.selectedCharacter + ", prefab=" + (prefab ? prefab.name : "null"));

        if (prefab == null)
        {
            Debug.LogError("[Spawner] No selected prefab.");
            return;
        }

        var p = spawnPoint != null ? spawnPoint : transform;
        GameObject player = Instantiate(prefab, p.position, p.rotation);

        Debug.Log("[Spawner] Player spawned: " + player.name);
    }
}

using System.Collections;
using UnityEngine;

public class PlayerRespawnOnDeath : MonoBehaviour
{
    [Header("HP (bạn tự gọi SetDead(true) khi HP = 0)")]
    public bool isDead = false;

    [Header("Refs")]
    public Animator animator;
    public Transform homeSpawn;

    [Header("Disable when dead")]
    public MonoBehaviour[] scriptsToDisable;
    public Collider[] collidersToDisable;

    [Header("Animator")]
    public string dieTrigger = "Die";

    [Header("Respawn")]
    public float respawnDelay = 2f;

    private bool running;

    void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();

        if (homeSpawn == null)
        {
            var hs = GameObject.Find("HomeSpawn");
            if (hs != null) homeSpawn = hs.transform;
        }
    }

    void Update()
    {
        if (!running && isDead)
            StartCoroutine(Co_Respawn());
    }

    public void SetDead(bool dead)
    {
        isDead = dead;
    }

    IEnumerator Co_Respawn()
    {
        running = true;

        // 1) chơi anim chết để VFX state Dead chạy
        if (animator != null) animator.SetTrigger(dieTrigger);

        // 2) tắt điều khiển
        foreach (var s in scriptsToDisable)
            if (s != null) s.enabled = false;

        foreach (var c in collidersToDisable)
            if (c != null) c.enabled = false;

        // 3) đợi
        yield return new WaitForSeconds(respawnDelay);

        // 4) teleport về nhà (xử lý CharacterController nếu có)
        var cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        if (homeSpawn != null)
        {
            transform.position = homeSpawn.position;
            transform.rotation = homeSpawn.rotation;
        }

        if (cc != null) cc.enabled = true;

        // 5) bật lại
        foreach (var c in collidersToDisable)
            if (c != null) c.enabled = true;

        foreach (var s in scriptsToDisable)
            if (s != null) s.enabled = true;

        // 6) reset dead để chơi lại
        isDead = false;
        running = false;
    }
}

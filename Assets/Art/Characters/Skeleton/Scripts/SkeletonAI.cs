using UnityEngine;

public class SkeletonAI : MonoBehaviour
{
    private GameObject player;
    private CharacterStats playerStats;  // Dùng CharacterStats thay vì PlayerHealth

    [Header("AI Settings")]
    public float chaseRange = 100f;
    public float attackRange = 1.8f;
    public float moveSpeed = 2f;
    public float attackCooldown = 1.3f;

    private Animator anim;
    private float lastAttackTime;
    public CharacterStats State;

    void Start()
    {
        anim = GetComponent<Animator>();
        RefreshPlayer();
    }

    public void RefreshPlayer()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerStats = player.GetComponent<CharacterStats>();
            if (playerStats == null) playerStats = player.GetComponentInChildren<CharacterStats>();
            if (playerStats == null) playerStats = player.GetComponentInParent<CharacterStats>();

            if (playerStats != null)
                Debug.Log($"[SkeletonAI] {name} tìm thấy player: {player.name}");
            else
                Debug.LogError($"[SkeletonAI] {name} player có Tag=Player nhưng KHÔNG tìm thấy CharacterStats ở root/child/parent!");
        }
        else
        {
            Debug.LogWarning($"[SkeletonAI] {name} không tìm thấy Player! (Tag Player)");
        }
    }


    void Update()
    {
        if (player == null)
        {
            if (Time.frameCount % 60 == 0)
            {
                RefreshPlayer();
            }
            return;
        }

        if (State != null && State.currentHP <= 0)
        {
            HandleDead();
            return;
        }

        float dist = Vector3.Distance(transform.position, player.transform.position);

        if (dist > chaseRange)
        {
            anim.SetBool("isMoving", false);
            return;
        }

        if (dist > attackRange)
        {
            MoveTowardsPlayer();
        }
        else
        {
            Attack();
        }
    }

    void MoveTowardsPlayer()
    {
        anim.SetBool("isMoving", true);

        Vector3 dir = (player.transform.position - transform.position).normalized;
        dir.y = 0;

        if (dir.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(dir);

        transform.position += dir * moveSpeed * Time.deltaTime;
    }

    void Attack()
    {
        anim.SetBool("isMoving", false);

        if (Time.time - lastAttackTime < attackCooldown)
        {
            return;
        }

        lastAttackTime = Time.time;
        anim.SetTrigger("attack");

        // SỬA: Kiểm tra playerStats thay vì playerHealth
        if (playerStats == null)
        {
            Debug.LogWarning("[SkeletonAI] playerStats == null, không gây damage");
            return;
        }

        // SỬA: Gọi TakeDamage của CharacterStats
        playerStats.TakeDamage(10);
        Debug.Log($"[SkeletonAI] {name} gây 10 damage, player HP: {playerStats.currentHP}/{playerStats.maxHP}");
    }

    void HandleDead()
    {
        Debug.Log($"[SkeletonAI] {name} HandleDead()");

        anim.SetTrigger("Dead");

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        StartCoroutine(Disappear());
        this.enabled = false;
    }

    System.Collections.IEnumerator Disappear()
    {
        yield return new WaitForSeconds(3f);

        float timer = 0;
        float disappearDuration = 2f;
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + Vector3.down * 2f;

        while (timer < disappearDuration)
        {
            transform.position = Vector3.Lerp(startPos, endPos, timer / disappearDuration);
            timer += Time.deltaTime;
            yield return null;
        }

        Debug.Log($"[SkeletonAI] {name} Destroy after disappear");
        Destroy(gameObject);
    }
}

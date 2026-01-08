using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class SkeletonAI : MonoBehaviour
{
    // ===== PLAYER REF =====
    private GameObject player;
    private CharacterStats playerStats;

    [Header("AI Settings")]
    public float chaseRange = 20f;
    public float attackRange = 1.8f;
    public float moveSpeed = 2f;
    public float attackCooldown = 1.3f;

    [Header("State")]
    public bool aggroOnStart = true;
    private bool hasAggro = false;

    [Header("CharacterStats của Skeleton")]
    public CharacterStats State;

    [Header("Avoid Clumping")]
    public float aroundPlayerRadius = 1.2f;
    private Vector2 personalOffset2D;

    [Header("Ground (stick)")]
    public LayerMask groundMask;
    public float groundCheckDistance = 3.5f;
    public float groundOffsetY = 0.0f;

    [Header("Obstacle Avoid (NO xuyên cây/đá/nhà)")]
    public LayerMask obstacleMask;           // set: Environment + Default (nếu nhà/đá ở Default)
    public float avoidProbeDistance = 1.2f;  // khoảng quét trước mặt
    [Range(0.1f, 1f)] public float avoidStrength = 0.85f;
    public int sideRaysPerSide = 3;
    public float sideRayAngleStep = 20f;

    [Header("Stop push")]
    public float stopDistance = 1.25f;

    [Header("Reward - HP Potion")]
    public GameObject hpBottlePrefab;
    [Range(0f, 1f)] public float hpDropChance = 0.85f;

    [Header("Reward - EXP")]
    public int expReward = 40;

    [Header("Reward - Gold")]
    public int goldRewardMin = 500;
    public int goldRewardMax = 1500;
    [Range(0f, 1f)] public float goldDropChance = 1f;
    public GameObject goldDropPrefab;

    // ===== internals =====
    private Animator anim;
    private float lastAttackTime;

    private Rigidbody rb;
    private CapsuleCollider capCol;

    private Vector3 desiredMoveDir; // tính ở Update, chạy ở FixedUpdate

    void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        capCol = GetComponent<CapsuleCollider>();

        // ✅ PHYSICS chuẩn để không xuyên + mượt
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        personalOffset2D = Random.insideUnitCircle.normalized * aroundPlayerRadius;
    }

    void Start()
    {
        RefreshPlayer();
        hasAggro = aggroOnStart;
    }

    public void RefreshPlayer()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        playerStats = player.GetComponent<CharacterStats>();
        if (playerStats == null) playerStats = player.GetComponentInChildren<CharacterStats>();
        if (playerStats == null) playerStats = player.GetComponentInParent<CharacterStats>();
    }

    void Update()
    {
        desiredMoveDir = Vector3.zero;

        if (player == null)
        {
            if (Time.frameCount % 60 == 0) RefreshPlayer();
            return;
        }

        if (State != null && State.currentHP <= 0)
        {
            HandleDead();
            return;
        }

        Vector3 myPos = rb.position;
        Vector3 playerPos = player.transform.position;
        float dist = Vector3.Distance(myPos, playerPos);

        // chưa aggro
        if (!hasAggro)
        {
            anim.SetBool("isMoving", false);

            Vector3 look = playerPos - myPos;
            look.y = 0;
            if (look.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(look);

            if (dist <= chaseRange) hasAggro = true;
            return;
        }

        // ngoài chase range -> đứng
        if (dist > chaseRange)
        {
            anim.SetBool("isMoving", false);
            return;
        }

        // gần quá thì đứng/đánh (không ép vào cây)
        if (dist <= Mathf.Max(stopDistance, attackRange))
        {
            anim.SetBool("isMoving", false);
            if (dist <= attackRange) Attack();
            return;
        }

        // tính hướng chạy + né
        Vector3 target = (dist > attackRange * 1.5f)
            ? playerPos + new Vector3(personalOffset2D.x, 0f, personalOffset2D.y)
            : playerPos;

        Vector3 dir = target - myPos;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.0001f)
        {
            anim.SetBool("isMoving", false);
            return;
        }

        dir.Normalize();

        // hướng né vật cản
        dir = ApplyAvoidance(dir);

        desiredMoveDir = dir;

        anim.SetBool("isMoving", desiredMoveDir != Vector3.zero);
        if (desiredMoveDir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(desiredMoveDir);
    }

    void FixedUpdate()
    {
        if (desiredMoveDir == Vector3.zero) return;

        Vector3 myPos = rb.position;
        Vector3 step = desiredMoveDir * moveSpeed * Time.fixedDeltaTime;

        // giữ quái dính đất bằng raycast (nhẹ thôi)
        Vector3 next = myPos + step;
        next = SnapToGround(next);

        // ✅ chặn cứng bằng capsule cast trước khi move
        if (!CapsuleBlocked(myPos, next, obstacleMask, out RaycastHit hit))
        {
            rb.MovePosition(next);
            return;
        }

        // bị chặn -> thử slide theo mặt
        Vector3 slideDir = Vector3.ProjectOnPlane(desiredMoveDir, hit.normal);
        slideDir.y = 0;
        if (slideDir.sqrMagnitude < 0.0001f) return;
        slideDir.Normalize();

        Vector3 slideNext = myPos + slideDir * moveSpeed * Time.fixedDeltaTime;
        slideNext = SnapToGround(slideNext);

        if (!CapsuleBlocked(myPos, slideNext, obstacleMask, out _))
        {
            rb.MovePosition(slideNext);
        }
    }

    // ===================== AVOIDANCE =====================
    Vector3 ApplyAvoidance(Vector3 forwardDir)
    {
        // ray origin ngang bụng (đỡ đâm xuống đất)
        Vector3 origin = rb.position + Vector3.up * (capCol.center.y + 0.2f);

        // nếu trước mặt trống thì đi thẳng
        if (!Physics.Raycast(origin, forwardDir, avoidProbeDistance, obstacleMask, QueryTriggerInteraction.Ignore))
            return forwardDir;

        // thử tìm hướng vòng trái/phải
        for (int i = 1; i <= sideRaysPerSide; i++)
        {
            float ang = sideRayAngleStep * i;

            Vector3 left = Quaternion.Euler(0f, -ang, 0f) * forwardDir;
            if (!Physics.Raycast(origin, left, avoidProbeDistance, obstacleMask, QueryTriggerInteraction.Ignore))
                return Vector3.Slerp(forwardDir, left, avoidStrength).normalized;

            Vector3 right = Quaternion.Euler(0f, ang, 0f) * forwardDir;
            if (!Physics.Raycast(origin, right, avoidProbeDistance, obstacleMask, QueryTriggerInteraction.Ignore))
                return Vector3.Slerp(forwardDir, right, avoidStrength).normalized;
        }

        // bí quá -> đứng lại (khỏi giật)
        return Vector3.zero;
    }

    bool CapsuleBlocked(Vector3 from, Vector3 to, LayerMask mask, out RaycastHit hit)
    {
        Vector3 dir = (to - from);
        float dist = dir.magnitude;
        if (dist <= 0.0001f)
        {
            hit = new RaycastHit();
            return false;
        }
        dir /= dist;

        float radius = Mathf.Max(0.05f, capCol.radius * 0.95f);
        float half = Mathf.Max(radius, capCol.height * 0.5f);
        float centerY = capCol.center.y;

        Vector3 center = from + Vector3.up * centerY;
        Vector3 p1 = center + Vector3.up * (half - radius);
        Vector3 p2 = center - Vector3.up * (half - radius);

        return Physics.CapsuleCast(p1, p2, radius, dir, out hit, dist, mask, QueryTriggerInteraction.Ignore);
    }

    Vector3 SnapToGround(Vector3 pos)
    {
        Vector3 origin = pos + Vector3.up * 1.5f;
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, groundCheckDistance, groundMask, QueryTriggerInteraction.Ignore))
        {
            float footOffset = capCol.height * 0.5f - capCol.center.y;
            pos.y = hit.point.y + footOffset + groundOffsetY;
        }
        return pos;
    }

    // ===================== ATTACK =====================
    void Attack()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;
        lastAttackTime = Time.time;

        if (anim != null) anim.SetTrigger("attack");
        if (playerStats == null) return;

        int atk = (State != null) ? State.atk_Total : 10;
        int def = playerStats.def_Total;
        int dmg = Mathf.Max(1, atk - def);

        playerStats.TakeDamage(dmg);
    }

    // ===================== DEAD / REWARD =====================
    void HandleDead()
    {
        GiveExpToPlayer();
        DropHPBottle();
        DropGold();

        if (anim != null) anim.SetTrigger("Dead");

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;

        StartCoroutine(Disappear());
        this.enabled = false;
    }

    void GiveExpToPlayer()
    {
        if (player == null) RefreshPlayer();
        if (player == null) return;

        // ✅ FIX lỗi CS0029: PlayerExperience phải GetComponent<PlayerExperience>, KHÔNG phải CharacterStats
        PlayerExperience exp = player.GetComponent<PlayerExperience>();
        if (exp == null) exp = player.GetComponentInChildren<PlayerExperience>();
        if (exp == null) exp = player.GetComponentInParent<PlayerExperience>();
        if (exp == null) return;

        exp.AddExp(expReward);
    }

    void DropGold()
    {
        if (Random.value > goldDropChance) return;
        if (goldDropPrefab == null) return;

        int goldReward = Random.Range(goldRewardMin, goldRewardMax + 1);
        Vector3 spawnPos = rb.position + Vector3.up * 0.5f;

        var coin = Instantiate(goldDropPrefab, spawnPos, Quaternion.identity);
        var pickup = coin.GetComponent<GoldPickup>();
        if (pickup != null) pickup.value = goldReward;
    }

    void DropHPBottle()
    {
        if (Random.value > hpDropChance) return;
        if (hpBottlePrefab == null) return;

        Vector3 spawnPos = rb.position + Vector3.up * 0.4f;
        Instantiate(hpBottlePrefab, spawnPos, Quaternion.identity);
    }

    IEnumerator Disappear()
    {
        yield return new WaitForSeconds(3f);
        Destroy(gameObject);
    }
}

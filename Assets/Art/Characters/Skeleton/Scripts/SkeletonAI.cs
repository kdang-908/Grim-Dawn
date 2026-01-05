using UnityEngine;
using System.Collections;

public class SkeletonAI : MonoBehaviour
{
    // ===== PLAYER REF =====
    private GameObject player;
    private CharacterStats playerStats;  // dùng CharacterStats của player

    [Header("Reward - HP Potion")]
    [Tooltip("Prefab bình HP (phải có collider + trigger)")]
    public GameObject hpBottlePrefab;

    [Range(0f, 1f)]
    [Tooltip("Tỉ lệ rơi bình máu (0.85 = 85%)")]
    public float hpDropChance = 0.85f;

    [Header("AI Settings")]
    public float chaseRange = 100f;
    public float attackRange = 1.8f;
    public float moveSpeed = 2f;
    public float attackCooldown = 1.3f;

    [Header("Reward - EXP")]
    public int expReward = 40;   // EXP thưởng khi giết con này

    [Header("Reward - Gold")]
    [Tooltip("Min vàng có thể rơi")]
    public int goldRewardMin = 500;
    [Tooltip("Max vàng có thể rơi")]
    public int goldRewardMax = 1500;

    [Range(0f, 1f)]
    [Tooltip("Tỉ lệ rơi vàng (1 = luôn rơi, 0.5 = 50%)")]
    public float goldDropChance = 1f;

    [Tooltip("Prefab coin (có gắn GoldPickup)")]
    public GameObject goldDropPrefab;

    [Header("State")]
    [Tooltip("False = đứng yên, chỉ khi player lại gần rồi đánh mới bắt đầu đuổi")]
    public bool aggroOnStart = false;

    private bool hasAggro = false;   // đã bắt đầu đuổi chưa

    private Animator anim;
    private float lastAttackTime;
    [Tooltip("CharacterStats của chính con Skeleton")]
    public CharacterStats State;

    [Header("Avoid Clumping")]
    [Tooltip("Bán kính đứng vòng quanh player để đỡ dính 1 cục")]
    public float aroundPlayerRadius = 1.2f;   // bán kính đứng quanh player

    // offset riêng cho từng con quanh player
    private Vector2 personalOffset2D;

    [Header("Ground (giữ quái dính mặt đất)")]
    [Tooltip("Layer mặt đất / đường / terrain")]
    public LayerMask groundMask;
    [Tooltip("Khoảng cách raycast xuống dưới")]
    public float groundCheckDistance = 5f;
    [Tooltip("Tinh chỉnh cao thấp nếu cần (+ lên, - xuống)")]
    public float groundOffsetY = 0.0f;

    private CapsuleCollider capCol;

    // =========================================
    // LIFE CYCLE
    // =========================================
    void Start()
    {
        anim = GetComponent<Animator>();
        capCol = GetComponent<CapsuleCollider>();

        RefreshPlayer();

        // nếu muốn quái spawn ra là aggro luôn thì bật aggroOnStart = true
        hasAggro = aggroOnStart;

        // mỗi con skeleton sẽ có 1 offset riêng quanh player
        personalOffset2D = Random.insideUnitCircle.normalized * aroundPlayerRadius;
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
                Debug.LogError($"[SkeletonAI] {name} player có Tag=Player nhưng KHÔNG tìm thấy CharacterStats!");
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
                RefreshPlayer();
            return;
        }

        // nếu quái chết rồi thì xử lý chết
        if (State != null && State.currentHP <= 0)
        {
            HandleDead();
            return;
        }

        float dist = Vector3.Distance(transform.position, player.transform.position);

        // ===== 1) TRẠNG THÁI CHƯA AGGRO =====
        if (!hasAggro)
        {
            anim.SetBool("isMoving", false);

            // quay mặt nhìn player cho tự nhiên
            Vector3 dirLook = (player.transform.position - transform.position);
            dirLook.y = 0;
            if (dirLook.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.LookRotation(dirLook);

            // nếu player CHƯA vào tầm đánh thì không làm gì thêm
            if (dist > attackRange)
                return;

            // player ĐÃ vào tầm đánh -> đánh 1 cái trước
            Attack();

            // sau cú đánh đầu tiên -> bắt đầu AGGRO
            hasAggro = true;
            return;
        }

        // ===== 2) TRẠNG THÁI ĐÃ AGGRO =====

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

        // GIỮ QUÁI DÍNH MẶT ĐẤT
        KeepOnGround();
    }

    // =========================================
    // MOVE / ATTACK
    // =========================================
    void MoveTowardsPlayer()
    {
        if (player == null) return;

        anim.SetBool("isMoving", true);

        // Mỗi con nhắm tới 1 vị trí lệch quanh player
        Vector3 targetPos = player.transform.position +
                            new Vector3(personalOffset2D.x, 0f, personalOffset2D.y);

        Vector3 dir = (targetPos - transform.position);
        dir.y = 0f;

        if (dir.sqrMagnitude > 0.0001f)
        {
            dir = dir.normalized;
            transform.rotation = Quaternion.LookRotation(dir);
            transform.position += dir * moveSpeed * Time.deltaTime;
        }
    }

    void Attack()
    {
        anim.SetBool("isMoving", false);

        if (Time.time - lastAttackTime < attackCooldown)
            return;

        lastAttackTime = Time.time;
        anim.SetTrigger("attack");

        if (playerStats == null)
            return;

        // ===== Damage lấy từ CharacterStats =====
        int atk = State != null ? State.atk_Total : 10;
        int def = playerStats.def_Total;

        int finalDamage = Mathf.Max(1, atk - def);

        playerStats.TakeDamage(finalDamage);

        Debug.Log($"[SkeletonAI] {name} đánh {finalDamage} dmg  (ATK={atk}, DEF={def})");
    }


    // =========================================
    // GROUND – GIỮ QUÁI ĐỨNG TRÊN MẶT ĐẤT
    // =========================================
    void KeepOnGround()
    {
        // bắn ray từ trên xuống dưới
        float rayStartHeight = 2f;
        Vector3 rayOrigin = transform.position + Vector3.up * rayStartHeight;

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, groundCheckDistance, groundMask))
        {
            float footOffset = 0f;

            // tính khoảng cách từ pivot xuống bàn chân dựa trên CapsuleCollider
            if (capCol != null)
            {
                // chiều cao từ tâm collider xuống đáy
                footOffset = capCol.height * 0.5f - capCol.center.y;
            }

            Vector3 pos = transform.position;
            pos.y = hit.point.y + footOffset + groundOffsetY;
            transform.position = pos;
        }
    }

    // =========================================
    // DEAD / REWARD
    // =========================================
    void HandleDead()
    {
        Debug.Log($"[SkeletonAI] {name} HandleDead()");

        // cộng EXP
        GiveExpToPlayer();

        // rơi bình máu
        DropHPBottle();

        // rơi vàng
        DropGold();

        anim.SetTrigger("Dead");

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        StartCoroutine(Disappear());
        this.enabled = false;
    }

    void GiveExpToPlayer()
    {
        if (player == null)
            RefreshPlayer();

        if (player == null)
        {
            Debug.LogWarning("[SkeletonAI] Không tìm thấy player để cộng EXP");
            return;
        }

        PlayerExperience exp = player.GetComponent<PlayerExperience>();
        if (exp == null) exp = player.GetComponentInChildren<PlayerExperience>();
        if (exp == null) exp = player.GetComponentInParent<PlayerExperience>();

        if (exp == null)
        {
            Debug.LogWarning("[SkeletonAI] Player không có PlayerExperience, không cộng EXP được");
            return;
        }

        exp.AddExp(expReward);
        Debug.Log($"[SkeletonAI] {name} chết → +{expReward} EXP cho player");
    }

    void DropGold()
    {
        // tỉ lệ rơi (0..1)
        if (Random.value > goldDropChance)
            return;

        // random vàng trong khoảng [min, max]
        int goldReward = Random.Range(goldRewardMin, goldRewardMax + 1);

        var gm = GameManager.Instance;
        if (gm != null)
        {
            gm.AddGold(goldReward);
        }

        // không có prefab thì chỉ cộng vàng, không spawn coin
        if (goldDropPrefab == null)
        {
            Debug.Log($"[SkeletonAI] {name} drop {goldReward} gold (no prefab)");
            return;
        }

        // spawn coin dưới chân quái
        Vector3 spawnPos = transform.position + Vector3.up * 0.5f;
        Quaternion spawnRot = Quaternion.identity;

        var coin = Instantiate(goldDropPrefab, spawnPos, spawnRot);

        var pickup = coin.GetComponent<GoldPickup>();
        if (pickup != null)
        {
            pickup.value = goldReward;
        }

        Debug.Log($"[SkeletonAI] {name} drop {goldReward} gold at {spawnPos}");
    }

    IEnumerator Disappear()
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

    void DropHPBottle()
    {
        // random theo tỉ lệ
        if (Random.value > hpDropChance)
            return;

        if (hpBottlePrefab == null)
        {
            Debug.LogWarning($"[SkeletonAI] {name} muốn drop HP nhưng chưa assign prefab!");
            return;
        }

        // spawn ngay vị trí xác quái
        Vector3 spawnPos = transform.position + Vector3.up * 0.4f;

        Instantiate(hpBottlePrefab, spawnPos, Quaternion.identity);

        Debug.Log($"[SkeletonAI] {name} drop HP Bottle at {spawnPos}");
    }
}

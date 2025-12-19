using UnityEngine;

public class SkeletonAI : MonoBehaviour
{
    private GameObject player;          // Player object
    private PlayerHealth playerHealth; 

    [Header("AI Settings")]
    public float chaseRange = 10f;
    public float attackRange = 1.8f;
    public float moveSpeed = 2f;
    public float attackCooldown = 1.3f;

    private Animator anim;
    private float lastAttackTime;
    public CharacterStats State;

    void Start()
    {
        anim = GetComponent<Animator>();

        //Tự tìm Player theo Tag
        player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
            playerHealth = player.GetComponent<PlayerHealth>();
        //    else
        //        Debug.LogWarning("⚠ SkeletonAI không tìm thấy Player! Hãy chắc chắn Player có Tag = Player");
        //}
    }
    void Update()
    {
        if (player == null) return;

            if (State != null && State.currentHP <= 0)
            {
                HandleDead();
                return;
            }

            float dist = Vector3.Distance(transform.position, player.transform.position);

        // Ngoài tầm đuổi → đứng yên
        if (dist > chaseRange)
        {
            anim.SetBool("isMoving", false);
            return;
        }

        // Trong tầm đuổi nhưng chưa tới tầm đánh → đi đến player
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

        // quay mặt về phía player
        transform.rotation = Quaternion.LookRotation(dir);

        // di chuyển
        transform.position += dir * moveSpeed * Time.deltaTime;
    }

    void Attack()
    {
        anim.SetBool("isMoving", false);

        // cooldown chưa kết thúc
        if (Time.time - lastAttackTime < attackCooldown)
            return;

        lastAttackTime = Time.time;

        // Animation đánh
        anim.SetTrigger("attack");

        // Player đã chết → không đánh nữa
        if (playerHealth == null) return;

        // gây damage
        playerHealth.TakeDamage(10);
    }
    void HandleDead()
    {
        anim.SetTrigger("Dead");

        // Tắt các thành phần vật lý
        if (GetComponent<Collider>()) GetComponent<Collider>().enabled = false;
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        // Bắt đầu quá trình tan biến
        StartCoroutine(Disappear());

        this.enabled = false;
    }
    System.Collections.IEnumerator Disappear()
    {
        // Đợi 3 giây để người chơi nhìn thấy xác quái
        yield return new WaitForSeconds(3f);

        float timer = 0;
        float disappearDuration = 2f; // Biến mất trong 2 giây
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + Vector3.down * 2f; // Chìm xuống 2 mét

        while (timer < disappearDuration)
        {
            // Vừa cho chìm xuống đất nhẹ nhàng
            transform.position = Vector3.Lerp(startPos, endPos, timer / disappearDuration);
            timer += Time.deltaTime;
            yield return null;
        }

        // Sau khi chìm xong thì mới xóa khỏi game
        Destroy(gameObject);
    }
}

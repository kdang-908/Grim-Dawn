using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class HumanController : MonoBehaviour
{
    [Header("Refs")]
    public Rigidbody rb;
    public Animator animator;
    public DamageHitbox hitbox;
    public CharacterStats State;

    [Header("Move")]
    public float moveSpeed = 5f;
    public float runSpeed = 8f;
    public float rotationSpeed = 12f;

    [Header("Jump")]
    public float jumpForce = 5f;
    public LayerMask groundMask;
    public float groundCheckDistance = 0.2f;

    [Header("Input Blocking")]
    public string characterSelectScene = "CharacterSelection";

    [Header("Selection Mode (Preview)")]
    public bool disableColliderInSelection = true;
    public bool freezeAllInSelection = true;

    [Header("UI State")]
    [Tooltip("Bật TRUE khi đang mở Inventory/Shop/Dialog để chặn điều khiển nhân vật")]
    public bool isUIOpen = false;

    private bool isRunning;
    private bool jumpPressed;
    private bool isGrounded;

    private CapsuleCollider col;

    void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        col = GetComponent<CapsuleCollider>();

        if (animator != null) animator.applyRootMotion = false;

        if (rb != null)
        {
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        ApplySceneMode(SceneManager.GetActiveScene().name);
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }

    void OnActiveSceneChanged(Scene oldScene, Scene newScene)
    {
        ApplySceneMode(newScene.name);
    }

    void ApplySceneMode(string sceneName)
    {
        bool isSelection = (sceneName == characterSelectScene);
        if (rb == null) return;

        // stop trước khi đổi mode
        if (!rb.isKinematic)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (isSelection)
        {
            rb.useGravity = false;
            rb.isKinematic = true;

            rb.constraints = freezeAllInSelection
                ? RigidbodyConstraints.FreezeAll
                : RigidbodyConstraints.FreezeRotation;

            if (disableColliderInSelection && col != null)
                col.enabled = false;
        }
        else
        {
            if (col != null) col.enabled = true;

            rb.isKinematic = false;
            rb.useGravity = true;

            // khóa lật người theo X/Z
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    void Update()
    {
        // scene chọn nhân vật: không điều khiển
        if (SceneManager.GetActiveScene().name == characterSelectScene)
            return;

        // chết thì stop
        if (State != null && State.currentHP <= 0)
        {
            if (animator != null) animator.SetTrigger("Dead");
            enabled = false;
            return;
        }

        // UI đang mở: chặn input + đứng yên
        if (isUIOpen)
        {
            StopMotionAndAnim();
            jumpPressed = false;
            return;
        }

        isRunning = Input.GetKey(KeyCode.LeftShift);

        if (Input.GetKeyDown(KeyCode.Space))
            jumpPressed = true;

        // Attack: chỉ khi click vào game world (không click UI)
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            HandleAttack();
        }
    }

    void FixedUpdate()
    {
        if (SceneManager.GetActiveScene().name == characterSelectScene)
            return;

        if (isUIOpen)
        {
            // UI mở thì không di chuyển bằng physics
            if (rb != null && !rb.isKinematic)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            return;
        }

        GroundCheck();
        HandleMovement();
        HandleJump();
    }

    void HandleMovement()
    {
        if (rb == null) return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 moveDir;

        if (Camera.main != null)
        {
            Transform cam = Camera.main.transform;
            Vector3 camForward = cam.forward; camForward.y = 0; camForward.Normalize();
            Vector3 camRight = cam.right; camRight.y = 0; camRight.Normalize();
            moveDir = camRight * h + camForward * v;
        }
        else
        {
            moveDir = new Vector3(h, 0, v);
        }

        if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();

        bool moving = moveDir.sqrMagnitude > 0.01f;
        if (animator != null)
        {
            animator.SetBool("isWalking", moving);
            animator.SetBool("isRunning", moving && isRunning);
        }

        float speed = isRunning ? runSpeed : moveSpeed;

        Vector3 nextPos = rb.position + moveDir * speed * Time.fixedDeltaTime;
        rb.MovePosition(nextPos);

        if (moving)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            Quaternion newRot = Quaternion.Slerp(rb.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(newRot);
        }
    }

    void HandleJump()
    {
        if (rb == null) return;
        if (!jumpPressed) return;

        if (isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            if (animator != null) animator.SetBool("isJumping", true);
        }

        jumpPressed = false;
    }

    void GroundCheck()
    {
        if (col == null) return;

        Vector3 origin = transform.position + Vector3.up * 0.05f;
        float rayLen = (col.height * 0.5f) + groundCheckDistance;

        if (groundMask.value != 0)
            isGrounded = Physics.Raycast(origin, Vector3.down, rayLen, groundMask, QueryTriggerInteraction.Ignore);
        else
            isGrounded = Physics.Raycast(origin, Vector3.down, rayLen, ~0, QueryTriggerInteraction.Ignore);

        if (isGrounded && animator != null)
            animator.SetBool("isJumping", false);
    }

    void HandleAttack()
    {
        if (animator != null) animator.SetTrigger("Attack");

        if (hitbox != null)
            StartCoroutine(AttackWindow());
    }

    IEnumerator AttackWindow()
    {
        hitbox.EnableHit();
        yield return new WaitForSeconds(0.4f);
        hitbox.DisableHit();
    }

    void StopMotionAndAnim()
    {
        // đứng yên
        if (rb != null && !rb.isKinematic)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // tránh kẹt anim đang chạy
        if (animator != null)
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isRunning", false);
            animator.SetBool("isJumping", false);
        }
    }
}

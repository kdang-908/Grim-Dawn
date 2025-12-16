using UnityEngine;

public class HumanController : MonoBehaviour
{
    public Rigidbody rb;
    private bool isGrounded = false;

    public Animator animator;
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;

    private bool jumpPressed = false;

    public float runSpeed = 8f;   // tốc độ khi chạy
    private bool isRunning = false;



    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            jumpPressed = true;
        if (Input.GetMouseButtonDown(0))
        {
            HandleAttack();
        }
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    void HandleMovement()
    {


        float ipHorizontal = Input.GetAxis("Horizontal");
        float ipVertical = Input.GetAxis("Vertical");

        // Camera-relative movement
        Transform cam = Camera.main.transform;

        Vector3 camForward = cam.forward; camForward.y = 0; camForward.Normalize();
        Vector3 camRight = cam.right; camRight.y = 0; camRight.Normalize();

        Vector3 movement = ipHorizontal * camRight + ipVertical * camForward;

        if (movement.magnitude > 1f)
            movement.Normalize();

        animator.SetBool("isWalking", movement.sqrMagnitude > 0.01f);

        float finalSpeed = isRunning ? runSpeed : moveSpeed;

        rb.MovePosition(transform.position + movement * finalSpeed * Time.fixedDeltaTime);

        // --- JUMP ---
        if (jumpPressed && isGrounded)
        {
            rb.AddForce(Vector3.up * 5f, ForceMode.Impulse);
            animator.SetBool("isJumping", true);
            isGrounded = false;
        }
        else
        {
            jumpPressed = false;
        }
        HandleRotation(movement);
        HandleRunning(movement);
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            animator.SetBool("isJumping", false);
        }
    }
    void HandleRunning(Vector3 movement)
    {
        // Chỉ chạy khi có lực di chuyển
        bool moving = movement.sqrMagnitude > 0.01f;

        // Giữ shift để chạy
        if (moving && Input.GetKey(KeyCode.LeftShift))
        {
            isRunning = true;
            animator.SetBool("isRunning", true);
        }
        else
        {
            isRunning = false;
            animator.SetBool("isRunning", false);
        }
    }

    void HandleRotation(Vector3 movement)
    {
        movement.y = 0;

        if (movement.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Slerp(transform.rotation,
                                                  targetRotation,
                                                  rotationSpeed * Time.deltaTime);
        }
    }

    void HandleAttack()
    {
        animator.SetTrigger("Attack");
    }


}

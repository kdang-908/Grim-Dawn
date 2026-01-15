using UnityEngine;
using UnityEngine.SceneManagement;

public class BlacksmithInteraction : MonoBehaviour
{
    [Header("UI to open")]
    [Tooltip("Canvas Forge: Canvas_Enhancement")]
    public GameObject enhancementCanvas;          // kéo Canvas_Enhancement vào đây

    [Header("Key")]
    public KeyCode interactKey = KeyCode.F;

    private bool playerInRange = false;
    private HumanController playerController;     // tham chiếu tới script điều khiển nhân vật

    void Start()
    {
        // tìm player theo Tag
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerController = player.GetComponent<HumanController>();
        }
        else
        {
            //Debug.LogWarning("[BlacksmithInteraction] Không tìm thấy Player với tag 'Player'");
        }

        // lúc đầu tắt Forge UI
        if (enhancementCanvas != null)
            enhancementCanvas.SetActive(false);
    }

    void Update()
    {
        // chỉ khi đứng trong vùng & bấm F
        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            bool open = enhancementCanvas != null && !enhancementCanvas.activeSelf;
            SetUIOpen(open);
        }
    }

    /// <summary>
    /// Bật/tắt Forge + set trạng thái chuột + chặn điều khiển nhân vật
    /// </summary>
    void SetUIOpen(bool open)
    {
        if (enhancementCanvas != null)
            enhancementCanvas.SetActive(open);

        // Chuột: Forge mở -> unlock + hiện, tắt -> lock + ẩn
        Cursor.lockState = open ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = open;

        // Chặn input nhân vật
        if (playerController != null)
            playerController.isUIOpen = open;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            // TODO: hiện chữ "Nhấn F để nâng cấp" nếu muốn
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            // ra khỏi vùng -> chắc chắn tắt Forge
            SetUIOpen(false);
        }
    }
}

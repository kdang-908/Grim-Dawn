using UnityEngine;

public class BlacksmithInteraction : MonoBehaviour
{
    [Header("UI to open")]
    [Tooltip("Kéo Canvas_Enhancement vào đây")]
    public GameObject enhancementCanvas;

    [Header("Conflict Handling")]
    public InventoryToggle inventoryToggle; // Kéo script InventoryToggle vào đây

    [Header("Key")]
    public KeyCode interactKey = KeyCode.F;

    private bool playerInRange = false;
    private HumanController playerController;

    void Start()
    {
        // 1. Tìm Player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerController = player.GetComponent<HumanController>();
        }

        // 2. Tự tìm InventoryToggle nếu quên kéo
        if (inventoryToggle == null)
            inventoryToggle = FindFirstObjectByType<InventoryToggle>();

        // 3. Đảm bảo UI tắt khi bắt đầu
        if (enhancementCanvas != null)
            enhancementCanvas.SetActive(false);
    }

    void Update()
    {
        // Chỉ xử lý khi đứng trong vùng trigger
        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            ToggleForge();
        }
    }

    void ToggleForge()
    {
        if (enhancementCanvas == null) return;

        bool isOpening = !enhancementCanvas.activeSelf; // Nếu đang tắt thì sẽ mở, và ngược lại

        if (isOpening)
        {
            // Trước khi mở Lò rèn, phải đóng Túi đồ (nếu đang mở)
            // Gọi hàm Close() để script kia tự reset TimeScale, Cursor, v.v.
            if (inventoryToggle != null)
            {
                inventoryToggle.Close();
            }

            OpenUI();
        }
        else
        {
            CloseUI();
        }
    }

    void OpenUI()
    {
        enhancementCanvas.SetActive(true);

        // Mở chuột + Chặn điều khiển nhân vật
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (playerController != null)
            playerController.isUIOpen = true;
    }

    void CloseUI()
    {
        enhancementCanvas.SetActive(false);

        // Khóa chuột + Trả lại điều khiển
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerController != null)
            playerController.isUIOpen = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            //Có thể hiện dòng text "Press F" ở đây
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            // Đi ra xa thì tự động đóng UI
            CloseUI();
        }
    }
}
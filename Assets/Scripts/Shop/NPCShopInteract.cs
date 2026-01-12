using UnityEngine;

public class NPCShopInteract : MonoBehaviour
{
    [Header("Refs")]
    public GameObject shopCanvas;     // kéo Canvas_Shop vào đây
    public KeyCode openKey = KeyCode.B;

    [Header("Optional")]
    public GameObject hintUI;         // (tuỳ) UI chữ "Press B"
    public bool lockPlayerWhenOpen = true;

    bool playerInRange;
    bool isOpen;

    void Start()
    {
        if (shopCanvas != null) shopCanvas.SetActive(false);
        if (hintUI != null) hintUI.SetActive(false);
    }

    void Update()
    {
        if (!playerInRange) return;

        if (Input.GetKeyDown(openKey))
        {
            ToggleShop();
        }
    }

    void ToggleShop()
    {
        isOpen = !isOpen;

        if (shopCanvas != null)
            shopCanvas.SetActive(isOpen);

        if (hintUI != null)
            hintUI.SetActive(!isOpen); // mở shop thì ẩn hint

        // (tuỳ chọn) bật chuột + khóa input
        if (isOpen)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        // Nếu bạn có PlayerController script, bạn có thể disable ở đây
        if (lockPlayerWhenOpen)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                // ví dụ disable script điều khiển (đổi tên cho đúng script bạn đang dùng)
                var controller = player.GetComponent<HumanController>();
                if (controller != null) controller.enabled = !isOpen;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;
        if (hintUI != null) hintUI.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;
        if (hintUI != null) hintUI.SetActive(false);

        // rời khỏi NPC thì auto đóng shop
        if (isOpen) ToggleShop();
    }
}

using UnityEngine;
using UnityEngine.UI;

public class InventoryDeletePopup : MonoBehaviour
{
    // --- MỚI: Tạo biến Instance (Số điện thoại nóng) ---
    public static InventoryDeletePopup Instance;

    [Header("Kéo 2 nút Có và Không vào đây")]
    public Button btnYes;
    public Button btnNo;

    private Image itemPendingDelete;

    // Đổi từ Start thành Awake để chạy sớm nhất có thể
    void Awake()
    {
        // Gán chính mình vào biến Instance
        if (Instance == null)
        {
            Instance = this;
        }

        btnYes.onClick.AddListener(OnConfirmDelete);
        btnNo.onClick.AddListener(OnCancelDelete);
    }

    void Start()
    {
        // Tự tắt đi khi bắt đầu game
        gameObject.SetActive(false);
    }

    public void ShowConfirmation(Image itemIcon)
    {
        itemPendingDelete = itemIcon;
        gameObject.SetActive(true); // Bật lên
    }

    void OnConfirmDelete()
    {
        if (itemPendingDelete != null)
        {
            itemPendingDelete.sprite = null;
            itemPendingDelete.enabled = false;
            Debug.Log("Đã xóa đồ!");
        }
        ClosePopup();
    }

    void OnCancelDelete()
    {
        ClosePopup();
    }

    void ClosePopup()
    {
        itemPendingDelete = null;
        gameObject.SetActive(false);
    }
}
using UnityEngine;
using UnityEngine.UI;

public class InventoryDeletePopup : MonoBehaviour
{
    
    public static InventoryDeletePopup Instance;

    [Header("Kéo 2 nút Có và Không vào đây")]
    public Button btnYes;
    public Button btnNo;

    private Image itemPendingDelete;
    private GameObject trashButtonPendingHide;
    
    void Awake()
    {
        
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

    public void ShowConfirmation(Image itemIcon, GameObject trashBtnObj)
    {
        itemPendingDelete = itemIcon;
        trashButtonPendingHide = trashBtnObj;
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
        if (trashButtonPendingHide != null)
        {
            trashButtonPendingHide.SetActive(false);
        }
        Debug.Log("Đã xóa đồ và ẩn nút thùng rác!");
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
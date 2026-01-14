using UnityEngine;
using UnityEngine.UI;

public class InventoryDeletePopup : MonoBehaviour
{
    public static InventoryDeletePopup Instance;

    public Button btnYes;
    public Button btnNo;

    private InventoryItem pendingItem;
    private InventoryGridManager targetGrid;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // tắt popup ban đầu
        gameObject.SetActive(false);

        if (btnYes != null)
        {
            btnYes.onClick.RemoveAllListeners();
            btnYes.onClick.AddListener(OnConfirmDelete);
        }

        if (btnNo != null)
        {
            btnNo.onClick.RemoveAllListeners();
            btnNo.onClick.AddListener(OnCancelDelete);
        }
    }

    public void ShowConfirmation(InventoryItem item, InventoryGridManager grid)
    {
        pendingItem = item;
        targetGrid = grid;

        gameObject.SetActive(true);
        transform.SetAsLastSibling(); // nổi lên trên cùng
    }

    void OnConfirmDelete()
    {
        if (pendingItem == null || targetGrid == null)
        {
            ClosePopup();
            return;
        }

        var data = pendingItem.GetCurrentData();
        if (data == null)
        {
            ClosePopup();
            return;
        }

        bool ok = targetGrid.RemoveItem(data);
        //Debug.Log(ok ? $"Đã xóa: {data.name}" : $"Xóa thất bại: {data.name}");

        ClosePopup();
    }

    void OnCancelDelete() => ClosePopup();

    void ClosePopup()
    {
        pendingItem = null;
        targetGrid = null;

        gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}

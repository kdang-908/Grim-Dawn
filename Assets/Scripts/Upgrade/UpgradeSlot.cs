using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UpgradeSlot : MonoBehaviour, IPointerClickHandler
{
    [Header("UI")]
    public Image iconImage;          // child: Icon
    public GameObject plusIcon;      // child: Plus hoặc Text (TMP) dấu +
    public GameObject inventoryPanel; // Panel túi đồ 

    [Header("Runtime")]
    public InventoryItem originalItem; // item gốc trong túi

    // Biến để tham chiếu tới script InventoryItem hiển thị visual
    private InventoryItem visualItem;

    private float lastClickTime;
    const float DoubleClickDelay = 0.25f;

    public bool IsEmpty => originalItem == null;

    private void Awake()
    {
        // Auto-bind nếu quên kéo trong Inspector
        if (iconImage == null)
        {
            var t = transform.Find("Icon");
            if (t != null) iconImage = t.GetComponent<Image>();
        }

        if (plusIcon == null)
        {
            var t = transform.Find("Plus");
            if (t == null) t = transform.Find("Text (TMP)");
            if (t != null) plusIcon = t.gameObject;
        }

        // Tìm component InventoryItem trên chính object này hoặc con của nó. Để dùng cho việc hiển thị Tooltip
        visualItem = GetComponentInChildren<InventoryItem>();
    }

    private void Start()
    {
        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
            iconImage.gameObject.SetActive(true);
        }

        if (plusIcon != null)
            plusIcon.SetActive(true);
    }

    // Gán item vào ô nâng cấp
    public void SetItem(InventoryItem item)
    {
        Debug.Log($"[UpgradeSlot] SetItem from {item.name}");

        // 1. LƯU MÓN ĐỒ GỐC 
        originalItem = item;

        // 2. Ẩn item trong túi đi 
        originalItem.gameObject.SetActive(false);

        // 3. Xử lý hiển thị (Visual)
        Image sourceImg = originalItem.GetComponent<Image>();
        if (sourceImg == null) sourceImg = originalItem.GetComponentInChildren<Image>(true);

        if (iconImage == null) return;

        if (sourceImg == null || sourceImg.sprite == null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }
        else
        {
            // Copy sprite
            iconImage.sprite = sourceImg.sprite;
            iconImage.type = Image.Type.Simple;
            iconImage.preserveAspect = true;
            iconImage.color = Color.white;
            iconImage.enabled = true;
            iconImage.gameObject.SetActive(true);

            
            // NẾU Ô NÀY CÓ SCRIPT INVENTORY ITEM -> PHẢI COPY DATA SANG ĐỂ TOOLTIP HIỆN ĐÚNG
            if (visualItem != null)
            {
                // Copy Data từ món đồ gốc sang món đồ hiển thị
                visualItem.SetItem(
                    sourceImg.sprite,
                    item.itemType,
                    null,
                    item.GetCurrentData() // Lấy WeaponData
                );

                // Copy Level
                visualItem.SetUpgradeLevel(item.GetUpgradeLevel());

                Debug.Log($"[UpgradeSlot] Đã copy Data sang VisualItem: Lv {item.GetUpgradeLevel()}");
            }
        }

        // 4. Tắt dấu +
        if (plusIcon != null) plusIcon.SetActive(false);
    }

    // Trả item về túi
    public void ClearSlot()
    {
        Debug.Log("[UpgradeSlot] ClearSlot");

        // Hiện lại món đồ gốc trong túi
        if (originalItem != null)
        {
            originalItem.gameObject.SetActive(true);
            originalItem = null;
        }

        // Xóa hiển thị
        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }

        // Xóa data trong visual item (nếu có)
        if (visualItem != null)
        {
            visualItem.ClearItem();
        }

        if (plusIcon != null)
            plusIcon.SetActive(true);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        // double click = remove item
        if (Time.time - lastClickTime < DoubleClickDelay)
        {
            OnDoubleClick();
            return;
        }

        lastClickTime = Time.time;

        // Nếu slot đang TRỐNG -> chỉ toggle túi
        if (IsEmpty && inventoryPanel != null)
        {
            bool newActive = !inventoryPanel.activeSelf;
            inventoryPanel.SetActive(newActive);
        }
    }

    private void OnDoubleClick()
    {
        if (IsEmpty) return;

        if (EnhancementPanel.Instance != null)
        {
            EnhancementPanel.Instance.ReturnItemFromSlot(this);
        }
    }
}
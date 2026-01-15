using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryItem : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [System.Serializable]
    public class HeadIconMap
    {
        public Sprite icon;
        public GameObject helmetPrefab;
    }
    public HeadIconMap[] headMaps;
    public enum ItemType { Weapon, Head, Chest, Legs }
    public ItemType itemType;

    private GameObject myPrefab;
    [SerializeField] private Image itemImage;
    private GameObject itemPrefab;
    private EquipmentManager equipmentManager;
    private GameObject removeButtonObj;

    // Data hiện tại của món đồ
    private WeaponData currentData;

    [Header("Upgrade")]
    public int upgradeLevel = 1;             // cấp hiện tại
    public const int MaxUpgradeLevel = 4;

    // --- HÀM ĐỂ ENHANCEMENT PANEL LẤY DATA ---
    public WeaponData GetCurrentData()
    {
        return currentData;
    }

    public int GetUpgradeLevel()
    {
        return upgradeLevel;
    }

    public void SetUpgradeLevel(int level)
    {
        upgradeLevel = Mathf.Clamp(level, 1, MaxUpgradeLevel);
    }

    public void IncreaseUpgradeLevel()
    {
        SetUpgradeLevel(upgradeLevel + 1);
    }

    private void Awake()
    {
        if (itemImage == null) itemImage = GetComponent<Image>();
        if (itemImage == null) itemImage = GetComponentInChildren<Image>(true);

        // ✅ include inactive để vẫn tìm được EquipmentManager dù inventoryPanel đang tắt
        equipmentManager = FindFirstObjectByType<EquipmentManager>(FindObjectsInactive.Include);

        // Icon -> ItemButton -> InventorySlot -> RemoveButton
        if (transform.parent != null && transform.parent.parent != null)
        {
            Transform slotTransform = transform.parent.parent;
            Transform btnTransform = slotTransform.Find("RemoveButton");
            if (btnTransform != null) removeButtonObj = btnTransform.gameObject;
        }

        RefreshRemoveButton();
    }

    // ================= XỬ LÝ SỰ KIỆN UI (FIX LỖI TOOLTIP) =================

    // 1. Khi tắt object (đóng túi đồ) -> Tắt Tooltip ngay lập tức
    private void OnDisable()
    {
        if (InventoryTooltip.Instance != null)
        {
            InventoryTooltip.Instance.HideTooltip();
        }
    }

    // 2. Khi di chuột vào -> Hiện Tooltip
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (itemImage.enabled && currentData != null && InventoryTooltip.Instance != null)
        {
            InventoryTooltip.Instance.ShowTooltip(currentData, upgradeLevel);
        }
    }

    // 3. Khi di chuột ra -> Tắt Tooltip
    public void OnPointerExit(PointerEventData eventData)
    {
        if (InventoryTooltip.Instance != null)
        {
            InventoryTooltip.Instance.HideTooltip();
        }
    }

    // ======================================================================

    public void SetItem(Sprite sprite, ItemType type, GameObject prefab, WeaponData data)
    {
        itemType = type;
        myPrefab = prefab;
        currentData = data; // Lưu lại data
        if (itemImage != null)
        {
            itemImage.sprite = sprite;
            itemImage.enabled = (sprite != null);
        }
        RefreshRemoveButton();
    }

    public void ClearItem()
    {
        if (itemImage != null)
        {
            itemImage.sprite = null;
            itemImage.enabled = false;
        }
        RefreshRemoveButton();
    }

    private void RefreshRemoveButton()
    {
        if (removeButtonObj == null || itemImage == null) return;
        bool show = itemImage.enabled && itemImage.sprite != null;
        if (removeButtonObj.activeSelf != show) removeButtonObj.SetActive(show);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // chỉ nhận chuột trái
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (itemImage == null || !itemImage.enabled || itemImage.sprite == null ||
            itemImage.sprite.name == "Icon" || itemImage.sprite.name == "EmptySlot")
        {
            //Debug.Log("Đây là ô trống, không gửi lệnh trang bị.");
            return;
        }

        bool enhanceMode =
            EnhancementPanel.Instance != null &&
            EnhancementPanel.Instance.IsOpen();

        //Debug.Log($"[InventoryItem] Click on {name} | enhanceMode={enhanceMode}");

        // 🔸 1) Đang ở màn NÂNG CẤP → đưa item sang ô dấu +
        if (enhanceMode)
        {
            EnhancementPanel.Instance.TryInsert(this);
            return; // ⛔ không equip
        }

        // 🔸 2) Bình thường → EQUIP / tháo trang bị như cũ
        if (equipmentManager == null)
            equipmentManager = FindFirstObjectByType<EquipmentManager>(FindObjectsInactive.Include);

        if (equipmentManager == null) return;

        WeaponData returnedData;
        int returnedLevel; // Biến hứng level trả về

        Sprite returnedSprite = equipmentManager.EquipItem(
            itemType,
            itemImage.sprite,
            myPrefab,
            upgradeLevel,
            out returnedData,
            out returnedLevel); // Nhận level từ EquipmentManager

        if (returnedSprite != null)
        {
            if (returnedData != null)
            {
                // Nếu có đầy đủ dữ liệu -> Hoán đổi hoàn hảo
                this.SetItem(returnedSprite, itemType, returnedData.prefab, returnedData);
                this.SetUpgradeLevel(returnedLevel); // Gán lại level cũ cho món đồ vừa về
                //Debug.Log($"Đã hoán đổi: {returnedData.displayName} về vị trí cũ với Lv {returnedLevel}.");
            }
            else
            {
                this.SetItem(returnedSprite, itemType, myPrefab, null);
                this.SetUpgradeLevel(returnedLevel);
                //Debug.LogWarning("Hoán đổi item: Có hình ảnh trả về nhưng không tìm thấy Data.");
            }
        }
        else
        {
            ClearItem();
        }

        RefreshRemoveButton();
        equipmentManager.BindPreviewNow();
    }

    public Sprite GetItemSprite()
    {
        if (itemImage == null) return null;
        return itemImage.sprite;
    }
}
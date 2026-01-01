using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryItem : MonoBehaviour, IPointerClickHandler
{
    public enum ItemType { Weapon, Head, Chest, Legs }
    public ItemType itemType;

    [SerializeField] private Image itemImage;

    private EquipmentManager equipmentManager;
    private GameObject removeButtonObj;
    [Header("Upgrade")]
    public int upgradeLevel = 1;             // cấp hiện tại
    public const int MaxUpgradeLevel = 4;
    public int GetUpgradeLevel()
    {
        return upgradeLevel;
    }

    public void SetUpgradeLevel(int level)
    {
        upgradeLevel = Mathf.Clamp(level, 1, MaxUpgradeLevel);

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

    public void SetItem(Sprite sprite, ItemType type)
    {
        itemType = type;
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

        if (itemImage == null || !itemImage.enabled || itemImage.sprite == null)
            return;

        bool enhanceMode =
            EnhancementPanel.Instance != null &&
            EnhancementPanel.Instance.IsOpen();

        Debug.Log($"[InventoryItem] Click on {name} | enhanceMode={enhanceMode}");

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

        Sprite returned = equipmentManager.EquipItem(itemType, itemImage.sprite);

        if (returned != null)
        {
            itemImage.sprite = returned;
            itemImage.enabled = true;
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

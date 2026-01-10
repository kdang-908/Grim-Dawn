using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryGridManager : MonoBehaviour
{
    [Header("UI References (Túi Đồ)")]
    public List<Image> inventorySlots = new List<Image>();

    [Header("UI References (Trang Bị Đang Mặc - Scene 1 Only)")]
    // Vẫn cần kéo thả ở Scene 1 để tham chiếu nếu cần, nhưng sẽ dùng EquipmentManager
    public InventoryItem headSlotUI;
    public InventoryItem chestSlotUI;
    public InventoryItem legSlotUI;
    public InventoryItem weaponSlotUI;

    [Header("Data Khởi Đầu")]
    public List<WeaponData> startItems = new List<WeaponData>();

    private bool hasInitialized = false;
    private EquipmentManager _cachedEquipManager; // Cache để tránh mất kết nối khi chuyển Scene

    
    // BỘ NHỚ TĨNH (STATIC MEMORY) - Sống qua các Scene
    
    public static List<SavedItemInfo> GlobalInventorySave = new List<SavedItemInfo>();
    public static bool HasSavedData = false;

    [System.Serializable]
    public class SavedItemInfo
    {
        public WeaponData data;
        public int level;
        public InventoryItem.ItemType type;
        public bool isEquipped;
    }

    void Awake()
    {
        // TÌM VÀ GIỮ LIÊN KẾT VỚI EQUIPMENT MANAGER 
        _cachedEquipManager = FindFirstObjectByType<EquipmentManager>();
        if (_cachedEquipManager == null)
        {
            Debug.LogWarning("⚠️ [Inventory] Không tìm thấy EquipmentManager trong Scene này. (Nếu đây là Scene Menu thì OK)");
        }
    }

    void Start()
    {
        InitData();
    }

    private void OnDestroy()
    {
        SaveInventoryState();
    }

    public void SaveInventoryState()
    {
        if (inventorySlots == null) return;

        GlobalInventorySave.Clear();
        Debug.Log("💾 [Inventory] BẮT ĐẦU LƯU DỮ LIỆU...");

        // 1. LƯU ĐỒ TRONG TÚI
        int bagCount = 0;
        foreach (Image slot in inventorySlots)
        {
            if (slot == null || !slot.enabled || slot.sprite == null) continue;

            InventoryItem item = slot.GetComponentInParent<InventoryItem>(true);
            if (item != null)
            {
                WeaponData wData = item.GetCurrentData();
                if (wData != null)
                {
                    SavedItemInfo info = new SavedItemInfo();
                    info.data = wData;
                    info.level = item.GetUpgradeLevel();
                    info.type = item.itemType;
                    info.isEquipped = false;
                    GlobalInventorySave.Add(info);
                    bagCount++;
                }
            }
        }
        Debug.Log($"   -> Đã lưu {bagCount} món trong túi.");

        // 2. LƯU ĐỒ ĐANG MẶC (Sử dụng Cache)
        EquipmentManager targetManager = _cachedEquipManager;

        // Nếu cache bị mất, tìm lại 
        if (targetManager == null) targetManager = FindFirstObjectByType<EquipmentManager>();

        if (targetManager != null)
        {
            Debug.Log("   -> Đang kiểm tra EquipmentManager...");

            // Lưu Vũ khí
            if (targetManager.currentWeapon != null)
            {
                AddEquippedItemToSave(targetManager.currentWeapon, targetManager.weaponUpgradeLevel, InventoryItem.ItemType.Weapon);
            }
            else Debug.Log("      [Weapon] Không có vũ khí (currentWeapon == null)");

            // Lưu Mũ
            if (targetManager.currentHelmet != null)
            {
                AddEquippedItemToSave(targetManager.currentHelmet, targetManager.helmetUpgradeLevel, InventoryItem.ItemType.Head);
            }
            else Debug.Log("      [Helmet] Không có mũ (currentHelmet == null)");

            // Lưu Giáp
            if (targetManager.currentChest != null)
            {
                AddEquippedItemToSave(targetManager.currentChest, targetManager.chestUpgradeLevel, InventoryItem.ItemType.Chest);
            }
            else Debug.Log("      [Chest] Không có giáp (currentChest == null)");

            
        }
        HasSavedData = true;
        Debug.Log($"✅ TỔNG KẾT: Đã lưu {GlobalInventorySave.Count} món vào bộ nhớ Global.");
    }

    private void AddEquippedItemToSave(WeaponData data, int level, InventoryItem.ItemType type)
    {
        if (data == null) return;

        SavedItemInfo info = new SavedItemInfo();
        info.data = data;
        info.level = level;
        info.type = type;
        info.isEquipped = true; // khi Init sẽ nhét vào túi

        GlobalInventorySave.Add(info);
        Debug.Log($"      + [LƯU THÀNH CÔNG] Trang bị: {data.name} (Lv.{level})");
    }

    public void InitData()
    {
        if (hasInitialized) return;

        RefreshInventorySlots();
        ClearInventory();

        if (HasSavedData && GlobalInventorySave.Count > 0)
        {
            Debug.Log($"♻️ [InitData] Khôi phục {GlobalInventorySave.Count} món từ Scene trước (Tất cả về túi)...");

            foreach (SavedItemInfo info in GlobalInventorySave)
            {
                if (info.data == null) continue;
                // Nhét tất cả vào túi 
                AddItemBackToInventory(info.data.icon, info.type, info.data.prefab, info.data, info.level);
            }
        }
        else
        {
            Debug.Log($"🚀 [InitData] Game mới hoặc không có dữ liệu cũ. Nạp StartItems...");
            if (startItems != null)
            {
                foreach (var itemData in startItems)
                {
                    if (itemData == null) continue;
                    AddItemBackToInventory(itemData.icon, itemData.itemType, itemData.prefab, itemData, 1);
                }
            }
        }

        hasInitialized = true;
    }

    void ClearInventory()
    {
        foreach (Image slot in inventorySlots)
        {
            if (slot == null) continue;
            slot.sprite = null;
            slot.enabled = false;
            InventoryItem itemScript = slot.GetComponentInParent<InventoryItem>(true);
            if (itemScript != null) itemScript.SetUpgradeLevel(1);
        }
        ClearSingleSlot(headSlotUI);
        ClearSingleSlot(chestSlotUI);
        ClearSingleSlot(legSlotUI);
        ClearSingleSlot(weaponSlotUI);
    }

    void ClearSingleSlot(InventoryItem item)
    {
        if (item == null) return;
        Image img = item.GetComponent<Image>() ?? item.transform.Find("Icon")?.GetComponent<Image>();
        if (img != null)
        {
            img.sprite = null;
            img.enabled = false;
        }
        item.SetUpgradeLevel(1);
    }

    public void RefreshInventorySlots()
    {
        inventorySlots.Clear();
        HashSet<Image> addedImages = new HashSet<Image>();
        InventoryItem[] allItems = GetComponentsInChildren<InventoryItem>(true);

        foreach (InventoryItem itemScript in allItems)
        {
            if (itemScript == headSlotUI || itemScript == chestSlotUI || itemScript == legSlotUI || itemScript == weaponSlotUI)
                continue;

            Transform iconTrans = itemScript.transform.Find("Icon");
            Image iconImg = (iconTrans != null) ? iconTrans.GetComponent<Image>() : itemScript.GetComponent<Image>();

            if (iconImg != null && !addedImages.Contains(iconImg))
            {
                inventorySlots.Add(iconImg);
                addedImages.Add(iconImg);
            }
        }
    }

    public bool AddItemBackToInventory(Sprite itemSprite, InventoryItem.ItemType newType, GameObject itemPrefab, WeaponData data, int level)
    {
        if (itemSprite == null) return false;

        foreach (Image slotImage in inventorySlots)
        {
            if (slotImage == null) continue;
            bool isEmpty = (slotImage.enabled == false) || (slotImage.sprite == null);

            if (isEmpty)
            {
                InventoryItem item = slotImage.GetComponentInParent<InventoryItem>(true);
                if (item != null)
                {
                    item.SetItem(itemSprite, newType, itemPrefab, data);
                    item.SetUpgradeLevel(level);
                    slotImage.sprite = itemSprite;
                    slotImage.enabled = true;
                    if (!item.gameObject.activeSelf) item.gameObject.SetActive(true);
                    return true;
                }
            }
        }
        return false;
    }

    public void SyncItemLevel(WeaponData targetData, int oldLevel, int newLevel)
    {
        if (targetData == null) return;
        if (!hasInitialized) InitData();

        string targetIconName = (targetData.icon != null) ? targetData.icon.name : "NULL";
        string cleanTarget = targetIconName.ToLower().Replace(" ", "").Replace("_", "").Replace("-", "");

        foreach (Image slot in inventorySlots)
        {
            if (slot == null) continue;
            InventoryItem item = slot.GetComponentInParent<InventoryItem>(true);
            if (item == null || item.GetItemSprite() == null) continue;

            string itemSpriteName = item.GetItemSprite().name;
            string cleanItem = itemSpriteName.ToLower().Replace(" ", "").Replace("_", "").Replace("-", "");

            if (cleanItem == cleanTarget)
            {
                if (item.GetUpgradeLevel() == oldLevel || item.GetUpgradeLevel() == newLevel || item.GetUpgradeLevel() == 0 || item.GetUpgradeLevel() == 1)
                {
                    item.SetUpgradeLevel(newLevel);
                    item.SetItem(item.GetItemSprite(), item.itemType, null, targetData);
                    Debug.Log($"✅ [SYNC SUCCESS] Update: {itemSpriteName} -> Lv{newLevel}");
                }
            }
        }
        SaveInventoryState();
    }
}
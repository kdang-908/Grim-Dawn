using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryGridManager : MonoBehaviour
{
    [Header("UI References")]
    public List<Image> inventorySlots = new List<Image>();

    public static List<SavedInvItem> GlobalInventorySave = new List<SavedInvItem>();
    [System.Serializable]
    public class SavedInvItem
    {
        public WeaponData data;
        public int level;
    }
    [Header("Data Khởi Đầu")]
    public List<WeaponData> startItems = new List<WeaponData>();

    private bool hasInitialized = false;
    [SerializeField] private Transform inventoryRoot;
    void Start()
    {
        InitData();
    }

    public void InitData()
    {
        Debug.Log($"[DEBUG] GlobalInventorySave at InitData = {(GlobalInventorySave == null ? -1 : GlobalInventorySave.Count)}");

        if (hasInitialized) return;

        RefreshInventorySlots();
        ClearInventory();

        // ✅ 1) Ưu tiên nạp dữ liệu đã save (nếu có)
        if (GlobalInventorySave != null && GlobalInventorySave.Count > 0)
        {
            Debug.Log($"📦 [InitData] Load from GlobalInventorySave: {GlobalInventorySave.Count} items");

            foreach (var s in GlobalInventorySave)
            {
                if (s == null || s.data == null || s.data.icon == null) continue;

                AddItemBackToInventory(
                    s.data.icon,
                    s.data.itemType,
                    s.data.prefab,
                    s.data,
                    Mathf.Max(1, s.level)
                );
            }

            hasInitialized = true;
            return; // QUAN TRỌNG: không nạp startItems nữa
        }

        // ✅ 2) Nếu chưa có save thì mới nạp StartItems (level 1)
        Debug.Log($"🚀 [InitData] Không có save, nạp startItems: {startItems.Count} món...");

        if (startItems != null && startItems.Count > 0)
        {
            for (int i = 0; i < startItems.Count; i++)
            {
                WeaponData itemData = startItems[i];
                if (itemData == null || itemData.icon == null) continue;

                AddItemBackToInventory(itemData.icon, itemData.itemType, itemData.prefab, itemData, 1);
            }
        }

        hasInitialized = true;
    }
    // Hàm dọn dẹp
    void ClearInventory()
    {
        foreach (Image slot in inventorySlots)
        {
            if (slot == null) continue;
            slot.sprite = null;
            slot.enabled = false;

            
            InventoryItem itemScript = slot.GetComponentInParent<InventoryItem>(true);
            if (itemScript != null)
            {
                // Nếu cần reset data trong script thì làm ở đây
            }
        }
    }


    // HÀM REFRESH 
    void RefreshInventorySlots()
    {
        inventorySlots.Clear();

        Transform root = inventoryRoot != null ? inventoryRoot : transform;

        var allItems = root.GetComponentsInChildren<InventoryItem>(true);

        foreach (var itemScript in allItems)
        {
            // CHỈ lấy icon của slot item (đúng object chứa InventoryItem)
            Transform iconTrans = itemScript.transform.Find("Icon");
            Image iconImg = null;

            if (iconTrans != null) iconImg = iconTrans.GetComponent<Image>();
            else iconImg = itemScript.GetComponent<Image>();

            if (iconImg != null)
                inventorySlots.Add(iconImg);
        }

        Debug.Log($"[Refresh] Inventory slots found = {inventorySlots.Count}");
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
                // để tìm script kể cả khi object đang tắt
                InventoryItem item = slotImage.GetComponentInParent<InventoryItem>(true);

                if (item != null)
                {
                    item.SetItem(itemSprite, newType, itemPrefab, data);
                    item.SetUpgradeLevel(level);

                    slotImage.sprite = itemSprite;
                    slotImage.enabled = true;

                    // Nếu object cha đang tắt thì bật nó lên 
                    if (!item.gameObject.activeSelf) item.gameObject.SetActive(true);

                    return true;
                }
            }
        }

        Debug.LogWarning($"[Inventory] Túi đầy! Không thể thêm: {data.name}");
        return false;
    }

    public void SyncItemLevel(WeaponData targetData, int oldLevel, int newLevel)
    {
        if (targetData == null) return;
        InitData();

        string targetIconName = (targetData.icon != null) ? targetData.icon.name : "NULL";
        string cleanTarget = targetIconName.ToLower().Replace(" ", "").Replace("_", "").Replace("-", "");

        Debug.Log($"🔍 [DEBUG] Đang đi tìm: '{targetIconName}' (Clean: {cleanTarget})");
        Debug.Log("📋 --- DANH SÁCH ĐỒ THỰC TẾ TRONG TÚI ---");

        bool foundAny = false;

        foreach (Image slot in inventorySlots)
        {
            if (slot == null) continue;
            InventoryItem item = slot.GetComponentInParent<InventoryItem>(true);
            if (item == null || item.GetItemSprite() == null) continue;

            string itemSpriteName = item.GetItemSprite().name;
            string cleanItem = itemSpriteName.ToLower().Replace(" ", "").Replace("_", "").Replace("-", "");

            
            Debug.Log($"   + Slot {slot.name} đang chứa: '{itemSpriteName}' (Clean: {cleanItem})");

            // So sánh
            bool isMatch = (cleanItem == cleanTarget);

            if (isMatch)
            {
                if (item.GetUpgradeLevel() == newLevel ||
                    item.GetUpgradeLevel() == oldLevel ||
                    item.GetUpgradeLevel() == 0)
                {
                    if (item.GetUpgradeLevel() != newLevel) item.SetUpgradeLevel(newLevel);
                    item.SetItem(item.GetItemSprite(), item.itemType, null, targetData);
                    item.SetUpgradeLevel(newLevel);
                    Debug.Log($"✅ [SYNC SUCCESS] Update: {itemSpriteName} -> Lv{newLevel}");
                    foundAny = true;
                }
            }
        }
    }
    public void SaveInventoryState()
    {
        RefreshInventorySlots();

        var temp = new List<SavedInvItem>();

        foreach (var slotImg in inventorySlots)
        {
            if (slotImg == null || slotImg.sprite == null) continue;

            var item = slotImg.GetComponentInParent<InventoryItem>(true);
            if (item == null) continue;
            if (inventoryRoot != null && !item.transform.IsChildOf(inventoryRoot)) continue;
            var data = item.GetCurrentData();
            if (data == null) continue;

            temp.Add(new SavedInvItem
            {
                data = data,
                level = Mathf.Max(1, item.GetUpgradeLevel())
            });
        }

        if (temp.Count == 0)
        {
            Debug.LogWarning("[Inventory] Skip SaveInventoryState vì không có item nào.");
            return;
        }

        GlobalInventorySave.Clear();
        GlobalInventorySave.AddRange(temp);

        Debug.Log($"✅ [Inventory] Saved {GlobalInventorySave.Count} items");
    }
}
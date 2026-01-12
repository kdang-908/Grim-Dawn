using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryGridManager : MonoBehaviour
{
    [Header("UI References")]
    public List<Image> inventorySlots = new List<Image>();

    // ✅ SAVE CHUNG CHO TOÀN GAME (Inventory thường + Forge/Enhance cùng nhìn chung)
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

    // ✅ NEW: Cho phép UI khác (Forge/Enhance) reload lại từ GlobalInventorySave
    // dùng khi Shop mua xong / nâng cấp xong / đổi scene...
    public void ReloadFromGlobalSave()
    {
        RefreshInventorySlots();
        ClearInventory();

        if (GlobalInventorySave != null && GlobalInventorySave.Count > 0)
        {
            Debug.Log($"🔁 [ReloadFromGlobalSave] Reload: {GlobalInventorySave.Count} items");

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
        }

        // Cho phép gọi nhiều lần
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
            Transform iconTrans = itemScript.transform.Find("Icon");
            Image iconImg = null;

            if (iconTrans != null) iconImg = iconTrans.GetComponent<Image>();
            else iconImg = itemScript.GetComponent<Image>();

            if (iconImg != null)
                inventorySlots.Add(iconImg);
        }

        Debug.Log($"[Refresh] Inventory slots found = {inventorySlots.Count} | manager={name}");
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

        Debug.LogWarning($"[Inventory] Túi đầy! Không thể thêm: {data.name} | manager={name}");
        return false;
    }

    public void SyncItemLevel(WeaponData targetData, int oldLevel, int newLevel)
    {
        if (targetData == null) return;
        InitData();

        string targetIconName = (targetData.icon != null) ? targetData.icon.name : "NULL";
        string cleanTarget = targetIconName.ToLower().Replace(" ", "").Replace("_", "").Replace("-", "");

        Debug.Log($"🔍 [DEBUG] Đang đi tìm: '{targetIconName}' (Clean: {cleanTarget})");

        foreach (Image slot in inventorySlots)
        {
            if (slot == null) continue;
            InventoryItem item = slot.GetComponentInParent<InventoryItem>(true);
            if (item == null || item.GetItemSprite() == null) continue;

            string itemSpriteName = item.GetItemSprite().name;
            string cleanItem = itemSpriteName.ToLower().Replace(" ", "").Replace("_", "").Replace("-", "");

            bool isMatch = (cleanItem == cleanTarget);

            if (isMatch)
            {
                if (item.GetUpgradeLevel() != newLevel)
                    item.SetUpgradeLevel(newLevel);

                item.SetItem(item.GetItemSprite(), item.itemType, null, targetData);
                item.SetUpgradeLevel(newLevel);

                Debug.Log($"✅ [SYNC SUCCESS] Update: {itemSpriteName} -> Lv{newLevel}");
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

    // ✅ SHOP sẽ gọi hàm này
    public bool AddItemFromShop(WeaponData data, int level)
    {
        if (data == null || data.icon == null) return false;

        bool ok = AddItemBackToInventory(data.icon, data.itemType, data.prefab, data, Mathf.Max(1, level));
        if (ok) SaveInventoryState(); // đồng bộ qua scene (GlobalInventorySave)

        return ok;
    }
}

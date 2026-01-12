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

    [SerializeField] private Transform inventoryRoot;

    private bool hasInitialized = false;

    void Start()
    {
        InitData();
    }

    // =========================
    // INIT / LOAD
    // =========================
    public void InitData()
    {
        if (hasInitialized) return;

        RefreshInventorySlots();
        ClearInventory();

        // ✅ 1) Ưu tiên nạp dữ liệu đã save (nếu có)
        if (GlobalInventorySave != null && GlobalInventorySave.Count > 0)
        {
            Debug.Log($"📦 [InitData] Load from GlobalInventorySave: {GlobalInventorySave.Count} items | manager={name}");
            LoadFromGlobalSaveToUI();
            hasInitialized = true;
            return;
        }

        // ✅ 2) Nếu chưa có save thì mới nạp StartItems (level 1)
        Debug.Log($"🚀 [InitData] No save, load startItems: {startItems.Count} items | manager={name}");

        if (startItems != null && startItems.Count > 0)
        {
            for (int i = 0; i < startItems.Count; i++)
            {
                WeaponData itemData = startItems[i];
                if (itemData == null || itemData.icon == null) continue;

                AddItemBackToInventory(itemData.icon, itemData.itemType, itemData.prefab, itemData, 1);
            }
        }

        // ❌ TUYỆT ĐỐI KHÔNG Save ở đây (vì UI có thể chưa fill sprite -> temp=0 -> wipe global)
        // SaveInventoryState();

        hasInitialized = true;
    }

    // ✅ NEW: Cho phép UI khác (Forge/Enhance) reload lại từ GlobalInventorySave
    public void ReloadFromGlobalSave()
    {
        RefreshInventorySlots();
        ClearInventory();

        LoadFromGlobalSaveToUI();

        // Cho phép gọi nhiều lần
        hasInitialized = true;
    }

    private void LoadFromGlobalSaveToUI()
    {
        if (GlobalInventorySave == null || GlobalInventorySave.Count == 0) return;

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

    // =========================
    // UI HELPERS
    // =========================
    void ClearInventory()
    {
        foreach (Image slot in inventorySlots)
        {
            if (slot == null) continue;
            slot.sprite = null;
            slot.enabled = false;
        }
    }

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

    // =========================
    // SAVE / SYNC
    // =========================
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

        // ✅ QUAN TRỌNG: temp rỗng thì KHÔNG được wipe Global
        if (temp.Count == 0)
        {
            Debug.LogWarning($"[Inventory] Skip SaveInventoryState (temp=0) to avoid wiping Global | manager={name}");
            return;
        }

        GlobalInventorySave.Clear();
        GlobalInventorySave.AddRange(temp);

        Debug.Log($"✅ [Inventory] Saved Global = {GlobalInventorySave.Count} items | by manager={name}");

        // ✅ Sau khi save -> reload toàn bộ túi khác (Inventory thường + Forge/Enhance)
        ReloadAllManagers();
    }

    // ✅ SHOP sẽ gọi hàm này
    public bool AddItemFromShop(WeaponData data, int level)
    {
        if (data == null || data.icon == null) return false;

        InitData(); // đảm bảo slots đã scan

        bool ok = AddItemBackToInventory(data.icon, data.itemType, data.prefab, data, Mathf.Max(1, level));
        if (ok)
        {
            SaveInventoryState();
        }
        return ok;
    }

    // ✅ Nâng cấp xong gọi để sync level
    public void SyncItemLevel(WeaponData targetData, int oldLevel, int newLevel)
    {
        if (targetData == null) return;

        InitData();

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
                item.SetUpgradeLevel(newLevel);
                item.SetItem(item.GetItemSprite(), item.itemType, null, targetData);
            }
        }

        SaveInventoryState();
    }

    // =========================
    // GLOBAL RELOAD FOR ALL UI
    // =========================
    private static void ReloadAllManagers()
    {
        var managers = Object.FindObjectsOfType<InventoryGridManager>(true);
        foreach (var m in managers)
        {
            if (m == null) continue;
            m.ReloadFromGlobalSave();
        }
    }
}

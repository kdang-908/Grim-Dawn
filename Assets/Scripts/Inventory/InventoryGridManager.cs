using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryGridManager : MonoBehaviour
{
    [Header("UI References (AUTO)")]
    public List<Image> inventorySlots = new List<Image>();

    [Header("IMPORTANT - Root chứa đúng các slot của túi này")]
    [SerializeField] private Transform inventoryRoot;

    public static List<SavedInvItem> GlobalInventorySave = new List<SavedInvItem>();

    [System.Serializable]
    public class SavedInvItem
    {
        public WeaponData data;
        public int level;
    }

    [Header("Data Khởi Đầu (CHỈ DÙNG CHO INVENTORY CHÍNH)")]
    public List<WeaponData> startItems = new List<WeaponData>();

    [Header("Init")]
    [Tooltip("Chỉ Inventory CHÍNH bật true để seed startItems. Forge/Enhance để false.")]
    public bool isMainSeeder = false;

    private static bool seededOnce = false;

    // ✅ registry các manager đang sống để refresh nhẹ (không FindObjectsOfType)
    private static readonly HashSet<InventoryGridManager> Alive = new HashSet<InventoryGridManager>();

    private bool hasSeededOrLoadedOnce = false;

    void OnEnable()
    {
        Alive.Add(this);
        EnsureLoaded(forceReloadFromGlobal: true);
    }

    void OnDisable()
    {
        Alive.Remove(this);
    }

    void Awake()
    {
        if (gameObject.activeInHierarchy)
            EnsureLoaded();
    }

    void Start()
    {
        if (gameObject.activeInHierarchy)
            EnsureLoaded();
    }

    // =========================
    // CORE LOAD
    // =========================
    public void EnsureLoaded(bool forceReloadFromGlobal = false)
    {
        RefreshInventorySlots();

        if (inventoryRoot == null || inventorySlots.Count == 0)
            return;

        if (forceReloadFromGlobal)
        {
            ClearInventoryUIOnly();
            LoadFromGlobalSaveToUI();
            hasSeededOrLoadedOnce = true;
            return;
        }

        if (hasSeededOrLoadedOnce) return;

        ClearInventoryUIOnly();

        if (GlobalInventorySave != null && GlobalInventorySave.Count > 0)
        {
            LoadFromGlobalSaveToUI();
            hasSeededOrLoadedOnce = true;
            return;
        }

        if (!seededOnce && isMainSeeder)
        {
            seededOnce = true;

            if (startItems != null && startItems.Count > 0)
            {
                for (int i = 0; i < startItems.Count; i++)
                {
                    var itemData = startItems[i];
                    if (itemData == null || itemData.icon == null) continue;

                    // add vào UI
                    AddItemBackToInventory(itemData.icon, itemData.itemType, itemData.prefab, itemData, 1);

                    // add vào GLOBAL (SOURCE OF TRUTH)
                    GlobalInventorySave.Add(new SavedInvItem { data = itemData, level = 1 });
                }
            }
        }

        // cuối cùng đổ UI theo Global để đồng bộ
        ReloadFromGlobalSave();
        hasSeededOrLoadedOnce = true;
    }

    public void ReloadFromGlobalSave()
    {
        RefreshInventorySlots();
        if (inventoryRoot == null || inventorySlots.Count == 0) return;

        ClearInventoryUIOnly();
        LoadFromGlobalSaveToUI();
        hasSeededOrLoadedOnce = true;
    }

    void LoadFromGlobalSaveToUI()
    {
        if (GlobalInventorySave == null || GlobalInventorySave.Count == 0) return;

        for (int i = 0; i < GlobalInventorySave.Count; i++)
        {
            var s = GlobalInventorySave[i];
            if (s == null || s.data == null || s.data.icon == null) continue;

            // ✅ Đặt item vào slot trống (KHÔNG check HasItemInUI theo buyOnce nữa để tránh bỏ sót)
            AddItemBackToInventory(
                s.data.icon,
                s.data.itemType,
                s.data.prefab,
                s.data,
                Mathf.Max(1, s.level)
            );
        }
    }

    void ClearInventoryUIOnly()
    {
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            var slot = inventorySlots[i];
            if (slot == null) continue;

            slot.sprite = null;
            slot.enabled = false;

            var item = slot.GetComponentInParent<InventoryItem>(true);
            if (item != null)
            {
                item.SetItem(null, item.itemType, null, null);
                item.SetUpgradeLevel(1);
            }
        }
    }

    void RefreshInventorySlots()
    {
        inventorySlots.Clear();

        if (inventoryRoot == null)
            return;

        var allItems = inventoryRoot.GetComponentsInChildren<InventoryItem>(true);

        foreach (var itemScript in allItems)
        {
            Transform iconTrans = itemScript.transform.Find("Icon");
            Image iconImg = iconTrans ? iconTrans.GetComponent<Image>() : itemScript.GetComponent<Image>();

            if (iconImg != null)
                inventorySlots.Add(iconImg);
        }
    }

    // =========================
    // ADD (UI)
    // =========================
    public bool AddItemBackToInventory(Sprite itemSprite, InventoryItem.ItemType newType, GameObject itemPrefab, WeaponData data, int level)
    {
        if (itemSprite == null) return false;

        for (int i = 0; i < inventorySlots.Count; i++)
        {
            var slotImage = inventorySlots[i];
            if (slotImage == null) continue;

            bool isEmpty = (!slotImage.enabled) || (slotImage.sprite == null);
            if (!isEmpty) continue;

            var item = slotImage.GetComponentInParent<InventoryItem>(true);
            if (item != null)
            {
                item.SetItem(itemSprite, newType, itemPrefab, data);
                item.SetUpgradeLevel(Mathf.Max(1, level));

                slotImage.sprite = itemSprite;
                slotImage.enabled = true;

                if (!item.gameObject.activeSelf) item.gameObject.SetActive(true);
                return true;
            }
        }
        return false;
    }

    // =========================
    // ✅ SHOP -> add vào GLOBAL trước, rồi Reload UI
    // =========================
    public bool AddItemFromShop(WeaponData data, int level)
    {
        if (data == null || data.icon == null) return false;

        EnsureLoaded();

        int lv = Mathf.Max(1, level);

        // buyOnce => không cho mua trùng
        if (data.buyOnce)
        {
            for (int i = 0; i < GlobalInventorySave.Count; i++)
            {
                var s = GlobalInventorySave[i];
                if (s == null || s.data == null) continue;

                if (s.data == data) return false;
                if (!string.IsNullOrEmpty(s.data.name) && s.data.name == data.name) return false;
            }
        }

        GlobalInventorySave.Add(new SavedInvItem { data = data, level = lv });
        ReloadFromGlobalSave();
        RefreshActiveManagers();
        return true;
    }

    // =========================
    // ✅ EQUIP: remove khỏi GLOBAL (source of truth) + reload UI
    // =========================
    public bool RemoveItemForEquip(WeaponData data, int level)
    {
        if (data == null) return false;

        EnsureLoaded();

        int targetLevel = Mathf.Max(1, level);

        // remove 1 item khỏi GLOBAL
        for (int i = GlobalInventorySave.Count - 1; i >= 0; i--)
        {
            var s = GlobalInventorySave[i];
            if (s == null || s.data == null) continue;

            bool match = (s.data == data && Mathf.Max(1, s.level) == targetLevel) ||
                         (data.buyOnce && s.data.name == data.name && Mathf.Max(1, s.level) == targetLevel);

            if (match)
            {
                GlobalInventorySave.RemoveAt(i);
                ReloadFromGlobalSave();
                RefreshActiveManagers();
                return true;
            }
        }

        return false;
    }

    // =========================
    // ✅ UNEQUIP: add lại GLOBAL + reload UI
    // =========================
    public bool AddBackFromEquip(WeaponData data, int level)
    {
        if (data == null || data.icon == null) return false;

        EnsureLoaded();

        GlobalInventorySave.Add(new SavedInvItem
        {
            data = data,
            level = Mathf.Max(1, level)
        });

        ReloadFromGlobalSave();
        RefreshActiveManagers();
        return true;
    }

    // =========================
    // ✅ DELETE VĨNH VIỄN (thùng rác)
    // =========================
    public bool DeleteItemForever(WeaponData data, int level)
    {
        if (data == null) return false;

        EnsureLoaded();

        int targetLevel = Mathf.Max(1, level);

        for (int i = GlobalInventorySave.Count - 1; i >= 0; i--)
        {
            var s = GlobalInventorySave[i];
            if (s == null || s.data == null) continue;

            bool match = (s.data == data && Mathf.Max(1, s.level) == targetLevel) ||
                         (data.buyOnce && s.data.name == data.name && Mathf.Max(1, s.level) == targetLevel);

            if (match)
            {
                GlobalInventorySave.RemoveAt(i);
                ReloadFromGlobalSave();
                RefreshActiveManagers();
                return true;
            }
        }
        return false;
    }

    static void RefreshActiveManagers()
    {
        foreach (var m in Alive)
        {
            if (m == null) continue;
            if (!m.isActiveAndEnabled) continue;
            m.ReloadFromGlobalSave();
        }
    }
    // =============================
    // BACKWARD COMPATIBILITY (Fix lỗi CS1061)
    // =============================

    // 1) InventoryDeletePopup gọi RemoveItem()
    public bool RemoveItem(WeaponData data)
    {
        if (data == null) return false;

        for (int i = GlobalInventorySave.Count - 1; i >= 0; i--)
        {
            var s = GlobalInventorySave[i];
            if (s == null || s.data == null) continue;

            bool match = (s.data == data) || (data.buyOnce && s.data.name == data.name);
            if (match)
            {
                GlobalInventorySave.RemoveAt(i);
                ReloadFromGlobalSave();
                return true;
            }
        }
        return false;
    }

    // 2) GameEndUIController gọi SaveInventoryState()
    public void SaveInventoryState()
    {
        // bản an toàn: không quét UI ghi đè Global
        ReloadFromGlobalSave();
    }

    // 3) EnhancementPanel gọi SyncItemLevel()
    public void SyncItemLevel(WeaponData targetData, int oldLevel, int newLevel)
    {
        if (targetData == null) return;

        int oldLv = Mathf.Max(1, oldLevel);
        int newLv = Mathf.Max(1, newLevel);

        for (int i = 0; i < GlobalInventorySave.Count; i++)
        {
            var s = GlobalInventorySave[i];
            if (s == null || s.data == null) continue;

            bool match = (s.data == targetData && Mathf.Max(1, s.level) == oldLv) ||
                         (targetData.buyOnce && s.data.name == targetData.name && Mathf.Max(1, s.level) == oldLv);

            if (match)
            {
                s.level = newLv;
                GlobalInventorySave[i] = s;
                break;
            }
        }

        ReloadFromGlobalSave();
    }

}

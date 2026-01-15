using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

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

    // tránh load/seed lại liên tục khi scene load
    private bool hasSeededOrLoadedOnce = false;

    void OnEnable()
    {
        Alive.Add(this);

        // ✅ Khi mở panel (false -> true): load lại từ Global để nhìn thấy đồ mới nhất
        EnsureLoaded(forceReloadFromGlobal: true);
    }

    void OnDisable()
    {
        Alive.Remove(this);
    }

    void Awake()
    {
        // Nếu panel active ngay lúc load scene
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
        {
            //Debug.LogWarning($"[InventoryGridManager] EnsureLoaded fail: inventoryRoot/slots invalid | manager={name}");
            return;
        }
        if (forceReloadFromGlobal)
        {
            ClearInventory();
            LoadFromGlobalSaveToUI();
            hasSeededOrLoadedOnce = true;
            return;
        }
        if (hasSeededOrLoadedOnce) return;

        ClearInventory();
        if (GlobalInventorySave != null && GlobalInventorySave.Count > 0)
        {
            //Debug.Log($"[Inventory] Load Global: {GlobalInventorySave.Count} items | manager={name}");
            LoadFromGlobalSaveToUI();
            hasSeededOrLoadedOnce = true;
            return;
        }
        if (!seededOnce && isMainSeeder)
        {
            seededOnce = true;
            //Debug.Log($"[Inventory] Seed startItems: {startItems.Count} | manager={name}");

            if (startItems != null && startItems.Count > 0)
            {
                for (int i = 0; i < startItems.Count; i++)
                {
                    var itemData = startItems[i];
                    if (itemData == null || itemData.icon == null) continue;

                    AddItemBackToInventory(itemData.icon, itemData.itemType, itemData.prefab, itemData, 1);
                }

                SaveInventoryState(); 
            }
        }
        else
        {
            //Debug.Log($"[Inventory] No Global, skip seed | isMainSeeder={isMainSeeder} seededOnce={seededOnce} | manager={name}");
        }

        hasSeededOrLoadedOnce = true;
    }

    public void ReloadFromGlobalSave()
    {
        RefreshInventorySlots();

        if (inventoryRoot == null || inventorySlots.Count == 0)
        {
            //Debug.LogWarning($"[InventoryGridManager] ReloadFromGlobalSave fail: inventoryRoot/slots invalid | manager={name}");
            return;
        }

        ClearInventory();
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

            if (HasItemInUI(s.data)) continue;

            AddItemBackToInventory(
                s.data.icon,
                s.data.itemType,
                s.data.prefab,
                s.data,
                Mathf.Max(1, s.level)
            );
        }
    }

    void ClearInventory()
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
        {
            //Debug.LogError($"[InventoryGridManager] inventoryRoot NULL ở '{name}'. Hãy kéo inventoryRoot đúng parent chứa slot của túi này!");
            return;
        }

        var allItems = inventoryRoot.GetComponentsInChildren<InventoryItem>(true);

        foreach (var itemScript in allItems)
        {
            Transform iconTrans = itemScript.transform.Find("Icon");
            Image iconImg = iconTrans ? iconTrans.GetComponent<Image>() : itemScript.GetComponent<Image>();

            if (iconImg != null)
                inventorySlots.Add(iconImg);
        }

        //Debug.Log($"[Inventory] Slots found = {inventorySlots.Count} | manager={name}");
    }

    // =========================
    // DUPLICATE CHECK
    // =========================
    bool HasItemInUI(WeaponData data)
    {
        if (data == null) return false;

        for (int i = 0; i < inventorySlots.Count; i++)
        {
            var slot = inventorySlots[i];
            if (slot == null || slot.sprite == null) continue;

            var item = slot.GetComponentInParent<InventoryItem>(true);
            if (item == null) continue;

            var d = item.GetCurrentData();
            if (d == null) continue;

            if (d == data) return true;

            if (data.buyOnce && !string.IsNullOrEmpty(d.name) && d.name == data.name)
                return true;
        }
        return false;
    }

    // =========================
    // ADD / SAVE
    // =========================
    public bool AddItemBackToInventory(Sprite itemSprite, InventoryItem.ItemType newType, GameObject itemPrefab, WeaponData data, int level)
    {
        if (itemSprite == null) return false;

        if (data != null && data.buyOnce && HasItemInUI(data))
            return false;

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

        //Debug.LogWarning($"[Inventory] Túi đầy! Không thể thêm: {(data != null ? data.name : "NULL")} | manager={name}");
        return false;
    }

    public void SaveInventoryState()
    {
        RefreshInventorySlots();
        if (inventoryRoot == null) return;

        var temp = new List<SavedInvItem>();

        for (int i = 0; i < inventorySlots.Count; i++)
        {
            var slotImg = inventorySlots[i];
            if (slotImg == null || slotImg.sprite == null) continue;

            var item = slotImg.GetComponentInParent<InventoryItem>(true);
            if (item == null) continue;

            if (!item.transform.IsChildOf(inventoryRoot)) continue;

            var data = item.GetCurrentData();
            if (data == null) continue;

            temp.Add(new SavedInvItem
            {
                data = data,
                level = Mathf.Max(1, item.GetUpgradeLevel())
            });
        }

        // ✅ LUÔN LƯU (kể cả 0) để khỏi bị “không lưu dữ liệu”
        GlobalInventorySave.Clear();
        GlobalInventorySave.AddRange(temp);

        //Debug.Log($"[Inventory] Saved Global = {GlobalInventorySave.Count} | by manager={name}");

        // ✅ Refresh nhẹ: chỉ refresh manager đang active (đang mở)
        RefreshActiveManagers();
    }

    static void RefreshActiveManagers()
    {
        foreach (var m in Alive)
        {
            if (m == null) continue;
            if (!m.isActiveAndEnabled) continue; // chỉ refresh cái đang mở
            m.ReloadFromGlobalSave();
        }
    }

    // =========================
    // SHOP
    // =========================
    public bool AddItemFromShop(WeaponData data, int level)
    {
        if (data == null || data.icon == null) return false;

        EnsureLoaded();

        if (data.buyOnce)
        {
            for (int i = 0; i < GlobalInventorySave.Count; i++)
            {
                var s = GlobalInventorySave[i];
                if (s == null || s.data == null) continue;

                if (s.data == data) return false;
                if (!string.IsNullOrEmpty(s.data.name) && s.data.name == data.name) return false;
            }

            if (HasItemInUI(data)) return false;
        }

        bool ok = AddItemBackToInventory(data.icon, data.itemType, data.prefab, data, Mathf.Max(1, level));
        if (ok) SaveInventoryState();

        return ok;
    }

    // =========================
    // SYNC LEVEL (ENHANCE)
    // =========================
    public void SyncItemLevel(WeaponData targetData, int oldLevel, int newLevel)
    {
        if (targetData == null) return;

        EnsureLoaded();

        for (int i = 0; i < inventorySlots.Count; i++)
        {
            var slot = inventorySlots[i];
            if (slot == null) continue;

            var item = slot.GetComponentInParent<InventoryItem>(true);
            if (item == null || item.GetItemSprite() == null) continue;

            var d = item.GetCurrentData();
            if (d == null) continue;

            if (d == targetData || (targetData.buyOnce && d.name == targetData.name))
            {
                item.SetUpgradeLevel(newLevel);
                item.SetItem(item.GetItemSprite(), item.itemType, null, targetData);
            }
        }

        SaveInventoryState();
    }

    // =========================
    // ✅ REMOVE ITEM (Fix lỗi InventoryDeletePopup)
    // =========================
    public bool RemoveItem(WeaponData data)
    {
        if (data == null) return false;

        EnsureLoaded();

        bool removedUI = false;

        // 1) Xóa khỏi UI slot
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            var slot = inventorySlots[i];
            if (slot == null) continue;

            var item = slot.GetComponentInParent<InventoryItem>(true);
            if (item == null) continue;

            var d = item.GetCurrentData();
            if (d == null) continue;

            bool match = (d == data) || (data.buyOnce && d.name == data.name);
            if (!match) continue;

            slot.sprite = null;
            slot.enabled = false;

            item.SetItem(null, item.itemType, null, null);
            item.SetUpgradeLevel(1);

            removedUI = true;
            break;
        }

        // 2) Xóa khỏi GlobalInventorySave
        for (int i = GlobalInventorySave.Count - 1; i >= 0; i--)
        {
            var s = GlobalInventorySave[i];
            if (s == null || s.data == null) continue;

            bool match = (s.data == data) || (data.buyOnce && s.data.name == data.name);
            if (match) GlobalInventorySave.RemoveAt(i);
        }

        // 3) Save + refresh nhẹ
        StartCoroutine(DelayedSave());

        return removedUI;
    }

    IEnumerator DelayedSave()
    {
        yield return null;
        SaveInventoryState();
    }
}

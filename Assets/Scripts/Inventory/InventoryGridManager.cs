using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryGridManager : MonoBehaviour
{
    [Header("UI References")]
    public List<Image> inventorySlots = new List<Image>();

    [Header("Data Khởi Đầu")]
    public List<WeaponData> startItems = new List<WeaponData>();

    private bool hasInitialized = false;

    void Start()
    {
        InitData();
    }

    public void InitData()
    {
        // debug
        if (hasInitialized) return;

        RefreshInventorySlots();
        ClearInventory(); // Xóa sạch để nạp lại từ đầu 

        Debug.Log($"🚀 [InitData] Bắt đầu nạp {startItems.Count} món...");

        if (startItems != null && startItems.Count > 0)
        {
            for (int i = 0; i < startItems.Count; i++)
            {
                WeaponData itemData = startItems[i];

                
                string itemName = (itemData != null) ? itemData.name : "NULL";
                Debug.Log($"   👉 Đang xử lý Element {i}: {itemName}");

                if (itemData == null)
                {
                    Debug.LogError($"   ❌ Element {i} bị NULL (Chưa kéo file vào Inspector)!");
                    continue;
                }

                if (itemData.icon == null)
                {
                    Debug.LogError($"   ❌ Món '{itemName}' có Icon bị NULL!");
                    continue;
                }

                // LOG 2: Thử thêm vào
                bool added = AddItemBackToInventory(itemData.icon, itemData.itemType, itemData.prefab, itemData, 1);

                if (added)
                    Debug.Log($"      ✅ Đã thêm '{itemName}' thành công.");
                else
                    Debug.LogError($"      ⛔ Thêm '{itemName}' THẤT BẠI (Có thể túi đầy hoặc không tìm thấy slot trống)!");
            }
        }
        else
        {
            Debug.LogError("❌ Danh sách StartItems đang bị RỖNG!");
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
    public void RefreshInventorySlots()
    {
        inventorySlots.Clear();

        // tìm TẤT CẢ object có gắn script InventoryItem
        InventoryItem[] allItems = GetComponentsInChildren<InventoryItem>(true);

        foreach (InventoryItem itemScript in allItems)
        {
            // Với mỗi script tìm thấy, tìm cái ảnh Icon 
            Transform iconTrans = itemScript.transform.Find("Icon");

            if (iconTrans != null)
            {
                Image iconImg = iconTrans.GetComponent<Image>();
                if (iconImg != null)
                {
                    inventorySlots.Add(iconImg);
                }
            }
            else
            {
                //  Nếu không thấy con "Icon", lấy chính Image ở object đó
                Image img = itemScript.GetComponent<Image>();
                if (img != null) inventorySlots.Add(img);
            }
        }

        Debug.Log($"[Refresh] Đã tìm thấy {inventorySlots.Count} ô chứa đồ (dựa trên InventoryItem script).");
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
        if (!foundAny)
        {
            Debug.LogError($"❌ [FAILED] Không tìm thấy món nào khớp với '{targetIconName}'");
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic; 

public class InventoryGridManager : MonoBehaviour
{
    public List<Image> inventorySlots = new List<Image>();

    
    [Header("Danh sách đồ khởi đầu (Kéo các WeaponData vào đây)")]
    public List<WeaponData> startItems = new List<WeaponData>();

    void Start()
    {
        RefreshInventorySlots();

        
        if (startItems != null && startItems.Count > 0)
        {
            foreach (WeaponData itemData in startItems)
            {
                
                if (itemData != null && itemData.icon != null)
                {

                    AddItemBackToInventory(itemData.icon, itemData.itemType,itemData.prefab);
                }
            }
        }

        Debug.Log("[InventoryGridManager] Slots = " + inventorySlots.Count);
    }

    public void RefreshInventorySlots()
    {
        inventorySlots.Clear();

        foreach (Transform slotTransform in transform)
        {
            Transform itemButton = slotTransform.Find("ItemButton");
            if (itemButton == null) continue;

            Transform iconObj = itemButton.Find("Icon");
            if (iconObj == null) continue;

            Image iconImage = iconObj.GetComponent<Image>();
            if (iconImage != null) inventorySlots.Add(iconImage);
        }
    }

    public bool AddItemBackToInventory(Sprite itemSprite, InventoryItem.ItemType newType, GameObject itemPrefab)
    {
        if (itemSprite == null) return false;

        foreach (Image slotImage in inventorySlots)
        {
            if (slotImage == null) continue;

            bool empty = (slotImage.sprite == null) || (slotImage.enabled == false);
            if (!empty) continue;

            InventoryItem item = slotImage.GetComponent<InventoryItem>();
            if (item != null)
            {
                item.SetItem(itemSprite, newType, itemPrefab);
            }
            else
            {
                slotImage.sprite = itemSprite;
                slotImage.enabled = true;
            }

            return true;
        }

        Debug.LogWarning("[InventoryGridManager] Inventory FULL! (Kho đồ đã đầy)");
        return false;
    }
}
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryGridManager : MonoBehaviour
{
    public List<Image> inventorySlots = new List<Image>();

    [Header("Starter WeaponData (kéo Sword_01_Data vào đây)")]
    public WeaponData startWeapon;

    void Start()
    {
        RefreshInventorySlots();

        // ✅ nạp icon bằng chính WeaponData.icon (để không lệch sprite)
        if (startWeapon != null && startWeapon.icon != null)
        {
            AddItemBackToInventory(startWeapon.icon, InventoryItem.ItemType.Weapon);
        }

        Debug.Log("[InventoryGridManager] Slots = " + inventorySlots.Count);
    }

    public void RefreshInventorySlots()
    {
        inventorySlots.Clear();

        foreach (Transform slotTransform in transform) // ItemsParent
        {
            Transform itemButton = slotTransform.Find("ItemButton");
            if (itemButton == null) continue;

            Transform iconObj = itemButton.Find("Icon");
            if (iconObj == null) continue;

            Image iconImage = iconObj.GetComponent<Image>();
            if (iconImage != null) inventorySlots.Add(iconImage);
        }
    }

    public bool AddItemBackToInventory(Sprite itemSprite, InventoryItem.ItemType newType)
    {
        if (itemSprite == null) return false;

        foreach (Image slotImage in inventorySlots)
        {
            if (slotImage == null) continue;

            bool empty = (slotImage.sprite == null) || (slotImage.enabled == false);
            if (!empty) continue;

            InventoryItem item = slotImage.GetComponent<InventoryItem>();
            if (item != null) item.SetItem(itemSprite, newType);
            else
            {
                slotImage.sprite = itemSprite;
                slotImage.enabled = true;
            }

            return true;
        }

        Debug.LogWarning("[InventoryGridManager] Inventory FULL!");
        return false;
    }
}

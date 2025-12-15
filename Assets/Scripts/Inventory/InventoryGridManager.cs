using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryGridManager : MonoBehaviour
{
    // Chúng ta vẫn giữ list này để code hoạt động, nhưng sẽ tự động điền lại ở Start
    public List<Image> inventorySlots = new List<Image>();

    void Start()
    {
        RefreshInventorySlots();
    }

    // Hàm tự động tìm và sắp xếp lại các slot theo đúng thứ tự Hierarchy
    public void RefreshInventorySlots()
    {
        inventorySlots.Clear(); // Xóa danh sách cũ (đang bị lộn xộn)

        // Duyệt qua từng object con (InventorySlot) của ItemsParent theo thứ tự từ trên xuống dưới
        foreach (Transform slotTransform in transform)
        {
            // Trong mỗi Slot, tìm cái Icon (nơi chứa script InventoryItem hoặc tên là "Icon")
            // Cách tìm: Tìm component Image nằm trong object con
            // Lưu ý: Slot có thể có nhiều Image (khung nền, nút...), ta cần tìm cái nào là Icon chứa đồ

            // Cách an toàn nhất dựa trên cấu trúc của bạn: InventorySlot -> ItemButton -> Icon
            Transform itemButton = slotTransform.Find("ItemButton");
            if (itemButton != null)
            {
                Transform iconObj = itemButton.Find("Icon");
                if (iconObj != null)
                {
                    Image iconImage = iconObj.GetComponent<Image>();
                    inventorySlots.Add(iconImage);
                }
            }
        }

        Debug.Log("Đã sắp xếp lại " + inventorySlots.Count + " ô túi đồ theo đúng thứ tự!");
    }

    public bool AddItemBackToInventory(Sprite itemSprite, InventoryItem.ItemType newType)
    {
        if (inventorySlots == null) return false;

        foreach (Image slotImage in inventorySlots)
        {
            if (slotImage != null)
            {
                // Tìm ô trống (không có hình hoặc bị tắt)
                if (slotImage.sprite == null || slotImage.enabled == false)
                {
                    slotImage.sprite = itemSprite;
                    slotImage.enabled = true;

                    InventoryItem itemScript = slotImage.GetComponent<InventoryItem>();
                    if (itemScript != null)
                    {
                        itemScript.itemType = newType;
                    }

                    return true;
                }
            }
        }
        return false;
    }
}
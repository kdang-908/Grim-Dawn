using UnityEngine;
using UnityEngine.UI;

public class EquipmentManager : MonoBehaviour
{
    [Header("Kéo các IMAGE hiển thị đồ vào đây")]
    public Image slotHead;
    public Image slotChest;
    public Image slotLegs;
    public Image slotWeapon;

    [Header("Kéo các NÚT REMOVE (Chữ X) vào đây")] // --- MỚI THÊM ---
    public GameObject btnRemoveHead;
    public GameObject btnRemoveChest;
    public GameObject btnRemoveLegs;
    public GameObject btnRemoveWeapon;

    private InventoryGridManager gridManager;

    void Start()
    {
        gridManager = FindObjectOfType<InventoryGridManager>();
        UpdateButtons(); // Tự động ẩn nút X ngay khi vào game
    }

    // --- HÀM MỚI: Kiểm tra để Bật/Tắt nút X ---
    void UpdateButtons()
    {
        // Nguyên tắc: Nếu ô Image đang bật (có đồ) -> Hiện nút X. Ngược lại -> Ẩn.
        if (slotHead.enabled) btnRemoveHead.SetActive(true); else btnRemoveHead.SetActive(false);
        if (slotChest.enabled) btnRemoveChest.SetActive(true); else btnRemoveChest.SetActive(false);
        if (slotLegs.enabled) btnRemoveLegs.SetActive(true); else btnRemoveLegs.SetActive(false);
        if (slotWeapon.enabled) btnRemoveWeapon.SetActive(true); else btnRemoveWeapon.SetActive(false);
    }

    public Sprite EquipItem(InventoryItem.ItemType type, Sprite newItemSprite)
    {
        Image targetSlot = null;
        switch (type)
        {
            case InventoryItem.ItemType.Head: targetSlot = slotHead; break;
            case InventoryItem.ItemType.Chest: targetSlot = slotChest; break;
            case InventoryItem.ItemType.Legs: targetSlot = slotLegs; break;
            case InventoryItem.ItemType.Weapon: targetSlot = slotWeapon; break;
        }

        if (targetSlot != null)
        {
            Sprite oldItem = null;
            if (targetSlot.enabled == true && targetSlot.sprite != null)
            {
                oldItem = targetSlot.sprite;
            }

            targetSlot.sprite = newItemSprite;
            targetSlot.enabled = true;

            UpdateButtons(); // --- Cập nhật lại nút X sau khi mặc đồ ---
            return oldItem;
        }
        return null;
    }

    public void UnequipItem(InventoryItem.ItemType type)
    {
        Image targetSlot = null;
        switch (type)
        {
            case InventoryItem.ItemType.Head: targetSlot = slotHead; break;
            case InventoryItem.ItemType.Chest: targetSlot = slotChest; break;
            case InventoryItem.ItemType.Legs: targetSlot = slotLegs; break;
            case InventoryItem.ItemType.Weapon: targetSlot = slotWeapon; break;
        }

        if (targetSlot != null && targetSlot.sprite != null && targetSlot.enabled == true)
        {
            if (gridManager != null && gridManager.AddItemBackToInventory(targetSlot.sprite, type))
            {
                targetSlot.sprite = null;
                targetSlot.enabled = false;

                UpdateButtons(); // --- Cập nhật lại nút X sau khi tháo đồ ---

                Debug.Log("Đã tháo trang bị: " + type);
            }
        }
    }
}
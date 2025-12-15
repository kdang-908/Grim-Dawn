using UnityEngine;
using UnityEngine.UI;

public class UnequipButton : MonoBehaviour
{
    // Chọn loại slot mà nút X này quản lý
    public InventoryItem.ItemType slotTypeToUnequip;

    private EquipmentManager equipmentManager;
    private Button btn;

    void Start()
    {
        equipmentManager = FindObjectOfType<EquipmentManager>();
        btn = GetComponent<Button>();
        // Tự động đăng ký sự kiện click
        btn.onClick.AddListener(OnClickRemoveBtn);
    }

    void OnClickRemoveBtn()
    {
        if (equipmentManager != null)
        {
            equipmentManager.UnequipItem(slotTypeToUnequip);
        }
    }
}
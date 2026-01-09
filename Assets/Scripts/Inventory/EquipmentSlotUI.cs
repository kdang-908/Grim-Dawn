using UnityEngine;
using UnityEngine.EventSystems;

public class EquipmentSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private WeaponData currentData;
    // Thêm biến để lưu Level của món đồ đang mặc
    private int currentLevel = 1;

    // nhận thêm 'int level' từ EquipmentManager gửi sang
    public void Setup(WeaponData data, int level)
    {
        currentData = data;
        currentLevel = level; // Lưu lại level vào biến
    }

    // tháo đồ
    public void Clear()
    {
        currentData = null;
        currentLevel = 1; // Reset về 1
    }

    // Khi chuột vào
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentData != null && InventoryTooltip.Instance != null)
        {
           
            InventoryTooltip.Instance.ShowTooltip(currentData, currentLevel);
        }
    }

    // Khi chuột ra
    public void OnPointerExit(PointerEventData eventData)
    {
        if (InventoryTooltip.Instance != null)
        {
            InventoryTooltip.Instance.HideTooltip();
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryItem : MonoBehaviour, IPointerClickHandler
{
    public enum ItemType { Weapon, Head, Chest, Legs }
    public ItemType itemType;

    private Image itemImage;
    private EquipmentManager equipmentManager;

    // Biến để giữ nút thùng rác của ô này
    private GameObject removeButtonObj;

    void Start()
    {
        itemImage = GetComponent<Image>();
        equipmentManager = FindObjectOfType<EquipmentManager>();

        // --- TỰ ĐỘNG TÌM NÚT REMOVEBUTTON ---
        // Logic: Script này nằm ở Icon -> Cha là ItemButton -> Ông nội là InventorySlot
        // Nút RemoveButton là con của ông nội (InventorySlot)
        if (transform.parent != null && transform.parent.parent != null)
        {
            Transform slotTransform = transform.parent.parent;
            Transform btnTransform = slotTransform.Find("RemoveButton"); // Tìm object tên chính xác là "RemoveButton"

            if (btnTransform != null)
            {
                removeButtonObj = btnTransform.gameObject;
            }
        }
    }

    // Dùng hàm Update để liên tục kiểm tra trạng thái
    // Cách này đảm bảo nút luôn ẩn/hiện đúng lúc dù bạn thêm đồ, xóa đồ, hay tráo đồ bằng bất cứ cách nào
    void Update()
    {
        if (removeButtonObj != null)
        {
            // Nguyên tắc: Nếu hình món đồ đang hiện (enabled = true) -> Hiện nút rác
            // Ngược lại -> Ẩn nút rác
            bool shouldShow = itemImage.enabled;

            // Chỉ set lại khi trạng thái thay đổi (để tối ưu hiệu năng)
            if (removeButtonObj.activeSelf != shouldShow)
            {
                removeButtonObj.SetActive(shouldShow);
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.clickCount == 2)
        {
            if (itemImage.sprite != null && equipmentManager != null)
            {
                Sprite returnedItem = equipmentManager.EquipItem(itemType, itemImage.sprite);

                if (returnedItem != null)
                {
                    itemImage.sprite = returnedItem;
                    // Khi tráo đồ, Image vẫn bật -> Update() sẽ giữ nút rác hiện -> Đúng logic
                }
                else
                {
                    itemImage.sprite = null;
                    itemImage.enabled = false;
                    // Khi mất đồ, Image tắt -> Update() sẽ tự ẩn nút rác -> Đúng logic
                }
            }
        }
    }
}
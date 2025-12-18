using UnityEngine;
using UnityEngine.UI;

public class TrashButton : MonoBehaviour
{
    private Button btn;
    private Image iconImage;

    // Không cần biến deletePopup nữa vì ta sẽ gọi trực tiếp

    void Start()
    {
        btn = GetComponent<Button>();

        // Tìm Icon (giữ nguyên code cũ)
        if (transform.parent != null)
        {
            Transform itemBtn = transform.parent.Find("ItemButton");
            if (itemBtn != null)
            {
                Transform iconTrans = itemBtn.Find("Icon");
                if (iconTrans != null)
                {
                    iconImage = iconTrans.GetComponent<Image>();
                }
            }
        }

        btn.onClick.AddListener(OnClickDelete);
    }

    void OnClickDelete()
    {
        if (iconImage != null)
        {
            // --- SỬA: Gọi qua biến Instance ---
            // Kiểm tra xem Popup có tồn tại không
            if (InventoryDeletePopup.Instance != null)
            {
                InventoryDeletePopup.Instance.ShowConfirmation(iconImage);
            }
            else
            {
                // Nếu vẫn lỗi thì debug để biết đường sửa
                Debug.LogError("LỖI: Không tìm thấy InventoryDeletePopup. Hãy đảm bảo Script đã được gắn vào Panel!");
            }
        }
    }
}
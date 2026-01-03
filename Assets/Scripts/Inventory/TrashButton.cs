using UnityEngine;
using UnityEngine.UI;

public class TrashButton : MonoBehaviour
{
    private Button btn;
    private Image iconImage;

   

    void Start()
    {
        btn = GetComponent<Button>();

        
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
            
            if (InventoryDeletePopup.Instance != null)
            {
                InventoryDeletePopup.Instance.ShowConfirmation(iconImage, gameObject);
            }
            else
            {
               
                Debug.LogError("LỖI: Không tìm thấy InventoryDeletePopup. Hãy đảm bảo Script đã được gắn vào Panel!");
            }
        }
    }
}
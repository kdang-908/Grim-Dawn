using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class TrashButton : MonoBehaviour
{
    private Button btn;
    private InventoryItem ownerItem;

    void Awake()
    {
        btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(OnClickDelete);
    }

    InventoryItem FindOwnerItem()
    {
        var slotRoot = transform.parent; // InventorySlot
        if (slotRoot == null) return null;

        return slotRoot.GetComponentInChildren<InventoryItem>(true);
    }

    void OnClickDelete()
    {
        ownerItem = FindOwnerItem();
        if (ownerItem == null)
        {
            //Debug.LogWarning("[TrashButton] ownerItem null");
            return;
        }

        var grid = ownerItem.GetComponentInParent<InventoryGridManager>(true);
        if (grid == null)
        {
            //Debug.LogError("[TrashButton] Không tìm thấy InventoryGridManager của slot này");
            return;
        }

        var popup = InventoryDeletePopup.Instance;
        if (popup == null)
        {
            //Debug.LogError("[TrashButton] InventoryDeletePopup.Instance null");
            return;
        }

        popup.ShowConfirmation(ownerItem, grid);
    }
}

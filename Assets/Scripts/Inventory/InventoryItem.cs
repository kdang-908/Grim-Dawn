using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryItem : MonoBehaviour, IPointerClickHandler
{
    public enum ItemType { Weapon, Head, Chest, Legs }
    public ItemType itemType;

    [SerializeField] private Image itemImage;

    private EquipmentManager equipmentManager;
    private GameObject removeButtonObj;

    private void Awake()
    {
        if (itemImage == null) itemImage = GetComponent<Image>();
        if (itemImage == null) itemImage = GetComponentInChildren<Image>(true);

        equipmentManager = FindFirstObjectByType<EquipmentManager>();

        // Icon -> ItemButton -> InventorySlot -> RemoveButton
        if (transform.parent != null && transform.parent.parent != null)
        {
            Transform slotTransform = transform.parent.parent;
            Transform btnTransform = slotTransform.Find("RemoveButton");
            if (btnTransform != null) removeButtonObj = btnTransform.gameObject;
        }

        RefreshRemoveButton();
    }

    public void SetItem(Sprite sprite, ItemType type)
    {
        itemType = type;
        itemImage.sprite = sprite;
        itemImage.enabled = (sprite != null);
        RefreshRemoveButton();
    }

    public void ClearItem()
    {
        itemImage.sprite = null;
        itemImage.enabled = false;
        RefreshRemoveButton();
    }

    private void RefreshRemoveButton()
    {
        if (removeButtonObj == null || itemImage == null) return;
        bool show = itemImage.enabled && itemImage.sprite != null;
        if (removeButtonObj.activeSelf != show) removeButtonObj.SetActive(show);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.clickCount != 2) return;
        if (equipmentManager == null || itemImage == null) return;
        if (!itemImage.enabled || itemImage.sprite == null) return;

        Sprite returned = equipmentManager.EquipItem(itemType, itemImage.sprite);

        if (returned != null)
        {
            itemImage.sprite = returned;
            itemImage.enabled = true;
        }
        else
        {
            ClearItem();
        }

        RefreshRemoveButton();
    }
}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryItem : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [System.Serializable]
    public class HeadIconMap
    {
        public Sprite icon;
        public GameObject helmetPrefab;
    }
    public HeadIconMap[] headMaps;
    public enum ItemType { Weapon, Head, Chest, Legs }
    public ItemType itemType;

    private GameObject myPrefab;
    [SerializeField] private Image itemImage;
    private GameObject itemPrefab;
    private EquipmentManager equipmentManager;
    private GameObject removeButtonObj;

    // Data hiện tại của món đồ
    [Header("Debug Info")]
    [SerializeField] private WeaponData currentData; // Để Serialized để bạn soi được trên Inspector

    [Header("Upgrade")]
    public int upgradeLevel = 1;
    public const int MaxUpgradeLevel = 4;

    public WeaponData GetCurrentData()
    {
        return currentData;
    }

    public int GetUpgradeLevel()
    {
        return upgradeLevel;
    }

    public void SetUpgradeLevel(int level)
    {
        upgradeLevel = Mathf.Clamp(level, 1, MaxUpgradeLevel);
    }

    public void IncreaseUpgradeLevel()
    {
        SetUpgradeLevel(upgradeLevel + 1);
    }

    private void Awake()
    {
        // Tự động tìm Image nếu chưa gán
        if (itemImage == null) itemImage = GetComponent<Image>();
        if (itemImage == null) itemImage = GetComponentInChildren<Image>(true);

        if (itemImage != null && itemImage.gameObject != this.gameObject)
        {
            itemImage.raycastTarget = false;
        }

        equipmentManager = FindFirstObjectByType<EquipmentManager>(FindObjectsInactive.Include);

        // Tìm nút xóa (RemoveButton)
        if (transform.parent != null && transform.parent.parent != null)
        {
            Transform slotTransform = transform.parent.parent;
            Transform btnTransform = slotTransform.Find("RemoveButton");
            if (btnTransform != null) removeButtonObj = btnTransform.gameObject;
        }

        RefreshRemoveButton();
    }

    private void OnDisable()
    {
        if (InventoryTooltip.Instance != null)
        {
            InventoryTooltip.Instance.HideTooltip();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Kiểm tra xem InventoryTooltip có tồn tại không
        if (InventoryTooltip.Instance == null)
        {
            return;
        }

        // 2. Kiểm tra Data
        if (currentData == null)
        {
            return;
        }

        // 3. Kiểm tra Image
        if (itemImage == null || !itemImage.enabled)
        {
            return;
        }

        // MỌI THỨ OK -> HIỆN TOOLTIP
        InventoryTooltip.Instance.ShowTooltip(currentData, upgradeLevel);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (InventoryTooltip.Instance != null)
        {
            InventoryTooltip.Instance.HideTooltip();
        }
    }

    public void SetItem(Sprite sprite, ItemType type, GameObject prefab, WeaponData data)
    {
        itemType = type;
        myPrefab = prefab;
        currentData = data;

        if (itemImage != null)
        {
            itemImage.sprite = sprite;
            itemImage.enabled = (sprite != null);

            // BẮT BUỘC BẬT RAYCAST TARGET
            // Nếu cái này bị tắt ở Scene 2, chuột sẽ không nhận diện được
            itemImage.raycastTarget = true;
        }
        RefreshRemoveButton();
    }

    public void ClearItem()
    {
        currentData = null; // Xóa data khi clear
        if (itemImage != null)
        {
            itemImage.sprite = null;
            itemImage.enabled = false;
        }
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
        if (eventData.button != PointerEventData.InputButton.Left) return;

        if (itemImage == null || !itemImage.enabled || itemImage.sprite == null) return;

        bool enhanceMode = EnhancementPanel.Instance != null && EnhancementPanel.Instance.IsOpen();

        if (enhanceMode)
        {
            EnhancementPanel.Instance.TryInsert(this);
            return;
        }

        if (equipmentManager == null)
            equipmentManager = FindFirstObjectByType<EquipmentManager>(FindObjectsInactive.Include);

        if (equipmentManager == null) return;

        WeaponData returnedData;
        int returnedLevel;

        Sprite returnedSprite = equipmentManager.EquipItem(
            itemType,
            itemImage.sprite,
            myPrefab,
            upgradeLevel,
            out returnedData,
            out returnedLevel);

        if (returnedSprite != null)
        {
            if (returnedData != null)
            {
                this.SetItem(returnedSprite, itemType, returnedData.prefab, returnedData);
                this.SetUpgradeLevel(returnedLevel);
            }
            else
            {
                this.SetItem(returnedSprite, itemType, myPrefab, null);
                this.SetUpgradeLevel(returnedLevel);
            }
        }
        else
        {
            ClearItem();
        }

        RefreshRemoveButton();
        equipmentManager.BindPreviewNow();
    }

    public Sprite GetItemSprite()
    {
        if (itemImage == null) return null;
        return itemImage.sprite;
    }
}
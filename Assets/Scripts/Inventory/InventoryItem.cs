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
    private EquipmentManager equipmentManager;
    private GameObject removeButtonObj;

    private WeaponData currentData;

    [Header("Upgrade")]
    public int upgradeLevel = 1;
    public const int MaxUpgradeLevel = 4;

    public WeaponData GetCurrentData() => currentData;
    public int GetUpgradeLevel() => upgradeLevel;

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
        if (itemImage == null) itemImage = GetComponent<Image>();
        if (itemImage == null) itemImage = GetComponentInChildren<Image>(true);

        equipmentManager = FindFirstObjectByType<EquipmentManager>(FindObjectsInactive.Include);

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
            InventoryTooltip.Instance.HideTooltip();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (itemImage.enabled && currentData != null && InventoryTooltip.Instance != null)
            InventoryTooltip.Instance.ShowTooltip(currentData, upgradeLevel);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (InventoryTooltip.Instance != null)
            InventoryTooltip.Instance.HideTooltip();
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
        }

        RefreshRemoveButton();
    }

    public void ClearItem()
    {
        if (itemImage != null)
        {
            itemImage.sprite = null;
            itemImage.enabled = false;
        }
        currentData = null;
        myPrefab = null;
        SetUpgradeLevel(1);
        RefreshRemoveButton();
    }

    private void RefreshRemoveButton()
    {
        if (removeButtonObj == null || itemImage == null) return;
        bool show = itemImage.enabled && itemImage.sprite != null;
        if (removeButtonObj.activeSelf != show) removeButtonObj.SetActive(show);
    }

    // ✅ helper: remove item khỏi GlobalInventorySave theo data + level
    private void RemoveThisFromGlobal()
    {
        if (currentData == null) return;

        int lv = Mathf.Max(1, upgradeLevel);

        for (int i = InventoryGridManager.GlobalInventorySave.Count - 1; i >= 0; i--)
        {
            var s = InventoryGridManager.GlobalInventorySave[i];
            if (s == null || s.data == null) continue;

            bool match = (s.data == currentData && Mathf.Max(1, s.level) == lv) ||
                         (currentData.buyOnce && s.data.name == currentData.name && Mathf.Max(1, s.level) == lv);

            if (match)
            {
                InventoryGridManager.GlobalInventorySave.RemoveAt(i);
                break; // remove 1 cái thôi
            }
        }
    }

    // ✅ helper: add item về GlobalInventorySave
    private void AddToGlobal(WeaponData data, int lv)
    {
        if (data == null) return;
        InventoryGridManager.GlobalInventorySave.Add(new InventoryGridManager.SavedInvItem
        {
            data = data,
            level = Mathf.Max(1, lv)
        });
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (itemImage == null || !itemImage.enabled || itemImage.sprite == null ||
            itemImage.sprite.name == "Icon" || itemImage.sprite.name == "EmptySlot")
        {
            return;
        }

        bool enhanceMode =
            EnhancementPanel.Instance != null &&
            EnhancementPanel.Instance.IsOpen();

        if (enhanceMode)
        {
            EnhancementPanel.Instance.TryInsert(this);
            return;
        }

        if (equipmentManager == null)
            equipmentManager = FindFirstObjectByType<EquipmentManager>(FindObjectsInactive.Include);

        if (equipmentManager == null) return;

        // ✅ Lưu lại data+level của item hiện tại (item sẽ được equip)
        WeaponData thisData = currentData;
        int thisLevel = Mathf.Max(1, upgradeLevel);
        Sprite thisSprite = itemImage.sprite;

        WeaponData returnedData;
        int returnedLevel;

        Sprite returnedSprite = equipmentManager.EquipItem(
            itemType,
            thisSprite,
            myPrefab,
            thisLevel,
            out returnedData,
            out returnedLevel
        );

        // ✅ 1) Item đang click đã được equip -> PHẢI remove khỏi GLOBAL
        // (nếu không, đóng/mở túi nó sẽ hiện lại)
        RemoveThisFromGlobal();

        if (returnedSprite != null)
        {
            // có đồ cũ trả về -> set vào slot inventory hiện tại
            if (returnedData != null)
            {
                this.SetItem(returnedSprite, itemType, returnedData.prefab, returnedData);
                this.SetUpgradeLevel(returnedLevel);

                // ✅ 2) đồ cũ trả về phải add lại GLOBAL (để không mất khi reopen)
                AddToGlobal(returnedData, returnedLevel);
            }
            else
            {
                this.SetItem(returnedSprite, itemType, myPrefab, null);
                this.SetUpgradeLevel(returnedLevel);
                // returnedData null thì không add global vì không biết data
            }
        }
        else
        {
            // không có đồ cũ trả về -> ô inventory trống
            ClearItem();
        }

        // ✅ reload UI từ global để chắc chắn đóng/mở không lệch
        var gm = FindFirstObjectByType<InventoryGridManager>(FindObjectsInactive.Include);
        if (gm != null) gm.ReloadFromGlobalSave();

        RefreshRemoveButton();
        equipmentManager.BindPreviewNow();

        // ✅ refresh shop lock state ngay lập tức (khỏi cần đóng/mở shop)
        var shop = FindFirstObjectByType<ShopUIController>(FindObjectsInactive.Include);
        if (shop != null) shop.RefreshAllShopSlots();

    }
    public WeaponData GetWeaponData()
    {
        // ✅ TRẢ VỀ BIẾN WeaponData MÀ InventoryItem ĐANG GIỮ
        // Bạn đổi đúng tên biến thật của bạn ở dòng return này.
        return currentData;
    }

    public Sprite GetItemSprite()
    {
        if (itemImage == null) return null;
        return itemImage.sprite;
    }
}

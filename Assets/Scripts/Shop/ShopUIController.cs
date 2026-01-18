using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ShopUIController : MonoBehaviour
{
    [Header("AUTO - Slots Parent (kéo 1 lần)")]
    public Transform slotsRoot;

    [Header("AUTO - Item Data Folder (GIỮ NGUYÊN CỦA BẠN)")]
    public string itemDataFolder = "Assets/Scripts/Inventory/Item Data";

    [Header("Shop Items (auto scan từ folder trên)")]
    [SerializeField] private List<WeaponData> shopItems = new List<WeaponData>();

    [Header("Preview UI")]
    public Image previewIcon;
    public TMP_Text previewName;

    [Header("Stats UI")]
    public TMP_Text txtATK;
    public TMP_Text txtDEF;
    public TMP_Text txtHP;
    public TMP_Text txtEnergy;

    [Header("Price UI")]
    public TMP_Text txtPrice;
    public TMP_Text txtGold;

    [Header("Buttons")]
    public Button btnBuy;

    [Header("Player Inventory (PHẢI KÉO TAY)")]
    public InventoryGridManager inventory;

    [Header("Sync other inventories (Forge/Enhancement)")]
    public InventoryGridManager[] extraInventoriesToRefresh;

    [Header("Fallback Price (nếu WeaponData chưa có price)")]
    public int fallbackPrice = 999;

    private List<ShopSlotUI> shopSlots = new List<ShopSlotUI>();
    private WeaponData selected;

    // auto-find
    private EquipmentManager equipmentManager;

    // tránh StopAllCoroutines()
    private Coroutine refreshCo;

    void Awake()
    {
        if (slotsRoot == null)
        {
            var t = transform.Find("Inventory_Shop/ShopSlotsRoot");
            if (t == null) t = transform.Find("ShopSlotsRoot");
            if (t == null) t = transform.Find("ShopSlots");
            if (t != null) slotsRoot = t;
        }

        if (slotsRoot != null)
            shopSlots = slotsRoot.GetComponentsInChildren<ShopSlotUI>(true).ToList();
        else
            shopSlots = GetComponentsInChildren<ShopSlotUI>(true).ToList();

#if UNITY_EDITOR
        if ((shopItems == null || shopItems.Count == 0) && !string.IsNullOrEmpty(itemDataFolder))
            AutoScanItems_EditorOnly();
#endif
    }

    void Start()
    {
        equipmentManager = FindFirstObjectByType<EquipmentManager>(FindObjectsInactive.Include);

        BuildShop();

        if (btnBuy != null)
        {
            btnBuy.onClick.RemoveAllListeners();
            btnBuy.onClick.AddListener(BuySelected);
        }

        UpdatePreview(null);
        RefreshGoldUI();
    }

    void OnEnable()
    {
        if (equipmentManager == null)
            equipmentManager = FindFirstObjectByType<EquipmentManager>(FindObjectsInactive.Include);

        if (refreshCo != null) StopCoroutine(refreshCo);
        refreshCo = StartCoroutine(RefreshShopWhenEquippedReady());
    }

    // ====== ID + PRICE ======
    string GetItemId(WeaponData data) => data != null ? data.name : "";

    int GetPrice(WeaponData data)
    {
        if (data == null) return 0;
        return (data.price > 0) ? data.price : fallbackPrice;
    }

    // ✅ check đã sở hữu trong save chung (túi)
    bool IsOwnedInGlobal(WeaponData data)
    {
        if (data == null) return false;

        var list = InventoryGridManager.GlobalInventorySave;
        if (list == null || list.Count == 0) return false;

        string id = GetItemId(data);

        for (int i = 0; i < list.Count; i++)
        {
            var s = list[i];
            if (s == null || s.data == null) continue;
            if (GetItemId(s.data) == id) return true;
        }
        return false;
    }

    // ✅ check đang trang bị
    bool IsEquipped(WeaponData data)
    {
        if (data == null) return false;

        if (equipmentManager == null)
            equipmentManager = FindFirstObjectByType<EquipmentManager>(FindObjectsInactive.Include);

        if (equipmentManager == null) return false;

        return equipmentManager.IsEquipped(data);
    }

    // ✅ khóa mua theo yêu cầu
    public bool IsSold(WeaponData data)
    {
        if (data == null) return false;
        if (!data.buyOnce) return false;

        return IsOwnedInGlobal(data) || IsEquipped(data);
    }

    // ====== BUILD SHOP ======
    public void BuildShop()
    {
        if (shopSlots == null || shopSlots.Count == 0) return;

        for (int i = 0; i < shopSlots.Count; i++)
        {
            WeaponData d = (shopItems != null && i < shopItems.Count) ? shopItems[i] : null;
            shopSlots[i].Setup(d, this);
        }

        foreach (var s in shopSlots)
            if (s != null) s.RefreshSoldState();
    }

    // ====== SELECT ======
    public void Select(WeaponData data)
    {
        selected = data;
        UpdatePreview(selected);
    }

    void UpdatePreview(WeaponData data)
    {
        bool sold = (data != null && IsSold(data));

        if (previewIcon != null)
        {
            previewIcon.sprite = (data != null) ? data.icon : null;
            previewIcon.enabled = (data != null && data.icon != null);
        }

        if (previewName != null)
            previewName.text = (data != null) ? data.displayName : "";

        if (txtATK != null) txtATK.text = (data != null) ? data.bonusATK.ToString() : "0";
        if (txtDEF != null) txtDEF.text = (data != null) ? data.bonusDEF.ToString() : "0";
        if (txtHP != null) txtHP.text = (data != null) ? data.bonusMaxHP.ToString() : "0";
        if (txtEnergy != null) txtEnergy.text = (data != null) ? data.bonusEnergy.ToString() : "0";

        if (txtPrice != null)
            txtPrice.text = (data != null) ? GetPrice(data).ToString() : "0";

        RefreshGoldUI();

        if (btnBuy != null)
            btnBuy.interactable = (data != null && !sold);
    }

    // ====== BUY ======
    void BuySelected()
    {
        if (selected == null) return;

        if (IsSold(selected))
        {
            UpdatePreview(selected);
            return;
        }

        if (inventory == null) return;

        var gm = GameManager.Instance;
        if (gm == null) return;

        int price = GetPrice(selected);

        if (!gm.SpendGold(price))
        {
            RefreshGoldUI();
            return;
        }

        bool ok = inventory.AddItemFromShop(selected, 1);
        if (!ok)
        {
            gm.AddGold(price);
            RefreshGoldUI();
            return;
        }

        if (extraInventoriesToRefresh != null && extraInventoriesToRefresh.Length > 0)
        {
            foreach (var inv in extraInventoriesToRefresh)
                if (inv != null) inv.ReloadFromGlobalSave();
        }

        RefreshAllShopSlots();
        UpdatePreview(selected);
        RefreshGoldUI();
    }

    void RefreshGoldUI()
    {
        var gm = GameManager.Instance;
        int currentGold = (gm != null) ? gm.gold : 0;
        if (txtGold != null) txtGold.text = currentGold.ToString();
    }

    public void RefreshAllShopSlots()
    {
        var slots = GetComponentsInChildren<ShopSlotUI>(true);
        foreach (var s in slots)
        {
            if (s == null) continue;
            s.RefreshSoldState();
            s.RefreshLockState();
        }
    }

    // =========================================================
    // ✅ FIX CHÍNH: ĐỢI EquipmentManager restore xong rồi mới refresh
    // - chờ tối đa 60 frame (≈ 1 giây)
    // - nếu HasEquippedSave=false thì vẫn refresh bình thường
    // =========================================================
    IEnumerator RefreshShopWhenEquippedReady()
    {
        // đợi 1 frame để scene settle
        yield return null;

        // tìm lại equipmentManager (phòng trường hợp object spawn trễ)
        if (equipmentManager == null)
            equipmentManager = FindFirstObjectByType<EquipmentManager>(FindObjectsInactive.Include);

        // Nếu game có dữ liệu equip (HasEquippedSave=true) thì đợi tới khi
        // currentWeapon/currentHelmet/currentChest đã được restore
        if (EquipmentManager.HasEquippedSave)
        {
            int maxFrames = 60; // tăng nếu máy yếu
            while (maxFrames-- > 0)
            {
                if (equipmentManager == null)
                    equipmentManager = FindFirstObjectByType<EquipmentManager>(FindObjectsInactive.Include);

                // restore xong khi equipmentManager tồn tại và đã set xong data
                if (equipmentManager != null &&
                    (equipmentManager.currentWeapon != null ||
                     equipmentManager.currentHelmet != null ||
                     equipmentManager.currentChest != null))
                {
                    break;
                }

                yield return null;
            }
        }
        else
        {
            // không có đồ mặc save -> khỏi đợi lâu
            yield return null;
        }

        RefreshAllShopSlots();
        if (selected != null) UpdatePreview(selected);
    }

#if UNITY_EDITOR
    [ContextMenu("SHOP/Auto Scan Items (Editor Only)")]
    public void AutoScanItems_EditorOnly()
    {
        if (string.IsNullOrEmpty(itemDataFolder)) return;

        string folder = itemDataFolder.Replace("\\", "/").Trim();
        if (!folder.StartsWith("Assets/"))
        {
            Debug.LogWarning($"[Shop] itemDataFolder phải bắt đầu bằng 'Assets/'. Hiện tại: {folder}");
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:WeaponData", new[] { folder });

        var list = new List<WeaponData>();
        foreach (var g in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(g);
            var asset = AssetDatabase.LoadAssetAtPath<WeaponData>(path);
            if (asset != null) list.Add(asset);
        }

        shopItems = list.OrderBy(x => x != null ? x.name : "").ToList();
        EditorUtility.SetDirty(this);

        Debug.Log($"[Shop] AutoScan OK: found {shopItems.Count} WeaponData in {folder}.");
    }
#endif
}

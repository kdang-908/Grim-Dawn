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
    public InventoryGridManager inventory; // ✅ KÉO ItemsParent (Inventory thật) vào đây

    [Header("Sync other inventories (Forge/Enhancement)")]
    public InventoryGridManager[] extraInventoriesToRefresh; // ✅ KÉO Inventory_Forge vào đây

    [Header("Fallback Price (nếu WeaponData chưa có price)")]
    public int fallbackPrice = 999;

    // ====== BOUGHT STATE (mua 1 lần trong lúc play) ======
    // soldIds chỉ giúp khóa NGAY trong scene hiện tại.
    // Qua scene khác sẽ reset -> nên ta sẽ check thêm GlobalInventorySave (đã sở hữu) để khóa xuyên scene.
    private HashSet<string> soldIds = new HashSet<string>();

    private List<ShopSlotUI> shopSlots = new List<ShopSlotUI>();
    private WeaponData selected;

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
        BuildShop();

        if (btnBuy != null)
            btnBuy.onClick.AddListener(BuySelected);

        UpdatePreview(null);
        RefreshGoldUI();
    }

    // ====== ID + PRICE ======
    string GetItemId(WeaponData data)
    {
        return data != null ? data.name : "";
    }

    int GetPrice(WeaponData data)
    {
        if (data == null) return 0;
        return (data.price > 0) ? data.price : fallbackPrice;
    }

    // ✅ NEW: check đã sở hữu trong save chung (xuyên scene)
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

    // ✅ SOLD = (đã mua trong scene này) OR (đã sở hữu trong GlobalInventorySave)
    public bool IsSold(WeaponData data)
    {
        if (data == null) return false;

        // 1) khóa ngay trong scene hiện tại
        if (soldIds.Contains(GetItemId(data)))
            return true;

        // 2) khóa xuyên scene (đã mua trước đó -> inventory save chung có)
        // chỉ khóa với item buyOnce
        if (data.buyOnce && IsOwnedInGlobal(data))
            return true;

        return false;
    }

    void MarkSold(WeaponData data)
    {
        if (data == null) return;
        soldIds.Add(GetItemId(data));
    }

    // ====== BUILD SHOP ======
    public void BuildShop()
    {
        if (shopSlots == null || shopSlots.Count == 0)
        {
            Debug.LogWarning("[Shop] Không tìm thấy ShopSlotUI trong slotsRoot.");
            return;
        }

        for (int i = 0; i < shopSlots.Count; i++)
        {
            WeaponData d = (shopItems != null && i < shopItems.Count) ? shopItems[i] : null;
            shopSlots[i].Setup(d, this);
        }

        // ✅ refresh trạng thái sold cho slot ngay khi build
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

        // ✅ nếu đã mua / đã sở hữu -> khóa nút mua
        if (btnBuy != null)
            btnBuy.interactable = (data != null && !sold);
    }

    // ====== BUY ======
    void BuySelected()
    {
        if (selected == null) return;

        // ✅ đã mua rồi (kể cả qua scene) thì khỏi mua
        if (IsSold(selected))
        {
            UpdatePreview(selected);
            return;
        }

        if (inventory == null)
        {
            Debug.LogError("[Shop] inventory NULL. Kéo ItemsParent (Inventory thật) vào ShopUIController.Inventory!");
            return;
        }

        var gm = GameManager.Instance;
        if (gm == null)
        {
            Debug.LogError("[Shop] GameManager.Instance NULL.");
            return;
        }

        int price = GetPrice(selected);

        if (!gm.SpendGold(price))
        {
            Debug.LogWarning($"[Shop] Không đủ vàng. Price={price}, Gold={gm.gold}");
            RefreshGoldUI();
            return;
        }

        // 1) add item vào inventory thật
        bool ok = inventory.AddItemFromShop(selected, 1);
        if (!ok)
        {
            gm.AddGold(price);
            Debug.LogWarning("[Shop] Túi đầy -> hoàn tiền.");
            RefreshGoldUI();
            return;
        }

        // 2) sync UI khác (Forge/Enhance) để cũng thấy item
        if (extraInventoriesToRefresh != null && extraInventoriesToRefresh.Length > 0)
        {
            foreach (var inv in extraInventoriesToRefresh)
                if (inv != null) inv.ReloadFromGlobalSave();
        }

        // 3) đánh dấu sold
        if (selected.buyOnce)
            MarkSold(selected);

        foreach (var s in shopSlots)
            if (s != null) s.RefreshSoldState();

        Debug.Log($"[Shop] Mua thành công: {selected.displayName} (-{price} gold)");
        UpdatePreview(selected);
        RefreshGoldUI();
    }

    void RefreshGoldUI()
    {
        var gm = GameManager.Instance;
        int currentGold = (gm != null) ? gm.gold : 0;
        if (txtGold != null) txtGold.text = currentGold.ToString();
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

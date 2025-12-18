using UnityEngine;
using UnityEngine.UI;

public class EquipmentManager : MonoBehaviour
{
    [Header("Kéo các IMAGE hiển thị đồ vào đây")]
    public Image slotHead;
    public Image slotChest;
    public Image slotLegs;
    public Image slotWeapon;

    [Header("Kéo các NÚT REMOVE (Chữ X) vào đây")]
    public GameObject btnRemoveHead;
    public GameObject btnRemoveChest;
    public GameObject btnRemoveLegs;
    public GameObject btnRemoveWeapon;

    private InventoryGridManager gridManager;

    [Header("3D Weapon Equip (ngoài map + preview)")]
    public WeaponEquipper playerWeaponEquipper;   // Remy ngoài map
    public WeaponEquipper previewWeaponEquipper;  // Remy preview (UI)

    [Header("Preview")]
    public Transform previewRoot;
    public string previewLayerName = "UIPreview";

    [System.Serializable]
    public class WeaponIconMap
    {
        public Sprite icon;
        public WeaponData data;
    }

    [Header("Map icon -> WeaponData")]
    public WeaponIconMap[] weaponMaps;

    // nhớ vũ khí đang equip để khi mở inventory sẽ auto hiện preview
    private WeaponData currentWeaponData;

    void Start()
    {
        gridManager = FindFirstObjectByType<InventoryGridManager>();

        // ✅ CHỈ TÌM ROOT, KHÔNG BIND SOCKET Ở ĐÂY
        if (previewRoot == null)
        {
            var go = GameObject.Find("UI_PreviewRoot");
            if (go != null) previewRoot = go.transform;
        }
        AutoBindRemoveButtons();
        UpdateButtons();
    }

    void UpdateButtons()
    {
        if (btnRemoveHead != null) btnRemoveHead.SetActive(slotHead && slotHead.enabled && slotHead.sprite);
        if (btnRemoveChest != null) btnRemoveChest.SetActive(slotChest && slotChest.enabled && slotChest.sprite);
        if (btnRemoveLegs != null) btnRemoveLegs.SetActive(slotLegs && slotLegs.enabled && slotLegs.sprite);
        if (btnRemoveWeapon != null) btnRemoveWeapon.SetActive(slotWeapon && slotWeapon.enabled && slotWeapon.sprite);
    }

    // ===================== EQUIP UI =====================
    public Sprite EquipItem(InventoryItem.ItemType type, Sprite newItemSprite)
    {
        Image targetSlot = GetTargetSlot(type);
        if (targetSlot == null || newItemSprite == null) return null;

        Sprite old = (targetSlot.enabled && targetSlot.sprite != null) ? targetSlot.sprite : null;

        targetSlot.sprite = newItemSprite;
        targetSlot.enabled = true;

        Debug.Log($"[EquipItem] type={type} icon={newItemSprite.name}");

        if (type == InventoryItem.ItemType.Weapon)
        {
            WeaponData wd = FindWeaponDataByIcon(newItemSprite);
            Debug.Log("[EquipItem] FindWeaponDataByIcon = " + (wd ? wd.name : "NULL"));

            if (wd != null)
            {
                currentWeaponData = wd;
                EquipWeapon3D(wd);
            }
            else
            {
                Debug.LogError($"[EquipItem] Không map được icon '{newItemSprite.name}' -> WeaponData. Check weaponMaps!");
            }
        }

        UpdateButtons();
        return old;
    }

    public void UnequipItem(InventoryItem.ItemType type)
    {
        Debug.Log($"[UnequipItem] called type={type}");
        Image targetSlot = GetTargetSlot(type);
        if (targetSlot == null) return;
        if (!targetSlot.enabled || targetSlot.sprite == null) return;

        if (gridManager != null && gridManager.AddItemBackToInventory(targetSlot.sprite, type))
        {
            targetSlot.sprite = null;
            targetSlot.enabled = false;

            if (type == InventoryItem.ItemType.Weapon)
                UnequipWeapon3D();

            UpdateButtons();
        }
    }

    Image GetTargetSlot(InventoryItem.ItemType type)
    {
        switch (type)
        {
            case InventoryItem.ItemType.Head: return slotHead;
            case InventoryItem.ItemType.Chest: return slotChest;
            case InventoryItem.ItemType.Legs: return slotLegs;
            case InventoryItem.ItemType.Weapon: return slotWeapon;
        }
        return null;
    }

    WeaponData FindWeaponDataByIcon(Sprite icon)
    {
        if (icon == null || weaponMaps == null) return null;

        foreach (var m in weaponMaps)
        {
            if (m == null || m.data == null || m.icon == null) continue;
            if (m.icon == icon) return m.data;
            if (m.icon.name == icon.name) return m.data;
        }
        return null;
    }

    // ===================== PREVIEW BIND (CHỈ GỌI KHI UI OPEN) =====================
    public void BindPreviewNow()
    {
        // ✅ tìm đúng UI_PreviewRoot nào thực sự chứa WeaponSocket_R
        if (previewRoot == null || !previewRoot.gameObject.scene.IsValid())
        {
            previewRoot = FindPreviewRootContainsSocket();
        }

        if (previewRoot == null)
        {
            Debug.LogError("[BindPreviewNow] Không tìm thấy UI_PreviewRoot chứa WeaponSocket_R");
            return;
        }

        LateBindPreviewEquipperIfNeeded();

        // ✅ nếu đang có weapon thì auto hiện preview khi mở UI
        if (currentWeaponData != null && previewWeaponEquipper != null)
        {
            int layer = LayerMask.NameToLayer(previewLayerName);
            previewWeaponEquipper.Equip(currentWeaponData, layer);
            Debug.Log("[BindPreviewNow] Auto preview equipped: " + currentWeaponData.name);
        }
    }

    Transform FindPreviewRootContainsSocket()
    {
        var all = Object.FindObjectsByType<Transform>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        foreach (var t in all)
        {
            if (t.name != "UI_PreviewRoot") continue;

            foreach (var c in t.GetComponentsInChildren<Transform>(true))
            {
                if (c.name == "WeaponSocket_R")
                {
                    Debug.Log("[FindPreviewRoot] Picked: " + GetPath(t));
                    return t;
                }
            }
        }

        return null;
    }

    void LateBindPreviewEquipperIfNeeded()
    {
        if (previewWeaponEquipper != null && previewWeaponEquipper.socketR != null)
            return;

        if (previewRoot == null)
        {
            Debug.LogError("[Preview] previewRoot NULL");
            return;
        }

        // ✅ tìm socket trong previewRoot (kể cả inactive)
        Transform socket = null;
        foreach (var t in previewRoot.GetComponentsInChildren<Transform>(true))
        {
            if (t.name == "WeaponSocket_R")
            {
                socket = t;
                break;
            }
        }

        if (socket == null)
        {
            Debug.LogError("[Preview] Không tìm thấy WeaponSocket_R trong " + GetPath(previewRoot));
            return;
        }

        var anim = socket.GetComponentInParent<Animator>(true);
        if (anim == null)
        {
            Debug.LogError("[Preview] Không tìm thấy Animator cho preview model (parent của socket)");
            return;
        }

        previewWeaponEquipper = anim.GetComponent<WeaponEquipper>();
        if (previewWeaponEquipper == null)
            previewWeaponEquipper = anim.gameObject.AddComponent<WeaponEquipper>();

        previewWeaponEquipper.socketR = socket;

        Debug.Log("[Preview] Bind OK socket path=" + GetPath(socket));
    }

    // ===================== EQUIP 3D =====================
    public void EquipWeapon3D(WeaponData weaponData)
    {
        if (weaponData == null) return;
        currentWeaponData = weaponData;

        // 1) ngoài map luôn equip được
        if (playerWeaponEquipper == null)
            Debug.LogError("[EquipWeapon3D] playerWeaponEquipper NULL -> kéo WeaponEquipper của Remy ngoài map vào EquipmentManager!");
        else
            playerWeaponEquipper.Equip(weaponData);

        // 2) preview chỉ equip nếu đã bind rồi
        if (previewWeaponEquipper != null)
        {
            int layer = LayerMask.NameToLayer(previewLayerName);
            previewWeaponEquipper.Equip(weaponData, layer);
        }
    }

    public void UnequipWeapon3D()
    {
        currentWeaponData = null;

        if (playerWeaponEquipper != null) playerWeaponEquipper.Unequip();
        if (previewWeaponEquipper != null) previewWeaponEquipper.Unequip();
    }

    static string GetPath(Transform t)
    {
        string p = t.name;
        while (t.parent != null) { t = t.parent; p = t.name + "/" + p; }
        return p;
    }
    void AutoBindRemoveButtons()
    {
        Bind(btnRemoveHead, InventoryItem.ItemType.Head);
        Bind(btnRemoveChest, InventoryItem.ItemType.Chest);
        Bind(btnRemoveLegs, InventoryItem.ItemType.Legs);
        Bind(btnRemoveWeapon, InventoryItem.ItemType.Weapon);
    }

    void Bind(GameObject btn, InventoryItem.ItemType type)
    {
        if (btn == null) { Debug.LogWarning($"[Bind] btn null: {type}"); return; }

        var b = btn.GetComponent<Button>();
        if (b == null) { Debug.LogWarning($"[Bind] no Button on {btn.name}"); return; }

        b.onClick.RemoveAllListeners();
        b.onClick.AddListener(() =>
        {
            Debug.Log($"[CLICK X] {type}  btn={btn.name}");
            UnequipItem(type);
        });

        Debug.Log($"[Bind OK] {type} -> {btn.name}");
    }
}

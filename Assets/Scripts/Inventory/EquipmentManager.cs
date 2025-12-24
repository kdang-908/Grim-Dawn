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

    [Header("3D Weapon Equip (runtime player + preview)")]
    public WeaponEquipper playerWeaponEquipper;   // runtime PlayerRuntime
    public WeaponEquipper previewWeaponEquipper;  // Preview_Player (UI)

    [Header("Preview")]
    public Transform previewRoot;                 // UI_PreviewRoot
    public string previewLayerName = "UIPreview";

    [System.Serializable]
    public class WeaponIconMap
    {
        public Sprite icon;
        public WeaponData data;
    }

    [Header("Map icon -> WeaponData")]
    public WeaponIconMap[] weaponMaps;

    private WeaponData currentWeaponData;

    void Start()
    {
        gridManager = FindFirstObjectByType<InventoryGridManager>();

        // Find preview root in loaded scenes
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

    // ===================== PREVIEW BIND =====================
    public void BindPreviewNow()
    {
        // Always refind preview root (additive scene / unload selection scene)
        var go = GameObject.Find("UI_PreviewRoot");
        if (go != null) previewRoot = go.transform;

        if (previewRoot == null)
        {
            Debug.LogError("[BindPreviewNow] UI_PreviewRoot NULL (không có trong Map?)");
            return;
        }

        LateBindPreviewEquipperIfNeeded();

        // If currently has weapon => show on preview immediately
        if (currentWeaponData != null && previewWeaponEquipper != null)
        {
            int layer = LayerMask.NameToLayer(previewLayerName);
            previewWeaponEquipper.Equip(currentWeaponData, layer);
            Debug.Log("[BindPreviewNow] Preview equipped: " + currentWeaponData.name);
        }
    }

    void LateBindPreviewEquipperIfNeeded()
    {
        if (previewWeaponEquipper != null && previewWeaponEquipper.socketR != null)
            return;

        // Find WeaponSocket_R inside previewRoot (robust trim)
        Transform socket = null;
        foreach (var t in previewRoot.GetComponentsInChildren<Transform>(true))
        {
            if (t.name.Trim() == "WeaponSocket_R")
            {
                socket = t;
                break;
            }
        }

        if (socket == null)
        {
            Debug.LogError("[BindPreviewNow] Không tìm thấy WeaponSocket_R trong UI_PreviewRoot. (check tên socket có đúng/không có dấu cách)");
            return;
        }

        var anim = socket.GetComponentInParent<Animator>(true);
        if (anim == null)
        {
            Debug.LogError("[BindPreviewNow] Preview model không có Animator (parent của socket)");
            return;
        }

        previewWeaponEquipper = anim.GetComponent<WeaponEquipper>();
        if (previewWeaponEquipper == null)
            previewWeaponEquipper = anim.gameObject.AddComponent<WeaponEquipper>();

        previewWeaponEquipper.socketR = socket;

        // optional: auto find WeaponGrip in preview too
        // (WeaponEquipper will auto find, but ok to keep)
        Debug.Log("[BindPreviewNow] Preview bind OK socket=" + GetPath(socket));
    }

    // ===================== EQUIP 3D =====================
    public void EquipWeapon3D(WeaponData weaponData)
    {
        if (weaponData == null) return;

        // Find runtime player equipper if missing
        if (playerWeaponEquipper == null)
        {
            var player = GameObject.FindWithTag("Player");
            if (player != null)
                playerWeaponEquipper = player.GetComponentInChildren<WeaponEquipper>(true);
        }

        if (playerWeaponEquipper == null)
        {
            Debug.LogError("[EquipWeapon3D] playerWeaponEquipper NULL (runtime player chưa có WeaponEquipper?)");
            return;
        }

        // set anim param
        var anim = playerWeaponEquipper.GetComponentInChildren<Animator>(true);
        if (anim != null) anim.SetInteger("WeaponType", weaponData.animationID);

        currentWeaponData = weaponData;

        // equip on runtime player
        playerWeaponEquipper.Equip(weaponData);

        // equip on preview if already bound
        if (previewWeaponEquipper != null)
        {
            int layer = LayerMask.NameToLayer(previewLayerName);
            previewWeaponEquipper.Equip(weaponData, layer);
        }
    }

    public void UnequipWeapon3D()
    {
        if (playerWeaponEquipper != null)
        {
            var anim = playerWeaponEquipper.GetComponentInChildren<Animator>(true);
            if (anim != null) anim.SetInteger("WeaponType", 0);

            playerWeaponEquipper.Unequip();
        }

        if (previewWeaponEquipper != null)
            previewWeaponEquipper.Unequip();

        currentWeaponData = null;
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
        if (btn == null) return;

        var b = btn.GetComponent<Button>();
        if (b == null) return;

        b.onClick.RemoveAllListeners();
        b.onClick.AddListener(() => UnequipItem(type));
    }
}

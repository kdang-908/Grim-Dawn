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

    [Header("KẾT NỐI STATS (MỚI)")]
    public CharacterStats playerStats; // ⚠️ QUAN TRỌNG: Kéo nhân vật Remy vào đây

    [Header("3D Weapon Equip (ngoài map + preview)")]
    public WeaponEquipper playerWeaponEquipper;   // Remy ngoài map
    public WeaponEquipper previewWeaponEquipper;  // Remy preview (UI)

    [Header("Preview")]
    public Transform previewRoot;
    public string previewLayerName = "UIPreview";

    // --- CẤU HÌNH VŨ KHÍ ---
    [System.Serializable]
    public class WeaponIconMap
    {
        public Sprite icon;
        public WeaponData data;
    }
    [Header("Map icon -> WeaponData")]
    public WeaponIconMap[] weaponMaps;

    // --- CẤU HÌNH NÓN (HELMET) ---
    [System.Serializable]
    public class HelmetIconMap
    {
        public Sprite icon;
        public ArmorData data;
    }
    [Header("Map icon -> HelmetData")]
    public HelmetIconMap[] helmetMaps;

    // --- CẤU HÌNH ÁO (CHEST) ---
    [System.Serializable]
    public class ChestIconMap
    {
        public Sprite icon;
        public ArmorData data;
    }
    [Header("Map icon -> ChestData")]
    public ChestIconMap[] chestMaps;

    // --- CÁC BIẾN QUẢN LÝ VỊ TRÍ ---
    [Header("Vị trí trang bị (Player)")]
    public Transform playerHeadBone;  // Xương Head
    public Transform playerChestBone; // Xương Spine2

    [Header("Vị trí trang bị (Preview UI)")]
    public Transform previewHeadBone;
    public Transform previewChestBone;

    // --- CÁC BIẾN LƯU OBJECT ĐANG MẶC ---
    private GameObject currentHelmetObj;
    private GameObject currentPreviewHelmetObj;

    private GameObject currentChestObj;
    private GameObject currentPreviewChestObj;

    // --- CÁC BIẾN LƯU DATA ĐỂ RELOAD KHI MỞ UI ---
    private WeaponData currentWeaponData;
    private ArmorData currentHelmetData;
    private ArmorData currentChestData;

    void Start()
    {
        gridManager = FindFirstObjectByType<InventoryGridManager>();

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

        if (type == InventoryItem.ItemType.Weapon)
        {
            WeaponData wd = FindWeaponDataByIcon(newItemSprite);
            if (wd != null)
            {
               
                EquipWeapon3D(wd);
            }
        }
        else if (type == InventoryItem.ItemType.Head)
        {
            ArmorData hd = FindHelmetDataByIcon(newItemSprite);
            if (hd != null) EquipHelmet3D(hd);
        }
        else if (type == InventoryItem.ItemType.Chest)
        {
            ArmorData cd = FindChestDataByIcon(newItemSprite);
            if (cd != null) EquipChest3D(cd);
        }

        UpdateButtons();
        return old;
    }

    public void UnequipItem(InventoryItem.ItemType type)
    {
        Image targetSlot = GetTargetSlot(type);
        if (targetSlot == null || !targetSlot.enabled || targetSlot.sprite == null) return;

        if (gridManager != null && gridManager.AddItemBackToInventory(targetSlot.sprite, type))
        {
            targetSlot.sprite = null;
            targetSlot.enabled = false;

            if (type == InventoryItem.ItemType.Weapon) UnequipWeapon3D();
            else if (type == InventoryItem.ItemType.Head) UnequipHelmet3D();
            else if (type == InventoryItem.ItemType.Chest) UnequipChest3D();

            UpdateButtons();
        }
    }

    // ... Helper functions ...
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
            if (m.icon == icon || (m.icon != null && m.icon.name == icon.name)) return m.data;
        }
        return null;
    }
    ArmorData FindHelmetDataByIcon(Sprite icon)
    {
        if (icon == null || helmetMaps == null) return null;
        foreach (var m in helmetMaps)
        {
            if (m.icon == icon || (m.icon != null && m.icon.name == icon.name)) return m.data;
        }
        return null;
    }
    ArmorData FindChestDataByIcon(Sprite icon)
    {
        if (icon == null || chestMaps == null) return null;
        foreach (var m in chestMaps)
        {
            if (m.icon == icon) return m.data;
            if (m.icon != null && m.icon.name == icon.name) return m.data;
        }
        return null;
    }

    // ===================== PREVIEW BIND =====================
    public void BindPreviewNow()
    {
        if (previewRoot == null || !previewRoot.gameObject.scene.IsValid())
            previewRoot = FindPreviewRootContainsSocket();

        if (previewRoot == null) return;

        LateBindPreviewEquipperIfNeeded();

        if (previewHeadBone == null)
        {
            foreach (var t in previewRoot.GetComponentsInChildren<Transform>(true))
            {
                if (t.name.Contains("Head") && !t.name.Contains("Header"))
                {
                    previewHeadBone = t; break;
                }
            }
        }

        if (previewChestBone == null)
        {
            foreach (var t in previewRoot.GetComponentsInChildren<Transform>(true))
            {
                if (t.name.Contains("Spine2"))
                {
                    previewChestBone = t; break;
                }
            }
        }

        int layer = LayerMask.NameToLayer(previewLayerName);
        if (currentWeaponData != null && previewWeaponEquipper != null)
            previewWeaponEquipper.Equip(currentWeaponData, layer);

        if (currentHelmetData != null) EquipArmorOnPreview(currentHelmetData, previewHeadBone, ref currentPreviewHelmetObj);
        if (currentChestData != null) EquipArmorOnPreview(currentChestData, previewChestBone, ref currentPreviewChestObj);
    }

    Transform FindPreviewRootContainsSocket()
    {
        var all = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var t in all)
        {
            if (t.name != "UI_PreviewRoot") continue;
            foreach (var c in t.GetComponentsInChildren<Transform>(true))
                if (c.name == "WeaponSocket_R") return t;
        }
        return null;
    }
    void LateBindPreviewEquipperIfNeeded()
    {
        if (previewWeaponEquipper != null && previewWeaponEquipper.socketR != null) return;
        if (previewRoot == null) return;
        Transform socket = null;
        foreach (var t in previewRoot.GetComponentsInChildren<Transform>(true))
            if (t.name == "WeaponSocket_R") { socket = t; break; }
        if (socket == null) return;
        var anim = socket.GetComponentInParent<Animator>(true);
        if (anim == null) return;
        previewWeaponEquipper = anim.GetComponent<WeaponEquipper>();
        if (previewWeaponEquipper == null) previewWeaponEquipper = anim.gameObject.AddComponent<WeaponEquipper>();
        previewWeaponEquipper.socketR = socket;
    }

    // ===================== EQUIP LOGIC (ĐÃ CÓ STATS) =====================

    // --- 1. VŨ KHÍ ---
    public void EquipWeapon3D(WeaponData weaponData)
    {
        if (weaponData == null) return;

        
        if (currentWeaponData != null && playerStats != null)
        {
            
            playerStats.RemoveBonus(currentWeaponData.atkBonus, currentWeaponData.defBonus, currentWeaponData.hpBonus);
        }
        
        currentWeaponData = weaponData; 

        playerWeaponEquipper.GetComponent<Animator>().SetInteger("WeaponType", weaponData.animationID);

        
        if (playerStats != null)
        {
            playerStats.AddBonus(weaponData.atkBonus, weaponData.defBonus, weaponData.hpBonus);
        }

        // Phần hiển thị 3D (giữ nguyên)
        if (playerWeaponEquipper != null) playerWeaponEquipper.Equip(weaponData);
        if (previewWeaponEquipper != null)
        {
            int layer = LayerMask.NameToLayer(previewLayerName);
            previewWeaponEquipper.Equip(weaponData, layer);
        }
    }
    public void UnequipWeapon3D()
    {
        // TRỪ CHỈ SỐ CŨ
        if (playerStats != null && currentWeaponData != null)
            playerStats.RemoveBonus(currentWeaponData.atkBonus, currentWeaponData.defBonus, currentWeaponData.hpBonus);

        playerWeaponEquipper.GetComponent<Animator>().SetInteger("WeaponType", 0);
        currentWeaponData = null;
        if (playerWeaponEquipper != null) playerWeaponEquipper.Unequip();
        if (previewWeaponEquipper != null) previewWeaponEquipper.Unequip();
    }

    // --- 2. HELMET (NÓN) ---
    public void EquipHelmet3D(ArmorData data)
    {
        //  TRỪ CHỈ SỐ CŨ
        if (currentHelmetData != null && playerStats != null)
        {
            playerStats.RemoveBonus(currentHelmetData.atkBonus, currentHelmetData.defBonus, currentHelmetData.hpBonus);
        }

        UnequipHelmet3D(); // Xóa hình ảnh nón cũ
        if (data == null || data.prefab == null) return;

        //  CẬP NHẬT & CỘNG MỚI
        currentHelmetData = data;
        if (playerStats != null)
        {
            playerStats.AddBonus(data.atkBonus, data.defBonus, data.hpBonus);
        }

        // (Phần hiển thị 3D giữ nguyên)
        if (playerHeadBone != null)
        {
            currentHelmetObj = Instantiate(data.prefab, playerHeadBone);
            ApplyArmorTransform(currentHelmetObj, data);
            SetLayerRecursively(currentHelmetObj, LayerMask.NameToLayer("Default"));
        }
        EquipArmorOnPreview(data, previewHeadBone, ref currentPreviewHelmetObj);
    }
    public void UnequipHelmet3D()
    {
        // TRỪ CHỈ SỐ CŨ
        if (playerStats != null && currentHelmetData != null)
            playerStats.RemoveBonus(currentHelmetData.atkBonus, currentHelmetData.defBonus, currentHelmetData.hpBonus);

        currentHelmetData = null;
        if (currentHelmetObj != null) Destroy(currentHelmetObj);
        if (currentPreviewHelmetObj != null) Destroy(currentPreviewHelmetObj);
    }

    // --- 3. CHEST (ÁO) ---
    public void EquipChest3D(ArmorData data)
    {
        // TRỪ CHỈ SỐ CŨ
        if (currentChestData != null && playerStats != null)
        {
            playerStats.RemoveBonus(currentChestData.atkBonus, currentChestData.defBonus, currentChestData.hpBonus);
        }

        UnequipChest3D(); // Xóa hình ảnh áo cũ
        if (data == null || data.prefab == null) return;

        //  CẬP NHẬT & CỘNG MỚI
        currentChestData = data;
        if (playerStats != null)
        {
            playerStats.AddBonus(data.atkBonus, data.defBonus, data.hpBonus);
        }

        // (Phần hiển thị 3D giữ nguyên)
        if (playerChestBone != null)
        {
            currentChestObj = Instantiate(data.prefab, playerChestBone);
            ApplyArmorTransform(currentChestObj, data);
            SetLayerRecursively(currentChestObj, LayerMask.NameToLayer("Default"));
        }
        EquipArmorOnPreview(data, previewChestBone, ref currentPreviewChestObj);
    }
    public void UnequipChest3D()
    {
        // TRỪ CHỈ SỐ CŨ
        if (playerStats != null && currentChestData != null)
            playerStats.RemoveBonus(currentChestData.atkBonus, currentChestData.defBonus, currentChestData.hpBonus);

        currentChestData = null;
        if (currentChestObj != null) Destroy(currentChestObj);
        if (currentPreviewChestObj != null) Destroy(currentPreviewChestObj);
    }

    // ===================== HELPER FUNCTIONS =====================
    void EquipArmorOnPreview(ArmorData data, Transform bone, ref GameObject currentObj)
    {
        if (currentObj != null) Destroy(currentObj);
        if (bone != null && data != null && data.prefab != null)
        {
            currentObj = Instantiate(data.prefab, bone);
            ApplyArmorTransform(currentObj, data);
            int uiLayer = LayerMask.NameToLayer(previewLayerName);
            SetLayerRecursively(currentObj, uiLayer);
        }
    }

    void ApplyArmorTransform(GameObject obj, ArmorData data)
    {
        obj.transform.localPosition = data.localPos;
        obj.transform.localEulerAngles = data.localEuler;
        obj.transform.localScale = data.localScale;
    }

    void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform) SetLayerRecursively(child.gameObject, newLayer);
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
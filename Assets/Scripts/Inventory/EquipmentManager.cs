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

    [Header("Tùy chỉnh kích thước nón")]
    public Vector3 helmetScaleRuntime = new Vector3(0.23f, 0.2f, 0.22f);
    public Vector3 helmetScaleUI = new Vector3(0.254415f, 0.18957f, 0.21072f);

    private InventoryGridManager gridManager;

    [Header("Lưu trữ đồ đang mặc")]
    public WeaponData currentWeapon;
    public WeaponData currentHelmet;
    public WeaponData currentChest;

    [Header("Upgrade level của đồ đang mặc")]
    public int weaponUpgradeLevel = 1;
    public int helmetUpgradeLevel = 1;
    public int chestUpgradeLevel = 1;

    [Header("Xương Nhân Vật (Preview)")]
    public Transform previewHeadBone;

    private Transform runtimeHeadBone;
    private GameObject currentHelmetObj_UI;
    private GameObject currentHelmetObj_Runtime;

    [Header("3D Weapon Equip (runtime player + preview)")]
    public WeaponEquipper playerWeaponEquipper;   // runtime PlayerRuntime
    public WeaponEquipper previewWeaponEquipper;  // Preview_Player (UI)

    [Header("Preview")]
    public Transform previewRoot;                 // UI_PreviewRoot
    public string previewLayerName = "UIPreview";

    [Header("Animator weapon type")]
    [Tooltip("Giá trị WeaponType khi KHÔNG cầm vũ khí")]
    public int unarmedWeaponType = 0;            // <<< THÊM DÒNG NÀY

    public bool isFemale = false;

    [System.Serializable]
    public class WeaponIconMap
    {
        public Sprite icon;
        public WeaponData data;
    }

    [System.Serializable]
    public class HelmetIconMap
    {
        public Sprite icon;
        public WeaponData data;
    }

    [System.Serializable]
    public class ChestIconMap
    {
        public Sprite icon;
        public WeaponData data;
    }

    [Header("Map icon -> Helmet Data")]
    public HelmetIconMap[] helmetMaps;

    [Header("Map icon -> WeaponData")]
    public WeaponIconMap[] weaponMaps;

    [Header("Map icon -> Chest Data")]
    public ChestIconMap[] chestMaps;

    private Transform runtimeChestBone;
    private Transform previewChestBone;
    private GameObject currentChestObj_UI;
    private GameObject currentChestObj_Runtime;

    private WeaponData currentWeaponData;

    void Start()
    {
        gridManager = FindFirstObjectByType<InventoryGridManager>();
        isFemale = GenderSelector.SelectedIsFemale;
        Debug.Log($"[EquipmentManager] Khởi tạo với giới tính: {(isFemale ? "NỮ" : "NAM")}");

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
    public Sprite EquipItem(InventoryItem.ItemType type,
                            Sprite newItemSprite,
                            GameObject prefab3D,
                            int upgradeLevel = 1)
    {
        if (newItemSprite == null || newItemSprite.name == "Icon" || newItemSprite.name == "EmptySlot")
        {
            Debug.Log("[EquipItem] Click vào ô trống, bỏ qua xử lý.");
            return null;
        }

        Image targetSlot = GetTargetSlot(type);
        if (targetSlot == null) return null;

        Sprite old = (targetSlot.enabled && targetSlot.sprite != null) ? targetSlot.sprite : null;

        targetSlot.sprite = newItemSprite;
        targetSlot.enabled = true;

        Debug.Log($"[EquipItem] type={type} icon={newItemSprite.name}, lv={upgradeLevel}");

        WeaponData wd = null;
        WeaponData hd = null;
        WeaponData cd = null;

        // VŨ KHÍ
        if (type == InventoryItem.ItemType.Weapon)
        {
            wd = FindWeaponDataByIcon(newItemSprite);
            if (wd != null)
            {
                currentWeapon = wd;
                currentWeaponData = wd;
                weaponUpgradeLevel = Mathf.Max(1, upgradeLevel);

                EquipWeapon3D(wd);
            }
            else
            {
                Debug.LogError($"[EquipItem] Không map được icon '{newItemSprite.name}' -> WeaponData.");
            }
        }
        // NÓN
        else if (type == InventoryItem.ItemType.Head)
        {
            hd = FindHelmetDataByIcon(newItemSprite);
            if (hd != null)
            {
                currentHelmet = hd;
                helmetUpgradeLevel = Mathf.Max(1, upgradeLevel);

                EquipHelmet3D(hd);
            }
            else
            {
                Debug.LogWarning($"[EquipItem] Không tìm thấy HelmetData trong helmetMaps cho {newItemSprite.name}.");
            }
        }
        // GIÁP
        else if (type == InventoryItem.ItemType.Chest)
        {
            cd = FindChestDataByIcon(newItemSprite);
            if (cd != null)
            {
                currentChest = cd;
                chestUpgradeLevel = Mathf.Max(1, upgradeLevel);

                slotChest.sprite = newItemSprite;
                slotChest.enabled = true;

                EquipChest3D(cd);
            }
            else
            {
                Debug.LogError($"[EquipItem] Không tìm thấy ChestData cho {newItemSprite.name} trong chestMaps.");
            }
        }

        // Cập nhật Stats
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) player = GameObject.Find("PlayerRuntime");

        if (player != null)
        {
            CharacterStats stats = player.GetComponent<CharacterStats>();
            if (stats != null)
            {
                stats.UpdateFinalStats();
                Debug.Log("[EquipmentManager] Đã cập nhật Stats cho Player thành công!");
            }
        }

        UpdateButtons();
        return old;
    }

    // Cho EnhancementPanel/ngoài dùng
    public WeaponData FindHelmetDataByIcon(Sprite icon)
    {
        if (icon == null || helmetMaps == null) return null;
        foreach (var m in helmetMaps)
        {
            if (m != null && m.icon != null && (m.icon == icon || m.icon.name == icon.name))
                return m.data;
        }
        return null;
    }

    public WeaponData FindWeaponDataByIcon(Sprite icon)
    {
        if (icon == null || weaponMaps == null) return null;
        foreach (var m in weaponMaps)
        {
            if (m != null && m.data != null && m.icon != null)
            {
                if (m.icon == icon || m.icon.name == icon.name)
                    return m.data;
            }
        }
        return null;
    }

    public WeaponData FindChestDataByIcon(Sprite icon)
    {
        if (icon == null || chestMaps == null) return null;
        foreach (var m in chestMaps)
        {
            if (m != null && m.icon != null && (m.icon == icon || m.icon.name == icon.name))
                return m.data;
        }
        return null;
    }

    public void UnequipItem(InventoryItem.ItemType type)
    {
        Image targetSlot = GetTargetSlot(type);
        if (targetSlot == null) return;
        if (!targetSlot.enabled || targetSlot.sprite == null) return;

        if (gridManager != null && gridManager.AddItemBackToInventory(targetSlot.sprite, type, null))
        {
            targetSlot.sprite = null;
            targetSlot.enabled = false;

            if (type == InventoryItem.ItemType.Weapon)
                UnequipWeapon3D();

            if (type == InventoryItem.ItemType.Head)
            {
                if (currentHelmetObj_UI != null) Destroy(currentHelmetObj_UI);
                if (currentHelmetObj_Runtime != null) Destroy(currentHelmetObj_Runtime);
            }

            if (type == InventoryItem.ItemType.Chest)
            {
                if (currentChestObj_UI != null) Destroy(currentChestObj_UI);
                if (currentChestObj_Runtime != null) Destroy(currentChestObj_Runtime);
            }

            // reset data + level
            if (type == InventoryItem.ItemType.Weapon)
            {
                currentWeapon = null;
                weaponUpgradeLevel = 1;
            }
            else if (type == InventoryItem.ItemType.Head)
            {
                currentHelmet = null;
                helmetUpgradeLevel = 1;
            }
            else if (type == InventoryItem.ItemType.Chest)
            {
                currentChest = null;
                chestUpgradeLevel = 1;
            }

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) player = GameObject.Find("PlayerRuntime");

            if (player != null)
            {
                CharacterStats stats = player.GetComponent<CharacterStats>();
                if (stats != null)
                {
                    stats.UpdateFinalStats();
                    Debug.Log("[EquipmentManager] Đã cập nhật Stats cho Player thành công (Unequip)!");
                }
            }

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

    public void BindPreviewNow()
    {
        var go = GameObject.Find("UI_PreviewRoot");
        if (go != null)
        {
            previewRoot = go.transform;
        }

        if (previewRoot == null) return;

        LateBindPreviewEquipperIfNeeded();

        if (currentWeaponData != null && previewWeaponEquipper != null)
        {
            int layer = LayerMask.NameToLayer(previewLayerName);
            previewWeaponEquipper.Equip(currentWeaponData, layer);
        }
    }

    void LateBindPreviewEquipperIfNeeded()
    {
        if (previewRoot == null) return;

        if (previewHeadBone == null)
        {
            foreach (var t in previewRoot.GetComponentsInChildren<Transform>(true))
            {
                if (t.name.Contains("Head"))
                {
                    previewHeadBone = t;
                    break;
                }
            }
        }

        if (previewWeaponEquipper == null)
        {
            Transform socket = null;
            foreach (var t in previewRoot.GetComponentsInChildren<Transform>(true))
            {
                if (t.name.Trim() == "WeaponSocket_R")
                {
                    socket = t;
                    break;
                }
            }

            if (socket != null)
            {
                var anim = socket.GetComponentInParent<Animator>(true);
                if (anim != null)
                {
                    previewWeaponEquipper = anim.GetComponent<WeaponEquipper>();
                    if (previewWeaponEquipper == null)
                        previewWeaponEquipper = anim.gameObject.AddComponent<WeaponEquipper>();
                    previewWeaponEquipper.socketR = socket;
                }
            }
        }
    }

    public void EquipWeapon3D(WeaponData weaponData)
    {
        if (weaponData == null) return;
        if (playerWeaponEquipper == null)
        {
            var player = GameObject.FindWithTag("Player");
            if (player != null)
                playerWeaponEquipper = player.GetComponentInChildren<WeaponEquipper>(true);
        }

        if (playerWeaponEquipper != null)
        {
            var anim = playerWeaponEquipper.GetComponentInChildren<Animator>(true);
            if (anim != null) anim.SetInteger("WeaponType", weaponData.animationID);
            playerWeaponEquipper.Equip(weaponData);
        }

        if (previewWeaponEquipper != null)
        {
            int layer = LayerMask.NameToLayer(previewLayerName);
            previewWeaponEquipper.Equip(weaponData, layer);
        }
    }

    public void UnequipWeapon3D()
    {
        // <<< SỬA HÀM NÀY
        if (playerWeaponEquipper != null)
        {
            var anim = playerWeaponEquipper.GetComponentInChildren<Animator>(true);
            if (anim != null)
            {
                // reset về trạng thái đánh tay không
                anim.SetInteger("WeaponType", unarmedWeaponType);
            }

            playerWeaponEquipper.Unequip();
        }

        if (previewWeaponEquipper != null)
            previewWeaponEquipper.Unequip();

        currentWeaponData = null;
    }

    void EquipHelmet3D(WeaponData hd)
    {
        if (currentHelmetObj_UI != null) Destroy(currentHelmetObj_UI);
        if (currentHelmetObj_Runtime != null) Destroy(currentHelmetObj_Runtime);

        if (hd == null || hd.prefab == null) return;

        if (previewRoot == null) BindPreviewNow();
        if (previewHeadBone == null) LateBindPreviewEquipperIfNeeded();

        if (previewHeadBone != null)
        {
            currentHelmetObj_UI = Instantiate(hd.prefab, previewHeadBone);

            currentHelmetObj_UI.transform.localPosition = isFemale ? hd.femaleHeadPos : hd.headPos;
            currentHelmetObj_UI.transform.localRotation = Quaternion.Euler(isFemale ? hd.femaleHeadEuler : hd.headEuler);
            currentHelmetObj_UI.transform.localScale = isFemale ? hd.femaleHeadScaleUI : hd.headScaleUI;

            SetLayerRecursively(currentHelmetObj_UI, previewHeadBone.gameObject.layer);
        }

        FindRuntimeHeadBone();
        if (runtimeHeadBone != null)
        {
            currentHelmetObj_Runtime = Instantiate(hd.prefab, runtimeHeadBone);

            currentHelmetObj_Runtime.transform.localPosition = isFemale ? hd.femaleHeadPos : hd.headPos;
            currentHelmetObj_Runtime.transform.localRotation = Quaternion.Euler(isFemale ? hd.femaleHeadEuler : hd.headEuler);
            currentHelmetObj_Runtime.transform.localScale = isFemale ? hd.femaleHeadScaleRuntime : hd.headScaleRuntime;

            SetLayerRecursively(currentHelmetObj_Runtime, 0);
        }
    }

    void EquipChest3D(WeaponData cd)
    {
        if (currentChestObj_UI != null) Destroy(currentChestObj_UI);
        if (currentChestObj_Runtime != null) Destroy(currentChestObj_Runtime);
        if (cd == null || cd.prefab == null) return;

        FindChestBones();

        if (previewChestBone != null)
        {
            currentChestObj_UI = Instantiate(cd.prefab, previewChestBone);

            currentChestObj_UI.transform.localPosition = isFemale ? cd.femaleChestPos : cd.chestPos;
            currentChestObj_UI.transform.localRotation = Quaternion.Euler(isFemale ? cd.femaleChestEuler : cd.chestEuler);
            currentChestObj_UI.transform.localScale = isFemale ? cd.femaleChestScaleUI : cd.chestScaleUI;

            SetLayerRecursively(currentChestObj_UI, previewChestBone.gameObject.layer);
        }

        if (runtimeChestBone != null)
        {
            currentChestObj_Runtime = Instantiate(cd.prefab, runtimeChestBone);

            currentChestObj_Runtime.transform.localPosition = isFemale ? cd.femaleChestPos : cd.chestPos;
            currentChestObj_Runtime.transform.localRotation = Quaternion.Euler(isFemale ? cd.femaleChestEuler : cd.chestEuler);
            currentChestObj_Runtime.transform.localScale = isFemale ? cd.femaleChestScaleRuntime : cd.chestScaleRuntime;

            SetLayerRecursively(currentChestObj_Runtime, 0);
        }
    }

    void FindRuntimeHeadBone()
    {
        if (runtimeHeadBone != null) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) player = GameObject.Find("PlayerRuntime");

        if (player != null)
        {
            foreach (var t in player.GetComponentsInChildren<Transform>(true))
            {
                if (t.name.Contains("Head"))
                {
                    runtimeHeadBone = t;
                    break;
                }
            }
        }
    }

    void FindChestBones()
    {
        if (previewChestBone == null && previewRoot != null)
        {
            previewChestBone = FindBestSpineBone(previewRoot);
        }

        if (runtimeChestBone == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) player = GameObject.Find("PlayerRuntime");

            if (player != null)
            {
                runtimeChestBone = FindBestSpineBone(player.transform);
            }
        }
    }

    Transform FindBestSpineBone(Transform root)
    {
        Transform bestBone = null;
        var allChildren = root.GetComponentsInChildren<Transform>(true);

        foreach (var t in allChildren)
        {
            if (t.name.Contains("Spine2")) return t;

            if (t.name.Contains("Spine1")) bestBone = t;

            if (bestBone == null && t.name.EndsWith("Spine")) bestBone = t;
        }
        return bestBone;
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

    void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}

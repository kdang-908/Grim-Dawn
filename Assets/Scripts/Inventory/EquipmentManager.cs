using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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
    public WeaponEquipper playerWeaponEquipper;
    public WeaponEquipper previewWeaponEquipper;

    [Header("Preview")]
    public Transform previewRoot;
    public string previewLayerName = "UIPreview";

    [Header("Animator weapon type")]
    [Tooltip("Giá trị WeaponType khi KHÔNG cầm vũ khí")]
    public int unarmedWeaponType = 0;

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

    [System.Serializable]
    public class EquippedSaveData
    {
        public WeaponData weapon;
        public int weaponLv;

        public WeaponData helmet;
        public int helmetLv;

        public WeaponData chest;
        public int chestLv;
    }

    public static EquippedSaveData GlobalEquippedSave = new EquippedSaveData();
    public static bool HasEquippedSave = false;

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
        gridManager = FindFirstObjectByType<InventoryGridManager>(FindObjectsInactive.Include);

        isFemale = GenderSelector.SelectedIsFemale;

        if (previewRoot == null)
        {
            var go = GameObject.Find("UI_PreviewRoot");
            if (go != null) previewRoot = go.transform;
        }

        AutoBindRemoveButtons();
        UpdateButtons();

        // ✅ Restore sau khi scene đã ổn định + chỉ còn 1 player
        StartCoroutine(RestoreEquippedSafe());
    }

    // =========================
    // ✅ RESTORE SAFE (chống 2 Player trong vài frame)
    // =========================
    private IEnumerator RestoreEquippedSafe()
    {
        yield return null;
        yield return new WaitForEndOfFrame();

        // chờ dọn duplicate Player (nếu có)
        for (int i = 0; i < 6; i++)
        {
            var players = GameObject.FindGameObjectsWithTag("Player");
            if (players == null || players.Length <= 1) break;
            yield return null;
        }

        // reset cache bone để không bám nhầm player cũ
        runtimeHeadBone = null;
        runtimeChestBone = null;

        RestoreEquippedState();
    }

    void UpdateButtons()
    {
        if (btnRemoveHead != null) btnRemoveHead.SetActive(slotHead && slotHead.enabled && slotHead.sprite);
        if (btnRemoveChest != null) btnRemoveChest.SetActive(slotChest && slotChest.enabled && slotChest.sprite);
        if (btnRemoveLegs != null) btnRemoveLegs.SetActive(slotLegs && slotLegs.enabled && slotLegs.sprite);
        if (btnRemoveWeapon != null) btnRemoveWeapon.SetActive(slotWeapon && slotWeapon.enabled && slotWeapon.sprite);
    }

    // =========================
    // ✅ FIX: Clear GlobalEquippedSave đúng slot ngay khi unequip
    // =========================
    void ClearGlobalEquippedSlot(InventoryItem.ItemType type)
    {
        if (type == InventoryItem.ItemType.Weapon)
        {
            GlobalEquippedSave.weapon = null;
            GlobalEquippedSave.weaponLv = 1;
        }
        else if (type == InventoryItem.ItemType.Head)
        {
            GlobalEquippedSave.helmet = null;
            GlobalEquippedSave.helmetLv = 1;
        }
        else if (type == InventoryItem.ItemType.Chest)
        {
            GlobalEquippedSave.chest = null;
            GlobalEquippedSave.chestLv = 1;
        }

        HasEquippedSave = (GlobalEquippedSave.weapon != null ||
                           GlobalEquippedSave.helmet != null ||
                           GlobalEquippedSave.chest != null);
    }

    // ============================================================
    // ✅ TAG SYSTEM (CÁCH CHẮC ĂN NHẤT)
    // - Mọi object equip instantiate sẽ được gắn EquippedRuntimeTag
    // - Khi tháo đồ hoặc restore sẽ quét và destroy theo tag (không đoán bone/socket)
    // ============================================================
    void AddTag(GameObject go)
    {
        if (go == null) return;
        if (go.GetComponent<EquippedRuntimeTag>() == null)
            go.AddComponent<EquippedRuntimeTag>();
    }

    void MarkWeaponChildren(WeaponEquipper eq)
    {
        if (eq == null || eq.socketR == null) return;

        for (int i = 0; i < eq.socketR.childCount; i++)
        {
            var ch = eq.socketR.GetChild(i);
            if (ch == null) continue;

            // ✅ KHÔNG TAG HITBOX (để ForceDestroyTaggedEquipped không xóa nhầm)
            if (ch.name == "PlayerHitbox") continue;
            if (ch.GetComponentInChildren<DamageHitbox>(true) != null) continue;

            AddTag(ch.gameObject);
        }
    }


    void ForceDestroyTaggedEquipped()
    {
        // Preview
        if (previewRoot != null)
        {
            var tags = previewRoot.GetComponentsInChildren<EquippedRuntimeTag>(true);
            foreach (var t in tags)
            {
                if (t == null) continue;

                // ✅ KHÔNG XÓA HITBOX
                if (t.name == "PlayerHitbox") continue;
                if (t.GetComponentInChildren<DamageHitbox>(true) != null) continue;

                t.gameObject.SetActive(false);
                Destroy(t.gameObject);
            }
        }

        // Runtime player
        var player = GameObject.FindGameObjectWithTag("Player") ?? GameObject.Find("PlayerRuntime");
        if (player != null)
        {
            var tags = player.GetComponentsInChildren<EquippedRuntimeTag>(true);
            foreach (var t in tags)
            {
                if (t == null) continue;

                // ✅ KHÔNG XÓA HITBOX
                if (t.name == "PlayerHitbox") continue;
                if (t.GetComponentInChildren<DamageHitbox>(true) != null) continue;

                t.gameObject.SetActive(false);
                Destroy(t.gameObject);
            }
        }
    }


    public Sprite EquipItem(InventoryItem.ItemType type,
                        Sprite newItemSprite,
                        GameObject prefab3D,
                        int upgradeLevel,
                        out WeaponData oldData,
                        out int oldLevel)
    {
        oldData = null;
        oldLevel = 1;

        if (newItemSprite == null || newItemSprite.name == "Icon" || newItemSprite.name == "EmptySlot")
            return null;

        Image targetSlot = GetTargetSlot(type);
        if (targetSlot == null) return null;

        Sprite oldSprite = (targetSlot.enabled && targetSlot.sprite != null) ? targetSlot.sprite : null;

        if (oldSprite != null)
        {
            if (type == InventoryItem.ItemType.Weapon)
            {
                oldData = currentWeapon;
                oldLevel = weaponUpgradeLevel;
                if (oldData == null) oldData = FindWeaponDataByIcon(oldSprite);
            }
            else if (type == InventoryItem.ItemType.Head)
            {
                oldData = currentHelmet;
                oldLevel = helmetUpgradeLevel;
                if (oldData == null) oldData = FindHelmetDataByIcon(oldSprite);
            }
            else if (type == InventoryItem.ItemType.Chest)
            {
                oldData = currentChest;
                oldLevel = chestUpgradeLevel;
                if (oldData == null) oldData = FindChestDataByIcon(oldSprite);
            }
        }

        targetSlot.sprite = newItemSprite;
        targetSlot.enabled = true;

        EquipmentSlotUI uiSlot = targetSlot.GetComponent<EquipmentSlotUI>();

        if (type == InventoryItem.ItemType.Weapon)
        {
            var wd = FindWeaponDataByIcon(newItemSprite);
            if (wd != null)
            {
                currentWeapon = wd;
                currentWeaponData = wd;
                weaponUpgradeLevel = Mathf.Max(1, upgradeLevel);

                EquipWeapon3D(wd);

                // ✅ tag vũ khí (vì WeaponEquipper instantiate bên trong)
                MarkWeaponChildren(playerWeaponEquipper);
                MarkWeaponChildren(previewWeaponEquipper);

                if (uiSlot != null) uiSlot.Setup(wd, weaponUpgradeLevel);
            }
        }
        else if (type == InventoryItem.ItemType.Head)
        {
            var hd = FindHelmetDataByIcon(newItemSprite);
            if (hd != null)
            {
                currentHelmet = hd;
                helmetUpgradeLevel = Mathf.Max(1, upgradeLevel);
                EquipHelmet3D(hd);
                if (uiSlot != null) uiSlot.Setup(hd, helmetUpgradeLevel);
            }
        }
        else if (type == InventoryItem.ItemType.Chest)
        {
            var cd = FindChestDataByIcon(newItemSprite);
            if (cd != null)
            {
                currentChest = cd;
                chestUpgradeLevel = Mathf.Max(1, upgradeLevel);
                EquipChest3D(cd);
                if (uiSlot != null) uiSlot.Setup(cd, chestUpgradeLevel);
            }
        }

        // update stats
        var player = GameObject.FindGameObjectWithTag("Player") ?? GameObject.Find("PlayerRuntime");
        if (player != null)
        {
            var stats = player.GetComponent<CharacterStats>();
            if (stats != null) stats.UpdateFinalStats(keepCurrentHP: true);
        }

        SaveEquippedState();
        UpdateButtons();
        return oldSprite;
    }

    // =========================
    // ✅ UNEQUIP (FIX chuẩn)
    // - remove khỏi UI
    // - destroy helmet objects
    // - clear GlobalEquippedSave slot
    // - ✅ QUAN TRỌNG: ForceDestroyTaggedEquipped() để xóa đồ còn dính ở nơi khác
    // =========================
    public void UnequipItem(InventoryItem.ItemType type)
    {
        Image targetSlot = GetTargetSlot(type);
        if (targetSlot == null) return;
        if (!targetSlot.enabled || targetSlot.sprite == null) return;

        if (gridManager == null)
            gridManager = FindFirstObjectByType<InventoryGridManager>(FindObjectsInactive.Include);

        WeaponData dataToReturn = null;
        int levelToReturn = 1;

        if (type == InventoryItem.ItemType.Weapon)
        {
            dataToReturn = currentWeapon;
            levelToReturn = weaponUpgradeLevel;
        }
        else if (type == InventoryItem.ItemType.Head)
        {
            dataToReturn = currentHelmet;
            levelToReturn = helmetUpgradeLevel;
        }
        else if (type == InventoryItem.ItemType.Chest)
        {
            dataToReturn = currentChest;
            levelToReturn = chestUpgradeLevel;
        }

        // ✅ Add về GLOBAL inventory + reload UI
        if (gridManager != null && dataToReturn != null)
        {
            InventoryGridManager.GlobalInventorySave.Add(new InventoryGridManager.SavedInvItem
            {
                data = dataToReturn,
                level = Mathf.Max(1, levelToReturn)
            });

            var managers = FindObjectsByType<InventoryGridManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var m in managers)
            {
                if (m != null && m.isActiveAndEnabled && m.gameObject.activeInHierarchy)
                    m.EnsureLoaded(forceReloadFromGlobal: true);
            }
        }

        // clear UI slot
        targetSlot.sprite = null;
        targetSlot.enabled = false;

        var uiSlot = targetSlot.GetComponent<EquipmentSlotUI>();
        if (uiSlot != null) uiSlot.Clear();

        // destroy 3D + clear data
        if (type == InventoryItem.ItemType.Weapon)
        {
            UnequipWeapon3D();
            currentWeapon = null;
            weaponUpgradeLevel = 1;
        }
        else if (type == InventoryItem.ItemType.Head)
        {
            DestroyHelmetObjects();
            currentHelmet = null;
            helmetUpgradeLevel = 1;
        }
        else if (type == InventoryItem.ItemType.Chest)
        {
            if (currentChestObj_UI != null) Destroy(currentChestObj_UI);
            if (currentChestObj_Runtime != null) Destroy(currentChestObj_Runtime);
            currentChestObj_UI = null;
            currentChestObj_Runtime = null;

            currentChest = null;
            chestUpgradeLevel = 1;
        }

        // ✅ FIX CHÍNH: clear slot trong save equip NGAY
        ClearGlobalEquippedSlot(type);

        // ✅ CÁI QUAN TRỌNG NHẤT: quét & xóa mọi đồ equip còn dính (dù nằm sai bone/socket)
        ForceDestroyTaggedEquipped();

        // update stats
        var player = GameObject.FindGameObjectWithTag("Player") ?? GameObject.Find("PlayerRuntime");
        if (player != null)
        {
            var stats = player.GetComponent<CharacterStats>();
            if (stats != null) stats.UpdateFinalStats(keepCurrentHP: true);
        }

        SaveEquippedState();
        UpdateButtons();

        // ✅ refresh shop lock state ngay lập tức (khỏi cần đóng/mở shop)
        var shop = FindFirstObjectByType<ShopUIController>(FindObjectsInactive.Include);
        if (shop != null) shop.RefreshAllShopSlots();

    }

    void DestroyHelmetObjects()
    {
        // ===== PREVIEW =====
        if (previewHeadBone != null)
        {
            var all = previewHeadBone.GetComponentsInChildren<Transform>(true);
            for (int i = all.Length - 1; i >= 0; i--)
            {
                var t = all[i];
                if (t == null) continue;
                if (t == previewHeadBone) continue;

                if (t.name == "EQ_HELMET_UI" || t.name.Contains("HELMET") || t.name.ToLower().Contains("helmet") || t.name.ToLower().Contains("hat"))
                {
                    t.gameObject.SetActive(false);
                    Destroy(t.gameObject);
                }
            }
        }

        // ===== RUNTIME =====
        runtimeHeadBone = null; // reset cache để tránh bám nhầm player cũ
        FindRuntimeHeadBone();

        if (runtimeHeadBone != null)
        {
            var all = runtimeHeadBone.GetComponentsInChildren<Transform>(true);
            for (int i = all.Length - 1; i >= 0; i--)
            {
                var t = all[i];
                if (t == null) continue;
                if (t == runtimeHeadBone) continue;

                if (t.name == "EQ_HELMET_RT" || t.name.Contains("HELMET") || t.name.ToLower().Contains("helmet") || t.name.ToLower().Contains("hat"))
                {
                    t.gameObject.SetActive(false);
                    Destroy(t.gameObject);
                }
            }
        }

        // fallback reference
        if (currentHelmetObj_UI != null)
        {
            currentHelmetObj_UI.SetActive(false);
            Destroy(currentHelmetObj_UI);
        }
        if (currentHelmetObj_Runtime != null)
        {
            currentHelmetObj_Runtime.SetActive(false);
            Destroy(currentHelmetObj_Runtime);
        }

        currentHelmetObj_UI = null;
        currentHelmetObj_Runtime = null;
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
        if (go != null) previewRoot = go.transform;
        if (previewRoot == null) return;

        LateBindPreviewEquipperIfNeeded();

        if (currentWeaponData != null && previewWeaponEquipper != null)
        {
            int layer = LayerMask.NameToLayer(previewLayerName);
            previewWeaponEquipper.Equip(currentWeaponData, layer);

            // ✅ tag weapon con
            MarkWeaponChildren(previewWeaponEquipper);
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

            // ✅ tag weapon con
            MarkWeaponChildren(playerWeaponEquipper);
        }

        if (previewWeaponEquipper != null)
        {
            int layer = LayerMask.NameToLayer(previewLayerName);
            previewWeaponEquipper.Equip(weaponData, layer);

            // ✅ tag weapon con
            MarkWeaponChildren(previewWeaponEquipper);
        }
    }

    public void UnequipWeapon3D()
    {
        if (playerWeaponEquipper != null)
        {
            var anim = playerWeaponEquipper.GetComponentInChildren<Animator>(true);
            if (anim != null) anim.SetInteger("WeaponType", unarmedWeaponType);
            playerWeaponEquipper.Unequip();
        }

        if (previewWeaponEquipper != null)
            previewWeaponEquipper.Unequip();

        currentWeaponData = null;
    }

    // =========================
    // ✅ EquipHelmet3D (GIỮ LOGIC CỦA BẠN)
    // + gắn EquippedRuntimeTag để ForceDestroyTaggedEquipped dọn sạch chắc chắn
    // =========================
    void EquipHelmet3D(WeaponData hd)
    {
        if (hd == null || hd.prefab == null) return;

        if (previewRoot == null) BindPreviewNow();
        if (previewHeadBone == null) LateBindPreviewEquipperIfNeeded();

        // clear preview by name
        if (previewHeadBone != null)
        {
            var oldUI = previewHeadBone.Find("EQ_HELMET_UI");
            if (oldUI != null) Destroy(oldUI.gameObject);
        }

        // clear runtime by name
        runtimeHeadBone = null;
        FindRuntimeHeadBone();
        if (runtimeHeadBone != null)
        {
            var oldRT = runtimeHeadBone.Find("EQ_HELMET_RT");
            if (oldRT != null) Destroy(oldRT.gameObject);
        }

        if (currentHelmetObj_UI != null) Destroy(currentHelmetObj_UI);
        if (currentHelmetObj_Runtime != null) Destroy(currentHelmetObj_Runtime);
        currentHelmetObj_UI = null;
        currentHelmetObj_Runtime = null;

        if (previewHeadBone != null)
        {
            currentHelmetObj_UI = Instantiate(hd.prefab, previewHeadBone);
            currentHelmetObj_UI.name = "EQ_HELMET_UI";
            currentHelmetObj_UI.transform.localPosition = isFemale ? hd.femaleHeadPos : hd.headPos;
            currentHelmetObj_UI.transform.localRotation = Quaternion.Euler(isFemale ? hd.femaleHeadEuler : hd.headEuler);
            currentHelmetObj_UI.transform.localScale = isFemale ? hd.femaleHeadScaleUI : hd.headScaleUI;
            SetLayerRecursively(currentHelmetObj_UI, previewHeadBone.gameObject.layer);

            // ✅ TAG
            AddTag(currentHelmetObj_UI);
        }

        if (runtimeHeadBone != null)
        {
            currentHelmetObj_Runtime = Instantiate(hd.prefab, runtimeHeadBone);
            currentHelmetObj_Runtime.name = "EQ_HELMET_RT";
            currentHelmetObj_Runtime.transform.localPosition = isFemale ? hd.femaleHeadPos : hd.headPos;
            currentHelmetObj_Runtime.transform.localRotation = Quaternion.Euler(isFemale ? hd.femaleHeadEuler : hd.headEuler);
            currentHelmetObj_Runtime.transform.localScale = isFemale ? hd.femaleHeadScaleRuntime : hd.headScaleRuntime;
            SetLayerRecursively(currentHelmetObj_Runtime, 0);

            // ✅ TAG
            AddTag(currentHelmetObj_Runtime);
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

            // ✅ TAG
            AddTag(currentChestObj_UI);
        }

        if (runtimeChestBone != null)
        {
            currentChestObj_Runtime = Instantiate(cd.prefab, runtimeChestBone);
            currentChestObj_Runtime.transform.localPosition = isFemale ? cd.femaleChestPos : cd.chestPos;
            currentChestObj_Runtime.transform.localRotation = Quaternion.Euler(isFemale ? cd.femaleChestEuler : cd.chestEuler);
            currentChestObj_Runtime.transform.localScale = isFemale ? cd.femaleChestScaleRuntime : cd.chestScaleRuntime;
            SetLayerRecursively(currentChestObj_Runtime, 0);

            // ✅ TAG
            AddTag(currentChestObj_Runtime);
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
            previewChestBone = FindBestSpineBone(previewRoot);

        if (runtimeChestBone == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) player = GameObject.Find("PlayerRuntime");
            if (player != null) runtimeChestBone = FindBestSpineBone(player.transform);
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
            SetLayerRecursively(child.gameObject, newLayer);
    }

    // =========================
    // SAVE/RESTORE
    // =========================
    public void SaveEquippedState()
    {
        if (slotWeapon == null && slotHead == null && slotChest == null)
            return;

        GlobalEquippedSave.weapon = currentWeapon;
        GlobalEquippedSave.weaponLv = weaponUpgradeLevel;

        GlobalEquippedSave.helmet = currentHelmet;
        GlobalEquippedSave.helmetLv = helmetUpgradeLevel;

        GlobalEquippedSave.chest = currentChest;
        GlobalEquippedSave.chestLv = chestUpgradeLevel;

        HasEquippedSave = (currentWeapon != null || currentHelmet != null || currentChest != null);
    }

    public void RestoreEquippedState()
    {
        if (!HasEquippedSave)
        {
            UpdateButtons();
            return;
        }

        if (gridManager == null)
            gridManager = FindFirstObjectByType<InventoryGridManager>(FindObjectsInactive.Include);

        isFemale = GenderSelector.SelectedIsFemale;

        // ✅ IMPORTANT: clear tag trước khi restore để không bị dư đồ khi đóng/mở UI hoặc đổi map
        ForceDestroyTaggedEquipped();

        // Weapon
        currentWeapon = GlobalEquippedSave.weapon;
        weaponUpgradeLevel = Mathf.Max(1, GlobalEquippedSave.weaponLv);

        if (slotWeapon != null)
        {
            if (currentWeapon != null && currentWeapon.icon != null)
            {
                slotWeapon.sprite = currentWeapon.icon;
                slotWeapon.enabled = true;

                var ui = slotWeapon.GetComponent<EquipmentSlotUI>();
                if (ui != null) ui.Setup(currentWeapon, weaponUpgradeLevel);

                EquipWeapon3D(currentWeapon);
            }
            else
            {
                slotWeapon.sprite = null;
                slotWeapon.enabled = false;
            }
        }

        // Helmet
        currentHelmet = GlobalEquippedSave.helmet;
        helmetUpgradeLevel = Mathf.Max(1, GlobalEquippedSave.helmetLv);

        if (slotHead != null)
        {
            if (currentHelmet != null && currentHelmet.icon != null)
            {
                slotHead.sprite = currentHelmet.icon;
                slotHead.enabled = true;

                var ui = slotHead.GetComponent<EquipmentSlotUI>();
                if (ui != null) ui.Setup(currentHelmet, helmetUpgradeLevel);

                EquipHelmet3D(currentHelmet);
            }
            else
            {
                slotHead.sprite = null;
                slotHead.enabled = false;
            }
        }

        // Chest
        currentChest = GlobalEquippedSave.chest;
        chestUpgradeLevel = Mathf.Max(1, GlobalEquippedSave.chestLv);

        if (slotChest != null)
        {
            if (currentChest != null && currentChest.icon != null)
            {
                slotChest.sprite = currentChest.icon;
                slotChest.enabled = true;

                var ui = slotChest.GetComponent<EquipmentSlotUI>();
                if (ui != null) ui.Setup(currentChest, chestUpgradeLevel);

                EquipChest3D(currentChest);
            }
            else
            {
                slotChest.sprite = null;
                slotChest.enabled = false;
            }
        }

        // update stats
        var player = GameObject.FindGameObjectWithTag("Player") ?? GameObject.Find("PlayerRuntime");
        if (player != null)
        {
            var stats = player.GetComponent<CharacterStats>() ?? player.GetComponentInChildren<CharacterStats>(true);
            if (stats != null) stats.UpdateFinalStats(keepCurrentHP: true);
        }

        UpdateButtons();
    }

    // ===== Data map helpers =====
    public WeaponData FindHelmetDataByIcon(Sprite icon)
    {
        if (icon == null || helmetMaps == null) return null;
        foreach (var m in helmetMaps)
            if (m != null && m.icon != null && (m.icon == icon || m.icon.name == icon.name))
                return m.data;
        return null;
    }

    public WeaponData FindWeaponDataByIcon(Sprite icon)
    {
        if (icon == null || weaponMaps == null) return null;
        foreach (var m in weaponMaps)
            if (m != null && m.data != null && m.icon != null && (m.icon == icon || m.icon.name == icon.name))
                return m.data;
        return null;
    }

    public WeaponData FindChestDataByIcon(Sprite icon)
    {
        if (icon == null || chestMaps == null) return null;
        foreach (var m in chestMaps)
            if (m != null && m.icon != null && (m.icon == icon || m.icon.name == icon.name))
                return m.data;
        return null;
    }

    public void UnequipAllToInventory()
    {
        if (gridManager == null)
            gridManager = FindFirstObjectByType<InventoryGridManager>(FindObjectsInactive.Include);

        if (currentWeapon != null && slotWeapon != null && slotWeapon.sprite != null)
            UnequipItem(InventoryItem.ItemType.Weapon);

        if (currentHelmet != null && slotHead != null && slotHead.sprite != null)
            UnequipItem(InventoryItem.ItemType.Head);

        if (currentChest != null && slotChest != null && slotChest.sprite != null)
            UnequipItem(InventoryItem.ItemType.Chest);
    }

    // ============================================
    // BACKWARD COMPATIBILITY (EnhancementPanel needs)
    // ============================================
    public bool IsEquipped(WeaponData data)
    {
        if (data == null) return false;

        if (currentWeapon == data) return true;
        if (currentHelmet == data) return true;
        if (currentChest == data) return true;

        // fallback compare by name (buyOnce / clone ScriptableObject)
        if (currentWeapon != null && currentWeapon.name == data.name) return true;
        if (currentHelmet != null && currentHelmet.name == data.name) return true;
        if (currentChest != null && currentChest.name == data.name) return true;

        return false;
    }

    public bool IsEquippedByIcon(Sprite icon)
    {
        if (icon == null) return false;

        // try map icon -> data
        var w = FindWeaponDataByIcon(icon);
        if (w != null && IsEquipped(w)) return true;

        var h = FindHelmetDataByIcon(icon);
        if (h != null && IsEquipped(h)) return true;

        var c = FindChestDataByIcon(icon);
        if (c != null && IsEquipped(c)) return true;

        // fallback compare sprite name
        if (slotWeapon != null && slotWeapon.sprite != null && slotWeapon.sprite.name == icon.name) return true;
        if (slotHead != null && slotHead.sprite != null && slotHead.sprite.name == icon.name) return true;
        if (slotChest != null && slotChest.sprite != null && slotChest.sprite.name == icon.name) return true;

        return false;
    }

    // EnhancementPanel gọi để update UI level + update stats + save
    public void RefreshEquippedItem(InventoryItem.ItemType type, int newLevel)
    {
        EquipmentSlotUI uiSlot = null;
        WeaponData targetData = null;

        if (type == InventoryItem.ItemType.Weapon && currentWeapon != null)
        {
            weaponUpgradeLevel = Mathf.Max(1, newLevel);
            targetData = currentWeapon;
            if (slotWeapon != null) uiSlot = slotWeapon.GetComponent<EquipmentSlotUI>();
        }
        else if (type == InventoryItem.ItemType.Head && currentHelmet != null)
        {
            helmetUpgradeLevel = Mathf.Max(1, newLevel);
            targetData = currentHelmet;
            if (slotHead != null) uiSlot = slotHead.GetComponent<EquipmentSlotUI>();
        }
        else if (type == InventoryItem.ItemType.Chest && currentChest != null)
        {
            chestUpgradeLevel = Mathf.Max(1, newLevel);
            targetData = currentChest;
            if (slotChest != null) uiSlot = slotChest.GetComponent<EquipmentSlotUI>();
        }

        // update UI level text/badge
        if (uiSlot != null && targetData != null)
            uiSlot.Setup(targetData, Mathf.Max(1, newLevel));

        // refresh tooltip nếu đang mở
        if (InventoryTooltip.Instance != null && InventoryTooltip.Instance.gameObject.activeSelf)
            InventoryTooltip.Instance.ShowTooltip(targetData, Mathf.Max(1, newLevel));

        // recalc stats (giữ HP hiện tại)
        var player = GameObject.FindGameObjectWithTag("Player") ?? GameObject.Find("PlayerRuntime");
        if (player != null)
        {
            var stats = player.GetComponent<CharacterStats>() ?? player.GetComponentInChildren<CharacterStats>(true);
            if (stats != null) stats.UpdateFinalStats(keepCurrentHP: true);
        }

        SaveEquippedState();
        UpdateButtons();
    }
}

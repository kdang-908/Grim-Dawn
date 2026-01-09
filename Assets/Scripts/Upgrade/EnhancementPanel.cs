using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EnhancementPanel : MonoBehaviour
{
    public static EnhancementPanel Instance;


    // ===== SLOT NÂNG CẤP =====
    [Header("Slot nâng cấp")]
    public UpgradeSlot selectSlot;

    // ===== UI TRÁNH XUNG ĐỘT =====
    [Header("UI Conflict Handling")]
    public GameObject mainInventoryPanel; // Kéo "Canvas/Inventory" 
    public InventoryGridManager mainInventoryManager;
    // ===== LEVEL =====
    [Header("UI Level")]
    public TMP_Text txtLevelFrom;        // số bên trái (cấp hiện tại)
    public TMP_Text txtLevelTo;          // số bên phải (cấp kế tiếp)

    // ===== CHỈ SỐ TRƯỚC / SAU =====
    [Header("UI Stats (Trước / Sau)")]
    public TMP_Text txtAtkFrom;
    public TMP_Text txtAtkTo;
    public TMP_Text txtDefFrom;
    public TMP_Text txtDefTo;
    public TMP_Text txtHpFrom;
    public TMP_Text txtHpTo;
    public TMP_Text txtEnergyFrom;
    public TMP_Text txtEnergyTo;

    // ===== DÒNG DƯỚI (GOLD, TỈ LỆ, NÚT) =====
    [Header("UI Bottom")]
    public TMP_Text txtSuccessRate;      // "100% Success Rate"
    public TMP_Text txtCost;             // "50 Gold"
    public Button btnEnhance;            // nút Nâng cấp

    // ====== ÂM THANH NÂNG CẤP ======
    [Header("Upgrade Sound")]
    public AudioSource audioSource;      // AudioSource để play SFX
    public AudioClip successClip;        // đập thành công
    public AudioClip failClip;           // đập fail
    [Range(0f, 1f)] public float sfxVolume = 0.9f;

    // ===== THAM CHIẾU EQUIPMENT MANAGER =====
    [Header("Refs")]
    public EquipmentManager equipmentManager;   // để map icon -> WeaponData / ArmorData

    private void Awake()
    {
        Instance = this;
    }

    // ================= XỬ LÝ ẨN HIỆN UI  =================
    private void OnEnable()
    {
        // Khi bật bảng nâng cấp -> TẮT túi đồ chính đi để không bị click nhầm
        if (mainInventoryPanel != null)
        {
            mainInventoryPanel.SetActive(false);
            Debug.Log("[EnhancementPanel] Đã ẩn Main Inventory để tránh xung đột.");
        }
        else
        {
            // Tự tìm nếu quên kéo
            GameObject foundInv = GameObject.Find("Inventory");
            if (foundInv != null)
            {
                mainInventoryPanel = foundInv;
                mainInventoryPanel.SetActive(false);
            }
        }
    }

    private void OnDisable()
    {
        // Khi tắt bảng nâng cấp -> BẬT lại túi đồ chính
        if (mainInventoryPanel != null)
        {
            mainInventoryPanel.SetActive(true);
        }
    }
    // ====================================================================

    public bool IsOpen() => gameObject.activeInHierarchy;

    private void Start()
    {
        // Nếu quên gán AudioSource thì tìm trên cùng object
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        // Nếu quên kéo EquipmentManager thì tìm trong scene (kể cả object đang tắt)
        if (equipmentManager == null)
            equipmentManager = FindFirstObjectByType<EquipmentManager>(FindObjectsInactive.Include);

        RefreshUI();
    }

    // ================= GOLD TỪ GAMEMANAGER =================
    int CurrentGold
    {
        get
        {
            var gm = GameManager.Instance;
            return gm != null ? gm.gold : 0;
        }
    }

    bool TrySpendGold(int amount)
    {
        var gm = GameManager.Instance;
        if (gm == null) return true; // nếu không có GameManager thì cho qua 

        if (gm.gold < amount)
            return false;

        gm.gold -= amount;
        return true;
    }

    // ================= NHẬN ITEM TỪ TÚI (DOUBLE CLICK) =================
    public void TryInsert(InventoryItem item)
    {
        if (selectSlot == null)
        {
            Debug.LogWarning("[EnhancementPanel] selectSlot NULL");
            return;
        }

        if (!selectSlot.IsEmpty)
        {
            Debug.Log("[EnhancementPanel] Slot đã có item, bỏ qua");
            return;
        }

        selectSlot.SetItem(item);
        RefreshUI();
    }

    // double click ô nâng cấp để trả item
    public void ReturnItemFromSlot(UpgradeSlot slot)
    {
        slot.ClearSlot();
        RefreshUI();
    }

    // ================= TỈ LỆ & COST =================
    float GetSuccessRate(int currentLevel, int nextLevel)
    {
        if (currentLevel == 1 && nextLevel == 2) return 1.0f;   // 100%
        if (currentLevel == 2 && nextLevel == 3) return 0.75f;  // 75%
        if (currentLevel == 3 && nextLevel == 4) return 0.30f;  // 30%
        return 0f;
    }

    int GetCost(int currentLevel, int nextLevel)
    {
        if (currentLevel == 1 && nextLevel == 2) return 50;
        if (currentLevel == 2 && nextLevel == 3) return 80;
        if (currentLevel == 3 && nextLevel == 4) return 120;
        return 0;
    }

    // ================= LẤY WEAPONDATA / ARMORDATA TỪ ICON =================
    WeaponData GetDataForItem(InventoryItem item)
    {
        if (item != null && item.GetCurrentData() != null)
        {
            return item.GetCurrentData();
        }

        if (item == null || equipmentManager == null) return null;
        Sprite sp = item.GetItemSprite();
        if (sp == null) return null;

        switch (item.itemType)
        {
            case InventoryItem.ItemType.Weapon:
                return equipmentManager.FindWeaponDataByIcon(sp);
            case InventoryItem.ItemType.Head:
                return equipmentManager.FindHelmetDataByIcon(sp);
            case InventoryItem.ItemType.Chest:
                return equipmentManager.FindChestDataByIcon(sp);
        }
        return null;
    }

    // ================= CẬP NHẬT STAT CỦA PLAYER SAU KHI ĐẬP =================
    void UpdatePlayerStats()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) player = GameObject.Find("PlayerRuntime");

        if (player != null)
        {
            var stats = player.GetComponent<CharacterStats>();
            if (stats != null)
                stats.UpdateFinalStats();
        }
    }

    // ================= NÚT NÂNG CẤP =================
    public void OnClickEnhance()
    {
        if (selectSlot == null || selectSlot.IsEmpty) return;

        InventoryItem itemInBag = selectSlot.originalItem;

        if (itemInBag == null)
        {
            Debug.LogError("LỖI: Không tìm thấy món đồ gốc trong túi!");
            return;
        }

        int curLevel = itemInBag.GetUpgradeLevel();
        if (curLevel >= InventoryItem.MaxUpgradeLevel) return;

        int nextLevel = curLevel + 1;
        float rate = GetSuccessRate(curLevel, nextLevel);
        int cost = GetCost(curLevel, nextLevel);

        if (!TrySpendGold(cost))
        {
            Debug.Log("Không đủ vàng!");
            PlayUpgradeSound(false);
            RefreshUI();
            return;
        }

        bool success = (Random.value <= rate);
        WeaponData itemData = GetDataForItem(itemInBag);

        if (success)
        {
            PlayUpgradeSound(true);

            itemInBag.SetUpgradeLevel(nextLevel);

            if (mainInventoryManager != null)
            {
                Debug.Log($"[Enhancement] Thành công! Sync {curLevel} -> {nextLevel}");
                mainInventoryManager.SyncItemLevel(itemData, curLevel, nextLevel);
            }

            if (itemInBag.GetCurrentData() == null && itemData != null)
            {
                itemInBag.SetItem(itemInBag.GetItemSprite(), itemInBag.itemType, null, itemData);
                itemInBag.SetUpgradeLevel(nextLevel);
            }

            InventoryItem visualItem = selectSlot.GetComponentInChildren<InventoryItem>();
            if (visualItem != null) visualItem.SetUpgradeLevel(nextLevel);

            if (equipmentManager != null && itemData != null)
            {
                if (equipmentManager.currentWeapon == itemData) equipmentManager.RefreshEquippedItem(InventoryItem.ItemType.Weapon, nextLevel);
                else if (equipmentManager.currentHelmet == itemData) equipmentManager.RefreshEquippedItem(InventoryItem.ItemType.Head, nextLevel);
                else if (equipmentManager.currentChest == itemData) equipmentManager.RefreshEquippedItem(InventoryItem.ItemType.Chest, nextLevel);
            }
        }
        else
        {
            // Tính level bị tụt
            int newLv = (curLevel > 1) ? curLevel - 1 : 1;

            // Cập nhật object
            itemInBag.SetUpgradeLevel(newLv);
            PlayUpgradeSound(false);
            Debug.Log($"[Enhance] THẤT BẠI! Item tụt về Lv {newLv}");

            // GỌI ĐỒNG BỘ SANG TÚI ĐỒ CHÍNH 
            if (mainInventoryManager != null)
            {
                Debug.Log($"[Enhancement] Thất bại! Đang gọi Sync giảm cấp: {curLevel} -> {newLv}");
                // curLevel là cấp cũ, newLv là cấp mới (thấp hơn)
                mainInventoryManager.SyncItemLevel(itemData, curLevel, newLv);
            }
            else
            {
                Debug.LogError("LỖI: mainInventoryManager bị Null, không thể đồng bộ thất bại!");
            }

            // Cập nhật Visual trong lò rèn
            InventoryItem visualItem = selectSlot.GetComponentInChildren<InventoryItem>();
            if (visualItem != null) visualItem.SetUpgradeLevel(newLv);

            // Đồng bộ trang bị đang mặc
            if (equipmentManager != null && itemData != null)
            {
                if (equipmentManager.currentWeapon == itemData) equipmentManager.RefreshEquippedItem(InventoryItem.ItemType.Weapon, newLv);
                else if (equipmentManager.currentHelmet == itemData) equipmentManager.RefreshEquippedItem(InventoryItem.ItemType.Head, newLv);
                else if (equipmentManager.currentChest == itemData) equipmentManager.RefreshEquippedItem(InventoryItem.ItemType.Chest, newLv);
            }
        }

        UpdatePlayerStats();
        RefreshUI();
    }

    void PlayUpgradeSound(bool success)
    {
        if (audioSource == null) return;
        AudioClip clip = success ? successClip : failClip;
        if (clip == null) return;
        audioSource.PlayOneShot(clip, sfxVolume);
    }

    // ================= CẬP NHẬT UI THEO LEVEL & STAT =================
    public void RefreshUI()
    {
        // reset mặc định
        if (btnEnhance != null) btnEnhance.interactable = false;
        if (txtCost != null) txtCost.text = "";
        if (txtSuccessRate != null) txtSuccessRate.text = "";

        void ClearStatsTexts()
        {
            if (txtAtkFrom) txtAtkFrom.text = "-";
            if (txtAtkTo) txtAtkTo.text = "-";
            if (txtDefFrom) txtDefFrom.text = "-";
            if (txtDefTo) txtDefTo.text = "-";
            if (txtHpFrom) txtHpFrom.text = "-";
            if (txtHpTo) txtHpTo.text = "-";
            if (txtEnergyFrom) txtEnergyFrom.text = "-";
            if (txtEnergyTo) txtEnergyTo.text = "-";
        }

        if (selectSlot == null || selectSlot.IsEmpty || selectSlot.originalItem == null)
        {
            if (txtLevelFrom != null) txtLevelFrom.text = "-";
            if (txtLevelTo != null) txtLevelTo.text = "-";
            ClearStatsTexts();
            return;
        }

        InventoryItem item = selectSlot.originalItem;
        int curLevel = item.GetUpgradeLevel();

        WeaponData data = GetDataForItem(item);
        if (data == null)
        {
            if (txtLevelFrom != null) txtLevelFrom.text = curLevel.ToString();
            if (txtLevelTo != null) txtLevelTo.text = "-";
            ClearStatsTexts();
            return;
        }

        // Đã MAX LEVEL
        if (curLevel >= InventoryItem.MaxUpgradeLevel)
        {
            if (txtLevelFrom != null) txtLevelFrom.text = curLevel.ToString();
            if (txtLevelTo != null) txtLevelTo.text = "MAX";

            // Hiện stats max
            int atk = data.GetATK(curLevel);
            int def = data.GetDEF(curLevel);
            int hp = data.GetMaxHP(curLevel);
            int en = data.GetEnergy(curLevel);

            if (txtAtkFrom) txtAtkFrom.text = atk.ToString();
            if (txtAtkTo) txtAtkTo.text = atk.ToString();
            if (txtDefFrom) txtDefFrom.text = def.ToString();
            if (txtDefTo) txtDefTo.text = def.ToString();
            if (txtHpFrom) txtHpFrom.text = hp.ToString();
            if (txtHpTo) txtHpTo.text = hp.ToString();
            if (txtEnergyFrom) txtEnergyFrom.text = en.ToString();
            if (txtEnergyTo) txtEnergyTo.text = en.ToString();

            if (txtSuccessRate != null) txtSuccessRate.text = "MAX LEVEL";
            return;
        }

        // Chưa max
        int nextLevel = curLevel + 1;
        float rate = GetSuccessRate(curLevel, nextLevel);
        int cost = GetCost(curLevel, nextLevel);

        if (txtLevelFrom != null) txtLevelFrom.text = curLevel.ToString();
        if (txtLevelTo != null) txtLevelTo.text = nextLevel.ToString();

        if (txtSuccessRate != null)
            txtSuccessRate.text = $"{rate * 100f:0}% Success Rate";

        if (txtCost != null)
            txtCost.text = $"{cost} Gold";

        int gold = CurrentGold;
        bool canEnhance = (cost > 0) && (gold >= cost);
        if (btnEnhance != null) btnEnhance.interactable = canEnhance;

        // Stats
        int atkNow = data.GetATK(curLevel);
        int defNow = data.GetDEF(curLevel);
        int hpNow = data.GetMaxHP(curLevel);
        int enNow = data.GetEnergy(curLevel);

        int atkNext = data.GetATK(nextLevel);
        int defNext = data.GetDEF(nextLevel);
        int hpNext = data.GetMaxHP(nextLevel);
        int enNext = data.GetEnergy(nextLevel);

        if (txtAtkFrom) txtAtkFrom.text = atkNow.ToString();
        if (txtAtkTo) txtAtkTo.text = atkNext.ToString();
        if (txtDefFrom) txtDefFrom.text = defNow.ToString();
        if (txtDefTo) txtDefTo.text = defNext.ToString();
        if (txtHpFrom) txtHpFrom.text = hpNow.ToString();
        if (txtHpTo) txtHpTo.text = hpNext.ToString();
        if (txtEnergyFrom) txtEnergyFrom.text = enNow.ToString();
        if (txtEnergyTo) txtEnergyTo.text = enNext.ToString();
    }
}
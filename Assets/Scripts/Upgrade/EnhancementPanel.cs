using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EnhancementPanel : MonoBehaviour
{
    public static EnhancementPanel Instance;

    // ===== SLOT NÂNG CẤP =====
    [Header("Slot nâng cấp")]
    public UpgradeSlot selectSlot;

    // ===== TÚI ĐẬP ĐỒ (Inventory_Forge) =====
    [Header("Forge Inventory (Inventory_Forge)")]
    [Tooltip("Kéo Canvas_Enhancement/Inventory_Forge vào đây. Túi này chỉ mở khi bấm nút Nâng cấp.")]
    public GameObject inventoryForgePanel;

    // (Giữ lại nếu bạn còn cần đồng bộ level về túi chính)
    [Header("Sync về túi chính (nếu dùng)")]
    public InventoryGridManager mainInventoryManager;

    // ===== LEVEL =====
    [Header("UI Level")]
    public TMP_Text txtLevelFrom;
    public TMP_Text txtLevelTo;

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
    public TMP_Text txtSuccessRate;
    public TMP_Text txtCost;
    public Button btnEnhance;

    // ====== ÂM THANH NÂNG CẤP ======
    [Header("Upgrade Sound")]
    public AudioSource audioSource;
    public AudioClip successClip;
    public AudioClip failClip;
    [Range(0f, 1f)] public float sfxVolume = 0.9f;

    // ===== THAM CHIẾU EQUIPMENT MANAGER =====
    [Header("Refs")]
    public EquipmentManager equipmentManager;

    private void Awake()
    {
        Instance = this;
    }

    // ================= XỬ LÝ ẨN HIỆN UI  =================
    private void OnEnable()
    {
        // ✅ Mở UI đập đồ thì ÉP túi đập đồ tắt trước
        // (bấm F chỉ mở bảng nâng cấp, chưa mở túi chọn item)
        if (inventoryForgePanel != null)
            inventoryForgePanel.SetActive(false);

        RefreshUI();
    }

    // ❌ KHÔNG OnDisable bật túi chính nữa (tránh tự mở inventory phía sau)
    // private void OnDisable() { }

    // ====================================================================

    public bool IsOpen() => gameObject.activeInHierarchy;

    private void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (equipmentManager == null)
            equipmentManager = FindFirstObjectByType<EquipmentManager>(FindObjectsInactive.Include);

        // Nếu muốn chắc chắn túi forge tắt từ đầu (phòng trường hợp quên)
        if (inventoryForgePanel != null)
            inventoryForgePanel.SetActive(false);

        RefreshUI();
    }

    // ================= NÚT "NÂNG CẤP" (MỞ TÚI ĐẬP ĐỒ) =================
    // Gắn hàm này cho button "Nâng cấp" trong UI đập đồ
    public void OnClickOpenForgeInventory()
    {
        if (inventoryForgePanel != null)
            inventoryForgePanel.SetActive(true);
    }

    // (Tuỳ chọn) Nút back/close để tắt túi đập đồ
    public void CloseForgeInventory()
    {
        if (inventoryForgePanel != null)
            inventoryForgePanel.SetActive(false);
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
        if (gm == null) return true;

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
        if (currentLevel == 1 && nextLevel == 2) return 1.0f;
        if (currentLevel == 2 && nextLevel == 3) return 0.75f;
        if (currentLevel == 3 && nextLevel == 4) return 0.30f;
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
            return item.GetCurrentData();

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

    // ================= NÚT NÂNG CẤP THẬT (ĐẬP ITEM) =================
    // Bạn gắn hàm này cho nút "Đập / Enhance" bên trong UI đập đồ
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
                mainInventoryManager.SyncItemLevel(itemData, curLevel, nextLevel);

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
            int newLv = (curLevel > 1) ? curLevel - 1 : 1;

            itemInBag.SetUpgradeLevel(newLv);
            PlayUpgradeSound(false);

            if (mainInventoryManager != null)
                mainInventoryManager.SyncItemLevel(itemData, curLevel, newLv);

            InventoryItem visualItem = selectSlot.GetComponentInChildren<InventoryItem>();
            if (visualItem != null) visualItem.SetUpgradeLevel(newLv);

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

        if (curLevel >= InventoryItem.MaxUpgradeLevel)
        {
            if (txtLevelFrom != null) txtLevelFrom.text = curLevel.ToString();
            if (txtLevelTo != null) txtLevelTo.text = "MAX";

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

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
    public InventoryToggle inventoryToggle;
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

    // ===== DÒNG DƯỚI =====
    [Header("UI Bottom")]
    public TMP_Text txtSuccessRate;
    public TMP_Text txtCost;
    public Button btnEnhance;

    // ====== ÂM THANH ======
    [Header("Upgrade Sound")]
    public AudioSource audioSource;
    public AudioClip successClip;
    public AudioClip failClip;
    [Range(0f, 1f)] public float sfxVolume = 0.9f;

    // ===== REFS =====
    [Header("Refs")]
    public EquipmentManager equipmentManager;

    private void Awake()
    {
        Instance = this;
    }

    // ================= XỬ LÝ ẨN HIỆN UI  =================
    private void OnEnable()
    {
        // Khi mở bảng nâng cấp -> Gọi hàm Close() của túi đồ để đảm bảo logic đồng nhất
        if (inventoryToggle != null)
        {
            inventoryToggle.Close();
            Debug.Log("[EnhancementPanel] Đã yêu cầu InventoryToggle đóng lại.");
        }
        else
        {
            inventoryToggle = FindFirstObjectByType<InventoryToggle>();
            if (inventoryToggle != null) inventoryToggle.Close();
        }
    }

    private void OnDisable()
    {   
    }
    public bool IsOpen() => gameObject.activeInHierarchy;

    private void Start()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (equipmentManager == null) equipmentManager = FindFirstObjectByType<EquipmentManager>(FindObjectsInactive.Include);

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
        if (gm == null) return true;
        if (gm.gold < amount) return false;
        gm.gold -= amount;
        return true;
    }

    // ================= NHẬN ITEM TỪ TÚI =================
    public void TryInsert(InventoryItem item)
    {
        if (selectSlot == null) return;
        if (!selectSlot.IsEmpty) return;

        selectSlot.SetItem(item);
        RefreshUI();
    }

    public void ReturnItemFromSlot(UpgradeSlot slot)
    {
        slot.ClearSlot();
        RefreshUI();
    }

    // ================= LOGIC TÍNH TOÁN =================
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

    WeaponData GetDataForItem(InventoryItem item)
    {
        if (item != null && item.GetCurrentData() != null) return item.GetCurrentData();
        if (item == null || equipmentManager == null) return null;
        Sprite sp = item.GetItemSprite();
        if (sp == null) return null;

        switch (item.itemType)
        {
            case InventoryItem.ItemType.Weapon: return equipmentManager.FindWeaponDataByIcon(sp);
            case InventoryItem.ItemType.Head: return equipmentManager.FindHelmetDataByIcon(sp);
            case InventoryItem.ItemType.Chest: return equipmentManager.FindChestDataByIcon(sp);
        }
        return null;
    }

    void UpdatePlayerStats()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) player = GameObject.Find("PlayerRuntime");
        if (player != null)
        {
            var stats = player.GetComponent<CharacterStats>();
            if (stats != null) stats.UpdateFinalStats();
        }
    }

    public void OnClickEnhance()
    {
        if (selectSlot == null || selectSlot.IsEmpty) return;

        InventoryItem itemInBag = selectSlot.originalItem;
        if (itemInBag == null) return;

        int curLevel = itemInBag.GetUpgradeLevel();
        if (curLevel >= InventoryItem.MaxUpgradeLevel) return;

        int nextLevel = curLevel + 1;
        float rate = GetSuccessRate(curLevel, nextLevel);
        int cost = GetCost(curLevel, nextLevel);

        if (!TrySpendGold(cost))
        {
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

            // Xử lý visual item
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
            ClearStatsTexts();
            return;
        }

        if (curLevel >= InventoryItem.MaxUpgradeLevel)
        {
            if (txtLevelFrom != null) txtLevelFrom.text = curLevel.ToString();
            if (txtLevelTo != null) txtLevelTo.text = "MAX";

            // Show max stats logic...
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
        if (txtSuccessRate != null) txtSuccessRate.text = $"{rate * 100f:0}% Success Rate";
        if (txtCost != null) txtCost.text = $"{cost} Gold";

        int gold = CurrentGold;
        btnEnhance.interactable = (cost > 0 && gold >= cost);

        int atkNow = data.GetATK(curLevel); int atkNext = data.GetATK(nextLevel);
        int defNow = data.GetDEF(curLevel); int defNext = data.GetDEF(nextLevel);
        int hpNow = data.GetMaxHP(curLevel); int hpNext = data.GetMaxHP(nextLevel);
        int enNow = data.GetEnergy(curLevel); int enNext = data.GetEnergy(nextLevel);

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
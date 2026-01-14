using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EnhancementPanel : MonoBehaviour
{
    public static EnhancementPanel Instance;

    [Header("Slot nâng cấp")]
    public UpgradeSlot selectSlot;

    [Header("Forge Inventory (Inventory_Forge)")]
    public GameObject inventoryForgePanel;

    [Header("Sync về túi chính (nếu dùng)")]
    public InventoryGridManager mainInventoryManager;

    [Header("UI Level")]
    public TMP_Text txtLevelFrom;
    public TMP_Text txtLevelTo;

    [Header("UI Stats (Trước / Sau)")]
    public TMP_Text txtAtkFrom;
    public TMP_Text txtAtkTo;
    public TMP_Text txtDefFrom;
    public TMP_Text txtDefTo;
    public TMP_Text txtHpFrom;
    public TMP_Text txtHpTo;
    public TMP_Text txtEnergyFrom;
    public TMP_Text txtEnergyTo;

    [Header("UI Bottom")]
    public TMP_Text txtSuccessRate;
    public TMP_Text txtCost;
    public Button btnEnhance;

    [Header("Upgrade Sound")]
    public AudioSource audioSource;
    public AudioClip successClip;
    public AudioClip failClip;
    [Range(0f, 1f)] public float sfxVolume = 0.9f;

    [Header("Refs")]
    public EquipmentManager equipmentManager;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        if (inventoryForgePanel != null)
            inventoryForgePanel.SetActive(false);

        RefreshUI();
    }

    public bool IsOpen() => gameObject.activeInHierarchy;

    private void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (equipmentManager == null)
            equipmentManager = FindFirstObjectByType<EquipmentManager>(FindObjectsInactive.Include);

        if (inventoryForgePanel != null)
            inventoryForgePanel.SetActive(false);

        RefreshUI();
    }

    public void OnClickOpenForgeInventory()
    {
        if (inventoryForgePanel != null)
            inventoryForgePanel.SetActive(true);
    }

    public void CloseForgeInventory()
    {
        if (inventoryForgePanel != null)
            inventoryForgePanel.SetActive(false);
    }

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

    // Hàm nhỏ để hiện message ngay trong UI (dùng txtSuccessRate cho tiện, khỏi phải tạo Text mới)
    void ShowMessage(string msg)
    {
        if (txtSuccessRate != null) txtSuccessRate.text = msg;
        //Debug.Log("[EnhancementPanel] " + msg);
    }

    // Trả về WeaponData của item trong túi:
    // ưu tiên lấy currentData (chuẩn nhất), nếu null thì map icon -> data từ EquipmentManager
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

    // Check “đang trang bị” theo data (và fallback icon) để chặn đập đồ
    bool IsItemEquipped(InventoryItem item)
    {
        if (equipmentManager == null || item == null) return false;

        WeaponData data = GetDataForItem(item);
        if (data != null) return equipmentManager.IsEquipped(data);

        // nếu data vẫn null thì fallback theo sprite icon
        Sprite sp = item.GetItemSprite();
        if (sp != null) return equipmentManager.IsEquippedByIcon(sp);

        return false;
    }

    // Nhận item từ túi khi double click (InventoryItem gọi TryInsert)
    public void TryInsert(InventoryItem item)
    {
        if (selectSlot == null)
        {
            //Debug.LogWarning("[EnhancementPanel] selectSlot NULL");
            return;
        }

        if (!selectSlot.IsEmpty)
        {
            //Debug.Log("[EnhancementPanel] Slot đã có item, bỏ qua");
            return;
        }

        // Chặn ngay từ lúc bỏ vào ô nâng cấp
        if (IsItemEquipped(item))
        {
           
            if (btnEnhance != null) btnEnhance.interactable = false;
            return;
        }

        selectSlot.SetItem(item);
        RefreshUI();
    }

    public void ReturnItemFromSlot(UpgradeSlot slot)
    {
        slot.ClearSlot();
        RefreshUI();
    }

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

    public void OnClickEnhance()
    {
        if (selectSlot == null || selectSlot.IsEmpty) return;

        InventoryItem itemInBag = selectSlot.originalItem;
        if (itemInBag == null)
        {
            //Debug.LogError("LỖI: Không tìm thấy món đồ gốc trong túi!");
            return;
        }

        // Chặn bất tử: dù có lách UI, bấm nút vẫn không nâng cấp nếu đang mặc
        if (IsItemEquipped(itemInBag))
        {
            //ShowMessage("Item đang trang bị. Tháo ra mới nâng cấp!");
            if (btnEnhance != null) btnEnhance.interactable = false;
            PlayUpgradeSound(false);
            return;
        }

        int curLevel = itemInBag.GetUpgradeLevel();
        if (curLevel >= InventoryItem.MaxUpgradeLevel) return;

        int nextLevel = curLevel + 1;
        float rate = GetSuccessRate(curLevel, nextLevel);
        int cost = GetCost(curLevel, nextLevel);

        if (!TrySpendGold(cost))
        {
            ShowMessage("Không đủ vàng!");
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

            // Theo yêu cầu mới: item đang mặc không được đập, nên đoạn refreshEquipped này về cơ bản sẽ không chạy.
            // Nhưng để an toàn (lỡ có case data mismatch), vẫn giữ.
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

        // Nếu item đang trang bị -> khóa luôn UI (đúng yêu cầu: tháo ra mới nâng cấp)
        if (IsItemEquipped(item))
        {
            if (txtLevelFrom != null) txtLevelFrom.text = item.GetUpgradeLevel().ToString();
            if (txtLevelTo != null) txtLevelTo.text = "-";
            ClearStatsTexts();
            ShowMessage("Item đang trang bị. Tháo ra mới nâng cấp!");
            if (btnEnhance != null) btnEnhance.interactable = false;
            return;
        }

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

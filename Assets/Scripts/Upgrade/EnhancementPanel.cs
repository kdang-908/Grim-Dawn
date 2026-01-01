using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EnhancementPanel : MonoBehaviour
{
    public static EnhancementPanel Instance;

    [Header("Slot nâng cấp")]
    public UpgradeSlot selectSlot;       // Btn_SelectEquipment

    [Header("UI Level")]
    public TMP_Text txtLevelFrom;        // số bên trái (cấp hiện tại)
    public TMP_Text txtLevelTo;          // số bên phải (cấp kế tiếp)

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

    private void Awake()
    {
        Instance = this;
    }

    public bool IsOpen() => gameObject.activeInHierarchy;

    private void Start()
    {
        // nếu quên gán AudioSource thì tự tìm trên cùng object
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        RefreshUI();
    }

    // ===== GOLD từ GameManager =====
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

    // Gọi khi double click item trong túi (mode nâng cấp)
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

    // Gọi khi double click ô nâng cấp để trả item
    public void ReturnItemFromSlot(UpgradeSlot slot)
    {
        slot.ClearSlot();
        RefreshUI();
    }

    // ===== TỈ LỆ & COST =====
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

    // ===== NÚT NÂNG CẤP =====
    public void OnClickEnhance()
    {
        if (selectSlot == null || selectSlot.IsEmpty)
        {
            Debug.Log("[EnhancementPanel] Chưa có trang bị trong slot");
            return;
        }

        InventoryItem item = selectSlot.originalItem;
        if (item == null)
        {
            Debug.LogWarning("[EnhancementPanel] originalItem NULL");
            return;
        }

        int curLevel = item.GetUpgradeLevel();
        if (curLevel >= InventoryItem.MaxUpgradeLevel)
        {
            Debug.Log("[EnhancementPanel] Đã max level");
            return;
        }

        int nextLevel = curLevel + 1;
        float rate = GetSuccessRate(curLevel, nextLevel);
        int cost = GetCost(curLevel, nextLevel);

        // check vàng
        if (!TrySpendGold(cost))
        {
            Debug.Log("[EnhancementPanel] Không đủ vàng");
            // có thể chơi thêm sound "fail" nhẹ nếu muốn
            PlayUpgradeSound(false);
            RefreshUI();
            return;
        }

        float roll = Random.value;
        bool success = roll <= rate;

        if (success)
        {
            item.SetUpgradeLevel(nextLevel);
            Debug.Log($"[EnhancementPanel] SUCCESS {curLevel} -> {nextLevel}");

            // 🔊 âm thanh thành công
            PlayUpgradeSound(true);
        }
        else
        {
            if (curLevel > 1)
            {
                int down = curLevel - 1;
                item.SetUpgradeLevel(down);
                Debug.Log($"[EnhancementPanel] FAIL {curLevel} -> {down}");
            }
            else
            {
                Debug.Log($"[EnhancementPanel] FAIL {curLevel} -> giữ lv1");
            }

            // 🔊 âm thanh thất bại
            PlayUpgradeSound(false);
        }

        RefreshUI();
    }

    void PlayUpgradeSound(bool success)
    {
        if (audioSource == null) return;

        AudioClip clip = success ? successClip : failClip;
        if (clip == null) return;

        audioSource.PlayOneShot(clip, sfxVolume);
    }

    // ===== CẬP NHẬT UI THEO LEVEL HIỆN TẠI =====
    public void RefreshUI()
    {
        // mặc định
        if (btnEnhance != null) btnEnhance.interactable = false;
        if (txtCost != null) txtCost.text = "";
        if (txtSuccessRate != null) txtSuccessRate.text = "";

        if (selectSlot == null || selectSlot.IsEmpty || selectSlot.originalItem == null)
        {
            if (txtLevelFrom != null) txtLevelFrom.text = "-";
            if (txtLevelTo != null) txtLevelTo.text = "-";
            return;
        }

        InventoryItem item = selectSlot.originalItem;
        int curLevel = item.GetUpgradeLevel();

        if (curLevel >= InventoryItem.MaxUpgradeLevel)
        {
            if (txtLevelFrom != null) txtLevelFrom.text = curLevel.ToString();
            if (txtLevelTo != null) txtLevelTo.text = "MAX";
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

        if (btnEnhance != null)
            btnEnhance.interactable = canEnhance;
    }
}

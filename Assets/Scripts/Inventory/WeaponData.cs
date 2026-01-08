using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Weapon Data")]
public class WeaponData : ScriptableObject
{
    public string id;
    public string displayName;

    public InventoryItem.ItemType itemType;
    public Sprite icon;
    public GameObject prefab;
    public int animationID;

    [Header("Attach Offset (local)")]
    public Vector3 localPos = Vector3.zero;
    public Vector3 localEuler = Vector3.zero;
    public Vector3 localScale = Vector3.one;

    [Header("Head Offset")]
    public Vector3 headPos = new Vector3(0.002f, 0.09f, 0.001f); 
    public Vector3 headEuler = new Vector3(-14.6f, 0.7f, -4.7f);
    public Vector3 headScaleUI = new Vector3(0.15f, 0.15f, 0.15f);
    public Vector3 headScaleRuntime = new Vector3(0.23f, 0.2f, 0.22f);

    [Header("Cấu hình cho Nhân vật Nữ (Female)")]
    public Vector3 femaleHeadPos = new Vector3(0.002f, 0.08f, 0.001f); 
    public Vector3 femaleHeadEuler = new Vector3(-10f, 0f, 0f);      
    public Vector3 femaleHeadScaleUI = new Vector3(0.35f, 0.35f, 0.35f); 
    public Vector3 femaleHeadScaleRuntime = new Vector3(0.2f, 0.18f, 0.2f);

    [Header("Cấu hình cho Giáp (Chest Offset)")]
    public Vector3 chestPos = Vector3.zero;
    public Vector3 chestEuler = Vector3.zero;
    public Vector3 chestScaleUI = Vector3.one;
    public Vector3 chestScaleRuntime = Vector3.one;
    [Header("Cấu hình Giáp cho Nữ (Female Armor)")]
    public Vector3 femaleChestPos;
    public Vector3 femaleChestEuler;
    public Vector3 femaleChestScaleUI = new Vector3(1, 1, 1);
    public Vector3 femaleChestScaleRuntime = new Vector3(1, 1, 1);

    [Header("Chỉ số cộng thêm (Stats)")]
    public int bonusATK;
    public int bonusDEF;
    public int bonusMaxHP;
    public int bonusEnergy;
    [TextArea] public string description; // Mô tả vật phẩm
    [Header("Nâng cấp")]
    [Tooltip("Mỗi cấp tăng thêm bao nhiêu % so với stat gốc (0.05 = 5%)")]
    public float upgradePercentPerLevel = 5f;

    // ========== HÀM TÍNH STAT THEO LEVEL ==========
    float GetMultiplier(int level)
    {
        // Lv1 = 100%, Lv2 = 105%, Lv3 = 110% ...
        int lv = Mathf.Max(level, 1);
        return 1f + upgradePercentPerLevel * (lv - 1);
    }

    public int GetATK(int level)
    {
        return Mathf.RoundToInt(bonusATK * GetMultiplier(level));
    }

    public int GetDEF(int level)
    {
        return Mathf.RoundToInt(bonusDEF * GetMultiplier(level));
    }

    public int GetMaxHP(int level)
    {
        return Mathf.RoundToInt(bonusMaxHP * GetMultiplier(level));
    }

    public int GetEnergy(int level)
    {
        return Mathf.RoundToInt(bonusEnergy * GetMultiplier(level));
    }
}

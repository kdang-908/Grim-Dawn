using UnityEngine;

// Dòng này giúp tạo menu "Create -> Inventory -> Armor Data"
[CreateAssetMenu(fileName = "New Armor Data", menuName = "Inventory/Armor Data")]
public class ArmorData : ScriptableObject
{
    [Header("Thông tin cơ bản")]
    public string id;
    public string displayName;
    public Sprite icon;
    public GameObject prefab;

    [Header("Chỉ số")]
    public int defense; // Giáp thì có thủ (Def) thay vì công (Atk)

    [Header("Căn chỉnh vị trí (Offset)")]
    // Cái này để chỉnh nón cho vừa đầu, giống cái Offset của kiếm
    public Vector3 localPos;
    public Vector3 localEuler;
    public Vector3 localScale = new Vector3(1, 1, 1);

    [Header("Chỉ số cộng thêm")]
    public int atkBonus;
    public int defBonus;
    public int hpBonus;

}
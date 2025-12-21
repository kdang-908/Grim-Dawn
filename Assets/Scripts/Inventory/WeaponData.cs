using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Weapon Data")]
public class WeaponData : ScriptableObject
{
    public string id;
    public string displayName;

    public Sprite icon;
    public GameObject prefab;
    public int animationID;

    [Header("Attach Offset (local)")]
    public Vector3 localPos = Vector3.zero;
    public Vector3 localEuler = Vector3.zero;
    public Vector3 localScale = Vector3.one;

    [Header("Chỉ số cộng thêm")]
    public int atkBonus;
    public int defBonus;
    public int hpBonus;
}

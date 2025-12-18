using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Weapon Data")]
public class WeaponData : ScriptableObject
{
    public string id;
    public string displayName;

    public Sprite icon;
    public GameObject prefab;

    [Header("Attach Offset (local)")]
    public Vector3 localPos = Vector3.zero;
    public Vector3 localEuler = Vector3.zero;
    public Vector3 localScale = Vector3.one;
}

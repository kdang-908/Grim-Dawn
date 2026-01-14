using UnityEngine;

public class EquipWeaponOnMapStart : MonoBehaviour
{
    public WeaponData defaultWeapon;

    void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            //Debug.Log("[EquipWeaponOnMapStart] Không tìm thấy Player");
            return;
        }

        WeaponEquipper equipper = player.GetComponentInChildren<WeaponEquipper>(true);
        if (equipper == null)
        {
            //Debug.Log("[EquipWeaponOnMapStart] Không tìm thấy WeaponEquipper");
            return;
        }

        // 🔒 ĐÃ CÓ VŨ KHÍ (từ EquipmentManager) → KHÔNG auto equip
        if (equipper.HasWeapon())
        {
            //Debug.Log("[EquipWeaponOnMapStart] Player đã có vũ khí → bỏ qua default");
            return;
        }

        if (defaultWeapon == null)
        {
            //Debug.Log("[EquipWeaponOnMapStart] defaultWeapon = NULL → bỏ qua");
            return;
        }

        equipper.Equip(defaultWeapon);
        //Debug.Log("[EquipWeaponOnMapStart] Equip default weapon: " + defaultWeapon.name);
    }
}

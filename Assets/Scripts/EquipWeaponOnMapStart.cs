using UnityEngine;

public class EquipWeaponOnMapStart : MonoBehaviour
{
    public WeaponData defaultWeapon;

    void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;

        WeaponEquipper equipper = player.GetComponent<WeaponEquipper>();
        if (equipper == null) return;

        equipper.Equip(defaultWeapon);
    }
}
